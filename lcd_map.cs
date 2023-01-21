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

void initDrawSurface(IMyTextSurface s)
{
	s.ContentType = ContentType.SCRIPT;
	s.Script = "";
}

public string getName(MyItemType type)
{
	return type.TypeId + '.' + type.SubtypeId;
}

public Program()
{
	// Set the script to run every 100 ticks, so no timer needed.
	Runtime.UpdateFrequency = UpdateFrequency.Update10;
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

public class gridPosition
{
    public bool isEnemy;
    public Vector3D position;
    public int type; //0 - static, 1 - large, 2 - small and characters

    public gridPosition(double x, double y, double z, bool enemyFlag, string gridType)
    {
        position = new Vector3D(x,y,z);
        isEnemy = enemyFlag;
        if(gridType.Contains("Static"))
        {
            type = 0;
        }
        else if(gridType.Contains("Large"))
        {
            type = 1;
        }
        else
        {
            type = 2;
        }
    }

    public void drawGrid2D(MySpriteDrawFrame frame, float maxRange, Vector2 surfaceSize, gridPosition myGrid)
    {
        Color baseColor = new Color(0, 0, 200);
        Color borderColor = new Color(0, 0, 100);
        var rPos = myGrid.position - position;
        double spriteX = rPos.X*(surfaceSize.X/maxRange);
        double spriteY = rPos.Y*(surfaceSize.Y/maxRange);
        double spriteZ = rPos.X*(surfaceSize.X/maxRange);

        if (isEnemy)
        {
            baseColor = new Color(200, 0, 0);
            borderColor = new Color(100, 0, 0);
        }

        if (rPos.Length() <= maxRange)
        {
            switch (type)
            {
                case 0: //square
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2((float)spriteX, (float)spriteY), new Vector2(10f, 10f), borderColor, null, TextAlignment.CENTER, 0f));
                    frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2((float)spriteX, (float)spriteY), new Vector2(9f, 9f), baseColor, null, TextAlignment.CENTER, 0f));
                    break;
                case 1: //triangle

                    break;
                case 2: //circle
                    frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2((float)spriteX, (float)spriteY), new Vector2(10f, 10f), borderColor, null, TextAlignment.CENTER, 0f));
                    frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2((float)spriteX, (float)spriteY), new Vector2(9f, 9f), baseColor, null, TextAlignment.CENTER, 0f));
                    break;
            }
        }
        else
        {
            //todo - bordermarker?
        }
    }
}
/*
void drawMapBorder(MySpriteDrawFrame frame, double maxRange, Vector2 surfaceSize)
{

}

void drawMapText(MySpriteDrawFrame frame, double maxRange, Vector2 surfaceSize, List<string> mapOptions = null)
{

}

void drawMap()
{
}

void mapScaleUp() //+1km
{

}

void mapScaleDown() //-1km
{

}
*/

void func()
{

}

List<gridPosition> parseGridPositions(string positionsList, bool isEnemy)
{
    List<gridPosition> result = new List<gridPosition>();
    JsonList jsonData;
    try
    {
        jsonData = (new JSON(positionsList)).Parse() as JsonList;
    }
    catch (Exception e) // in case something went wrong (either your json is wrong or my library has a bug :P)
    {
        Echo("There's somethign wrong with your json: " + e.Message);
        return null;
    }
    foreach(var elem in jsonData)
    {
        //result.Add(new gridPosition(double.Parse((JsonPrimitive)e["X"]), double.Parse((JsonPrimitive)e["Y"]), double.Parse((JsonPrimitive)e["Z"]), isEnemy, "Small"));
        Echo(elem.ToString());
    }
    return result;
}

