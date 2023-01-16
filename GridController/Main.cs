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

        ArgParser args;

        //item order
        Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
        Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

        List<IMyRadioAntenna> antenna = new List<IMyRadioAntenna>();

        IMyBroadcastListener commandListener;
        IMyBroadcastListener statusListener;

        //--------------------------------- rescan and config functions --------------------------------
        public class ArgParser
        {
            private Dictionary<string, string> modes;
            private List<string> actions;
            private Dictionary<string, int> values;
            public int Count
            {
                get; set;
            }
            public ArgParser(string arg)
            {
                List<string> argList = arg.Split(',').Select(a => a.Trim()).ToList();
                foreach (var elem in argList)
                {
                    List<string> currentElem = elem.Split('=').Select(a => a.Trim()).ToList();
                    if (currentElem.Count() > 1)
                    {
                        int val;
                        if (Int32.TryParse(currentElem[1], out val))
                        {
                            values.Add(currentElem[0], val);
                        }
                        else
                        {
                            modes.Add(currentElem[0], currentElem[1]);
                        }
                    }
                    else
                    {
                        actions.Add(currentElem[0]);
                    }
                }
            }
            public bool isInModes(string argName)
            {
                return modes.ContainsKey(argName);
            }
            public bool isInActions(string argName)
            {
                return actions.Contains(argName);
            }
            public string getModeValue(string argName)
            {
                if (modes.ContainsKey(argName))
                {
                    return modes[argName];
                }
                else
                {
                    return "";
                }
            }
        }

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

            //finish message
            statusMessage = Status.ToString(false);
            Echo(statusMessage);
            return statusMessage;

        }


        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            loadConfig();
            logger = new Log(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string arg)
        {
//logging and service actions
            logger.write("Main " + arg);
            if (arg.StartsWith(LogTag)) return;

//parse args and execute commands
            //args = new ArgParser(arg);

            if (arg != "")
            {
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
                    default:
                        break;
                }
            }


//rescan and init - remove section after refactor
            List<IMyTextPanel> request_displays = new List<IMyTextPanel>();
            reScanObjectGroupLocal(request_displays, RequestTag);
            targets.Clear();

//runtime actions
            updateGridInfo();
            checkMaxSpeed();
            lcdDraw(InfoTag, StatusTag);

//send
            //statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
            IGC.SendBroadcastMessage(statusChannelTag, getStatus(), TransmissionDistance.CurrentConstruct);

            commandListener = IGC.RegisterBroadcastListener(commandChannelTag);
            while (commandListener.HasPendingMessage)
            {
                string message;
                MyIGCMessage newRequest = commandListener.AcceptMessage();
                if (commandListener.Tag == commandChannelTag)
                {
                    if (newRequest.Data is string)
                    {
                        message = newRequest.Data.ToString();
                        foreach (IMyTextSurface d in request_displays)
                        {
                            d.WriteText(message);
                        }
                    }
                }
            }


        }

    }  //@remove
}  //@remove