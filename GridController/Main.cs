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
namespace SpaceEngineers.UWBlockPrograms.GridStatus //@remove
{ //@remove
    public class Program : GridStatusActions.Program //@remove
    { //@remove
      // Your code goes between the next #endregion and #region
      //all terminalblocks
        //all armor blocks - be defined
        //item order
        Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
        Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

        int iPos = 1;
        int iStep = 1;
        Scheduler _scheduler;

        //---------------------------- grid status info ----------------------------------

        public bool isAttacked()
        {
            bool isTargets = (targets.Count() > 0);
            bool isLocked = lockedState;
            bool isLargeDamage = false;
            //isLargeDamage = (damagedBlockRatio > damageTreshold);          
            bool isDestroyedBlocks = (destroyedAmount > 0);
            return (isTargets || isLocked || isLargeDamage || isDestroyedBlocks);
        }

        public bool isLowAmmo()
        {
            bool result = false;
            //todo
            return result;
        }

        public bool isLowFuel(out string ftype)
        {
            //todo: implement Json serialize
            bool result = false;
            ftype = "";
            List<string> ftypes = new List<string>();
            if (gridCharge < energyTreshold)
            {
                result = true;
                ftypes.Add("\"energy\"");
            }

            if ((gridGas < gasTreshold) && (hasGasFueledBlocks))
            {
                result = true;
                ftypes.Add("\"gas\"");
            }
            //todo check uranium (if reactors == true)

            if (ftypes.Count > 0)
            {
                ftype = "[" + String.Join(",", ftypes) + "]";
            }
            return result;
        }

        public string getStatus()
        {
            string statusMessage = "";
            string ftype;
            List<string> events = new List<string>();
            var Status = new JsonObject("");
            statusMessage = "green";
            var statusEvents = new JsonList("Events");

            if (isLowAmmo())
            {
                //todo check what type of ammo need and how many, add to details
                statusMessage = "orange";
                var curEvent = new JsonObject("");
                statusEvents.Add(curEvent);
                curEvent.Add(new JsonPrimitive("Event", "lowAmmo"));
            }

            if (gridDamagedBlocks.Count() > 0)
            {
                statusMessage = "orange";
                var curEvent = new JsonObject("");
                JsonList damagedJson = new JsonList("Blocks"); ;
                statusEvents.Add(curEvent);
                curEvent.Add(new JsonPrimitive("Event", "damaged"));
                foreach (var b in gridDamagedBlocks)
                {
                    damagedJson.Add(new JsonPrimitive("", b));
                }
                curEvent.Add(damagedJson);
            }

            if (isLowFuel(out ftype))
            {
                statusMessage = "orange";
                var curEvent = new JsonObject("");
                statusEvents.Add(curEvent);
                curEvent.Add(new JsonPrimitive("Event", "lowFuel"));
                curEvent.Add(new JsonPrimitive("fueltype", ftype));
            }

            if (isAttacked())
            {
                statusMessage = "red";
                var curEvent = new JsonObject("");
                curEvent.Add(new JsonPrimitive("Event", "underAttack"));
                curEvent.Add(new JsonPrimitive("Locked", lockedState.ToString()));
                curEvent.Add(new JsonPrimitive("Damaged", damagedBlockRatio * 100));
                curEvent.Add(new JsonPrimitive("Destroyed", destroyedAmount));
                if (targets.Count() > 0)
                {
                    statusEvents.Add(getEnemyTargetsData());
                }
            }

            Status.Add(new JsonPrimitive("Name", Me.CubeGrid.CustomName));
            Status.Add(new JsonPrimitive("Additional", additionalStatus));
            Status.Add(new JsonPrimitive("Status", statusMessage));
            Status.Add(new JsonPrimitive("GasAmount", gridGas));
            Status.Add(new JsonPrimitive("BatteryCharge", gridCharge));
            Status.Add(new JsonPrimitive("CargoUsed", gridLoad));
            Status.Add(new JsonObject("Position", Me.GetPosition()));
            Status.Add(new JsonPrimitive("Velocity", getGridVelocity()));
            if (statusEvents.Count > 0)
            {
                Status.Add(statusEvents);
            }
            statusMessage = Status.ToString(false);
            Echo(statusMessage);
            return statusMessage;
        }
        public string selfState()
        {
            StringBuilder sb = new StringBuilder("----");
            iPos += iStep;
            if(iPos == 3)
            {
                iStep = -1;
            }
            if(iPos == 0)
            {
                iStep = 1;
            }
            string result = "Grid Controller v" + Ver;
            sb[iPos] = '|';
            result += " working " + sb.ToString() + "\n";
            return result;
        }

