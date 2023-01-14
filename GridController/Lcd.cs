
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
				result = "Battery:" + result + spacer + Math.Round(gridCharge*100).ToString() + "%\n";
				return result;
			}
			else
			{
				string result = "Battery:" + Math.Round(gridCharge*100).ToString() + "%\n";
				return result;
			}
        }

        public string lcdFuelInfo(int strsize)
        {
			if (strsize > 9)
			{
				int chargeLen = (int)((strsize - 12) * gridCharge);
				string result = new string('█', chargeLen);
				string spacer = new string(' ', strsize - chargeLen - 12);
				result = "Fuel:" + result + spacer + Math.Round(gridGas*100).ToString() + "%\n";
				return result;
			}
			else
			{
				string result = "Fuel:" + Math.Round(gridGas*100).ToString() + "%\n";
				return result;
			}
        }
		
        public string lcdDamageInfo(int strsize)
        {
            string result = "Damaged blocks\n";
			
			//todo
			
			result += "\n";
            return result;
        }

        public string lcdShowLine(int strsize)
        {
            string result = new string('-', strsize);
			result += "\n";
            return result;
        }		

        public void lcdDraw(string infoTag, string statusTag)
        {
            var info_displays = new List<IMyTextPanel>();
            // var ini = new MyIni();
            var status_displays = new List<IMyTextPanel>();
			
			reScanObjectGroupLocal(status_displays, statusTag);
            reScanObjectGroupLocal(info_displays, infoTag);
            // reScanObjectGroupLocal(displays, GridTag, display => !MyIni.HasSection(display.CustomData, ConfSection));

            logger.write("draw " + info_displays.Count);
            
			var items = gridInventory;
            // string statusMessage = getStatus(); todo move to main here use only value
            // status_displays.ForEach(display => display.WriteText(statusMessage));


            foreach (var display in info_displays)
            {
                var result = "";
                var letters = (int)(display.SurfaceSize.X * (100.0 - 2.0 * display.TextPadding) / 100.0 / display.MeasureStringInPixels(new StringBuilder("X"), display.Font, display.FontSize).X);
                logger.write("screen " + display.FontSize + " letters:" + letters );

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
						case "-":
                            result += lcdShowLine(letters);
                            break;						
                        default:
                            logger.write("lcd: " + display.CustomName + " wrong conf " + command);
                            break;
                    }
                }

                display.WriteText(result);
            }
        }


    }  //@remove
}  //@remove