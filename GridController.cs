#region Prelude
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
using VRage.Game.GUI.TextPanel;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatus
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
        //all terminalblocks
        List<IMyTerminalBlock> allTBlocks = new List<IMyTerminalBlock>();
        //all armor blocks - be defined

        //connect,Коннектор Низ Таскалище 02,TB Handle Connect Таскалище 02
        //Battery,TB Handle Start Таскалище 02
        //cargoLoad,,TB Handle Start Таскалище 02
        //maxSpeed,50


        //cargoLoad,,Таскало 01 TB handleCharged
        //cargoSave,
        //Battery,Таскало 01 TB handleCharged
        //connect,Таскало 01 TB handleConnected
        //batteryCharge,Recharge
        //batteryCharge,Auto
        //MyObjectBuilder_Ore.Ice: 3000		
        //MyObjectBuilder_Ore.Iron: 43000
        //MyObjectBuilder_Ore.Stone: 43000
        //MyObjectBuilder_Ingot.Iron: 42000
        //MyObjectBuilder_Ore.Gold: 42000
        //MyObjectBuilder_Component.SteelPlate: 3000
        //MyObjectBuilder_AmmoMagazine.NATO_25x184mm: 3000

        //Tags
        const string SKIP = "[SKIP]";
        const string StatusTag = "[STATUS]";
        const string RequestTag = "[REQUEST]";
        const string infoTag = "[INFO]";
        
        string LogTag = "[LOG]";
        const double BATTERY_MAX_LOAD = 0.95;

        Color mainColor = new Color(0, 255, 0);

        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";
        string additionalStatus = "";
        List<string> actions = new List<string>();

        bool setupcomplete = false;
        bool checkDestroyedBlocks = true;
        bool lockedState = false;

        int LogMaxCount = 100;
        int maxSpeed = 0;        

        //alert tresholds
        double energyTreshold = 0.25; //% of max capacity, default - 25%
        double gasTreshold = 0.25; //% of max capacity, default - 25%
        double uraniumTreshold = 0; //kg
        double damageTreshold = 0.2; //% of terminal blocks, default - 20%

        //grid info
        double gridCharge = 0;
        double gridGas = 0;
        double gridLoad = 0;
        double damagedBlockRatio = 0;
        double destroyedAmount = 0;
        List<string> gridDamagedBlocks = new List<string>();
        List<string> gridDestroyedBlocks = new List<string>();
        Dictionary<string, int> gridInventory = new Dictionary<string, int>();
        List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();

        //item order
        Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
        Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

        List<IMyRadioAntenna> antenna = new List<IMyRadioAntenna>();

        IMyBroadcastListener commandListener;
        IMyBroadcastListener statusListener;


        //---------------------------logger--------------------------------------------
        public class LogEntry
        {
            public string log;
            public int count;

            public LogEntry(string info)
            {
                log = info;
                count = 1;
            }

            public void inc()
            {
                count++;
            }
            public override string ToString() => $"x{count}: {log}";
        }

        public class Log
        {
            List<IMyTextPanel> surfaces = new List<IMyTextPanel>();
            private Program parent;
            List<LogEntry> logs = new List<LogEntry>();


            public void rescan()
            {
                parent.reScanObjectGroupLocal(surfaces, parent.LogTag);
            }
            public Log(Program program)
            {
                this.parent = program;
            }

            public void write(String info)
            {
                rescan();
                if (logs.Count > 0 && logs[0].log == info)
                {
                    logs[0].inc();
                }
                else
                {
                    logs.Insert(0, new LogEntry(info));
                }

                if (logs.Count > parent.LogMaxCount)
                {
                    logs.RemoveRange(parent.LogMaxCount, logs.Count - parent.LogMaxCount);
                }

                var result = String.Join("\n", logs);
                surfaces.ForEach((surface) => surface.WriteText(result));
            }
        }

        Log logger;

        public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
        {
            public new TValue this[TKey key]
            {
                get
                {
                    TValue val;
                    if (!TryGetValue(key, out val))
                    {
                        val = new TValue();
                        Add(key, val);
                    }
                    return val;
                }
                set { base[key] = value; }
            }
        }
        //--------------------------------- logger end -------------------------------------------------

        //--------------------------------- rescan and config functions --------------------------------
        public void parseArgs(string args)
        {
            //todo - move parse args here
            actions.Clear();

            //actions
        }
        
        public void reReadConfig(Dictionary<string, int> minResourses, String CustomData)
        {
            minResourses.Clear();

            foreach (String row in CustomData.Split('\n'))
            {
                if (row.Contains(":"))
                {
                    var tup = row.Split(':');
                    if (tup.Length != 2)
                    {
                        Echo("Err: " + row);
                        continue;
                    }
                    minResourses[tup[0].Trim()] = Convert.ToInt32(tup[1].Trim());
                }
            }
        }

        public void reScanObjectExact<T>(List<T> result, String name) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName == name);
        }
        public void reScanObjectExactLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName == name);
        }
        public void reScanObjectGroupLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName.Contains(name));
        }
        public void reScanObjectGroup<T>(List<T> result, String name) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName.Contains(name));
        }
        public void reScanObjectsLocal<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && check(item));
        }
        public void reScanObjectsLocal<T>(List<T> result) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid);
        }
        public void reScanObjects<T>(List<T> result) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result);
        }
        public void reScanObjects<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, check);
        }
        //-----------------------------------------------------------------

        //---------------------------- grid status info ----------------------------------
        public double percentOf(double current, double maximum)
        {
            return (maximum == 0 ? 1 : 1 * current / maximum);
        }
        public delegate void OutAction<T>(T x, ref double a, ref double b);
        public double getGridInfo<T>(OutAction<T> update, Func<T, bool> check = null) where T : class, IMyTerminalBlock
        {
            var objects = new List<T>();
            double current_capacity = 0;
            double max_capacity = 0;

            if (check == null)
            {
                reScanObjectsLocal(objects);
            }
            else
            {
                reScanObjectsLocal(objects, check);
            }

            objects.ForEach((obj) => update(obj, ref current_capacity, ref max_capacity));

            return percentOf(current_capacity, max_capacity);
        }

        public double getGridBatteryCharge()
        {
            return getGridInfo((IMyBatteryBlock batt, ref double current, ref double max) =>
            {
                current += batt.CurrentStoredPower;
                max += batt.MaxStoredPower;
            });
        }

        public double getGridGasAmount(string gasType)
        {
            return getGridInfo((IMyGasTank tank, ref double current, ref double max) =>
            {
                current += tank.Capacity * tank.FilledRatio;
                max += tank.Capacity;
            }, tank => tank.BlockDefinition.SubtypeId.Contains(gasType));
        }

        public double getGridUsedCargoSpace()
        {
            return getGridInfo((IMyCargoContainer cargo, ref double current, ref double max) =>
            {
                current += (double)cargo.GetInventory(0).CurrentVolume;
                max += (double)cargo.GetInventory(0).MaxVolume;
            });
        }

        public JsonList getEnemyTargetsData()
        {
            JsonList result = new JsonList("Targets");
            foreach (MyDetectedEntityInfo target in targets)
            {
                var t = new JsonObject("");
                t.Add(new JsonPrimitive("Name", target.Name));
                t.Add(new JsonPrimitive("Type", target.Type.ToString()));
                t.Add(new JsonPrimitive("Position", target.Position.ToString()));                
                result.Add(t);
            }
            return result;
        }

        private string getName(MyItemType type)
        {
            return type.TypeId + '.' + type.SubtypeId;
        }

        public Dictionary<string, int> getGridInventory()
        {
            var cargo_blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(cargo_blocks, b => b.HasInventory);
            var result = new DefaultDictionary<string, int>();
            var items = new List<MyInventoryItem>();
            foreach (var block in cargo_blocks)
            {
                items.Clear();
                block.GetInventory(0).GetItems(items);
                foreach (var item in items)
                {
                    result[getName(item.Type)] += (int)item.Amount;
                }
            }

            return result;
        }

        public bool isAttacked()
        {
            bool isTargets = (targets.Count() > 0);
            bool isLocked = lockedState;
            bool isLargeDamage = false;
            //isLargeDamage = (damagedBlockRatio > damageTreshold);
            bool isDestroyedBlocks = (destroyedAmount > 0);
            return (isTargets || isLocked || isLargeDamage || isDestroyedBlocks);
        }

