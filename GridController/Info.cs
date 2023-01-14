
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

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusInfo //@remove
{ //@remove
    public class Program : GridStatusConfig.Program //@remove
    { //@remove

        public List<IMyTerminalBlock> allTBlocks = new List<IMyTerminalBlock>();

        public string additionalStatus = "";

        //grid info
        public double gridCharge = 0;
        public double gridGas = 0;
        public double gridLoad = 0;
        public double damagedBlockRatio = 0;
        public double destroyedAmount = 0;
        public List<string> gridDamagedBlocks = new List<string>();
        public List<string> gridDestroyedBlocks = new List<string>();
        public Dictionary<string, int> gridInventory = new Dictionary<string, int>();
        public List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();

        public bool gridStateSaved = false;
        public bool lockedState = false;
		
        public void setAdditionalStatus(String s)
        {
            additionalStatus = s;

            var surface = Me.GetSurface(0);
            surface.Alignment = TextAlignment.CENTER;
            surface.FontColor = mainColor;
            surface.FontSize = 4;
            surface.WriteText(s);
        }

        public double getGridVelocity()
        {
            double result = 0;
            List<IMyShipController> controls = new List<IMyShipController>();
            reScanObjectsLocal(controls);
            if (controls.Count() > 0)
            {
                result = (double)controls[0].GetShipVelocities().LinearVelocity.Length();
            }
            return result;
        }

        public List<string> getDestroyedBlocks()
        {
            List<string> result = new List<string>();
            if (checkDestroyedBlocks)
            {
                List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
                reScanObjectsLocal(currentState);
                destroyedAmount = allTBlocks.Count() - currentState.Count();
                //if block in allBlocks and not in current state add Block.CustomName to result;
            }
            return result;
        }

        public void updateGridInfo()
        {
			saveGridState();
            gridCharge = getGridBatteryCharge();
            gridGas = getGridGasAmount("Hydrogen");
            gridLoad = getGridUsedCargoSpace();
            gridInventory = getGridInventory();
            targets = getTurretsTargets();
            gridDamagedBlocks = getDamagedBlocks();
            gridDestroyedBlocks = getDestroyedBlocks();
            damagedBlockRatio = gridDamagedBlocks.Count() / allTBlocks.Count();
            destroyedAmount = gridDestroyedBlocks.Count();
        }



        //===========================================================================================

        //------------------------------------ arg commands ------------------------------------------
        public void saveGridState(bool update = false)
        {
            if (gridStateSaved && !update) return;
            reScanObjectsLocal(allTBlocks);
            //todo save armor block state
            gridStateSaved = true;
        }

    }  //@remove
}  //@remove