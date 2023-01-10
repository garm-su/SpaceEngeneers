
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
namespace SpaceEngineers.UWBlockPrograms.LL
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

        string LogTag = "[LOG]";
        int LogMaxCount = 100;

        public void reScanObjectGroupLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName.Contains(name));
        }

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
                parent.Echo(info);
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


        #region PreludeFooter
    }
}
#endregion