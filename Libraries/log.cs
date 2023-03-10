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
using SpaceEngineers.UWBlockPrograms.Grid;

namespace SpaceEngineers.UWBlockPrograms.LogLibrary //@remove
{ //@remove
    public class Program : Grid.Program //@remove
    { //@remove


        public string LogTag = "[LOG]"; //move to script //@remove
        public int LogMaxCount = 100; //move to script //@remove

        public class LogEntry
        {

            public string log;
            public int count;
            public string time;

            public LogEntry(string info)
            {
                log = info;
                count = 1;
                time = DateTime.Now.ToString("HH:mm:ss");
            }

            public void inc()
            {
                count++;
                time = DateTime.Now.ToString("HH:mm:ss");
            }
            public override string ToString() => $"{time}{(count > 1 ? " " + count.ToString() : "")}: {log}";
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

            public void printToSurfaces()
            {
                var result = String.Join("\n", logs);
                surfaces.ForEach((surface) => surface.WriteText(result));
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
            }
        }

    } //@remove
} //@remove