public void Main(string arg)
{
	List<IMyTextPanel> displays = new List<IMyTextPanel> ();
	List<IMyTextPanel> maps = new List<IMyTextPanel> ();
	reScanObjectGroupLocal(displays, "[DRAW]");
	reScanObjectGroupLocal(maps, "[MAP]");
    Vector3D myPosition = Me.GetPosition();
    double maxRange = 1000;
	int c = 0;
    List<gridPosition> allyGrids = new List<gridPosition>();
    gridPosition myGrid = new gridPosition(myPosition.X, myPosition.Y, myPosition.Z, false, "Small");


    if (maps.Count() == 0)
    {
        Echo("No map data detected");
        return;
    }
    
	foreach(var elem in displays)
	{	
		initDrawSurface(elem);
        using (var frame = elem.DrawFrame())
        {
            var allyGrid = parseGridPositions(maps[0].GetText(), false);
            foreach(var ally in allyGrids)
            {
                ally.drawGrid2D(frame, (float)maxRange, elem.SurfaceSize, myGrid);
            }
        }

	/*	Vector2 tst = new Vector2(0f, 0f);
		IMyTextSurface d = elem as IMyTextSurface;
        using (var frame = d.DrawFrame())
        {
            var text = "No damage ";
			var sprite = MySprite.CreateText(text, "Debug", new Color(255,150,0), 1.5f, TextAlignment.LEFT);
			sprite.Position = new Vector2(10f,10f);
            frame.Add(sprite);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(0f, 20f), new Vector2(512f, 1f), new Color(255,150,0), "test", TextAlignment.CENTER, 0f));
		}*/

		Echo(elem.SurfaceSize.ToString());
		c += 1;
	}
	Echo("displays:" + c.ToString());
}

interface IJsonNonPrimitive
{
    void Add(JsonElement child);
}
public class JsonList : JsonElement, IJsonNonPrimitive, ICollection<JsonElement>
{
    private List<JsonElement> Values;

    public override JSONValueType ValueType
    {
        get
        {
            return JSONValueType.LIST;
        }
    }

    public JsonElement this[int i]
    {
        get
        {
            return Values[i];
        }
    }

    public int Count
    {
        get
        {
            return Values.Count;
        }
    }

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public JsonList(string key)
    {
        Key = key;
        Values = new List<JsonElement>();
    }

    public void Add(JsonElement value)
    {
        Values.Add(value);
    }


    public override string ToString(bool pretty)
    {
        var result = "";
        if (Key != "")
            result = "\"" + Key + (pretty ? "\": " : "\":");
        result += "[";
        foreach (var jsonObj in Values)
        {
            var childResult = jsonObj.ToString(pretty);
            if (pretty)
                childResult = childResult.Replace("\n", "\n  ");
            result += (pretty ? "\n  " : "") + childResult + ",";
        }
        if (Values.Count > 0)
        {
            result = result.Substring(0, result.Length - 1);
        }
        result += (pretty ? "\n]" : "]");

        return result;
    }

    public void Clear()
    {
        Values.Clear();
    }

    public bool Contains(JsonElement item)
    {
        return Values.Contains(item);
    }

    public void CopyTo(JsonElement[] array, int arrayIndex)
    {
        Values.CopyTo(array, arrayIndex);
    }

    public bool Remove(JsonElement item)
    {
        return Values.Remove(item);
    }

    private IEnumerable<JsonElement> Elements()
    {
        foreach (var value in Values)
        {
            yield return value;
        }
    }

