
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
using Program = SpaceEngineers.UWBlockPrograms.GridStatusInfo.Program;

public class Gyroscope
{
    public IMyGyro gyro;
    private int[] conversionVector = new int[3];

    public Gyroscope(IMyGyro gyroscope, IMyTerminalBlock reference)
    {
        gyro = gyroscope;

        for (int i = 0; i < 3; i++)
        {
            Vector3D vectorShip = GetAxis(i, reference);

            for (int j = 0; j < 3; j++)
            {
                double dot = vectorShip.Dot(GetAxis(j, gyro));

                if (dot > 0.9)
                {
                    conversionVector[j] = i;
                    break;
                }
                if (dot < -0.9)
                {
                    conversionVector[j] = i + 3;
                    break;
                }
            }
        }
    }

    public void SetRotation(float[] rotationVector)
    {
        gyro.Pitch = rotationVector[conversionVector[0]];
        gyro.Yaw = rotationVector[conversionVector[1]];
        gyro.Roll = rotationVector[conversionVector[2]];
    }

    private Vector3D GetAxis(int dimension, IMyTerminalBlock block)
    {
        switch (dimension)
        {
            case 0:
                return block.WorldMatrix.Right;
            case 1:
                return block.WorldMatrix.Up;
            default:
                return block.WorldMatrix.Backward;
        }
    }

    internal void Override(bool is_script_controlling)
    {
        gyro.GyroOverride = is_script_controlling;

        gyro.Roll = 0;
        gyro.Yaw = 0;
        gyro.Pitch = 0;
    }
}

public class GyroAligner
{
    private Program parent;

    internal float PitchSensitivity { get; set; } = 1;
    internal float RollSensitivity { get; set; } = 1;
    internal float YawSensitivity { get; set; } = 1;

    public Vector3D? direction { get; set; } = null;
    public string orientation = "top";

    internal List<Gyroscope> gyros = new List<Gyroscope>();
    internal HashSet<long> gyroIds = new HashSet<long>();

    internal bool active = false;

    public override string ToString()
    {
        return "Active: " + (active ? orientation : "none");
    }

    //add gyros by Rescan
    public void gyroJoin()
    {
        if (parent.currentControl == null) return;

        var Gyros = new List<IMyGyro>();
        parent.reScanObjectsLocal(Gyros);

        foreach (var g in Gyros)
        {
            var idx = g.GetId();
            if (gyroIds.Contains(idx)) continue;
            gyroIds.Add(idx);
            gyros.Add(new Gyroscope(g, parent.currentControl));
        }
    }

    public GyroAligner(Program parent)
    {
        this.parent = parent;

        gyroJoin();
    }

    internal void Override(bool new_active)
    {
        active = new_active;
        foreach (var gyro in gyros) gyro.Override(active);
    }
    internal void Toggle(string new_orientation)
    {
        if (new_orientation != "" && orientation != new_orientation)
        {
            orientation = new_orientation;
            Override(true);
            return;
        }
        Override(!active);
    }



    internal void Aligment()
    {
        var cntrl = parent.currentControl;
        if (!active || cntrl == null) return;
        var mnl = parent.manualControl ?? cntrl;

        Vector3D orient = Vector3D.Normalize(direction == null ?
            cntrl.GetNaturalGravity() :
            (Vector3D)direction - cntrl.CubeGrid.GridIntegerToWorld(cntrl.Position)
        );

        float pitch = 0, roll = 0, yaw = 0;

        switch (orientation)
        {
            case "bottom":
                pitch = (float)orient.Dot(cntrl.WorldMatrix.Backward);
                roll = (float)orient.Dot(cntrl.WorldMatrix.Left);
                break;
            case "top":
                pitch = (float)orient.Dot(cntrl.WorldMatrix.Forward);
                roll = (float)orient.Dot(cntrl.WorldMatrix.Right);
                break;
            case "forward":
                pitch = (float)orient.Dot(cntrl.WorldMatrix.Down);
                yaw = (float)orient.Dot(cntrl.WorldMatrix.Right);
                break;
            case "backward":
                pitch = (float)orient.Dot(cntrl.WorldMatrix.Up);
                yaw = (float)orient.Dot(cntrl.WorldMatrix.Left);
                break;
            case "left":
                roll = (float)orient.Dot(cntrl.WorldMatrix.Up);
                yaw = (float)orient.Dot(cntrl.WorldMatrix.Forward);
                break;
            case "right":
                roll = (float)orient.Dot(cntrl.WorldMatrix.Down);
                yaw = (float)orient.Dot(cntrl.WorldMatrix.Backward);
                break;
            default:
                return;
        }

        pitch += mnl.RotationIndicator.X;
        roll += mnl.RollIndicator;
        yaw += mnl.RotationIndicator.Y;

        float[] controls = new float[] { pitch, yaw, roll, -pitch, -yaw, -roll };
        foreach (Gyroscope g in gyros)
            g.SetRotation(controls);
    }
}