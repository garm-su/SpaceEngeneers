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

        string link = "UNLINKED: ";
        string dmg = "DAMAGED: ";
        string constuct = "CNSTRCT: ";
        string hud = " :HUD";
        string alertLine = "";


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

            if (gridUnlinkedBlocks.Count() > 0)
            {
                statusMessage = "orange";
                var curEvent = new JsonObject("");
                JsonList unlinkedJson = new JsonList("Blocks"); ;
                statusEvents.Add(curEvent);
                curEvent.Add(new JsonPrimitive("Event", "unlinked"));
                foreach (var b in gridUnlinkedBlocks)
                {
                    unlinkedJson.Add(new JsonPrimitive("", b));
                }
                curEvent.Add(unlinkedJson);
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
            var surface = Me.GetSurface(1);
            surface.TextPadding = 30f;
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.Alignment = TextAlignment.CENTER;
            surface.Font = "DEBUG";
            surface.FontColor = new Color(0, 200, 0);
            surface.FontSize = 3f;
            surface.BackgroundColor = new Color(0, 5, 0);
            surface.WriteText(result);
            Echo(result);
            Echo(showConfig());
            Echo(echoLine);
            echoLine = "";
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
                                echoLine += "TB\n";
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

        public void autoEngines()
        {
            if (currentControl == null || orientedThrusters == null) return;
            Echo("0");
            logger.write("currentControl=" + currentControl.CustomName);
            // Echo("0");
            // logger.write("manualControl=" + manualControl.CustomName);
            Echo("1");
            var maxThrust = allDirections.ToDictionary(dir => dir, dir => orientedThrusters[dir].Where(th => th.Enabled).Sum(th => th.MaxEffectiveThrust));
            Echo("2");
            var needThrust = new Dictionary<Base6Directions.Direction, double>();
            var velosityCompensation = new Dictionary<Base6Directions.Direction, double>();

            var requirementVelocity = -currentControl.GetShipVelocities().LinearVelocity;
            Echo("3");
            var gravity = shipMass * currentControl.GetTotalGravity();
            Echo("4");
            var cntrl = manualControl ?? currentControl;
            var keys = cntrl.MoveIndicator;

            needThrust[Base6Directions.Direction.Down] = gravity.Dot(currentControl.WorldMatrix.Down);
            velosityCompensation[Base6Directions.Direction.Down] = keys.Y == 0 ? requirementVelocity.Dot(currentControl.WorldMatrix.Down) : -keys.Y;
            needThrust[Base6Directions.Direction.Left] = gravity.Dot(currentControl.WorldMatrix.Left);
            velosityCompensation[Base6Directions.Direction.Left] = keys.X == 0 ? requirementVelocity.Dot(currentControl.WorldMatrix.Left) : -keys.X;
            needThrust[Base6Directions.Direction.Forward] = gravity.Dot(currentControl.WorldMatrix.Forward);
            velosityCompensation[Base6Directions.Direction.Forward] = keys.Z == 0 ? requirementVelocity.Dot(currentControl.WorldMatrix.Forward) : -keys.Z;
            needThrust[Base6Directions.Direction.Up] = -needThrust[Base6Directions.Direction.Down];
            velosityCompensation[Base6Directions.Direction.Up] = -velosityCompensation[Base6Directions.Direction.Down];
            needThrust[Base6Directions.Direction.Right] = -needThrust[Base6Directions.Direction.Left];
            velosityCompensation[Base6Directions.Direction.Right] = -velosityCompensation[Base6Directions.Direction.Left];
            needThrust[Base6Directions.Direction.Backward] = -needThrust[Base6Directions.Direction.Forward];
            velosityCompensation[Base6Directions.Direction.Backward] = -velosityCompensation[Base6Directions.Direction.Forward];

            logger.write("needThrust+" + String.Join(", ", needThrust.Select(obj => obj.Key + ":" + obj.Value)));
            logger.write("maxThrust+" + String.Join(", ", maxThrust.Select(obj => obj.Key + ":" + obj.Value)));

            try
            {
                logger.write("DownTh: " + orientedThrusters[Base6Directions.Direction.Down][0].CustomName);
                logger.write("LeftTh: " + orientedThrusters[Base6Directions.Direction.Left][0].CustomName);
                logger.write("ForwTh: " + orientedThrusters[Base6Directions.Direction.Forward][0].CustomName);
            }
            catch
            {
                logger.write("EX");
            }


            foreach (var dir in allDirections)
            {
                if (!needThrust.ContainsKey(dir)) Echo("NO needThrust" + dir);
                if (!maxThrust.ContainsKey(dir)) Echo("NO maxThrust" + dir);
                if (!orientedThrusters.ContainsKey(dir)) Echo("NO orientedThrusters" + dir);

                if (maxThrust[dir] == 0) continue;

                var thrust = Math.Max(0, Math.Min(1, (needThrust[dir] <= 0 ? 0 : needThrust[dir] / maxThrust[dir]) - velosityCompensation[dir]));
                logger.write("" + dir + ": " + thrust);
                foreach (var th in orientedThrusters[dir].Where(th => th.Enabled))
                {
                    th.ThrustOverridePercentage = (float)thrust;
                }
            }

            // shipMass * gravityFactor 
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
            if (prefix != link && blck.CustomName.StartsWith(link))
            {
                blck.CustomName = blck.CustomName.Substring(link.Length);
                found = true;
            }
            if (prefix != constuct && blck.CustomName.StartsWith(constuct))
            {
                blck.CustomName = blck.CustomName.Substring(constuct.Length);
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

        public void hudDmg(bool flag)
        {
            if (!flag)
            {
                return;
            }
            IMyInventory source = null;
            var baseBlock = new List<IMyCargoContainer>();
            reScanObjectExactLocal(baseBlock, cargoAlignment);
            if (baseBlock.Count > 0)
            {
                source = baseBlock[0].GetInventory();
            }
            var scanBlocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(scanBlocks, blck => blck.HasInventory);
            var blocks = new List<IMyTerminalBlock>();
            reScanObjectsLocal(blocks);
            foreach (var block in blocks)
            {
                IMySlimBlock slimblock = block.CubeGrid.GetCubeBlock(block.Position);

                clearBlockName(block,
                    slimblock.CurrentDamage > 0 ? dmg :
                    source != null && block.HasInventory && unlinked.ContainsKey(block.GetId()) && unlinked[block.GetId()].check ? link :
                    slimblock.BuildIntegrity < slimblock.MaxIntegrity ? constuct :
                "");
            }
        }

        public Program()
        {
            loadConfig();
            logger = new Log(this);
            logger.write("Inited");
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _scheduler = new Scheduler(this);

            _scheduler.AddScheduledAction(updateGridInfo, 1, false, 9);
            if (autoBot)
            {
                _scheduler.AddScheduledAction(autoEngines, 10);
            }
            _scheduler.AddScheduledAction(checkMaxSpeed, 10);
            _scheduler.AddScheduledAction(setupCameras, 1);
            _scheduler.AddScheduledAction(updateTargets, 10);
            _scheduler.AddScheduledAction(lcdDraw, 0.5);
            _scheduler.AddScheduledAction(() => drawMap(mapRange), 6);
            _scheduler.AddScheduledAction(sendStatus, 1);
            _scheduler.AddScheduledAction(logger.printToSurfaces, 1);

            _scheduler.AddScheduledAction(() => hudDmg(showDmg), 0.2);

            _aligner = new GyroAligner(this);

            _scheduler.AddScheduledAction(_aligner.Aligment, 50);
            _scheduler.AddScheduledAction(_aligner.gyroJoin, 0.1);

            echoLine += "Started\n";
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
                    echoLine += "Grid is locked\n";
                    break;
                case "unlocked":
                    lockedState = false;
                    echoLine += "Manual override lock status\n";
                    break;
                case "fix":
                    saveGridState(update: true);
                    echoLine += "Grid state saved\n";
                    break;
                case "showDmg":
                    showDmg = (bool)(props[1] == "on");
                    echoLine += "Show DMG on HUD set to:" + showDmg.ToString() + "\n";
                    break;
                case "alignGravityToggle":
                    _aligner.direction = null;
                    _aligner.Toggle(props[1]);
                    break;
                case "alignStop":
                    _aligner.Override(false);
                    break;
                case "remoteTimerBlock":
                    remoteTimerBlock(props[1], props[2]);
                    break;
                case "autolock":
                    autoLock = !autoLock;
                    isSearching = autoLock;
                    break;
                case "autoaim":
                    autoAim = !autoAim;
                    break;
                case "lock":
                    isSearching = true;
                    echoLine += "Locking target...\n";
                    break;
                case "release":
                    lockedTarget = new MyDetectedEntityInfo();
                    echoLine += "Target released\n";
                    break;
                case "detectAll":
                    detectAll = !detectAll;
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