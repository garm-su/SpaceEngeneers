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
