
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
using SpaceEngineers.UWBlockPrograms.Grid;
using SpaceEngineers.UWBlockPrograms.LogLibrary;
using static SpaceEngineers.UWBlockPrograms.LogLibrary.Program;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatus //@remove
{ //@remove
    public class Program : LogLibrary.Program //@remove
    { //@remove
        // Your code goes between the next #endregion and #region
        //all terminalblocks
        List<IMyTerminalBlock> allTBlocks = new List<IMyTerminalBlock>();
        //all armor blocks - be defined
        //Tags
        const string SKIP = "[SKIP]";
        const string StatusTag = "[STATUS]";
        const string RequestTag = "[REQUEST]";
        const string infoTag = "[INFO]";
        const string aimTag = "[AIM]";

        public new string LogTag = "[LOG]";
        const double BATTERY_MAX_LOAD = 0.95;

        Color mainColor = new Color(0, 255, 0);

        ArgParser args;
        bool gridStateSaved = false;
        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";
        string additionalStatus = "";

        bool checkDestroyedBlocks = true;
        bool lockedState = false;

        public new int LogMaxCount = 100;
        Log logger;

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

        //--------------------------------- rescan and config functions --------------------------------
        public class ArgParser
        {
            private Dictionary<string,string> modes;
            private List<string> actions;
            private Dictionary<string,int> values;
            public int Count
            {
                get; set;
            }
            public ArgParser(string arg)
            {
                List<string> argList = arg.Split(',').Select(a => a.Trim()).ToList();
                foreach(var elem in argList)
                {
                    List<string> currentElem = elem.Split('=').Select(a => a.Trim()).ToList();                    
                    if (currentElem.Count() > 1)
                    {
                        int val;
                        if (Int32.TryParse(currentElem[1], val))
                        {
                            values.Add(currentElem[0], val);
                        }
                        else
                        {
                            modes.Add(currentElem[0], currentElem[1]);                            
                        }
                    }
                    else
                    {
                        actions.Add(currentElem[0]);
                    }
                }
            }
            public bool isInModes(string argName)
            {
                return modes.ContainsKey(argName);
            }
            public bool isInActions(string argName)
            {
                return actions.Contains(argName);
            }            
            public string getModeValue(string argName)
            {
                if(modes.ContainsKey(argName))
                {
                    return modes[argName];
                }
                else
                {
                    return "";
                }
            }
        }

        //---------------------------- grid status info ----------------------------------


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
            if (controls.Count() > 0)
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
            string itemStr = "";
            foreach (var i in items)
            {
                int len = strsize - i.Key.Split('.')[1].Length - i.Value.ToString().Length;
                itemStr += i.Key.Split('.')[1] + " ";
                if (len >= 0)
                {
                    string t = new string(' ', len);
                    itemStr += t;
                }
                itemStr += i.Value.ToString() + "\n";
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
                JsonList damagedJson = new JsonList("Blocks"); ;
                statusEvents.Add(curEvent);
                curEvent.Add(new JsonPrimitive("Event", "damaged"));
                foreach (var b in gridDamagedBlocks)
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
                if (targets.Count() > 0)
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

        public List<string> getDestroyedBlocks()
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
            targets = getTurretsTargets();
            gridDamagedBlocks = getDamagedBlocks();
            gridDestroyedBlocks = getDestroyedBlocks();
            damagedBlockRatio = gridDamagedBlocks.Count() / allTBlocks.Count();
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
        public void saveGridState()
        {
            reScanObjectsLocal(allTBlocks);
            //todo save armor block state
            gridStateSaved = true;
        }

        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            logger = new Log(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string arg)
        {
            //GridTerminalSystem.GetBlockGroupWithName("");
            logger.write("Main " + arg);
            if (arg.StartsWith(LogTag)) return;
            args = new ArgParser(arg);
            List<IMyTextPanel> infoDisplays = new List<IMyTextPanel>();
            reScanObjectGroupLocal(infoDisplays, infoTag);
/*            foreach (var display in infoDisplays)
            {
                display.WriteText(showInventory(getGridInventory(), 30));
            }*/

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
                        saveGridState();
                        Echo("Grid state saved");
                        break;
                    default:
                        break;
                }
            }

            if (!gridStateSaved)
            {
                saveGridState();
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
                IGC.SendBroadcastMessage(statusChannelTag, statusMessage, TransmissionDistance.CurrentConstruct);

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

    }  //@remove
}  //@remove