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
using SpaceEngineers.UWBlockPrograms.Grid;

public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    public new TValue this[TKey key]
    {
        get
        {
            TValue val;
            if (!TryGetValue(key, out val))
            {
                val = new TValue();
                Add(key, val);
            }
            return val;
        }
        set { base[key] = value; }
    }
}

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.Helpers //@remove
{ //@remove
    public class Program : MyGridProgram //@remove
    { //@remove
        public double percentOf(double current, double maximum)
        {
            return (maximum == 0 ? 1 : 1 * current / maximum);
        }

        public string getName(MyItemType type)
        {
            return type.TypeId + '.' + type.SubtypeId;
        }

        public String number(double count)
        {
            var prefix = "  ";
            if (count >= 1e6)
            {
                count /= 1e6;
                prefix = "M";
            }
            else if (count >= 1e3)
            {
                count /= 1e3;
                prefix = "K";
            }

            return String.Format("{0,3:0.0}", count) + prefix;

        }
    }//@remove
}//@remove