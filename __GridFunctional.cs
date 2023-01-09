
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
using VRage.Game.ModAPI.Ingame.Utilities;
using LitJson;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GF
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

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


        #region PreludeFooter
    }
}
#endregion