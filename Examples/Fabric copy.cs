
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
namespace SpaceEngineers.UWBlockPrograms.TMP2
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
        /*AutoAssembly script created by Hitori*/

        //HellArea Assembler Queue Manager v1.83 (2018-2020) / mailto:HellArea@Outlook.com
        //Run parameters: On, Off, OnOff, ToDefault, ToZero
        //Panel keys : AQM Log, AQM Queue, AQM Comp, AQM Tool, AQM Res (or AQM [first letter of word])

        string ThisGridUserName = "";//Preffered Grid CustomName
        bool DontRenameThisPB = false;//This program block will not be ranamed to •• AQM

        bool AQM_AssemblersCoopMode = true;
        int AQM_LogLinesCount = 5;
        string AQM_LogPanelKey = "AQM L";//"AQM Log";

        //Set ""  for to stop next features
        string AQM_QueuePanelKey = "AQM Q";//"AQM Queue"
        string AQM_ComponentsPanelKey = "AQM C";// "AQM Comp"
        string AQM_ToolsPanelKey = "AQM T";//"AQM Tool"
        string AQM_ResPanelKey = "AQM R";//"AQM Res"
        string AQM_ModPanelKey = "AQM M";// "AQM Mod"

        bool FillBootles = true;
        bool FreeAssemblersInventory = true;//Once per 17 sec(1/10*1.66)
        bool StackInventoriesOptimization = true;//Once per 10 min (1/360*1.66)
        bool AQM_EliteToolsDefQuota_On = true;//1 by default
        int DelayTick = 1; //1 is about 1.66 second, so 10 is about 17 secons 

        void InitModObjects()
        {
            //  Example for CSD Battlecanon mod
            //AddMod("25cm HE Shell", "250shell", eDefinitionType.MyObjectBuilder_AmmoMagazine, "250shell");
            //AddMod("8.8cm APCR Shell", "88shell", eDefinitionType.MyObjectBuilder_AmmoMagazine, "88shell");
            //  Example for PDC Gatling turret mod
            //AddMod("25x184mm Depleted Uranium", "DeU_25x184mm", eDefinitionType.MyObjectBuilder_AmmoMagazine, "DeU_25x184mm");
            //  Example for More Solar Power mod
            //AddMod("Black Solar Cell", "SolarCellBlack", eDefinitionType.MyObjectBuilder_Component, "SolarCellBlack");
            //AddMod("Gold Solar Cell", "SolarCellGold", eDefinitionType.MyObjectBuilder_Component, "SolarCellGold");
            //  Example for Revived Large Ship Railguns
            //AddMod("50mm NiFe/DU Slug", "NiFeDUSlugMagazineLZM", eDefinitionType.MyObjectBuilder_AmmoMagazine, "NiFeDUSlugMagazineLZM");
            //  Example for Energy Shields
            //AddMod("Shield Component", "ShieldComponent", eDefinitionType.MyObjectBuilder_Component, "Shield");
            //  Example for Concrete Tool - placing voxels in survival
            //AddMod("Concrete Mix", "Blueprint_ConcreteMix", eDefinitionType.MyObjectBuilder_AmmoMagazine, "ConcreteMix");
            //AddMod("Concrete Tool", "Blueprint_ConcreteTool", eDefinitionType.MyObjectBuilder_PhysicalGunObject, "PhysicalConcreteTool");
            /* -- Game Updates -- */
            AddMod("Datapad", "Datapad", eDefinitionType.MyObjectBuilder_Datapad, "Datapad");
        }

        public Program() { Boot(); Runtime.UpdateFrequency = UpdateFrequency.Update100; }
        public void Save() { InitStorage(false); }

        int Tick = 0;
        public void Main(string Argument, UpdateType UpdateSource)
        {
            if (Core.State < 1) { Boot(); return; }
            if (!string.IsNullOrEmpty(Argument)) { ProceedArgument(Argument); }
            else if (Tick < DelayTick) { Tick++; }
            else { Tick = 0; Thread(); }
            //Echo(DateTime.Now.ToLongTimeString() + "  " + Runtime.CurrentInstructionCount.ToString());
        }

        public struct tCore
        {
            public int State; public List<string> StateName;
            public int GlobalBlocksCount;
            public List<IMyTerminalBlock> Blocks;
            public List<IMyAssembler> Assemblers; public IMyAssembler MainAssembler;
            public List<IMyTerminalBlock> CargoBlocks;
            public List<IMyTerminalBlock> BlocksWithInventory;
            public List<IMyGasGenerator> GasGens;
            public Dictionary<string, List<IMyTextSurface>> Surfaces;
            public Dictionary<string, string> SurfaceValue;
            public double CargoStatus;
            public string CustomValues;
            public int Lang;
            public List<string> LogLines;
        }
        tCore Core;

        Dictionary<string, int> iDefinitionId, iSubTypeName, iName, rIngotDefinitionIDName, rOreDefinitionIDName, rName;
        int[] iQuota, iDef, iQueue, iAmount, rIngotAmount, rOreAmount;
        int iFor, iForV, rFor;

        const string BlueprintTypeID = "MyObjectBuilder_BlueprintDefinition";
        const string IngotTypeID = "MyObjectBuilder_Ingot/";
        const string OreTypeID = "MyObjectBuilder_Ore/";
        const string BottleOxygen = "MyObjectBuilder_OxygenContainerObject/OxygenBottle";
        const string BottleHydrogen = "MyObjectBuilder_GasContainerObject/HydrogenBottle";
        enum eDefinitionType { MyObjectBuilder_Component, MyObjectBuilder_AmmoMagazine, MyObjectBuilder_PhysicalGunObject, MyObjectBuilder_Datapad };
        string[] DefinitionTypeName = new string[] { "MyObjectBuilder_Component/", "MyObjectBuilder_AmmoMagazine/", "MyObjectBuilder_PhysicalGunObject/", "MyObjectBuilder_Datapad/" };

        void Boot()
        {
            if (String.IsNullOrEmpty(ThisGridUserName)) { ThisGridUserName = Me.CubeGrid.CustomName; } else { Me.CubeGrid.CustomName = ThisGridUserName; }
            InitStorage(true, true);
            LoadTranslation();
            Core = new tCore()
            {
                State = 0,
                MainAssembler = null,
                CustomValues = Me.CustomData,
                StateName = new List<string>() { TT[0], TT[1], TT[2] },
                LogLines = new List<string>(),
                Surfaces = new Dictionary<string, List<IMyTextSurface>>(),
                SurfaceValue = new Dictionary<string, string>(),
                Assemblers = new List<IMyAssembler>(),
                BlocksWithInventory = new List<IMyTerminalBlock>(),
                CargoBlocks = new List<IMyTerminalBlock>(),
                Blocks = new List<IMyTerminalBlock>(),
                GasGens = new List<IMyGasGenerator>()
            };
            AQM_LogLinesCount = Math.Abs(AQM_LogLinesCount);
            AQM_LogPanelKey = AQM_LogPanelKey.ToLower();
            AQM_QueuePanelKey = AQM_QueuePanelKey.ToLower();
            AQM_ComponentsPanelKey = AQM_ComponentsPanelKey.ToLower();
            AQM_ToolsPanelKey = AQM_ToolsPanelKey.ToLower();
            AQM_ResPanelKey = AQM_ResPanelKey.ToLower();
            AQM_ModPanelKey = AQM_ModPanelKey.ToLower();
            InitDevices();
            EnumerateComponents();
            FillBootles &= Core.GasGens.Any();
            InitStorage(true);
        }

        int WorldLoading = 0;
        void SetState(int State, string Message = null)
        {
            switch (State)
            {
                case 0: Core.State = 0; break;
                case 1:
                    Core.State = 1; ToScreen(TT[4]);
                    break;
                case 2:
                    if (InitAssemblers())
                    {
                        if (Core.MainAssembler.Mode == MyAssemblerMode.Disassembly)
                        { Core.State = 1; ToScreen(TT[5]); break; }
                        Core.State = 2;
                        if (String.IsNullOrEmpty(Message))
                        { Message = TT[6] + Core.Assemblers.Count + TT[92] + Core.BlocksWithInventory.Count + TT[7] + ((AQM_AssemblersCoopMode) ? TT[8] : TT[9]); }
                        ToScreen(Message);
                    }
                    else { if (WorldLoading > 5) { ToScreen(TT[10]); Core.State = 1; } else { ToScreen(TT[11]); WorldLoading++; } }
                    break;
            }
            if (Core.State == 1) { ClearAssemblersQueue(); }
            InitStorage(false);
        }

        const string _tMA = "MyAssembler";
        void InitDevices()
        {
            if (!DontRenameThisPB) { Me.CustomName = " •• AQM"; }
            Core.Blocks.Clear();
            Core.Assemblers.Clear();
            Core.CargoBlocks.Clear();
            Core.BlocksWithInventory.Clear();
            GridTerminalSystem.GetBlocksOfType(Core.Blocks, R => R.CubeGrid == Me.CubeGrid);
            Core.Surfaces.Clear();
            Core.SurfaceValue.Clear();
            Core.GasGens.Clear();
            Core.GlobalBlocksCount = Core.Blocks.Count;
            Core.Surfaces.Add(AQM_ComponentsPanelKey, GetSurfaces(AQM_ComponentsPanelKey, ref Core.Blocks));
            Core.Surfaces.Add(AQM_LogPanelKey, GetSurfaces(AQM_LogPanelKey, ref Core.Blocks));
            Core.Surfaces.Add(AQM_ModPanelKey, GetSurfaces(AQM_ModPanelKey, ref Core.Blocks));
            Core.Surfaces.Add(AQM_QueuePanelKey, GetSurfaces(AQM_QueuePanelKey, ref Core.Blocks));
            Core.Surfaces.Add(AQM_ResPanelKey, GetSurfaces(AQM_ResPanelKey, ref Core.Blocks));
            Core.Surfaces.Add(AQM_ToolsPanelKey, GetSurfaces(AQM_ToolsPanelKey, ref Core.Blocks));
            Core.Assemblers.AddRange(Core.Blocks.Where(R => R.GetType().Name == _tMA).Cast<IMyAssembler>());
            Core.CargoBlocks.AddRange(Core.Blocks.Where(R => R.HasInventory && (R is IMyCargoContainer || R is IMyCockpit || R is IMyShipDrill || R is IMyShipGrinder || R is IMyShipWelder)));
            Core.BlocksWithInventory.AddRange(Core.Blocks.Where(R => R.HasInventory));
            Core.GasGens.AddRange(Core.Blocks.Where(R => R is IMyGasGenerator).Cast<IMyGasGenerator>());
        }

        bool InitAssemblers()
        {
            Core.MainAssembler = null; if (Core.Assemblers.Count < 1) { return false; }
            foreach (IMyAssembler A in Core.Assemblers) { if (A.CustomName.ToLower() == " • assembler aqm") { Core.MainAssembler = A; break; } }
            if (Core.MainAssembler == null) { foreach (IMyAssembler A in Core.Assemblers) { if (A.CooperativeMode) { Core.MainAssembler = A; break; } } }
            if (Core.MainAssembler == null) { Core.MainAssembler = Core.Assemblers[0]; }
            Core.MainAssembler.CustomName = " • Assembler AQM"; Core.MainAssembler.CooperativeMode = false;
            if (AQM_AssemblersCoopMode)
            {
                for (int i = 0; i < Core.Assemblers.Count; i++)
                { if (Core.Assemblers[i].EntityId != Core.MainAssembler.EntityId) { Core.Assemblers[i].CooperativeMode = true; } }
            }
            return Core.MainAssembler.IsFunctional;
        }

        void EnumerateComponents()
        {
            // for Queue
            iSubTypeName = new Dictionary<string, int>()
{
{ "BulletproofGlass", 0}, { "Canvas", 1}, { "ComputerComponent", 2}, { "ConstructionComponent", 3}, { "DetectorComponent", 4},
{ "Display", 5}, { "ExplosivesComponent", 6}, { "GirderComponent", 7}, { "GravityGeneratorComponent", 8}, { "InteriorPlate", 9},
{ "LargeTube", 10}, { "MedicalComponent", 11}, { "MetalGrid", 12}, { "MotorComponent", 13}, { "PowerCell", 14},
{ "RadioCommunicationComponent", 15}, { "ReactorComponent", 16}, { "SmallTube", 17}, { "SolarCell", 18}, { "SteelPlate", 19},
{ "Superconductor", 20}, { "ThrustComponent", 21}, { "Missile200mm", 22}, { "NATO_25x184mmMagazine", 23}, { "NATO_5p56x45mmMagazine", 24},
{ "HydrogenBottle", 25}, { "OxygenBottle", 26}, { "AutomaticRifle", 27}, { "PreciseAutomaticRifle", 28}, { "RapidFireAutomaticRifle", 29},
{ "UltimateAutomaticRifle", 30}, { "AngleGrinder", 31}, { "AngleGrinder2", 32}, { "AngleGrinder3", 33}, { "AngleGrinder4", 34},
{ "HandDrill", 35}, { "HandDrill2", 36}, { "HandDrill3", 37}, { "HandDrill4", 38}, { "Welder", 39},
{ "Welder2", 40}, { "Welder3", 41}, { "Welder4", 42}
};


            //for Cargo
            string C = DefinitionTypeName[0]; string A = DefinitionTypeName[1]; string P = DefinitionTypeName[2];
            iDefinitionId = new Dictionary<string, int>()
{
{C +"BulletproofGlass", 0}, {C +"Canvas", 1}, {C +"Computer", 2}, {C +"Construction", 3}, {C +"Detector", 4},
{C +"Display", 5}, {C +"Explosives", 6}, {C +"Girder", 7}, {C +"GravityGenerator", 8}, {C +"InteriorPlate", 9},
{C +"LargeTube", 10}, {C +"Medical", 11}, {C +"MetalGrid", 12}, {C +"Motor", 13}, {C +"PowerCell", 14},
{C +"RadioCommunication", 15}, {C +"Reactor", 16}, {C +"SmallTube", 17}, {C +"SolarCell", 18}, {C +"SteelPlate", 19},
{C +"Superconductor", 20}, {C +"Thrust", 21}, {A +"Missile200mm", 22}, {A +"NATO_25x184mm", 23}, {A +"NATO_5p56x45mm", 24},
{"MyObjectBuilder_GasContainerObject/HydrogenBottle", 25}, {"MyObjectBuilder_OxygenContainerObject/OxygenBottle", 26},
{P +"AutomaticRifleItem", 27}, {P +"PreciseAutomaticRifleItem", 28}, {P +"RapidFireAutomaticRifleItem", 29}, {P +"UltimateAutomaticRifleItem", 30},
{ P +"AngleGrinderItem", 31}, {P +"AngleGrinder2Item", 32}, {P +"AngleGrinder3Item", 33}, {P +"AngleGrinder4Item", 34},
{P +"HandDrillItem", 35}, {P +"HandDrill2Item", 36}, {P +"HandDrill3Item", 37}, {P +"HandDrill4Item", 38}, {P +"WelderItem", 39},
{P +"Welder2Item", 40}, {P +"Welder3Item", 41}, {P +"Welder4Item", 42}
};

            //for View and Translation
            iName = new Dictionary<string, int>()
{
{TT[12], 0}, {TT[13], 1}, {TT[14], 2}, {TT[15], 3}, {TT[16], 4}, {TT[17], 5}, {TT[18], 6}, {TT[19], 7}, {TT[20], 8}, {TT[21], 9},
{TT[22], 10}, {TT[23], 11}, {TT[24], 12}, {TT[25], 13}, {TT[26], 14}, {TT[27], 15}, {TT[28], 16}, {TT[29], 17}, {TT[30], 18}, {TT[31], 19},
{TT[32], 20}, {TT[33], 21}, {TT[34], 22}, {TT[35], 23}, {TT[36], 24}, {TT[37], 25}, {TT[38], 26}, {TT[39], 27}, {TT[40], 28}, {TT[41], 29},
{TT[42], 30}, {TT[43], 31}, {TT[44], 32}, {TT[45], 33}, {TT[46], 34}, {TT[47], 35}, {TT[48], 36}, {TT[49], 37}, {TT[50], 38}, {TT[51], 39},
{TT[52], 40}, {TT[53], 41}, {TT[54], 42}
};

            iFor = iDefinitionId.Count; iForV = iFor;
            iQuota = new int[iFor]; iDef = new int[iFor];
            int D = 0;//Default 0
            for (int i = 0; i < iFor; i++) { iQuota[i] = D; iDef[i] = D; }
            //Preffered defaults, works by SubTypeName
            if (AQM_EliteToolsDefQuota_On) { _iDef(new string[] { "AngleGrinder4", "HandDrill4", "HydrogenBottle", "OxygenBottle", "UltimateAutomaticRifle", "Welder4" }, 1); }
            _iDef(new string[] { "ComputerComponent", "ConstructionComponent", "InteriorPlate", "LargeTube", "MetalGrid", "MotorComponent",
"ReactorComponent","SmallTube", "SteelPlate","ThrustComponent"}, 300);
            _iDef(new string[] { "Display" }, 100);
            _iDef(new string[] { "BulletproofGlass", "PowerCell", "SolarCell" }, 50);
            _iDef(new string[] { "RadioCommunicationComponent" }, 40);
            _iDef(new string[] { "DetectorComponent" }, 25);

            InitModObjects();
            LoadQuotas();

            //Resources
            string K = IngotTypeID;
            rIngotDefinitionIDName = new Dictionary<string, int>()
{
{ K + "Silver", 0}, { K + "Gold", 1}, { K + "Cobalt", 2}, { K + "Iron", 3}, { K + "Magnesium", 4},
{ K + "Nickel", 5}, { K + "Platinum", 6}, { K + "Silicon", 7}, { K + "Uranium", 8}, { K + "Stone", 9},
{ K + "Fake Ice", 10}, { K + "Fake Scrap", 11}
};
            K = OreTypeID;
            rOreDefinitionIDName = new Dictionary<string, int>()
{
{ K + "Silver", 0}, { K + "Gold", 1}, { K + "Cobalt", 2}, { K + "Iron", 3}, { K + "Magnesium", 4},
{ K + "Nickel", 5}, { K + "Platinum", 6}, { K + "Silicon", 7}, { K + "Uranium", 8}, { K + "Stone", 9},
{ K + "Ice", 10}, { K + "Scrap", 11}
};
            //for View and Translation
            rName = new Dictionary<string, int>()
{{TT[55], 0}, {TT[56], 1}, {TT[57], 2}, {TT[58], 3}, {TT[59], 4},{TT[60], 5}, {TT[61], 6}, {TT[62], 7}, { TT[63], 8}, {TT[64], 9},{ TT[65], 10}, { TT[66], 11}};
            rFor = rIngotDefinitionIDName.Count;
            rIngotAmount = new int[rFor]; rOreAmount = new int[rFor];

        }

        void _iDef(string[] Name, int Default) { foreach (string N in Name) { int i; if (iSubTypeName.TryGetValue(N, out i)) { iDef[i] = Default; } } }

        void AddMod(string Name, string BlueprintSubTypeName, eDefinitionType DefinitionType, string DefinitionId)
        {
            iName.Add(Name, iName.Count);
            iSubTypeName.Add(BlueprintSubTypeName, iSubTypeName.Count);
            iDefinitionId.Add(DefinitionTypeName[(int)DefinitionType] + DefinitionId, iDefinitionId.Count);
            iFor = iDefinitionId.Count;
            Array.Resize(ref iAmount, iFor);
            Array.Resize(ref iQueue, iFor);
            Array.Resize(ref iQuota, iFor);
            Array.Resize(ref iDef, iFor);
        }

        void LoadQuotas(int Value = -1)
        {
            ClearAssemblersQueue();
            if (String.IsNullOrEmpty(Core.CustomValues))
            {
                Core.CustomValues = "";
                for (int i = 0; i < iFor; i++) { Core.CustomValues += iName.ElementAt(i).Key + " = " + ((Value > -1) ? Value : iDef[i]) + "\n"; }
                Me.CustomData = Core.CustomValues;
            }
            string[] C = Core.CustomValues.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (string L in C)
            {
                int N = L.IndexOf("=");
                if (N > 0) { int i; if (iName.TryGetValue(L.Substring(0, N).Trim(), out i)) { iQuota[i] = (int)Convert.ToDouble("0" + L.Substring(N + 1).Trim()); } }
            }
        }

        void UpdateQueue()
        {
            iQueue = new int[iFor]; QueueWarning = false; int i;
            foreach (IMyAssembler A in Core.Assemblers)
            {
                if (A.Mode == MyAssemblerMode.Assembly)
                {
                    List<MyProductionItem> AQ = new List<MyProductionItem>(); A.GetQueue(AQ);
                    foreach (MyProductionItem P in AQ)
                    {
                        if (iSubTypeName.TryGetValue(P.BlueprintId.SubtypeName, out i))
                        { iQueue[i] += P.Amount.ToIntSafe(); }
                        else { QueueWarning = true; Echo(P.BlueprintId.ToString()); }
                    }
                }
            }
        }

        void ShowQueue()
        {
            string S = TT[67];
            for (int i = 0; i < iFor; i++) { if (iQueue[i] > 0) { S += " • " + iName.ElementAt(i).Key + " x " + iQueue[i] + "\n"; } }
            ToSurfaces(AQM_QueuePanelKey, S);
        }

        void ClearAssemblersQueue()
        {
            if (Core.Assemblers.Any())
            {
                if (Core.MainAssembler != null) { Core.MainAssembler.ClearQueue(); }
                foreach (IMyAssembler A in Core.Assemblers) { if (A.CooperativeMode) { A.ClearQueue(); } }
            }
        }

        void UpdateComponents()
        {
            iAmount = new int[iFor]; rIngotAmount = new int[rFor]; rOreAmount = new int[rFor]; CargoWarning = false;
            rIngotAmount[10] = -1; rIngotAmount[11] = -1;//-1 for NoIngotType (like Water)
            IMyInventory C; List<MyInventoryItem> Items = new List<MyInventoryItem>(); int i; string ID; IEnumerable<IMyGasGenerator> GG;
            foreach (IMyTerminalBlock B in Core.BlocksWithInventory)
            {
                for (int N = 0; N < B.InventoryCount; N++)
                {
                    C = B.GetInventory(N); Items.Clear(); C.GetItems(Items);
                    foreach (MyInventoryItem A in Items)
                    {
                        try
                        {
                            ID = A.Type.TypeId + "/" + A.Type.SubtypeId;
                            if (iDefinitionId.TryGetValue(ID, out i))
                            {
                                iAmount[i] += A.Amount.ToIntSafe();
                                if (FillBootles && (ID == BottleHydrogen || ID == BottleOxygen) && !(B is IMyGasGenerator))
                                {//honey badger style
                                    GG = Core.GasGens.Where(R => R.GetInventory().CanItemsBeAdded(A.Amount, A.Type) && R.GetInventory().CanTransferItemTo(C, A.Type));
                                    if (GG.Any()) { C.TransferItemTo(GG.First().GetInventory(), A); }
                                    else { foreach (IMyGasGenerator R in Core.GasGens) { if (C.TransferItemTo(R.GetInventory(), A)) { break; } } }
                                }
                            }
                            else if (rIngotDefinitionIDName.TryGetValue(ID, out i)) { rIngotAmount[i] += A.Amount.ToIntSafe(); }
                            else if (rOreDefinitionIDName.TryGetValue(ID, out i)) { rOreAmount[i] += A.Amount.ToIntSafe(); }
                            else { CargoWarning = true; Echo(ID); }
                        }
                        catch { }
                    }
                }
            }
        }

        void FreeAssemblers()
        {
            IMyInventory C; List<MyInventoryItem> Items = new List<MyInventoryItem>(); IEnumerable<IMyCargoContainer> Crg;
            Core.Assemblers.ForEach(R =>
            {
                C = R.GetInventory(0); Items.Clear(); C.GetItems(Items);
                if ((double)C.CurrentVolume.RawValue / (double)C.MaxVolume.RawValue > .6)
                {
                    foreach (MyInventoryItem A in Items)
                    {
                        try
                        {
                            Crg = Core.CargoBlocks.Where(B => B.GetInventory().CanItemsBeAdded(A.Amount, A.Type) && R.GetInventory().CanTransferItemTo(C, A.Type)).Cast<IMyCargoContainer>();
                            if (Crg.Any()) { C.TransferItemTo(Crg.First().GetInventory(), A); }
                        }
                        catch { }
                    }
                }
            });
        }

        void ShowMacro()
        {
            if (!String.IsNullOrEmpty(AQM_ComponentsPanelKey)) { ShowComponents(AQM_ComponentsPanelKey, "_Component/", TT[68]); }
            if (!String.IsNullOrEmpty(AQM_ToolsPanelKey)) { ShowComponents(AQM_ToolsPanelKey, "_Component/", TT[69], true); }
            if (!String.IsNullOrEmpty(AQM_ResPanelKey)) { ShowResources(); }
            if (!String.IsNullOrEmpty(AQM_QueuePanelKey)) { ShowQueue(); }
            if (!String.IsNullOrEmpty(AQM_ModPanelKey)) { ShowMods(); }
        }

        void ShowComponents(string PanelKey, string Mask, string ScreenTitle, bool Except = false)
        {
            int Q;
            string S = "   " + ScreenTitle + "\n";
            for (int i = 0; i < iForV; i++)
            {
                bool Add = iDefinitionId.ElementAt(i).Key.IndexOf(Mask) > -1; if (Except) { Add = !Add; }
                if (Add)
                {
                    Q = iAmount[i] - iQuota[i];
                    S += ((Q < 0) ? " • " : "    ") + iName.ElementAt(i).Key + "  " + iAmount[i] + ((iQuota[i] > 0) ? "…" + iQuota[i] : "") + "\n";
                }
            }
            ToSurfaces(PanelKey, S);
        }

        void ShowResources()
        {
            string S = TT[70];
            for (int i = 0; i < rFor; i++)
            {
                string IN = (rIngotAmount[i] > 1000) ? Math.Round((double)(rIngotAmount[i] / 1000), 1) + "k" : rIngotAmount[i].ToString();
                string OR = (rOreAmount[i] > 1000) ? Math.Round((double)(rOreAmount[i] / 1000), 1) + "k" : rOreAmount[i].ToString();
                S += ((rIngotAmount[i] < 50 && rIngotAmount[i] > -1) ? " • " : "    ")
                + rName.ElementAt(i).Key + "  " + ((rIngotAmount[i] >= 0) ? IN + "∙∙∙" : "* ") + OR + "\n";
            }
            ToSurfaces(AQM_ResPanelKey, S);
        }

        void ShowMods()
        {
            int Q;
            string S = TT[71];
            if (iFor > iForV)
            {
                for (int i = iForV; i < iFor; i++)
                {
                    Q = iAmount[i] - iQuota[i];
                    S += ((Q < 0) ? " • " : "    ") + iName.ElementAt(i).Key + "  " + iAmount[i] + ((iQuota[i] > 0) ? "…" + iQuota[i] : "") + "\n";
                }
            }
            ToSurfaces(AQM_ModPanelKey, S);
        }

        List<MyProductionItem> AQ = new List<MyProductionItem>(); MyDefinitionId MAQ = new MyDefinitionId();
        void ManageQueue()
        {
            AQ.Clear(); Core.MainAssembler.GetQueue(AQ);
            for (int i = 0; i < iFor; i++)
            {
                int A = iQuota[i] - iAmount[i] - iQueue[i];
                if (A > 0) { try { if (MyDefinitionId.TryParse(BlueprintTypeID, iSubTypeName.ElementAt(i).Key, out MAQ)) { Core.MainAssembler.AddQueueItem(MAQ, (decimal)A); } } catch { Echo("Warning : " + iSubTypeName.ElementAt(i).Key); } }
            }
        }

        void ProceedArgument(string Argument)
        {
            string A = Argument.ToLower();
            if (A == "on") { SetState(2); }
            else if (A == "off") { SetState(1); }
            else if (A == "onoff") { SetState(((Core.State == 2) ? 1 : 2)); }
            else if (A == "todefault") { Core.CustomValues = ""; LoadQuotas(); ToScreen(TT[72]); }
            else if (A == "tozero") { Core.CustomValues = ""; LoadQuotas(0); ToScreen(TT[73]); }
            else if (A == "eng") { Core.Lang = 0; InitStorage(false); LoadTranslation(); Core.State = 0; }
            else if (A == "ru") { Core.Lang = 1; InitStorage(false); LoadTranslation(); Core.State = 0; }
            else if (A == "stack") { StackCargo(); }
            else { ToScreen(TT[78]); }
        }

        int Impulse = 0;
        void Thread()
        {
            Impulse = Impulse > 360 ? 0 : Impulse + 1;
            if (FreeAssemblersInventory && Impulse % 10 == 0) { FreeAssemblers(); }
            if (StackInventoriesOptimization && Impulse == 1) { StackCargo(); }
            Core.CargoStatus = GetShipCargoStatus();
            if (Core.State == 2 && Me.CustomData != Core.CustomValues) { Core.CustomValues = Me.CustomData; LoadQuotas(); ToScreen(TT[80]); }
            UpdateComponents(); UpdateQueue();
            if (Core.State == 2) { if (Core.MainAssembler.Mode != MyAssemblerMode.Disassembly) { ManageQueue(); UpdateQueue(); } }
            ShowMacro();
            ToScreen();
        }

        double GetShipCargoStatus()
        {
            double U = 0; double S = 0; IMyInventory C;
            foreach (IMyTerminalBlock T in Core.CargoBlocks) { try { C = T.GetInventory(); U += C.CurrentVolume.RawValue; S += C.MaxVolume.RawValue; } catch { } }
            return (S == 0) ? -1 : Math.Round(U / S * 100, 1);
        }

        string sNow() => DateTime.Now.ToString("HH:mm:ss");
        string sNowHM() => DateTime.Now.ToString("HH:mm");
        string GetAsGPS(Vector3D argument, string Name = "Unknown") =>
        string.Format("GPS:{0}:{1:0.00}:{2:0.00}:{3:0.00}:", Name, argument.X, argument.Y, argument.Z);
        Vector3D GetForwardVector(double Distance, IMyTerminalBlock Master,
        Base6Directions.Direction ToDirection = Base6Directions.Direction.Forward)
        { return new Vector3D(Master.GetPosition() + Distance * Master.WorldMatrix.GetDirectionVector(ToDirection)); }

        List<IMyTextSurface> GetSurfaces(string Key, ref List<IMyTerminalBlock> Blocks)
        {
            Key = Key.ToLower();
            IMyTextSurfaceProvider N;
            List<IMyTextSurface> S = new List<IMyTextSurface>();
            S.AddRange(Blocks.Where(R => R.CustomName.ToLower().Contains(Key) && R is IMyTextPanel).Cast<IMyTextSurface>());
            foreach (IMyTerminalBlock T in Blocks.Where(R => R.CubeGrid == Me.CubeGrid && R.CustomName.ToLower().Contains(Key) && R is IMyTextSurfaceProvider).ToList())
            {
                N = (T as IMyTextSurfaceProvider);
                for (int i = 0; i < N.SurfaceCount; i++) { if (T.CustomName.ToLower().Contains(Key + (i + 1))) { S.Add(N.GetSurface(i)); } }
            }
            return S;
        }

        public void ToSurfaces(string Key, string Text)
        {
            if (!Core.SurfaceValue.ContainsKey(Key) || Core.SurfaceValue[Key] != Text)
            {
                Core.Surfaces[Key].ForEach(b => b.WriteText(Text));
                Core.SurfaceValue[Key] = Text;
            }
        }

        string AMLog = ""; bool QueueWarning = false; bool CargoWarning = false;
        void ToScreen(string Argument = null)
        {
            if (!string.IsNullOrEmpty(Argument))
            {
                Core.LogLines.Insert(0, " " + sNow() + " " + Argument);
                if (Core.LogLines.Count > AQM_LogLinesCount) { Core.LogLines.RemoveAt(AQM_LogLinesCount); }
                AMLog = String.Join("\n", Core.LogLines);
            }
            string Out = TT[88] + Core.StateName[Core.State] + TT[89] + Core.CargoStatus + "%\n"
            + ((!QueueWarning) ? "" : TT[90])
            + ((!CargoWarning) ? "" : TT[91])
            + "\n AQM>_ " + sNowHM() + "\n" + AMLog;
            if (Core.Surfaces[AQM_LogPanelKey].Count < 1) { Echo(Out); } else { ToSurfaces(AQM_LogPanelKey, Out); }
        }

        void InitStorage(bool Read, bool LangOnly = false)
        {
            if (Read)
            {
                string S = Storage;
                if (!String.IsNullOrEmpty(S))
                {
                    string[] C = S.Split(new string[] { "|-|" }, StringSplitOptions.None);
                    if (C.Length > 0)
                    {
                        int i = (int)Convert.ToDouble("0" + C[0].Trim());
                        if (!LangOnly) { SetState((i == 0) ? 1 : i); }
                        if (C.Length > 1) { Core.Lang = (int)Convert.ToDouble("0" + C[1].Trim()); }
                    }
                }
            }
            else { Storage = Core.State.ToString() + "|-|" + Core.Lang.ToString(); }
        }

        void StackCargo()
        {
            IMyInventory Inv; List<MyInventoryItem> Items = new List<MyInventoryItem>();
            Dictionary<string, int> D = new Dictionary<string, int>();
            int F = 0, C; string S;
            foreach (IMyTerminalBlock B in Core.Blocks.Where(R => R.HasInventory))
            {
                for (int N = 0; N < B.InventoryCount; N++)
                {
                    Inv = B.GetInventory(N); Items.Clear(); Inv.GetItems(Items);
                    if (Items.Count > 1)
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            S = Items[i].Type.SubtypeId + Items[i].Type.TypeId;
                            if (D.TryGetValue(S, out F)) { C = Items.Count; Inv.TransferItemTo(Inv, i, F); Items.Clear(); Inv.GetItems(Items); if (C > Items.Count) { i--; } } else { D.Add(S, i); }
                        }
                    }
                }
            }
        }

        string[] TT;
        void LoadTranslation()
        {
            switch (Core.Lang)//1 RU, Other Eng
            {
                case 1:
                    TT = new string[] { "Загрузка","Не активен","Активен",
"Антенна не доступна.\n Global Log не активен.",
"Режим ожидания \n(используй On, Off, Off)","Err.Главный сборщик\n  Режим разбора",
"Успешно запущен\n Сборщиков ","\n Совместный режим ","Вкл","Откл",
"Err.Сборщик не готов","Warn.Ожидание сборщика",
"Бронированное стекло","Полотно парашюта","Компьютер","Строительные компоненты","Компоненты детектора",
"Экран","Взрывчатка","Балка","Компоненты гравитационного генератора","Внутренняя пластина",
"Большая стальная труба","Медицинские компоненты","Компонент решётки","Мотор","Энергоячейка",
"Радиокомпоненты","Компоненты реактора","Малая трубка","Солнечная ячейка","Стальная пластина",
"Сверхпроводник","Детали ионного ускорителя","Ракета 200мм","Боеприпасы 25x184 мм НАТО","Магазин 5.56x45мм НАТО",
"Водородный баллон","Кислородный баллон","Автоматическая винтовка","Точная автоматическая винтовка","Скорострельная автоматическая винтовка",
"Элитная автоматическая винтовка","Болгарка","Улучшенная болгарка","Продвинутая болгарка","Элитная болгарка",
"Ручной бур","Улучшенный ручной бур","Продвинутый ручной бур","Элитный ручной бур","Сварщик",
"Улучшенный сварщик","Продвинутый сварщик","Элитный сварщик",
"Ag  Серебряная руда","Au  Золотая руда","Co  Кобальтовая руда","Fe  Железная руда","Mg  Магниевая руда",
"Ni  Никелевая руда","Pt  Платиновая руда","Si  Кремниевая руда","Ur  Урановая руда","Камень","H2O Лёд","Металлолом",
"   Очередь сборщика>_\n","Компоненты","Инструменты и боеприпасы","   Ресурсы   Слитки ∙∙∙ Руда\n","   Штуки из модов и обновлений\n",
"Квоты по умолчанию","Квоты обнулены","Отправлено","• Запрос списка гридов ..","Отправка запроса на стыковку ..",
"Отправка запроса на отстыковку ..","Err.Неизвестная команда","Err.Ошибка главного сборщика","Квоты обновлены",
"∙Потерян ","∙Поврежден ","\n Статус оборудования: ","\n Повреждения:","Устройства обновлены","Радио сообщение:\n ",
"Warn.Нет свободных коннекторов"," AQM.Статус : ","\n Загружено на "," > Неизвестные компоненты в очереди <\n"," > Хранятся неизвестные компоненты <\n","\n Блоков с инвентарем "};
                    break;
                default:
                    TT = new string[] { "Boot","Not active","Activated",
"Antenna is not available.\n Global Log not activated.",
"Idle mode\n (Use On, Off, OnOff\n as parameter)","Err.Main Assembler in\n  Disassemble mode",
"Started successfully\n Assemblers count ","\n Cooperative mode ","On","Off",
"Err.Assembler not ready","Warn.Waiting for Assembler",
"Bulletproof Glass","Canvas","Computer","Construction Component","Detector Components",
"Display","Explosives","Girder","Gravity Generator Components","Interior Plate",
"Large Steel Tube","Medical Components","Metal Grid","Motor","Power Cell",
"Radio-communication Components","Reactor Components","Small Steel Tube","Solar Cell","Steel Plate",
"Superconductor Conduits","Thruster Components","200 mm missle container","25x184mm NATO ammo container","NATO_5p56x45mm",
"Hydrogen bottle","Oxygen bottle","Automatic Rifle","Precise Automatic Rifle","Rapid-fire Automatic Rifle",
"Elite Automatic Rifle","Grinder","Enhanced Grinder","Proficient Grinder","Elite Grinder",
"Hand Drill","Enhanced Hand Drill","Proficient Hand Drill","Elite Hand Drill","Welder",
"Enhanced Welder","Proficient Welder","Elite Welder",
"Ag  Silver","Au  Gold","Co  Cobalt","Fe  Iron","Mg  Magnesium",
"Ni  Nickel","Pt  Platinum","Si  Silicon","Ur  Uranium","Stone","H2O Ice","Metal Scrap",
"   Assemblers Queue>_\n","Components","Ammo and Tools","   Resources   Ingot ∙∙∙ Ore\n","   Items from mods and updates\n",
"Quotas to default","Quotas to zero","Sended","• Grid List Requested ..","Sending docking request ..",
"Sending unloading request ..","Err.Unknown Run parameter","Err.Check the Main Assembler","Quotas updated",
"∙Lost ","∙Damaged ","\n Hardware Status: ","\n Malfunctions detected:","Devices updated","Radio message:\n ",
"Warn.No free connectors"," AQM.Status is : ","\n Cargo loaded at "," > Queue has unknown items <\n"," > Cargo has unknown items <\n", "\n Blocks with inventory "};
                    break;
            }
        }
    }
}
