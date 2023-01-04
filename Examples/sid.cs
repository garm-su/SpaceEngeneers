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
namespace SpaceEngineers.UWBlockPrograms.BatteryMonitor
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion


        public Program()
        {
        }

        public void Main(string args)
        {

        }


        #region Limits
        static float limOre = 0;

        static float limOreScr = limOre;
        static float limOreFe = limOre;
        static float limOreNi = limOre;
        static float limOreCo = limOre;
        static float limOreSi = limOre;
        static float limOreAg = limOre;
        static float limOreAu = limOre;
        static float limOrePt = limOre;
        static float limOreMg = limOre;
        static float limOreU = limOre;
        static float limOreStn = limOre;
        static float limOreIce = 1000;


        static float limIng = 5000;

        static float limIngFe = 20000;
        static float limIngNi = limIng;
        static float limIngCo = limIng;
        static float limIngSi = limIng;
        static float limIngAg = 500;
        static float limIngAu = 250;
        static float limIngPt = 100;
        static float limIngMg = 500;
        static float limIngU = 50;
        static float limIngStn = limIng;

        static float limO2 = 0;
        static float limH2 = 0;


        static int limCmpSP = 0;
        static int limCmpIP = 0;
        static int limCmpCC = 0;
        static int limCmpMG = 0;
        static int limCmpSST = 0;
        static int limCmpGrd = 0;
        static int limCmpLST = 0;
        static int limCmpGls = 0;
        static int limCmpDsp = 0;
        static int limCmpCmp = 0;
        static int limCmpMtr = 0;

        static int limCmpO2 = 0;
        static int limCmpH2 = 0;
        static int limCmpCnv = 0;
        static int limCmpDtpd = 0;

        static int limCmpRad = 0;
        static int limCmpDet = 0;
        static int limCmpMed = 0;
        static int limCmpThr = 0;
        static int limCmpRct = 0;
        static int limCmpPwr = 0;
        static int limCmpSol = 0;
        static int limCmpSup = 0;
        static int limCmpGrv = 0;
        static int limCmpExp = 0;

        static int limCmpBul = 0;
        static int limCmpGat = 0;
        static int limCmpMis = 0;

        static int limCmpW1 = 0;
        static int limCmpW2 = 0;
        static int limCmpW3 = 0;
        static int limCmpW4 = 0;
        static int limCmpG1 = 0;
        static int limCmpG2 = 0;
        static int limCmpG3 = 0;
        static int limCmpG4 = 0;
        static int limCmpD1 = 0;
        static int limCmpD2 = 0;
        static int limCmpD3 = 0;
        static int limCmpD4 = 0;
        static int limCmpR1 = 0;
        static int limCmpR2 = 0;
        static int limCmpR3 = 0;
        static int limCmpR4 = 0;
        #endregion

        #region Colors
        static float lerp(float x0, float x1, float t)
        {
            return x0 + (x1 - x0) * t;
        }

        static Color lerp(Color c0, Color c1, float t)
        {
            return new Color(
                (int)lerp(c0.R, c1.R, t),
                (int)lerp(c0.G, c1.G, t),
                (int)lerp(c0.B, c1.B, t),
                (int)lerp(c0.A, c1.A, t));
        }

        static Color colBack = new Color(0x00, 0x00, 0x00);
        static Color colOff = new Color(0x22, 0x22, 0x22);
        static Color colNone = new Color(0x88, 0x33, 0x33);
        static Color colVal = new Color(0xaa, 0xaa, 0xaa);
        static Color colBar = colVal.Alpha(0.04f);
        static Color colOut = colNone.Alpha(0.1f);
        static Color colF1 = lerp(colBar, colOut, 1 / 3f);
        static Color colF2 = lerp(colBar, colOut, 2 / 3f);
        static Color colBox = colVal.Alpha(0.003f);
        static Color colTitle = new Color(0x88, 0x66, 0x22);
        static Color colIce = new Color(0x00, 0x33, 0x66);
        static Color colRef = new Color(0x66, 0x66, 0x00);
        static Color colAsm = new Color(0x00, 0x66, 0x00);
        static Color colBld = new Color(0x99, 0x33, 0x00);
        static Color colQue = new Color(0x0c, 0x33, 0x0c);
        #endregion


        #region Ore
        class Ore
        {
            public string ShortName;
            public string FullName;
            public string AltName;

            public MyItemType OreType;
            public MyItemType IngotType;

            public float OreAmount;
            public float IngotAmount;

            public float LastOreAmount;
            public float LastIngotAmount;
            public float LastAmountInAsm;

            public float COV;
            public float MOV;

            public float CIV;
            public float MIV;

            public bool N;


            public Ore(string shortName, string fullName)
            {
                ShortName = shortName;
                FullName = fullName;
                AltName = "";

                OreType = MyItemType.MakeOre(FullName);
                IngotType = MyItemType.MakeIngot(FullName);

                OreAmount = 0;
                IngotAmount = 0;

                LastOreAmount = float.NaN;
                LastIngotAmount = float.NaN;
                LastAmountInAsm = float.NaN;

                COV = 0;
                MOV = 0;

                CIV = 0;
                MIV = 0;

                N = false;
            }
        }

        static Ore oreScr = new Ore("Scr", "Scrap");
        static Ore oreFe = new Ore("Fe", "Iron");
        static Ore oreNi = new Ore("Ni", "Nickel");
        static Ore oreCo = new Ore("Co", "Cobalt");
        static Ore oreSi = new Ore("Si", "Silicon");
        static Ore oreAg = new Ore("Ag", "Silver");
        static Ore oreAu = new Ore("Au", "Gold");
        static Ore orePt = new Ore("Pt", "Platinum");
        static Ore oreMg = new Ore("Mg", "Magnesium");
        static Ore oreU = new Ore("U", "Uranium");
        static Ore oreStn = new Ore("Stn", "Stone");
        static Ore oreIce = new Ore("Ice", "Ice");


        static List<Ore> Ores = new List<Ore> { oreScr, oreFe, oreNi, oreCo, oreSi, oreAg, oreAu, orePt, oreMg, oreU, oreStn, oreIce };
        #endregion

        #region Gases
        class Gas
        {
            public string ShortName;
            public string FullName;

            public float Amount;
            public float LastAmount;
            public bool HasTanks;

            public float Capacity;

            public Gas(string shortName, string fullName)
            {
                ShortName = shortName;
                FullName = fullName;
                Amount = 0;
                LastAmount = float.NaN;
                HasTanks = false;
                Capacity = 0;
            }
        }

        static Gas gasO2 = new Gas("O2", "Oxygen");
        static Gas gasH2 = new Gas("H2", "Hydrogen");

        static List<Gas> Gases = new List<Gas> { gasO2, gasH2 };
        #endregion

        #region Compsonents
        class Comp
        {
            public string ShortName;
            public string FullName;
            public string TypeName;
            public string AltName;

            public MyItemType Type;

            public int Amount;
            public int LastAmount;

            public float CV;
            public float MV;

            public List<Ingr> R;

            public static Comp FromSubtype(string subtype)
            {
                if (cmpSP.HasName(subtype)) return cmpSP;
                else if (cmpIP.HasName(subtype)) return cmpIP;
                else if (cmpCC.HasName(subtype)) return cmpCC;
                else if (cmpMG.HasName(subtype)) return cmpMG;
                else if (cmpSST.HasName(subtype)) return cmpSST;
                else if (cmpGrd.HasName(subtype)) return cmpGrd;
                else if (cmpLST.HasName(subtype)) return cmpLST;
                else if (cmpGls.HasName(subtype)) return cmpGls;
                else if (cmpDsp.HasName(subtype)) return cmpDsp;
                else if (cmpCmp.HasName(subtype)) return cmpCmp;
                else if (cmpMtr.HasName(subtype)) return cmpMtr;
                else if (cmpDtpd.HasName(subtype)) return cmpDtpd;
                else if (cmpO2.HasName(subtype)) return cmpO2;
                else if (cmpH2.HasName(subtype)) return cmpH2;
                else if (cmpRad.HasName(subtype)) return cmpRad;
                else if (cmpDet.HasName(subtype)) return cmpDet;
                else if (cmpMed.HasName(subtype)) return cmpMed;
                else if (cmpThr.HasName(subtype)) return cmpThr;
                else if (cmpRct.HasName(subtype)) return cmpRct;
                else if (cmpPwr.HasName(subtype)) return cmpPwr;
                else if (cmpSol.HasName(subtype)) return cmpSol;
                else if (cmpSup.HasName(subtype)) return cmpSup;
                else if (cmpGrv.HasName(subtype)) return cmpGrv;
                else if (cmpExp.HasName(subtype)) return cmpExp;
                else if (cmpCnv.HasName(subtype)) return cmpCnv;
                else if (cmpBul.HasName(subtype)) return cmpBul;
                else if (cmpGat.HasName(subtype)) return cmpGat;
                else if (cmpMis.HasName(subtype)) return cmpMis;
                else if (cmpW1.HasName(subtype)) return cmpW1;
                else if (cmpW2.HasName(subtype)) return cmpW2;
                else if (cmpW3.HasName(subtype)) return cmpW3;
                else if (cmpW4.HasName(subtype)) return cmpW4;
                else if (cmpG1.HasName(subtype)) return cmpG1;
                else if (cmpG2.HasName(subtype)) return cmpG2;
                else if (cmpG3.HasName(subtype)) return cmpG3;
                else if (cmpG4.HasName(subtype)) return cmpG4;
                else if (cmpD1.HasName(subtype)) return cmpD1;
                else if (cmpD2.HasName(subtype)) return cmpD2;
                else if (cmpD3.HasName(subtype)) return cmpD3;
                else if (cmpD4.HasName(subtype)) return cmpD4;
                else if (cmpR1.HasName(subtype)) return cmpR1;
                else if (cmpR2.HasName(subtype)) return cmpR2;
                else if (cmpR3.HasName(subtype)) return cmpR3;
                else if (cmpR4.HasName(subtype)) return cmpR4;
                else return null;
            }

            public Comp(string shortName, string fullName, string typeName, List<Ingr> recipe, CompType cmpType = CompType.Comp) : this(shortName, fullName, typeName, "", recipe, cmpType) { }
            public Comp(string shortName, string fullName, string typeName, string altName, List<Ingr> recipe, CompType cmpType = CompType.Comp, bool altNameIsType = false)
            {
                ShortName = shortName;
                FullName = fullName;
                TypeName = typeName;
                AltName = altName;

                Type =
                    altNameIsType
                    ? new MyItemType(AltName, TypeName)
                    : CreateType(cmpType);

                Amount = 0;
                LastAmount = -1;

                R = recipe;
            }

            MyItemType CreateType(CompType cmpType)
            {
                if (cmpType == CompType.Ammo) return MyItemType.MakeAmmo(TypeName);
                else if (cmpType == CompType.Tool) return MyItemType.MakeTool(TypeName);
                else return MyItemType.MakeComponent(TypeName);
            }

            public bool HasName(string name)
            {
                return
                       TypeName == name
                    || AltName == name;
            }
        }
        class Ingr
        {
            public Ore Ore;
            public float Amount;

            public Ingr(Ore ore, float amount)
            {
                Ore = ore;
                Amount = amount;
            }
        }


        static Comp cmpSP = new Comp("SP", "Steel Plates", "SteelPlate", new List<Ingr> { new Ingr(oreFe, 7) });
        static Comp cmpIP = new Comp("IP", "Interior Plt.", "InteriorPlate", new List<Ingr> { new Ingr(oreFe, 1) });
        static Comp cmpCC = new Comp("CC", "Construction", "Construction", "ConstructionComponent", new List<Ingr> { new Ingr(oreFe, 8 / 3f) });
        static Comp cmpMG = new Comp("MG", "Metal Grids", "MetalGrid", new List<Ingr> { new Ingr(oreFe,   4     ),
    new Ingr(oreNi,   5/  3f),
    new Ingr(oreCo,   1     ) });
        static Comp cmpSST = new Comp("SST", "Small Tubes", "SmallTube", new List<Ingr> { new Ingr(oreFe, 5 / 3f) });
        static Comp cmpGrd = new Comp("Grd", "Girders", "Girder", "GirderComponent", new List<Ingr> { new Ingr(oreFe, 2) });
        static Comp cmpLST = new Comp("LST", "Large Tubes", "LargeTube", new List<Ingr> { new Ingr(oreFe, 10) });
        static Comp cmpGls = new Comp("Gls", "Glass", "BulletproofGlass", new List<Ingr> { new Ingr(oreSi, 5) });
        static Comp cmpDsp = new Comp("Dsp", "Displays", "Display", new List<Ingr> { new Ingr(oreFe,   1/  3f),
    new Ingr(oreSi,   5/  3f) });
        static Comp cmpCmp = new Comp("Cmp", "Computers", "Computer", "ComputerComponent", new List<Ingr> { new Ingr(oreFe,   1/  6f),
    new Ingr(oreSi,   1/ 15f) });
        static Comp cmpMtr = new Comp("Mtr", "Motors", "Motor", "MotorComponent", new List<Ingr> { new Ingr(oreFe,  20/  3f),
    new Ingr(oreNi,   5/  3f) });

        static Comp cmpO2 = new Comp("O2", "O2 Bottles", "OxygenBottle", "MyObjectBuilder_OxygenContainerObject", new List<Ingr> { new Ingr(oreFe,  80/  3f),
    new Ingr(oreSi,  10/  3f),
    new Ingr(oreNi,  10     ) }, altNameIsType: true);
        static Comp cmpH2 = new Comp("H2", "H2 Bottles", "HydrogenBottle", "MyObjectBuilder_GasContainerObject", new List<Ingr> { new Ingr(oreFe,  80/  3f),
    new Ingr(oreSi,  10/  3f),
    new Ingr(oreNi,  10     ) }, altNameIsType: true);
        static Comp cmpCnv = new Comp("Cnv", "Canvases", "Canvas", new List<Ingr> { new Ingr(oreSi,  35/  3f),
    new Ingr(oreFe,   2/  3f) });
        static Comp cmpDtpd = new Comp("Dtpd", "Datapads", "Datapad", "MyObjectBuilder_Datapad", new List<Ingr> { new Ingr(oreFe,   1/  3f),
    new Ingr(oreSi,   5/  3f),
    new Ingr(oreStn,  1/  3f) }, altNameIsType: true);

        static Comp cmpRad = new Comp("Rad", "Radio", "RadioCommunication", "RadioCommunicationComponent", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreSi,   1/  3f) });
        static Comp cmpDet = new Comp("Det", "Detector", "Detector", "DetectorComponent", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreNi,   5     ) });
        static Comp cmpMed = new Comp("Med", "Medical", "Medical", "MedicalComponent", new List<Ingr> { new Ingr(oreFe,  20     ),
    new Ingr(oreNi,  70/  3f),
    new Ingr(oreAg,  20/  3f) });
        static Comp cmpThr = new Comp("Thr", "Thrust", "Thrust", "ThrustComponent", new List<Ingr> { new Ingr(oreFe,  10     ),
    new Ingr(oreCo,  10/  3f),
    new Ingr(oreAu,   1/  3f),
    new Ingr(orePt,   2/ 15f) });
        static Comp cmpRct = new Comp("Rct", "Reactor", "Reactor", "ReactorComponent", new List<Ingr> { new Ingr(oreFe,   5     ),
    new Ingr(oreStn, 20/  3f),
    new Ingr(oreAg,   5/  3f) });
        static Comp cmpPwr = new Comp("Pwr", "Power Cells", "PowerCell", new List<Ingr> { new Ingr(oreFe,  10/  3f),
    new Ingr(oreSi,   1/  3f),
    new Ingr(oreNi,   2/  3f) });
        static Comp cmpSol = new Comp("Sol", "Solar Cells", "SolarCell", new List<Ingr> { new Ingr(oreNi,   1     ),
    new Ingr(oreSi,   2     ) });
        static Comp cmpSup = new Comp("Sup", "Supercond.", "Superconductor", new List<Ingr> { new Ingr(oreFe,  10/  3f),
    new Ingr(oreAu,   2/  3f) });
        static Comp cmpGrv = new Comp("Grv", "Gravity", "GravityGenerator", "GravityGeneratorComponent", new List<Ingr> { new Ingr(oreAg,   2/  3f),
    new Ingr(oreAu,  10/  3f),
    new Ingr(oreCo, 220/  3f),
    new Ingr(oreFe, 200     ) });
        static Comp cmpExp = new Comp("Exp", "Explosives", "Explosives", "ExplosivesComponent", new List<Ingr> { new Ingr(oreSi,   1/  6f),
    new Ingr(oreMg,   2/  3f) });

        static Comp cmpBul = new Comp("Bul", "Small Ammo", "NATO_5p56x45mm", "NATO_5p56x45mmMagazine", new List<Ingr> { new Ingr(oreFe,   4/ 15f),
    new Ingr(oreNi,   2/ 30f),
    new Ingr(oreMg,   1/ 20f) }, CompType.Ammo);
        static Comp cmpGat = new Comp("Gat", "Large Ammo", "NATO_25x184mm", "NATO_25x184mmMagazine", new List<Ingr> { new Ingr(oreFe,  40/  3f),
    new Ingr(oreNi,   5/  3f),
    new Ingr(oreMg,   1     ) }, CompType.Ammo);
        static Comp cmpMis = new Comp("Mis", "Missiles", "Missile200mm", new List<Ingr> { new Ingr(oreFe,  55/  3f),
    new Ingr(oreNi,   7/  3f),
    new Ingr(oreSi,   2/ 30f),
    new Ingr(oreU,    1/ 30f),
    new Ingr(orePt,   1/100f),
    new Ingr(oreMg,   2/  5f) }, CompType.Ammo);

        static Comp cmpW1 = new Comp("W", "Welders", "WelderItem", "Welder", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreStn,  1     ) }, CompType.Tool);
        static Comp cmpW2 = new Comp("W\uE030", "   W\uE030", "Welder2Item", "Welder2", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   2/ 30f),
    new Ingr(oreSi,   2/  3f) }, CompType.Tool);
        static Comp cmpW3 = new Comp("W\uE031", "   W\uE031", "Welder3Item", "Welder3", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   2/ 30f),
    new Ingr(oreAg,   2/  3f) }, CompType.Tool);
        static Comp cmpW4 = new Comp("W\uE032", "   W\uE032", "Welder4Item", "Welder4", new List<Ingr> { new Ingr(oreFe,   5/  3f),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   2/ 30f),
    new Ingr(orePt,   2/  3f) }, CompType.Tool);
        static Comp cmpG1 = new Comp("G", "Grinder", "AngleGrinderItem", "AngleGrinder", new List<Ingr> { new Ingr(oreFe,   1     ),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreStn,  5/  3f),
    new Ingr(oreSi,   1/  3f) }, CompType.Tool);
        static Comp cmpG2 = new Comp("G\uE030", "   G\uE030", "AngleGrinder2Item", "AngleGrinder2", new List<Ingr> { new Ingr(oreFe,   1     ),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   2/  3f),
    new Ingr(oreSi,   2     ) }, CompType.Tool);
        static Comp cmpG3 = new Comp("G\uE031", "   G\uE031", "AngleGrinder3Item", "AngleGrinder3", new List<Ingr> { new Ingr(oreFe,   1     ),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   1/  3f),
    new Ingr(oreSi,   2/  3f),
    new Ingr(oreAg,   2/  3f) }, CompType.Tool);
        static Comp cmpG4 = new Comp("G\uE032", "   G\uE032", "AngleGrinder4Item", "AngleGrinder4", new List<Ingr> { new Ingr(oreFe,   1     ),
    new Ingr(oreNi,   1/  3f),
    new Ingr(oreCo,   1/  3f),
    new Ingr(oreSi,   2/  3f),
    new Ingr(orePt,   2/  3f) }, CompType.Tool);
        static Comp cmpD1 = new Comp("D", " Drills", "HandDrillItem", "HandDrill", new List<Ingr> { new Ingr(oreFe,  20/  3f),
    new Ingr(oreNi,   1     ),
    new Ingr(oreSi,   1     ) }, CompType.Tool);
        static Comp cmpD2 = new Comp("D\uE030", "   D\uE030", "HandDrill2Item", "HandDrill2", new List<Ingr> { new Ingr(oreFe,  20/  3f),
    new Ingr(oreNi,   1     ),
    new Ingr(oreSi,   5/  3f) }, CompType.Tool);
        static Comp cmpD3 = new Comp("D\uE031", "   D\uE031", "HandDrill3Item", "HandDrill3", new List<Ingr> { new Ingr(oreFe,  20/  3f),
    new Ingr(oreNi,   1     ),
    new Ingr(oreSi,   1     ),
    new Ingr(oreAg,   2/  3f) }, CompType.Tool);
        static Comp cmpD4 = new Comp("D\uE032", "   D\uE032", "HandDrill4Item", "HandDrill4", new List<Ingr> { new Ingr(oreFe,  20/  3f),
                                                                                                                                                    new Ingr(oreNi,   1     ),
                                                                                                                                                    new Ingr(oreSi,   1     ),
                                                                                                                                                    new Ingr(orePt,   2/  3f) }, CompType.Tool);
        static Comp cmpR1 = new Comp("R", " Rifles", "AutomaticRifleItem", "AutomaticRifle", new List<Ingr> { new Ingr(oreFe,   1     ),
                                                                                                                                                    new Ingr(oreNi,   1/  3f) }, CompType.Tool);
        static Comp cmpR2 = new Comp("R\uE030", "   R\uE030", "RapidFireAutomaticRifleItem", "RapidFireAutomaticRifle", new List<Ingr> { new Ingr(oreFe,   1     ),
                                                                                                                                                    new Ingr(oreNi,   8/  3f) }, CompType.Tool);
        static Comp cmpR3 = new Comp("R\uE031", "   R\uE031", "PreciseAutomaticRifleItem", "PreciseAutomaticRifle", new List<Ingr> { new Ingr(oreFe,   1     ),
                                                                                                                                                    new Ingr(oreNi,   1/  3f),
                                                                                                                                                    new Ingr(oreCo,   5/  3f) }, CompType.Tool);
        static Comp cmpR4 = new Comp("R\uE032", "   R\uE032", "UltimateAutomaticRifleItem", "UltimateAutomaticRifle", new List<Ingr> { new Ingr(oreFe,   1     ),
                                                                                                                                                    new Ingr(oreNi,   1/  3f),
                                                                                                                                                    new Ingr(orePt,   4/  3f),
                                                                                                                                                    new Ingr(oreAg,   2     ) }, CompType.Tool);


        static List<Comp> Comps = new List<Comp> {
        cmpSP, cmpIP, cmpCC, cmpMG, cmpSST, cmpGrd, cmpLST, cmpGls, cmpDsp, cmpCmp, cmpMtr,
        cmpO2, cmpH2, cmpCnv, cmpDtpd,
        cmpRad, cmpDet, cmpMed, cmpThr, cmpRct, cmpPwr, cmpSol, cmpSup, cmpGrv, cmpExp,
        cmpBul, cmpGat, cmpMis,
        cmpW1, cmpW2, cmpW3, cmpW4, cmpG1, cmpG2, cmpG3, cmpG4, cmpD1, cmpD2, cmpD3, cmpD4, cmpR1, cmpR2, cmpR3, cmpR4 };
        #endregion


        #region Variables
        List<string> m_cmd;

        List<string> m_grids;
        List<GridShortcut> m_short;
        List<DS> m_ds;

        List<Display> m_dsp;


        float m_fs;

        static bool m_showCalibration = false;
        bool m_alive;
        int m_updateTick;

        static float m_brd = 4;
        static float m_step = 15;

        List<IMyInventory> m_inv;
        List<IMyAssembler> m_asm;
        List<List<MyProductionItem>> m_que;

        float m_TCOV;
        float m_TMOV;
        float m_TCIV;
        float m_TMIV;
        float m_TCCV;
        float m_TMCV;

        bool m_o;
        bool m_ic;
        bool m_c;
        bool m_s;
        bool m_b;

        int[] m_cpx;
        int[] m_scpx;
        float m_offOre;
        float m_offCmp1;
        float m_offCmp2;
        float m_offCmp3;


        enum CompType { Ore, Ingot, Comp, Ammo, Tool };
        #endregion

        #region Format text
        string printNoZero(double d, int dec)
        {
            return d.ToString((Math.Abs(d) < 1 ? "" : "0") + "." + new string('0', dec));
        }
        string printValue(double val, int dec, bool showZero, int pad)
        {
            if (showZero)
            {
                string format =
                      "0"
                    + (dec > 0 ? "." : "")
                    + new string('0', dec);

                return
                    val
                    .ToString(format)
                    .PadLeft(pad + dec + (dec > 0 ? 1 : 0));
            }
            else
            {
                return
                    printNoZero(val, dec)
                    .PadLeft(pad + dec + (dec > 0 ? 1 : 0));
            }
        }

        string formatOre(float amount, bool spaceAfter = false)
        {
            if (amount >= 1000000000) return printValue(amount / 1000000000, 1, true, 5) + " B";
            else if (amount >= 1000000) return printValue(amount / 1000000, 1, true, 5) + " M";
            else if (amount >= 1000) return printValue(amount / 1000, 1, true, 5) + " k";
            else return printValue(amount, 2, true, 5) + (spaceAfter ? " " : "");
        }
        #endregion

        #region Config
        int Cmd(string[] set, string cmd)
        {
            for (int i = 0; i < set.Length; i++)
            {
                if (Has(set[i], cmd))
                    return i;
            }

            return -1;
        }
        bool Has(string line, string cmd)
        {
            if (cmd[cmd.Length - 1] == ':')
            {
                var parts = line.Split(':');

                if (parts.Length == 2
                    && parts[0].Trim().ToLower() == cmd.Substring(0, cmd.Length - 1).ToLower()
                    && parts[1].Trim() == "")
                    return true;
            }

            if (line.Trim().ToLower() == cmd.ToLower())
                return true;

            return false;
        }
        bool Any(string line)
        {
            foreach (var cmd in m_cmd)
            {
                if (Has(line, cmd))
                    return true;
            }

            return false;
        }

        void LoadConfig()
        {
            var G = "Grid:";
            var Gs = "Grids:";
            var D = "Display:";
            var Ds = "Displays:";

            m_cmd = new List<string>() { G, Gs, D, Ds };


            var set = Me.CustomData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);


            var iG = Cmd(set, G);
            var iGs = Cmd(set, Gs);
            var iD = Cmd(set, D);
            var iDs = Cmd(set, Ds);


            m_offOre = 45;
            m_offCmp1 = 80;
            m_offCmp2 = 70;
            m_offCmp3 = 37;


            var _iG = iG >= 0 ? iG : iGs;
            var _iD = iD >= 0 ? iD : iDs;


            if (_iG >= 0)
            {
                for (int i = _iG + 1; i < set.Length; i++)
                {
                    if (Any(set[i]))
                        break;


                    var n = set[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (n.Length > 1)
                    {
                        m_short.Add(new GridShortcut(
                            n[1].Trim().Trim('"'),
                            n[0].Trim().Trim('"')));

                        m_grids.Add(n[1].Trim().Trim('"'));
                    }
                    else
                    {
                        var e = set[i].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                        if (e.Length > 1)
                        {
                            m_short.Add(new GridShortcut(
                                e[1].Trim().Trim('"'),
                                e[0].Trim().Trim('"')));
                        }
                        else
                        {
                            m_grids.Add(set[i].Trim().Trim('"'));
                        }
                    }
                }
            }


            if (_iD >= 0)
            {
                for (int i = _iD + 1; i < set.Length; i++)
                {
                    if (Any(set[i]))
                        break;


                    var ds = new DS("");


                    var parts = set[i].Split(new char[] { '@' }, StringSplitOptions.None);
                    var spec = set[i];


                    if (parts.Length > 1)
                    {
                        spec = parts[0].Trim().Trim('"');


                        var flags = parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        bool posCmd = false;

                        if (flags.Length > 0)
                        {
                            foreach (var flag in flags)
                                ParseDisplayFlag(ref ds, flag, ref posCmd);
                        }
                        else
                            ParseDisplayFlag(ref ds, parts[1], ref posCmd);
                    }


                    var args = spec.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (args.Length == 3)
                    {
                        ds.Grid = args[0].Trim().Trim('"');
                        ds.Name = args[1].Trim().Trim('"');
                        ds.Ind = int.Parse(args[2]);
                    }
                    else if (args.Length == 2)
                    {
                        int index;
                        if (int.TryParse(args[1], out index))
                        {
                            ds.Name = args[0].Trim().Trim('"');
                            ds.Ind = index;
                        }
                        else
                        {
                            ds.Grid = args[0].Trim().Trim('"');
                            ds.Name = args[1].Trim().Trim('"');
                        }
                    }
                    else if (args.Length == 1)
                    {
                        ds.Name = args[0].Trim().Trim('"');
                    }
                    else
                    {
                        ds.Name = spec.Trim().Trim('"');
                    }

                    if (ds.Name == "")
                        ds.Ind = 0;

                    m_ds.Add(ds);
                }
            }
        }


        void ParseDisplayFlag(ref DS ds, string flag, ref bool posCmd)
        {
            var fn = "fullnames";
            var noarr = "noarrows";
            var nobox = "noboxes";
            var bars = "bars";

            var nO = "noore";
            var nO_ = "noores";
            var nI = "noingots";
            var nIc = "noice";
            var nC = "nocomps";
            var nC_ = "nocomponents";
            var nIt = "noitems";
            var nA = "noammo";
            var nT = "notools";
            var nS = "nostatus";

            var O = "ore";
            var O_ = "ores";
            var I = "ingots";
            var Ic = "ice";
            var C = "comps";
            var C_ = "components";
            var It = "items";
            var A = "ammo";
            var T = "tools";
            var S = "status";

            flag = flag.Trim().ToLower();

            if (flag == fn) ds.SFN = true;
            else if (flag == noarr) ds.Arr = false;
            else if (flag == nobox) ds.Box = false;
            else if (flag == bars) ds.Bars = true;

            else if (flag == nO) ds.O = false;
            else if (flag == nO_) ds.O = false;
            else if (flag == nI) ds.I = false;
            else if (flag == nIc) ds.Ic = false;
            else if (flag == nC) ds.C = false;
            else if (flag == nC_) ds.C = false;
            else if (flag == nIt) ds.It = false;
            else if (flag == nA) ds.A = false;
            else if (flag == nT) ds.T = false;
            else if (flag == nS) ds.S = false;

            else if (flag == O) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.O = true; }
            else if (flag == O_) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.O = true; }
            else if (flag == I) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.I = true; }
            else if (flag == Ic) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.Ic = true; }
            else if (flag == C) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.C = true; }
            else if (flag == C_) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.C = true; }
            else if (flag == It) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.It = true; }
            else if (flag == A) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.A = true; }
            else if (flag == T) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.T = true; }
            else if (flag == S) { if (!posCmd) { ds.CSF(); posCmd = true; } ds.S = true; }

            else
            {
                float scale;
                if (float.TryParse(flag, out scale))
                    ds.Scale = scale;
            }
        }

        struct DS
        {
            public string Grid;
            public string Name;
            public int Ind;

            public float Scale;

            public bool SFN;
            public bool Arr;
            public bool Box;
            public bool Bars;

            public bool O;
            public bool I;
            public bool Ic;
            public bool C;
            public bool It;
            public bool A;
            public bool T;
            public bool S;

            public DS(string name, int ind = -1, float scale = 0, bool sfn = false, bool arr = true, bool box = true, bool bars = false)
            {
                Grid = "";
                Name = name;
                Ind = ind;

                Scale = scale;

                SFN = sfn;
                Arr = arr;
                Box = box;
                Bars = bars;

                O = true;
                I = true;
                Ic = true;
                C = true;
                It = true;
                A = true;
                T = true;
                S = true;
            }

            public void CSF()
            {
                O = false;
                I = false;
                Ic = false;
                C = false;
                It = false;
                A = false;
                T = false;
                S = false;
            }
        }
        struct GridShortcut
        {
            public string Grid;
            public string Shortcut;

            public GridShortcut(string name, string shrt)
            {
                Grid = name;
                Shortcut = shrt;
            }
        }
        #endregion

        #region Inventory
        float GTA(MyItemType cmpType)
        {
            MyFixedPoint total = 0;

            foreach (var inv in m_inv)
                total += inv.GetItemAmount(cmpType);

            return (float)total;
        }

        bool Lacks(Ore ore, float amount)
        {
            if (m_asm.Count != m_que.Count)
                return false;


            for (int i = 0; i < m_que.Count; i++)
            {
                var needs = 0f;

                foreach (var item in m_que[i])
                {
                    var cmp = Comp.FromSubtype(item.BlueprintId.SubtypeName); if (cmp == null) continue;
                    var ing = cmp.R.Find(c => c.Ore == ore); if (ing != null) needs += ing.Amount; // ing is either ingots or ingredient here, both make sense, and I love it! :)
                }


                var has = 0f;

                for (int j = 0; j < m_asm[i].InventoryCount; j++)
                    has += (float)m_asm[i].GetInventory(j).GetItemAmount(ore.IngotType);


                if (needs > has)
                    return true;
            }


            return false;
        }
        bool IsNeededRightNow(Ore ore)
        {
            if (m_asm.Count != m_que.Count)
                return false;


            for (int i = 0; i < m_que.Count; i++)
            {
                var next = m_que[i].Find(item => item.ItemId == m_asm[i].NextItemId);

                var cmp = Comp.FromSubtype(next.BlueprintId.SubtypeName);
                if (cmp == null) continue;

                if (cmp.R.Exists(ing => ing.Ore == ore))
                    return true;
            }


            return false;
        }
        bool IsContentGrid(IMyCubeGrid grid)
        {
            return
                   grid == Me.CubeGrid
                || m_grids.Exists(g => g == grid.CustomName);
        }
        #endregion


        #region Updates
        void UpdateDisplays()
        {
            var ts = new List<IMyTextSurface>();
            GridTerminalSystem.GetBlocksOfType(ts);
            foreach (var s in ts) ResetSurface(s);

            var tsp = new List<IMyTextSurfaceProvider>();
            GridTerminalSystem.GetBlocksOfType(tsp);
            foreach (var sp in tsp)
            {
                for (int i = 0; i < sp.SurfaceCount; i++)
                    ResetSurface(sp.GetSurface(i));
            }


            if (m_ds.Count == 0)
                m_dsp.Add(new Display(Me, new DS(""), Me.EntityId));

            foreach (var set in m_ds)
            {
                if (set.Ind >= 0)
                {
                    if (set.Name == "")
                    {
                        if (set.Ind < Me.SurfaceCount)
                            m_dsp.Add(new Display(Me, set, Me.EntityId));
                    }
                    else
                    {
                        var blocks = GDStr(set);

                        foreach (var b in blocks)
                        {
                            if (b is IMyTextSurfaceProvider)
                            {
                                if (set.Grid == ""
                                    && b.CubeGrid == Me.CubeGrid)
                                {
                                    if (set.Ind < ((IMyTextSurfaceProvider)b).SurfaceCount)
                                        m_dsp.Add(new Display((IMyTextSurfaceProvider)b, set, Me.EntityId));
                                }
                                else if (b.CubeGrid.CustomName.ToLower() == set.Grid.ToLower())
                                {
                                    if (set.Ind < ((IMyTextSurfaceProvider)b).SurfaceCount)
                                        m_dsp.Add(new Display((IMyTextSurfaceProvider)b, set, Me.EntityId));
                                }
                                else
                                {
                                    var gridName =
                                        set.Grid != ""
                                        ? set.Grid
                                        : Me.CubeGrid.CustomName;

                                    var i = m_short.FindIndex(sh => sh.Shortcut.ToLower() == gridName.ToLower());
                                    if (i >= 0 && b.CubeGrid.CustomName.ToLower() == m_short[i].Grid.ToLower())
                                    {
                                        if (set.Ind < ((IMyTextSurfaceProvider)b).SurfaceCount)
                                            m_dsp.Add(new Display((IMyTextSurfaceProvider)b, set, Me.EntityId));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (set.Name == "")
                {
                    m_dsp.Add(new Display(Me, new DS(""), Me.EntityId));
                }
                else
                {
                    var blocks = GDStr(set);

                    foreach (var b in blocks)
                    {
                        if (b is IMyTextSurface)
                        {
                            if (set.Grid == ""
                                && b.CubeGrid == Me.CubeGrid)
                            {
                                m_dsp.Add(new Display((IMyTextSurface)b, set, Me.EntityId));
                            }
                            else if (b.CubeGrid.CustomName.ToLower() == set.Grid.ToLower())
                            {
                                m_dsp.Add(new Display((IMyTextSurface)b, set, Me.EntityId));
                            }
                            else
                            {
                                var gridName =
                                    set.Grid != ""
                                    ? set.Grid
                                    : Me.CubeGrid.CustomName;

                                var i = m_short.FindIndex(sh => sh.Shortcut.ToLower() == gridName.ToLower());
                                if (i >= 0 && b.CubeGrid.CustomName.ToLower() == m_short[i].Grid.ToLower())
                                {
                                    m_dsp.Add(new Display((IMyTextSurface)b, set, Me.EntityId));
                                }
                            }
                        }
                    }
                }
            }


            m_o = false;
            m_ic = false;
            m_c = false;
            m_s = false;
            m_b = false;

            foreach (var d in m_dsp)
            {
                if (d.Set.O || d.Set.I) m_o = true;
                if (d.Set.Ic) m_ic = true;
                if (d.Set.C || d.Set.It || d.Set.A || d.Set.T) m_c = true;
                if (d.Set.S) m_s = true;
                if (d.Set.Bars) m_b = true;
            }
        }
        void ResetSurface(IMyTextSurface s)
        {
            var sb = new StringBuilder();
            s.ReadText(sb, false);

            var parts = sb.ToString().Split('/');
            if (parts.Length == 2)
            {
                long eid;
                if (long.TryParse(parts[0].ToString(), out eid) && eid == Me.EntityId)
                {
                    ContentType ct = ContentType.NONE;

                    int i;
                    if (int.TryParse(parts[1].ToString(), out i))
                        ct = (ContentType)i;

                    s.WriteText("", false);
                    s.ContentType = ct;
                }
            }
        }
        List<IMyCubeBlock> GDStr(DS set)
        {
            var blocks = new List<IMyCubeBlock>();

            var gridName = set.Grid;

            var i = m_short.FindIndex(sh => sh.Shortcut.ToLower() == gridName.ToLower());
            if (i >= 0) gridName = m_short[i].Grid;

            GridTerminalSystem.GetBlocksOfType(blocks, b =>
                      gridName == ""
                   && b.DisplayNameText.ToLower() == set.Name.ToLower()
                || b.CubeGrid.CustomName.ToLower() == gridName.ToLower()
                   && b.DisplayNameText.ToLower() == set.Name.ToLower());

            return blocks;
        }
        void UpdateO2()
        {
            gasO2.Amount = 0;
            gasO2.Capacity = 0;
            gasO2.HasTanks = false;

            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(null, t =>
            {
                if (IsContentGrid(t.CubeGrid))
                {
                    MyResourceSinkComponent sink;

                    t.Components.TryGet<MyResourceSinkComponent>(out sink);
                    var list = sink.AcceptedResources;

                    foreach (var item in list)
                    {
                        if (item.SubtypeId.ToString() == "Oxygen")
                        {
                            gasO2.Amount += (float)(t.Capacity * t.FilledRatio);
                            gasO2.Capacity += (float)t.Capacity;
                            gasO2.HasTanks = true;
                        }
                    }
                }

                return false;
            });
        }
        void UpdateH2()
        {
            gasH2.Amount = 0;
            gasH2.Capacity = 0;
            gasH2.HasTanks = false;

            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(null, t =>
            {
                if (IsContentGrid(t.CubeGrid))
                {
                    MyResourceSinkComponent sink;

                    t.Components.TryGet<MyResourceSinkComponent>(out sink);
                    var list = sink.AcceptedResources;

                    foreach (var item in list)
                    {
                        if (item.SubtypeId.ToString() == "Hydrogen")
                        {
                            gasH2.Amount += (float)(t.Capacity * t.FilledRatio);
                            gasH2.Capacity += (float)t.Capacity;
                            gasH2.HasTanks = true;
                        }
                    }
                }

                return false;
            });
        }
        void UpdateOre()
        {
            foreach (var o in Ores)
            {
                o.COV = 0;
                o.MOV = 0;
                o.CIV = 0;
                o.MIV = 0;
            }


            if (m_b)
            {
                m_TMOV = 0;
                m_TMIV = 0;

                var types = new List<MyItemType>();
                var items = new List<MyInventoryItem>();

                foreach (var inv in m_inv)
                {
                    types.Clear();
                    inv.GetAcceptedItems(types);

                    var od = false;
                    var id = false;

                    foreach (var o in Ores)
                    {
                        if (types.Exists(t => t == o.OreType))
                        {
                            if (!od) { m_TMOV += (float)inv.MaxVolume; od = true; }

                            items.Clear();
                            inv.GetItems(items, i => i.Type == o.OreType);
                            foreach (var i in items)
                                o.COV += (float)i.Type.GetItemInfo().Volume * (float)i.Amount;

                            o.MOV += (float)inv.MaxVolume;
                        }

                        if (types.Exists(t => t == o.IngotType))
                        {
                            if (!id) { m_TMIV += (float)inv.MaxVolume; id = true; }

                            items.Clear();
                            inv.GetItems(items, i => i.Type == o.IngotType);
                            foreach (var i in items)
                                o.CIV += (float)i.Type.GetItemInfo().Volume * (float)i.Amount;

                            o.MIV += (float)inv.MaxVolume;
                        }
                    }
                }
            }

            m_TCOV = 0;
            m_TCIV = 0;

            foreach (var o in Ores)
            {
                m_TCOV += o.COV;
                m_TCIV += o.CIV;

                o.OreAmount = GTA(o.OreType);
                if (float.IsNaN(o.LastOreAmount)) o.LastOreAmount = o.OreAmount;

                o.IngotAmount = GTA(o.IngotType);
                if (float.IsNaN(o.LastIngotAmount)) o.LastIngotAmount = o.IngotAmount;
            }
        }
        void UpdateComps()
        {
            foreach (var c in Comps)
            {
                c.CV = 0;
                c.MV = 0;
            }

            if (m_b)
            {
                m_TMCV = 0;

                var types = new List<MyItemType>();
                var items = new List<MyInventoryItem>();

                foreach (var inv in m_inv)
                {
                    types.Clear();
                    inv.GetAcceptedItems(types);

                    var cd = false;

                    foreach (var c in Comps)
                    {
                        if (types.Exists(t => t == c.Type))
                        {
                            if (!cd) { m_TMCV += (float)inv.MaxVolume; cd = true; }

                            items.Clear();
                            inv.GetItems(items, i => i.Type == c.Type);
                            foreach (var i in items)
                                c.CV += (float)i.Type.GetItemInfo().Volume * (float)i.Amount;

                            c.MV += (float)inv.MaxVolume;
                        }
                    }
                }
            }

            m_TCCV = 0;

            foreach (var c in Comps)
            {
                m_TCCV += c.CV;

                c.Amount = (int)GTA(c.Type);
                if (c.LastAmount < 0) c.LastAmount = c.Amount;
            }
        }
        void UpdateQueues()
        {
            float needFe = 0;
            float needNi = 0;
            float needCo = 0;
            float needSi = 0;
            float needAg = 0;
            float needAu = 0;
            float needPt = 0;
            float needMg = 0;
            float needU = 0;
            float needStn = 0;


            foreach (var asm in m_asm)
            {
                m_que.Add(new List<MyProductionItem>());
                var queue = m_que[m_que.Count - 1];

                if (asm.Mode == MyAssemblerMode.Assembly
                    && !asm.IsQueueEmpty)
                    asm.GetQueue(queue);

                foreach (var item in queue)
                {
                    var cmp = Comp.FromSubtype(item.BlueprintId.SubtypeName); if (cmp == null) continue;

                    var fe = cmp.R.Find(i => i.Ore == oreFe); if (fe != null) needFe += fe.Amount;
                    var ni = cmp.R.Find(i => i.Ore == oreNi); if (ni != null) needNi += ni.Amount;
                    var co = cmp.R.Find(i => i.Ore == oreCo); if (co != null) needCo += co.Amount;
                    var si = cmp.R.Find(i => i.Ore == oreSi); if (si != null) needSi += si.Amount;
                    var ag = cmp.R.Find(i => i.Ore == oreAg); if (ag != null) needAg += ag.Amount;
                    var au = cmp.R.Find(i => i.Ore == oreAu); if (au != null) needAu += au.Amount;
                    var pt = cmp.R.Find(i => i.Ore == orePt); if (pt != null) needPt += pt.Amount;
                    var mg = cmp.R.Find(i => i.Ore == oreMg); if (mg != null) needMg += mg.Amount;
                    var u = cmp.R.Find(i => i.Ore == oreU); if (u != null) needU += u.Amount;
                    var stn = cmp.R.Find(i => i.Ore == oreStn); if (stn != null) needStn += stn.Amount;
                }
            }


            oreFe.N = GTA(oreFe.IngotType) < needFe || Lacks(oreFe, needFe);
            oreNi.N = GTA(oreNi.IngotType) < needNi || Lacks(oreNi, needNi);
            oreCo.N = GTA(oreCo.IngotType) < needCo || Lacks(oreCo, needCo);
            oreSi.N = GTA(oreSi.IngotType) < needSi || Lacks(oreSi, needSi);
            oreAg.N = GTA(oreAg.IngotType) < needAg || Lacks(oreAg, needAg);
            oreAu.N = GTA(oreAu.IngotType) < needAu || Lacks(oreAu, needAu);
            orePt.N = GTA(orePt.IngotType) < needPt || Lacks(orePt, needPt);
            oreMg.N = GTA(oreMg.IngotType) < needMg || Lacks(oreMg, needMg);
            oreU.N = GTA(oreU.IngotType) < needU || Lacks(oreU, needU);
            oreStn.N = GTA(oreStn.IngotType) < needStn || Lacks(oreStn, needStn);
        }
        #endregion


        #region Rendering
        void RndOI(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            pnl.TS.Add(DStr("kg", m_brd + 10, m_brd + 1, m_fs * 2 / 3f, colRef));

            pnl.TS.Add(DStr("   ORE", m_brd + 42 + offOre, m_brd, m_fs, colRef));
            pnl.TS.Add(DStr("   INGOTS", m_brd + 103 + offOre, m_brd, m_fs, colRef));

            pnl.X = 0;
            pnl.Y = 0;

            var w = 187 + offOre;
            pnl.Width = w + m_brd * 2;

            var y = m_brd + 20;

            var line = 0;


            var bo = 0f;
            var bi = 0f;

            foreach (var o in Ores)
            {
                if (o.COV > bo) bo = o.COV;
                if (o.CIV > bi) bi = o.CIV;
            }


            RndS(pnl, m_brd, y + line++ * m_step, sfn, bo);
            RndOI(pnl, m_brd, y + line++ * m_step, oreFe, limOreFe, limIngFe, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreNi, limOreNi, limIngNi, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreCo, limOreCo, limIngCo, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreSi, limOreSi, limIngSi, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreAg, limOreAg, limIngAg, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreAu, limOreAu, limIngAu, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, orePt, limOrePt, limIngPt, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreMg, limOreMg, limIngMg, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreU, limOreU, limIngU, sfn, bo, bi);
            RndOI(pnl, m_brd, y + line++ * m_step, oreStn, limOreStn, limIngStn, sfn, bo, bi);

            pnl.Height = y + line * m_step + m_brd;

            pnl.DS.Add(FRct(m_brd, y, w, line * m_step, colBox));
        }
        void RndOI(DO pnl, float x, float y, Ore ore, double oreLimit, double ingLimit, bool sfn, float bo, float bi)
        {
            var notNeeded =
                   ore.N
                && ore.IngotAmount >= ore.LastIngotAmount;


            var movingIn = "►";
            var refining = "►";
            var movingOut = notNeeded ? "→" : "►";

            var strOreAmount = formatOre(ore.OreAmount);
            var strIngAmount = formatOre(ore.IngotAmount);


            var oreName =
                sfn
                ? ore.FullName
                : ore.ShortName;

            var offOre = sfn ? m_offOre : 0;
            var offIng = 74;

            var w = 58;
            var fs = 1.2f;
            var fo = Math.Min(ore.COV / ore.MOV * m_TCOV / bo * fs, 1);
            var cbo = fo > 0.99 ? colOut : (fo > 0.9f ? colF2 : (fo > 0.8f ? colF1 : colBar));
            var fi = ore.CIV / ore.MIV * m_TCIV / bi;
            var cbi = fi > 0.99 ? colOut : (fi > 0.9f ? colF2 : (fi > 0.8f ? colF1 : colBar));

            pnl.FS.Add(FRct(x + 45 + offOre, y, fo * w, 14, cbo));
            pnl.FS.Add(FRct(x + 120 + offOre, y, fi * w, 14, cbi));


            if (ore.OreAmount > ore.LastOreAmount) pnl.AS.Add(DStr(movingIn, x + 1, y, m_fs, colRef));
            pnl.TS.Add(DStr(oreName, x + 10, y, m_fs, ore.IngotAmount < ingLimit || ore.OreAmount < oreLimit ? colNone : colVal));
            pnl.TS.Add(DStr(strOreAmount, x + 30 + offOre, y, m_fs, ore.IngotAmount < ingLimit || ore.OreAmount < oreLimit ? colNone : colVal));
            if (ore.OreAmount < ore.LastOreAmount
                || ore.IngotAmount > ore.LastIngotAmount) pnl.AS.Add(DStr(refining, x + 30 + offIng + offOre, y, m_fs, colRef));
            pnl.TS.Add(DStr(strIngAmount, x + 30 + offIng + 1 + offOre, y, m_fs, ore.IngotAmount < ingLimit ? colNone : colVal));

            if (IsNeededRightNow(ore)
                    && ore.IngotAmount < ore.LastIngotAmount
                || ore.N
                || ore.IngotAmount < ore.LastIngotAmount)
                pnl.AS.Add(DStr(movingOut, x + 105 + offIng + offOre, y, m_fs, notNeeded ? colNone : colRef));
        }
        void RndO(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            pnl.TS.Add(DStr("kg", m_brd + 10, m_brd + 1, m_fs * 2 / 3f, colRef));

            pnl.TS.Add(DStr("   ORE", m_brd + 42 + offOre, m_brd, m_fs, colRef));

            pnl.X = 0;
            pnl.Y = 0;

            var w = 112 + offOre;
            pnl.Width = w + m_brd * 2;

            var y = m_brd + 20;

            var line = 0;

            var bo = 0f;
            foreach (var o in Ores)
                if (o.COV > bo) bo = o.COV;


            RndS(pnl, m_brd, y + line++ * m_step, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreFe, limOreFe, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreNi, limOreNi, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreCo, limOreCo, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreSi, limOreSi, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreAg, limOreAg, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreAu, limOreAu, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, orePt, limOrePt, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreMg, limOreMg, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreU, limOreU, sfn, bo);
            RndO(pnl, m_brd, y + line++ * m_step, oreStn, limOreStn, sfn, bo);

            pnl.Height = y + line * m_step + m_brd;

            pnl.DS.Add(FRct(m_brd, y, w, line * m_step, colBox));
        }
        void RndS(DO pnl, float x, float y, bool sfn, float bo)
        {
            if (float.IsNaN(oreScr.LastOreAmount)) oreScr.LastOreAmount = oreScr.OreAmount;

            var strValue = formatOre(oreScr.OreAmount);

            var scrName =
                sfn
                ? oreScr.FullName
                : oreScr.ShortName;

            var offOre = sfn ? m_offOre : 0;

            var w = 58;
            var sf = 1.2f;
            var f = Math.Min(oreScr.COV / oreScr.MOV * m_TCOV / bo * sf, 1);
            var ft = m_TCOV / m_TMOV;
            var cb = ft > 0.99 ? colOut : (ft > 0.9f ? colF2 : (ft > 0.8f ? colF1 : colBar));
            pnl.FS.Add(FRct(x + 45 + offOre, y, f * w, 14, cb));

            if (oreScr.OreAmount > oreScr.LastOreAmount) pnl.AS.Add(DStr("►", x + 1, y, m_fs, colRef));
            pnl.TS.Add(DStr(scrName, x + 10, y, m_fs, oreFe.IngotAmount < limIngFe ? colNone : colVal));
            pnl.TS.Add(DStr(strValue, x + 30 + offOre, y, m_fs, oreFe.IngotAmount < limIngFe ? colNone : colVal));
            if (oreScr.OreAmount < oreScr.LastOreAmount) pnl.AS.Add(DStr("►", x + 103 + offOre, y, m_fs, colRef));
        }
        void RndO(DO pnl, float x, float y, Ore ore, double oreLimit, bool sfn, float bo)
        {
            var notNeeded =
                   ore.N
                && ore.IngotAmount >= ore.LastIngotAmount;

            var movingIn = "►";
            var movingOut = notNeeded ? "→" : "►";

            var strOreAmount = formatOre(ore.OreAmount);

            var oreName =
                sfn
                ? ore.FullName
                : ore.ShortName;

            var offOre = sfn ? m_offOre : 0;

            var w = 58;
            var sf = 1.2f;
            var f = Math.Min(ore.COV / ore.MOV * m_TCOV / bo * sf, 1);
            var ft = m_TCOV / m_TMOV;
            var cb = ft > 0.85 ? colOut : (ft > 0.7f ? colF2 : (ft > 0.55f ? colF1 : colBar));
            pnl.FS.Add(FRct(x + 45 + offOre, y, f * w, 14, cb));

            if (ore.OreAmount > ore.LastOreAmount) pnl.AS.Add(DStr(movingIn, x + 1, y, m_fs, colRef));
            pnl.TS.Add(DStr(oreName, x + 10, y, m_fs, ore.OreAmount < oreLimit ? colNone : colVal));
            pnl.TS.Add(DStr(strOreAmount, x + 30 + offOre, y, m_fs, ore.OreAmount < oreLimit ? colNone : colVal));
            if (ore.OreAmount < ore.LastOreAmount
                || IsNeededRightNow(ore)
                   && ore.IngotAmount < ore.LastIngotAmount
                || ore.N
                || ore.IngotAmount < ore.LastIngotAmount) pnl.AS.Add(DStr(movingOut, x + 104 + offOre, y, m_fs, notNeeded ? colNone : colRef));
        }
        void RndI(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            pnl.TS.Add(DStr("kg", m_brd + 10, m_brd + 1, m_fs * 2 / 3f, colRef));

            pnl.TS.Add(DStr("   INGOTS", m_brd + 23 + offOre, m_brd, m_fs, colRef));

            pnl.X = 0;
            pnl.Y = 0;

            var w = 114 + offOre;
            pnl.Width = w + m_brd * 2;

            var y = m_brd + 20;

            var line = 1;

            var bi = 0f;

            foreach (var o in Ores)
                if (o.CIV > bi) bi = o.CIV;

            RndI(pnl, m_brd, y + line++ * m_step, oreFe, limIngFe, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreNi, limIngNi, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreCo, limIngCo, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreSi, limIngSi, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreAg, limIngAg, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreAu, limIngAu, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, orePt, limIngPt, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreMg, limIngMg, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreU, limIngU, sfn, bi);
            RndI(pnl, m_brd, y + line++ * m_step, oreStn, limIngStn, sfn, bi);

            pnl.Height = y + line * m_step + m_brd;

            pnl.DS.Add(FRct(m_brd, y, w, line * m_step, colBox));
        }
        void RndI(DO pnl, float x, float y, Ore ore, double ingLimit, bool sfn, float bi)
        {
            var notNeeded =
                   ore.N
                && ore.IngotAmount >= ore.LastIngotAmount;

            var refining = "►";
            var movingOut = notNeeded ? "→" : "►";

            var strIngAmount = formatOre(ore.IngotAmount);

            var oreName =
                sfn
                ? ore.FullName
                : ore.ShortName;

            var offOre = sfn ? m_offOre : 0;

            var w = 58;
            var f = ore.CIV / ore.MIV * m_TCIV / bi;
            var ft = m_TCIV / m_TMIV;
            var cb = ft > 0.99 ? colOut : (ft > 0.9f ? colF2 : (ft > 0.8f ? colF1 : colBar));
            pnl.FS.Add(FRct(x + 46 + offOre, y, f * w, 14, cb));

            if (ore.IngotAmount > ore.LastIngotAmount) pnl.AS.Add(DStr(refining, x + 1, y, m_fs, colRef));
            pnl.TS.Add(DStr(oreName, x + 10, y, m_fs, ore.IngotAmount < ingLimit ? colNone : colVal));
            pnl.TS.Add(DStr(strIngAmount, x + 30 + offOre + 1, y, m_fs, ore.IngotAmount < ingLimit ? colNone : colVal));
            if (IsNeededRightNow(ore)
                   && ore.IngotAmount < ore.LastIngotAmount
                || ore.N
                || ore.IngotAmount < ore.LastIngotAmount) pnl.AS.Add(DStr(movingOut, x + 106 + offOre, y, m_fs, notNeeded ? colNone : colRef));
        }

        void RndIcHO(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            var bo = 0f;
            foreach (var o in Ores)
                if (o.COV > bo) bo = o.COV;

            var bw = 58;
            var fs = 1.2f;
            var f = Math.Min(oreIce.COV / oreIce.MOV * m_TCOV / bo * fs, 1);
            var ft = m_TCOV / m_TMOV * fs;
            var cb = ft > 0.99 ? colOut : (ft > 0.9f ? colF2 : (ft > 0.8f ? colF1 : colBar));

            pnl.FS.Add(FRct(49 + offOre, 4, f * bw, 14, cb));

            var strValue = formatOre(oreIce.OreAmount);

            if (oreIce.OreAmount > oreIce.LastOreAmount) pnl.AS.Add(DStr("►", m_brd + 1, m_brd, m_fs, colIce));
            pnl.TS.Add(DStr(oreIce.ShortName, m_brd + 10, m_brd, m_fs, oreIce.OreAmount < limOreIce ? colNone : colVal));
            pnl.TS.Add(DStr(strValue, m_brd + 30 + offOre, m_brd, m_fs, oreIce.OreAmount < limOreIce ? colNone : colVal));
            if (oreIce.OreAmount < oreIce.LastOreAmount) pnl.AS.Add(DStr("►", m_brd + 104 + offOre, m_brd, m_fs, colIce));

            pnl.X = 0;
            pnl.Y = 0;

            var w = 187 + offOre;
            pnl.Width = w + m_brd * 2;

            var line = 2;

            var oName = sfn ? "Oxygen" : "O2";
            var hName = sfn ? "Hydrogen" : "H2";

            if (gasO2.HasTanks)
            {
                var fo = Math.Min(gasO2.Amount / gasO2.Capacity, 1);
                var cbo = fo > 0.99 ? colOut : (fo > 0.9f ? colF2 : (fo > 0.8f ? colF1 : colBar));

                pnl.FS.Add(FRct(118 + offOre, m_brd + line * m_step, fo * 65, 14, cbo));

                if (float.IsNaN(gasO2.LastAmount)) gasO2.LastAmount = gasO2.Amount;

                if (gasO2.Amount > gasO2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 78, m_brd + line * m_step, m_fs, colIce));
                pnl.TS.Add(DStr(oName, m_brd + 88, m_brd + line * m_step, m_fs, 0 < limO2 ? colNone : colVal));
                pnl.TS.Add(DStr(formatOre(gasO2.Amount, true) + "l", m_brd + 99 + offOre, m_brd + line * m_step, m_fs, 0 < limO2 ? colNone : colVal));
                if (gasO2.Amount < gasO2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 180 + offOre, m_brd + line++ * m_step, m_fs, colIce));
                else line++;
            }
            else line++;

            if (gasH2.HasTanks)
            {
                var fh = Math.Min(gasH2.Amount / gasH2.Capacity, 1);
                var cbh = fh > 0.99 ? colOut : (fh > 0.9f ? colF2 : (fh > 0.8f ? colF1 : colBar));

                pnl.FS.Add(FRct(118 + offOre, m_brd + line * m_step, fh * 65, 14, cbh));

                if (float.IsNaN(gasH2.LastAmount)) gasH2.LastAmount = gasH2.Amount;

                if (gasH2.Amount > gasH2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 78, m_brd + line * m_step, m_fs, colIce));
                pnl.TS.Add(DStr(hName, m_brd + 88, m_brd + line * m_step, m_fs, 0 < limH2 ? colNone : colVal));
                pnl.TS.Add(DStr(formatOre(gasH2.Amount, true) + "l", m_brd + 99 + offOre, m_brd + line * m_step, m_fs, 0 < limH2 ? colNone : colVal));
                if (gasH2.Amount < gasH2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 180 + offOre, m_brd + line++ * m_step, m_fs, colIce));
                else line++;
            }
            else line++;

            pnl.Height = line * m_step + m_brd * 2;

            pnl.DS.Add(FRct(m_brd, m_brd, w, line * m_step, colBox));
        }

        void RndIc(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            var bo = 0f;
            foreach (var o in Ores)
                if (o.COV > bo) bo = o.COV;

            var bw = 58;
            var fs = 1.2f;
            var f = Math.Min(oreIce.COV / oreIce.MOV * m_TCOV / bo * fs, 1);
            var ft = m_TCOV / m_TMOV * fs;
            var cb = ft > 0.99 ? colOut : (ft > 0.9f ? colF2 : (ft > 0.8f ? colF1 : colBar));

            pnl.FS.Add(FRct(49 + offOre, 4, f * bw, 14, cb));

            var strValue = formatOre(oreIce.OreAmount);

            if (oreIce.OreAmount > oreIce.LastOreAmount) pnl.AS.Add(DStr("►", m_brd + 1, m_brd, m_fs, colIce));
            pnl.TS.Add(DStr(oreIce.ShortName, m_brd + 10, m_brd, m_fs, oreIce.OreAmount < limOreIce ? colNone : colVal));
            pnl.TS.Add(DStr(strValue, m_brd + 30 + offOre, m_brd, m_fs, oreIce.OreAmount < limOreIce ? colNone : colVal));
            if (oreIce.OreAmount < oreIce.LastOreAmount) pnl.AS.Add(DStr("►", m_brd + 104 + offOre, m_brd, m_fs, colIce));

            pnl.X = 0;
            pnl.Y = 0;

            var w = 112 + offOre;
            pnl.Width = w + m_brd * 2;

            var line = 1;
            pnl.Height = line * m_step + m_brd * 2;

            pnl.DS.Add(FRct(m_brd, m_brd, w, line * m_step, colBox));
        }

        void RndHO(DO pnl, bool sfn)
        {
            var offOre = sfn ? m_offOre : 0;

            pnl.X = 0;
            pnl.Y = 0;

            var w = 114 + offOre;
            pnl.Width = w + m_brd * 2;

            var line = 0;

            var oName = sfn ? "Oxygen" : "O2";
            var hName = sfn ? "Hydrogen" : "H2";

            if (gasO2.HasTanks)
            {
                var fo = Math.Min(gasO2.Amount / gasO2.Capacity, 1);
                var cbo = fo > 0.99 ? colOut : (fo > 0.9f ? colF2 : (fo > 0.8f ? colF1 : colBar));

                pnl.FS.Add(FRct(45 + offOre, m_brd + line * m_step, fo * 65, 14, cbo));

                if (float.IsNaN(gasO2.LastAmount)) gasO2.LastAmount = gasO2.Amount;

                if (gasO2.Amount > gasO2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 1, m_brd + line * m_step, m_fs, colIce));
                pnl.TS.Add(DStr(oName, m_brd + 10, m_brd + line * m_step, m_fs, 0 < limO2 ? colNone : colVal));
                pnl.TS.Add(DStr(formatOre(gasO2.Amount, true) + "l", m_brd + 26 + offOre, m_brd + line * m_step, m_fs, 0 < limO2 ? colNone : colVal));
                if (gasO2.Amount < gasO2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 107 + offOre, m_brd + line++ * m_step, m_fs, colIce));
                else line++;
            }
            else line++;

            if (gasH2.HasTanks)
            {
                var fh = Math.Min(gasH2.Amount / gasH2.Capacity, 1);
                var cbh = fh > 0.99 ? colOut : (fh > 0.9f ? colF2 : (fh > 0.8f ? colF1 : colBar));

                pnl.FS.Add(FRct(45 + offOre, m_brd + line * m_step, fh * 65, 14, cbh));

                if (float.IsNaN(gasH2.LastAmount)) gasH2.LastAmount = gasH2.Amount;

                if (gasH2.Amount > gasH2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 1, m_brd + line * m_step, m_fs, colIce));
                pnl.TS.Add(DStr(hName, m_brd + 10, m_brd + line * m_step, m_fs, 0 < limH2 ? colNone : colVal));
                pnl.TS.Add(DStr(formatOre(gasH2.Amount, true) + "l", m_brd + 26 + offOre, m_brd + line * m_step, m_fs, 0 < limH2 ? colNone : colVal));
                if (gasH2.Amount < gasH2.LastAmount) pnl.AS.Add(DStr("►", m_brd + 107 + offOre, m_brd + line++ * m_step, m_fs, colIce));
                else line++;
            }
            else line++;

            pnl.Height = line * m_step + m_brd * 2;

            pnl.DS.Add(FRct(m_brd, m_brd, w, line * m_step, colBox));
        }

        void RndC(DO pnl, bool sfn)
        {
            var offCmp1 = sfn ? m_offCmp1 : 0;
            var offCmp2 = sfn ? m_offCmp2 : 0;
            var offCol = 99;

            pnl.X = 0;
            pnl.Y = 0;

            var w = 189 + offCmp1 + offCmp2;
            pnl.Width = w + m_brd * 2;

            pnl.TS.Add(DStr("COMPONENTS", pnl.Width / 2, m_brd, m_fs, colAsm, TextAlignment.CENTER));

            var y = m_brd + 20;
            var line = 0;

            var bc = 0f;
            foreach (var c in Comps)
                if (c.CV > bc) bc = c.CV;

            RndC(pnl, m_brd, y + line++ * m_step, cmpSP, 35, limCmpSP, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpIP, 35, limCmpIP, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpCC, 35, limCmpCC, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpMG, 35, limCmpMG, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpSST, 35, limCmpSST, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpGrd, 35, limCmpGrd, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpLST, 35, limCmpLST, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpGls, 35, limCmpGls, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpDsp, 35, limCmpDsp, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpCmp, 35, limCmpCmp, offCmp1, sfn, bc);
            RndC(pnl, m_brd, y + line++ * m_step, cmpMtr, 35, limCmpMtr, offCmp1, sfn, bc);

            pnl.Height = y + line * m_step + m_brd;

            pnl.DS.Add(FRct(m_brd, y, w, line * m_step, colBox));

            line = 0;

            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpRad, 32, limCmpRad, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpDet, 32, limCmpDet, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpMed, 32, limCmpMed, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpThr, 32, limCmpThr, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpRct, 32, limCmpRct, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpPwr, 32, limCmpPwr, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpSol, 32, limCmpSol, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpSup, 32, limCmpSup, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpGrv, 32, limCmpGrv, offCmp2, sfn, bc);
            RndC(pnl, m_brd + offCol + offCmp1, y + line++ * m_step, cmpExp, 32, limCmpExp, offCmp2, sfn, bc);
        }
        void RndIt(DO pnl, bool sfn)
        {
            var offCmp1 = sfn ? m_offCmp1 : 0;

            pnl.X = 0;
            pnl.Y = 0;

            var w = 93 + offCmp1;
            pnl.Width = w + m_brd * 2;

            var line = 0;

            var bc = 0f;
            foreach (var c in Comps)
                if (c.CV > bc) bc = c.CV;

            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpO2, 35, limCmpO2, offCmp1, sfn, bc);
            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpH2, 35, limCmpH2, offCmp1, sfn, bc);
            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpCnv, 35, limCmpCnv, offCmp1, sfn, bc);
            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpDtpd, 35, limCmpDtpd, offCmp1, sfn, bc);

            pnl.Height = line * m_step + m_brd * 2;

            pnl.DS.Add(FRct(m_brd, m_brd, w, line * m_step, colBox));
        }
        void RndA(DO pnl, bool sfn)
        {
            var offCmp2 = sfn ? m_offCmp2 : 0;

            pnl.X = 0;
            pnl.Y = 0;

            var w = 90 + offCmp2;
            pnl.Width = w + m_brd * 2;

            var line = 0;

            var bc = 0f;
            foreach (var c in Comps)
                if (c.CV > bc) bc = c.CV;

            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpBul, 32, limCmpBul, offCmp2, sfn, bc);
            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpGat, 32, limCmpGat, offCmp2, sfn, bc);
            RndC(pnl, m_brd, m_brd + line++ * m_step, cmpMis, 32, limCmpMis, offCmp2, sfn, bc);

            pnl.Height = ++line * m_step + m_brd * 2;

            pnl.DS.Add(FRct(m_brd, m_brd, w, line * m_step, colBox));
        }
        void RndT(DO pnl, bool sfn)
        {
            var offCmp3 = sfn ? m_offCmp3 : 0;

            pnl.X = 0;
            pnl.Y = 0;

            var w = 73 + offCmp3;
            pnl.Width = w + m_brd * 2;

            pnl.TS.Add(DStr("TOOLS", pnl.Width / 2, m_brd, m_fs, colAsm, TextAlignment.CENTER));

            var y = m_brd + 20;
            var line = 0;

            var bc = 0f;
            foreach (var c in Comps)
                if (c.CV > bc) bc = c.CV;

            RndC(pnl, m_brd, y + line++ * m_step, cmpW1, 15, limCmpW1, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpW2, 15, limCmpW2, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpW3, 15, limCmpW3, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpW4, 15, limCmpW4, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpG1, 15, limCmpG1, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpG2, 15, limCmpG2, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpG3, 15, limCmpG3, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpG4, 15, limCmpG4, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpD1, 15, limCmpD1, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpD2, 15, limCmpD2, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpD3, 15, limCmpD3, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpD4, 15, limCmpD4, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpR1, 15, limCmpR1, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpR2, 15, limCmpR2, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpR3, 15, limCmpR3, offCmp3, sfn, bc, 14);
            RndC(pnl, m_brd, y + line++ * m_step, cmpR4, 15, limCmpR4, offCmp3, sfn, bc, 14);

            pnl.Height = y + line * m_step + m_brd;

            pnl.DS.Add(FRct(m_brd, y, w, line * m_step, colBox));
        }

        void RndC(DO pnl, float x, float y, Comp cmp, float offVal, int lim, float offCmp, bool sfn, float bc, float cut = 0f)
        {
            var inColor = colQue;
            var movingIn = "→";
            var movingOut = "►";

            var typeName =
                cmp.AltName != ""
                ? cmp.AltName
                : cmp.Type.SubtypeId;


            bool itemQueued = false;

            if (m_asm.Count == m_que.Count)
            {
                for (int i = 0; i < m_asm.Count; i++)
                {
                    var asm = m_asm[i];
                    var queue = m_que[i];


                    itemQueued |= queue.Exists(item =>
                           item.BlueprintId.SubtypeName == cmp.Type.SubtypeId
                        || item.BlueprintId.SubtypeName == cmp.AltName);

                    foreach (var item in queue)
                    {
                        if ((item.BlueprintId.SubtypeName == cmp.Type.SubtypeId
                                   || item.BlueprintId.SubtypeName == cmp.AltName)
                               && item.ItemId == asm.NextItemId
                            || cmp.Amount > cmp.LastAmount)
                        {
                            inColor = colAsm;
                            movingIn = "►";
                            goto Done;
                        }
                    }
                }
            }

        Done:

            var w = 40;
            var f = cmp.CV / cmp.MV * m_TCCV / bc;
            var cb = f > 0.99 ? colOut : (f > 0.9f ? colF2 : (f > 0.8f ? colF1 : colBar));
            pnl.FS.Add(FRct(x + 9 + offVal + offCmp + cut, y, f * (w - cut), 14, cb));

            var cmpName =
                sfn
                ? cmp.FullName
                : cmp.ShortName;

            if (itemQueued
                || cmp.Amount > cmp.LastAmount) pnl.AS.Add(DStr(movingIn, x + 1, y, m_fs, inColor));
            pnl.TS.Add(DStr(cmpName + (cmp == cmpG1 && sfn && cmp.Amount < 1000 ? "s" : ""), x + 10, y, m_fs, cmp.Amount < lim ? colNone : colVal));
            pnl.TS.Add(DStr(printValue(cmp.Amount, 0, true, 6), x + offVal + offCmp, y, m_fs, cmp.Amount < lim ? colNone : colVal));
            if (cmp.Amount < cmp.LastAmount) pnl.AS.Add(DStr(movingOut, x + offVal + offCmp + 50, y, m_fs, colAsm));
        }

        void RndS(DO pnl, bool sfn)
        {
            var gens = new List<IMyGasGenerator>(); GridTerminalSystem.GetBlocksOfType(gens, g => IsContentGrid(g.CubeGrid));
            var farms = new List<IMyOxygenFarm>(); GridTerminalSystem.GetBlocksOfType(farms, f => IsContentGrid(f.CubeGrid));
            var refs = new List<IMyRefinery>(); GridTerminalSystem.GetBlocksOfType(refs, r => IsContentGrid(r.CubeGrid));
            var asms = new List<IMyAssembler>(); GridTerminalSystem.GetBlocksOfType(asms, a => IsContentGrid(a.CubeGrid));

            var gensOn = gens.Where(g => g.Enabled).Count();
            var refsOn = refs.Where(r => r.Enabled).Count();
            var asmsOn = asms.Where(a => a.Enabled).Count();

            var strGens = "";
            var strRefs = "";
            var strAsms = "";

            var totalGens = gens.Count + farms.Count;


            if (totalGens > 1)
            {
                strGens =
                       gensOn + farms.Count == totalGens
                    || gensOn + farms.Count == 0
                    ? (gens.Count + farms.Count).ToString()
                    : (gensOn + farms.Count).ToString() + "/" + totalGens.ToString();

                strGens += " ";
            }


            if (refs.Count > 1)
            {
                strRefs =
                       refsOn == refs.Count
                    || refsOn == 0
                    ? refs.Count.ToString()
                    : refsOn.ToString() + "/" + refs.Count.ToString();

                strRefs += " ";
            }


            if (asms.Count > 1)
            {
                strAsms =
                       asmsOn == asms.Count
                    || asmsOn == 0
                    ? asms.Count.ToString()
                    : asmsOn.ToString() + "/" + asms.Count.ToString();

                strAsms += " ";
            }


            pnl.X = 0;
            pnl.Y = 0;

            pnl.Width = sfn ? 580 : 360;
            pnl.Height = 18 + m_brd * 2;

            var off = sfn ? 200 : 120;

            // X == -1f is a flag to signal to the display pass that there are nO blocks of this type
            pnl.TS.Add(DStr(strGens + (sfn ? "Generator" + (totalGens != 1 ? "s" : "") : "GEN"), 0, totalGens > 0 ? m_brd : float.NaN, m_fs * 1.2f, gens.Exists(g => g.Enabled) || farms.Count > 0 ? colIce : colOff, TextAlignment.CENTER));
            pnl.TS.Add(DStr(strRefs + (sfn ? "Refiner" + (refs.Count != 1 ? "ies" : "y") : "REF"), 0, refs.Count > 0 ? m_brd : float.NaN, m_fs * 1.2f, refs.Exists(r => r.Enabled) ? colRef : colOff, TextAlignment.CENTER));
            pnl.TS.Add(DStr(strAsms + (sfn ? "Assembler" + (asms.Count != 1 ? "s" : "") : "ASM"), 0, asms.Count > 0 ? m_brd : float.NaN, m_fs * 1.2f, asms.Exists(a => a.Enabled) ? colAsm : colOff, TextAlignment.CENTER));


            pnl.DS.Add(FRct(m_brd, m_brd, 0, pnl.Height - m_brd * 2, colBox));
        }

        static MySprite DStr(string str, float x, float y, float scale, Color color, TextAlignment align = TextAlignment.LEFT)
        {
            return new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = str,
                Position = new Vector2(x, y),
                RotationOrScale = scale,
                Color = color,
                Alignment = align,
                FontId = "Monospace"
            };
        }
        static MySprite DTx(string texture, Vector2 pos, Vector2 size, Color color, float rotation = 0)
        {
            return new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = texture,
                Position = pos + size / 2,
                Size = size,
                Color = color,
                Alignment = TextAlignment.CENTER,
                RotationOrScale = rotation
            };
        }
        static MySprite DTx(string texture, float x, float y, float w, float h, Color color, float rotation = 0)
        {
            return DTx(texture, new Vector2(x, y), new Vector2(w, h), color, rotation);
        }
        static MySprite DLn(Vector2 p1, Vector2 p2, Color color, double width = 1)
        {
            var dp = p2 - p1;
            var length = dp.Length();
            var angle = (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);

            return DTx(
                "SquareSimple",
                p1.X + dp.X / 2 - length / 2,
                p1.Y + dp.Y / 2 - (float)width / 2,
                length,
                (float)width,
                color,
               -angle);
        }
        static MySprite DLn(double x1, double y1, double x2, double y2, Color color, double width = 1)
        {
            return DLn(new Vector2((float)x1, (float)y1), new Vector2((float)x2, (float)y2), color, width);
        }
        static MySprite FRct(float x, float y, float w, float h, Color color)
        {
            return DTx("SquareSimple", x, y, w, h, color);
        }
        static List<MySprite> DRct(float x, float y, float w, float h, Color color)
        {
            return new List<MySprite>()
    {
        DTx("SquareSimple", x,       y,       w-1, 1,   color),
        DTx("SquareSimple", x,       y + h-1, w,   1,   color),
        DTx("SquareSimple", x,       y,       1,   h-1, color),
        DTx("SquareSimple", x + w-1, y,       1,   h-1, color)
    };
        }
        #endregion


        class DO
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;

            public List<MySprite> DS;
            public List<MySprite> TS;
            public List<MySprite> AS;
            public List<MySprite> FS;

            public DO()
            {
                X = 0;
                Y = 0;
                Width = 0;
                Height = 0;

                DS = new List<MySprite>();
                TS = new List<MySprite>();
                AS = new List<MySprite>();
                FS = new List<MySprite>();
            }

            public void CS()
            {
                DS.Clear();
                TS.Clear();
                AS.Clear();
                FS.Clear();
            }
        }

        DO pnlO;
        DO pnlI;
        DO pnlOI;
        DO pnlIcHO;
        DO pnlIc;
        DO pnlHO;
        DO pnlC;
        DO pnlIt;
        DO pnlA;
        DO pnlT;
        DO pnlS;

        DO pnlFO;
        DO pnlFI;
        DO pnlFOI;
        DO pnlFIcHO;
        DO pnlFIc;
        DO pnlFHO;
        DO pnlFC;
        DO pnlFIt;
        DO pnlFA;
        DO pnlFT;
        DO pnlFS;


        class Display
        {
            IMyTextSurfaceProvider m_provider;
            bool m_useSurfaceSize;

            public IMyTextSurface Surface;
            public RectangleF V;

            public float ContentWidth;
            public float ContentHeight;

            public DS Set;


            public Display(IMyTextSurfaceProvider provider, DS set, long eid)
            {
                if (set.Ind < 0)
                    set.Ind = 0;

                m_provider = provider;
                Surface = m_provider.GetSurface(set.Ind);
                m_useSurfaceSize = true;

                Init(set, eid);
            }
            public Display(IMyTextSurface surface, DS set, long eid, bool useSurfaceSize = true)
            {
                m_provider = null;
                Surface = surface;
                m_useSurfaceSize = useSurfaceSize;

                Init(set, eid);
            }

            void Init(DS set, long eid)
            {
                Surface.WriteText(eid.ToString() + "/" + ((int)Surface.ContentType).ToString(), false);

                Surface.ContentType = ContentType.SCRIPT;

                ContentWidth = 0;
                ContentHeight = 0;

                Set = set;

                Surface.Script = "";
                Surface.ScriptBackgroundColor = colBack;

                V = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2, Surface.SurfaceSize);
            }


            public float ContentScale
            {
                get
                {
                    return
                          Math.Min(Surface.TextureSize.X, Surface.TextureSize.Y) / 512
                        * Math.Min(Surface.SurfaceSize.X, Surface.SurfaceSize.Y)
                        / Math.Min(Surface.TextureSize.Y, Surface.TextureSize.Y);
                }
            }

            public float UserScale
            {
                get
                {
                    if (Set.Scale == 0)
                    {
                        return
                            Surface.SurfaceSize.X / ContentWidth < Surface.SurfaceSize.Y / ContentHeight
                            ? (Surface.SurfaceSize.X - 10) / ContentWidth
                            : (Surface.SurfaceSize.Y - 10) / ContentHeight;
                    }
                    else return Set.Scale;
                }
            }

            public void Draw(ref MySpriteDrawFrame frame, List<MySprite> sprites)
            {
                Add(ref frame, 0, 0, sprites);
            }
            public void Draw(ref MySpriteDrawFrame frame, DO pnl)
            {
                if (Set.Box) Add(ref frame, pnl.X, pnl.Y, pnl.DS);
                if (Set.Bars) Add(ref frame, pnl.X, pnl.Y, pnl.FS);
                Add(ref frame, pnl.X, pnl.Y, pnl.TS);
                if (Set.Arr) Add(ref frame, pnl.X, pnl.Y, pnl.AS);
            }

            public void Add(ref MySpriteDrawFrame frame, float x, float y, List<MySprite> sprites)
            {
                foreach (var sprite in sprites)
                    Add(ref frame, x, y, sprite);
            }
            public void Add(ref MySpriteDrawFrame frame, float x, float y, MySprite sprite)
            {
                if (sprite.Type == SpriteType.TEXT) sprite.RotationOrScale *= UserScale;
                else if (sprite.Type == SpriteType.TEXTURE) sprite.Size *= UserScale;

                sprite.Position *= UserScale;

                sprite.Position +=
                      V.Position
                    + V.Size / 2
                    - new Vector2(ContentWidth, ContentHeight) / 2 * UserScale
                    + new Vector2(x, y) * UserScale;

                frame.Add(sprite);
            }
        }


        public Program()
        {
            this.Runtime.UpdateFrequency = UpdateFrequency.Update10;

            m_grids = new List<string>();
            m_ds = new List<DS>();
            m_short = new List<GridShortcut>();
            m_dsp = new List<Display>();

            pnlOI = new DO();
            pnlO = new DO();
            pnlI = new DO();
            pnlIcHO = new DO();
            pnlIc = new DO();
            pnlHO = new DO();
            pnlC = new DO();
            pnlIt = new DO();
            pnlA = new DO();
            pnlT = new DO();
            pnlS = new DO();

            pnlFOI = new DO();
            pnlFO = new DO();
            pnlFI = new DO();
            pnlFIcHO = new DO();
            pnlFIc = new DO();
            pnlFHO = new DO();
            pnlFC = new DO();
            pnlFIt = new DO();
            pnlFA = new DO();
            pnlFT = new DO();
            pnlFS = new DO();

            m_fs = 0.42f;

            m_inv = new List<IMyInventory>();
            m_asm = new List<IMyAssembler>();
            m_que = new List<List<MyProductionItem>>();

            m_TCOV = 0;
            m_TMOV = 0;
            m_TCIV = 0;
            m_TMIV = 0;
            m_TCCV = 0;
            m_TMCV = 0;

            m_o = false;
            m_ic = false;
            m_c = false;
            m_s = false;
            m_b = false;

            m_updateTick = 0;

            m_cpx = new int[10];
            m_scpx = new int[10];
        }


        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource == UpdateType.Update10)
            {
                switch (m_updateTick++)
                {
                    case 0: // config
                        {
                            LoadConfig();
                            UpdateDisplays();

                            m_cpx[0] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 1: // inventories
                        {
                            IMyCargoContainer refCargo = null;

                            var cargo = new List<IMyCargoContainer>();
                            GridTerminalSystem.GetBlocksOfType(cargo);

                            var mv = 0f;

                            foreach (var c in cargo) // find largest cargo container
                            {
                                var cmv = 0f;

                                for (int i = 0; i < c.InventoryCount; i++)
                                    cmv += (float)c.GetInventory(i).MaxVolume;

                                if (cmv > mv)
                                {
                                    mv = cmv;
                                    refCargo = c;
                                }
                            }

                            var blocks = new List<IMyCubeBlock>();
                            GridTerminalSystem.GetBlocksOfType(blocks, b => IsContentGrid(b.CubeGrid));

                            foreach (var block in blocks)
                            {
                                for (int i = 0; i < block.InventoryCount; i++)
                                {
                                    //if (   refCargo == null
                                    //    || block.GetInventory(i).IsConnectedTo(refCargo.GetInventory(0)))
                                    m_inv.Add(block.GetInventory(i));
                                }
                            }

                            GridTerminalSystem.GetBlocksOfType(m_asm, a => IsContentGrid(a.CubeGrid));

                            m_cpx[1] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 2: // oxygen
                        {
                            if (m_ic) UpdateO2();
                            m_cpx[2] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 3: // hydrogen
                        {
                            if (m_ic) UpdateH2();
                            m_cpx[3] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 4: // ore & components
                        {
                            if (m_o) UpdateOre();
                            if (m_c) UpdateComps();
                            m_cpx[4] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 5: // queues
                        {
                            if (m_c) UpdateQueues();
                            m_cpx[5] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 6: // rendering
                        {
                            m_alive = !m_alive;

                            if (m_o) RndOI(pnlOI, false);
                            if (m_o) RndO(pnlO, false);
                            if (m_o) RndI(pnlI, false);
                            if (m_o || m_ic) RndIcHO(pnlIcHO, false);
                            if (m_o) RndIc(pnlIc, false);
                            if (m_ic) RndHO(pnlHO, false);
                            if (m_c) RndC(pnlC, false);
                            if (m_c) RndIt(pnlIt, false);
                            if (m_c) RndA(pnlA, false);
                            if (m_c) RndT(pnlT, false);
                            if (m_s) RndS(pnlS, false);

                            if (m_ds.Exists(ds => ds.SFN))
                            {
                                if (m_o) RndOI(pnlFOI, true);
                                if (m_o) RndO(pnlFO, true);
                                if (m_o) RndI(pnlFI, true);
                                if (m_o || m_ic) RndIcHO(pnlFIcHO, true);
                                if (m_o) RndIc(pnlFIc, true);
                                if (m_ic) RndHO(pnlFHO, true);
                                if (m_c) RndC(pnlFC, true);
                                if (m_c) RndIt(pnlFIt, true);
                                if (m_c) RndA(pnlFA, true);
                                if (m_c) RndT(pnlFT, true);
                                if (m_s) RndS(pnlFS, true);
                            }

                            m_cpx[6] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 7:
                        {
                            break;
                        }
                    case 8: // display
                        {
                            foreach (var d in m_dsp)
                            {
                                var frame = d.Surface.DrawFrame();
                                var set = d.Set;


                                if (m_showCalibration)
                                {
                                    frame.AddRange(DRct(d.V.X + 2, d.V.Y + 2, d.V.Width - 4, d.V.Height - 4, colVal));

                                    frame.Add(DLn(d.V.Position + new Vector2(2, 2), d.V.Size - new Vector2(2, 2), colVal));
                                    frame.Add(DLn(d.V.Position + new Vector2(2, d.V.Height - 2), new Vector2(d.V.Width - 2, 2), colVal));

                                    frame.Add(DStr(
                                          "Texture size: "
                                        + d.Surface.TextureSize.X.ToString()
                                        + " x "
                                        + d.Surface.TextureSize.Y.ToString(),
                                        d.V.X + d.V.Width / 2,
                                        d.V.Y + d.V.Height / 2 - 40 * d.ContentScale,
                                        d.ContentScale,
                                        colVal,
                                        TextAlignment.CENTER));

                                    frame.Add(DStr(
                                          "Surface size: "
                                        + d.Surface.SurfaceSize.X.ToString()
                                        + " x "
                                        + d.Surface.SurfaceSize.Y.ToString(),
                                        d.V.X + d.V.Width / 2,
                                        d.V.Y + d.V.Height / 2 - 15 * d.ContentScale,
                                        d.ContentScale,
                                        colVal,
                                        TextAlignment.CENTER));

                                    frame.Add(DStr(
                                        "Content scale: " + d.ContentScale.ToString("0.0000"),
                                        d.V.X + d.V.Width / 2,
                                        d.V.Y + d.V.Height / 2 + 10 * d.ContentScale,
                                        d.ContentScale,
                                        colVal,
                                        TextAlignment.CENTER));

                                    frame.Dispose();
                                    continue;
                                }

                                var pFO = set.O && set.I ? pnlFOI : (set.O ? pnlFO : pnlFI);
                                var pO = set.O && set.I ? pnlOI : (set.O ? pnlO : pnlI);

                                var pFIc = set.O && set.I ? pnlFIcHO : (set.O ? pnlFIc : pnlFHO);
                                var pIc = set.O && set.I ? pnlIcHO : (set.O ? pnlIc : pnlHO);

                                var _pnlO = set.SFN ? pFO : pO;
                                var _pnlIc = set.SFN ? pFIc : pIc;
                                var _pnlC = set.SFN ? pnlFC : pnlC;
                                var _pnlIt = set.SFN ? pnlFIt : pnlIt;
                                var _pnlA = set.SFN ? pnlFA : pnlA;
                                var _pnlT = set.SFN ? pnlFT : pnlT;


                                d.ContentWidth = 0;
                                d.ContentHeight = 0;


                                if ((set.O
                                        || set.I)
                                    && set.Ic)
                                {
                                    d.ContentWidth += Math.Max(_pnlO.Width, _pnlIc.Width);
                                    d.ContentHeight += _pnlO.Height + _pnlIc.Height;

                                    _pnlIc.Y = _pnlO.Y + _pnlO.Height + (set.T ? 7 : 0);
                                }
                                else if (set.O
                                         || set.I)
                                {
                                    d.ContentWidth += _pnlO.Width;
                                    d.ContentHeight += _pnlO.Height;
                                }
                                else if (set.Ic)
                                {
                                    if (!set.O && !set.I && !set.C && set.T)
                                        _pnlIc.Y = 20;

                                    d.ContentWidth += _pnlIc.Width;
                                    d.ContentHeight += _pnlIc.Y + _pnlIc.Height;
                                }


                                if (set.C)
                                {
                                    _pnlC.X = d.ContentWidth;

                                    d.ContentWidth += _pnlC.Width;
                                    d.ContentHeight = Math.Max(d.ContentHeight, _pnlC.Height);
                                }


                                var xRight =
                                    set.O || set.I || set.Ic
                                    ? _pnlIc.Width
                                    : 0;


                                if (set.It
                                    && set.A)
                                {
                                    _pnlIt.X = xRight;
                                    _pnlA.X = _pnlIt.X + _pnlIt.Width - 2;

                                    _pnlIt.Y = set.C ? _pnlC.Y + _pnlC.Height + (set.T ? 7 : 0) : (set.O || set.I || set.C || set.T ? 20 : 0);
                                    _pnlA.Y = _pnlIt.Y;

                                    if (!set.C)
                                        d.ContentWidth += _pnlIt.Width + _pnlA.Width - 2;

                                    d.ContentHeight =
                                        set.C
                                        ? _pnlC.Height + Math.Max(_pnlIt.Height, _pnlA.Height)
                                        : Math.Max(d.ContentHeight, Math.Max(_pnlIt.Height, _pnlA.Height));
                                }
                                else if (set.It)
                                {
                                    _pnlIt.X = xRight;
                                    _pnlIt.Y = set.C ? _pnlC.Y + _pnlC.Height + (set.T ? 7 : 0) : (set.O || set.I || set.C || set.T ? 20 : 0);

                                    if (!set.C)
                                        d.ContentWidth += _pnlIt.Width;

                                    d.ContentHeight = Math.Max(d.ContentHeight, (set.C ? _pnlC.Height : 0) + _pnlIt.Height);
                                }
                                else if (set.A)
                                {
                                    _pnlA.X = xRight + (set.C ? _pnlIt.Width - 2 : 0);
                                    _pnlA.Y = set.C ? _pnlC.Y + _pnlC.Height + (set.T ? 7 : 0) : (set.O || set.I || set.C || set.T ? 20 : 0);

                                    if (!set.C)
                                        d.ContentWidth += _pnlA.Width;

                                    d.ContentHeight = Math.Max(d.ContentHeight, (set.C ? _pnlC.Height : 0) + _pnlA.Height);
                                }


                                if (set.T)
                                {
                                    _pnlT.X = d.ContentWidth;

                                    d.ContentWidth += _pnlT.Width;
                                    d.ContentHeight = Math.Max(d.ContentHeight, _pnlT.Height);
                                }


                                if (set.S)
                                {
                                    pnlS.Y = d.ContentHeight;
                                    pnlFS.Y = d.ContentHeight;

                                    d.ContentHeight += pnlS.Height;
                                }


                                if (set.O
                                 || set.I) d.Draw(ref frame, _pnlO);
                                if (set.Ic) d.Draw(ref frame, _pnlIc);
                                if (set.C) d.Draw(ref frame, _pnlC);
                                if (set.It) d.Draw(ref frame, _pnlIt);
                                if (set.A) d.Draw(ref frame, _pnlA);
                                if (set.T) d.Draw(ref frame, _pnlT);


                                if (set.S)
                                {
                                    var nWords = 0;

                                    var b0 = !float.IsNaN(pnlS.TS[0].Position.Value.X) && set.Ic;
                                    var b1 = !float.IsNaN(pnlS.TS[1].Position.Value.X) && (set.O || set.I);
                                    var b2 = !float.IsNaN(pnlS.TS[2].Position.Value.X) && (set.C || set.I || set.A || set.T);

                                    if (b0) nWords++;
                                    if (b1) nWords++;
                                    if (b2) nWords++;

                                    var fullStatus =
                                           set.SFN
                                        && (d.Surface.SurfaceSize.X != d.Surface.SurfaceSize.Y
                                            || nWords == 1 && (!set.C && !set.I && !set.A && !set.T
                                                               || !set.O && !set.I && !set.Ic && !set.C && !set.T
                                                               || !set.Ic && !set.I && !set.A && !set.T)
                                            || (set.O || set.I) && set.C
                                            || set.C);

                                    var _pnlS = fullStatus ? pnlFS : pnlS;
                                    _pnlS.X = (d.ContentWidth - _pnlS.Width) / 2;

                                    if (set.Box) d.Add(ref frame, _pnlS.X, _pnlS.Y, _pnlS.DS);

                                    var step = fullStatus ? 200 : 140;
                                    var width = (nWords - 1) * step;

                                    var x = _pnlS.X + _pnlS.Width / 2 - width / 2;

                                    if (b0) { d.Add(ref frame, x, _pnlS.Y, _pnlS.TS[0]); x += step; }
                                    if (b1) { d.Add(ref frame, x, _pnlS.Y, _pnlS.TS[1]); x += step; }
                                    if (b2) { d.Add(ref frame, x, _pnlS.Y, _pnlS.TS[2]); }
                                }


                                if (m_alive)
                                    frame.Add(FRct(d.V.X, d.V.Y + d.V.Height - 6, 6, 6, colOff));

                                frame.Dispose();
                            }


                            m_cpx[8] = this.Runtime.CurrentInstructionCount;
                            break;
                        }
                    case 9: // cleanup
                        {
                            foreach (var o in Ores)
                            {
                                o.LastOreAmount = o.OreAmount;
                                o.LastIngotAmount = o.IngotAmount;
                            }
                            foreach (var g in Gases) g.LastAmount = g.Amount;
                            foreach (var c in Comps) c.LastAmount = c.Amount;

                            m_grids.Clear();
                            m_ds.Clear();
                            m_short.Clear();

                            m_dsp.Clear();
                            m_inv.Clear();
                            m_que.Clear();

                            pnlOI.CS();
                            pnlO.CS();
                            pnlI.CS();
                            pnlIcHO.CS();
                            pnlIc.CS();
                            pnlHO.CS();
                            pnlC.CS();
                            pnlIt.CS();
                            pnlA.CS();
                            pnlT.CS();
                            pnlS.CS();

                            pnlFOI.CS();
                            pnlFO.CS();
                            pnlFI.CS();
                            pnlFIcHO.CS();
                            pnlFIc.CS();
                            pnlFHO.CS();
                            pnlFC.CS();
                            pnlFIt.CS();
                            pnlFA.CS();
                            pnlFT.CS();
                            pnlFS.CS();

                            for (int i = 0; i < m_cpx.Length; i++)
                                m_scpx[i] = m_cpx[i];

                            m_updateTick = 0;
                            break;
                        }
                }


                this.Echo("             Simple Inventory Display");
                this.Echo("         –––––––––––v 0.4––––––––––––");
                this.Echo("                 Complexity // " + this.Runtime.MaxInstructionCount.ToString());
                this.Echo("                 Config             " + m_scpx[0].ToString());
                this.Echo("                 Inventories     " + m_scpx[1].ToString());
                this.Echo("                 Oxygen           " + m_scpx[2].ToString());
                this.Echo("                 Hydrogen        " + m_scpx[3].ToString());
                this.Echo("                 Ore & Comps  " + m_scpx[4].ToString());
                this.Echo("                 Queues           " + m_scpx[5].ToString());
                this.Echo("                 Rendering       " + m_scpx[6].ToString());
                this.Echo("                 Display           " + m_scpx[8].ToString());
            }
        }



        #region PreludeFooter
    }
}
#endregion