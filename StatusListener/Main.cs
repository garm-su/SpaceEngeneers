#region Prelude  //@remove
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
using SpaceEngineers.UWBlockPrograms.Grid;
using SpaceEngineers.UWBlockPrograms.LogLibrary;
using static SpaceEngineers.UWBlockPrograms.LogLibrary.Program;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.StatusListener //@remove
{ //@remove
    public class Program : LogLibrary.Program //@remove
    { //@remove
        // Your code goes between the next #endregion and #region //@remove
        #endregion //@remove

        public new string LogTag = "[LOG STATUS]";
        string MapTag = "[MAP]";
        public new int LogMaxCount = 100;


        bool setupcomplete = false;
        List<IMyRadioAntenna> antenna;
        IMyBroadcastListener statusListener;

        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";

        Dictionary<string, ObjectInfo> nameToObject = new Dictionary<string, ObjectInfo>();
        List<ObjectInfo> objects = new List<ObjectInfo>();

        Log logger;
        
        public void PrintMap()
        {
            logger.write("PRINT MAP " + objects.Count());
            var listResult = new JsonList("");
            objects.ForEach(obj => listResult.Add(obj.toJson()));
            var map = listResult.ToString();

            List<IMyTextPanel> surfaces = new List<IMyTextPanel>();
            reScanObjectGroupLocal(surfaces, MapTag);
            logger.write(map);
            surfaces.ForEach(s => s.WriteText(map));
        }

        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            logger = new Log(this);
        }

        public void Setup()
        {
            List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(list, x => x.CubeGrid == Me.CubeGrid);
            antenna = list.ConvertAll(x => (IMyRadioAntenna)x);

            if (antenna.Count() > 0)
            {
                logger.write("Setup complete");
                setupcomplete = true;
            }
            else
            {
                logger.write("Setup failed. No antenna found");
            }
        }

        public void Use(JsonObject jsonData)
        {
            if (!jsonData.ContainsKey("Name")) return;

            var name = ((JsonPrimitive)jsonData["Name"]).GetValue<string>();
            ObjectInfo obj;

            if (nameToObject.ContainsKey(name))
            {
                obj = nameToObject[name];
            }
            else
            {
                obj = new ObjectInfo(name);
                objects.Add(obj);
                nameToObject[name] = obj;
            }
            Echo(jsonData.ToString(false));
            obj.Update(jsonData);
        }

        public void Main(string arg, UpdateType updateSource)
        {
            // If setupcomplete is false, run Setup method.
            if (!setupcomplete)
            {
                logger.write("Running setup");
                Setup();
                return;
            }

            PrintMap();

            statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
            while (statusListener.HasPendingMessage)
            {
                MyIGCMessage newStatus = statusListener.AcceptMessage();
                if (statusListener.Tag == statusChannelTag)
                {
                    if (newStatus.Data is string)
                    {
                        logger.write("in: " + (string)newStatus.Data);
                        JsonObject jsonData;
                        try
                        {
                            jsonData = (new JSON((string)newStatus.Data)).Parse() as JsonObject;
                        }
                        catch (Exception e) // in case something went wrong (either your json is wrong or my library has a bug :P)
                        {
                            logger.write("There's somethign wrong with your json: " + e.Message);
                            continue;
                        }
                        Echo(">>> " + jsonData["Name"].ToString());
                        Use(jsonData);
                    }
                }
            }


        }       



        #region PreludeFooter //@remove
    } //@remove
} //@remove
#endregion //@remove