    public IEnumerator<JsonElement> GetEnumerator()
    {
        return Elements().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class JsonObject : JsonElement, IJsonNonPrimitive
{
    private Dictionary<string, JsonElement> Value;

    public override JSONValueType ValueType
    {
        get
        {
            return JSONValueType.NESTED;
        }
    }

    public JsonElement this[string key]
    {
        get
        {
            return Value[key];
        }
    }

    public Dictionary<string, JsonElement>.KeyCollection Keys
    {
        get
        {
            return Value.Keys;
        }
    }

    public bool ContainsKey(string key)
    {
        return Value.ContainsKey(key);
    }

    public JsonElement GetValueOrDefault(string key)
    {
        if (ContainsKey(key))
            return this[key];
        return null;
    }

    public JsonObject(string key = "")
    {
        Key = key;
        Value = new Dictionary<string, JsonElement>();
    }
    public JsonObject(string key = "", Vector3D? vector = null)
    {
        Key = key;
        Value = new Dictionary<string, JsonElement>();
        if (vector != null)
        {
            Vector3D tmp = (Vector3D)vector;
            Add(new JsonPrimitive("X", tmp.X));
            Add(new JsonPrimitive("Y", tmp.Y));
            Add(new JsonPrimitive("Z", tmp.Z));
        }
    }

    public void Add(JsonElement jsonObj)
    {
        Value.Add(jsonObj.Key, jsonObj);
    }

    public override string ToString(bool pretty = true)
    {
        var result = "";
        if (Key != "" && Key != null)
            result = "\"" + Key + (pretty ? "\": " : "\":");
        result += "{";
        foreach (var kvp in Value)
        {
            var childResult = kvp.Value.ToString(pretty);
            if (pretty)
                childResult = childResult.Replace("\n", "\n  ");
            result += (pretty ? "\n  " : "") + childResult + ",";
        }
        if (Value.Count > 0)
        {
            result = result.Substring(0, result.Length - 1);
        }
        result += (pretty ? "\n}" : "}");

        return result;
    }
}

public class JsonPrimitive : JsonElement
{
    public string Value = null;
    public double? dValue = null;
    public int? iValue = null;

    public override JSONValueType ValueType
    {
        get
        {
            return JSONValueType.PRIMITIVE;
        }
    }

    public JsonPrimitive(string key, string value)
    {
        Key = key;
        Value = value;
    }
    public JsonPrimitive(string key, double value)
    {
        Key = key;
        dValue = value;
    }
    public JsonPrimitive(string key, float value)
    {
        Key = key;
        dValue = value;
    }
    public JsonPrimitive(string key, int value)
    {
        Key = key;
        iValue = value;
    }

    public override void SetKey(string key)
    {
        base.SetKey(key);
    }

    public void SetValue(string value)
    {
        Value = value;
    }


    public T GetValue<T>()
    {
        object value = null;
        if (typeof(T) == typeof(string))
        {
            value = Value;
        }
        else if (typeof(T) == typeof(int))
        {
            value = Int32.Parse(Value);
        }
        else if (typeof(T) == typeof(float))
        {
            value = Single.Parse(Value);
        }
        else if (typeof(T) == typeof(double))
        {
            value = Double.Parse(Value);
        }
        else if (typeof(T) == typeof(char))
        {
            value = Char.Parse(Value);
        }
        else if (typeof(T) == typeof(DateTime))
        {
            value = DateTime.Parse(Value);
        }
        else if (typeof(T) == typeof(decimal))
        {
            value = Decimal.Parse(Value);
        }
        else if (typeof(T) == typeof(bool))
        {
            value = Boolean.Parse(Value);
        }
        else if (typeof(T) == typeof(byte))
        {
            value = Byte.Parse(Value);
        }
        else if (typeof(T) == typeof(uint))
        {
            value = UInt32.Parse(Value);
        }
        else if (typeof(T) == typeof(short))
        {
            value = short.Parse(Value);
        }
        else if (typeof(T) == typeof(long))
        {
            value = long.Parse(Value);
        }
        /*else if (typeof(T) == typeof(List<JsonObject>))
        {
            var values = GetBody()?.Values;
            if (values == null)
                value = new List<JsonObject>();
            else
                value = new List<JsonObject>(values);
        }
        else if (typeof(T) == typeof(Dictionary<string, JsonObject>))
        {
            value = GetBody();
        }*/
        else
        {
            throw new ArgumentException("Invalid type '" + typeof(T).ToString() + "' requested!");
        }

        return (T)value;
    }

    public bool TryGetValue<T>(out T result)
    {
        try
        {
            result = GetValue<T>();
            return true;
        }
        catch (Exception)
        {
            result = default(T);
            return false;
        }
    }

    public override string ToString(bool pretty = true)
    {
        if (Value == null && iValue == null && dValue == null)
            return "";
        var result = "";
        if (Key != "" && Key != null)
            result = "\"" + Key + (pretty ? "\": " : "\":");

        if (Value != null)
        {
            result += "\"" + Value + "\"";
        }
        else
        {
            result += (iValue == null ? dValue : iValue).ToString();
        }


        return result;
    }
}
public abstract class JsonElement
{
    public enum JSONValueType { NESTED, LIST, PRIMITIVE }
    public string Key { get; protected set; }

    public bool IsPrimitive
    {
        get
        {
            return ValueType == JSONValueType.PRIMITIVE;
        }
    }
    public abstract JSONValueType ValueType { get; }

    public virtual void SetKey(string key)
    {
        Key = key;
    }

    public override string ToString()
    {
        return ToString(true);
    }

    public abstract string ToString(bool pretty = true);

}
public class JSON
{
    enum JSONPart { KEY, KEYEND, VALUE, VALUEEND }

    private int LastCharIndex;
    public string Serialized { get; private set; }
    private bool ReadOnly;

    public int Progress
    {
        get
        {
            if (Serialized.Length == 0) return 999;
            return 100 * Math.Max(0, LastCharIndex) / Serialized.Length;
        }
    }


    public JSON(string serialized, bool readOnly = true)
    {
        Serialized = serialized;
        ReadOnly = readOnly;
    }


    public JsonElement Parse()
    {
        LastCharIndex = -1;
        JSONPart Expected = JSONPart.VALUE;
        Stack<Dictionary<string, JsonElement>> ListStack = new Stack<Dictionary<string, JsonElement>>();
        Stack<IJsonNonPrimitive> JsonStack = new Stack<IJsonNonPrimitive>();
        IJsonNonPrimitive CurrentNestedJsonObject = null;
        IJsonNonPrimitive LastNestedJsonObject = null;
        var trimChars = new char[] { '"', '\'', ' ', '\n', '\r', '\t', '\f' };
        string Key = "";
        var keyDelims = new char[] { '}', ':' };
        var valueDelims = new char[] { '{', '}', ',', '[', ']' };
        var expectedDelims = valueDelims;
        var charIndex = -1;
        bool isInsideList = false;

        while (LastCharIndex < Serialized.Length - 1)
        {
            charIndex = Serialized.IndexOfAny(expectedDelims, LastCharIndex + 1);
            if (charIndex == -1)
                throw new UnexpectedCharacterException(expectedDelims, "EOF", LastCharIndex);

            char foundChar = Serialized[charIndex];
            if (Expected == JSONPart.VALUE)
            {
                switch (foundChar)
                {
                    case '[':
                        CurrentNestedJsonObject = new JsonList(Key);
                        JsonStack.Peek().Add(CurrentNestedJsonObject as JsonElement);
                        JsonStack.Push(CurrentNestedJsonObject);
                        break;
                    case '{':
                        CurrentNestedJsonObject = new JsonObject(Key);
                        if (JsonStack.Count > 0)
                            JsonStack.Peek().Add(CurrentNestedJsonObject as JsonElement);
                        JsonStack.Push(CurrentNestedJsonObject);
                        Expected = JSONPart.KEY;
                        expectedDelims = keyDelims;
                        break;
                    case ',':
                    case '}':
                    case ']':
                        var value = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                        JsonStack.Peek().Add(new JsonPrimitive(Key, value));
                        if (foundChar == '}' || foundChar == ']')
                        {
                            /*if (foundChar == ']')
                            {
                                Console.WriteLine("Leaving List...");
                            }
                            else
                            {
                                Console.WriteLine("Leaving JsonObject...");
                            }*/
                            if (charIndex < Serialized.Length - 1 && Serialized[charIndex + 1] == ',')
                                charIndex++;
                            LastNestedJsonObject = JsonStack.Pop();
                        }
                        break;
                }

                isInsideList = JsonStack.Count == 0 || JsonStack.Peek() is JsonList;
                if (isInsideList)
                {
                    Key = null;
                    Expected = JSONPart.VALUE;
                    expectedDelims = valueDelims;
                }
                else
                {
                    Expected = JSONPart.KEY;
                    expectedDelims = keyDelims;
                }
            }
            else if (Expected == JSONPart.KEY)
            {

                switch (Serialized[charIndex])
                {
                    case ':':
                        Key = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                        Expected = JSONPart.VALUE;
                        expectedDelims = valueDelims;
                        break;
                    case '}':
                        if (charIndex < Serialized.Length - 1 && Serialized[charIndex + 1] == ',')
                            charIndex++;
                        LastNestedJsonObject = JsonStack.Pop();
                        break;
                    default:
                        break;
                }
            }

            LastCharIndex = charIndex;
        }
        if (JsonStack.Count > 0)
        {
            throw new ParseException("StackCount " + JsonStack.Count, LastCharIndex);
        }
        return LastNestedJsonObject as JsonElement;
    }

    private class ParseException : Exception
    {
        public ParseException(string message, int position = -1)
            : base("PARSE ERROR" + (position == -1 ? "" : " after char " + position.ToString()) + ": " + message) { }

    }

    private class UnexpectedCharacterException : ParseException
    {
        public UnexpectedCharacterException(char[] expected, string received, int position = -1)
            : base("Expected one of [ '" + string.Join("', '", expected) + "' ] but received " + received + "!", position)
        { }

    }

}