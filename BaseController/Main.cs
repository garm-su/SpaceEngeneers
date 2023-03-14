using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.ModAPI.Contracts;
using Sandbox.Game;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.ModAPI.Ingame.Utilities;
// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.BaseController //@remove
{ //@remove
    public sealed class Program : LogLibrary.Program //@remove
    { //@remove
        // Your code goes between the next #endregion and #region

        public new string LogTag = "[LOG BASE CONTROLLER]";
        public new int LogMaxCount = 100;

        string
            SKIP = "[SKIP]",
            Components = "Components",
            ComponentsTag = "[Components]",
            UserTag = "[User]", //todo: money here
            Resources = "Resources",
            ResourcesTag = "[Resources]",
            QueueTag = "[QUEUE]",

            AmmoTag = "[Ammo]",

            cargoTag = "[CARGO]",
            alarmTag = "[ALARM]",
            infoTag = "[INFO BASE]";

        Scheduler _scheduler;

        public string statusChannelTag = "RDOStatusChannel";
        public string commandChannelTag = "RDOCommandChannel";

        public Log logger;

        const double
            warningVolume = 85,
            alarmVolume = 95,
            warningCount = 50,
            alarmCount = 25;
        //MyObjectBuilder_Component / MyObjectBuilder_PhysicalObject / MyObjectBuilder_PhysicalGunObject / MyObjectBuilder_ConsumableItem / MyObjectBuilder_AmmoMagazine / MyObjectBuilder_Datapad / MyObjectBuilder_Ingot

        double minBatteryCharge = 0.8;
        double maxBatteryCharge = 0.9;
        bool h2generatorStarted = false;

        const String ICE = "Ice";
        bool iceAlarm = false;

        const String
        GUN = "MyObjectBuilder_PhysicalGunObject",
        CONSUMABLE = "MyObjectBuilder_ConsumableItem",
        DATAPAD = "MyObjectBuilder_Datapad",
        ORE = "MyObjectBuilder_Ore",
        AMMO = "MyObjectBuilder_AmmoMagazine",
        COMPONENT = "MyObjectBuilder_Component",
        INGOT = "MyObjectBuilder_Ingot";

        HashSet<string> MainContainerTypes = new HashSet<string> { COMPONENT };
        HashSet<string> ResourcesContainerTypes = new HashSet<string> { ORE, INGOT, DATAPAD };
        HashSet<string> UserContainerTypes = new HashSet<string> { GUN, CONSUMABLE, };
        HashSet<string> AmmoContainerTypes = new HashSet<string> { AMMO };
        List<String> BaseResoursesType = new List<String>() { ORE, INGOT };
        List<String> ProdResoursesType = new List<String>() { COMPONENT };
        List<String> ProdResoursesAMMO = new List<String>() { AMMO };
        HashSet<String> SkipItems = new HashSet<String> { "EngineerPlushie" };
        HashSet<String> CrushedQueued = new HashSet<String> { };
        HashSet<String> Types = new HashSet<String> { };
        Dictionary<string, int> minComponents = new Dictionary<string, int> { };
        Dictionary<string, int> minResources = new Dictionary<string, int> { };
        Dictionary<string, int> queueResources = new Dictionary<string, int> { };

        long countClear = 0;

        Messaging Alarms;

        Dictionary<string, FactoryController> factories = new Dictionary<string, FactoryController>();
        Dictionary<String, List<IMyAssembler>> assemblersInUse = new Dictionary<String, List<IMyAssembler>>();
        List<IMyAssembler> allAssemblers = new List<IMyAssembler>();
        List<IMyAssembler> availableAssemblers = new List<IMyAssembler>();
        Dictionary<long, String> assemblerProduction = new Dictionary<long, String>();
        bool assembleSeveral = false;

        List<IMyCargoContainer>
        componentCargos = new List<IMyCargoContainer>(),
        userCargos = new List<IMyCargoContainer>(),
        resourcesCargos = new List<IMyCargoContainer>(),
        ammoCargos = new List<IMyCargoContainer>();

        private void recalcAll()
        {

            iceAlarm = false;

            if (h2generatorStarted)
            {
                Alarms.warn("H2 Engines is ON");
            }
            else
            {

            }

            reScanAssemblers();
            reScanObjectGroup(userCargos, UserTag); //todo move to one loop
            reScanObjectGroup(resourcesCargos, ResourcesTag);
            reScanObjectGroup(componentCargos, ComponentsTag);
            reScanObjectGroup(ammoCargos, AmmoTag);

            Alarms.info("Resources Cargo: " + resourcesCargos.Count.ToString());
            Alarms.info("Component Cargo: " + componentCargos.Count.ToString());
            Alarms.info("User Cargo: " + userCargos.Count.ToString());
            Alarms.info("Ammo Cargo: " + ammoCargos.Count.ToString());
            Alarms.info("Assemblers: " + allAssemblers.Count.ToString());
            Alarms.info("");
            Alarms.info("Cleared: " + countClear.ToString());
            calculateCount();

            var ini = new MyIni();
            ini.TryParse(Me.CustomData);

            var keys = new List<MyIniKey>();
            ini.GetKeys(Components, keys);
            minComponents.Clear();
            keys.ForEach(key => minComponents[key.Name] = ini.Get(key).ToInt32());

            ini.GetKeys(Resources, keys);
            minResources.Clear();
            keys.ForEach(key => minResources[key.Name] = ini.Get(key).ToInt32());

            // if (!reReadConfig(ResourcesTag, minResources)) Alarms.warn("There is no " + ResourcesTag + " screen");
            // if (!reReadConfig(ComponentsTag, minComponents)) Alarms.warn("There is no " + ComponentsTag + " screen");
            // if (!reReadConfig(AmmoTag, minComponents)) Alarms.warn("There is no " + AmmoTag + " screen");

            if (minComponents.Count > 0)
            {
                calculateQueueCount();
            }

            var foundObjects = new HashSet<string>();
            printCount("Resources ( raw / ingot ) :\n\n", ResourcesTag, BaseResoursesType, foundObjects);
            printCount("", ComponentsTag, ProdResoursesType, foundObjects);
            printCount("AMMO: \n\n", AmmoTag, ProdResoursesAMMO, foundObjects);

            alarmNoObject(foundObjects, minResources);
            alarmNoObject(foundObjects, minComponents);

            checkRunGenerators();

            calculateVolume();
            Alarms.next();
        }

        public void factoryReScan(){
            foreach (var fc in factories.Values) fc.reScan();
        }

        public void factorySecond(){
            foreach (var fc in factories.Values) fc.second();
        }
        public void factoryCheck()
        {
            foreach (var fc in factories.Values) fc.check();
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            logger = new Log(this);
            Alarms = new Messaging(this);

            h2generatorEnable(false);

            _scheduler = new Scheduler(this);
            _scheduler.AddScheduledAction(recalcAll, 1);
            _scheduler.AddScheduledAction(factorySecond, 1);
            _scheduler.AddScheduledAction(factoryReScan, 0.1);
            _scheduler.AddScheduledAction(factoryCheck, 10);

            _scheduler.AddScheduledAction(logger.printToSurfaces, 1);

            logger.write("Inited");

        }


        public void addAssembler(IMyAssembler assembler)
        {
            allAssemblers.Add(assembler);
            availableAssemblers.Add(assembler);
            assemblerProduction[assembler.GetId()] = null;
        }

        public void removeAssembler(IMyAssembler assembler)
        {
            Echo("Remove " + assembler.GetId());
            stopAssembler(assembler);
            assemblerProduction.Remove(assembler.GetId());
            availableAssemblers.Remove(assembler);
            allAssemblers.Remove(assembler);
        }

        public void reScanAssemblers()
        {
            var currentAssemblers = new List<IMyAssembler>();
            reScanObjectGroup(currentAssemblers, QueueTag);
            var oldAssemblersIds = (from assembler in allAssemblers select assembler.GetId()).ToHashSet();

            foreach (var assembler in currentAssemblers.Where(assembler => !oldAssemblersIds.Contains(assembler.GetId())))
            {
                addAssembler(assembler);
            }

            var currAssemblerIds = (from assembler in currentAssemblers select assembler.GetId()).ToHashSet();

            foreach (var assembler in allAssemblers.Where(assembler => !currAssemblerIds.Contains(assembler.GetId())).ToList())
            {
                removeAssembler(assembler);
            }
        }

        public bool startAssembler(String construct)
        {
            if (availableAssemblers.Count == 0) return false;

            var assembler = availableAssemblers.Pop();
            Echo("Assembler start " + construct);
            if (!assemblersInUse.ContainsKey(construct)) assemblersInUse[construct] = new List<IMyAssembler>();

            assemblersInUse[construct].Add(assembler);
            assemblerProduction[assembler.GetId()] = construct;

            MyDefinitionId objectIdToAdd = new MyDefinitionId();
            bool found;

            if (found = MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + construct, out objectIdToAdd))
            {
                if (!assembler.CanUseBlueprint(objectIdToAdd))
                {
                    found = MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + construct + "Component", out objectIdToAdd);
                    if (!assembler.CanUseBlueprint(objectIdToAdd))
                    {
                        found = MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + construct + "Magazine", out objectIdToAdd);

                    }
                }
            }
            if (found)
            {
                assembler.ClearQueue();
                try
                {
                    assembler.AddQueueItem(objectIdToAdd, 50.0);
                    assembler.Repeating = true;
                    assembler.Enabled = true;
                }
                catch
                {
                    CrushedQueued.Add(construct);
                    Alarms.alarm("CRUSH " + construct);
                    stopAssembler(assembler);
                    return false;
                }
            }
            else
            {
                Alarms.alarm("Cant parse " + construct);
                stopAssembler(assembler);
                return false;
            }
            return true;
        }
        public void stopAssembler(IMyAssembler assembler, bool removeFromInUse = true)
        {

            var assemblerId = assembler.GetId();
            Echo("Assembler stop " + assemblerId);
            var construct = assemblerProduction[assemblerId];
            if (construct == null) return;


            if (removeFromInUse)
            {
                assemblersInUse[construct].Remove(assembler);
                if (assemblersInUse[construct].Count == 0)
                {
                    assemblersInUse.Remove(construct);
                }
            }

            assembler.ClearQueue();
            assembler.Enabled = false;
            assembler.Repeating = false;

            availableAssemblers.Add(assembler);
            assemblerProduction[assemblerId] = null;
        }
        public void stopAllAssembler(String construct)
        {
            if (!assemblersInUse.ContainsKey(construct)) return;
            Echo("<StopAll " + construct);
            foreach (var assembler in assemblersInUse[construct])
            {
                stopAssembler(assembler, false);
            }
            Echo(">StopAll " + construct);
            assemblersInUse[construct].Clear();
            assemblersInUse.Remove(construct);
        }

        DefaultDictionary<string, DefaultDictionary<string, int>>
            count_objects = new DefaultDictionary<string, DefaultDictionary<string, int>>(),
            count_assembled = new DefaultDictionary<string, DefaultDictionary<string, int>>();
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

        public void doTransfer(IMyTerminalBlock block, MyInventoryItem item, IMyInventory sourse, string tag, HashSet<string> itemTypes, List<IMyCargoContainer> cargos, ref int moved)
        {
            if (!block.CustomName.Contains(tag) && itemTypes.Contains(item.Type.TypeId))
            {
                foreach (var cargo in cargos)
                {
                    var destination = cargo.GetInventory();
                    var old_mass = destination.CurrentMass;

                    if (!destination.IsFull && sourse.IsConnectedTo(destination) && sourse.CanTransferItemTo(destination, item.Type))
                    {
                        var transfered = sourse.TransferItemTo(destination, item);
                        var new_mass = cargo.GetInventory().CurrentMass;
                        moved++;
                        if (transfered && new_mass != old_mass) break;
                    }
                }
            }
        }

        public void calculateCount()
        {
            MyInventoryItem item;
            IMyInventory sourse;
            bool skip_move;
            int moved = 0;

            count_objects.Clear();

            GridTerminalSystem.GetBlocks(blocks);

            HashSet<string> tps = new HashSet<string> { };
            HashSet<string> names = new HashSet<string> { };


            foreach (var block in blocks)
            {

                if (block.HasInventory)
                {
                    skip_move = block.CustomName.Contains(SKIP);
                    for (int j = 0; j < block.InventoryCount; j++)
                    {

                        var items = new List<MyInventoryItem>();
                        sourse = block.GetInventory(j);
                        sourse.GetItems(items);

                        for (int k = 0; k < items.Count; k++)
                        {
                            item = items[k];
                            Types.Add(item.Type.TypeId);
                            count_objects[item.Type.SubtypeId][item.Type.TypeId] += (int)item.Amount;
                            tps.Add(item.Type.TypeId);

                            if (skip_move) continue;
                            if (item.Type.TypeId == ORE && block is IMyRefinery) continue;
                            if (item.Type.TypeId == ORE && block is IMyGasGenerator) continue;
                            if (item.Type.TypeId == ORE && block is IMyGasGenerator) continue;
                            if (item.Type.TypeId == INGOT && block is IMyAssembler) continue;
                            if (item.Type.TypeId == AMMO && block is IMyLargeTurretBase) continue;
                            if (item.Type.TypeId == AMMO && block is IMyUserControllableGun) continue;

                            doTransfer(block, item, sourse, ComponentsTag, MainContainerTypes, componentCargos, ref moved);
                            doTransfer(block, item, sourse, ResourcesTag, ResourcesContainerTypes, resourcesCargos, ref moved);
                            doTransfer(block, item, sourse, UserTag, UserContainerTypes, userCargos, ref moved);
                            doTransfer(block, item, sourse, AmmoTag, AmmoContainerTypes, ammoCargos, ref moved);
                            //todo in one loop and ammo moving
                        }
                    }
                }
            }
            Alarms.info("Moved: " + moved.ToString());// MyObjectBuilder_Component \ MyObjectBuilder_Ore \ MyObjectBuilder_AmmoMagazine \ MyObjectBuilder_ConsumableItem \ MyObjectBuilder_PhysicalGunObject \ MyObjectBuilder_PhysicalObject \ MyObjectBuilder_Ingot \ MyObjectBuilder_Datapad \ MyObjectBuilder_GasContainerObject \ MyObjectBuilder_OxygenContainerObject
        }
        public void printCount(string panelTextInit, string terminalName, List<String> resoursesType, HashSet<string> foundObjects)
        {
            bool found;
            double count;
            int val;
            string panelText = "" + panelTextInit;

            foreach (KeyValuePair<string, DefaultDictionary<string, int>> entry in count_objects)
            {
                if (SkipItems.Contains(entry.Key))
                {
                    continue;
                }
                var row = new List<string>();
                found = false;
                count = 0;

                foreach (var type in resoursesType)
                {

                    if (entry.Value.ContainsKey(type))
                    {
                        val = entry.Value[type];
                        if (val > 0)
                        {
                            found = true;
                            count += val;
                        }
                    }
                    else
                    {
                        val = 0;
                    }
                    row.Add(number(val));
                }

                if (found)
                {
                    foundObjects.Add(entry.Key);
                    if (count > 0)
                    {
                        panelText += entry.Key + " : " + String.Join(" / ", row) + "\n";
                    }

                    double minCount = 0;
                    if (minComponents.ContainsKey(entry.Key)) minCount += minComponents[entry.Key];
                    if (minResources.ContainsKey(entry.Key)) minCount += minResources[entry.Key];

                    if (minCount > 0)
                    {
                        var percent = 100 * count / minCount;
                        if (percent < warningCount)
                        {
                            var err = "Too few " + entry.Key + " ( " + number(count) + " )";
                            if (percent < alarmCount)
                            {
                                Alarms.alarm(err);
                                if (entry.Key == ICE)
                                {
                                    iceAlarm = true;
                                }
                            }
                            else
                            {
                                Alarms.warn(err);
                            }
                        }
                    }
                }
            }
        }



        public void calculateQueueCount()
        {
            logger.write("calculateQueueCount");
            int curValue;
            queueResources.Clear();
            CrushedQueued.Clear();

            foreach (KeyValuePair<string, int> entry in minComponents)
            {
                curValue = (count_objects?[entry.Key]?[COMPONENT] ?? 0) + (count_objects?[entry.Key]?[AMMO] ?? 0);
                if (curValue < entry.Value)
                {
                    queueResources[entry.Key] = entry.Value - curValue;
                }
            }


            foreach (string prod in (from x in assemblersInUse.Keys select x).ToList())
            {
                if (!queueResources.ContainsKey(prod)) stopAllAssembler(prod);
            }

            var needAssemblers = queueResources.Where(res => !assemblersInUse.ContainsKey(res.Key)).Count();
            var nowSeveralAssemblers = availableAssemblers.Count >= needAssemblers;

            if (!nowSeveralAssemblers) //stop all except one on all resources
            {
                logger.write("Clear avail=" + availableAssemblers.Count + " Used:" + String.Join(",", assemblersInUse.Keys) + " Need:" + String.Join(",", queueResources.Keys));
                foreach (var productors in assemblersInUse.Values)
                {
                    while (productors.Count > 1)
                    {
                        logger.write("Assembler clear except 1");
                        countClear++;
                        stopAssembler(productors[1]);
                    }
                }
            }
            var first = true;
            while (availableAssemblers.Count > 0)
            {
                var queuedStarted = false;
                foreach (string prod in queueResources.Keys)
                {
                    if (first && assemblersInUse.ContainsKey(prod)) continue;
                    queuedStarted |= startAssembler(prod);
                }
                if (!first && !queuedStarted) break;
                first = false;
            }

            String queued;
            if (queueResources.Count == 0)
            {
                queued = "Queue EMPTY";
            }
            else
            {
                queued = "Assemblers used: " + (allAssemblers.Count - availableAssemblers.Count).ToString() + " of " + allAssemblers.Count.ToString() + "\nQueued:\n";
                foreach (KeyValuePair<string, int> entry in queueResources)
                {
                    queued += "\n" + (CrushedQueued.Contains(entry.Key) ? "(CRUSHED) " : "") + entry.Key + ": " + entry.Value.ToString();
                }
            }


            var surfaces = new List<IMyTextPanel>();
            reScanObjectGroup(surfaces, QueueTag);
            if (surfaces.Count == 0)
            {
                Alarms.warn("There is no " + QueueTag + " screen");
            }
            else
            {
                surfaces.ForEach(surface => surface.WriteText(queued));
            }
        }

        private String getPercentVolume(String name, List<IMyCargoContainer> containers)
        {
            double space = 0, used = 0;
            foreach (var container in containers)
            {
                for (int j = 0; j < container.InventoryCount; j++)
                {
                    var inventory = container.GetInventory(j);
                    space += (double)inventory.MaxVolume;
                    used += (double)inventory.CurrentVolume;
                }
            }
            var volume = (int)(100 * (space == 0 ? 1 : used / space));
            if (volume > warningVolume)
            {
                var err = name + " volume too low (" + (100 - volume) + ")%";
                if (volume > alarmVolume) { Alarms.alarm(err); } else { Alarms.warn(err); }
            }

            return name + ": " + volume + "%";
        }

        public void calculateVolume()
        {
            var LCD = GridTerminalSystem.GetBlockWithName(cargoTag) as IMyTextSurface;
            if (LCD == null)
            {
                Alarms.warn("THERE IS NO " + cargoTag + " LCD");
                return;
            }

            LCD.WriteText(
                "Cargo Used: \n\n" +
                getPercentVolume("Components", componentCargos) + "\n" +
                getPercentVolume("Resources", resourcesCargos) + "\n" +
                getPercentVolume("User", userCargos) + "\n" +
                getPercentVolume("Ammo", ammoCargos) + "\n"
            );
        }

        public void h2generatorEnable(bool newEnable)
        {
            var h2gens = new List<IMyPowerProducer>();
            reScanObjects(h2gens, item => item.BlockDefinition.TypeId.ToString().Contains("Hydrogen") && !item.CustomName.Contains(SKIP));
            h2gens.ForEach(gen => gen.Enabled = newEnable);
            h2generatorStarted = newEnable;
        }

        public void checkRunGenerators()
        {
            var batteryParcent = getGridBatteryCharge();

            if (h2generatorStarted && (iceAlarm || batteryParcent > maxBatteryCharge))
            {
                h2generatorEnable(false);
            }
            else if (!iceAlarm && !h2generatorStarted && batteryParcent < minBatteryCharge)
            {
                h2generatorEnable(true);
            }
        }


        public void Main(string arg, UpdateType updateSource)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                _scheduler.Update();
                return;
            }

            logger.write("Main " + arg);
            if (arg.StartsWith(LogTag)) return;
            arg += ",,";
            var props = arg.Split(',');
            switch (props[0])
            {
                case "factoryStart":
                    if (!factories.ContainsKey(props[1]))
                    {
                        factories[props[1]] = new FactoryController(props[1], this);
                    }
                    factories[props[1]].start();
                    break;
                case "factoryStop":
                    if (!factories.ContainsKey(props[1]))
                    {
                        factories[props[1]] = new FactoryController(props[1], this);
                    }
                    factories[props[1]].stop();
                    break;
                default:
                    break;
            }
        }

        private void alarmNoObject(HashSet<string> foundObjects, Dictionary<string, int> minObj)
        {
            foreach (var objName in minObj.Keys)
            {
                if (!foundObjects.Contains(objName))
                {
                    Alarms.alarm("There is no " + objName);
                    if (objName == ICE)
                    {
                        iceAlarm = true;
                    }
                }
            }
        }


    } //@remove
} //@remove
