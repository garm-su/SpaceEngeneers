
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
namespace SpaceEngineers.UWBlockPrograms.TMP
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
        /*AutoAssembly script created by Hitori*/

        bool AUTOSETUP = false; //use true only one time, this will automatically rename your assemblers. 

        /*stock printout settings*/
        const bool PRINT = true; // Do you want the print out function used? set as true or false  
        const string MESSAGE_TAG = "TAG"; //used to mark what terminal to print the result in 

        /* Stock list multiplyer settings*/
        const int STOCK_MULTIPLYER = 1; //Main multiplyer for all items 
        const int IRON_MULTIPLYER = 1;  //uses only iron 
        const int SYSTEMS_MULTIPLYER = 1; //computers/displays/Motors 
        const int POWER_MULTIPLYER = 1;  //power sources 
        const int SPECIAL_MULTIPLYER = 1; //normally only 1 needed 
        const int MILLITARY_MULTIPLYER = 1;  //things that use magnesium and goes boom! 
        const int THRUSTER_MULTIPLYER = 1;  //things that fly! 
        const int GLASS_MULTIPLYER = 1;  //what things that fly hits! 

        /* Stock list setting function. */
        List<stockItem> getStockOrder()
        {
            List<stockItem> order = new List<stockItem>();
            //Iron only 
            order.Add(new stockItem(1200, 5604, "SteelPlate", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            order.Add(new stockItem(280, 1993, "Construction", STOCK_MULTIPLYER * IRON_MULTIPLYER));//construction component 
            order.Add(new stockItem(360, 1069, "InteriorPlate", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            order.Add(new stockItem(40, 371, "LargeTube", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            order.Add(new stockItem(60, 513, "SmallTube", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            order.Add(new stockItem(80, 319, "MetalGrid", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            order.Add(new stockItem(40, 64, "Girder", STOCK_MULTIPLYER * IRON_MULTIPLYER));
            //Systems 
            order.Add(new stockItem(100, 859, "Computer", STOCK_MULTIPLYER * SYSTEMS_MULTIPLYER));
            order.Add(new stockItem(10, 45, "Display", STOCK_MULTIPLYER * SYSTEMS_MULTIPLYER));
            order.Add(new stockItem(20, 203, "Motor", STOCK_MULTIPLYER * SYSTEMS_MULTIPLYER));
            //Power Systems 
            order.Add(new stockItem(64, 64 * 2, "SolarCell", STOCK_MULTIPLYER * POWER_MULTIPLYER));
            order.Add(new stockItem(100, 2000, "Reactor", STOCK_MULTIPLYER * POWER_MULTIPLYER));
            order.Add(new stockItem(25, 25 * 4, "PowerCell", STOCK_MULTIPLYER * POWER_MULTIPLYER));
            //Special/unique 
            order.Add(new stockItem(6, 15, "GravityGenerator", STOCK_MULTIPLYER * SPECIAL_MULTIPLYER));
            order.Add(new stockItem(25, 31, "Detector", STOCK_MULTIPLYER * SPECIAL_MULTIPLYER));
            order.Add(new stockItem(40, 87, "RadioCommunication", STOCK_MULTIPLYER * SPECIAL_MULTIPLYER));
            order.Add(new stockItem(15, 15 * 2, "Medical", STOCK_MULTIPLYER * SPECIAL_MULTIPLYER));
            //Defence/Offence (Millitary) 
            order.Add(new stockItem(6, 6 * 2, "Explosives", STOCK_MULTIPLYER * MILLITARY_MULTIPLYER));
            order.Add(new stockItem(10, 100, "NATO_25x184mm", STOCK_MULTIPLYER * MILLITARY_MULTIPLYER));
            order.Add(new stockItem(10, 100, "NATO_5p56x45mm", STOCK_MULTIPLYER * MILLITARY_MULTIPLYER));
            //Other 
            order.Add(new stockItem(196, 213, "BulletproofGlass", STOCK_MULTIPLYER * GLASS_MULTIPLYER));
            order.Add(new stockItem(80 * 6, 960 * 6, "Thrust", STOCK_MULTIPLYER * THRUSTER_MULTIPLYER));
            //order.Add(new stockItem(1, 1, "Full Stock")); 
            return order;
        }

        /* The following is none of your consern if your not a programmer/scripter */
        /*-----------------------------------------------------------------------*/

        public struct stockItem
        {
            public int low;
            public int max;
            public string name;
            public float count;
            public bool exist;
            public stockItem(int low2, int max2, string name2)
            { low = low2; max = max2 + 1; name = name2; count = 0; exist = false; }
            public stockItem(int low2, int max2, string name2, int multiplyer)
            { low = low2 * multiplyer; max = (max2 + 1) * multiplyer; name = name2; count = 0; exist = false; }
            public stockItem(stockItem temp, float newcount)
            { low = temp.low; max = temp.max; name = temp.name; count = temp.count + newcount; exist = true; }
            public float getCompareValue() //higer value get priority 
            {
                if (low >= max || count > max) return 0;
                else return (float)(max - count) / low;
            }
            public int missing()
            { return max - ((int)count + 1); }
        }

        void printtext(string tag, string message) //used to print to tagged terminal 
        {
            List<IMyTerminalBlock> results = new List<IMyTerminalBlock>(); //make empty list 
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].CustomName.StartsWith(tag))
                { results[i].SetCustomName(tag + " " + message); }
            }
        }

        int readindex(string tag) //used to print to tagged terminal 
        {
            List<IMyTerminalBlock> results = new List<IMyTerminalBlock>(); //make empty list 
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].CustomName.StartsWith(tag) && results[i].DisplayNameText.Length > tag.Length + 3)
                {
                    string temp = results[i].DisplayNameText.Substring(tag.Length, 3);
                    int number = 0;
                    if (int.TryParse(temp, out number))
                    { return number; }
                }
            }
            return -1;
        }

        string printindex(int number)
        {
            string temp = number.ToString();
            if (number < 10 && number > -1)
            { temp.Insert(0, "0"); }
            return temp;
        }

        const string STORAGES_MARKED = null; //what storages are included 
        List<stockItem> getMNP(List<stockItem> stockOrder) //MNP Most Needed Part 
        {
            List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyCollector>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(containers);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(containers);

            for (int i = 0; i < stockOrder.Count; i++)
            {
                //stockOrder[i].name; 
                for (int j = 0; j < containers.Count; j++)
                {
                    if (STORAGES_MARKED == null || containers[j].CustomName.StartsWith(STORAGES_MARKED))
                    {
                        int assembler = 0;
                        if (containers[j] as IMyAssembler != null) { assembler = 1; }
                        IMyInventory tempinventory = ((IMyInventoryOwner)containers[j]).GetInventory(assembler); //assemblers use GetInventory(1)    

                        List<IMyInventoryItem> tempitem = tempinventory.GetItems();
                        for (int k = 0; k < tempitem.Count; k++)
                        {
                            if (string.Equals(tempitem[k].Content.SubtypeName, stockOrder[i].name))
                            {
                                stockOrder.Insert(i, new stockItem(stockOrder[i], (float)tempitem[k].Amount));
                                stockOrder.RemoveAt(i + 1);
                            }
                        }
                    }
                }
            }
            return stockOrder;
        }

        List<stockItem> sortOrderList(List<stockItem> stockOrder)
        {
            for (int i = 0; i < stockOrder.Count - 1; i++) //organice priority 
            {
                if (stockOrder[i].getCompareValue() < stockOrder[i + 1].getCompareValue())
                {
                    stockOrder.Move(i + 1, i);
                    if (i > 1) { i -= 2; }
                }
            }
            return stockOrder;
        }

        void AssemblerControll(List<stockItem> parts)
        {
            List<IMyTerminalBlock> assemblers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
            for (int i = 0; i < assemblers.Count; i++)
            {
                for (int k = 0; k < parts.Count; k++)
                {
                    if (assemblers[i].CustomName.Contains(parts[k].name))
                    {
                        if (parts[k].missing() > 0 && parts[k].exist)
                        {
                            ITerminalAction temp = assemblers[i].GetActionWithName("OnOff_On");
                            temp.Apply(assemblers[i]);
                        }
                        else
                        {
                            ITerminalAction temp = assemblers[i].GetActionWithName("OnOff_Off");
                            temp.Apply(assemblers[i]);
                        }
                    }
                }
            }
        }

        void run_first_time_setup(List<stockItem> stockOrder)
        {
            List<IMyTerminalBlock> assemblers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
            for (int i = 0; i < assemblers.Count && i < stockOrder.Count; i++)
            {
                assemblers[i].SetCustomName("Assembler " + stockOrder[i].name);
            }
        }

        void Main()
        {
            List<stockItem> stockOrder = getStockOrder();
            stockOrder = getMNP(stockOrder);
            if (AUTOSETUP)
            { run_first_time_setup(stockOrder); }
            AssemblerControll(stockOrder);
            if (PRINT)
            {
                stockOrder = sortOrderList(stockOrder);

                int showWithPriority = readindex(MESSAGE_TAG);//order in list starts at 0 
                if (stockOrder.Count - 1 > showWithPriority || showWithPriority == -1)
                { showWithPriority++; }
                else
                { showWithPriority = 0; }

                printtext(MESSAGE_TAG, "" + printindex(showWithPriority) + "  " + stockOrder[showWithPriority].name + " Need+ " + stockOrder[showWithPriority].missing());
            }
            return;
        }
    }
}
