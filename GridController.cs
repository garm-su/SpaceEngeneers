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


        const string SKIP = "[SKIP]";
        const string StatusTag = "[STATUS]";
        const string RequestTag = "[REQUEST]";
        const string infoTag = "[INFO]";
        const double BATTERY_MAX_LOAD = 0.95;
        Color mainColor = new Color(0, 255, 0);

        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";

        string additionalStatus = "";

        bool setupcomplete = false;
        bool checkDestroyedBlocks = true;
        bool lockedState = false;
        double damagedBlockRatio = 0;
        double destroyedAmount = 0;

        int energyTreshold = 25; //% of max capacity, default - 25%
        int gasTreshold = 25; //% of max capacity, default - 25%
        int uraniumTreshold = 0; //kg
        int damageTreshold = 20; //% of terminal blocks, default - 20%

        List<IMyRadioAntenna> antenna = new List<IMyRadioAntenna>();
        List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();
        Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
        Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

        IMyBroadcastListener commandListener;
        IMyBroadcastListener statusListener;

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

        public double percentOf(double current, double maximum)
        {
            return Math.Round(maximum == 0 ? 100 : 100 * current / maximum);
        }
        public delegate void OutAction<T>(T x, ref double a, ref double b);
        public double getGridInfo<T>(OutAction<T> update, Func<T, bool> check = null) where T : class, IMyTerminalBlock
        {
            var objects = new List<T>();
            double current_capacity = 0;
            double max_capacity = 0;

            if (check == null) {
                reScanObjectsLocal(objects);
            } else {
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


        private string getName(MyItemType type)
        {
            return type.TypeId + '.' + type.SubtypeId;
        }

        private void maxSpeed(string v)
        {
            int newSpeed;
            if (!Int32.TryParse(v, out newSpeed))
            {
                Echo("Wrong int " + v);
                return;
            }

            Echo("Setup max speed");

            var irs = new List<IMyRemoteControl>();
            reScanObjectsLocal(irs, ir => ir.IsAutoPilotEnabled);
            irs.ForEach(ir => ir.SpeedLimit = newSpeed);
        }

        public void cargoLoad(string group, String after)
        {
            Echo("cargoLoad: " + group);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory);
            var outerCargo = new List<IMyCargoContainer>();
            var needResources = new Dictionary<string, int>();
            reScanObjectsLocal(outerCargo, b => b.HasInventory);

            bool full = true;
            double need = 0, found = 0;

            foreach (var block in blocks)
            {
                reReadConfig(needResources, block.CustomData);
                need += needResources.Values.Sum();
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
                    Echo("NotFull: " + block.CustomName);
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
            Echo("Move to " + block.CustomName + " " + dictionary.Keys.Count());
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
            Echo("cargoSave: " + group);
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
            reScanObjectsLocal(blocks, b => b.HasInventory);
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

        public void setAdditionalStatus(String s)
        {
            additionalStatus = s;

            var surface = Me.GetSurface(0);
            surface.Alignment = TextAlignment.CENTER;
            surface.FontColor = mainColor;
            surface.FontSize = 4;
            surface.WriteText(s);
        }

        public void runTbByName(String name)
        {
            var tbs = new List<IMyTimerBlock>();
            reScanObjectExactLocal(tbs, name);
            tbs.ForEach(tb => tb.Trigger());
        }

        public void batteryLoad(String after)
        {
            Echo("batteryLoad");
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
            Echo("batteryCharge: " + type);
            ChargeMode mode;
            if (!Enum.TryParse(type, out mode)) return;

            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            reScanObjectsLocal(batteries);
            batteries.ForEach(bat => bat.ChargeMode = mode);
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

        public string showInventory(Dictionary<string, int> items, int strsize)
        {

            string itemStr = "";

            foreach (var i in items)
            {
                string t = new string(' ', strsize - i.Key.Split('.')[1].Length - i.Value.ToString().Length);
                itemStr += i.Key.Split('.')[1] + " " + t + i.Value.ToString() + "\n";
                //test strsize - add logic if itemStr larger than strsize
            }
            return itemStr;
        }

        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void fixGridState()
        {
            reScanObjectsLocal(allTBlocks);
            //todo save armor block state

        }

        public string getMyCoords()
        {
            string result = "";
            result = Me.GetPosition().ToString();
            return result;
        }

        public bool isAttacked()
        {
            //is turrets locked and target is grid
            List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
            reScanObjects(turrets);
            bool isTarget = false;
            foreach (IMyLargeTurretBase t in turrets)
            {
                if (t.HasTarget)
                {
                    MyDetectedEntityInfo trg = t.GetTargetedEntity();
                    if (trg.Type != MyDetectedEntityType.None || trg.Type != MyDetectedEntityType.FloatingObject || trg.Type != MyDetectedEntityType.Meteor || trg.Type != MyDetectedEntityType.Planet)
                    {
                        targets.Add(trg);
                        isTarget = true;
                    }
                }
            }
            bool isLocked = false;
            if (lockedState)
            {        //lockedState initiate by cockpit, run PB with argument "locked"
                isLocked = true;
            }

            bool isLargeDamage = false;
            //isLargeDamage = (damagedBlockRatio > (damageTreshold/100));

            bool isDestroyedBlocks = false;
            if (checkDestroyedBlocks)
            {
                List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
                reScanObjectsLocal(currentState);
                isDestroyedBlocks = currentState.Count() != allTBlocks.Count();
                destroyedAmount = allTBlocks.Count() - currentState.Count();
            }

            return (isTarget || isLocked || isLargeDamage || isDestroyedBlocks);
        }

        public string getDamagedBlocks()
        {
            List<string> result = new List<string>();
            List<IMySlimBlock> slim = new List<IMySlimBlock>();
            List<IMyTerminalBlock> grid = new List<IMyTerminalBlock>();

            double damagedCounter = 0;

            reScanObjectsLocal(grid);

            foreach (IMyTerminalBlock terminalBlock in grid)
            {
                IMySlimBlock slimBlock = terminalBlock.CubeGrid.GetCubeBlock(terminalBlock.Position);
                slim.Add(slimBlock);
                if (slimBlock.CurrentDamage > 0)
                {
                    damagedCounter = damagedCounter + 1;
                    result.Add("\"" + terminalBlock.DisplayNameText + "\"");
                }
            }
            damagedBlockRatio = damagedCounter / allTBlocks.Count();
            return String.Join(",", result);
        }

        public bool isDamaged()
        {
            return getDamagedBlocks() != "";
        }

        public bool isLowAmmo()
        {
            bool result = false;
            //todo
            return result;
        }

        public double getGridVelocity()
        {
            return 0; //sometimes zero controls

            double result = 0;
            List<IMyShipController> controls = new List<IMyShipController>();
            reScanObjectsLocal(controls);
            result = (double)controls[0].GetShipVelocities().LinearVelocity.Length();
            return result;
        }

        public bool isLowFuel(out string ftype)
        {
            bool result = false;
            //check battery charges
            ftype = "";
            List<string> ftypes = new List<string>();
            if (getGridBatteryCharge() < energyTreshold)
            {
                result = true;
                ftypes.Add("\"energy\"");
            }
            //check gas
            if (getGridGasAmount("Hydrogen") < gasTreshold)
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

        public string getEnemyTargetsData()
        {
            string result = "";
            List<string> t = new List<string>();
            foreach (MyDetectedEntityInfo target in targets)
            {
                result = result + "{\"Name\":\"" + target.Name + "\",";
                result = result + "\"Type\":\"" + target.Type.ToString() + "\",";
                result = result + "\"Position\":\"" + target.Position.ToString() + "\"}";
                t.Add(result);
                result = "";
            }
            if (t.Count > 0)
            {
                result = "[" + String.Join(",", t) + "]";
            }
            return result;
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

        public void Main(string arg)
        {
            List<IMyTextPanel> infoDisplays = new List<IMyTextPanel>();
            reScanObjectGroupLocal(infoDisplays, infoTag);
            foreach (var display in infoDisplays)
            {
                display.WriteText(showInventory(getGridInventory(), 30));
            }

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
                        maxSpeed(props[1]);
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
                targets.Clear();
                string statusMessage = "";
                string ftype;
                List<string> events = new List<string>();
                List<IMyTextPanel> status_displays = new List<IMyTextPanel>();
                List<IMyTextPanel> request_displays = new List<IMyTextPanel>();

                statusMessage = "\"green\"";
                if (isDamaged() || isLowAmmo() || isLowFuel(out ftype))
                {
                    statusMessage = "\"orange\"";
                    if (isLowAmmo())
                    {
                        //todo check what type of ammo need and how many, add to details
                        string tmp = "{\"Event\":\"lowAmmo\"}";
                        events.Add(tmp);
                    }
                    if (isDamaged())
                    {
                        string tmp = "{\"event\":\"damaged\"";
                        tmp = tmp + ",\"blocks\":[" + getDamagedBlocks() + "]";
                        tmp = tmp + "}";
                        events.Add(tmp);
                    }
                    if (isLowFuel(out ftype))
                    {
                        string tmp = "{\"event\":\"lowFuel\"";
                        tmp = tmp + ",\"fueltype\":" + ftype;
                        tmp = tmp + "}";
                        events.Add(tmp);
                    }
                }
                if (isAttacked())
                {
                    statusMessage = "\"red\"";
                    string tmp = "{\"Event\":\"underAttack\"";
                    string t = "";
                    if (lockedState)
                    {
                        tmp = tmp + ",\"Locked\": \"true\"";
                    }
                    if (damagedBlockRatio > 0)
                    {
                        tmp = tmp + ",\"Damaged\": " + (damagedBlockRatio * 100).ToString();
                    }

                    if (destroyedAmount > 0)
                    {
                        tmp = tmp + ",\"Destroyed\": " + destroyedAmount.ToString();
                    }

                    t = getEnemyTargetsData();
                    if (t != "")
                    {
                        tmp = tmp + ",\"Targets\":" + t;
                        tmp = tmp + "}";
                    }
                    events.Add(tmp);
                }

                statusMessage = "{\"Name\":\"" + Me.CubeGrid.CustomName + "\",\"Additional\":\"" + additionalStatus + "\",\"Status\":" + statusMessage;

                statusMessage = statusMessage + ",\"Info\":[";
                statusMessage = statusMessage + "{\"GasAmount\":" + getGridGasAmount("Hydrogen") + "},";
                statusMessage = statusMessage + "{\"BatteryCharge\":" + getGridBatteryCharge() + "},";
                statusMessage = statusMessage + "{\"CargoUsed\":" + getGridUsedCargoSpace() + "},";
                statusMessage = statusMessage + "{\"Position\":\"" + getMyCoords() + "\"},";
                statusMessage = statusMessage + "{\"Velocity\":" + getGridVelocity() + "}";
                statusMessage = statusMessage + "]";

                if (events.Count > 0)
                {
                    statusMessage = statusMessage + ",\"Events\":[";
                    foreach (string e in events)
                    {
                        statusMessage = statusMessage + e + ",";
                    }
                    statusMessage = statusMessage.Remove(statusMessage.Length - 1) + "]";
                }
                //finish message
                statusMessage = statusMessage + "}";
                Echo(statusMessage);

                reScanObjectGroupLocal(status_displays, StatusTag);
                status_displays.ForEach(display => display.WriteText(statusMessage));
                reScanObjectGroupLocal(request_displays, RequestTag);

                //Me.CustomData = statusMessage;
                //string cmdTag = commandChannelTag + "." + Me.CubeGrid.CustomName;
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
        #region PreludeFooter
    }
}
#endregion#region Prelude