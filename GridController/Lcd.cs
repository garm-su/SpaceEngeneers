
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

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusLcd //@remove
{ //@remove
    public class Programы : GridStatusConfig.Program //@remove
    { //@remove
        Color mainColor = new Color(0, 255, 0);

        public void showInventory(IMyTextSurface display, Dictionary<string, int> items, int strsize)
        {
            String itemsStr = "";

            foreach (var i in items)
            {
                itemsStr += i.Key + ":" + number(i.Value, strsize - i.Key.Length - 1) + "\n";
                //test strsize - add logic if itemStr larger than strsize
            }
            display.WriteText(itemsStr);
        }


    }  //@remove
}  //@remove