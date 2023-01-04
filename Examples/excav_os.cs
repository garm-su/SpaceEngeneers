
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
using VRage.Game.ModAPI.Ingame.Utilities;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.ModAPI.Contracts;
using Sandbox.Game;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.ExcavOS
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
        /*
         * ExcavOS
         * -----------
         * 
         * Welcome to ExcavOS. You don't need to change anything in code kthxbye.
         */

        private ExcavOS _scriptHandler;
        private MyIni _storage;

        public Program()
        {
            _storage = new MyIni();
            _storage.TryParse(Storage);
            _scriptHandler = new ExcavOS(this, _storage);
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
        }

        public void Save()
        {
            _scriptHandler.Save();
            Storage = _storage.ToString();        }

        public void Main(string argument, UpdateType updateSource)
        {
            _scriptHandler.Update(argument, updateSource, Runtime.TimeSinceLastRun);
        }

        public class AllCargoScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "Cargo";
            private readonly StringBuilder sb = new StringBuilder();

            public AllCargoScreen(ExcavOSContext context) : base(context)
            {
            }

            private string ExtractName(string itemType)
            {
                return itemType.Split('/').Last();
            }

            private string FormatWithSuffix(double amount)
            {
                if (amount >= 1000000)
                {
                    return string.Format("{0,10:0.00}Mt", amount / 1000000);
                }
                else if (amount >= 1000)
                {
                    return string.Format("{0,10:0.00}t", amount / 1000);
                }
                return string.Format("{0,10:0.00}kg", amount);
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    float margin = Painter.Width >= 512.0f ? 25.0f : 5.0f;
                    float gap = Painter.Width >= 512.0f ? 10.0f : 2.0f;
                    float fontSize = (Painter.Width >= 512.0f ? 1.0f : 0.8f) * surface.FontSize;
                    Vector2 position = new Vector2(margin, margin);
                    Vector2 barSize = new Vector2(Painter.Width - margin * 2, Painter.Width >= 512.0f ? 2.0f : 1.0f);

                    if (!_context._cargoManager.hasSomethingExceptOre)
                    {
                        Painter.SpriteCentered(Painter.Center, new Vector2(128f, 128f), "MyObjectBuilder_Component/Construction", Painter.SecondaryColor);
                        Painter.Text(Painter.Center, "Empty cargo");
                        return;
                    }

                    _context._cargoManager.IterateCargoDescending((name, entry) =>
                    {
                        if (entry.typeid == "MyObjectBuilder_Ore")
                        {
                            return;
                        }
                        sb.Clear();
                        sb.Append(ExtractName(name));
                        String amount;
                        if (entry.typeid == "MyObjectBuilder_Ore")
                        {
                            amount = FormatWithSuffix(entry.amount);
                        }
                        else
                        {
                            amount = string.Format("{0,10:0}", entry.amount);
                        }
                        Vector2 textSize = surface.MeasureStringInPixels(sb, surface.Font, fontSize);
                        Painter.Sprite(position, new Vector2(textSize.Y, textSize.Y), name);
                        Painter.Text(position + new Vector2(textSize.Y + gap, 0), sb.ToString(), fontSize, TextAlignment.LEFT);
                        Painter.Text(new Vector2(Painter.Width - margin, position.Y), amount, fontSize, TextAlignment.RIGHT);
                        position.Y += textSize.Y + gap;
                        Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                        position.Y += gap;
                    });
                }
            }
        }

        public class BlankScreen : ScreenHandler<ExcavOSContext>
        {

            public BlankScreen(ExcavOSContext context) : base(context)
            {
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                }
            }
        }

        public class BlockFinder<T> where T : class
        {
            private const double CACHE_TIME = 10.0f;
            private readonly Program program;
            private DateTime lastFetch;
            public readonly List<T> blocks = new List<T>();

            public BlockFinder(Program program)
            {
                this.program = program;
                lastFetch = DateTime.Now;
                lastFetch.AddSeconds(-CACHE_TIME);
            }

            public void FindBlocks(bool sameConstruct = true, Func<T, bool> filter = null, string groupName = null)
            {
                if (blocks.Count > 0 && lastFetch.AddSeconds(CACHE_TIME).CompareTo(DateTime.Now) >= 0)
                {
                    return;
                }

                Func<T, bool> filterFunc = block =>
                {
                    bool constructCheck = true;
                    if (block is IMyTerminalBlock)
                    {
                        if (sameConstruct)
                        {
                            constructCheck = (block as IMyTerminalBlock).IsSameConstructAs(program.Me);
                        }
                        else
                        {
                            constructCheck = !(block as IMyTerminalBlock).IsSameConstructAs(program.Me);
                        }
                    }
                    return constructCheck && ((filter != null) ? filter(block) : true);
                };

                lastFetch = new DateTime();
                blocks.Clear();

                if (groupName != null && groupName != "")
                {
                    IMyBlockGroup group = program.GridTerminalSystem.GetBlockGroupWithName(groupName);
                    if (group != null)
                    {
                        group.GetBlocksOfType(blocks, filterFunc);
                    }
                }
                else
                {
                    program.GridTerminalSystem.GetBlocksOfType(blocks, filterFunc);
                }
            }

            public void ForEach(Action<T> callback)
            {
                blocks.ForEach(callback);
            }

            public bool HasBlocks()
            {
                return blocks.Count > 0;
            }

            public int Count()
            {
                return blocks.Count;
            }
        }

        public class BlockHelper
        {

            private const string LARGE_BLOCK = "LargeBlock";
            private const string SMALL_BLOCK = "SmallBlock";
            private const string TYPEID_WIND_TURBINE = "MyObjectBuilder_WindTurbine";
            private const string TYPEID_HYDROGEN_ENGINE = "MyObjectBuilder_HydrogenEngine";
            private const string TYPEID_OXYGEN_TANK = "MyObjectBuilder_OxygenTank";
            private const string SUBTYPEID_HYDROGEN_TANK = "HydrogenTank";

            private static string ExtractData(string marker, string value)
            {
                int startPos = value.IndexOf(marker);
                if (startPos == -1)
                {
                    return "";
                }
                string part = value.Substring(startPos + marker.Length);
                int endPos = part.IndexOf('\n');
                if (endPos > 0)
                {
                    return part.Substring(0, endPos);
                }
                return part;
            }

            public static bool IsLargeBlock(IMyTerminalBlock block)
            {
                return block.BlockDefinition.SubtypeId.StartsWith(LARGE_BLOCK);
            }

            public static bool IsSmallBlock(IMyTerminalBlock block)
            {
                return block.BlockDefinition.SubtypeId.StartsWith(SMALL_BLOCK);
            }

            public static bool IsWindTurbine(IMyTerminalBlock block)
            {
                return block.BlockDefinition.TypeId.ToString() == TYPEID_WIND_TURBINE;
            }

            public static bool IsHydrogenEngine(IMyTerminalBlock block)
            {
                return block.BlockDefinition.TypeId.ToString() == TYPEID_HYDROGEN_ENGINE;
            }

            public static float GetReactorFuelLevel(IMyReactor reactor)
            {
                IMyInventory inventory = reactor.GetInventory(0);
                return (float)inventory.CurrentVolume / (float)inventory.MaxVolume;
            }

            public static float GetHydrogenEngineFuelLevel(IMyPowerProducer hydrogenEngine)
            {
                string filledLine = ExtractData("Filled: ", hydrogenEngine.DetailedInfo);
                string[] fillValues = ExtractData(" (", filledLine).TrimEnd(')').Replace("L", string.Empty).Split('/');
                if (fillValues.Length != 2)
                {
                    return 0.0f;
                }
                float filled = float.Parse(fillValues[0]);
                float max = float.Parse(fillValues[1]);
                return filled / max;
            }

            public static string GetWindTurbineClearance(IMyPowerProducer block)
            {
                return ExtractData("Wind Clearance: ", block.DetailedInfo);
            }

            public static float GetTankFill(IMyGasTank block)
            {
                return (float)(block.FilledRatio * block.Capacity);
            }

            public static bool IsOxygenTank(IMyGasTank block)
            {
                return block.BlockDefinition.TypeId.ToString() == TYPEID_OXYGEN_TANK;
            }

            public static bool IsHydrogenTank(IMyGasTank block)
            {
                return block.BlockDefinition.SubtypeId.ToString().Contains(SUBTYPEID_HYDROGEN_TANK);
            }
        }

        public class CargoEntry
        {
            public string typeid;
            public double amount;
        }

        public class CargoManager
        {
            private readonly BlockFinder<IMyTerminalBlock> cargoBlocks;
            private readonly List<MyInventoryItem> items = new List<MyInventoryItem>();
            public bool hasAnyOre = false;
            public bool hasSomethingExceptOre = false;

            private Config _config;
            public double CurrentCapacity;
            public double TotalCapacity;
            public IDictionary<string, CargoEntry> cargo = new Dictionary<string, CargoEntry>();

            public CargoManager(Program program, Config config)
            {
                cargoBlocks = new BlockFinder<IMyTerminalBlock>(program);
                _config = config;
            }

            public void QueryData()
            {
                cargoBlocks.FindBlocks(true, block =>
                {
                    if (_config.CargoTrackGroupName == "" && block is IMyConveyorSorter)
                    {
                        return false;
                    }
                    return block.HasInventory && block.IsFunctional;
                }, _config.CargoTrackGroupName);
                CurrentCapacity = 0;
                TotalCapacity = 0;
                hasAnyOre = false;
                hasSomethingExceptOre = false;
                cargo.Clear();
                cargoBlocks.ForEach(ProcessBlock);
            }

            private void ProcessBlock(IMyTerminalBlock block)
            {
                for (int i = 0; i < block.InventoryCount; i++)
                {
                    items.Clear();
                    IMyInventory inventory = block.GetInventory(0);

                    CurrentCapacity += (double)inventory.CurrentVolume;
                    TotalCapacity += (double)inventory.MaxVolume;
                    block.GetInventory(i).GetItems(items);
                    foreach (MyInventoryItem item in items)
                    {
                        if (item.Type.TypeId == "MyObjectBuilder_Ore")
                        {
                            hasAnyOre = true;
                        }
                        else
                        {
                            hasSomethingExceptOre = true;
                        }
                        //string itemName = item.Type.SubtypeId;
                        string itemName = item.Type.ToString();
                        double amount = (double)item.Amount;
                        if (cargo.ContainsKey(itemName))
                        {
                            CargoEntry ce = cargo[itemName];
                            ce.amount += amount;
                            CargoEntry ce2 = cargo[itemName];
                        }
                        else
                        {
                            CargoEntry ce = new CargoEntry
                            {
                                amount = amount,
                                typeid = item.Type.TypeId
                            };
                            cargo.Add(itemName, ce);
                        }
                    }
                }
            }

            public bool IsEmpty()
            {
                return cargo.Count() == 0;
            }

            public void IterateCargoDescending(Action<string, CargoEntry> callback)
            {
                foreach (var item in cargo.OrderByDescending(key => key.Value.amount))
                {
                    callback(item.Key, item.Value);
                }
            }

        }

        public class CargoScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "CargoOre";
            private readonly StringBuilder sb = new StringBuilder();

            public CargoScreen(ExcavOSContext context) : base(context)
            {
            }

            private string ExtractName(string itemType)
            {
                return itemType.Split('/').Last();
            }

            private string FormatWithSuffix(double amount)
            {
                if (amount >= 1000000)
                {
                    return string.Format("{0,10:0.00}Mt", amount / 1000000);
                }
                else if (amount >= 1000)
                {
                    return string.Format("{0,10:0.00}t", amount / 1000);
                }
                return string.Format("{0,10:0.00}kg", amount);
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    float margin = Painter.Width >= 512.0f ? 25.0f : 5.0f;
                    float gap = Painter.Width >= 512.0f ? 10.0f : 2.0f;
                    float fontSize = (Painter.Width >= 512.0f ? 1.0f : 0.8f) * surface.FontSize;
                    Vector2 position = new Vector2(margin, margin);
                    Vector2 barSize = new Vector2(Painter.Width - margin * 2, Painter.Width >= 512.0f ? 2.0f : 1.0f);

                    if (!_context._cargoManager.hasAnyOre)
                    {
                        Painter.SpriteCentered(Painter.Center, new Vector2(128f, 128f), "MyObjectBuilder_Ore/Stone", Painter.SecondaryColor);
                        Painter.Text(Painter.Center, "No ores");
                        return;
                    }

                    _context._cargoManager.IterateCargoDescending((name, entry) =>
                    {
                        if (entry.typeid != "MyObjectBuilder_Ore")
                        {
                            return;
                        }
                        sb.Clear();
                        sb.Append(ExtractName(name));
                        Vector2 textSize = surface.MeasureStringInPixels(sb, surface.Font, fontSize);
                        Painter.Sprite(position, new Vector2(textSize.Y, textSize.Y), name);
                        Painter.Text(position + new Vector2(textSize.Y + gap, 0), sb.ToString(), fontSize, TextAlignment.LEFT);
                        Painter.Text(new Vector2(Painter.Width - margin, position.Y), FormatWithSuffix(entry.amount), fontSize, TextAlignment.RIGHT);
                        position.Y += textSize.Y + gap;
                        Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                        position.Y += gap;
                    });
                }
            }
        }

        public class Config : ScriptConfig
        {

            public string LiftThrustersGroupName = "";
            public string StopThrustersGroupName = "";
            public string CargoTrackGroupName = "";
            public string AlignGyrosGroupName = "";
            public string DumpSortersGroupName = "";
            public float LiftThresholdWarning = 0.9f;

            public Config(MyIni ini, string section) : base(ini, section)
            {
            }

            public override void SetupDefaults()
            {
                _ini.Set(_section, "LiftThrustersGroupName", LiftThrustersGroupName);
                _ini.Set(_section, "StopThrustersGroupName", StopThrustersGroupName);
                _ini.Set(_section, "CargoTrackGroupName", CargoTrackGroupName);
                _ini.Set(_section, "AlignGyrosGroupName", AlignGyrosGroupName);
                _ini.Set(_section, "DumpSortersGroupName", DumpSortersGroupName);
                _ini.Set(_section, "LiftThresholdWarning", LiftThresholdWarning);
            }

            public override void ReadConfig()
            {
                LiftThrustersGroupName = GetValue("LiftThrustersGroupName").ToString(LiftThrustersGroupName);
                StopThrustersGroupName = GetValue("StopThrustersGroupName").ToString(StopThrustersGroupName);
                CargoTrackGroupName = GetValue("CargoTrackGroupName").ToString(CargoTrackGroupName);
                AlignGyrosGroupName = GetValue("AlignGyrosGroupName").ToString(AlignGyrosGroupName);
                DumpSortersGroupName = GetValue("DumpSortersGroupName").ToString(DumpSortersGroupName);
                LiftThresholdWarning = GetValue("LiftThresholdWarning").ToSingle(LiftThresholdWarning);
            }
        }

        public class ExcavOS : ScriptHandler
        {

            protected ExcavOSContext _context;
            private readonly BlockFinder<IMyTerminalBlock> _surfaceProviders;
            private Dictionary<long, RegisteredProvider> _registeredProviders = new Dictionary<long, RegisteredProvider>();

            public ExcavOS(Program program, MyIni storage) : base(program, storage, "ExcavOS", "0.3")
            {
                _surfaceProviders = new BlockFinder<IMyTerminalBlock>(program);
                _context = new ExcavOSContext(program, _config, _storage);
                Initialize();
            }

            public void Save()
            {
                _context.Save();
            }

            public override void FetchBlocks()
            {
                FetchSurfaces();
            }

            private void FetchSurfaces()
            {
                _surfaceProviders.FindBlocks(true, block =>
                {

                    if (!(block is IMyTextSurfaceProvider))
                    {
                        return false;
                    }

                    if (!MyIni.HasSection(block.CustomData, _scriptName))
                    {
                        return false;
                    }

                    RegisteredProvider registeredProvider;
                    if (!_registeredProviders.ContainsKey(block.EntityId))
                    {
                        registeredProvider = new RegisteredProvider(_context, block, _ini, _scriptName);
                        _registeredProviders.Add(block.EntityId, registeredProvider);
                    }
                    else
                    {
                        registeredProvider = _registeredProviders[block.EntityId];
                    }

                    if (block == _program.Me)
                    {
                        registeredProvider.SetScreenHandlerForSurface(ExcavOSScreen.SCREEN_NAME, 0);
                    }
                    else
                    {
                        registeredProvider.LoadConfig(block.CustomData);
                    }

                    if (!registeredProvider.HasSurfaces())
                    {
                        _registeredProviders.Remove(block.EntityId);
                    }

                    return true;

                });

            }

            protected override void Update10()
            {
                _context.Update(_timeAccumulator);
                foreach (var registeredProvider in _registeredProviders.Values)
                {
                    registeredProvider.Update();
                }
            }

            protected override void HandleCommand(string argument)
            {
                _context.HandleCommand(argument);
            }

        }

        public class ExcavOSContext
        {
            public Program _program;
            private Config _config;
            public MyIni _storage;

            public TimeSpan TimeAccumulator;
            public Random Randomizer = new Random();
            public CargoManager _cargoManager;
            public WeightAnalizer _weightAnalizer;
            public UtilityManager _utilitymanager;
            public SystemManager _systemmanager;

            public int tick;

            public ExcavOSContext(Program program, Config config, MyIni storage)
            {
                _program = program;
                _config = config;
                _storage = storage;
                _cargoManager = new CargoManager(_program, _config);
                _systemmanager = new SystemManager(_program, _config);
                _weightAnalizer = new WeightAnalizer(_program, _config, _cargoManager, _systemmanager);
                _utilitymanager = new UtilityManager(_program, _config, _cargoManager, _systemmanager, _storage);
            }

            public void Save()
            {
                _utilitymanager.Save();
            }

            public void Update(TimeSpan time)
            {
                TimeAccumulator = time;
                _cargoManager.QueryData();
                _weightAnalizer.QueryData(time);
                _utilitymanager.Update();
                _systemmanager.Update();
                tick++;
            }

            public void HandleCommand(string argument)
            {
                string[] args = argument.Split(' ');
                switch (args[0].ToLower())
                {
                    case "toggle_gaa":
                        _utilitymanager.GravityAlign = !_utilitymanager.GravityAlign;
                        break;
                    case "set_gaa_pitch":
                        char modifier = args[1][0];
                        float pitch = float.Parse(args[1]);
                        if (modifier.ToString() == "+" || modifier.ToString() == "-")
                        {
                            _utilitymanager.GravityAlignPitch += pitch;
                        }
                        else if (!float.IsNaN(pitch))
                        {
                            _utilitymanager.GravityAlignPitch = pitch;
                        }
                        _utilitymanager.GravityAlignPitch = MathHelper.Clamp(_utilitymanager.GravityAlignPitch, -90, 90);
                        break;
                    case "toggle_cruise":
                        _utilitymanager.CruiseEnabled = !_utilitymanager.CruiseEnabled;
                        if (_utilitymanager.CruiseEnabled)
                        {
                            _utilitymanager.thrustPID.Reset();
                        }
                        else
                        {
                            _systemmanager.CruiseThrusters.ForEach(thruster => thruster.ThrustOverridePercentage = 0.0f);
                            _systemmanager.CruiseReverseThrusters.ForEach(thruster =>
                            {
                                thruster.ThrustOverridePercentage = 0.0f;
                                thruster.Enabled = true;
                            });
                        }
                        break;
                    case "set_cruise":
                        modifier = args[1][0];
                        float speed = float.Parse(args[1]);
                        if (modifier.ToString() == "+" || modifier.ToString() == "-")
                        {
                            _utilitymanager.CruiseTarget += speed;
                        }
                        else if (!float.IsNaN(speed))
                        {
                            _utilitymanager.CruiseTarget = speed;
                        }
                        break;
                    case "dump":
                        _utilitymanager.SetSortersFilter(args[1]);
                        break;
                }
            }
        }

        public class ExcavOSScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "ExcavOS";

            public ExcavOSScreen(ExcavOSContext context) : base(context)
            {
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    Painter.SpriteCentered(Painter.Center, new Vector2(Painter.Height * 0.8f, Painter.Height * 0.8f), "Textures\\FactionLogo\\Miners\\MinerIcon_3.dds");
                    Painter.TextEx(new Vector2(Painter.Center.X, Painter.Center.Y - 60.0f), Painter.BackgroundColor, "ExcavOS", 1.6f);
                }
            }
        }

        public class LoadingScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "LoadingScreen";
            private readonly double loadingStart;
            private readonly double loadingTime;
            private readonly int quotesPerLoading;
            private int currentQuote = 0;
            private string quote;

            public LoadingScreen(ExcavOSContext context) : base(context)
            {
                loadingStart = _context.TimeAccumulator.TotalSeconds;
                loadingTime = 1.0 + _context.Randomizer.Next(1000, 2000) / 1000.0;
                quotesPerLoading = _context.Randomizer.Next(2, 8);
            }

            private string GetInitializationSimsLikeText(double progress)
            {
                int quoteNumber = (int)(progress / (1.0 / quotesPerLoading));
                if (quoteNumber == 0)
                {
                    return "Booting";
                }

                string[] quotes = {
            "Praying to Clang",
            "Couting subgrids",
            "Something something",
            "Calming Clang Wrath",
            "Counting stones in cargo",
            "Doing important stuff",
            "Formatting drive",
            "Generating phantom forces",
            "Connecting dots"
        };
                if (currentQuote != quoteNumber)
                {
                    int quoteIndex = _context.Randomizer.Next(0, quotes.Length);
                    quote = quotes[quoteIndex];
                }

                currentQuote = quoteNumber;
                return quote;
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    float value = (float)((_context.TimeAccumulator.TotalSeconds - loadingStart) / loadingTime);
                    float margin = 20.0f;
                    float bottomYPos = Painter.AvailableSize.Y - margin;

                    Painter.Text(new Vector2(Painter.Center.X, margin), "ExcavOS");
                    Painter.SpriteCentered(Painter.Center, new Vector2(80, 80), "Textures\\FactionLogo\\Miners\\MinerIcon_3.dds");
                    Painter.Text(new Vector2(margin, bottomYPos - margin), GetInitializationSimsLikeText(value) + "...", 0.5f, TextAlignment.LEFT);
                    Painter.ProgressBar(new Vector2(margin, bottomYPos), new Vector2(Painter.AvailableSize.X - margin * 2, 10), value, 2.0f);
                }
            }

            public override bool ShouldDispose()
            {
                return _context.TimeAccumulator.TotalSeconds - loadingStart > loadingTime;
            }
        }

        public class LockScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "LockScreen";

            public LockScreen(ExcavOSContext context) : base(context)
            {
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    Painter.Text(Painter.Center, "No user", 1.0f);
                }
            }
        }

        public class OldPainter
        {
            public const float TEXT_SIZE_PER_UNIT = 18;

            public struct Paint
            {
                public Color PrimaryColor;
                public Color SecondaryColor;
                public Color BackgroundColor;
                public string Font;
                public Vector2 Offset;
                public float AvailableWidth;
                public float AvailableHeight;
                public Vector2 Center;
                //public ScreenSize ScreenSize;
            }

            public static Paint paint;
            public static MySpriteDrawFrame frame;

            public static void SetPaintFromSurface(IMyTextSurface surface, double secondaryColorShade = 0.2f)
            {
                paint.Font = surface.Font;
                paint.PrimaryColor = surface.ScriptForegroundColor;
                paint.BackgroundColor = surface.ScriptBackgroundColor;
                Vector3 hsv = surface.ScriptForegroundColor.ColorToHSV();
                paint.SecondaryColor = (hsv.Z < 0.5f) ? Color.Lighten(paint.PrimaryColor, secondaryColorShade) : Color.Darken(paint.PrimaryColor, secondaryColorShade);
                paint.Offset = new Vector2(0, (surface.TextureSize.Y - surface.SurfaceSize.Y) / 2.0f);
                paint.AvailableWidth = surface.SurfaceSize.X;
                paint.AvailableHeight = surface.SurfaceSize.Y;
                paint.Center = surface.SurfaceSize / 2.0f;
            }

            public static Vector2 TranslateCenterToTopLeftCorner(Vector2 position, Vector2 size)
            {
                return position - size / 2.0f;
            }

            public static Vector2 TranslateTopLeftCornerToCenter(Vector2 position, Vector2 size)
            {
                return position + size / 2.0f;
            }

            public static void DrawText(Vector2 position, Color color, string text, float fontSize = 1.0f, TextAlignment textAlignment = TextAlignment.CENTER)
            {
                MySprite sprite;
                sprite = MySprite.CreateText(text, paint.Font, color, fontSize, textAlignment);
                sprite.Position = position + paint.Offset;
                frame.Add(sprite);
            }

            public static void DrawRectangle(Color color, Vector2 position, Vector2 size, float thickness = 1.0f)
            {
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(size.X, thickness), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(position, sprite.Size.Value) + paint.Offset;
                frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(size.X, thickness), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(new Vector2(position.X, position.Y + size.Y - thickness), sprite.Size.Value) + paint.Offset;
                frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(thickness, size.Y), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(position, sprite.Size.Value) + paint.Offset;
                frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(thickness, size.Y), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(new Vector2(position.X + size.X - thickness, position.Y), sprite.Size.Value) + paint.Offset;
                frame.Add(sprite);
            }

            public static void DrawFilledRectangle(Color color, Vector2 position, Vector2 size)
            {
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: size, color: color)
                {
                    Position = TranslateTopLeftCornerToCenter(position, size) + paint.Offset
                };
                frame.Add(sprite);
            }

            public static void DrawProgressBar(Vector2 position, Vector2 size, double value, float borderThickness = 1.0f, Color? outerColor = null, Color? innerColor = null)
            {
                if (!outerColor.HasValue)
                {
                    outerColor = paint.PrimaryColor;
                }
                if (!innerColor.HasValue)
                {
                    innerColor = paint.SecondaryColor;
                }
                DrawRectangle(outerColor.Value, position, size, borderThickness);
                Vector2 barSize = size - borderThickness * 2.0f;
                if (value < 0.0f) value = 0.0f;
                if (value > 1.0f) value = 1.0f;
                barSize.X *= (float)value;
                DrawFilledRectangle(innerColor.Value, position + borderThickness, barSize);
            }

            public static void DrawSprite(Vector2 position, Vector2 size, string spriteName, Color? color = null)
            {
                if (!color.HasValue)
                {
                    color = paint.PrimaryColor;
                }
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, spriteName, size: size, color: color.Value);
                sprite.Position = TranslateTopLeftCornerToCenter(position, sprite.Size.Value) + paint.Offset;
                frame.Add(sprite);
            }

            public static void DrawVerticalTriBar(Vector2 position, Vector2 size, string min, string current, string maxSoft, string maxHard, float value, float softValue)
            {
                float barWidth = size.X * 0.5f;
                Vector2 pos = new Vector2(position.X, position.Y);

                Vector2 barPosition = new Vector2(position.X + (size.X - barWidth) / 2.0f, position.Y);
                Vector2 barSize = new Vector2(barWidth, size.Y);
                DrawFilledRectangle(paint.SecondaryColor, new Vector2(barPosition.X, barPosition.Y + barSize.Y * (1.0f - softValue)), new Vector2(barSize.X, barSize.Y * softValue));
                DrawRectangle(paint.PrimaryColor, barPosition, barSize);

                float barCount = 9;
                float barPadding = 8.0f;
                float barGap = size.Y / (barCount + 1);
                Vector2 lineSize = new Vector2(barSize.X - 2 * barPadding, 1.0f);
                Vector2 linePosition = new Vector2(barPosition.X + barPadding, barPosition.Y + barGap);
                for (int bar = 0; bar < barCount; bar++)
                {
                    DrawFilledRectangle(paint.PrimaryColor, linePosition, lineSize);
                    linePosition.Y += barGap;
                }

                pos.X = position.X + barWidth + (size.X - barWidth) / 2.0f + 10.0f;
                pos.Y = position.Y;
                DrawText(pos, paint.SecondaryColor, maxHard, 1.0f, TextAlignment.LEFT);

                pos.Y = position.Y + size.Y - TEXT_SIZE_PER_UNIT * 1.0f;
                DrawText(pos, paint.SecondaryColor, min, 1.0f, TextAlignment.LEFT);

                if (maxSoft != maxHard)
                {
                    pos.Y = position.Y + size.Y * (1.0f - softValue) - TEXT_SIZE_PER_UNIT * 1.0f;
                    DrawText(pos, paint.SecondaryColor, maxSoft, 1.0f, TextAlignment.LEFT);
                }

                float markerSize = 20.0f;
                pos.X = position.X + (size.X - barWidth) / 2.0f - 10.0f - markerSize;
                pos.Y = position.Y + size.Y * (1.0f - value) - markerSize / 2.0f;
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, "Triangle", size: new Vector2(markerSize), color: paint.PrimaryColor);
                sprite.Position = TranslateTopLeftCornerToCenter(pos, sprite.Size.Value);
                sprite.RotationOrScale = 1.57079633f;
                frame.Add(sprite);

            }

            public static void DrawButton(Vector2 position, Vector2 size, string label, bool toggled, float fontSize = 1.0f)
            {
                Color color = toggled ? paint.PrimaryColor : paint.SecondaryColor;
                DrawFilledRectangle(color, position, size);
                Vector2 labelPosition = position + size / 2.0f;
                labelPosition.Y -= fontSize * TEXT_SIZE_PER_UNIT;
                DrawText(labelPosition, paint.BackgroundColor, label, fontSize);
            }

        }

        public class Painter
        {
            private static IMyTextSurface _surface;
            private static MySpriteDrawFrame _frame;
            private static Vector2 _offset;

            public static float Width;
            public static float Height;
            public static Vector2 Center;
            public static Vector2 AvailableSize;
            public static Color PrimaryColor;
            public static Color BackgroundColor;
            public static Color SecondaryColor;

            public static void SetCurrentSurfaceAndFrame(IMyTextSurface surface, MySpriteDrawFrame frame)
            {
                _surface = surface;
                _frame = frame;
                _offset = (_surface.TextureSize - _surface.SurfaceSize) / 2.0f;

                Width = _surface.SurfaceSize.X;
                Height = _surface.SurfaceSize.Y;
                Center = new Vector2(Width, Height) / 2.0f + _offset;
                AvailableSize = new Vector2(Width, Height);

                Vector3 hsv = surface.ScriptForegroundColor.ColorToHSV();
                PrimaryColor = _surface.ScriptForegroundColor;
                BackgroundColor = _surface.ScriptBackgroundColor;
                SecondaryColor = (hsv.Z < 0.5f) ? Color.Lighten(PrimaryColor, 0.3f) : Color.Darken(PrimaryColor, 0.3f);
            }

            private static Vector2 TranslateCenterToTopLeftCorner(Vector2 position, Vector2 size)
            {
                return position - size / 2.0f;
            }

            private static Vector2 TranslateTopLeftCornerToCenter(Vector2 position, Vector2 size)
            {
                return position + size / 2.0f;
            }

            public static void RectangleEx(Vector2 position, Vector2 size, float borderThickness = 1.0f, Color? color = null)
            {
                if (!color.HasValue)
                {
                    color = _surface.ScriptForegroundColor;
                }
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(size.X, borderThickness), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(position, sprite.Size.Value) + _offset;
                _frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(size.X, borderThickness), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(new Vector2(position.X, position.Y + size.Y - borderThickness), sprite.Size.Value) + _offset;
                _frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(borderThickness, size.Y), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(position, sprite.Size.Value) + _offset;
                _frame.Add(sprite);
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(borderThickness, size.Y), color: color);
                sprite.Position = TranslateTopLeftCornerToCenter(new Vector2(position.X + size.X - borderThickness, position.Y), sprite.Size.Value) + _offset;
                _frame.Add(sprite);
            }

            public static void FilledRectangleEx(Vector2 position, Vector2 size, Color color, float rotation = 0.0f)
            {
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: size, color: color)
                {
                    Position = TranslateTopLeftCornerToCenter(position, size) + _offset,
                    RotationOrScale = rotation

                };
                _frame.Add(sprite);
            }

            public static void Rectangle(Vector2 position, Vector2 size, float borderThickness = 1.0f)
            {
                RectangleEx(position, size, borderThickness, _surface.ScriptForegroundColor);
            }

            public static void FilledRectangle(Vector2 position, Vector2 size, float rotation = 0.0f)
            {
                FilledRectangleEx(position, size, _surface.ScriptForegroundColor, rotation);
            }

            public static void SpriteCentered(Vector2 position, Vector2 size, string spriteName, Color? color = null, float rotation = 0.0f)
            {
                if (!color.HasValue)
                {
                    color = _surface.ScriptForegroundColor;
                }
                MySprite sprite;
                sprite = new MySprite(SpriteType.TEXTURE, spriteName, size: size, color: color.Value)
                {
                    Position = position,
                    RotationOrScale = rotation
                };
                _frame.Add(sprite);
            }

            public static void Sprite(Vector2 position, Vector2 size, string spriteName, Color? color = null, float rotation = 0.0f)
            {
                SpriteCentered(TranslateTopLeftCornerToCenter(position, size) + _offset, size, spriteName, color, rotation);
            }

            public static void TextEx(Vector2 position, Color color, string text, float fontSize = 1.0f, TextAlignment textAlignment = TextAlignment.CENTER)
            {
                MySprite sprite;
                sprite = MySprite.CreateText(text, _surface.Font, color, fontSize, textAlignment);
                sprite.Position = position + _offset;
                _frame.Add(sprite);
            }

            public static void Text(Vector2 position, string text, float fontSize = 1.0f, TextAlignment textAlignment = TextAlignment.CENTER)
            {
                TextEx(position, _surface.ScriptForegroundColor, text, fontSize, textAlignment);
            }

            public static void Radial(Vector2 position, Vector2 size, float value, string subText = "", int bars = 20, bool flip = false)
            {
                if (value < 0.0f) value = 0;
                if (value > 1.0f) value = 1.0f;
                Color secondary = new Color(SecondaryColor, 0.1f);
                Vector2 barPosition, barSize;
                MySprite sprite;
                barSize = new Vector2(size.X / 256 * 20.0f, size.X / 128 * 4.0f);
                float radius = (size.X - barSize.X) / 2.0f;
                float fontSize = 0.5f + size.X / 256.0f;
                Vector2 origin = new Vector2(position.X + radius, flip ? position.Y + barSize.Y : position.Y + size.Y);
                string text = string.Format("{0:0.00}%", value * 100);
                StringBuilder sb = new StringBuilder();
                sb.Append(text);
                Vector2 mainTextSize = _surface.MeasureStringInPixels(sb, _surface.Font, fontSize);
                sb.Clear();
                sb.Append(subText);
                Vector2 subTextSize = _surface.MeasureStringInPixels(sb, _surface.Font, fontSize / 2.0f);
                for (int n = 0; n <= bars; n++)
                {
                    float angle = -(float)Math.PI / 2.0f + (flip ? -n : n) * ((float)Math.PI / bars);
                    float v = (float)n / bars;
                    float barScale = 0.2f + v * 0.8f;
                    barPosition = new Vector2((float)(radius * Math.Sin(angle)) + barSize.X / 2.0f, -(float)(radius * Math.Cos(angle)) - barSize.Y / 2.0f);
                    sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(barSize.X, barSize.Y * barScale), color: value > v ? _surface.ScriptForegroundColor : secondary)
                    {
                        Position = origin + barPosition + _offset,
                        RotationOrScale = angle + (float)Math.PI / 2.0f
                    };
                    _frame.Add(sprite);
                    Vector2 textPosition = new Vector2(position.X + size.X / 2.0f, flip ? position.Y : origin.Y - mainTextSize.Y);
                    Text(textPosition, text, fontSize);
                    textPosition.Y += flip ? mainTextSize.Y : -subTextSize.Y;
                    TextEx(textPosition, SecondaryColor, subText, fontSize / 2.0f);
                }
            }

            public static void FullRadial(Vector2 position, Vector2 size, float value, string subText = "", int bars = 20)
            {
                if (value < 0.0f) value = 0;
                if (value > 1.0f) value = 1.0f;
                Color secondary = new Color(SecondaryColor, 0.1f);
                Vector2 barPosition, barSize;
                MySprite sprite;
                barSize = new Vector2(size.X / 256 * 20.0f, size.X / 128 * 4.0f);
                float radius = (size.X - barSize.X) / 2.0f;
                float fontSize = 0.5f + size.X / 256.0f;
                Vector2 origin = new Vector2(position.X + radius, position.Y + size.Y);
                string text = string.Format("{0:0.00}%", value * 100);
                StringBuilder sb = new StringBuilder();
                sb.Append(text);
                Vector2 mainTextSize = _surface.MeasureStringInPixels(sb, _surface.Font, fontSize);
                sb.Clear();
                sb.Append(subText);
                Vector2 subTextSize = _surface.MeasureStringInPixels(sb, _surface.Font, fontSize / 2.0f);
                for (int n = 0; n <= bars; n++)
                {
                    float angle = -(float)Math.PI / 2.0f + n * (2.0f * (float)Math.PI / (bars + 1));
                    float v = (float)n / bars;
                    float barScale = 0.2f + v * 0.8f;
                    barPosition = new Vector2((float)(radius * Math.Sin(angle)) + barSize.X / 2.0f, -(float)(radius * Math.Cos(angle)) - barSize.Y / 2.0f);
                    sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: new Vector2(barSize.X, barSize.Y * barScale), color: value > v ? _surface.ScriptForegroundColor : secondary)
                    {
                        Position = origin + barPosition + _offset,
                        RotationOrScale = angle + (float)Math.PI / 2.0f
                    };
                    _frame.Add(sprite);
                    Vector2 textPosition = new Vector2(position.X + size.X / 2.0f, origin.Y - mainTextSize.Y / 2.0f);
                    Text(textPosition, text, fontSize);
                    textPosition.Y += -subTextSize.Y;
                    TextEx(textPosition, SecondaryColor, subText, fontSize / 2.0f);
                }
            }

            public static void ProgressBar(Vector2 position, Vector2 size, float value, float borderThickness = 1.0f, string sprite = "")
            {
                if (value < 0.0f) value = 0.0f;
                if (value > 1.0f) value = 1.0f;
                RectangleEx(position, size, borderThickness, SecondaryColor);
                size -= 2 * borderThickness;
                FilledRectangle(position + borderThickness, new Vector2(size.X * value, size.Y));
                if (sprite != "")
                {
                    Vector2 spriteSize = new Vector2(size.Y, size.Y);
                    Vector2 center = position + size / 2.0f - spriteSize / 2.0f;
                    Sprite(center, spriteSize, sprite, SecondaryColor);
                }
            }

            public static void ProgressBarWithIconAndText(Vector2 position, Vector2 size, float value, float borderThickness = 1.0f, string sprite = "", string text = "")
            {
                if (value < 0.0f) value = 0.0f;
                if (value > 1.0f) value = 1.0f;
                RectangleEx(position, size, borderThickness, SecondaryColor);
                size -= 2 * borderThickness;
                FilledRectangle(position + borderThickness, new Vector2(size.X * value, size.Y));
                if (sprite != "")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(text);
                    Vector2 textSize = _surface.MeasureStringInPixels(sb, _surface.Font, 0.7f);
                    Vector2 spriteSize = new Vector2(size.Y, size.Y);
                    Vector2 rightPos = new Vector2(position.X + size.X, position.Y + (size.Y - textSize.Y) / 2.0f);
                    TextEx(rightPos, SecondaryColor, text, 0.7f, TextAlignment.RIGHT);
                    Vector2 leftPos = new Vector2(position.X, position.Y + (size.Y - spriteSize.Y) / 2.0f);
                    Sprite(leftPos, spriteSize, sprite, SecondaryColor);
                }
            }

            public static void ProgressBarVertical(Vector2 position, Vector2 size, float value, float borderThickness = 1.0f, string sprite = "")
            {
                if (value < 0.0f) value = 0.0f;
                if (value > 1.0f) value = 1.0f;
                RectangleEx(position, size, borderThickness, SecondaryColor);
                size -= 2 * borderThickness;
                Vector2 spriteSize = new Vector2(size.X / 2.0f, (size.X / 2.0f));
                Vector2 center = position + size / 2.0f - spriteSize / 2.0f;

                position += borderThickness;
                FilledRectangle(new Vector2(position.X, position.Y + size.Y * (1.0f - value)), new Vector2(size.X, size.Y * value));
                if (sprite != "")
                {
                    Sprite(center, spriteSize, sprite, SecondaryColor);
                }
            }

        }


        public class PIDController
        {
            public readonly double dt; // i.e. 1.0 / ticks per second
            public double min { get; set; }
            public double max { get; set; }
            public double Kp { get; set; }
            public double Ki
            {
                get { return m_Ki; }
                set { m_Ki = value; }
            }
            private double m_Ki;
            public double Ti
            {
                get { return m_Ki != 0.0 ? Kp / m_Ki : 0.0; }
                set
                {
                    if (value != 0.0)
                    {
                        Ki = Kp / value;
                    }
                    else Ki = 0.0;
                }
            }
            public double Kd
            {
                get { return m_Kd; }
                set { m_Kd = value; m_Kddt = m_Kd / dt; }
            }
            private double m_Kd, m_Kddt;
            public double Td
            {
                get { return Kd / Kp; }
                set { Kd = Kp * value; }
            }
            private double integral = 0.0;
            private double lastError = 0.0;
            public PIDController(double dt)
            {
                this.dt = dt;
                min = -1.0;
                max = 1.0;
            }
            public void Reset()
            {
                integral = 0.0;
                lastError = 0.0;
            }
            public double Compute(double error)
            {
                var newIntegral = integral + error * dt;
                var derivative = error - lastError;
                lastError = error;
                var CV = ((Kp * error) +
                (m_Ki * newIntegral) +
                (m_Kddt * derivative));
                if (CV > max)
                {
                    if (newIntegral <= integral) integral = newIntegral;
                    return max;
                }
                else if (CV < min)
                {
                    if (newIntegral >= integral) integral = newIntegral;
                    return min;
                }
                integral = newIntegral;
                return CV;
            }
        }

        public class RegisteredProvider : ScriptConfig
        {
            private IMyTerminalBlock _block;
            private ExcavOSContext _context;
            private readonly Dictionary<int, ScreenHandler<ExcavOSContext>> _screenHandlers = new Dictionary<int, ScreenHandler<ExcavOSContext>>();
            private readonly Dictionary<int, ScreenHandler<ExcavOSContext>> _immersiveHandlers = new Dictionary<int, ScreenHandler<ExcavOSContext>>();
            public bool WasUnderControl = false;
            private bool EnableImmersion = false;

            public RegisteredProvider(ExcavOSContext context, IMyTerminalBlock block, MyIni ini, string section) : base(ini, section)
            {
                _block = block;
                _context = context;
            }

            public override void ReadConfig()
            {
                EnableImmersion = GetValue("EnableImmersion").ToBoolean(EnableImmersion);
                IMyTextSurfaceProvider surfaceProvider = _block as IMyTextSurfaceProvider;
                for (int n = 0; n < surfaceProvider.SurfaceCount; n++)
                {
                    string key = $"Surface{n}";
                    if (KeyExists(key))
                    {
                        string screen = GetValue(key).ToString();
                        SetScreenHandlerForSurface(screen, n);
                    }
                    else if (_screenHandlers.ContainsKey(n))
                    {
                        ResetScreenHandlerForSurface(n);
                    }
                }

            }

            public override void SetupDefaults()
            {

            }

            public void Update()
            {
                if (!_block.IsWorking)
                {
                    return;
                }

                IMyTextSurfaceProvider surfaceProvider = _block as IMyTextSurfaceProvider;

                if (EnableImmersion && _block is IMyCockpit)
                {
                    IMyCockpit cockpit = _block as IMyCockpit;
                    if (!WasUnderControl && cockpit.IsUnderControl)
                    {
                        // player entered cockpit
                        WasUnderControl = true;
                        for (int n = 0; n < surfaceProvider.SurfaceCount; n++)
                        {
                            if (_screenHandlers.ContainsKey(n))
                            {
                                _immersiveHandlers[n] = new LoadingScreen(_context);
                            }
                        }
                    }
                    else if (WasUnderControl && !cockpit.IsUnderControl)
                    {
                        // player exited cockpit
                        WasUnderControl = false;
                        for (int n = 0; n < surfaceProvider.SurfaceCount; n++)
                        {
                            if (_screenHandlers.ContainsKey(n))
                            {
                                _immersiveHandlers[n] = new LockScreen(_context);
                            }
                        }
                    }
                }


                for (int n = 0; n < surfaceProvider.SurfaceCount; n++)
                {
                    IMyTextSurface surface = surfaceProvider.GetSurface(n);
                    if (_screenHandlers.ContainsKey(n))
                    {
                        surface.Script = "";
                        surface.ContentType = ContentType.SCRIPT;
                        if (_immersiveHandlers.ContainsKey(n))
                        {
                            _immersiveHandlers[n].Draw(surface);
                            if (_immersiveHandlers[n].ShouldDispose())
                            {
                                _immersiveHandlers.Remove(n);
                            }
                        }
                        else
                        {
                            _screenHandlers[n].Draw(surface);
                        }
                    }
                }
            }

            public void SetScreenHandlerForSurface(string screenName, int surfaceIndex)
            {
                _screenHandlers[surfaceIndex] = ScreenHandlerFactory.GetScreenHandler(screenName, _context);
            }

            public void ResetScreenHandlerForSurface(int surfaceIndex)
            {
                _screenHandlers.Remove(surfaceIndex);
                IMyTextSurfaceProvider surfaceProvider = _block as IMyTextSurfaceProvider;
                IMyTextSurface surface = surfaceProvider.GetSurface(surfaceIndex);
                surface.Script = "";
                surface.ContentType = ContentType.NONE;
            }

            public bool HasSurfaces()
            {
                return _screenHandlers.Count > 0;
            }

        }

        abstract public class ScreenHandler<T>
        {
            public const string SCREEN_NAME = "BlankScreen";
            protected readonly T _context;
            public ScreenHandler(T context)
            {
                _context = context;
            }

            public abstract void Draw(IMyTextSurface surface);

            public virtual bool ShouldDispose()
            {
                return false;
            }
        }

        public class ScreenHandlerFactory
        {
            private static Dictionary<string, ScreenHandler<ExcavOSContext>> _handlers = new Dictionary<string, ScreenHandler<ExcavOSContext>>();
            public static ScreenHandler<ExcavOSContext> GetScreenHandler(string name, ExcavOSContext context)
            {
                if (_handlers.ContainsKey(name))
                {
                    return _handlers[name];
                }

                ScreenHandler<ExcavOSContext> handler;
                switch (name)
                {
                    case ExcavOSScreen.SCREEN_NAME:
                        handler = new ExcavOSScreen(context);
                        break;
                    case CargoScreen.SCREEN_NAME:
                        handler = new CargoScreen(context);
                        break;
                    case WeightScreen.SCREEN_NAME:
                        handler = new WeightScreen(context);
                        break;
                    case UtilityScreen.SCREEN_NAME:
                        handler = new UtilityScreen(context);
                        break;
                    case AllCargoScreen.SCREEN_NAME:
                        handler = new AllCargoScreen(context);
                        break;
                    default:
                        handler = new BlankScreen(context);
                        break;
                }
                _handlers.Add(name, handler);
                return handler;

            }
        }

        abstract public class ScriptConfig
        {
            protected readonly MyIni _ini;
            protected readonly string _section;

            public ScriptConfig(MyIni ini, string section)
            {
                _ini = ini;
                _section = section;
            }

            protected MyIniValue GetValue(string key)
            {
                return _ini.Get(_section, key);
            }

            protected void SetValue(string key, string value)
            {
                _ini.Set(_section, key, value);
            }

            protected void SetValue(string key, float value)
            {
                _ini.Set(_section, key, value);
            }

            protected void SetValue(string key, int value)
            {
                _ini.Set(_section, key, value);
            }

            protected void SetValue(string key, bool value)
            {
                _ini.Set(_section, key, value);
            }

            protected bool KeyExists(string key)
            {
                return _ini.ContainsKey(_section, key);
            }

            public void LoadConfig(string blob)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(blob, _section, out result))
                {
                    return;
                }

                if (!_ini.ContainsSection(_section))
                {
                    return;
                }

                ReadConfig();
            }

            abstract public void SetupDefaults();
            abstract public void ReadConfig();

        }

        abstract public class ScriptHandler
        {
            protected Program _program;
            private int _tick;
            private int _tick10;
            private readonly string _spinner = "|/-\\|/-\\";
            protected readonly string _scriptName;
            private readonly string _scriptVersion;
            protected readonly MyIni _ini = new MyIni();
            protected readonly MyIni _storage;
            protected Config _config;
            protected TimeSpan _timeAccumulator = new TimeSpan();

            public ScriptHandler(Program program, MyIni storage, string scriptName, string scriptVersion)
            {
                _program = program;
                _storage = storage;
                _scriptName = scriptName;
                _scriptVersion = scriptVersion;
                _config = new Config(_ini, _scriptName);
            }

            public void Update(string argument, UpdateType updateSource, TimeSpan time)
            {
                _timeAccumulator += time;
                if (updateSource == UpdateType.Update100)
                {
                    _tick++;

                    _program.Echo($"{_scriptName} (ver. {_scriptVersion}) is running {_spinner.Substring(_tick % _spinner.Length, 1)}\nLast run time: {_program.Runtime.LastRunTimeMs}ms");

                    if (_tick % 5 == 0)
                    {
                        Initialize();
                    }

                }
                else if (updateSource == UpdateType.Update10)
                {
                    _tick10++;
                    if (_tick10 % 3 == 0)
                    {
                        Update10();
                    }

                }
                else if (argument != "")
                {
                    HandleCommand(argument);
                }
            }

            private void CreateConfig()
            {
                if (MyIni.HasSection(_program.Me.CustomData, _scriptName))
                {
                    return;
                }

                _ini.Clear();
                _config.SetupDefaults();
                _program.Me.CustomData = _ini.ToString();

            }

            protected void Initialize()
            {
                CreateConfig();
                _config.LoadConfig(_program.Me.CustomData);
                FetchBlocks();
            }

            abstract public void FetchBlocks();
            protected virtual void Update10()
            {

            }

            protected virtual void HandleCommand(string argument)
            {

            }
        }

        public class SystemManager
        {

            public string Status;
            private Program _program;
            private Config _config;

            private readonly BlockFinder<IMyGyro> _gyros;
            private readonly BlockFinder<IMyShipController> _controllers;
            private IMyShipController _controller;

            private readonly BlockFinder<IMyThrust> _liftThrusters;
            private readonly BlockFinder<IMyThrust> _cruiseThrusters;
            private readonly BlockFinder<IMyThrust> _cruiseReverseThrusters;
            private readonly BlockFinder<IMyThrust> _stopThrusters;

            public IMyShipController ActiveController { get { return _controller; } }
            public List<IMyThrust> LiftThrusters { get { return _liftThrusters.blocks; } }
            public List<IMyThrust> StopThrusters { get { return _stopThrusters.blocks; } }
            public List<IMyThrust> CruiseThrusters { get { return _cruiseThrusters.blocks; } }
            public List<IMyThrust> CruiseReverseThrusters { get { return _cruiseReverseThrusters.blocks; } }

            public SystemManager(Program program, Config config)
            {
                _program = program;
                _config = config;
                _gyros = new BlockFinder<IMyGyro>(_program);
                _controllers = new BlockFinder<IMyShipController>(_program);
                _liftThrusters = new BlockFinder<IMyThrust>(_program);
                _stopThrusters = new BlockFinder<IMyThrust>(_program);
                _cruiseThrusters = new BlockFinder<IMyThrust>(_program);
                _cruiseReverseThrusters = new BlockFinder<IMyThrust>(_program);
            }

            public void Update()
            {
                UpdateController();
                UpdateThrusterGroups();
            }
            private void UpdateController()
            {
                IMyShipController firstWorking = null;
                _controllers.FindBlocks(true, null, "");
                foreach (IMyShipController _controller in _controllers.blocks)
                {
                    if (!_controller.IsWorking) continue;
                    if (firstWorking == null) firstWorking = _controller;
                    if (this._controller == null && _controller.IsUnderControl && _controller.CanControlShip) this._controller = _controller;
                    if (_controller.IsMainCockpit) this._controller = _controller;
                }
                if (_controller == null) _controller = firstWorking;

                if (_controller == null)
                {
                    throw new Exception("Missing Controller!");
                }
            }
            private void UpdateThrusterGroups()
            {
                if (_controller == null) return;
                if (_config.LiftThrustersGroupName != "")
                {
                    _liftThrusters.FindBlocks(true, null, _config.LiftThrustersGroupName);
                }
                else
                {
                    _liftThrusters.FindBlocks(true, thruster =>
                    {
                        Vector3D thrusterDirection = -thruster.WorldMatrix.Forward;
                        //double forwardDot = Vector3D.Dot(thrusterDirection, _controller.WorldMatrix.Forward);
                        double upDot = Vector3D.Dot(thrusterDirection, -Vector3.Normalize(_controller.GetTotalGravity()));
                        //double leftDot = Vector3D.Dot(thrusterDirection, _controller.WorldMatrix.Left);

                        if (upDot >= 0.2)
                        {
                            return true;
                        }
                        return false;
                    });
                }
                if (_config.StopThrustersGroupName != "")
                {
                    _stopThrusters.FindBlocks(true, null, _config.StopThrustersGroupName);
                }
                else
                {
                    _stopThrusters.FindBlocks(true, thruster =>
                    {
                        Vector3D thrusterDirection = -thruster.WorldMatrix.Forward;
                        double forwardDot = Vector3D.Dot(thrusterDirection, _controller.GetShipVelocities().LinearVelocity);
                        //double upDot = Vector3D.Dot(thrusterDirection, _controller.WorldMatrix.Up);
                        //double leftDot = Vector3D.Dot(thrusterDirection, _controller.WorldMatrix.Left);

                        if (forwardDot <= -0.7)
                        {
                            return true;
                        }
                        return false;
                    });
                }
                _cruiseThrusters.FindBlocks(true, thruster =>
                {
                    var facing = thruster.Orientation.TransformDirection(Base6Directions.Direction.Forward);
                    return facing == Base6Directions.Direction.Backward;
                });
                _cruiseReverseThrusters.FindBlocks(true, thruster =>
                {
                    var facing = thruster.Orientation.TransformDirection(Base6Directions.Direction.Forward);
                    return facing == Base6Directions.Direction.Forward;
                });
            }
        }

        public class UtilityManager
        {
            private string sectionKey = "UtilMan";
            private Program _program;
            private Config _config;
            private MyIni _storage;
            public string Status;
            private CargoManager _cargoManager;
            private SystemManager _systemManager;

            private readonly BlockFinder<IMyGyro> _gyros;
            private readonly BlockFinder<IMyShipController> _controllers;
            private readonly BlockFinder<IMyConveyorSorter> _sorters;
            private readonly BlockFinder<IMyBatteryBlock> _batteries;
            private readonly BlockFinder<IMyGasTank> _hydrogenTanks;
            private readonly BlockFinder<IMyReactor> _reactors;
            private readonly List<MyInventoryItemFilter> sorterList = new List<MyInventoryItemFilter>();
            public readonly PIDController thrustPID = new PIDController(1.0 / 60.0);

            private const double ThrustKp = 0.5;
            private const double ThrustTi = 0.1;
            private const double ThrustTd = 0.0;

            public bool GravityAlign = false;
            public bool CruiseEnabled = false;
            private bool _GravityAlignActive = false;
            public float GravityAlignPitch = 0;
            public float CruiseTarget = 0;
            public string BatteryCharge = "";
            public string HydrogenCharge = "";
            public double BatteryLevel = 0;
            public double HydrogenLevel = 0;
            public double UraniumLevel = 0;

            public UtilityManager(Program program, Config config, CargoManager cargoManager, SystemManager systemManager, MyIni storage)
            {
                _program = program;
                _config = config;
                _storage = storage;
                _cargoManager = cargoManager;
                _systemManager = systemManager;
                _gyros = new BlockFinder<IMyGyro>(_program);
                _sorters = new BlockFinder<IMyConveyorSorter>(_program);
                _controllers = new BlockFinder<IMyShipController>(_program);
                _batteries = new BlockFinder<IMyBatteryBlock>(_program);
                _hydrogenTanks = new BlockFinder<IMyGasTank>(_program);
                _reactors = new BlockFinder<IMyReactor>(_program);
                thrustPID.Kp = ThrustKp;
                thrustPID.Ti = ThrustTi;
                thrustPID.Td = ThrustTd;
                Initialize();
            }

            public void Save()
            {
                _storage.Set(sectionKey, "GAP", GravityAlignPitch);
                _storage.Set(sectionKey, "CruiseTarget", CruiseTarget);
            }

            protected void Initialize()
            {
                GravityAlignPitch = (float)_storage.Get(sectionKey, "GAP").ToDouble(0);
                CruiseTarget = (float)_storage.Get(sectionKey, "CruiseTarget").ToDouble(0);
            }

            public void Update()
            {
                _gyros.FindBlocks(true, null, _config.AlignGyrosGroupName);
                _sorters.FindBlocks(true, null, _config.DumpSortersGroupName);
                _controllers.FindBlocks(true, null);
                _batteries.FindBlocks(true, null);
                _reactors.FindBlocks(true, null);
                _hydrogenTanks.FindBlocks(true, tank =>
                {
                    return BlockHelper.IsHydrogenTank(tank);
                });
                CalculateCharge();
                CalculateHydrogen();
                CalculateUranium();

                IMyShipController controller = null;
                IMyShipController firstWorking = null;
                foreach (IMyShipController _controller in _controllers.blocks)
                {
                    if (!_controller.IsWorking) continue;
                    if (firstWorking == null) firstWorking = _controller;
                    if (controller == null && _controller.IsUnderControl && _controller.CanControlShip) controller = _controller;
                    if (_controller.IsMainCockpit) controller = _controller;
                }
                if (controller == null) controller = firstWorking;

                if (controller == null)
                {
                    Status = "Missing controller";
                    return;
                }

                if (_gyros.Count() == 0)
                {
                    Status = "Missing gyros";
                    return;
                }

                Status = "";

                if (GravityAlign)
                {
                    _GravityAlignActive = true;
                    DoGravityAlign(controller, _gyros.blocks, GravityAlignPitch);
                }
                if (!GravityAlign && _GravityAlignActive)
                {
                    ReleaseGyros(_gyros.blocks);
                }

                if (CruiseEnabled)
                {
                    double currentSpeed = controller.GetShipSpeed();

                    var error = CruiseTarget - currentSpeed;
                    var force = thrustPID.Compute(error);
                    _systemManager.CruiseThrusters.ForEach(thruster =>
                    {
                        if (Math.Abs(error) < 0.02f * CruiseTarget)
                        {
                            thruster.ThrustOverridePercentage = 0.0f;
                        }
                        else if (force > 0.0)
                        {
                            thruster.Enabled = true;
                            thruster.ThrustOverridePercentage = (float)force * 0.1f;
                        }
                        else
                        {
                            thruster.ThrustOverridePercentage = 0.0f;
                            thruster.Enabled = false;
                        }
                    });

                    _systemManager.CruiseReverseThrusters.ForEach(thruster =>
                    {
                        if (Math.Abs(error) < 0.02f * CruiseTarget)
                        {
                            thruster.ThrustOverridePercentage = 0.0f;
                            thruster.Enabled = false;
                        }
                        else if (force > 0.0)
                        {
                            thruster.ThrustOverridePercentage = 0.0f;
                            thruster.Enabled = false;
                        }
                        else
                        {
                            thruster.Enabled = true;
                            thruster.ThrustOverridePercentage = -(float)force * 0.1f;
                        }
                    });

                }
            }

            public void SetSortersFilter(string item_id)
            {
                try
                {
                    List<MyInventoryItemFilter> currentList = new List<MyInventoryItemFilter>();
                    MyInventoryItemFilter item = new MyInventoryItemFilter("MyObjectBuilder_Ore/" + item_id);
                    _sorters.ForEach(sorter =>
                    {
                        sorter.GetFilterList(currentList);
                        if (currentList.Contains(item))
                        {
                            currentList.Remove(item);
                        }
                        else
                        {
                            currentList.Add(item);
                        }
                        sorter.SetFilter(MyConveyorSorterMode.Whitelist, currentList);
                    });
                }
                catch (Exception)
                {
                }
            }

            public string GetSortersFilter()
            {
                if (_sorters.Count() == 0)
                {
                    return "No sorters";
                }

                string firstItem = "";
                foreach (var sorter in _sorters.blocks)
                {
                    sorter.GetFilterList(sorterList);
                    if (sorterList.Count == 0)
                    {
                        continue;
                    }
                    if (sorterList.Count > 1)
                    {
                        return "Multiple items";
                    }
                    else
                    {
                        if (firstItem == "")
                        {
                            firstItem = sorterList[0].ItemId.ToString();
                        }
                        if (firstItem != sorterList[0].ItemId.ToString())
                        {
                            return "Multiple items";
                        }
                    }
                }
                if (firstItem == "")
                {
                    return "No filters";
                }
                return firstItem.Replace("MyObjectBuilder_Ore/", "");
            }

            private void CalculateHydrogen()
            {
                if (_hydrogenTanks.Count() == 0)
                {
                    HydrogenCharge = "N/A";
                    HydrogenLevel = 0;
                    return;
                }
                double total = 0;
                double capacity = 0;
                _hydrogenTanks.ForEach(tank =>
                {
                    total += tank.FilledRatio * tank.Capacity;
                    capacity += tank.Capacity;
                });
                HydrogenLevel = total / capacity;
                HydrogenCharge = string.Format("{0:0.0}%", (total / capacity) * 100);
            }

            private void CalculateCharge()
            {
                float stored = 0;
                float max = 0;
                float delta = 0;
                _batteries.ForEach(battery =>
                {
                    if (!battery.IsWorking)
                    {
                        return;
                    }

                    delta += battery.CurrentInput - battery.CurrentOutput;
                    stored += battery.CurrentStoredPower;
                    max += battery.MaxStoredPower;
                });

                if (max > 0)
                {
                    BatteryCharge = string.Format("{0:0.0}%", (stored / max) * 100);
                    BatteryLevel = stored / max;
                }
                else
                {
                    BatteryCharge = "N/A";
                    BatteryLevel = 0;
                }
            }

            private void CalculateUranium()
            {
                if (_reactors.Count() == 0)
                {
                    //ReactorsCharge = "N/A";
                    UraniumLevel = 0;
                    return;
                }
                double total = 0;
                _reactors.ForEach(reactor =>
                {
                    total += BlockHelper.GetReactorFuelLevel(reactor);
                });
                UraniumLevel = total / _reactors.Count();
            }
            private double DoGravityAlign(IMyShipController controller, List<IMyGyro> gyrosToUse, float pitch = 0f, bool onlyCalculate = false)
            {

                // Thanks to https://forum.keenswh.com/threads/aligning-ship-to-planet-gravity.7373513/#post-1286885461

                double coefficient = 0.9;
                Matrix orientation;
                controller.Orientation.GetMatrix(out orientation);
                Vector3D down = orientation.Down;
                if (pitch < 0)
                {
                    down = Vector3D.Lerp(orientation.Down, orientation.Forward, -pitch / 90);
                }
                else if (pitch > 0)
                {
                    down = Vector3D.Lerp(orientation.Down, -orientation.Forward, pitch / 90);
                }

                Vector3D gravity = controller.GetNaturalGravity();
                gravity.Normalize();

                double offLevel = 0.0;

                foreach (var gyro in gyrosToUse)
                {
                    gyro.Orientation.GetMatrix(out orientation);
                    var localDown = Vector3D.Transform(down, MatrixD.Transpose(orientation));
                    var localGrav = Vector3D.Transform(gravity, MatrixD.Transpose(gyro.WorldMatrix.GetOrientation()));

                    var rotation = Vector3D.Cross(localDown, localGrav);
                    double ang = rotation.Length();
                    ang = Math.Atan2(ang, Math.Sqrt(Math.Max(0.0, 1.0 - ang * ang)));

                    if (ang < 0.01)
                    {
                        gyro.GyroOverride = false;
                        continue;
                    }
                    offLevel += ang * 180.0 / 3.14;

                    if (!onlyCalculate)
                    {
                        double controlVelocity = gyro.GetMaximum<float>("Yaw") * (ang / Math.PI) * coefficient;
                        controlVelocity = Math.Min(gyro.GetMaximum<float>("Yaw"), controlVelocity);
                        controlVelocity = Math.Max(0.01, controlVelocity); //Gyros don't work well at very low speeds
                        rotation.Normalize();
                        rotation *= controlVelocity;
                        gyro.SetValueFloat("Pitch", (float)rotation.GetDim(0));
                        gyro.SetValueFloat("Yaw", -(float)rotation.GetDim(1));
                        gyro.SetValueFloat("Roll", -(float)rotation.GetDim(2));
                        gyro.SetValueFloat("Power", 1.0f);
                        gyro.GyroOverride = true;
                    }
                }
                if (gyrosToUse.Count() == 0)
                {
                    return -1000;
                }
                return offLevel / gyrosToUse.Count();
            }
            private void ReleaseGyros(List<IMyGyro> gyros)
            {
                foreach (IMyGyro gyro in gyros)
                {
                    gyro.SetValueFloat("Pitch", 0f);
                    gyro.SetValueFloat("Yaw", 0f);
                    gyro.SetValueFloat("Roll", 0f);
                    gyro.SetValueFloat("Power", 1.0f);
                    gyro.GyroOverride = false;
                }
            }
        }


        public class UtilityScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "Utility";

            private readonly StringBuilder sb = new StringBuilder();

            public UtilityScreen(ExcavOSContext context) : base(context)
            {
            }

            public override void Draw(IMyTextSurface surface)
            {
                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);
                    float margin = Painter.Width >= 512.0f ? 25.0f : 5.0f;
                    float gap = Painter.Width >= 512.0f ? 10.0f : 2.0f;
                    float fontSize = (Painter.Width >= 512.0f ? 1.0f : 0.8f) * surface.FontSize;
                    sb.Clear();
                    sb.Append("Xy");
                    Vector2 textSize = surface.MeasureStringInPixels(sb, surface.Font, fontSize);
                    Vector2 position = new Vector2(margin, margin);
                    Vector2 barSize = new Vector2(Painter.Width - margin * 2, Painter.Width >= 512.0f ? 2.0f : 1.0f);

                    Painter.Text(position, "Gravity Align", fontSize, TextAlignment.LEFT);
                    if (_context._utilitymanager.Status == "")
                    {
                        float pitch = _context._utilitymanager.GravityAlignPitch;
                        string status;
                        if (pitch == 0) status = "(Level)";
                        else if (pitch == 90) status = "(Up)";
                        else if (pitch == -90) status = "(Down)";
                        else status = string.Format("({0}°)", pitch);
                        Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), (_context._utilitymanager.GravityAlign ? Painter.PrimaryColor : Painter.SecondaryColor), string.Format("{0} {1}", (_context._utilitymanager.GravityAlign ? "On" : "Off"), status), fontSize, TextAlignment.RIGHT);
                    }
                    else
                    {
                        Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._utilitymanager.Status, fontSize, TextAlignment.RIGHT);
                    }

                    position.Y += textSize.Y + gap;
                    Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                    position.Y += gap;

                    Painter.Text(position, "Cruise Control", fontSize, TextAlignment.LEFT);
                    if (_context._utilitymanager.CruiseEnabled)
                    {
                        Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.PrimaryColor, string.Format("On ({0} m/s)", _context._utilitymanager.CruiseTarget), fontSize, TextAlignment.RIGHT);
                    }
                    else
                    {
                        Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, string.Format("Off ({0} m/s)", _context._utilitymanager.CruiseTarget), fontSize, TextAlignment.RIGHT);
                    }


                    position.Y += textSize.Y + gap;
                    Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                    position.Y += gap;

                    Painter.Text(position, "Stop", fontSize, TextAlignment.LEFT);
                    if (_context._weightAnalizer.Status == "")
                    {
                        if (_context._weightAnalizer.StoppingDistance > 0)
                        {
                            string w = _context._weightAnalizer.StopThrustersWarning ? " (!)" : "";
                            string s = string.Format("{0:0.00}m @ {1:0.00}s{2}", _context._weightAnalizer.StoppingDistance, _context._weightAnalizer.StoppingTime, w);
                            Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, s, fontSize, TextAlignment.RIGHT);
                        }
                        else
                        {
                            Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, "-", fontSize, TextAlignment.RIGHT);
                        }
                    }
                    else
                    {
                        Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._weightAnalizer.Status, fontSize, TextAlignment.RIGHT);
                    }

                    position.Y += textSize.Y + gap;
                    Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                    position.Y += gap;

                    Painter.Text(position, "Jettison", fontSize, TextAlignment.LEFT);
                    Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._utilitymanager.GetSortersFilter(), fontSize, TextAlignment.RIGHT);

                    position.Y += textSize.Y + gap;
                    Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                    position.Y += gap;


                    float maxWidth = (barSize.X - (4 * gap)) / 3;
                    //float maxHeight = Painter.Height - 2 * margin - position.Y;
                    float maxHeight = textSize.Y + gap;
                    position.X += gap;
                    Painter.ProgressBarWithIconAndText(position, new Vector2(maxWidth, maxHeight), (float)_context._utilitymanager.BatteryLevel, 1.0f, "IconEnergy", _context._utilitymanager.BatteryCharge);
                    position.X += maxWidth + gap;

                    Painter.ProgressBarWithIconAndText(position, new Vector2(maxWidth, maxHeight), (float)_context._utilitymanager.HydrogenLevel, 1.0f, "IconHydrogen", _context._utilitymanager.HydrogenCharge);
                    position.X += maxWidth + gap;

                    Painter.ProgressBar(position, new Vector2(maxWidth, maxHeight), (float)_context._utilitymanager.UraniumLevel * 1000, 1.0f, "MyObjectBuilder_Ingot/Uranium");

                    /*
                            Painter.Text(position, "Batteries", fontSize, TextAlignment.LEFT);
                            Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._utilitymanager.BatteryCharge, fontSize, TextAlignment.RIGHT);

                            position.Y += textSize.Y + gap;
                            Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                            position.Y += gap;

                            Painter.Text(position, "Hydrogen", fontSize, TextAlignment.LEFT);
                            Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._utilitymanager.HydrogenLevel, fontSize, TextAlignment.RIGHT);

                            position.Y += textSize.Y + gap;
                            Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                            position.Y += gap;

                            Painter.Text(position, "Uranium", fontSize, TextAlignment.LEFT);
                            Painter.TextEx(new Vector2(Painter.Width - margin, position.Y), Painter.SecondaryColor, _context._utilitymanager.HydrogenLevel, fontSize, TextAlignment.RIGHT);

                            position.Y += textSize.Y + gap;
                            Painter.FilledRectangleEx(position, barSize, Painter.SecondaryColor);
                            position.Y += gap;
                            */
                }
            }
        }

        public class WeightAnalizer
        {
            private Program _program;
            private Config _config;
            public string Status;
            private CargoManager _cargoManager;
            private SystemManager _systemManager;

            public float LiftThrustNeeded;
            public float LiftThrustAvailable;
            public float StoppingDistance;
            public float StoppingTime;
            public float CapacityDelta;
            public bool StopThrustersWarning = false;

            protected struct WeightPoint
            {
                public double time;
                public double capacity;
            }
            private const int MaxWeightPoints = 20;
            private WeightPoint[] _weightPoints = new WeightPoint[MaxWeightPoints];
            private int addedWeightPoints = 0;

            public WeightAnalizer(Program program, Config config, CargoManager cargoManager, SystemManager systemManager)
            {
                _program = program;
                _config = config;
                _cargoManager = cargoManager;
                _systemManager = systemManager;
            }

            public void QueryData(TimeSpan time)
            {
                Calculate();
                CalculateCapacityDelta(time);
            }

            public float GetLiftThresholdWarning()
            {
                return _config.LiftThresholdWarning;
            }

            private void Calculate()
            {

                if (_systemManager.ActiveController == null)
                {
                    Status = "Missing controller";
                    return;
                }

                if (_systemManager.ActiveController.CalculateShipMass().PhysicalMass == 0)
                {
                    Status = "Grid is static";
                    LiftThrustNeeded = 0;
                    LiftThrustAvailable = 0;
                    StoppingTime = 0;
                    StoppingDistance = 0;
                    return;
                }

                Status = "";
                CalculateLiftThrustUsage(_systemManager.ActiveController, _systemManager.LiftThrusters);
                CalculateStopDistance(_systemManager.ActiveController, _systemManager.StopThrusters);
            }

            private void CalculateLiftThrustUsage(IMyShipController controller, List<IMyThrust> thrusters)
            {
                float mass = controller.CalculateShipMass().PhysicalMass;
                float gravityStrength = (float)(controller.GetNaturalGravity().Length() / 9.81);
                LiftThrustNeeded = (mass * (float)gravityStrength / 100) * 1000;

                Vector3 gravity = controller.GetNaturalGravity();


                LiftThrustAvailable = 0;
                thrusters.ForEach(thruster =>
                {
                    if (thruster.IsWorking)
                    {
                        Vector3D thrusterDirection = thruster.WorldMatrix.Forward;
                        double upDot = Vector3D.Dot(thrusterDirection, Vector3.Normalize(gravity));
                        LiftThrustAvailable += (thruster.MaxEffectiveThrust * (float)upDot);
                    }
                });

            }

            private void CalculateStopDistance(IMyShipController controller, List<IMyThrust> thrusters)
            {
                float mass = controller.CalculateShipMass().PhysicalMass;
                double stopThrustAvailable = 0;
                int disabledThrusters = 0;
                thrusters.ForEach(thruster =>
                {
                    if (!thruster.IsWorking) disabledThrusters++;
                    if (thruster.IsFunctional)
                    {
                        stopThrustAvailable += thruster.MaxEffectiveThrust;
                    }
                });
                StopThrustersWarning = disabledThrusters > 0;
                double deacceleration = -stopThrustAvailable / mass;
                double currentSpeed = controller.GetShipSpeed();
                StoppingTime = (float)(-currentSpeed / deacceleration);
                StoppingDistance = (float)(currentSpeed * StoppingTime + (deacceleration * StoppingTime * StoppingTime) / 2.0f);
            }

            private void CalculateCapacityDelta(TimeSpan time)
            {
                WeightPoint wp = new WeightPoint
                {
                    time = time.TotalSeconds,
                    capacity = _cargoManager.CurrentCapacity / _cargoManager.TotalCapacity
                };

                if (addedWeightPoints < MaxWeightPoints)
                {
                    _weightPoints[addedWeightPoints] = wp;
                    addedWeightPoints++;
                    CapacityDelta = 0;
                }
                else
                {
                    for (int n = 1; n < MaxWeightPoints; n++)
                    {
                        _weightPoints[n - 1] = _weightPoints[n];
                    }
                    _weightPoints[MaxWeightPoints - 1] = wp;

                    float capacityIncrease = (float)(_weightPoints[MaxWeightPoints - 1].capacity - _weightPoints[0].capacity);
                    float timeIncrease = (float)(_weightPoints[MaxWeightPoints - 1].time - _weightPoints[0].time);
                    CapacityDelta = capacityIncrease / timeIncrease;
                }

            }
        }


        public class WeightScreen : ScreenHandler<ExcavOSContext>
        {
            public new const string SCREEN_NAME = "Weight";

            public WeightScreen(ExcavOSContext context) : base(context)
            {

            }

            public override void Draw(IMyTextSurface surface)
            {

                using (var frame = surface.DrawFrame())
                {
                    Painter.SetCurrentSurfaceAndFrame(surface, frame);

                    bool roverMode = _context._systemmanager.LiftThrusters.Count == 0;
                    float liftUsage = _context._weightAnalizer.LiftThrustNeeded / _context._weightAnalizer.LiftThrustAvailable;
                    float cargoUsage = (float)(_context._cargoManager.CurrentCapacity / _context._cargoManager.TotalCapacity);
                    float margin = 20.0f;
                    float max = Math.Min(Painter.AvailableSize.X, Painter.AvailableSize.Y);
                    bool shortMode = max < Painter.AvailableSize.X;

                    if (roverMode)
                    {
                        Vector2 position = new Vector2((Painter.AvailableSize.X - max) / 2.0f + margin, margin);
                        Vector2 size = new Vector2(max - margin * 2, max / 2.0f - margin);
                        string subText = shortMode ? "Cargo" : "Cargo capacity";
                        if (_context._weightAnalizer.CapacityDelta > 0.0001)
                        {
                            float timeLeft = (float)((1.0f - cargoUsage) / _context._weightAnalizer.CapacityDelta);
                            subText = string.Format("+{0:0.00}% @ {1:0.0}s", _context._weightAnalizer.CapacityDelta * 100, timeLeft);
                        }
                        Painter.FullRadial(position, size, cargoUsage, subText, 60);

                    }
                    else
                    {
                        Vector2 position = new Vector2((Painter.AvailableSize.X - max) / 2.0f + margin, margin / 2.0f);
                        Vector2 size = new Vector2(max - margin * 2, max / 2.0f - margin);
                        Painter.Radial(position, size, liftUsage, shortMode ? "Lift thrust" : "Lift thrust usage", 30);
                        position.Y += Painter.AvailableSize.Y / 2.0f;
                        Painter.FilledRectangleEx(new Vector2(position.X, position.Y - 1.0f - margin / 2.0f), new Vector2(max - 2 * margin, 2.0f), Painter.SecondaryColor);
                        string subText = shortMode ? "Cargo" : "Cargo capacity";
                        if (_context._weightAnalizer.CapacityDelta > 0.0001)
                        {
                            float timeLeft = (float)((1.0f - cargoUsage) / _context._weightAnalizer.CapacityDelta);
                            subText = string.Format("+{0:0.00}% @ {1:0.0}s", _context._weightAnalizer.CapacityDelta * 100, timeLeft);
                        }
                        Painter.Radial(position, size, cargoUsage, subText, 30, true);
                        if (liftUsage > _context._weightAnalizer.GetLiftThresholdWarning())
                        {
                            position.Y -= Painter.AvailableSize.Y / 2.0f;
                            Vector2 spriteSize = new Vector2(64, 64);
                            Vector2 spritePos = new Vector2((Painter.AvailableSize.X - spriteSize.X) / 2.0f, margin / 2.0f + size.Y - spriteSize.Y);
                            if (_context.tick % 2 == 0)
                            {
                                Painter.Sprite(spritePos, spriteSize, "Danger");
                            }
                        }
                    }
                }
            }
        }
        #region PreludeFooter
    }
}
#endregion
