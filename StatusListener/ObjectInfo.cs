using VRageMath;

public class ObjectInfo
{
    public string name;
    public string status;
    public Vector3D position;
    public void Update(JsonObject info)
    {
        if (info.ContainsKey("Status"))
        {
            status = ((JsonPrimitive)info["Status"]).GetValue<string>();
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
