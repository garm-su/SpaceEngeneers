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
    public List<IMyTextSurface> LCDs { get; set; }
    public List<IMyShipWelder> welders { get; set; }
    public List<IMyPistonBase> pistons { get; set; }
    public IMyCubeGrid CubeGrid { get; set; }

    public bool started = false;

    private string group_name;

    private Program parent;
    public FactoryController(string group_name, Program program)
    {
        this.parent = program;
        this.group_name = group_name;
        reScan();
    }
    public void reScan()
    {
        var group = parent.GridTerminalSystem.GetBlockGroupWithName(this.group_name);

        var projectors = new List<IMyProjector>();
        group.GetBlocksOfType(projectors);
        if (projectors.Count == 1)
        {
            this.projector = projectors[0];
            this.CubeGrid = this.projector.CubeGrid;
        }
        else
        {
            this.projector = null;
            this.CubeGrid = null;
        }

        this.LCDs = new List<IMyTextSurface>();
        group.GetBlocksOfType(this.LCDs);

        this.welders = new List<IMyShipWelder>();
        group.GetBlocksOfType(this.welders);

        this.pistons = new List<IMyPistonBase>();
        group.GetBlocksOfType(this.pistons);
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

    public void start()
    {
        parent.logger.write("START");
        started = true;
        WelderEnabled(true);
    }

    public void stop()
    {
        parent.logger.write("STOP");
        if (!started) return;
        started = false;
        WelderEnabled(false);
        PistonEnabled(false);
        foreach (var lcd in this.LCDs)
        {
            lcd.WriteText("DONE");
        }
    }

    private void PistonEnabled(bool active)
    {
        if (pistons.Count == 0) return;

        parent.logger.write("Piston " + active + " was " + pistons[0].Enabled);
        if (active == pistons[0].Enabled) return;
        foreach (var pistons in pistons)
        {
            pistons.Enabled = active;
        }
    }

    private void WelderEnabled(bool active)
    {
        parent.logger.write("Welder " + active);
        foreach (var welder in welders)
        {
            welder.Enabled = active;
        }
    }

    public void second()
    {
        if (started && pistons.Count > 0 && pistons[0].Enabled)
        {
            PistonEnabled(false);
        }
    }

    public void check()
    {
        if (!started) return;
        if (!this.projector.IsProjecting || this.projector.TotalBlocks == 0 || this.projector.RemainingBlocks == 0)
        {
            stop();
            return;
        }

        stopEngines();

        var allConstucted = true;
        foreach (var block in this.parent.slimBlocks(CubeGrid))
        {
            if (!block.IsFullIntegrity)
            {
                allConstucted = false;
                break;
            }
        }
        PistonEnabled(allConstucted);

        String result = "Progression: \n";
        result += String.Format("Buildable: {0}\n", this.projector.BuildableBlocksCount);
        int remaining = this.projector.RemainingBlocks - this.projector.RemainingArmorBlocks;

        result += String.Format("Remaining armor: {0}%\n", 100 * this.projector.RemainingArmorBlocks / this.projector.TotalBlocks);
        result += String.Format("Remaining actions: {0}%\n", 100 * remaining / this.projector.TotalBlocks);

        result += String.Format("\nProgression: {0}\n", 100 * (1 - this.projector.RemainingBlocks / this.projector.TotalBlocks));

        result += "\n" + this.projector.DetailedInfo;

        foreach (var lcd in this.LCDs)
        {
            lcd.WriteText(result);
        }
    }
}