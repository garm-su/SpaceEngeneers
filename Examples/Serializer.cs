/**   
README:  
Any object that extends from Serializable can be converted to a string and back  
using obj.Serialize and Serializable.DeSerialize<Type>().  
This allows you to store objects in the storage variable or send objects to other grids.  
The following field types are supported:    
    Byte[], int, boolean, float, double, string, vector and anything that extends from Serializable  
  
The first part of this is script is an example of how you can share data between two grids using this method.  
However this script can do much more than that.  
  
You are free to use this in your scripts and upload them to the workshop   
but please give credit and link to the original workshop page in the description  
If you have any problems leave them in the comments of the workshop page  
Author: Tentacola  
*/  
  
IMyRadioAntenna antenna;  
Message<State> myMessage = new Message<State>();  
  
  
/// <summary>  
/// Example of serializable class contains an object's position and state(moving or stopped)  
/// </summary>  
public class Message<T> : Serializable where T: Serializable, new() {  
  
    public T status = new T();  
    public Vector3 pos;  
      
    /// <summary>  
    /// Saves fields to a fields variable that will be used to serialize Transform later 
    /// The following field types are supported:   
    ///     Byte[], int, boolean, float, double, string, vector and anything that extends from Serializable  
    /// </summary>  
    public override void SaveToFields () {  
        fields["status"] = new Field (this.status);  
        fields["pos"] = new Field (this.pos);  
    }  
  
    /// <summary>  
    /// Loads fields from a dictionary of fields  
    /// NOTE:  
    ///     Make sure that you are not trying to load fields that are not saved or are a different type!! 
    ///     You will get the error key not found if you do try to do this 
    /// </summary>  
    /// <param name="fields">Dictionary to load from</param>  
    public override void LoadFields (Dictionary<String, Field> fields) {  
        this.status = fields["status"].GetObject<T>();  
        this.pos = fields["pos"].GetVector3 ();  
    }  
  
    /// <summary>  
    /// Sets new position and changes status  
    /// </summary>  
    /// <param name="newPos">New position of the object</param>  
    public void Update (Vector3 newPos) {  
        pos = newPos;  
    }  
}  
  
/// <summary>  
/// Fields that are also serializable can also be used in a serializable class 
/// </summary>  
public class State : Serializable {  
      
    public string text = "Stopped";  
      
    public override void SaveToFields () {  
        fields["text"] = new Field (this.text);  
    }  
  
    public override void LoadFields (Dictionary<String, Field> fields) {  
        this.text = fields["text"].GetString ();  
    }  

    public override String ToString() {
        return this.text;
    }
}  
  
public Program () {  
    Runtime.UpdateFrequency = UpdateFrequency.Update1;  
  
    //Get antenna  
    List<IMyRadioAntenna> radioList = new List<IMyRadioAntenna> ();  
    GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna> (radioList);  
    antenna = radioList.First(); 
}  
  
public void Save() { 
}  
  
public void Main (string argument, UpdateType updateSource) {  
    Message<State> other = null;  
    if ((updateSource & (UpdateType.Antenna)) != 0) {  
        other = DeSerialize<Message<State>> (argument);  
        antenna.CustomName = "Received message\n State: " + other.status.text  
            + "\n Distance: " + (myMessage.pos - other.pos).Length();  
    }  
    if ((updateSource & (UpdateType.Update1)) != 0) {  
        myMessage.Update(antenna.GetPosition());  
        if (Math.Abs(Vector3.Subtract(myMessage.pos, antenna.GetPosition()).Length()) < 0.000001) {  
            myMessage.status.text = "stopped";  
        } else {  
            myMessage.status.text = "moving";  
        }  
        String tranformString = myMessage.Serialize();  
        antenna.TransmitMessage (tranformString, MyTransmitTarget.Everyone);  
    }  
}  
  
  
/**  
STOP EDITING HERE! THE REST IS USED TO PARSE, SERIALIZE AND DESERIALIZE OBJECTS  
Read the comments to get a better understanding or edit if you know what you are doing ;)  
You are free to use this in your scripts and upload them to the workshop   
but please give credit and link to the original workshop page in the description  
If you have any problems leave them in the comments of the workshop page  
Author: Tentacola  
*/  
  
  
  
/// <summary>  
/// Abstract class that allows classes that extend from it to serialize and deserialize their fields  
/// Usefull for storing data or sending data to other grids  
/// </summary>  
public abstract class Serializable {  
    protected Dictionary<String, Field> fields = new Dictionary<String, Field> ();  
    /// <summary>  
    /// Stores all fields that need to be serialized in the fields dictionary  
    /// </summary>  
    public abstract void SaveToFields ();  
  
