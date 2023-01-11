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

namespace SpaceEngineers.UWBlockPrograms.JSONtest   //@remove
{  //@remove
    public class Program : LogLibrary.Program  //@remove
    { //@remove
        // Your code goes between the next #endregion and #region 
        public new string LogTag = "[LOG JSON]";
        public new int LogMaxCount = 100;

        public Program()
        {
            Echo("Executed");
        }

        public bool retf()
        {
            Echo("stop?");
            return false;
        }

        public void Main(string argument)
        {
            Echo("Hello World!");
            if (argument == "xxx")
            {

                var testStr = "{\"Name\":\"xxx\", \"Val\":123.3}";

                try
                {
                    var jsonData = (new JSON(testStr)).Parse();
                    Echo(jsonData.ToString());
                }
                catch (Exception e) // in case something went wrong (either your json is wrong or my library has a bug :P)
                {
                    Echo("There's somethign wrong with your json: " + e.Message);
                    return;
                }
            }
        }

        

        
    } //@remove
} //@remove