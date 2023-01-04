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


        /*
         * All My Stuff
         * ------------
         * A script to keep track of your inventory
         * 
         * Configure using CustomData. Put an Inventory section into any screen. Here are
         * the defaults:
         * 
         * [inventory]
         * delay=3         # Set this to change the update frequency.
         * display=0      # Use this to control a display for output (they start at 0)
         * scale = 1.0     # Make things on the display larger or smaller
         * skip = 0         # Use this on a second screen if the lines don't all fit on the first, to skip lines
         * color = 00FF55 # Use this to customise the header color of a screen
         * mono = false     # change to true to display numbers in the monospace font
         * suppress_zeros = false # Change to truew to hide (red) lines showing zero stock
         * enablefilter = true  # Enable filtering
         * filter = ConsumableItem, Datapad, PhysicalGunObject, AmmoMagazine, Ore, Ingot, Component
         * 
         * An alternative, for those who wish to use more than one screen on a given
         * block at once, is to configure displays in the following manner (this example
         * works on the Sci-Fi Button Panel):
         * 
         * [inventory_display0]
         * scale=0.4
         * 
         * [inventory_display1]
         * scale=0.4
         * skip=5
         * 
         * [inventory_display2]
         * scale=0.4
         * skip=10
         * 
         * [inventory_display3]
         * scale=0.4
         * skip=15
         */

        List<IMyTerminalBlock> Containers = new List<IMyTerminalBlock>();
        static readonly string Version = "Version 1.2.2";
        MyIni ini = new MyIni();
        static readonly string ConfigSection = "Inventory";
        static readonly string DisplaySectionPrefix = ConfigSection + "_Display";
        StringBuilder SectionCandidateName = new StringBuilder();
        List<String> SectionNames = new List<string>();
        SortedDictionary<string, Item> Stock = new SortedDictionary<string, Item>();
        List<MyInventoryItem> Items = new List<MyInventoryItem>();
        List<ManagedDisplay> Screens = new List<ManagedDisplay>();
        IEnumerator<bool> _stateMachine;
        int delayCounter = 0;
        int delay;
        bool TranslateEnabled; // Enable translate feature globally
        bool FilterEnabled;    // Enable filter feature globally
        bool rebuild = false;

        public class Item
        {
            public Item(MyInventoryItem item, int Amount = 0)
            {
                this.Sprite = item.Type.ToString();
                this.Name = item.Type.SubtypeId;
                this.ItemType = item.Type.TypeId;
                this.Amount = Amount;
            }
            public int Amount;
            public string Sprite;
            public string Name;
            public string ItemType;
        }

        public void GetBlocks()
        {
            Containers.Clear();
            Screens.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Containers, block =>
            {
                if (!block.IsSameConstructAs(Me))
                    return false;
                if (!TryAddDiscreteScreens(block))
                    TryAddScreen(block);
                return block.HasInventory & block.ShowInInventory;
            });
        }

        void AddScreen(IMyTextSurfaceProvider provider, int displayNumber, string section)
        {
            var display = ((IMyTextSurfaceProvider)provider).GetSurface(displayNumber);
            var linesToSkip = ini.Get(section, "skip").ToInt16();
            bool monospace = ini.Get(section, "mono").ToBoolean();
            bool suppressZeros = ini.Get(section, "suppress_zeros").ToBoolean();
            float scale = ini.Get(section, "scale").ToSingle(1.0f);
            string DefaultColor = "FF4500";
            string ColorStr = ini.Get(section, "color").ToString(DefaultColor);
            if (ColorStr.Length < 6)
                ColorStr = DefaultColor;
            Color color = new Color()
            {
                R = byte.Parse(ColorStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                G = byte.Parse(ColorStr.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                B = byte.Parse(ColorStr.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                A = 255
            };
            var managedDisplay = new ManagedDisplay(display, scale, color, linesToSkip, monospace, suppressZeros);
            if (FilterEnabled)
            {
                managedDisplay.SetFilter(ini.Get(section, "filter").ToString(null));
            }
            Screens.Add(managedDisplay);
        }

        private bool TryAddDiscreteScreens(IMyTerminalBlock block)
        {
            bool retval = false;
            IMyTextSurfaceProvider Provider = block as IMyTextSurfaceProvider;
            if (null == Provider || Provider.SurfaceCount == 0)
                return true;
            StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            ini.TryParse(block.CustomData);
            ini.GetSections(SectionNames);
            foreach (var section in SectionNames)
            {
                if (section.StartsWith(DisplaySectionPrefix, ignoreCase))
                {
                    for (int displayNumber = 0; displayNumber < Provider.SurfaceCount; ++displayNumber)
                    {
                        if (displayNumber < Provider.SurfaceCount)
                        {
                            SectionCandidateName.Clear();
                            SectionCandidateName.Append(DisplaySectionPrefix).Append(displayNumber.ToString());
                            if (section.Equals(SectionCandidateName.ToString(), ignoreCase))
                            {
                                AddScreen(Provider, displayNumber, section);
                                retval = true;
                            }
                        }
                    }
                }
            }
            return retval;
        }

        private void TryAddScreen(IMyTerminalBlock block)
        {
            IMyTextSurfaceProvider Provider = block as IMyTextSurfaceProvider;
            if (null == Provider || Provider.SurfaceCount == 0 || !MyIni.HasSection(block.CustomData, ConfigSection))
                return;
            ini.TryParse(block.CustomData);
            var displayNumber = ini.Get(ConfigSection, "display").ToUInt16();
            if (displayNumber < Provider.SurfaceCount || Provider.SurfaceCount == 0)
            {
                AddScreen(Provider, displayNumber, ConfigSection);
            }
            else
            {
                Echo("Warning: " + block.CustomName + " doesn't have a display number " + ini.Get(ConfigSection, "display").ToString());
            }
        }

        public void RunItemCounter()
        {
            if (_stateMachine != null)
            {
                bool hasMoreSteps = _stateMachine.MoveNext();

                if (hasMoreSteps)
                {
                    Runtime.UpdateFrequency |= UpdateFrequency.Once;
                }
                else
                {
                    _stateMachine.Dispose();
                    _stateMachine = null;
                }
            }
        }

        public IEnumerator<bool> CountItems()
        {
            foreach (var Item in Stock.Keys)
                Stock[Item].Amount = 0;
            yield return true;
            ReadConfig();
            yield return true;
            yield return true;
            foreach (var container in Containers)
            {
                for (int i = 0; i < container.InventoryCount; ++i)
                {
                    var inventory = container.GetInventory(i);
                    if (inventory.ItemCount > 0)
                    {
                        Items.Clear();
                        inventory.GetItems(Items);
                        foreach (var item in Items)
                        {
                            string key = item.Type.ToString();
                            if (!Stock.ContainsKey(key))
                                Stock.Add(key, new Item(item));
                            Stock[key].Amount += item.Amount.ToIntSafe();
                        }
                        yield return true;
                    }
                }
            }
            EchoStuff();
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            ReadConfig();
            GetBlocks();
        }

        private void ReadConfig()
        {
            if (ini.TryParse(Me.CustomData))
            {
                delay = ini.Get(ConfigSection, "delay").ToInt32(3);
                TranslateEnabled = ini.Get(ConfigSection, "enabletranslate").ToBoolean(false);
                FilterEnabled = ini.Get(ConfigSection, "enablefilter").ToBoolean(false);
            }
        }

        private void EchoStuff()
        {
            Echo(Version);
            Echo(Screens.Count + " screens");
            Echo(Containers.Count + " blocks with inventories");
            Echo(Stock.Count + " items being tracked");
            Echo("Filtering " + (FilterEnabled ? "enabled" : "disabled"));
            Echo("Translation " + (TranslateEnabled ? "enabled" : "disabled"));
            foreach (var display in Screens)
            {
                display.Render(Stock);
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Once) == UpdateType.Once)
            {
                RunItemCounter();
            }
            if ((updateSource & UpdateType.Update100) == UpdateType.Update100)
            {
                if (delayCounter > delay && _stateMachine == null)
                {
                    if (rebuild)
                    {
                        rebuild = false;
                        ReadConfig();
                        GetBlocks();
                    }
                    delayCounter = 0;
                    _stateMachine = CountItems();
                    RunItemCounter();
                }
                else
                {
                    ++delayCounter;
                }
            }
            if (argument == "count" && _stateMachine == null)
            {
                _stateMachine = CountItems();
                RunItemCounter();
            }
            if (argument == "rebuild")
            {
                rebuild = true;
            }
        }


        class ManagedDisplay
        {
            private IMyTextSurface surface;
            private RectangleF viewport;
            private MySpriteDrawFrame frame;
            private float StartHeight = 0f;
            private float HeadingHeight = 35f;
            private float LineHeight = 30f;
            private float HeadingFontSize = 1.3f;
            private float RegularFontSize = 1.0f;
            private Vector2 Position;
            private int WindowSize;         // Number of lines shown on screen at once after heading
            private Color HighlightColor;
            private int linesToSkip;
            private bool monospace;
            private bool MakeSpriteCacheDirty = false;
            private string previousType;
            private string Heading = "Item";
            private bool unfiltered = true;
            private bool SupressZeros = false;
            private string Filter;
            private int characters_to_skip = "MyObjectBuilder_".Length;
            private Color BackgroundColor, ForegroundColor;

            public ManagedDisplay(IMyTextSurface surface, float scale = 1.0f, Color highlightColor = new Color(), int linesToSkip = 0, bool monospace = false, bool suppressZeros = false)
            {
                this.surface = surface;
                this.HighlightColor = highlightColor;
                this.linesToSkip = linesToSkip;
                this.monospace = monospace;
                this.SupressZeros = suppressZeros;
                this.BackgroundColor = surface.ScriptBackgroundColor;
                this.ForegroundColor = surface.ScriptForegroundColor;

                // Scale everything!
                StartHeight *= scale;
                HeadingHeight *= scale;
                LineHeight *= scale;
                HeadingFontSize *= scale;
                RegularFontSize *= scale;

                surface.ContentType = ContentType.SCRIPT;
                surface.Script = "TSS_FactionIcon";
                Vector2 padding = surface.TextureSize * (surface.TextPadding / 100);
                viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f + padding, surface.SurfaceSize - (2 * padding));
                WindowSize = ((int)((viewport.Height - 10 * scale) / LineHeight));
            }

            private void AddHeading()
            {
                float finalColumnWidth = HeadingFontSize * 80;
                // that thing above is rough - this is just used to stop headings colliding, nothing serious,
                // and is way cheaper than allocating a StringBuilder and measuring the width of the final
                // column heading text in pixels.
                if (surface.Script != "")
                {
                    surface.Script = "";
                    surface.ScriptBackgroundColor = BackgroundColor;
                    surface.ScriptForegroundColor = ForegroundColor;
                }
                if (!unfiltered)
                    Heading = Filter;
                Position = new Vector2(viewport.Width / 10f, StartHeight) + viewport.Position;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "Textures\\FactionLogo\\Builders\\BuilderIcon_1.dds",
                    Position = Position + new Vector2(0f, LineHeight / 2f),
                    Size = new Vector2(LineHeight, LineHeight),
                    RotationOrScale = HeadingFontSize,
                    Color = HighlightColor,
                    Alignment = TextAlignment.CENTER
                });
                Position.X += viewport.Width / 8f;
                frame.Add(MySprite.CreateClipRect(new Rectangle((int)Position.X, (int)Position.Y, (int)(viewport.Width - Position.X - finalColumnWidth), (int)(Position.Y + HeadingHeight))));
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = Heading,
                    Position = Position,
                    RotationOrScale = HeadingFontSize,
                    Color = HighlightColor,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                frame.Add(MySprite.CreateClearClipRect());
                Position.X = viewport.Width;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Stock",
                    Position = Position,
                    RotationOrScale = HeadingFontSize,
                    Color = HighlightColor,
                    Alignment = TextAlignment.RIGHT,
                    FontId = "White"
                });
                Position.Y += HeadingHeight;
            }

            internal void SetFilter(string filter)
            {
                if (null == filter || filter.Length == 0)
                    return;
                Filter = filter;
                unfiltered = false;
            }

            private void RenderRow(Program.Item item)
            {
                float finalColumnWidth = HeadingFontSize * 80;
                // that thing above is rough - this is just used to stop headings colliding, nothing serious,
                // and is way cheaper than allocating a StringBuilder and measuring the width of the final
                // column heading text in pixels.
                Color TextColor;
                if (item.Amount == 0)
                {
                    TextColor = Color.Brown;
                }
                else
                {
                    TextColor = surface.ScriptForegroundColor;
                }
                var first = previousType == "FIRST";
                if (previousType != item.ItemType)
                {
                    previousType = item.ItemType;
                    if (!first)
                        frame.Add(new MySprite()
                        {
                            Type = SpriteType.TEXTURE,
                            Data = "SquareSimple",
                            Position = new Vector2(viewport.X, Position.Y),
                            Size = new Vector2(viewport.Width, 1),
                            RotationOrScale = 0,
                            Color = HighlightColor,
                        });
                }
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = item.Sprite,
                    Position = Position + new Vector2(0f, LineHeight / 2f),
                    Size = new Vector2(LineHeight, LineHeight),
                    RotationOrScale = 0,
                    Color = TextColor,
                    Alignment = TextAlignment.CENTER,
                });
                Position.X += viewport.Width / 8f;
                frame.Add(MySprite.CreateClipRect(new Rectangle((int)Position.X, (int)Position.Y, (int)(viewport.Width - Position.X - finalColumnWidth), (int)(Position.Y + HeadingHeight))));
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = item.Name,
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = TextColor,
                    Alignment = TextAlignment.LEFT,
                    FontId = monospace ? "Monospace" : "White"
                });
                frame.Add(MySprite.CreateClearClipRect());
                Position.X += viewport.Width * 6f / 8f;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = item.Amount.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = TextColor,
                    Alignment = TextAlignment.RIGHT,
                    FontId = monospace ? "Monospace" : "White"
                });
            }

            internal void Render(SortedDictionary<String, Program.Item> Stock)
            {
                MakeSpriteCacheDirty = !MakeSpriteCacheDirty;
                frame = surface.DrawFrame();
                if (MakeSpriteCacheDirty)
                {
                    frame.Add(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = "SquareSimple",
                        Color = surface.BackgroundColor,
                        Position = new Vector2(0, 0),
                        Size = new Vector2(0, 0)
                    });
                }
                AddHeading();
                int renderLineCount = 0;
                previousType = "FIRST";
                foreach (var item in Stock.Keys)
                {
                    // Contains with StringComparison.InvariantCultureIgnoreCase is prohibited )-:
                    if (unfiltered || Filter.ToLower().Contains(Stock[item].ItemType.Substring(characters_to_skip).ToLower()))
                    {
                        if ((Stock[item].Amount != 0 || !SupressZeros) && ++renderLineCount > linesToSkip)
                        {
                            Position.X = viewport.Width / 10f + viewport.Position.X;
                            if (renderLineCount >= linesToSkip && renderLineCount < linesToSkip + WindowSize)
                                RenderRow(Stock[item]);
                            Position.Y += LineHeight;
                        }
                    }
                }
                frame.Dispose();
            }
        }


        #region PreludeFooter
    }
}
#endregion