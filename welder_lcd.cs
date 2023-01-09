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

public Program(){
	// Set the script to run every 100 ticks, so no timer needed.
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public Dictionary<string, int> getCurrentInventory()
{
	List<IMyTerminalBlock> cargo_blocks = new List<IMyTerminalBlock>();
	reScanObjectsLocal<IMyTerminalBlock>(cargo_blocks, b => b.HasInventory);
	Dictionary<string, int> result = new Dictionary<string, int>();
    var items = new List<MyInventoryItem>();
	cargo_blocks.GetInventory(0).GetItems(items);
	foreach (var item in items){
		var itemName = getName(item.Type);
		if (result.ContainsKey(itemName)){
			result[itemName] += (int)item.Amount;
		}
		else{
			result.Add(itemName, (int)item.Amount);
		}
	}
	
	return result;
}

public void showInventory(IMyTextSurface display, Dictionary<string, int> items, int strsize){

	string itemStr = "";
	
	foreach(i in items){
		itemStr += i.Key + ":" + string(" ", strsize - i.Key.Length - i.Value.ToString().Length) + i.Value.ToString() + "\n";
		//test strsize - add logic if itemStr larger than strsize
	}
	display.WriteText(itemStr);
}

public void unloadCargo(){
	
}

public void Main(string arg){

}