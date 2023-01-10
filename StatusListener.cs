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
namespace SpaceEngineers.UWBlockPrograms.StatusListener
{
    public sealed class Program : MyGridProgram
    {
        // Your code goes between the next #endregion and #region
        #endregion

        string LogTag = "[LOG STATUS]";
        string MapTag = "[MAP]";
        int LogMaxCount = 100;


        bool setupcomplete = false;
        List<IMyRadioAntenna> antenna;
        IMyBroadcastListener statusListener;

        string statusChannelTag = "RDOStatusChannel";
        string commandChannelTag = "RDOCommandChannel";

        Dictionary<string, ObjectInfo> nameToObject = new Dictionary<string, ObjectInfo>();
        List<ObjectInfo> objects = new List<ObjectInfo>();

        Log logger;

        public class ObjectInfo
        {
            public string name;
            public string status;
            public Vector3D position;
            public void Update(JsonObject info)
            {
                if (info.ContainsKey("Status"))
                {
                    status = info["Status"].ToString();
                }

                if (info.ContainsKey("Position"))
                {
                    var pos = info["Position"] as JsonObject;

                    position = new Vector3D(
                        ((JsonPrimitive)pos["X"]).GetValue<double>(),
                        ((JsonPrimitive)pos["Y"]).GetValue<double>(),
                        ((JsonPrimitive)pos["Z"]).GetValue<double>()
                    );
                }
            }
            public ObjectInfo(string name)
            {
                this.name = name;
            }

            internal JsonObject toJson()
            {
                var info = new JsonObject("");

                info.Add(new JsonPrimitive("Name", name));
                info.Add(new JsonPrimitive("Status", status));
                info.Add(new JsonObject("Position", position));

                return info;
            }
        }

        public void PrintMap()
        {
            logger.write("PRINT MAP " + objects.Count());
            var listResult = new JsonList("");
            objects.ForEach(obj => listResult.Add(obj.toJson()));
            var map = listResult.ToString();

            List<IMyTextPanel> surfaces = new List<IMyTextPanel>();
            reScanObjectGroupLocal(surfaces, MapTag);
            logger.write(map);
            surfaces.ForEach(s => s.WriteText(map));
        }

        public Program()
        {
            // Set the script to run every 100 ticks, so no timer needed.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            logger = new Log(this);
        }

        public void Setup()
        {
            List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(list, x => x.CubeGrid == Me.CubeGrid);
            antenna = list.ConvertAll(x => (IMyRadioAntenna)x);

            if (antenna.Count() > 0)
            {
                logger.write("Setup complete");
                setupcomplete = true;
            }
            else
            {
                logger.write("Setup failed. No antenna found");
            }
        }

        public void Use(JsonObject jsonData)
        {
            if (!jsonData.ContainsKey("Name")) return;
            var name = jsonData["Name"].ToString();
            ObjectInfo obj;

            if (nameToObject.ContainsKey(name))
            {
                obj = nameToObject[name];
            }
            else
            {
                obj = new ObjectInfo(name);
                objects.Add(obj);
                nameToObject[name] = obj;
            }
            Echo(jsonData.ToString());
            obj.Update(jsonData);
        }

        public void Main(string arg, UpdateType updateSource)
        {
            // If setupcomplete is false, run Setup method.
            if (!setupcomplete)
            {
                logger.write("Running setup");
                Setup();
                return;
            }

            PrintMap();

            statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
            while (statusListener.HasPendingMessage)
            {
                MyIGCMessage newStatus = statusListener.AcceptMessage();
                if (statusListener.Tag == statusChannelTag)
                {
                    if (newStatus.Data is string)
                    {
                        logger.write((string)newStatus.Data);
                        JsonObject jsonData;
                        try
                        {
                            jsonData = (new JSON((string)newStatus.Data)).Parse() as JsonObject;
                        }
                        catch (Exception e) // in case something went wrong (either your json is wrong or my library has a bug :P)
                        {
                            logger.write("There's somethign wrong with your json: " + e.Message);
                            return;
                        }

                        Use(jsonData);
                    }
                }
            }

            
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
                    result = Key + (pretty ? ": " : ":");
                result += "[";
                foreach (var jsonObj in Values)
                {
                    var childResult = jsonObj.ToString(pretty);
                    if (pretty)
                        childResult = childResult.Replace("\n", "\n  ");
                    result += (pretty ? "\n  " : "") + childResult + ",";
                }
                result = result.Substring(0, result.Length - 1);
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
                    result = Key + (pretty ? ": " : ":");
                result += "{";
                foreach (var kvp in Value)
                {
                    var childResult = kvp.Value.ToString(pretty);
                    if (pretty)
                        childResult = childResult.Replace("\n", "\n  ");
                    result += (pretty ? "\n  " : "") + childResult + ",";
                }
                result = result.Substring(0, result.Length - 1);
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
                    result = Key + (pretty ? ": " : ":");

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
                                var value = Serialized.Substring(LastCharIndex + 1, charIndex - LastCharIndex - 1).Trim(trimChars);
                                //Console.WriteLine("value is: '" + value + "'");
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

        public class LogEntry
        {
            public string log;
            public int count;

            public LogEntry(string info)
            {
                log = info;
                count = 1;
            }

            public void inc()
            {
                count++;
            }
            public override string ToString() => $"x{count}: {log}";
        }

        public class Log
        {
            List<IMyTextPanel> surfaces = new List<IMyTextPanel>();
            private Program parent;
            List<LogEntry> logs = new List<LogEntry>();


            public void rescan()
            {
                parent.reScanObjectGroupLocal(surfaces, parent.LogTag);
            }
            public Log(Program program)
            {
                this.parent = program;
            }

            public void write(String info)
            {
                parent.Echo(info);
                rescan();
                if (logs.Count > 0 && logs[0].log == info)
                {
                    logs[0].inc();
                }
                else
                {
                    logs.Insert(0, new LogEntry(info));
                }

                if (logs.Count > parent.LogMaxCount)
                {
                    logs.RemoveRange(parent.LogMaxCount, logs.Count - parent.LogMaxCount);
                }

                var result = String.Join("\n", logs);
                surfaces.ForEach((surface) => surface.WriteText(result));
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
        public void reScanObjectsLocal<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
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
        public void reScanObjects<T>(List<T> result, Func<T, bool> check) where T : class, IMyTerminalBlock
        {
            GridTerminalSystem.GetBlocksOfType<T>(result, check);
        }


        #region PreludeFooter
    }
}
#endregion