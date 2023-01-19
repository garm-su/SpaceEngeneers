
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
using VRage.Game.ModAPI.Ingame.Utilities;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusConfig //@remove
{ //@remove
    public class Program : LogLibrary.Program //@remove
    { //@remove
        public Color mainColor = new Color(0, 255, 0);
        public string SKIP = "[SKIP]";
        public string StatusTag = "[STATUS]";
        public string RequestTag = "[REQUEST]";
        public string InfoTag = "[INFO]";
        public string AimTag = "[AIM]";

        public string ConfSection = "GARM";
        public string GridTag = "[GRID]"; //зачем? есть тэг [INFO]
        public new string LogTag = "[LOG]";
        public double BATTERY_MAX_LOAD = 0.95;

        public string statusChannelTag = "RDOStatusChannel";
        public string commandChannelTag = "RDOCommandChannel";


        public bool checkDestroyedBlocks = true;

        public new int LogMaxCount = 100;


        //alert tresholds
        public double energyTreshold = 0.25; //% of max capacity, default - 25%
        public double gasTreshold = 0.25; //% of max capacity, default - 25%
        public double uraniumTreshold = 0; //kg
        public double damageTreshold = 0.2; //% of terminal blocks, default - 20%

        public double decelerationDistance = 200;  //distanse of start deceleration with autopilot
        public double minPercentSpeed = 0.3;//min percent of max speed with autopilot


        public Log logger;

        public bool hasConfSection(IMyTerminalBlock obj) => MyIni.HasSection(obj.CustomData, ConfSection);

        public void loadConfig()
        {
            if (!hasConfSection(Me)) return;
            var ini = new MyIni();
            if (!ini.TryParse(Me.CustomData)) return;
            InitValue(ini.Get(ConfSection, "SKIP"), ref SKIP);
            InitValue(ini.Get(ConfSection, "StatusTag"), ref StatusTag);
            InitValue(ini.Get(ConfSection, "RequestTag"), ref RequestTag);
            InitValue(ini.Get(ConfSection, "InfoTag"), ref InfoTag);
            InitValue(ini.Get(ConfSection, "AimTag"), ref AimTag);
            InitValue(ini.Get(ConfSection, "LogTag"), ref LogTag);
            InitValue(ini.Get(ConfSection, "LogMaxCount"), ref LogMaxCount);

            InitValue(ini.Get(ConfSection, "BATTERY_MAX_LOAD"), ref BATTERY_MAX_LOAD);
            InitValue(ini.Get(ConfSection, "statusChannelTag"), ref statusChannelTag);
            InitValue(ini.Get(ConfSection, "commandChannelTag"), ref commandChannelTag);
            InitValue(ini.Get(ConfSection, "checkDestroyedBlocks"), ref checkDestroyedBlocks);

            InitValue(ini.Get(ConfSection, "energyTreshold"), ref energyTreshold);
            InitValue(ini.Get(ConfSection, "gasTreshold"), ref gasTreshold);
            InitValue(ini.Get(ConfSection, "uraniumTreshold"), ref uraniumTreshold);
            InitValue(ini.Get(ConfSection, "damageTreshold"), ref damageTreshold);

            InitValue(ini.Get(ConfSection, "decelerationDistance"), ref decelerationDistance);
            InitValue(ini.Get(ConfSection, "minPercentSpeed"), ref minPercentSpeed);

        }

        public void saveConfig()
        {
            var ini = new MyIni();
            if (ini.TryParse(Me.CustomData))
            {
                ini.DeleteSection(ConfSection);
            }
            ini.AddSection(ConfSection);
            ini.Set(ConfSection, "SKIP", SKIP);
            ini.Set(ConfSection, "StatusTag", StatusTag);
            ini.Set(ConfSection, "RequestTag", RequestTag);
            ini.Set(ConfSection, "InfoTag", InfoTag);
            ini.Set(ConfSection, "AimTag", AimTag);
            ini.Set(ConfSection, "LogTag", LogTag);
            ini.Set(ConfSection, "LogMaxCount", LogMaxCount);
            ini.Set(ConfSection, "BATTERY_MAX_LOAD", BATTERY_MAX_LOAD);
            ini.Set(ConfSection, "statusChannelTag", statusChannelTag);
            ini.Set(ConfSection, "commandChannelTag", commandChannelTag);
            ini.Set(ConfSection, "checkDestroyedBlocks", checkDestroyedBlocks);
            ini.Set(ConfSection, "energyTreshold", energyTreshold);
            ini.Set(ConfSection, "gasTreshold", gasTreshold);
            ini.Set(ConfSection, "uraniumTreshold", uraniumTreshold);
            ini.Set(ConfSection, "damageTreshold", damageTreshold);
            ini.Set(ConfSection, "decelerationDistance", decelerationDistance);
            ini.Set(ConfSection, "minPercentSpeed", minPercentSpeed);

            Me.CustomData = ini.ToString();
        }

        static void InitValue(MyIniValue iniValue, ref string value)
        {
            string tempValue;
            if (!iniValue.IsEmpty && iniValue.TryGetString(out tempValue))
                value = tempValue;
        }

        static void InitValue(MyIniValue iniValue, ref float value)
        {
            float tempValue;
            if (!iniValue.IsEmpty && iniValue.TryGetSingle(out tempValue))
                value = tempValue;
        }

        static void InitValue(MyIniValue iniValue, ref double value)
        {
            double tempValue;
            if (!iniValue.IsEmpty && iniValue.TryGetDouble(out tempValue))
                value = tempValue;
        }

        static void InitValue(MyIniValue iniValue, ref bool value)
        {
            bool tempValue;
            if (!iniValue.IsEmpty && iniValue.TryGetBoolean(out tempValue))
                value = tempValue;
        }

        static void InitValue(MyIniValue iniValue, ref int value)
        {
            int tempValue;
            if (!iniValue.IsEmpty && iniValue.TryGetInt32(out tempValue))
                value = tempValue;
        }

    }  //@remove
}  //@remove