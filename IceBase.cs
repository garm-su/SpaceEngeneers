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

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.BatteryMonitor
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

        string
            CONNECT = "[BOT CONNECT]",
            LCD = "[BOT LCD]";

        Vector3D connectorToParking = new Vector3D(1, 2, 3);

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string args)
        {
            var connector = GridTerminalSystem.GetBlockWithName(CONNECT);
            var connectorPosintion = connector.CubeGrid.GridIntegerToWorld(connector.Position);
            var addOneShipCoord = new Vector3I(connector.Position.X, connector.Position.Y + 1, connector.Position.Z);
            var addOneVector = connector.CubeGrid.GridIntegerToWorld(addOneShipCoord) - connectorPosintion;
            var lcd = GridTerminalSystem.GetBlockWithName(LCD) as IMyTextSurface;

            var positionBase0 = connectorPosintion + 2.3 * addOneVector;
            var positionBase10 = connectorPosintion + 10 * addOneVector;
            var positionBase20 = connectorPosintion + 20 * addOneVector;

            lcd.WriteText(
                string.Format("GPS:{0}:{1}:{2}:{3}:", "positionBase0", positionBase0.X, positionBase0.Y, positionBase0.Z) + "\n" +
                string.Format("GPS:{0}:{1}:{2}:{3}:", "positionBase10", positionBase10.X, positionBase10.Y, positionBase10.Z) + "\n" +
                string.Format("GPS:{0}:{1}:{2}:{3}:", "positionBase20", positionBase20.X, positionBase20.Y, positionBase20.Z) + "\n" 
                );

            Echo("Start");
        }

        #region PreludeFooter
    }
}
#endregion