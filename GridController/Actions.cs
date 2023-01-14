
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
using VRage.Game.ModAPI.Ingame.Utilities;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusActions //@remove
{ //@remove
    public class Program : GridStatusRadar.Program //@remove
    { //@remove

        int maxSpeed = 0;

        public void cargoUnLoad()
        {
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory);
            var outerCargo = new List<IMyCargoContainer>();
            reScanObjects(outerCargo, b => b.CubeGrid != Me.CubeGrid);
            logger.write("cargoUnLoad " + blocks.Count + " " + outerCargo.Count);
            foreach (var block in blocks)
            {
                var source = block.GetInventory();
                var items = new List<MyInventoryItem>();
                source.GetItems(items);
                for (int k = 0; k < items.Count; k++)
                {
                    var item = items[k];
                    foreach (var outer in outerCargo)
                    {
                        var destination = outer.GetInventory();
                        if (!source.IsConnectedTo(destination) || destination.IsFull) continue;
                        source.TransferItemTo(destination, k, null, true);
                    }
                }
            }

        }

        public void cargoLoad(string group, String after)
        {
            group = group == "" ? "Cargo" : "Cargo-" + group;
            logger.write("cargoLoad: " + group);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory && MyIni.HasSection(b.CustomData, group));
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
                reReadConfig(needResources, block.CustomData, group);
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
            group = group == "" ? "Cargo" : "Cargo-" + group;
            logger.write("cargoSave: " + group);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks, b => b.HasInventory);

            foreach (var block in blocks)
            {
                var ini = new MyIni();
                if (!ini.TryParse(block.CustomData))
                {
                    ini.Clear();
                }
                else
                {
                    ini.DeleteSection(group);
                }

                ini.AddSection(group);

                var items = new List<MyInventoryItem>();
                for (int j = 0; j < block.InventoryCount; j++)
                {
                    block.GetInventory(j).GetItems(items);
                    foreach (var item in items)
                    {
                        ini.Set(group, getName(item.Type), item.Amount.ToString());
                    }
                }
                block.CustomData = ini.ToString();
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

        public void setMaxSpeed(string v)
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


    }  //@remove
}  //@remove