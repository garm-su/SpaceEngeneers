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

void initDrawPanel(IMyTextSurface s)
{
//	s.ScriptBackgroundColor = _black;
	s.ContentType = ContentType.SCRIPT;
	s.Script = "";
}

//MySpriteDrawFrame
//frame = surface.DrawFrame();
//frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2(0f, 0f) * scale + centerPos, new Vector2(200f, 200f) * scale, _white, null, TextAlignment.CENTER, 0f)); // circle
//frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(cos * 0f - sin * 45f, sin * 0f + cos * 45f) * scale + centerPos, new Vector2(10f, 90f) * scale, _white, null, TextAlignment.CENTER, 0f + rotation)); // line
//frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2(-sin * -63f, cos * -63f) * scale + centerPos, new Vector2(200f, 200f) * scale, color, null, TextAlignment.CENTER, rotation)); // triangle
//frame.Add(new MySprite.CreateText("Test", "Debug", new Color(1f), 2f, TextAlignment.CENTER)); //text
//https://forum.keenswh.com/threads/api-or-example-script-for-the-new-lcd.7402966/
//steamapps\common\SpaceEngineers\Content\Textures\Sprites\
//IMyCockpit
//IMyTextSurfaceProvider
//https://github.com/malware-dev/MDK-SE/wiki/Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider

public void Main(string arg)
{
	List<IMyTextPanel> displays = new List<IMyTextPanel> ();
	reScanObjectGroupLocal(displays, "[DRAW]");
	
	foreach(var elem in displays)
	{	
		Vector2 tst = new Vector2(0f, 0f);
		IMyTextSurface d = elem as IMyTextSurface;
		initDrawPanel(d);
		MySpriteDrawFrame frame = d.DrawFrame();
		var sprite = new MySprite(SpriteType.TEXTURE, "Circle", color: new Color(200, 200, 200), size: new Vector2(100f, 100f));
        sprite.Position = new Vector2(256f,256f);
    	frame.Add(sprite);
		Echo(d.TextureSize.ToString());
		Echo(d.SurfaceSize.ToString());
		Echo(tst.ToString());
	}
}