/*        public bool damaged(out JsonList result)
        {
        }*/

        public bool isLowAmmo()
        {
            bool result = false;
            //todo
            return result;
        }

        public double getGridVelocity()
        {
            double result = 0;
            List<IMyShipController> controls = new List<IMyShipController>();
            reScanObjectsLocal(controls);
            if(controls.Count() > 0)
            {
                result = (double)controls[0].GetShipVelocities().LinearVelocity.Length();
            }
            return result;
        }

        public bool isLowFuel(out string ftype)
        {
            bool result = false;
            ftype = "";
            List<string> ftypes = new List<string>();
            if (gridCharge < energyTreshold)
            {
                result = true;
                ftypes.Add("\"energy\"");
            }
            //todo: check gas if hydrogen thrusters or engines in grid
            if (gridGas < gasTreshold)
            {
                result = true;
                ftypes.Add("\"gas\"");
            }
            //todo check uranium (if reactors)

            if (ftypes.Count > 0)
            {
                ftype = "[" + String.Join(",", ftypes) + "]";
            }
            return result;
        }        

        public string showInventory(Dictionary<string, int> items, int strsize)
        {
            return ""; //todo: bug https://cloud.garm.work/s/d4wtW8qgxf6EcLQ/preview

            string itemStr = "";

            foreach (var i in items)
            {
                string t = new string(' ', strsize - i.Key.Split('.')[1].Length - i.Value.ToString().Length);
                itemStr += i.Key.Split('.')[1] + " " + t + i.Value.ToString() + "\n";
                //test strsize - add logic if itemStr larger than strsize
            }
            return itemStr;
        }

        public void setAdditionalStatus(String s)
        {
            additionalStatus = s;

            var surface = Me.GetSurface(0);
            surface.Alignment = TextAlignment.CENTER;
            surface.FontColor = mainColor;
            surface.FontSize = 4;
            surface.WriteText(s);
        }

        public string getStatus()
        {
                string statusMessage = "";
                string ftype;
                List<string> events = new List<string>();
                var Status = new JsonObject("");
                statusMessage = "green";
                var statusEvents = new JsonList("Events");

                if (isLowAmmo())
                {
                    //todo check what type of ammo need and how many, add to details
                    statusMessage = "orange";
                    var curEvent = new JsonObject("");
                    statusEvents.Add(curEvent);
                    curEvent.Add(new JsonPrimitive("Event", "lowAmmo"));
                }

                if (gridDamagedBlocks.Count() > 0)
                {
                    statusMessage = "orange";
                    var curEvent = new JsonObject("");
                    JsonList damagedJson = new JsonList("Blocks");;
                    statusEvents.Add(curEvent);
                    curEvent.Add(new JsonPrimitive("Event", "damaged"));
                    foreach(var b in gridDamagedBlocks)
                    {
                        damagedJson.Add(new JsonPrimitive("", b));
                    }
                    curEvent.Add(damagedJson);
                }

                if (isLowFuel(out ftype))
                {
                    statusMessage = "orange";
                    var curEvent = new JsonObject("");
                    statusEvents.Add(curEvent);
                    curEvent.Add(new JsonPrimitive("Event", "lowFuel"));
                    curEvent.Add(new JsonPrimitive("fueltype", ftype));
                }

                if (isAttacked())
                {
                    statusMessage = "red";
                    var curEvent = new JsonObject("");
                    curEvent.Add(new JsonPrimitive("Event", "underAttack"));
                    curEvent.Add(new JsonPrimitive("Locked", lockedState.ToString()));
                    curEvent.Add(new JsonPrimitive("Damaged", damagedBlockRatio * 100));
                    curEvent.Add(new JsonPrimitive("Destroyed", destroyedAmount));
                    if(targets.Count() > 0)
                    {
                        statusEvents.Add(getEnemyTargetsData());
                    }
                }

                Status.Add(new JsonPrimitive("Name", Me.CubeGrid.CustomName));
                Status.Add(new JsonPrimitive("Additional", additionalStatus));
                Status.Add(new JsonPrimitive("Status", statusMessage));
                Status.Add(new JsonPrimitive("GasAmount", gridGas));
                Status.Add(new JsonPrimitive("BatteryCharge", gridCharge));
                Status.Add(new JsonPrimitive("CargoUsed", gridLoad));
                Status.Add(new JsonObject("Position", Me.GetPosition()));
                Status.Add(new JsonPrimitive("Velocity", getGridVelocity()));
                if (statusEvents.Count > 0)
                {
                    Status.Add(statusEvents);
                }

                //finish message
                statusMessage = Status.ToString(false);
                Echo(statusMessage);
                return statusMessage;

        }

        public List<MyDetectedEntityInfo> updateTurretsTargets()
        {
            List<MyDetectedEntityInfo> result = new List<MyDetectedEntityInfo>();
            List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
            reScanObjects(turrets);
            foreach (IMyLargeTurretBase t in turrets)
            {
                if (t.HasTarget)
                {
                    MyDetectedEntityInfo trg = t.GetTargetedEntity();
                    result.Add(trg);
                }
            }
            return result;
        }

        public List<string> updateDamagedBlocks()
        {
            List<string> result = new List<string>();
            List<IMyTerminalBlock> grid = new List<IMyTerminalBlock>();
            reScanObjectsLocal(grid);
            foreach (IMyTerminalBlock terminalBlock in grid)
            {
                IMySlimBlock slimBlock = terminalBlock.CubeGrid.GetCubeBlock(terminalBlock.Position);
                if (slimBlock.CurrentDamage > 0)
                {
                    result.Add(terminalBlock.DisplayNameText);
                }
            }
            return result;
        }

        public List<string> updateDestroyedBlocks()
        {
            List<string> result = new List<string>();
            if (checkDestroyedBlocks)
            {
                List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
                reScanObjectsLocal(currentState);
                destroyedAmount = allTBlocks.Count() - currentState.Count();
                //if block in allBlocks and not in current state add Block.CustomName to result;
            }
            return result;
        }

        public void updateGridInfo()
        {
            gridCharge = getGridBatteryCharge();
            gridGas = getGridGasAmount("Hydrogen");
            gridLoad = getGridUsedCargoSpace();
            gridInventory = getGridInventory();
            targets = updateTurretsTargets();
            gridDamagedBlocks = updateDamagedBlocks();
            gridDestroyedBlocks = updateDestroyedBlocks();
            damagedBlockRatio = gridDamagedBlocks.Count()/allTBlocks.Count();
            destroyedAmount = gridDestroyedBlocks.Count();
        }
        //===================================================================================

        //------------------------- car helper -----------------------------------------------
        private void setMaxSpeed(string v)
        {
            int newSpeed;
            if (!Int32.TryParse(v, out newSpeed))
            {
                Echo("Wrong int " + v);
                return;
            }

            maxSpeed = newSpeed;

            logger.write("Setup max speed");

            var irs = new List<IMyRemoteControl>();
            reScanObjectsLocal(irs, ir => ir.IsAutoPilotEnabled);
            irs.ForEach(ir => ir.SpeedLimit = newSpeed);
        }

        public void checkMaxSpeed()
        {
            var irs = new List<IMyRemoteControl>();
            reScanObjectsLocal(irs, ir => ir.IsAutoPilotEnabled);
            if (irs.Count != 1) return;
            var rc = irs[0];

            var distance = (Me.CubeGrid.GridIntegerToWorld(Me.Position) - rc.CurrentWaypoint.Coords).Length();
            if (distance < 90)
            {
                rc.SpeedLimit = (float)(maxSpeed * (distance + 10) / 100);
            }
        }

        public void cargoLoad(string group, String after)
        {
            logger.write("cargoLoad: " + group);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory);
            var needResources = new Dictionary<string, int>();

            var outerCargo = new List<IMyCargoContainer>();
            reScanObjects(outerCargo, b => b.CubeGrid != Me.CubeGrid);
            if (outerCargo.Count == 0)
            {
                logger.write("No external cargos");
                return;
            }

            bool full = true;
            double need = 0, found = 0;

            foreach (var block in blocks)
            {
                reReadConfig(needResources, block.CustomData);
                var needResCount = needResources.Values.Sum();
                if (needResCount == 0) continue;
                need += needResCount;
                var items = new List<MyInventoryItem>();
                for (int j = 0; j < block.InventoryCount; j++)
                {
                    block.GetInventory(j).GetItems(items);
                    foreach (var item in items)
                    {
                        var resourceName = getName(item.Type);
                        if (needResources.ContainsKey(resourceName))
                        {
                            needResources[resourceName] -= (int)item.Amount;
                            found += (int)item.Amount;
                        }
                    }
                }

                var currentfull = moveResources(
                    outerCargo,
                    block,
                    needResources.Where(r => r.Value > 0).ToDictionary(i => i.Key, i => i.Value)
                );
                if (!currentfull)
                {
                    logger.write("NotFull: " + block.CustomName);
                }
                full &= currentfull;
            }

            if (full)
            {
                setAdditionalStatus("");
                runTbByName(after);
            }
            else
            {
                setAdditionalStatus("C." + (int)(need == 0 ? 100 : 100 * found / need) + "%");
            }
        }

        private bool moveResources(List<IMyCargoContainer> outerCargo, IMyTerminalBlock block, Dictionary<string, int> dictionary)
        {
            logger.write("Move to " + dictionary.Keys.Count() + " " + outerCargo.Count() + " " + block.CustomName);
            if (!block.HasInventory || dictionary.Count == 0) return true;
            IMyInventory sourse, destination = block.GetInventory();
            if (destination.IsFull) return true;

            for (int i = 0; i < outerCargo.Count; i++)
            {
                if (outerCargo[i].CustomName.Contains(SKIP)) continue;

                for (int j = 0; j < outerCargo[i].InventoryCount; j++)
                {
                    var items = new List<MyInventoryItem>();
                    sourse = outerCargo[i].GetInventory(j);
                    sourse.GetItems(items);
                    if (!sourse.IsConnectedTo(destination)) continue;
                    for (int k = 0; k < items.Count; k++)
                    {
                        var item = items[k];
                        var resourceName = getName(item.Type);
                        if (dictionary.ContainsKey(resourceName) && dictionary[resourceName] > 0)
                        {
                            var countToMove = Math.Min(dictionary[resourceName], (int)item.Amount);
                            sourse.TransferItemTo(destination, k, null, true);
                            dictionary[resourceName] -= countToMove;
                        }
                    }
                }
            }
            return false;
        }

        public void cargoSave(string group)
        {
            logger.write("cargoSave: " + group);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory);
            var info = new List<String>();

            foreach (var block in blocks)
            {
                info.Clear();
                var items = new List<MyInventoryItem>();
                for (int j = 0; j < block.InventoryCount; j++)
                {
                    block.GetInventory(j).GetItems(items);
                    foreach (var item in items)
                    {
                        info.Add(getName(item.Type) + ": " + item.Amount.ToString());
                    }
                }
                block.CustomData = String.Join("\n", info);
            }
        }

        public void connect(String connectorName, String tbName)
        {
            var blocks = new List<IMyShipConnector>();
            reScanObjectExactLocal(blocks, connectorName);
            logger.write("Connect \"" + connectorName + "\" (" + blocks.Count + ")");
            foreach (var connector in blocks)
            {
                connector.Connect();
                if (connector.Status == MyShipConnectorStatus.Connected)
                {
                    runTbByName(tbName);
                    return;
                }
            }
        }

        public void runTbByName(String name)
        {
            var tbs = new List<IMyTimerBlock>();
            reScanObjectExactLocal(tbs, name);
            tbs.ForEach(tb => tb.Trigger());
        }

        public void batteryLoad(String after)
        {
            logger.write("batteryLoad");
            var curLoad = getGridBatteryCharge();
            if (curLoad > BATTERY_MAX_LOAD)
            {
                setAdditionalStatus("");
                runTbByName(after);
            }
            else
            {
                setAdditionalStatus("B." + (int)(100 * curLoad) + "%");
            }
        }

        public void batteryCharge(string type)
        {
            logger.write("batteryCharge: " + type);
            ChargeMode mode;
            if (!Enum.TryParse(type, out mode)) return;

            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            reScanObjectsLocal(batteries);
            batteries.ForEach(bat => bat.ChargeMode = mode);
        }

        //===========================================================================================

        //------------------------------------ arg commands ------------------------------------------
        public void fixGridState()
        {
            reScanObjectsLocal(allTBlocks);
            //todo save armor block state

        }

        public void Setup()
        {
            reScanObjectsLocal(antenna);

            if (antenna.Count() > 0)
            {
                Echo("Setup complete");
                setupcomplete = true;
            }
            else
            {
                Echo("Setup failed. No antenna found");
            }
        }

        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            logger = new Log(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string arg)
        {
            logger.write("Main " + arg);
            if (arg.StartsWith(LogTag)) return;

            List<IMyTextPanel> infoDisplays = new List<IMyTextPanel>();
            reScanObjectGroupLocal(infoDisplays, infoTag);
            foreach (var display in infoDisplays)
            {
                display.WriteText(showInventory(getGridInventory(), 30));
            }

            checkMaxSpeed();

            if (arg != "")
            {
                //change parameters
                // .ToList<String>()
                //todo myInit CustomData

                if (arg.Contains("=")) //todo: remove
                {
                    List<string> args = arg.Split('=').Select(a => a.Trim()).ToList();
                    string cmd = args[0];
                    string val = args[1];

                    switch (cmd)
                    {
                        case "ammo":
                            //todo: set ammo request
                            break;
                        case "items":
                            //todo: set items request
                            break;
                        case "energy":
                            energyTreshold = Int32.Parse(val);
                            break;
                        case "gas":
                            gasTreshold = Int32.Parse(val);
                            break;
                        case "damage":
                            damageTreshold = Int32.Parse(val);
                            break;
                        case "uranium":
                            uraniumTreshold = Int32.Parse(val);
                            break;
                        default:
                            Echo("Unknown argument");
                            break;
                    }
                }

                arg += ",,";
                var props = arg.Split(',');

                switch (props[0])
                {
                    case "Battery":
                        batteryLoad(props[1]);
                        break;
                    case "cargoSave":
                        cargoSave(props[1]);
                        break;
                    case "cargoLoad":
                        cargoLoad(props[1], props[2]);
                        break;
                    case "maxSpeed":
                        setMaxSpeed(props[1]);
                        break;
                    case "connect":
                        connect(props[1], props[2]);
                        break;
                    case "batteryCharge":
                        batteryCharge(props[1]);
                        break;
                    case "locked":
                        lockedState = true;
                        Echo("Grid is locked");
                        break;
                    case "unlocked":
                        lockedState = false;
                        Echo("Manual override lock status");
                        break;
                    case "fix":
                        fixGridState();
                        Echo("Grid state saved");
                        break;
                    default:
                        break;
                }
            }

            if (!setupcomplete)
            {
                //If setupcomplete is false, run Setup method.
                Echo("Running setup");
                Setup();
                fixGridState();
            }
            else
            {

                List<IMyTextPanel> status_displays = new List<IMyTextPanel>();
                List<IMyTextPanel> request_displays = new List<IMyTextPanel>();


                reScanObjectGroupLocal(status_displays, StatusTag);
                reScanObjectGroupLocal(request_displays, RequestTag);
                targets.Clear();
                updateGridInfo();
                string statusMessage = getStatus();
                status_displays.ForEach(display => display.WriteText(statusMessage));

                //statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
                IGC.SendBroadcastMessage(statusChannelTag, statusMessage);

                commandListener = IGC.RegisterBroadcastListener(commandChannelTag);
                while (commandListener.HasPendingMessage)
                {
                    string message;
                    MyIGCMessage newRequest = commandListener.AcceptMessage();
                    if (commandListener.Tag == commandChannelTag)
                    {
                        if (newRequest.Data is string)
                        {
                            message = newRequest.Data.ToString();
                            foreach (IMyTextSurface d in request_displays)
                            {
                                d.WriteText(message);
                            }
                        }
                    }
                }

            }
        }

    // ---------------------------------------------------------- json parser ----------------------------------------------------
        interface IJsonNonPrimitive
        {
            void Add(JsonElement child);
        }
        public class JsonList : JsonElement, IJsonNonPrimitive, ICollection<JsonElement>
        {
            private List<JsonElement> Values;

            public override JSONValueType ValueType
            {
                get
                {
                    return JSONValueType.LIST;
                }
            }

            public JsonElement this[int i]
            {
                get
                {
                    return Values[i];
                }
            }

            public int Count
            {
                get
                {
                    return Values.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public JsonList(string key)
            {
                Key = key;
                Values = new List<JsonElement>();
            }

            public void Add(JsonElement value)
            {
                Values.Add(value);
            }


            public override string ToString(bool pretty)
            {
                var result = "";
                if (Key != "")
                    result = "\"" + Key + (pretty ? "\": " : "\":");
                result += "[";
                foreach (var jsonObj in Values)
                {
                    var childResult = jsonObj.ToString(pretty);
                    if (pretty)
                        childResult = childResult.Replace("\n", "\n  ");
                    result += (pretty ? "\n  " : "") + childResult + ",";
                }
                result = result.Substring(0, result.Length - 1);
                result += (pretty ? "\n]" : "]");

                return result;
            }

            public void Clear()
            {
                Values.Clear();
            }

            public bool Contains(JsonElement item)
            {
                return Values.Contains(item);
            }

            public void CopyTo(JsonElement[] array, int arrayIndex)
            {
                Values.CopyTo(array, arrayIndex);
            }

            public bool Remove(JsonElement item)
            {
                return Values.Remove(item);
            }

            private IEnumerable<JsonElement> Elements()
            {
                foreach (var value in Values)
                {
                    yield return value;
                }
            }

            public IEnumerator<JsonElement> GetEnumerator()
            {
                return Elements().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class JsonObject : JsonElement, IJsonNonPrimitive
        {
            private Dictionary<string, JsonElement> Value;

            public override JSONValueType ValueType
            {
                get
                {
                    return JSONValueType.NESTED;
                }
            }

            public JsonElement this[string key]
            {
                get
                {
                    return Value[key];
                }
            }

            public Dictionary<string, JsonElement>.KeyCollection Keys
            {
                get
                {
                    return Value.Keys;
                }
            }

            public bool ContainsKey(string key)
            {
                return Value.ContainsKey(key);
            }

            public JsonElement GetValueOrDefault(string key)
            {
                if (ContainsKey(key))
                    return this[key];
                return null;
            }

            public JsonObject(string key = "")
            {
                Key = key;
                Value = new Dictionary<string, JsonElement>();
            }
            public JsonObject(string key = "", Vector3D? vector = null)
            {
                Key = key;
                Value = new Dictionary<string, JsonElement>();
                if (vector != null)
                {
                    Vector3D tmp = (Vector3D)vector;
                    Add(new JsonPrimitive("X", tmp.X));
                    Add(new JsonPrimitive("Y", tmp.Y));
                    Add(new JsonPrimitive("Z", tmp.Z));
                }
            }

            public void Add(JsonElement jsonObj)
            {
                Value.Add(jsonObj.Key, jsonObj);
            }

            public override string ToString(bool pretty = true)
            {
                var result = "";
                if (Key != "" && Key != null)
                    result = "\"" + Key + (pretty ? "\": " : "\":");
                result += "{";
                foreach (var kvp in Value)
                {
                    var childResult = kvp.Value.ToString(pretty);
                    if (pretty)
                        childResult = childResult.Replace("\n", "\n  ");
                    result += (pretty ? "\n  " : "") + childResult + ",";
                }
                result = result.Substring(0, result.Length - 1);
                result += (pretty ? "\n}" : "}");

                return result;
            }
        }

        public class JsonPrimitive : JsonElement
        {
            public string Value = null;
            public double? dValue = null;
            public int? iValue = null;

            public override JSONValueType ValueType
            {
                get
                {
                    return JSONValueType.PRIMITIVE;
                }
            }

            public JsonPrimitive(string key, string value)
            {
                Key = key;
                Value = value;
            }
            public JsonPrimitive(string key, double value)
            {
                Key = key;
                dValue = value;
            }
            public JsonPrimitive(string key, float value)
            {
                Key = key;
                dValue = value;
            }
            public JsonPrimitive(string key, int value)
            {
                Key = key;
                iValue = value;
            }

            public override void SetKey(string key)
            {
                base.SetKey(key);
            }

            public void SetValue(string value)
            {
                Value = value;
            }


            public T GetValue<T>()
            {
                object value = null;
                if (typeof(T) == typeof(string))
                {
                    value = Value;
                }
                else if (typeof(T) == typeof(int))
                {
                    value = Int32.Parse(Value);
                }
                else if (typeof(T) == typeof(float))
                {
                    value = Single.Parse(Value);
                }
                else if (typeof(T) == typeof(double))
                {
                    value = Double.Parse(Value);
                }
                else if (typeof(T) == typeof(char))
                {
                    value = Char.Parse(Value);
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    value = DateTime.Parse(Value);
                }
                else if (typeof(T) == typeof(decimal))
                {
                    value = Decimal.Parse(Value);
                }
                else if (typeof(T) == typeof(bool))
                {
                    value = Boolean.Parse(Value);
                }
                else if (typeof(T) == typeof(byte))
                {
                    value = Byte.Parse(Value);
                }
                else if (typeof(T) == typeof(uint))
                {
                    value = UInt32.Parse(Value);
                }
                else if (typeof(T) == typeof(short))
                {
                    value = short.Parse(Value);
                }
                else if (typeof(T) == typeof(long))
                {
                    value = long.Parse(Value);
                }
                /*else if (typeof(T) == typeof(List<JsonObject>))
                {
                    var values = GetBody()?.Values;
                    if (values == null)
                        value = new List<JsonObject>();
                    else
                        value = new List<JsonObject>(values);
                }
                else if (typeof(T) == typeof(Dictionary<string, JsonObject>))
                {
                    value = GetBody();
                }*/
                else
                {
                    throw new ArgumentException("Invalid type '" + typeof(T).ToString() + "' requested!");
                }

                return (T)value;
            }

            public bool TryGetValue<T>(out T result)
            {
                try
                {
                    result = GetValue<T>();
                    return true;
                }
                catch (Exception)
                {
                    result = default(T);
                    return false;
                }
            }

            public override string ToString(bool pretty = true)
            {
                if (Value == null && iValue == null && dValue == null)
                    return "";
                var result = "";
                if (Key != "" && Key != null)
                    result = "\"" + Key + (pretty ? "\": " : "\":");

                if (Value != null)
                {
                    result += "\"" + Value + "\"";
                }
                else
                {
                    result += (iValue == null ? dValue : iValue).ToString();
                }


                return result;
            }
        }
        public abstract class JsonElement
        {
            public enum JSONValueType { NESTED, LIST, PRIMITIVE }
            public string Key { get; protected set; }

            public bool IsPrimitive
            {
                get
                {
                    return ValueType == JSONValueType.PRIMITIVE;
                }
            }
            public abstract JSONValueType ValueType { get; }

            public virtual void SetKey(string key)
            {
                Key = key;
            }

            public override string ToString()
            {
                return ToString(true);
            }

            public abstract string ToString(bool pretty = true);

        }
        public class JSON
        {
            enum JSONPart { KEY, KEYEND, VALUE, VALUEEND }

            private int LastCharIndex;
            public string Serialized { get; private set; }
            private bool ReadOnly;

            public int Progress
            {
                get
                {
                    if (Serialized.Length == 0) return 999;
                    return 100 * Math.Max(0, LastCharIndex) / Serialized.Length;
                }
            }


            public JSON(string serialized, bool readOnly = true)
            {
                Serialized = serialized;
                ReadOnly = readOnly;
            }


            public JsonElement Parse()
            {
                LastCharIndex = -1;
                JSONPart Expected = JSONPart.VALUE;
                Stack<Dictionary<string, JsonElement>> ListStack = new Stack<Dictionary<string, JsonElement>>();
                Stack<IJsonNonPrimitive> JsonStack = new Stack<IJsonNonPrimitive>();
                IJsonNonPrimitive CurrentNestedJsonObject = null;
                IJsonNonPrimitive LastNestedJsonObject = null;
                //Func<object, JsonObject> Generator = JsonObject.NewJsonObject("", readOnly);
                var trimChars = new char[] { '"', '\'', ' ', '\n', '\r', '\t', '\f' };
                string Key = "";
                var keyDelims = new char[] { '}', ':' };
                var valueDelims = new char[] { '{', '}', ',', '[', ']' };
                var expectedDelims = valueDelims;
                var charIndex = -1;
                bool isInsideList = false;

                while (LastCharIndex < Serialized.Length - 1)
                {
                    charIndex = Serialized.IndexOfAny(expectedDelims, LastCharIndex + 1);
                    if (charIndex == -1)
                        throw new UnexpectedCharacterException(expectedDelims, "EOF", LastCharIndex);

                    char foundChar = Serialized[charIndex];
                    if (Expected == JSONPart.VALUE)
                    {
                        //Console.WriteLine("Expecting Value...");
                        //Console.WriteLine("Found " + Serialized[charIndex] + " (" + charIndex + ")");
                        switch (foundChar)
                        {
                            case '[':
                                CurrentNestedJsonObject = new JsonList(Key);
                                JsonStack.Peek().Add(CurrentNestedJsonObject as JsonElement);
                                JsonStack.Push(CurrentNestedJsonObject);
                                //Console.WriteLine("List started");
                                break;
                            case '{':
                                //Console.WriteLine("Found new JsonObject");
                                CurrentNestedJsonObject = new JsonObject(Key);
                                if (JsonStack.Count > 0)
                                    JsonStack.Peek().Add(CurrentNestedJsonObject as JsonElement);
                                JsonStack.Push(CurrentNestedJsonObject);
                                Expected = JSONPart.KEY;
                                expectedDelims = keyDelims;
                                break;
                            case ',':
                            case '}':
                            case ']':
                                var value = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                                //Console.WriteLine("value is: '" + value + "'");
                                JsonStack.Peek().Add(new JsonPrimitive(Key, value));
                                if (foundChar == '}' || foundChar == ']')
                                {
                                    /*if (foundChar == ']')
                                    {
                                        Console.WriteLine("Leaving List...");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Leaving JsonObject...");
                                    }*/
                                    if (charIndex < Serialized.Length - 1 && Serialized[charIndex + 1] == ',')
                                        charIndex++;
                                    LastNestedJsonObject = JsonStack.Pop();
                                }
                                break;
                        }

                        isInsideList = JsonStack.Count == 0 || JsonStack.Peek() is JsonList;
                        if (isInsideList)
                        {
                            Key = null;
                            Expected = JSONPart.VALUE;
                            expectedDelims = valueDelims;
                        }
                        else
                        {
                            Expected = JSONPart.KEY;
                            expectedDelims = keyDelims;
                        }
                    }
                    else if (Expected == JSONPart.KEY)
                    {
                        //Console.WriteLine("Expecting Key...");
                        //Console.WriteLine("Found " + Serialized[charIndex] + " (" + charIndex + ")");

                        switch (Serialized[charIndex])
                        {
                            case ':':
                                Key = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                                //Console.WriteLine("key is: '" + Key + "'");
                                //Generator = JsonObject.NewJsonObject(Key, readOnly);
                                Expected = JSONPart.VALUE;
                                expectedDelims = valueDelims;
                                break;
                            case '}':
                                //Console.WriteLine("Leaving JsonObject...");
                                if (charIndex < Serialized.Length - 1 && Serialized[charIndex + 1] == ',')
                                    charIndex++;
                                LastNestedJsonObject = JsonStack.Pop();
                                break;
                            default:
                                //Console.WriteLine($"Invalid character found: '{Serialized[charIndex]}', expected ':'!");
                                break;
                        }
                    }

                    LastCharIndex = charIndex;
                }
                if (JsonStack.Count > 0)
                {
                    throw new ParseException("StackCount " + JsonStack.Count, LastCharIndex);
                }
                return LastNestedJsonObject as JsonElement;
            }

            private class ParseException : Exception
            {
                public ParseException(string message, int position = -1)
                    : base("PARSE ERROR" + (position == -1 ? "" : " after char " + position.ToString()) + ": " + message) { }

            }

            private class UnexpectedCharacterException : ParseException
            {
                public UnexpectedCharacterException(char[] expected, string received, int position = -1)
                    : base("Expected one of [ '" + string.Join("', '", expected) + "' ] but received " + received + "!", position)
                { }

            }

        }

        #region PreludeFooter
    }
}
#endregion#region Prelude