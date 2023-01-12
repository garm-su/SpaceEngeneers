﻿using System;
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
// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.Sorter //@remove
{ //@remove
    public sealed class Program : LogLibrary.Program //@remove
    { //@remove
        // Your code goes between the next #endregion and #region

        public new string LogTag = "[LOG BASE CONTROLLER]";
        public new int LogMaxCount = 100;

        string
            SKIP = "[SKIP]",
            ComponentsTag = "[Components]",
            UserTag = "[User]", //todo: money here
            ResourcesTag = "[Resources]",
            QueueTag = "[QUEUE]",

            AmmoTag = "[Ammo]",

            cargoTag = "[CARGO]",
            alarmTag = "[ALARM]",
            infoTag = "[INFO]";

        const double
            warningVolume = 85,
            alarmVolume = 95,
            warningCount = 50,
            alarmCount = 25;
        //MyObjectBuilder_Component / MyObjectBuilder_PhysicalObject / MyObjectBuilder_PhysicalGunObject / MyObjectBuilder_ConsumableItem / MyObjectBuilder_AmmoMagazine / MyObjectBuilder_Datapad / MyObjectBuilder_Ingot

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


        public class MessageOfType
        {
            public List<string> messages;
            public bool was;

            public MessageOfType()
            {
                this.messages = new List<String>();
                this.was = false;
            }

            public void next()
            {
                this.was = this.messages.Count > 0;
                this.messages.Clear();
            }

            public void add(String mess)
            {
                this.messages.Add(mess);
            }
        }

        public class Messaging
        {
            private bool alarmActive;
            private MessageOfType alarmMess;
            private MessageOfType warningMess;
            private MessageOfType infoMess;

            readonly string[] animFrames = new[] { "|---", "-|--", "--|-", "---|", "--|-", "-|--" };
            int runningFrame;
            int runningMult;

            public Messaging()
            {
                alarmActive = false;
                alarmMess = new MessageOfType();
                warningMess = new MessageOfType();
                infoMess = new MessageOfType();
                runningFrame = 0;
                runningMult = 10;
            }

            public void alarm(String message)
            {
                alarmMess.add(message);
            }
            public void info(String message)
            {
                infoMess.add(message);
            }
            public void warn(String message)
            {
                warningMess.add(message);
            }

            public void next()
            {
                runningFrame = (runningFrame + 1) % (animFrames.Count() * runningMult);
                warningMess.next();
                infoMess.next();
                alarmMess.next();
            }

            public String getInfo()
            {
                return String.Join("\n", infoMess.messages);
            }

            public string getError()
            {
                return animFrames[runningFrame / runningMult] + "\n" + String.Join("\n", (alarmMess.messages.Count() > 0 ? alarmMess : warningMess).messages);
            }
        }

        Messaging Alarms = new Messaging();



        List<FactoryController> factories = new List<FactoryController> {
           new FactoryController("Factory Small Projector", "LCD Factory")
        };



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


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            foreach (var fc in factories) fc.connect(this);
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


        public String number(double count)
        {
            var prefix = "  ";
            if (count >= 1e6)
            {
                count /= 1e6;
                prefix = "M";
            }
            else if (count >= 1e3)
            {
                count /= 1e3;
                prefix = "K";
            }

            return String.Format("{0,3:0.0}", count) + prefix;

        }


        public class FactoryController
        {
            public IMyProjector projector { get; set; }
            public IMyTextSurface LCD { get; set; }
            public IMyCubeGrid CubeGrid { get; set; }
            private string LCD_name;
            private string projector_name;

            private Program parent;
            public FactoryController(string projector, string LCD)
            {
                this.LCD_name = LCD;
                this.projector_name = projector;
            }
            public void connect(Program program)
            {
                this.parent = program;
                this.projector = parent.GridTerminalSystem.GetBlockWithName(this.projector_name) as IMyProjector;
                this.LCD = parent.GridTerminalSystem.GetBlockWithName(this.LCD_name) as IMyTextSurface;
                this.CubeGrid = this.projector.CubeGrid;
            }

            public void stopEngines()
            {
                var thrusters = new List<IMyThrust>();
                this.parent.GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters,
                    item => item.CubeGrid == this.CubeGrid
                );
                foreach (var thruster in thrusters)
                {
                    thruster.Enabled = false;
                }
            }

            public void check()
            {
                if (!this.projector.IsProjecting || this.projector.TotalBlocks == 0) return;

                stopEngines();

                String result = "Progression: \n";
                result += String.Format("Buildable: {0}\n", this.projector.BuildableBlocksCount);
                int remaining = this.projector.RemainingBlocks - this.projector.RemainingArmorBlocks;

                result += String.Format("Remaining armor: {0}%\n", 100 * this.projector.RemainingArmorBlocks / this.projector.TotalBlocks);
                result += String.Format("Remaining actions: {0}%\n", 100 * remaining / this.projector.TotalBlocks);

                result += String.Format("\nProgression: {0}\n", 100 * (1 - this.projector.RemainingBlocks / this.projector.TotalBlocks));

                // Dictionary<MyDefinitionBase, int> RemainingBlocksPerType { get; }
                result += "\n" + this.projector.DetailedInfo;

                this.LCD.WriteText(result);
            }
        }

        public void calculateCount()
        {
            MyInventoryItem item;
            IMyInventory sourse, destination;
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
                            if (item.Type.TypeId == INGOT && block is IMyAssembler) continue;
                            if (item.Type.TypeId == AMMO && block is IMyLargeTurretBase) continue;
                            if (item.Type.TypeId == AMMO && block is IMyUserControllableGun) continue;

                            if (!block.CustomName.Contains(ComponentsTag) && MainContainerTypes.Contains(item.Type.TypeId))
                            {
                                foreach (var cargo in componentCargos)
                                {
                                    destination = cargo.GetInventory();
                                    if (!destination.IsFull && sourse.IsConnectedTo(destination))
                                    {
                                        sourse.TransferItemTo(destination, k, null, true);
                                        moved++;
                                        break;
                                    }
                                }
                            }
                            if (!block.CustomName.Contains(ResourcesTag) && ResourcesContainerTypes.Contains(item.Type.TypeId))
                            {
                                foreach (var cargo in resourcesCargos)
                                {
                                    destination = cargo.GetInventory();
                                    if (!destination.IsFull && sourse.IsConnectedTo(destination))
                                    {
                                        sourse.TransferItemTo(destination, k, null, true);
                                        moved++;
                                        break;
                                    }
                                }
                            }
                            if (!block.CustomName.Contains(UserTag) && UserContainerTypes.Contains(item.Type.TypeId))
                            {
                                foreach (var cargo in userCargos)
                                {
                                    destination = cargo.GetInventory();
                                    if (!destination.IsFull && sourse.IsConnectedTo(destination))
                                    {
                                        sourse.TransferItemTo(destination, k, null, true);
                                        moved++;
                                        break;
                                    }
                                }
                            }

                            if (!block.CustomName.Contains(AmmoTag) && AmmoContainerTypes.Contains(item.Type.TypeId))
                            {
                                foreach (var cargo in ammoCargos)
                                {
                                    destination = cargo.GetInventory();
                                    if (!destination.IsFull && sourse.IsConnectedTo(destination))
                                    {
                                        sourse.TransferItemTo(destination, k, null, true);
                                        moved++;
                                        break;
                                    }
                                }
                            }

                            //todo in one loop and ammo moving
                        }
                    }
                }
            }
            Alarms.info("Moved: " + moved.ToString());// MyObjectBuilder_Component \ MyObjectBuilder_Ore \ MyObjectBuilder_AmmoMagazine \ MyObjectBuilder_ConsumableItem \ MyObjectBuilder_PhysicalGunObject \ MyObjectBuilder_PhysicalObject \ MyObjectBuilder_Ingot \ MyObjectBuilder_Datapad \ MyObjectBuilder_GasContainerObject \ MyObjectBuilder_OxygenContainerObject
        }
        public void printCount(string panelTextInit, string terminalName, List<String> resoursesType, HashSet<string> foundObjects)
        {
            IMyTextSurface surface = GridTerminalSystem.GetBlockWithName(terminalName) as IMyTextSurface;
            if (surface == null)
            {
                Alarms.warn("There is no surface " + terminalName);
                return;
            }
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
                            if (percent < alarmCount) { Alarms.alarm(err); } else { Alarms.warn(err); }
                        }
                    }
                }
            }
            surface.WriteText(panelText);
        }



        public void calculateQueueCount()
        {
            //MyProductionItem item;
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

            Echo("1");


            var needAssemblers = queueResources.Where(res => !assemblersInUse.ContainsKey(res.Key)).Count();
            var nowSeveralAssemblers = availableAssemblers.Count >= needAssemblers;
            Echo("2");

            if (!nowSeveralAssemblers) //stop all except one on all resources
            {
                foreach (var productors in assemblersInUse.Values)
                {
                    Echo("3");
                    while (productors.Count > 1)
                    {
                        Echo("Assembler clear except 1");
                        countClear++;
                        stopAssembler(productors[1]);
                    }
                    Echo("4");
                }
            }
            Echo("5");
            while (availableAssemblers.Count > 0)
            {
                var queuedStarted = false;
                foreach (string prod in queueResources.Keys)
                {
                    queuedStarted |= startAssembler(prod);
                }
                if (!queuedStarted) break;
            }
            Echo("7");

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

            IMyTextSurface surface = GridTerminalSystem.GetBlockWithName(QueueTag) as IMyTextSurface;
            if (surface == null)
            {
                Alarms.warn("There is no " + QueueTag + " screen");
            }
            else
            {
                surface.WriteText(queued);
            }
        }

        public bool reReadConfig(String name, Dictionary<string, int> res)
        {
            var config = GridTerminalSystem.GetBlockWithName(name);
            if (config == null) return false;

            foreach (String row in config.CustomData.Split('\n'))
            {
                if (row.Contains(":"))
                {
                    var tup = row.Split(':');
                    if (tup.Length != 2)
                    {
                        Echo("Err: " + row);
                        continue;
                    }
                    res[tup[0].Trim()] = Convert.ToInt32(tup[1].Trim());
                }
            }

            return true;
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

        public void Main(string argument, UpdateType updateSource)
        {
            Alarms.next();

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

            minComponents.Clear();
            minResources.Clear();


            if (!reReadConfig(ResourcesTag, minResources)) Alarms.warn("There is no " + ResourcesTag + " screen");
            if (!reReadConfig(ComponentsTag, minComponents)) Alarms.warn("There is no " + ComponentsTag + " screen");
            if (!reReadConfig(AmmoTag, minComponents)) Alarms.warn("There is no " + AmmoTag + " screen");

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

            foreach (var fc in factories) fc.check();

            calculateVolume();

            var infoLCD = GridTerminalSystem.GetBlockWithName(infoTag) as IMyTextSurface;
            if (infoLCD == null)
            {
                Alarms.warn("THERE IS NO " + infoTag + " LCD");
            }
            else
            {
                infoLCD.WriteText(Alarms.getInfo());
            }

            var alarmLCD = GridTerminalSystem.GetBlockWithName(alarmTag) as IMyTextSurface;
            if (alarmLCD == null)
            {
                Echo("THERE IS NO " + alarmTag + " LCD");
            }
            else
            {
                alarmLCD.WriteText(Alarms.getError());
            }
        }

        private void alarmNoObject(HashSet<string> foundObjects, Dictionary<string, int> minObj)
        {
            foreach (var objName in minObj.Keys)
            {
                if (!foundObjects.Contains(objName))
                {
                    Alarms.alarm("There is no " + objName);
                }
            }
        }


    } //@remove
} //@remove