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
	args = arg.Split(",").ToList().ForEach(p => p.Trim());
//rescan blocks

	List<IMyCameraBlock> aimCams = new List<IMyCameraBlock>(); //cameras
	List<IMySensorBlock> aimSensors = new List<IMySensorBlock>(); //sensors
	List<IMyDisplayPanel> aimDisplays = new List<IMyDisplayPanel>(); //aim lcds
	List<IMyDisplayPanel> aimModeDisplays = new List<IMyDisplayPanel>(); //aiming modes info lcds
	reScanObjectGroupLocal(aimCams, aimTag);
	reScanObjectGroupLocal(aimSensors, aimTag);
	reScanObjectGroupLocal(aimDisplays, aimTag);
	reScanObjectGroupLocal(aimModeDisplays, infoTag);

//init cameras - todo frontal only
	foreach(var cam in aimCam)
	{
		cam.EnableRaycast = true;
	}

//execute commands - autolock=on/off, autoaim=on/off, lock, release
	if (args.Count() > 0)
	{
		foreach(var a in args)
		{
			List<string> param = new List<string>();
			param = a.Split('=').ToList().ForEach(p => p.Trim());
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
					lockedTarget = null;
					Echo("Target released");
				break;

				default:
					Echo("Unknown command - skipped");
				break;
			}
		}
	}

//execute runtime	
	if (lockedTarget != null)
	{
		//raycast to predicted position
		//if target not found? - try N times with random deviations then breakLock
		//if 
		string targetInfo = "Target locked\n";
		targetInfo = "ID:" + Int64.Parse(lockedTarget.EntityId) + "\n";
		targetInfo = "Type:" + lockedTarget.Type.ToString() + "\n";
		targetInfo = "Position:" + lockedTarget.Position.ToString() + "\n";
		targetInfo = "VelocityVector:" + lockedTarget.Velocity.ToString() + "\n";
		targetInfo = "Velocity:" + lockedTarget.Velocity.Length().ToString() + "\n";
	}
	else
	{
		if (autoLock || isSearching) 
		{
			//raycast front 1km all aim cameras
			foreach(var cam in aimCams)
			{
				if (cam.CanScan(scanRange) && cam.TimeUntilScan(scanRange) == 0)
				{
					MyDetectedEntityInfo t = cam.raycast(scanRange);
					if (t != null && (t.Type == SmallGrid || t.Type == LargeGrid || t.Type == CharacterHuman || t.Type == CharacterOther))
					{
						if (detectAll || t.Relationship == Enemies)
						{
							lockedTarget = t;
							isSearching == false;
							break;
						}
					}
				}
			}
		}
	}
	if (lockedTarget != null)
	{
		//predict next position
	}
	if (autoAim)
	{
		//List<> gyros = new List<>();
		//set angular speed for gyros 
	}
}
