
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
    public class Program : GridStatusVersion.Program //@remove
    { //@remove
        public const String Ver = "1.0";
        public List<IMyTerminalBlock> allGridTerminalBlocks = new List<IMyTerminalBlock>();
        public List<IMyThrust> gridThrusters = new List<IMyThrust>();
        public List<IMySensorBlock> gridSensors = new List<IMySensorBlock>();
        List<IMyPowerProducer> gridGasEngines = new List<IMyPowerProducer>();
        List<IMyReactor> gridReactors = new List<IMyReactor>();

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
        public bool hasGasFueledBlocks = false;
        public bool hasReactors = false;

        public Dictionary<String, String> BaseStatus = new Dictionary<String, String>();

        public int MeGridType()
        {
            return Me.CubeGrid.IsStatic ? -1 : (int)Me.CubeGrid.GridSizeEnum;
        }

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
            int counter = 0;
            if (checkDestroyedBlocks)
            {
                //List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
                foreach (var block in allGridTerminalBlocks)
                {
                    if (block.Closed)
                    {
                        counter += 1;
                        result.Add(block.CustomName);
                    }
                }
                destroyedAmount = counter;
                //todo: add repair projector with tag [STATUS] as source
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
            gridDamagedBlocks = getDamagedBlocks();
            gridDestroyedBlocks = getDestroyedBlocks();
            damagedBlockRatio = gridDamagedBlocks.Count() / allGridTerminalBlocks.Count();
            destroyedAmount = gridDestroyedBlocks.Count();
            if (destroyedAmount > 0)
            {
                reScanObjectsLocal(gridThrusters);
            }
        }

        //===========================================================================================

        //------------------------------------ arg commands ------------------------------------------
        public void saveGridState(bool update = false)
        {
            if (gridStateSaved && !update) return;
            reScanObjectsLocal(allGridTerminalBlocks);

            List<IMyThrust> gasThrusters = new List<IMyThrust>();
            reScanObjectsLocal(gridThrusters);
            reScanObjectsLocal(gridGasEngines, item => item.BlockDefinition.SubtypeId.Contains("Hydrogen"));
            reScanObjectsLocal(gasThrusters, item => item.BlockDefinition.SubtypeId.Contains("Hydrogen"));
            reScanObjectsLocal(gridReactors);
            reScanObjectsLocal(gridSensors);

            hasGasFueledBlocks = (gridGasEngines.Count() > 0) || (gasThrusters.Count() > 0);
            hasReactors = gridReactors.Count() > 0;

            //todo save armor block state
            gridStateSaved = true;
        }

    }  //@remove
}  //@remove