        public void sendStatus()
        {
            Echo(selfState());
            IGC.SendBroadcastMessage(statusChannelTag, getStatus());
            //targetsChannelTag;

            IMyBroadcastListener commandListener = IGC.RegisterBroadcastListener(commandChannelTag);
            while (commandListener.HasPendingMessage)
            {
                MyIGCMessage newRequest = commandListener.AcceptMessage();
                if (commandListener.Tag == commandChannelTag)
                {
                    if (newRequest.Data is string)
                    {
                        JsonObject jsonData;
                        try
                        {
                            jsonData = (new JSON((string)newRequest.Data)).Parse() as JsonObject;
                        }
                        catch (Exception e)
                        {
                            logger.write("There's somethign wrong with your json: " + e.Message);
                            continue;
                        }

                        //todo move to sparate files
                        switch (((JsonPrimitive)jsonData["action"]).GetValue<String>())
                        {
                            case "BaseStatus":
                                BaseStatus[((JsonPrimitive)jsonData["type"]).GetValue<String>()] = ((JsonPrimitive)jsonData["value"]).GetValue<String>();
                                break;
                        }
                    }
                }
            }

            IMyBroadcastListener gpsListener = IGC.RegisterBroadcastListener(gpsChannelTag);
            allyPositions.Clear();
            while (gpsListener.HasPendingMessage)
            {
                MyIGCMessage newPos = gpsListener.AcceptMessage();
                if (gpsListener.Tag == gpsChannelTag)
                {
                    if (newPos.Data is string)
                    {
                        JsonObject jsonData;
                        try
                        {
                            jsonData = (new JSON((string)newPos.Data)).Parse() as JsonObject;
                        }
                        catch (Exception e)
                        {
                            logger.write("There's somethign wrong with your json: " + e.Message);
                            continue;
                        }
                        allyPositions.Add((string)newPos.Data);
                    }
                }
            }

            var pos = new JsonObject("");
            pos.Add(new JsonPrimitive("Name", Me.CubeGrid.CustomName));
            pos.Add(new JsonObject("Position", Me.GetPosition()));
            //pos.Add(new JsonObject("Type", )); //get my type
            IGC.SendBroadcastMessage(gpsChannelTag, pos.ToString(false));

        }

        public Program()
        {
            loadConfig();
            logger = new Log(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _scheduler = new Scheduler(this);

            _scheduler.AddScheduledAction(updateGridInfo, 1);
            _scheduler.AddScheduledAction(checkMaxSpeed, 10);
            _scheduler.AddScheduledAction(updateTargets, 10);
            _scheduler.AddScheduledAction(lcdDraw, 0.5);
            _scheduler.AddScheduledAction(() => drawMap(mapRange), 0.5);
            _scheduler.AddScheduledAction(sendStatus, 1);
        }

        public void ProcessArguments(string arg)
        {
            logger.write("Main " + arg);
            if (arg.StartsWith(LogTag)) return;
            arg += ",,";
            var props = arg.Split(',');

            switch (props[0])
            {
                case "Battery":
                    batteryLoad(props[1]);
                    break;
                case "cargoSave":
                    cargoSave(props[1]);
                    break;
                case "cargoLoad":
                    cargoLoad(props[1], props[2]);
                    break;
                case "cargoUnLoad":
                    cargoUnLoad();
                    break;
                case "maxSpeed":
                    setMaxSpeed(props[1]);
                    break;
                case "connect":
                    connect(props[1], props[2]);
                    break;
                case "batteryCharge":
                    batteryCharge(props[1]);
                    break;
                case "saveConfig":
                    saveConfig();
                    break;
                case "loadConfig":
                    loadConfig();
                    break;
                case "locked":
                    lockedState = true;
                    Echo("Grid is locked");
                    break;
                case "unlocked":
                    lockedState = false;
                    Echo("Manual override lock status");
                    break;
                case "fix":
                    saveGridState(update: true);
                    Echo("Grid state saved");
                    break;
                case "mapScaleUp":
                    if (mapRange < 1000)
                    {
                        mapRange = mapRange * 2;
                    }
                    else
                    {
                        mapRange += 1000;
                    }
                    break;
                case "mapScaleDown":
                    if (mapRange <= 1000)
                    {
                        mapRange = mapRange / 2;
                    }
                    else
                    {
                        mapRange -= 1000;
                    }
                    break;
                default:
                    break;
            }
        }

        public void Main(string arg, UpdateType updateSource)
        {
            if (!string.IsNullOrWhiteSpace(arg))
            {
                ProcessArguments(arg);
            }

            _scheduler.Update();
        }

    }  //@remove
}  //@remove