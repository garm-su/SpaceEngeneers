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
using VRage.Game.GUI.TextPanel;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatus
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion
		//all terminalblocks
		List<IMyTerminalBlock> allTBlocks = new List<IMyTerminalBlock>();
		//all armor blocks - be defined

		//cargoLoad,,Таскало 01 TB handleCharged
		//cargoSave,
		//Battery,Таскало 01 TB handleCharged
		//connect,Таскало 01 TB handleConnected
		//batteryCharge,Recharge
		//batteryCharge,Auto
		//MyObjectBuilder_Ore.Ice: 3000
		//MyObjectBuilder_Ingot.Iron: 42000
		//MyObjectBuilder_Ore.Gold: 42000
		//MyObjectBuilder_Component.SteelPlate: 3000
		//MyObjectBuilder_AmmoMagazine.NATO_25x184mm: 3000


		const String SKIP = "[SKIP]";
		const String StatusTag = "[STATUS]";
		const String RequestTag = "[REQUEST]";
		const double BATTERY_MAX_LOAD = 0.95;
		Color mainColor = new Color(0, 255, 0);

		string statusChannelTag = "RDOStatusChannel";
		string commandChannelTag = "RDOCommandChannel";

		bool setupcomplete = false;
		bool checkDestroyedBlocks = true;
		bool lockedState = false;
		double damagedBlockRatio = 0;
		double destroyedAmount = 0;

		Int32 energyTreshold = 25; //% of max capacity, default - 25%
		Int32 gasTreshold = 25; //% of max capacity, default - 25%
		Int32 uraniumTreshold = 0; //kg
		Int32 damageTreshold = 20; //% of terminal blocks, default - 20%

		List<IMyRadioAntenna> antenna = new List<IMyRadioAntenna>();
		List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();
		Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
		Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

		IMyBroadcastListener commandListener;
		IMyBroadcastListener statusListener;

		public void reReadConfig(Dictionary<string, int> minResourses, String CustomData)
		{
			minResourses.Clear();

			foreach (String row in CustomData.Split('\n'))
			{
				if (row.Contains(":"))
				{
					var tup = row.Split(':');
					if (tup.Length != 2)
					{
						Echo("Err: " + row);
						continue;
					}
					minResourses[tup[0].Trim()] = Convert.ToInt32(tup[1].Trim());
				}
			}
		}

		public void reScanObjectExact<T>(List<T> result, String name) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName == name);
		}
		public void reScanObjectExactLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName == name);
		}
		public void reScanObjectGroupLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName.Contains(name));
		}
		public void reScanObjectGroup<T>(List<T> result, String name) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName.Contains(name));
		}
		public void reScanObjectsLocal<T>(List<T> result, Func<IMyTerminalBlock, bool> check) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && check(item));
		}
		public void reScanObjectsLocal<T>(List<T> result) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid);
		}
		public void reScanObjects<T>(List<T> result) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result);
		}
		public void reScanObjects<T>(List<T> result, Func<IMyTerminalBlock, bool> check) where T : class, IMyTerminalBlock
		{
			GridTerminalSystem.GetBlocksOfType<T>(result, check);
		}

		private string getName(MyItemType type)
		{
			return type.TypeId + '.' + type.SubtypeId;
		}

		public void cargoLoad(string group, String after)
		{
			Echo("cargoLoad: " + group);
			var blocks = new List<IMyTerminalBlock>();
			reScanObjectsLocal(blocks, b => b.HasInventory);
			var outerCargo = new List<IMyCargoContainer>();
			var needResources = new Dictionary<string, int>();
			reScanObjectsLocal(outerCargo, b => b.HasInventory);

			bool full = true;
			double need = 0, found = 0;

			foreach (var block in blocks)
			{
				reReadConfig(needResources, block.CustomData);
				need += needResources.Values.Sum();
				var items = new List<MyInventoryItem>();
				for (int j = 0; j < block.InventoryCount; j++)
				{
					block.GetInventory(j).GetItems(items);
					foreach (var item in items)
					{
						var resourceName = getName(item.Type);
						if (needResources.ContainsKey(resourceName))
						{
							needResources[resourceName] -= (int)item.Amount;
							found += (int)item.Amount;
						}
					}
				}

				var currentfull = moveResources(
					outerCargo,
					block,
					needResources.Where(r => r.Value > 0).ToDictionary(i => i.Key, i => i.Value)
				);
				if (!currentfull)
				{
					Echo("NotFull: " + block.CustomName);
				}
				full &= currentfull;
			}

			if (full)
			{
				run(after);
			}
			else
			{
				print("C." + (int)(need == 0 ? 100 : 100 * found / need) + "%");
			}
		}

		private bool moveResources(List<IMyCargoContainer> outerCargo, IMyTerminalBlock block, Dictionary<string, int> dictionary)
		{
			Echo("Move to " + block.CustomName + " " + dictionary.Keys.Count());
			if (!block.HasInventory || dictionary.Count == 0) return true;
			IMyInventory sourse, destination = block.GetInventory();
			if (destination.IsFull) return true;

			for (int i = 0; i < outerCargo.Count; i++)
			{
				if (outerCargo[i].CustomName.Contains(SKIP)) continue;

				for (int j = 0; j < outerCargo[i].InventoryCount; j++)
				{
					var items = new List<MyInventoryItem>();
					sourse = outerCargo[i].GetInventory(j);
					sourse.GetItems(items);
					if (!sourse.IsConnectedTo(destination)) continue;
					for (int k = 0; k < items.Count; k++)
					{
						var item = items[k];
						var resourceName = getName(item.Type);
						if (dictionary.ContainsKey(resourceName) && dictionary[resourceName] > 0)
						{
							var countToMove = Math.Min(dictionary[resourceName], (int)item.Amount);
							sourse.TransferItemTo(destination, k, null, true);
							dictionary[resourceName] -= countToMove;
						}
					}
				}
			}
			return false;
		}

		public void cargoSave(string group)
		{
			Echo("cargoSave: " + group);
			var blocks = new List<IMyTerminalBlock>();
			reScanObjectsLocal(blocks, b => b.HasInventory);
			var info = new List<String>();

			foreach (var block in blocks)
			{
				info.Clear();
				var items = new List<MyInventoryItem>();
				for (int j = 0; j < block.InventoryCount; j++)
				{
					block.GetInventory(j).GetItems(items);
					foreach (var item in items)
					{
						info.Add(getName(item.Type) + ": " + item.Amount.ToString());
					}
				}
				block.CustomData = String.Join("\n", info);
			}
		}

		public void connect(String connectorName, String tbName)
		{
			var blocks = new List<IMyShipConnector>();
			reScanObjectsLocal(blocks, b => b.HasInventory);
			foreach (var connector in blocks)
			{
				connector.Connect();
				if (connector.Status == MyShipConnectorStatus.Connected)
				{
					run(tbName);
					return;
				}
			}
		}

		public void print(String s)
		{
			var surface = Me.GetSurface(0);
			surface.Alignment = TextAlignment.CENTER;
			surface.FontColor = mainColor;
			surface.FontSize = 4;
			surface.WriteText(s);
		}

		public void run(String name)
		{
			var tbs = new List<IMyTimerBlock>();
			reScanObjectExactLocal(tbs, name);
			tbs.ForEach(tb => tb.Trigger());
		}

		public void batteryLoad(String after)
		{
			Echo("batteryLoad");
			List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
			reScanObjectsLocal(batteries);
			float curLoad;
			if (batteries.Count > 0)
			{
				curLoad = 0;
				foreach (var bat in batteries)
				{
					curLoad += bat.CurrentStoredPower / bat.MaxStoredPower;
				}
				curLoad /= batteries.Count;

				print("B." + (int)(100 * curLoad) + "%");
				if (curLoad > BATTERY_MAX_LOAD)
				{
					run(after);
				}
			}
		}

		public void batteryCharge(string type)
		{
			Echo("batteryCharge: " + type);
			ChargeMode mode;
			switch (type)
			{
				case "Recharge":
					mode = ChargeMode.Recharge;
					break;
				case "Discharge":
					mode = ChargeMode.Discharge;
					break;
				//.............
				case "Auto":
					mode = ChargeMode.Auto;
					break;
				default:
					return;
			}
			List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
			reScanObjectsLocal(batteries);
			foreach (var bat in batteries)
			{
				bat.ChargeMode = mode;
			}
		}

		public Dictionary<string, int> getCurrentInventory()
		{
			List<IMyTerminalBlock> cargo_blocks = new List<IMyTerminalBlock>();
			reScanObjectsLocal<IMyTerminalBlock>(cargo_blocks, b => b.HasInventory);
			Dictionary<string, int> result = new Dictionary<string, int>();
			var items = new List<MyInventoryItem>();
			foreach(var block in cargo_blocks){
				block.GetInventory(0).GetItems(items);
				foreach (var item in items){
					var itemName = getName(item.Type);
					if (result.ContainsKey(itemName)){
						result[itemName] += (int)item.Amount;
					}
					else{
						result.Add(itemName, (int)item.Amount);
					}
				}
			}
			
			return result;
		}

		public void showInventory(IMyTextSurface display, Dictionary<string, int> items, int strsize){

			string itemStr = "";
			
			foreach(var i in items){
				string t = new string(' ', strsize - i.Key.Length - i.Value.ToString().Length);
				itemStr += i.Key + ":" + t + i.Value.ToString() + "\n";
				//test strsize - add logic if itemStr larger than strsize
			}
			display.WriteText(itemStr);
		}

		public Program()
		{
			// Set the script to run every 100 ticks, so no timer needed.
			Runtime.UpdateFrequency = UpdateFrequency.Update100;
		}

		public void fixGridState()
		{
			reScanObjectsLocal(allTBlocks);
			//todo save armor block state

		}

		public string getMyCoords()
		{
			string result = "";
			result = Me.GetPosition().ToString();
			return result;
		}

		public bool isAttacked()
		{
			//is turrets locked and target is grid
			List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
			reScanObjects(turrets);
			bool isTarget = false;
			foreach (IMyLargeTurretBase t in turrets)
			{
				if (t.HasTarget)
				{
					MyDetectedEntityInfo trg = t.GetTargetedEntity();
					if (trg.Type != MyDetectedEntityType.None || trg.Type != MyDetectedEntityType.FloatingObject || trg.Type != MyDetectedEntityType.Meteor || trg.Type != MyDetectedEntityType.Planet)
					{
						targets.Add(trg);
						isTarget = true;
					}
				}
			}
			bool isLocked = false;
			if (lockedState)
			{        //lockedState initiate by cockpit, run PB with argument "locked"
				isLocked = true;
			}

			bool isLargeDamage = false;
			//isLargeDamage = (damagedBlockRatio > (damageTreshold/100));

			bool isDestroyedBlocks = false;
			if (checkDestroyedBlocks)
			{
				List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
				reScanObjectsLocal(currentState);
				isDestroyedBlocks = currentState.Count() != allTBlocks.Count();
				destroyedAmount = allTBlocks.Count() - currentState.Count();
			}

			return (isTarget || isLocked || isLargeDamage || isDestroyedBlocks);
		}

		public string getDamagedBlocks()
		{
			List<string> result = new List<string>();
			List<IMySlimBlock> slim = new List<IMySlimBlock>();
			List<IMyTerminalBlock> grid = new List<IMyTerminalBlock>();

			double damagedCounter = 0;

			reScanObjectsLocal(grid);

			foreach (IMyTerminalBlock terminalBlock in grid)
			{
				IMySlimBlock slimBlock = terminalBlock.CubeGrid.GetCubeBlock(terminalBlock.Position);
				slim.Add(slimBlock);
				if (slimBlock.CurrentDamage > 0)
				{
					damagedCounter = damagedCounter + 1;
					result.Add("\"" + terminalBlock.DisplayNameText + "\"");
				}
			}
			damagedBlockRatio = damagedCounter / allTBlocks.Count();
			return String.Join(",", result);
		}

		public bool isDamaged()
		{
			return getDamagedBlocks() != "";
		}

		public bool isLowAmmo()
		{
			bool result = false;
			//todo
			return result;
		}

		public double getGridBatteryCharge()
		{
			List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
			reScanObjectsLocal(batteries);
			float current_capacity = 0;
			float max_capacity = 0;
			foreach (IMyBatteryBlock b in batteries)
			{
				current_capacity = current_capacity + b.CurrentStoredPower;
				max_capacity = max_capacity + b.MaxStoredPower;
			}
			return Math.Round((max_capacity == 0 ? 1 : current_capacity / max_capacity) * 100);
		}

		public double getCurrentGasAmount(string gasType)
		{
			//gasType {"Oxygen","Hydrogen"}                                            
			List<IMyGasTank> tanks = new List<IMyGasTank>();
			reScanObjectsLocal(tanks, x => x.BlockDefinition.SubtypeId.Contains(gasType));
			double current_amount = 0;
			double max_capacity = 0;
			foreach (IMyGasTank t in tanks)
			{
				current_amount += t.Capacity * t.FilledRatio;
				max_capacity += t.Capacity;
			}
			return Math.Round((max_capacity == 0 ? 1 : current_amount / max_capacity) * 100);
		}

		public double getUsedCargoSpace() //% of max
		{
			double maxVolume = 0;
			double currentVolume = 0;
			List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
			reScanObjectsLocal(cargos);
			foreach (IMyCargoContainer container in cargos)
			{
				maxVolume = maxVolume + (double)container.GetInventory(0).MaxVolume;
				currentVolume = currentVolume + (double)container.GetInventory(0).CurrentVolume;
			}

			return Math.Round((maxVolume == 0 ? 1 : currentVolume / maxVolume) * 100);
		}

		public double getGridVelocity()
		{
			double result = 0;
			List<IMyShipController> controls = new List<IMyShipController>();
			reScanObjectsLocal(controls);
			result = (double)controls[0].GetShipVelocities().LinearVelocity.Length();
			return result;
		}

		public bool isLowFuel(out string ftype)
		{
			bool result = false;
			//check battery charges
			ftype = "";
			List<string> ftypes = new List<string>();
			if (getGridBatteryCharge() < energyTreshold)
			{
				result = true;
				ftypes.Add("\"energy\"");
			}
			//check gas
			if (getCurrentGasAmount("Hydrogen") < gasTreshold)
			{
				result = true;
				ftypes.Add("\"gas\"");
			}
			//todo check uranium (if reactors)

			if (ftypes.Count > 0)
			{
				ftype = "[" + String.Join(",", ftypes) + "]";
			}
			return result;
		}

		public string getEnemyTargetsData()
		{
			string result = "";
			List<string> t = new List<string>();
			foreach (MyDetectedEntityInfo target in targets)
			{
				result = result + "{\"Name\":\"" + target.Name + "\",";
				result = result + "\"Type\":\"" + target.Type.ToString() + "\",";
				result = result + "\"Position\":\"" + target.Position.ToString() + "\"}";
				t.Add(result);
				result = "";
			}
			if (t.Count > 0)
			{
				result = "[" + String.Join(",", t) + "]";
			}
			return result;
		}

		public void Setup()
		{
			reScanObjectsLocal(antenna);

			if (antenna.Count() > 0)
			{
				Echo("Setup complete");
				setupcomplete = true;
			}
			else
			{
				Echo("Setup failed. No antenna found");
			}
		}

		public void Main(string arg)
		{
			if (arg != "")
			{
				//change parameters                
				// .ToList<String>()

				if (arg.Contains("=")) //todo: remove
				{
					List<string> args = arg.Split('=').Select(a => a.Trim()).ToList();
					string cmd = args[0];
					string val = args[1];

					switch (cmd)
					{
						case "ammo":
							//todo: set ammo request
							break;
						case "items":
							//todo: set items request
							break;
						case "energy":
							energyTreshold = Int32.Parse(val);
							break;
						case "gas":
							gasTreshold = Int32.Parse(val);
							break;
						case "damage":
							damageTreshold = Int32.Parse(val);
							break;
						case "uranium":
							uraniumTreshold = Int32.Parse(val);
							break;
						default:
							Echo("Unknown argument");
							break;
					}
				}

				arg += ",,";
				var props = arg.Split(',');

				switch (props[0])
				{
					case "Battery":
						batteryLoad(props[1]);
						break;
					case "cargoSave":
						cargoSave(props[1]);
						break;
					case "cargoLoad":
						cargoLoad(props[1], props[2]);
						break;
					case "connect":
						connect(props[1], props[2]);
						break;
					case "batteryCharge":
						batteryCharge(props[1]);
						break;
					case "locked":
						lockedState = true;
						Echo("Grid is locked");
						break;
					case "unlocked":
						lockedState = false;
						Echo("Manual override lock status");
						break;
					case "fix":
						fixGridState();
						Echo("Grid state saved");
						break;
					default:
						break;
				}
			}

			if (!setupcomplete)
			{
				//If setupcomplete is false, run Setup method.
				Echo("Running setup");
				Setup();
				fixGridState();
			}
			else
			{
				targets.Clear();
				string statusMessage = "";
				string ftype;
				List<string> events = new List<string>();
				List<IMyTextPanel> status_displays = new List<IMyTextPanel>();
				List<IMyTextPanel> request_displays = new List<IMyTextPanel>();

				statusMessage = "\"green\"";
				if (isDamaged() || isLowAmmo() || isLowFuel(out ftype))
				{
					statusMessage = "\"orange\"";
					if (isLowAmmo())
					{
						//todo check what type of ammo need and how many, add to details
						string tmp = "{\"Event\":\"lowAmmo\"}";
						events.Add(tmp);
					}
					if (isDamaged())
					{
						string tmp = "{\"event\":\"damaged\"";
						tmp = tmp + ",\"blocks\":[" + getDamagedBlocks() + "]";
						tmp = tmp + "}";
						events.Add(tmp);
					}
					if (isLowFuel(out ftype))
					{
						string tmp = "{\"event\":\"lowFuel\"";
						tmp = tmp + ",\"fueltype\":" + ftype;
						tmp = tmp + "}";
						events.Add(tmp);
					}
				}
				if (isAttacked())
				{
					statusMessage = "\"red\"";
					string tmp = "{\"Event\":\"underAttack\"";
					string t = "";
					if (lockedState)
					{
						tmp = tmp + ",\"Locked\": \"true\"";
					}
					if (damagedBlockRatio > 0)
					{
						tmp = tmp + ",\"Damaged\": " + (damagedBlockRatio * 100).ToString();
					}

					if (destroyedAmount > 0)
					{
						tmp = tmp + ",\"Destroyed\": " + destroyedAmount.ToString();
					}

					t = getEnemyTargetsData();
					if (t != "")
					{
						tmp = tmp + ",\"Targets\":" + t;
						tmp = tmp + "}";
					}
					events.Add(tmp);
				}

				statusMessage = "{\"Name\":\"" + Me.CubeGrid.CustomName + "\",\"Status\":" + statusMessage;

				statusMessage = statusMessage + ",\"Info\":[";
				statusMessage = statusMessage + "{\"GasAmount\":" + getCurrentGasAmount("Hydrogen") + "},";
				statusMessage = statusMessage + "{\"BatteryCharge\":" + getGridBatteryCharge() + "},";
				statusMessage = statusMessage + "{\"CargoUsed\":" + getUsedCargoSpace() + "},";
				statusMessage = statusMessage + "{\"Position\":\"" + getMyCoords() + "\"},";
				statusMessage = statusMessage + "{\"Velocity\":" + getGridVelocity() + "}";
				statusMessage = statusMessage + "]";

				if (events.Count > 0)
				{
					statusMessage = statusMessage + ",\"Events\":[";
					foreach (string e in events)
					{
						statusMessage = statusMessage + e + ",";
					}
					statusMessage = statusMessage.Remove(statusMessage.Length - 1) + "]";
				}
				//finish message
				statusMessage = statusMessage + "}";
				Echo(statusMessage);

				//reScanObjectGroupLocal(status_displays, StatusTag);
				reScanObjectGroupLocal(request_displays, RequestTag);

				//Me.CustomData = statusMessage;
				//string cmdTag = commandChannelTag + "." + Me.CubeGrid.CustomName;
				//statusListener = IGC.RegisterBroadcastListener(statusChannelTag);

				IGC.SendBroadcastMessage(statusChannelTag, statusMessage);
				commandListener = IGC.RegisterBroadcastListener(commandChannelTag);

				while (commandListener.HasPendingMessage)
				{
					string message;
					MyIGCMessage newRequest = commandListener.AcceptMessage();
					if (commandListener.Tag == commandChannelTag)
					{
						if (newRequest.Data is string)
						{
							message = newRequest.Data.ToString();
							foreach (IMyTextSurface d in request_displays)
							{
								d.WriteText(message);
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