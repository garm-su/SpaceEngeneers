
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

        public String lcdInventoryInfo(Dictionary<string, int> items, int strsize)
        {
            String itemsStr = "";

            foreach (var i in items)
            {
                var objectName = i.Key.Split('.').Last();
                itemsStr += objectName + ":" + number(i.Value, strsize - objectName.Length - 1) + "\n";
                //test strsize - add logic if itemStr larger than strsize
            }
            return itemsStr;
        }

        public void lcdDraw()
        {
            var displays = new List<IMyTextPanel>();
            // var ini = new MyIni();
            reScanObjectGroupLocal(displays, GridTag);
            // reScanObjectGroupLocal(displays, GridTag, display => !MyIni.HasSection(display.CustomData, ConfSection));
            logger.write("draw " + displays.Count);

            var items = getCurrentInventory(); // todo: move out of here;

            foreach (var display in displays)
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
                            // result += lcdBatteryInfo();
                            break;
                        case "Storage":
                            result += lcdInventoryInfo(items, letters);
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