
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
            if (items.Count == 0) return "[Empty]";
            string itemsStr = "";

            foreach (var i in items)
            {
                var objectName = i.Key.Split('.').Last();
                itemsStr += objectName + ":" + number(i.Value, strsize - objectName.Length - 1) + "\n";
                //todo: if (strsize - objectName.Length - 1 < 0)
            }
            return itemsStr;
        }

        public string lcdBatteryCharge(int strsize)
        {
            if (strsize > 12)
            {
                int chargeLen = (int)((strsize - 12) * gridCharge);
                string result = new string('█', chargeLen);
                string spacer = new string(' ', strsize - chargeLen - 12);
                result = "Battery:" + result + spacer + Math.Round(gridCharge * 100).ToString() + "%\n";
                return result;
            }
            else
            {
                string result = "Battery:" + Math.Round(gridCharge * 100).ToString() + "%\n";
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
                result = "Fuel:" + result + spacer + Math.Round(gridGas * 100).ToString() + "%\n";
                return result;
            }
            else
            {
                string result = "Fuel:" + Math.Round(gridGas * 100).ToString() + "%\n";
                return result;
            }
        }

        public string lcdDamageInfo(int strsize)
        {
            string result = "";
            result = "Damaged blocks:\n" + string.Join("\n", gridDamagedBlocks) + "\n";
            result = result + "Destroyed blocks:\n" + string.Join("\n", gridDestroyedBlocks) + "\n";
            return result;
        }

        public string lcdShowLine(char delimiter, int strsize)
        {
            string result = new string(delimiter, strsize);
            result += "\n";
            return result;
        }

        public string lcdInventorySpecificInfo(int strsize, string subtype)
        {
            List<string> result = new List<string>();
            foreach (var i in gridInventory)
            {
                var objectType = i.Key.Split('.').First();
                var objectName = i.Key.Split('.').Last();
                //todo: if (strsize - objectName.Length - 1 < 0)
                switch (subtype)
                {
                    case "Ore":
                        if (objectType.Contains("_Ore"))
                        {
                            result.Add(objectName + ":" + number(i.Value, strsize - objectName.Length - 1));
                        }
                        break;
                    case "Ingots":
                        if (objectType.Contains("_Ingot"))
                        {
                            result.Add(objectName + " ingots:" + number(i.Value, strsize - objectName.Length - 8));
                        }
                        break;

                    case "Components":
                        if (objectType.Contains("_Component"))
                        {
                            //todo: map to readable names
                            result.Add(objectName + ":" + number(i.Value, strsize - objectName.Length - 1));
                        }
                        break;

                    case "Ammo":
                        if (objectType.Contains("_AmmoMagazine"))
                        {
                            //todo: map to readable names
                            result.Add(objectName + ":" + number(i.Value, strsize - objectName.Length - 1));
                        }
                        break;

                    case "Equip":
                        if (objectType.Contains("_PhysicalGunObject") || objectType.Contains("_ConsumableItem") || (objectType.Contains("ContainerObject")))
                        {
                            //todo: map to readable names
                            result.Add(objectName + ":" + number(i.Value, strsize - objectName.Length - 1));
                        }
                        break;
                    default:
                        break;
                }

            }
            if (result.Count() == 0)
            {
                result.Add("[EMPTY]");
            }
            return string.Join("\n", result) + "\n";
        }

        public void lcdDraw(string infoTag, string statusTag)
        {
            var info_displays = new List<IMyTextPanel>();
            // var ini = new MyIni();
            var status_displays = new List<IMyTextPanel>();

            reScanObjectGroupLocal(status_displays, statusTag);
            reScanObjectGroupLocal(info_displays, infoTag);
            // reScanObjectGroupLocal(displays, GridTag, display => !MyIni.HasSection(display.CustomData, ConfSection));

            var items = gridInventory;
            // string statusMessage = getStatus(); todo move to main here use only value
            // status_displays.ForEach(display => display.WriteText(statusMessage));


            foreach (var display in info_displays)
            {
                var result = "";
                var letters = (int)(display.SurfaceSize.X * (100.0 - 2.0 * display.TextPadding) / 100.0 / display.MeasureStringInPixels(new StringBuilder("X"), display.Font, display.FontSize).X);

                foreach (var command in display.CustomData.Split('\n'))
                {
                    display.ContentType = ContentType.TEXT_AND_IMAGE;
                    display.Font = _lcdFont;
                    display.FontColor = mainColor;

                    if (command == "") continue;
                    switch (command)
                    {
                        case "Battery":
                            result += lcdBatteryCharge(letters);
                            break;
                        case "Fuel":
                            result += lcdFuelInfo(letters);
                            break;
                        case "Damage":
                            result += lcdDamageInfo(letters);
                            break;
                        case "Alerts":
                            //result += lcdAlerts(letters);
                            break;
                        case "Inventory":
                            result += lcdInventoryInfo(items, letters);
                            break;
                        case "AimInfo":
                            //result += lcdAimModes(letters);
                            break;
                        case "Ore":
                            result += lcdInventorySpecificInfo(letters, "Ore");
                            break;
                        case "Ingots":
                            result += lcdInventorySpecificInfo(letters, "Ingots");
                            break;
                        case "Ammo":
                            result += lcdInventorySpecificInfo(letters, "Ammo");
                            break;
                        case "Components":
                            result += lcdInventorySpecificInfo(letters, "Components");
                            break;
                        case "BaseLoader":
                            if (BaseStatus.ContainsKey("Loader")) result += BaseStatus["Loader"] + "\n";
                            break;
                        case "BaseInfo":
                            if (BaseStatus.ContainsKey("Info")) result += BaseStatus["Info"] + "\n";
                            break;
                        case "BaseWarn":
                            if (BaseStatus.ContainsKey("Warn")) result += BaseStatus["Warn"] + "\n";
                            break;
                        case "BaseAlarm":
                            if (BaseStatus.ContainsKey("Alarm")) result += BaseStatus["Alarm"] + "\n";
                            break;

                        default:
                            if (command.Length == 1)
                            {
                                result += lcdShowLine(command[0], letters);
                            }
                            else
                            {
                                logger.write("lcd: " + display.CustomName + " wrong conf " + command);
                            }
                            break;
                    }
                }

                display.WriteText(result);
            }
        }


    }  //@remove
}  //@remove