    /// <summary>  
    /// Applies all fields stored in a dictionary  
    /// Remember fields that are not saved can not be loaded!!!  
    /// </summary>  
    /// <param name="fields">Dictionary to take load fields from</param>  
    public abstract void LoadFields (Dictionary<String, Field> fields);  
  
    /// <summary>  
    /// Serializes this object and its fields to a string  
    /// </summary>  
    /// <returns></returns>  
    public string Serialize () {  
        SaveToFields ();  
        return Field.DicToString (fields);  
    }  
  
    /// <summary>  
    /// Gets the protected fields variable after the SaveToFields function is called  
    /// </summary>  
    /// <returns>Dictionary of fields</returns>  
    public Dictionary<String, Field> GetFields () {  
        SaveToFields ();  
        return fields;  
    }  
}  
  
/// <summary>  
/// Representation of a field  
/// A field can contain the following value:  
/// Byte[], int, boolean, float, double, string, vector  
/// </summary>  
public class Field {  
    private Byte[] value;  
    public Dictionary<string, Field> children = new Dictionary<string, Field> ();  
  
    /// <summary>  
    /// Creates a field with value of a simple byte array  
    /// </summary>  
    /// <param name="value">Value of the field</param>  
    public Field (Byte[] value) {  
        this.value = value;  
    }  
  
    /// <summary>  
    /// Creates a field with children from a serializable object  
    /// </summary>  
    /// <param name="sObject">Object to store</param>  
    public Field (Serializable sObject) {  
        sObject.SaveToFields();
        children = sObject.GetFields ();  
    }  
  
    /// <summary>  
    /// Creates a field with children from a vector  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (Vector3 value) {  
        children["x"] = new Field (value.X);  
        children["y"] = new Field (value.Y);  
        children["z"] = new Field (value.Z);  
    }  
      
    /// <summary>  
    /// Creates a field with value converted from a float  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (float value) {  
        this.value = BitConverter.GetBytes (value);  
    }  
      
    /// <summary>  
    /// Creates a field with value converted from a double  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (double value) {  
        this.value = BitConverter.GetBytes (value);  
    }  
  
    /// <summary>  
    /// Creates a field with value converted from a integer  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (int value) {  
        this.value = BitConverter.GetBytes (value);  
    }  
  
    /// <summary>  
    /// Creates a field with value converted from a boolean  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (bool value) {  
        this.value = BitConverter.GetBytes (value);  
    }  
      
    /// <summary>  
    /// Creates a field with value converted from a string  
    /// </summary>  
    /// <param name="value"></param>  
    public Field (string value) {  
        this.value = System.Text.ASCIIEncoding.ASCII.GetBytes (value);  
    }  
  
    /// <summary>  
    /// Sets the children of a field  
    /// Used for storing for storage when type is unknown try to used serializable objects for anything else  
    /// </summary>  
    /// <param name="children">Children fields</param>  
    public Field (Dictionary<string, Field> children) {  
        this.children = children;  
    }  
  
    /**  
    Functions that convert the byte value to a usable object  
    Make sure that the correct get function is used!!  
        */  
  
    /// <summary>  
    /// Gets the byte value as a vector  
    /// </summary>  
    /// <returns></returns>  
    public Vector3 GetVector3 () {  
        Vector3 vector = new Vector3 ();  
        vector.X = children["x"].GetFloat ();  
        vector.Y = children["y"].GetFloat ();  
        vector.Z = children["z"].GetFloat ();  
        return vector;  
    }  
    /// <summary>  
    /// Gets the byte value as a float  
    /// </summary>  
    /// <returns></returns>  
    public float GetFloat () {  
        return BitConverter.ToSingle (value, 0);  
    }  
  
    /// <summary>  
    /// Gets the byte value as a double  
    /// </summary>  
    /// <returns></returns>  
    public double GetDouble () {  
        return BitConverter.ToDouble (value, 0);  
    }  
  
    /// <summary>  
    /// Gets the byte value as an integer  
    /// </summary>  
    /// <returns></returns>  
    public int GetInt () {  
        return BitConverter.ToInt32 (value, 0);  
    }  
  
    /// <summary>  
    /// Gets the byte value as an integer  
    /// </summary>  
    /// <returns>Value as bool</returns>  
    public bool GetBool () {  
        return BitConverter.ToBoolean (value, 0);  
    }  
  
    /// <summary>  
    /// Gets the byte value as a string  
    /// </summary>  
    /// <returns>Value as string</returns>  
    public string GetString () {  
        return System.Text.ASCIIEncoding.ASCII.GetString (value);  
    }  
  
    /// <summary>  
    /// Gets the byte value as an object of type T  
    /// </summary>  
    /// <returns>Object of type T</returns>  
    public T GetObject<T> () where T : Serializable, new () {  
        T obj = new T();  
        obj.LoadFields(children);  
        return obj;  
    }  
  
    /// <summary>  
    /// Gets the value  
    /// </summary>  
    /// <returns>Byte array</returns>  
    public Byte[] GetBytes () {  
        return value;  
    }  
  
    /// <summary>  
    /// Coverts a dictionary of fields to a string  
    /// </summary>  
    /// <param name="fields"></param>  
    /// <returns></returns>  
    public static string DicToString (Dictionary<string, Field> fields) {  
        string result = "";  
        foreach (KeyValuePair<string, Field> child in fields) {  
            result += child.Key + ":" + child.Value.ToString ();  
        };  
        return result;  
    }  
  
    /// <summary>  
    /// Converts a Field to a string representation  
    /// </summary>  
    /// <returns></returns>  
    public override string ToString () {  
        string result = "{";  
        if (value != null){  
            result += BytesToString (value) ;  
        } else{  
            result += DicToString (children);  
        }  
        result +="}";  
        return result;  
    }  
}  
  
