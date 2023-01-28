using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;

// ---------------------------------------------------------- json parser ----------------------------------------------------
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

    public Vector3D ToVector3D(bool pretty = true)
    {
        return new Vector3D(((JsonPrimitive)Value["X"]).GetValue<double>(), ((JsonPrimitive)Value["Y"]).GetValue<double>(), ((JsonPrimitive)Value["Z"]).GetValue<double>());
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
        //Func<object, JsonObject> Generator = JsonObject.NewJsonObject("", readOnly);
        var trimChars = new char[] { '"', '\'', ' ', '\n', '\r', '\t', '\f' };
        string Key = "";
        bool endOf = false;
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
                //Console.WriteLine("Expecting Value...");
                //Console.WriteLine("Found " + Serialized[charIndex] + " (" + charIndex + ")");
                switch (foundChar)
                {
                    case '[':
                        CurrentNestedJsonObject = new JsonList(Key);
                        if (JsonStack.Count > 0)
                            JsonStack.Peek().Add(CurrentNestedJsonObject as JsonElement);
                        JsonStack.Push(CurrentNestedJsonObject);
                        //Console.WriteLine("List started");
                        break;
                    case '{':
                        //Console.WriteLine("Found new JsonObject");
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
                        if (!endOf)
                        {
                            var value = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                            //Console.WriteLine("value is: '" + value + "'");
                            JsonStack.Peek().Add(new JsonPrimitive(Key, value));
                        }
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
                endOf = false;
            }
            else if (Expected == JSONPart.KEY)
            {
                endOf = false;
                //Console.WriteLine("Expecting Key...");
                //Console.WriteLine("Found " + Serialized[charIndex] + " (" + charIndex + ")");

                switch (Serialized[charIndex])
                {
                    case ':':
                        Key = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                        //Console.WriteLine("key is: '" + Key + "'");
                        //Generator = JsonObject.NewJsonObject(Key, readOnly);
                        Expected = JSONPart.VALUE;
                        expectedDelims = valueDelims;
                        break;
                    case '}':
                        //Console.WriteLine("Leaving JsonObject...");
                        if (charIndex < Serialized.Length - 1 && Serialized[charIndex + 1] == ',')
                            charIndex++;
                        LastNestedJsonObject = JsonStack.Pop();
                        isInsideList = JsonStack.Count == 0 || JsonStack.Peek() is JsonList;
                        if (isInsideList)
                        {
                            Key = null;
                            endOf = true;
                            Expected = JSONPart.VALUE;
                            expectedDelims = valueDelims;
                        }
                        break;
                    default:
                        //Console.WriteLine($"Invalid character found: '{Serialized[charIndex]}', expected ':'!");
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