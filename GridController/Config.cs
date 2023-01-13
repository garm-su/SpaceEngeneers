
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

        const string SKIP = "[SKIP]";
        const string StatusTag = "[STATUS]";
        const string RequestTag = "[REQUEST]";
        const string infoTag = "[INFO]";
        const string aimTag = "[AIM]";

        public new string LogTag = "[LOG]";
        const double BATTERY_MAX_LOAD = 0.95;

        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";


        bool checkDestroyedBlocks = true;

        public new int LogMaxCount = 100;


        //alert tresholds
        double energyTreshold = 0.25; //% of max capacity, default - 25%
        double gasTreshold = 0.25; //% of max capacity, default - 25%
        double uraniumTreshold = 0; //kg
        double damageTreshold = 0.2; //% of terminal blocks, default - 20%

    }  //@remove
}  //@remove