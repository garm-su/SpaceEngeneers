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
        GyroAligner _aligner;

        string dmg = "DAMAGED: ";
        string constuct = "CNSTRCT: ";
        string hud = " :HUD";


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

        public string getStatus(out string statusMessage)
        {
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

            return Status.ToString(false);
        }
        public void selfState()
        {
            StringBuilder sb = new StringBuilder("----");
            iPos += iStep;
            if (iPos == 3)
            {
                iStep = -1;
            }
            if (iPos == 0)
            {
                iStep = 1;
            }
            string result = "Grid Controller v" + Ver;
            sb[iPos] = '|';
            result += "\nworking " + sb.ToString() + "\n";
            var surface = Me.GetSurface(0);
            surface.TextPadding = 30f;
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.Font = "DEBUG";
            surface.FontColor = new Color(0, 200, 0);
            surface.FontSize = 1.2f;
            surface.BackgroundColor = new Color(0, 5, 0);
            surface.WriteText(result);
        }

        public void sendStatus()
        {
            selfState();
            string statusMessage;
            IGC.SendBroadcastMessage(statusChannelTag, getStatus(out statusMessage));
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
                            logger.write("There's somethign wrong with your json: " + e.Message + " > " + (string)newRequest.Data);
                            continue;
                        }
                        if (jsonData.ContainsKey("Target") && ((JsonPrimitive)jsonData["Target"]).GetValue<String>() != Me.CubeGrid.CustomName) continue;
                        //todo move to sparate files
                        var name = jsonData.ContainsKey("Name") ? ((JsonPrimitive)jsonData["Name"]).GetValue<String>() : "";
                        switch (((JsonPrimitive)jsonData["Action"]).GetValue<String>())
                        {
                            case "BaseStatus":
                                BaseStatus[((JsonPrimitive)jsonData["Type"]).GetValue<String>()] = ((JsonPrimitive)jsonData["Value"]).GetValue<String>();
                                break;
                            case "Coords":
                                if (allyPositions.ContainsKey(name))
                                {
                                    allyPositions[name].update(jsonData);
                                }
                                else
                                {
                                    allyPositions[name] = new gridPosition(jsonData);
                                }
                                break;
                            case "TB":
                                Echo("TB");
                                var tbs = new List<IMyTimerBlock>();
                                reScanObjectExactLocal(tbs, name);
                                tbs.ForEach(tb => tb.Trigger());
                                break;
                        }
                    }
                }
            }
            sendCoordsCmd(statusMessage);
        }

        public void clearBlockName(IMyTerminalBlock blck, string prefix)
        {
            if (prefix != "" && blck.CustomName.StartsWith(prefix)) return;
            var found = false;

            if (prefix != dmg && blck.CustomName.StartsWith(dmg))
            {
                blck.CustomName = blck.CustomName.Substring(dmg.Length);
                found = true;
            }
            if (prefix != constuct && blck.CustomName.StartsWith(constuct))
            {
                blck.CustomName = blck.CustomName.Substring(dmg.Length);
                found = true;
            }
            if (prefix == "")
            {
                if (blck.CustomName.EndsWith(hud))
                {
                    blck.CustomName = blck.CustomName.Substring(0, blck.CustomName.Length - hud.Length);
                }
                else if (found)
                {
                    blck.ShowOnHUD = false;
                }
            }
            else
            {
                var was = blck.ShowOnHUD && !found;
                blck.CustomName = prefix + blck.CustomName + (was ? hud : "");
                blck.ShowOnHUD = true;
            }

        }

        public void hudDmg()
        {
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks);
            foreach (var block in blocks)
            {
                IMySlimBlock slimblock = block.CubeGrid.GetCubeBlock(block.Position);
                clearBlockName(block, slimblock.CurrentDamage > 0 ? dmg : slimblock.BuildIntegrity < slimblock.MaxIntegrity ? constuct : "");
            }
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
            _scheduler.AddScheduledAction(() => drawMap(mapRange), 6);
            _scheduler.AddScheduledAction(sendStatus, 1);
            _scheduler.AddScheduledAction(logger.printToSurfaces, 1);

            _scheduler.AddScheduledAction(hudDmg, 0.2);

            _aligner = new GyroAligner(this, controller);

            _scheduler.AddScheduledAction(_aligner.Aligment, 50);
            _scheduler.AddScheduledAction(_aligner.gyroJoin, 0.1);

            Echo("Started");
        }

        public void remoteTimerBlock(string gridName, string tbName)
        {
            var command = new JsonObject("");
            command.Add(new JsonPrimitive("Action", "TB"));
            command.Add(new JsonPrimitive("Target", gridName));
            command.Add(new JsonPrimitive("Name", tbName));
            IGC.SendBroadcastMessage(commandChannelTag, command.ToString());
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
                case "alignGravity":
                    _aligner.direction = null;
                    _aligner.Override(true);
                    break;
                case "alignStop":
                    _aligner.Override(false);
                    break;
                case "remoteTimerBlock":
                    remoteTimerBlock(props[1], props[2]);
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