/// <summary>  
/// Finds the matching closing bracket  
/// </summary>  
/// <param name="s"></param>  
/// <returns></returns>  
public static int closingBracket (string s){  
    int nextOpening = s.IndexOf ('{');  
    int nextClosing = s.IndexOf ('}');  
    while (nextOpening != -1 && nextOpening < nextClosing){  
        nextOpening = s.IndexOf ('{', nextOpening + 1);  
        nextClosing =  s.IndexOf ('}', nextClosing + 1);  
    }  
    return nextClosing;  
}  
  
/// <summary>  
/// Converts a byte array to a string  
/// </summary>  
/// <param name="ba"> The byte array to convert</param>  
/// <returns>String represantation of byte array</returns>  
public static string BytesToString (byte[] ba) {  
    string hex = BitConverter.ToString (ba);  
    return hex.Replace ("-","");  
}  
  
/// <summary>  
/// Converts a string to a byte array  
/// </summary>  
/// <param name="byteString"> The string to convert</param>  
/// <returns>Byte array from string</returns>  
private static Byte[] StringToBytes (string byteString) {  
    int NumberChars = byteString.Length;  
    byte[] bytes = new byte[NumberChars / 2];  
    for (int i = 0; i < NumberChars; i += 2){  
        bytes[i / 2] = Convert.ToByte (byteString.Substring (i, 2), 16);  
    }  
    return bytes;  
}  
  
/// <summary>  
/// Parses a string to a field  
/// </summary>  
/// <param name="field">The string to parse</param>  
/// <returns>The resulting field from the string</returns>  
public static Field parseField (string field){   
    string value = "";   
    if (field.IndexOf (':') != -1){  
        return new Field (StringToFields (field));   
    }  
    for (int i = 0; i < field.Length; i++){  
        value += field[i];   
    }   
    return new Field (StringToBytes (value));   
}  
  
/// <summary>  
/// Parses multiple fields to a dictionary of fields  
/// </summary>  
/// <param name="fields">The string of fields</param>  
/// <returns>The resulting dictionary</returns>  
private static Dictionary<String,Field> StringToFields (string fields){  
    Dictionary<String,Field> result = new Dictionary<String,Field> ();  
    string fieldName = "";  
    for (int i = 0; i < fields.Length; i++){  
        if (fields[i] == '}'){  
            return result;  
        } else if (fields[i] == ':' && fields[i+1] == '{'){  
            string subField = fields.Substring (i+2);  
            int subEnd = closingBracket (subField);  
            subField = subField.Substring (0,subEnd);  
            result.Add (fieldName,parseField (subField));  
            i += 2 + subEnd;  
            fieldName = "";  
        } else{  
            fieldName+= fields[i];  
        }  
    }  
    return result;  
}    
/// <summary>  
/// Deserializes a class T from a string  
/// </summary>  
/// <param name="obj">The string to deserialize</param>  
/// <returns>Object of type T with deserialized fields</returns>  
public T DeSerialize<T> (string obj) where T : Serializable, new () {  
    T result = new T ();  
    Dictionary<String, Field> fieldsFromString = StringToFields (obj);  
    result.LoadFields (fieldsFromString);  
    return result;  
} 