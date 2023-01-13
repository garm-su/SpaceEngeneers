
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
namespace SpaceEngineers.UWBlockPrograms.GridStatusRadar //@remove
{ //@remove
    public class Program : GridStatusLog.Program //@remove
    { //@remove

        public JsonList getEnemyTargetsData()
        {
            JsonList result = new JsonList("Targets");
            foreach (MyDetectedEntityInfo target in targets)
            {
                var t = new JsonObject("");
                t.Add(new JsonPrimitive("Name", target.Name));
                t.Add(new JsonPrimitive("Type", target.Type.ToString()));
                t.Add(new JsonPrimitive("Position", target.Position.ToString()));
                result.Add(t);
            }
            return result;
        }



    }  //@remove
}  //@remove