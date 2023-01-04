
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
using Sandbox.ModAPI.Contracts;
using Sandbox.Game;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.Engines
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

        List<IMyThrust> thrusters = new List<IMyThrust>();
        public Program()
        {
        }

        public void Main(string args)
        {
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
            for (int i = 0; i < thrusters.Count; i++)
            {
                thrusters[i].Enabled = false;
            }
        }

        #region PreludeFooter
    }
}
#endregion