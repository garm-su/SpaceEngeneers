
#region Prelude
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
// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.TMP3
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
        /*AutoAssembly script created by Hitori*/

        //HellArea Assembler Queue Manager v1.83 (2018-2020) / mailto:HellArea@Outlook.com
        //Run parameters: On, Off, OnOff, ToDefault, ToZero
        //Panel keys : AQM Log, AQM Queue, AQM Comp, AQM Tool, AQM Res (or AQM [first letter of word])

        /*

        Configuration:

        Configuration is stored in the Custom Data field of the Programmable Block.

        There are three stanzas in the config, "Resource", "Assembler", and "LCD".
        Resource takes 2 arguments:
        1. Desired quantity.
        1. Maximum quantity.

        Once all components have reached their desired quantity, the assemblers will start
        producing more of things until their reach their maximum. At any point, if something
        drops below the desired amounts, all assemblers will be put to work bringing things
        to their desired, before going back to working up to the maximums.

        Assembler takes 1 argument, the name of an assembler. Any number of assemblers can
        be used by specifying the stanza multiple times. See the example config below.

        LCD takes 1 argument, the name of an LCD. Any number of LCD can be used, to spread the
        text across if you're producing a lot of resources. The example config below produces
        enough that it spreads it across 2 LCDs;

        Example Config: (I recommend copy-pasting this to the Custom Data to get started.)

        resource: SteelPlate, 5000, 10000
        resource: LargeTube, 100, 200
        resource: SmallTube, 500, 1000
        resource: InteriorPlate, 750, 1500
        resource: Thrust, 3000, 6000
        resource: Construction, 1000, 2000
        resource: Motor, 200, 400
        resource: BulletproofGlass, 500, 1000
        resource: Display, 50, 100
        resource: Girder, 20, 40
        resource: MetalGrid, 500, 1000
        resource: Computer, 500, 1000
        resource: Detector, 10, 20
        resource: Medical, 10, 20
        resource: GravityGenerator, 1, 2
        resource: PowerCell, 10, 20
        // break between two LCDs
        resource: RadioCommunication, 10, 20
        resource: Superconductor, 200, 400
        resource: SolarCell, 10, 20
        resource: Reactor, 4000, 8000
        resource: Canvas, 10, 20
        resource: Explosives, 10, 20
        resource: Missile200mm, 10, 20
        // These two don't work yet:
        //resource: NATO_25x184mm, 10, 20
        //resource: NATO_5p56x45mm, 10, 20

        assembler: Station Assembler 1
        assembler: Station Assembler 2
        assembler: Station Assembler 3

        lcd: Assembler LCD 1
        lcd: Assembler LCD 2

        */







        // You should not need to modify anything below here for configuration.















        // This script was built at 2018-07-20 00:05

        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project,
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

        Char[] NEWLINE_SEP = new Char[] { '\n' };
        Char[] COLON_SEP = new Char[] { ':' };
        Char[] COMMA_SEP = new Char[] { ',' };
        String oldConfig = null;
        bool configError = false;
        // Dictionaries keyed by Inventory name
        List<String> orderedResources = new List<String>();
        Dictionary<String, int> currentQuantities = new Dictionary<String, int>();
        Dictionary<String, int> desiredQuantities = new Dictionary<String, int>();
        Dictionary<String, int> maxQuantities = new Dictionary<String, int>();
        Dictionary<String, String> errors = new Dictionary<String, String>();

        const int inventoriesUpdateInterval = 100;
        int nextInventoriesUpdate = 0;
        List<IMyInventory> inventories = new List<IMyInventory>();

        Dictionary<String, HashSet<IMyProductionBlock>> assemblersInUse = new Dictionary<String, HashSet<IMyProductionBlock>>();
        List<IMyProductionBlock> assemblers = new List<IMyProductionBlock>();
        List<IMyProductionBlock> availableAssemblers = new List<IMyProductionBlock>();

        List<IMyTextPanel> lcds = new List<IMyTextPanel>();

        LCDText lcdText;
        ComponentDetails componentDetails;

        IMyCubeGrid grid;

        public Program()
        {
            lcdText = new LCDText(this);
            componentDetails = new ComponentDetails(this);

            LoadConfigIfChanged();
            grid = Me.CubeGrid;

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means.
            //
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            bool debug = false;
            if ("debug".Equals(argument))
            {
                debug = true;
            }

            if (debug) Echo("LoadConfigIfChanged");
            LoadConfigIfChanged();
            if (configError)
            {
                return;
            }
            if (debug) Echo("PruneIdleAssemblers");
            PruneIdleAssemblers();
            if (availableAssemblers.Count == 0)
            {
                return;
            }
            if (nextInventoriesUpdate-- <= 0)
            {
                nextInventoriesUpdate = inventoriesUpdateInterval;
                if (debug) Echo("UpdateInventories");
                UpdateInventories();
            }
            if (debug) Echo("UpdateCurrentInventory");
            UpdateCurrentInventory();
            if (debug) Echo("PruneAssemblersInUse");
            PruneAssemblersInUse();
            if (debug) Echo("QueueNecessaryProduction(false)");
            QueueNecessaryProduction(false);
            if (debug) Echo("QueueNecessaryProduction(true)");
            QueueNecessaryProduction(true);
            if (debug) Echo("UpdateLCDs");
            UpdateLCDs();
        }

        void LoadConfigIfChanged()
        {
            String config = Me.CustomData;
            if (config.Equals(oldConfig))
            {
                return;
            }
            //Echo("Config: " + config);

            if (assemblers != null)
            {
                foreach (IMyProductionBlock assembler in assemblers)
                {
                    assembler.ClearQueue();
                }
            }

            orderedResources.Clear();
            desiredQuantities.Clear();
            assemblers.Clear();
            availableAssemblers.Clear();
            foreach (String key in assemblersInUse.Keys)
            {
                assemblersInUse[key].Clear();
            }

            Echo("Loading config");
            // Parse config
            var configLines = config.Split(NEWLINE_SEP);
            foreach (String rawLine in configLines)
            {
                String line = rawLine.Trim();
                if (line.Equals(""))
                {
                    continue;
                }
                if (line.StartsWith("#") || line.StartsWith("//"))
                {
                    continue;
                }

                //Echo("Reading line: " + line);
                var parts = line.Split(COLON_SEP, 2);
                if (parts.Length < 2)
                {
                    Echo("Unparsable config line: " + line);
                    configError = true;
                }

                var args = parts[1].Split(COMMA_SEP);
                if (parts[0].Trim().ToLower().Equals("resource"))
                {
                    if (args.Length == 3)
                    {
                        String resourceName = args[0].Trim();
                        int desiredQuantity = int.Parse(args[1]);
                        int maxQuantity = int.Parse(args[2]);
                        orderedResources.Add(resourceName);
                        desiredQuantities[resourceName] = desiredQuantity;
                        maxQuantities[resourceName] = maxQuantity;
                        assemblersInUse[resourceName] = new HashSet<IMyProductionBlock>();
                    }
                    else
                    {
                        Echo("Wrong number of arguments (expected 3) to 'resource': " + line);
                        configError = true;
                    }
                }
                else if (parts[0].Trim().ToLower().Equals("assembler"))
                {
                    if (args.Length == 1)
                    {
                        String assemblerName = args[0].Trim();
                        IMyProductionBlock assembler = GridTerminalSystem.GetBlockWithName(assemblerName) as IMyProductionBlock;
                        if (assembler != null)
                        {
                            assemblers.Add(assembler);
                            availableAssemblers.Add(assembler);
                            assembler.ClearQueue();
                        }
                        else
                        {
                            Echo("Unable to find assembler: " + assemblerName);
                            configError = true;
                        }
                    }
                    else
                    {
                        Echo("Wrong number of arguments (expected 1) to 'assembler': " + line);
                        configError = true;
                    }
                }
                else if (parts[0].Trim().ToLower().Equals("lcd"))
                {
                    if (args.Length == 1)
                    {
                        String lcdName = args[0].Trim();
                        IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
                        if (lcd != null)
                        {
                            lcds.Add(lcd);
                        }
                        else
                        {
                            Echo("Unable to find LCD: " + lcdName);
                            configError = true;
                        }
                    }
                    else
                    {
                        Echo("Wrong number of arguments (expected 3) to 'lcd': " + line);
                        configError = true;
                    }
                }
                else
                {
                    Echo("Unknown config element: " + line);
                    configError = true;
                }
            }

            if (configError)
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Echo("Config errors found. Script stopped. Recompile after fixing config.");
            }

            oldConfig = config;
        }

        // Remove any assemblers with empty queues
        void PruneIdleAssemblers()
        {
            foreach (IMyProductionBlock assembler in assemblers.ToList())
            {
                if (assembler.IsQueueEmpty)
                {
                    Echo(assembler.CustomName + " was empty");
                    ReturnAssembler(assembler);
                }
            }
        }

        private void ReturnAssembler(IMyProductionBlock assembler, String key = "")
        {
            assembler.ClearQueue();
            if (key.Length > 0)
            {
                assemblersInUse[key].Remove(assembler);
            }
            else
            {
                foreach (String key2 in assemblersInUse.Keys)
                {
                    assemblersInUse[key2].Remove(assembler);
                }
            }
            availableAssemblers.Add(assembler);
        }

        void UpdateInventories()
        {
            inventories.Clear();
            List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventoryBlocks);
            // add the inventories from the blocks to the inventory list
            foreach (IMyTerminalBlock block in inventoryBlocks)
            {
                if (!block.HasInventory || block.CubeGrid != grid)
                {
                    continue;
                }
                if (block is IMyReactor)
                {
                    // Don't count input resources (Uranium ingots) in reactors,
                    // since assemblers won't pull it from there.
                    continue;
                }
                for (int i = 0; i < block.InventoryCount; ++i)
                {
                    inventories.Add(block.GetInventory(i));
                }
            }
        }

        void UpdateCurrentInventory()
        {
            currentQuantities.Clear();
            foreach (IMyInventory inventory in inventories)
            {
                List<IMyInventoryItem> items = inventory.GetItems();
                foreach (IMyInventoryItem item in items)
                {
                    String name = item.Content.SubtypeId.ToString();
                    String subtype = item.Content.TypeId.ToString().Split('_')[1];
                    Boolean isIngot = subtype.Equals("Ingot");
                    if (isIngot)
                    {
                        name = name + "." + subtype;
                    }
                    // Only bother keeping track if it's an ingot (input) or something we're producing (output)
                    if (isIngot || desiredQuantities.ContainsKey(name))
                    {
                        if (currentQuantities.ContainsKey(name))
                        {
                            currentQuantities[name] += (int)item.Amount;
                        }
                        else
                        {
                            currentQuantities[name] = (int)item.Amount;
                        }
                    }
                }
            }
        }

        // Remove any assemblers that we have produced enough resources from
        void PruneAssemblersInUse()
        {
            Echo("Pruning over-producing assemblers");
            foreach (String key in assemblersInUse.Keys)
            {
                int max = maxQuantities[key];
                int current = currentQuantities.ContainsKey(key) ? currentQuantities[key] : 0;
                foreach (IMyProductionBlock assembler in assemblersInUse[key].ToList())
                {
                    if (current >= max)
                    {
                        // if we have enough (because of parallel production, or from elsewhere) let’s stop.
                        Echo("Have enough " + key + ", clearing " + assembler.CustomName);
                        ReturnAssembler(assembler, key);
                    }
                }
            }
        }

        void QueueNecessaryProduction(bool useMax)
        {
            Echo("Queueing production");
            int maxAssemblersPerComponent = 1;
            bool haveNeed = true;
            int maxLoops = assemblers.Count;
            while (availableAssemblers.Count > 0 && haveNeed && maxLoops > 0)
            {
                haveNeed = false;
                foreach (String key in desiredQuantities.Keys)
                {
                    int desired = useMax ? maxQuantities[key] : desiredQuantities[key];
                    int current = currentQuantities.ContainsKey(key) ? currentQuantities[key] : 0;
                    haveNeed |= (desired > current);
                    int assemblerCount = assemblersInUse.ContainsKey(key) ?
                            assemblersInUse[key].Count : 0;
                    if (assemblerCount < maxAssemblersPerComponent)
                    {
                        if (desired > current)
                        {
                            Echo("Queuing " + key + " with " + assemblerCount + " assemblers already");
                            QueueProduction(key, Math.Min(componentDetails.QueueQuantity(key), desired - current));
                        }
                    }
                    else
                    {
                        Echo("Already have " + assemblerCount + " assemblers for " + key);
                    }
                    if (availableAssemblers.Count == 0)
                    {
                        return;
                    }
                }
                maxAssemblersPerComponent++;
                --maxLoops;
            }
        }

        void QueueProduction(String component, int count)
        {
            int quantity = GetMaxQuantityAndDeductResources(component, count);
            if (quantity == 0)
            {
                return;
            }
            Echo("Queuing " + quantity + " " + component);
            IMyProductionBlock assembler = availableAssemblers.Pop();
            MyDefinitionId objectIdToAdd = new MyDefinitionId();
            if (MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + componentDetails.ProductName(component), out objectIdToAdd))
            {
                try
                {
                    assembler.AddQueueItem(objectIdToAdd, (double)quantity);
                    Echo("Queued " + (int)quantity + " " + component + " on " + assembler.CustomName);
                    HashSet<IMyProductionBlock> assemblersForComponent = assemblersInUse[component];
                    assemblersForComponent.Add(assembler);
                    if (!assemblersInUse.ContainsKey(component))
                    {
                        assemblersInUse[component] = assemblersForComponent;
                    }
                    availableAssemblers.Remove(assembler);
                    errors.Remove(component);
                }
                catch (Exception e)
                {
                    Echo("Error queueing " + component + ": " + e);
                    errors[component] = "error";
                }
            }
            else
            {
                Echo("Something went wrong parsing definition for " + component);
            }
        }

        int GetMaxQuantityAndDeductResources(String component, double initialMax)
        {
            Dictionary<String, double> resources = componentDetails.RequiredResources(component);
            Echo("Checking quantities for " + component);
            double max = initialMax;
            foreach (String resource in resources.Keys)
            {
                int resourceQuantity = currentQuantities.ContainsKey(resource) ? currentQuantities[resource] : 0;
                Echo(resourceQuantity + " " + resource);
                max = Math.Min(max, resourceQuantity / resources[resource]);
            }
            foreach (String resource in resources.Keys)
            {
                if (currentQuantities.ContainsKey(resource))
                {
                    currentQuantities[resource] = currentQuantities[resource] - (int)(resources[resource] * max);
                }
            }
            Echo("Max " + max);
            return (int)max;
        }

        void UpdateLCDs()
        {
            Echo("Updating LCD");
            lcdText.Clear();
            lcdText.RightPad("Component", 22).Append(" ")
                            .LeftPad("Status", 11).Append(" ")
                            .LeftPad("Qty", 5).Append("/")
                            .LeftPad("Targ", 5).Append("/")
                            .LeftPad("Max", 5).EndLine();
            foreach (String key in desiredQuantities.Keys)
            {
                String displayName = componentDetails.DisplayName(key);
                int desired = desiredQuantities[key];
                int max = maxQuantities[key];
                int current = currentQuantities.ContainsKey(key) ? currentQuantities[key] : 0;
                int assemblerCount = assemblersInUse.ContainsKey(key) ? assemblersInUse[key].Count : 0;
                String note = "";
                if (errors.ContainsKey(key))
                {
                    note = errors[key];
                }
                else if (assemblerCount == 1)
                {
                    note = "building";
                }
                else if (assemblerCount > 1)
                {
                    note = "building x" + assemblerCount;
                }
                else if (desired > current)
                {
                    note = "waiting";
                }
                lcdText.RightPad(displayName, 22).Append(" ")
                        .LeftPad(note, 11).Append(" ")
                        .LeftPad(current.ToString(), 5).Append("/")
                        .LeftPad(desired.ToString(), 5).Append("/")
                        .LeftPad(max.ToString(), 5).EndLine();
            }
            lcdText.WriteTextToLCDs(lcds);
        }

        public class ComponentDetails
        {
            private Dictionary<String, Dictionary<String, double>> requiredResources = new Dictionary<String, Dictionary<String, double>>();
            private Dictionary<String, String> displayNames = new Dictionary<String, String>();
            private Dictionary<String, String> productNames = new Dictionary<String, String>();
            private Dictionary<String, int> queueQuantities = new Dictionary<String, int>();

            private Dictionary<String, double> EMPTY = new Dictionary<string, double>();
            private Program program;

            public ComponentDetails(Program program)
            {
                this.program = program;

                Dictionary<String, double> resources;
                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 21;
                requiredResources["SteelPlate"] = resources;
                queueQuantities["SteelPlate"] = 10;
                productNames["SteelPlate"] = "SteelPlate";
                displayNames["SteelPlate"] = "Steel Plates";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 3.5;
                requiredResources["InteriorPlate"] = resources;
                queueQuantities["InteriorPlate"] = 10;
                productNames["InteriorPlate"] = "InteriorPlate";
                displayNames["InteriorPlate"] = "Interior Plates";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 5;
                requiredResources["SmallTube"] = resources;
                queueQuantities["SmallTube"] = 10;
                productNames["SmallTube"] = "SmallTube";
                displayNames["SmallTube"] = "Small Tubes";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 30;
                requiredResources["LargeTube"] = resources;
                queueQuantities["LargeTube"] = 10;
                productNames["LargeTube"] = "LargeTube";
                displayNames["LargeTube"] = "Large Tubes";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 10;
                requiredResources["Construction"] = resources;
                queueQuantities["Construction"] = 7;
                productNames["Construction"] = "ConstructionComponent";
                displayNames["Construction"] = "Construction Comps";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 7;
                requiredResources["Girder"] = resources;
                queueQuantities["Girder"] = 10;
                productNames["Girder"] = "GirderComponent";
                displayNames["Girder"] = "Girders";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 20;
                resources["Nickel.Ingot"] = 5;
                requiredResources["Motor"] = resources;
                queueQuantities["Motor"] = 10;
                productNames["Motor"] = "MotorComponent";
                displayNames["Motor"] = "Motors";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 0.5;
                resources["Silicon.Ingot"] = 0.2;
                requiredResources["Computer"] = resources;
                queueQuantities["Computer"] = 10;
                productNames["Computer"] = "ComputerComponent";
                displayNames["Computer"] = "Computers";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 1;
                resources["Silicon.Ingot"] = 5;
                requiredResources["Display"] = resources;
                queueQuantities["Display"] = 10;
                productNames["Display"] = "Display";
                displayNames["Display"] = "Displays";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 10;
                resources["Silicon.Ingot"] = 8;
                requiredResources["SolarCell"] = resources;
                queueQuantities["SolarCell"] = 1;
                productNames["SolarCell"] = "SolarCell";
                displayNames["SolarCell"] = "Solar Cells";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 10;
                resources["Silicon.Ingot"] = 1;
                resources["Nickel.Ingot"] = 2;
                requiredResources["PowerCell"] = resources;
                queueQuantities["PowerCell"] = 3;
                productNames["PowerCell"] = "PowerCell";
                displayNames["PowerCell"] = "Power Cells";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 2;
                resources["Nickel.Ingot"] = 5;
                resources["Cobalt.Ingot"] = 3;
                requiredResources["MetalGrid"] = resources;
                queueQuantities["MetalGrid"] = 5;
                productNames["MetalGrid"] = "MetalGrid";
                displayNames["MetalGrid"] = "Metal Grids";

                resources = new Dictionary<String, double>();
                resources["Silicon.Ingot"] = 15;
                requiredResources["BulletproofGlass"] = resources;
                queueQuantities["BulletproofGlass"] = 10;
                productNames["BulletproofGlass"] = "BulletproofGlass";
                displayNames["BulletproofGlass"] = "Bulletproof Glass";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 15;
                resources["Stone.Ingot"] = 20;
                resources["Silver.Ingot"] = 5;
                requiredResources["Reactor"] = resources;
                queueQuantities["Reactor"] = 10;
                productNames["Reactor"] = "ReactorComponent";
                displayNames["Reactor"] = "Reactor Components";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 30;
                resources["Cobalt.Ingot"] = 10;
                resources["Gold.Ingot"] = 1;
                resources["Platinum.Ingot"] = 0.4;
                requiredResources["Thrust"] = resources;
                queueQuantities["Thrust"] = 10;
                productNames["Thrust"] = "ThrustComponent";
                displayNames["Thrust"] = "Thruster Components";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 10;
                resources["Gold.Ingot"] = 2;
                requiredResources["Superconductor"] = resources;
                queueQuantities["Superconductor"] = 2;
                productNames["Superconductor"] = "Superconductor";
                displayNames["Superconductor"] = "Super Conducting Comps";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 600;
                resources["Cobalt.Ingot"] = 220;
                resources["Silver.Ingot"] = 5;
                resources["Gold.Ingot"] = 10;
                requiredResources["GravityGenerator"] = resources;
                queueQuantities["GravityGenerator"] = 10;
                productNames["GravityGenerator"] = "GravityGeneratorComponent";
                displayNames["GravityGenerator"] = "Gravity Generator Comps";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 8;
                resources["Silicon.Ingot"] = 1;
                requiredResources["RadioCommunication"] = resources;
                queueQuantities["RadioCommunication"] = 10;
                productNames["RadioCommunication"] = "RadioCommunicationComponent";
                displayNames["RadioCommunication"] = "Radio Communication";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 60;
                resources["Nickel.Ingot"] = 70;
                resources["Silver.Ingot"] = 20;
                requiredResources["Medical"] = resources;
                queueQuantities["Medical"] = 10;
                productNames["Medical"] = "MedicalComponent";
                displayNames["Medical"] = "Medical Components";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 15;
                resources["Nickel.Ingot"] = 5;
                requiredResources["Detector"] = resources;
                queueQuantities["Detector"] = 10;
                productNames["Detector"] = "DetectorComponent";
                displayNames["Detector"] = "Detector Components";

                resources = new Dictionary<String, double>();
                resources["Magnesium.Ingot"] = 2;
                resources["Silicon.Ingot"] = 0.5;
                requiredResources["Explosives"] = resources;
                queueQuantities["Explosives"] = 1;
                productNames["Explosives"] = "ExplosivesComponent";
                displayNames["Explosives"] = "Explosives";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 2;
                resources["Silicon.Ingot"] = 35;
                requiredResources["Canvas"] = resources;
                queueQuantities["Canvas"] = 3;
                productNames["Canvas"] = "CanvasComponent";
                displayNames["Canvas"] = "Canvas";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 8;
                resources["Nickel.Ingot"] = 0.2;
                resources["Magnesium.Ingot"] = 0.15;
                requiredResources["NATO_5p56x45mm"] = resources;
                queueQuantities["NATO_5p56x45mm"] = 1;
                productNames["NATO_5p56x45mm"] = "NATO_5p56x45mm";
                displayNames["NATO_5p56x45mm"] = "Ammo Magazines";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 40;
                resources["Nickel.Ingot"] = 5;
                resources["Magnesium.Ingot"] = 3;
                requiredResources["NATO_25x184mm"] = resources;
                queueQuantities["NATO_25x184mm"] = 1;
                productNames["NATO_25x184mm"] = "NATO_25x184mm";
                displayNames["NATO_25x184mm"] = "Ammo Containers";

                resources = new Dictionary<String, double>();
                resources["Iron.Ingot"] = 55;
                resources["Nickel.Ingot"] = 7;
                resources["Magnesium.Ingot"] = 1.2;
                resources["Silicon.Ingot"] = 0.2;
                resources["Platinum.Ingot"] = 0.04;
                resources["Uranium.Ingot"] = 0.1;
                requiredResources["Missile200mm"] = resources;
                queueQuantities["Missile200mm"] = 1;
                productNames["Missile200mm"] = "Missile200mm";
                displayNames["Missile200mm"] = "Missile Containers";
            }

            public Dictionary<String, double> RequiredResources(String resource)
            {
                if (requiredResources.ContainsKey(resource))
                {
                    return requiredResources[resource];
                }
                else
                {
                    program.Echo("Warning: no requirements found for " + resource);
                    return EMPTY;
                }
            }

            public String DisplayName(String key)
            {
                return displayNames[key];
            }

            public String ProductName(String key)
            {
                return productNames[key];
            }

            public int QueueQuantity(String key)
            {
                return queueQuantities[key];
            }
        }

        public class LCDText
        {
            private Program program;
            private Dictionary<int, String> padding = new Dictionary<int, String>();
            private StringBuilder text;
            private List<StringBuilder> pages = new List<StringBuilder>();
            int lines = 0;

            public LCDText(Program program)
            {
                this.program = program;
                text = new StringBuilder();
                pages.Add(text);

                padding[0] = "";
                padding[1] = " ";
                padding[2] = "  ";
                padding[3] = "   ";
                padding[4] = "    ";
                padding[5] = "     ";
            }

            public LCDText Clear()
            {
                lines = 0;
                text = pages[0];
                text.Clear();
                return this;
            }

            public LCDText Append(String str)
            {
                text.Append(str);
                return this;
            }

            public LCDText EndLine()
            {
                if (lines < 16)
                {
                    text.Append("\n");
                }
                else
                {
                    lines = 0;
                    int curPage = pages.IndexOf(text);
                    program.Echo("curPage = " + curPage);
                    int nextPage = curPage + 1;
                    if (pages.Count <= nextPage)
                    {
                        program.Echo("Creating new page" + nextPage);
                        text = new StringBuilder();
                        pages.Add(text);
                    }
                    else
                    {
                        program.Echo("Using existing page " + nextPage);
                        text = pages[nextPage];
                        text.Clear();
                    }
                }
                ++lines;
                return this;
            }

            public LCDText LeftPad(String source, int charWidths)
            {
                int count = (charWidths - source.Length);
                text.Append(GetPadding(count)).Append(source);
                return this;
            }

            public LCDText RightPad(String source, int charWidths)
            {
                int count = (charWidths - source.Length);
                text.Append(source).Append(GetPadding(count));
                return this;
            }

            private String GetPadding(int length)
            {
                if (!padding.ContainsKey(length))
                {
                    padding[length] = new String(' ', Math.Max(0, length));
                }
                return padding[length];
            }

            public void WriteTextToLCDs(List<IMyTextPanel> lcds)
            {
                // Spread the text across all the LCD screens
                /*
                        List<String> pages = new List<String>();
                        String[] lines = text.ToString().Split(new Char[] { '\n' });
                        program.Echo(lines.Length + " lines");
                        int linesOnCurrentPage = 0;
                        StringBuilder page = new StringBuilder();
                        foreach (String line in lines)
                        {
                            page.Append(line + "\n");
                            linesOnCurrentPage++;
                            if (linesOnCurrentPage == 17)
                            {
                                pages.Add(page.ToString());
                                page.Clear();
                                linesOnCurrentPage = 0;
                            }
                        }
                        if (page.Length > 0)
                        {
                            pages.Add(page.ToString());
                        }

                        for (int i = 0; i < Math.Min(pages.Count, lcds.Count); ++i)
                        {
                            lcds[i].WritePublicText(pages[i], false);
                            lcds[i].ShowPublicTextOnScreen();
                        }
                        */
                for (int i = 0; i < Math.Min(pages.Count, lcds.Count); ++i)
                {
                    program.Echo("Writing to " + lcds[i].CustomName);
                    lcds[i].WritePublicText(pages[i], false);
                    lcds[i].ShowPublicTextOnScreen();
                }
            }
        }


    }
}
