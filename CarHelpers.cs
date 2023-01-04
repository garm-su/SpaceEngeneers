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
namespace SpaceEngineers.UWBlockPrograms.Car
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

//cargoLoad,,Таскало 01 TB handleCharged
//cargoSave,
//Battery,Таскало 01 TB handleCharged
//batteryCharge,Recharge
//batteryCharge,Auto
//MyObjectBuilder_Ore.Ice: 3000
//MyObjectBuilder_Ore.Ice: 42000
//MyObjectBuilder_Ore.Gold: 42000
//MyObjectBuilder_Ore.Gold: 3000

        const String SKIP = "[SKIP]";
        const double BATTERY_MAX_LOAD = 0.95;
        Color mainColor = new Color(0, 255, 0);

        public Program()
        {
        }

        public void print(String s)
        {
            var surface = Me.GetSurface(0);
            surface.Alignment = TextAlignment.CENTER;
            surface.FontColor = mainColor;
            surface.FontSize = 4;
            surface.WriteText(s);
        }

        public void run(String name)
        {
            Echo("Run after: " + name);
            var tb = GridTerminalSystem.GetBlockWithName(name) as IMyTimerBlock;
            try
            {
                tb.Trigger();
            }
            catch { }
        }

        public void batteryLoad(String after)
        {
            Echo("batteryLoad");
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, b => b.CubeGrid == Me.CubeGrid);
            float curLoad;
            if (batteries.Count > 0)
            {
                curLoad = 0;
                foreach (var bat in batteries)
                {
                    curLoad += bat.CurrentStoredPower / bat.MaxStoredPower;
                }
                curLoad /= batteries.Count;

                print("B." + (int)(100 * curLoad) + "%");
                if (curLoad > BATTERY_MAX_LOAD)
                {
                    run(after);
                }
            }
        }

        public void batteryCharge(string type)
        {
            Echo("batteryCharge: " + type);
            ChargeMode mode;
            switch (type)
            {
                case "Recharge":
                    mode = ChargeMode.Recharge;
                    break;
                case "Discharge":
                    mode = ChargeMode.Discharge;
                    break;
                //.............
                case "Auto":
                    mode = ChargeMode.Auto;
                    break;
                default:
                    return;
            }
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, b => b.CubeGrid == Me.CubeGrid);
            foreach (var bat in batteries)
            {
                bat.ChargeMode = mode;
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

        private string getName(MyItemType type)
        {
            return type.TypeId + '.' + type.SubtypeId;
        }

        public void cargoLoad(string group, String after)
        {
            Echo("cargoLoad: " + group);
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, b => b.CubeGrid == Me.CubeGrid && b.HasInventory);
            var outerCargo = new List<IMyCargoContainer>();
            var needResources = new Dictionary<string, int>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(outerCargo, b => b.CubeGrid != Me.CubeGrid && b.HasInventory);

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
                run(after);
            }
            else
            {
                print("C." + (int)(need == 0 ? 100 : 100 * found / need) + "%");
            }
        }

        private bool moveResources(List<IMyCargoContainer> outerCargo, IMyTerminalBlock block, Dictionary<string, int> dictionary)
        {
            if (!block.HasInventory || dictionary.Count == 0) return true;
            IMyInventory sourse, destination = block.GetInventory();

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
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, b => b.CubeGrid == Me.CubeGrid && b.HasInventory);
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

        public void Main(string args)
        {
            args += ",,";
            var props = args.Split(',');
            // .ToList<String>()
            switch (props[0])
            {
                case "Battery":
                    batteryLoad(props[1]);
                    break;
                case "cargoSave":
                    cargoSave(props[1]);
                    break;
                //.............
                case "cargoLoad":
                    cargoLoad(props[1], props[2]);
                    break;
                case "batteryCharge":
                    batteryCharge(props[1]);
                    break;
                default:
                    break;
            }

        }
        #region PreludeFooter
    }
}
#endregion