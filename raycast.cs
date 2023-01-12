string aimTag = "[AIM]";
string infoTag = "[INFO]";

public List<string> actions = new List<string>();
public List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();

public MyDetectedEntityInfo lockedTarget;
public bool autoLock = false;
public bool autoAim = false;
public bool isSearching = false;
public bool detectAll = true;
public bool targetLost = false;
public double scanRange = 1000f;
public List<IMyUserControllableGun> guns = new List<IMyUserControllableGun>();

//--------------------------------- rescan and config functions -------------------------------------------------------------

public void reReadConfig(Dictionary<string, int> minResourses, String CustomData){
	minResourses.Clear();
	foreach (String row in CustomData.Split('\n')){
        	if (row.Contains(":")){
			var tup = row.Split(':');	
			if (tup.Length != 2){
                        	Echo("Err: " + row);
                        	continue;
                    	}
			minResourses[tup[0].Trim()] = Convert.ToInt32(tup[1].Trim());
        	}}}

public void reScanObjectExact<T>(List<T> result, String name) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName == name);}
public void reScanObjectExactLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName == name);}
public void reScanObjectGroupLocal<T>(List<T> result, String name) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && item.CustomName.Contains(name));}
public void reScanObjectGroup<T>(List<T> result, String name) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CustomName.Contains(name));}
public void reScanObjectsLocal<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid && check(item));}
public void reScanObjectsLocal<T>(List<T> result) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, item => item.CubeGrid == Me.CubeGrid);}
public void reScanObjects<T>(List<T> result) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result);}
public void reScanObjects<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
{GridTerminalSystem.GetBlocksOfType<T>(result, check);}

//---------------------------------------------------------------------------------------------------------------------------


public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string arg)
{
//parse args
	List<string> args = new List<string>();
	Echo(arg);
	if (arg != "")
	{
		args = arg.Split(',').ToList();
	}
//rescan blocks

	List<IMyCameraBlock> aimCams = new List<IMyCameraBlock>(); //cameras
	List<IMySensorBlock> aimSensors = new List<IMySensorBlock>(); //sensors
	List<IMyTextPanel> aimDisplays = new List<IMyTextPanel>(); //aim lcds
	List<IMyTextPanel> aimModeDisplays = new List<IMyTextPanel>(); //aiming modes info lcds
	reScanObjectGroupLocal(aimCams, aimTag);
	reScanObjectGroupLocal(aimSensors, aimTag);
	reScanObjectGroupLocal(aimDisplays, aimTag);
	reScanObjectGroupLocal(aimModeDisplays, infoTag);

//init cameras - todo frontal only
	foreach(var cam in aimCams)
	{
		cam.EnableRaycast = true;
	}

//execute commands - autolock=on/off, autoaim=on/off, lock, release
	if (args.Count() > 0)
	{
		foreach(var a in args)
		{
			List<string> param = new List<string>();
			param = a.Split('=').ToList();
			switch (param[0])
			{
				case "autolock":
					autoLock = (param[1] == "on");
				break;

				case "autoaim":
					autoAim = (param[1] == "on");
				break;

				case "detectAll":
					detectAll = (param[1] == "on");
				break;

				case "lock":
					isSearching = true;
					Echo("Locking target...");
				break;

				case "release":
					lockedTarget = new MyDetectedEntityInfo();
					Echo("Target released");
				break;

				default:
					Echo("Unknown command - skipped");
				break;
			}
		}
	}

//execute runtime	
	if (!lockedTarget.IsEmpty())
	{
		//raycast to predicted position
		//if target not found? - try N times with random deviations then breakLock
		//if 
		string targetInfo = "Target locked\n";
		targetInfo = targetInfo + "ID:" + lockedTarget.EntityId.ToString() + "\n";
		targetInfo = targetInfo + "Type:" + lockedTarget.Type.ToString() + "\n";
		targetInfo = targetInfo + "Position:" + lockedTarget.Position.ToString() + "\n";
		targetInfo = targetInfo + "VelocityVector:" + lockedTarget.Velocity.ToString() + "\n";
		targetInfo = targetInfo + "Velocity:" + lockedTarget.Velocity.Length().ToString() + "\n";
		Echo(targetInfo);
	}
	else
	{
		if (autoLock || isSearching) 
		{
			//raycast front 1km all aim cameras
			Echo("Locking target...");
			foreach(var cam in aimCams)
			{
				if (cam.CanScan(scanRange) && cam.TimeUntilScan(scanRange) == 0)
				{
					MyDetectedEntityInfo t = cam.Raycast(scanRange);
					if (!t.IsEmpty() && (t.Type == MyDetectedEntityType.SmallGrid || t.Type == MyDetectedEntityType.LargeGrid || t.Type == MyDetectedEntityType.CharacterHuman || t.Type == MyDetectedEntityType.CharacterOther))
					{
						if (detectAll || t.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies)
						{
							lockedTarget = t;
							isSearching = false;
							break;
						}
					}
				}
			}
		}
	}
	if (!lockedTarget.IsEmpty())
	{
		//predict next position
	}
	if (autoAim)
	{
		//List<> gyros = new List<>();
		//set angular speed for gyros 
	}
}
