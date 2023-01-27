
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
using VRage.Game.ModAPI.Ingame.Utilities;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusLcd //@remove
{ //@remove

    public class Program : GridStatusInfo.Program //@remove
    { //@remove

        private string _lcdFont = "Monospace";
        private float letterWidth = 18;

        public string lcdInventoryInfo(Dictionary<string, int> items, int strsize)
        {

            List<string> result = new List<string>();

            if (items.Count == 0) return "[Empty]";

            foreach (var i in items)
            {
                var objectName = i.Key.Split('.').Last();
                //todo: map objectName to readable names
                result.Add(objectName + ":" + number(i.Value, strsize - objectName.Length - 1));
                //todo: if (strsize - objectName.Length - 1 < 0)
            }
            return string.Join("\n", result);
        }

        public string lcdInventoryInfo(Dictionary<string, int> items, int strsize, string subtype)
        {
            List<string> result = new List<string>();

            if (items.Count == 0) return "[Empty]";

            foreach (var i in items)
            {
                var objectType = i.Key.Split('.').First();
                var objectName = i.Key.Split('.').Last();
                //todo: map objectName to readable names                
                //todo: if (strsize - objectName.Length - 1 < 0)
                int tLen = strsize - objectName.Length - 1;
                switch (subtype)
                {
                    case "Ore":
                        if (objectType.Contains("_Ore"))
                        {
                            result.Add(objectName + ":" + number(i.Value, tLen));
                        }
                        break;
                    case "Ingots":
                        if (objectType.Contains("_Ingot"))
                        {
                            result.Add(objectName + " ingots:" + number(i.Value, tLen - 7));
                        }
                        break;

                    case "Components":
                        if (objectType.Contains("_Component"))
                        {
                            result.Add(objectName + ":" + number(i.Value, tLen));
                        }
                        break;

                    case "Ammo":
                        if (objectType.Contains("_AmmoMagazine"))
                        {
                            result.Add(objectName + ":" + number(i.Value, tLen));
                        }
                        break;

                    case "Equip":
                        if (objectType.Contains("_PhysicalGunObject") || objectType.Contains("_ConsumableItem") || (objectType.Contains("ContainerObject")))
                        {
                            result.Add(objectName + ":" + number(i.Value, tLen));
                        }
                        break;
                    default:
                        break;
                }

            }
            if (result.Count() == 0)
            {
                result.Add("[Empty]");
            }
            return string.Join("\n", result);
        }

        public string lcdBatteryCharge(int strsize)
        {
            if (strsize > 12)
            {
                int chargeLen = (int)((strsize - 12) * gridCharge);
                string result = new string('█', chargeLen);
                string spacer = new string(' ', strsize - chargeLen - 12);
                result = "Battery " + result + spacer + Math.Round(gridCharge * 100).ToString() + "%";
                return result;
            }
            else
            {
                string result = "Battery " + Math.Round(gridCharge * 100).ToString() + "%";
                return result;
            }
        }

        public string lcdFuelInfo(int strsize)
        {
            if (strsize > 9)
            {
                int chargeLen = (int)((strsize - 9) * gridCharge);
                string result = new string('█', chargeLen);
                string spacer = new string(' ', strsize - chargeLen - 9);
                result = "Fuel:" + result + spacer + Math.Round(gridGas * 100).ToString() + "%";
                return result;
            }
            else
            {
                string result = "Fuel:" + Math.Round(gridGas * 100).ToString() + "%";
                return result;
            }
        }

        public string lcdDamageInfo(int strsize)
        {
            string result = "";
            //todo: add info about damaged armor    
            if (gridDamagedBlocks.Count() == 0 && gridDestroyedBlocks.Count() == 0)
            {
                return "No damage";
            }
            else
            {
                bool flag = false;
                if (gridDamagedBlocks.Count() != 0)
                {
                    result = "Damaged\n" + string.Join("\n", gridDamagedBlocks);
                    flag = true;
                }
                if (gridDestroyedBlocks.Count() != 0)
                {
                    if (flag) result += "\n";
                    result = result + "Destroyed\n" + string.Join("\n", gridDestroyedBlocks);
                }
            }
            return result;
        }

        public string lcdShowLine(char delimiter, int strsize)
        {
            string result = new string(delimiter, strsize);
            return result;
        }
        public void initDrawSurface(IMyTextSurface s)
        {
            s.ContentType = ContentType.SCRIPT;
            s.Script = "";
        }

        public void lcdDrawSpriteText(IMyTextSurface s, string text, Color textColor, float textSize)
        {
            Vector2 surfaceSize = s.TextureSize;
            string textFont = "Debug";
            List<string> strList = new List<string>();
            strList = text.Split('\n').ToList();
            float strN = 0f;
            float offsetH = 10f;
            using (var frame = s.DrawFrame())
            {
                foreach (var e in strList)
                {
                    if (e == "---")
                    {
                        frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, strN * 30f * textSize + offsetH), new Vector2(s.TextureSize.X, 1f), textColor, null, TextAlignment.LEFT, 0f));
                        offsetH += 10f;
                    }
                    else
                    {
                        List<string> tmp = e.Split(':').ToList();
                        var sprite = MySprite.CreateText(tmp.First(), textFont, textColor, textSize, TextAlignment.LEFT);
                        sprite.Position = new Vector2(10f, strN * 30f * textSize + offsetH);
                        frame.Add(sprite);
                        if (tmp.Count() > 1)
                        {
                            sprite = MySprite.CreateText(tmp.Last(), textFont, new Color(200, 200, 200), textSize, TextAlignment.RIGHT);
                            sprite.Position = new Vector2(surfaceSize.X - 20f, strN * 30f * textSize + offsetH);
                            frame.Add(sprite);
                        }
                        strN += 1f;

                    }
                }
            }
        }

        public void surfaceDraw(String CustomName, IMyTextSurface surface, MyIniValue conf)
        {
            if (conf.IsEmpty) return;

            var result = new List<string>();
            var letters = (int)(surface.SurfaceSize.X * (100.0 - 2.0 * surface.TextPadding) / 100.0 / surface.MeasureStringInPixels(new StringBuilder("X"), surface.Font, surface.FontSize).X);
            var commands = conf.ToString().Split(',');
            var isSprite = (commands[0] == "Sprite");
            foreach (var command in commands)
            {
                if (!isSprite)
                {
                    surface.ContentType = ContentType.TEXT_AND_IMAGE;
                    surface.Font = _lcdFont;
                    //display.FontColor = mainColor; /todo from config
                }
                else
                {
                    initDrawSurface(surface);
                    surface.ScriptBackgroundColor = new Color(0, 0, 0);
                }

                if (command == "") continue;
                switch (command)
                {
                    case "Sprite":
                        break;
                    case "Battery":
                        result.Add(lcdBatteryCharge(letters));
                        break;
                    case "Fuel":
                        result.Add(lcdFuelInfo(letters));
                        break;
                    case "Damage":
                        result.Add(lcdDamageInfo(letters));
                        break;
                    case "Alerts":
                        //result += lcdAlerts(letters);
                        break;
                    case "Inventory":
                        result.Add(lcdInventoryInfo(gridInventory, letters));
                        break;
                    case "AimInfo":
                        //result += lcdAimModes(letters);
                        break;
                    case "Ore":
                        result.Add(lcdInventoryInfo(gridInventory, letters, "Ore"));
                        break;
                    case "Ingots":
                        result.Add(lcdInventoryInfo(gridInventory, letters, "Ingots"));
                        break;
                    case "Ammo":
                        result.Add(lcdInventoryInfo(gridInventory, letters, "Ammo"));
                        break;
                    case "Components":
                        result.Add(lcdInventoryInfo(gridInventory, letters, "Components"));
                        break;
                    case "BaseLoader":
                        if (BaseStatus.ContainsKey("Loader")) result.Add(BaseStatus["Loader"]);
                        break;
                    case "BaseInfo":
                        if (BaseStatus.ContainsKey("Info")) result.Add(BaseStatus["Info"]);
                        break;
                    case "BaseWarn":
                        if (BaseStatus.ContainsKey("Warn")) result.Add(BaseStatus["Warn"]);
                        break;
                    case "BaseAlarm":
                        if (BaseStatus.ContainsKey("Alarm")) result.Add(BaseStatus["Alarm"]);
                        break;

                    default:
                        if (command.Length == 1)
                        {
                            if (!isSprite)
                            {
                                result.Add(lcdShowLine(command[0], letters));
                            }
                            else
                            {
                                result.Add("---");
                            }
                        }
                        else
                        {
                            logger.write("lcd: " + CustomName + " wrong conf " + command);
                        }
                        break;
                }
            }

            if (isSprite)
            {
                Color tColor = new Color(0, 200, 0);
                lcdDrawSpriteText(surface, string.Join("\n", result), tColor, 1.5f);
            }
            else
            {
                surface.WriteText(string.Join("\n", result));
            }
        }
        public void lcdDraw()
        {

            // IMyCockpit x;
            // IMyTextPanel z;
            // var a1 = x.SurfaceCount;
            // var a2 = x.GetSurface(0);
            // y.

            var info_displays = new List<IMyTextPanel>();
            var info_cockpits = new List<IMyCockpit>();

            reScanObjectsLocal(info_displays, hasConfSection);

            var ini = new MyIni();

            foreach (var display in info_displays)
            {
                if (ini.TryParse(display.CustomData, ConfSection))
                {
                    surfaceDraw(display.CustomName, display, ini.Get(ConfSection, "Surface"));
                }
                else
                {
                    Echo("Can't parse ini on " + display.CustomData);
                }
            }

            reScanObjectsLocal(info_cockpits, hasConfSection);
            foreach (var cockpit in info_cockpits)
            {
                if (ini.TryParse(cockpit.CustomData, ConfSection))
                {
                    for (var i = 0; i < cockpit.SurfaceCount; i++)
                    {
                        surfaceDraw(cockpit.CustomName, cockpit.GetSurface(i), ini.Get(ConfSection, "Surface" + i));
                    }
                }
                else
                {
                    Echo("Can't parse ini on " + cockpit.CustomData);
                }
            }
        }

    }  //@remove
}  //@remove