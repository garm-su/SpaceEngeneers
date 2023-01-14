
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
namespace SpaceEngineers.UWBlockPrograms.GridStatusConfig //@remove
{ //@remove
    public class Program : LogLibrary.Program //@remove
    { //@remove
        public Color mainColor = new Color(0, 255, 0);
        public const string SKIP = "[SKIP]";
        public const string StatusTag = "[STATUS]";
        public const string RequestTag = "[REQUEST]";
        public const string InfoTag = "[INFO]";
        public const string AimTag = "[AIM]";

        public string ConfSection = "GARM";
        public string GridTag = "[GRID]"; //зачем? есть тэг [INFO]
        public new string LogTag = "[LOG]";
        public const double BATTERY_MAX_LOAD = 0.95;

        public string statusChannelTag = "RDOStatusChannel";
        public string commandChannelTag = "RDOCommandChannel";


        public bool checkDestroyedBlocks = true;

        public new int LogMaxCount = 100;


        //alert tresholds
        public double energyTreshold = 0.25; //% of max capacity, default - 25%
        public double gasTreshold = 0.25; //% of max capacity, default - 25%
        public double uraniumTreshold = 0; //kg
        public double damageTreshold = 0.2; //% of terminal blocks, default - 20%

        public Log logger;

    }  //@remove
}  //@remove