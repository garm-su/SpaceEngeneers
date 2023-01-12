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
using SpaceEngineers.UWBlockPrograms.BaseController;

public class FactoryController
{
    public IMyProjector projector { get; set; }
    public IMyTextSurface LCD { get; set; }
    public IMyCubeGrid CubeGrid { get; set; }
    private string LCD_name;
    private string projector_name;

    private Program parent;
    public FactoryController(string projector, string LCD)
    {
        this.LCD_name = LCD;
        this.projector_name = projector;
    }
    public void connect(Program program)
    {
        this.parent = program;
        this.projector = parent.GridTerminalSystem.GetBlockWithName(this.projector_name) as IMyProjector;
        this.LCD = parent.GridTerminalSystem.GetBlockWithName(this.LCD_name) as IMyTextSurface;
        this.CubeGrid = this.projector.CubeGrid;
    }

    public void stopEngines()
    {
        var thrusters = new List<IMyThrust>();
        this.parent.GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters,
            item => item.CubeGrid == this.CubeGrid
        );
        foreach (var thruster in thrusters)
        {
            thruster.Enabled = false;
        }
    }

    public void check()
    {
        if (!this.projector.IsProjecting || this.projector.TotalBlocks == 0) return;

        stopEngines();

        String result = "Progression: \n";
        result += String.Format("Buildable: {0}\n", this.projector.BuildableBlocksCount);
        int remaining = this.projector.RemainingBlocks - this.projector.RemainingArmorBlocks;

        result += String.Format("Remaining armor: {0}%\n", 100 * this.projector.RemainingArmorBlocks / this.projector.TotalBlocks);
        result += String.Format("Remaining actions: {0}%\n", 100 * remaining / this.projector.TotalBlocks);

        result += String.Format("\nProgression: {0}\n", 100 * (1 - this.projector.RemainingBlocks / this.projector.TotalBlocks));

        // Dictionary<MyDefinitionBase, int> RemainingBlocksPerType { get; }
        result += "\n" + this.projector.DetailedInfo;

        this.LCD.WriteText(result);
    }
}