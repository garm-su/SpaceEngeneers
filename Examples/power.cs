#if DEBUG 
using System;using System.Collections.Generic;using System.Text;using Sandbox.Game.EntityComponents;using Sandbox.ModAPI.Ingame;using SpaceEngineers.Game.ModAPI.Ingame;using VRage.Game;using VRage.Game.ModAPI.Ingame;using VRage.Game.ModAPI.Ingame.Utilities;using VRage.Game.ObjectBuilders.Definitions;using VRageMath;using VRage.Game.GUI.TextPanel;namespace PowerGraphs{class Program : MyGridProgram{ 
#endif 
 
// ========================== 
// -- Power Graphs v1.8.17 -- 
// By Remaarn 
// ========================== 
// Updates: 
// v1.8.17 - Code cleanup around reading data from blocks. 
// v1.8.16 - Fixed screen display indices being wrong when specified out of order. 
// v1.8.15 - Added resetData command to reset saved information. 
// For previous updates see the change notes on the steam workshop page. 
 
// ======================= 
//      -- Setup -- 
// ======================= 
// To tell the script to display graphs on a block you must either: 
// 
//  A: Include [PowerGraph] in the name of the block or 
// 
//  B: Have [PowerGraph] written on a line in the Custom Data of the LCD block. 
//     Note: when doing it this way any graph types you want to specify in 
//     the Custom Data must come after this line. 
// 
// If you want or need to use a name tag other than [PowerGraph] you can change it 
// in the program settings which are stored in the Custom Data of the program 
// block that is running the script. 
// 
// To show only specific graph types on a display, write the graph 
// types you want into the CustomData of the LCD, one per line. 
// 
// Available graph types, not case sensitive: 
//   usage            - Total power usage / required power 
//   usageScaled      - Same as usage but scaled to the total max required power 
//   battery          - Battery charge 
//   batteryIn        - Battery input 
//   batteryOut       - Battery output 
//   batteryInOut     - Battery input and output 
//   batteryInOutAuto - Same as batteryInOut but scaled by peak value 
//   solar            - Solar power output / used solar power 
//   reactor          - Reactor power output 
//   h2engine         - Hydrogen engine power output 
//   wind             - Wind turbine power output 
//   storedOxygen     - Stored Oxygen * 
//   storedHydrogen   - Stored Hydrogen * 
// 
// * Tracking of gas tanks must be enabled in the script settings (in Custom Data) 
// 
// If a block has multiple display surfaces on it (eg. cockpits) you can specify 
// which surface a graph type will show on by using the format: 
// 
//      graph_type=screen_index 
// 
// where graph_type is one of the types listed above and screen_index is a 
// number from 0 to the number of surfaces - 1. For example: 
// 
//      usage=0 
//      battery=1 
// 
// will display the usage graph on the first surface of the block and 
// the battery graph on the second surface. 
// 
// NOTE: When sharing the Custom Data of a block with other scripts you may 
// need to add [PowerGraph] on the line directly above the graph types. 
 
//======================== 
// - Sensor LCD Culling - 
//======================== 
// To save performance, LCDs can be set to only update when a player is in 
// range of a sensor. To enable this behavior, add the sensor name tag to 
// any sensor you want to use. Then add the names of any LCD panels that you 
// want the sensor to activate to the CustomData of the sensor, one name per line. 
 
//======================== 
//  - Additional Notes - 
//======================== 
// Running the program block with the argument 'togglestatus' will manually 
// cycle the graph on any displays that are not set to a specific display 
// type. This is usefull if auto cycling is set to false. 
// 
// The command 'resetData' will reset any stored data back to zero. 
 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
//      NOTICE  NOTICE 
//      NOTICE  NOTICE 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
// Configuration has been moved to the Custom Data of the program block. 
// Do not modify the settings below. 
// The script must be recompiled to apply configuration changes. 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
 
//======================== 
//   -- Graph Colors -- 
//======================== 
// Colors are defined as RGB. Each R G B value is an integer ranging 
// from 0 to 255. 0 Being the darkest and 255 being the brightest. 
 
Color Background_Color        = new Color(r:000, g:000, b:000); 
Color Text_Color              = new Color(r:255, g:255, b:255); 
Color Text_Dim_Color          = new Color(r:100, g:100, b:100); 
Color Error_Background_Color  = new Color(r:255, g:073, b:073); 
Color Graph_Axes_Color        = new Color(r:255, g:255, b:255); 
Color Max_Capacity_Color      = new Color(r:109, g:109, b:109); 
readonly Color Current_Usage_Color     = new Color(r:109, g:255, b:109); 
readonly Color Required_Power_Color    = new Color(r:255, g:073, b:073); 
readonly Color Battery_Charge_Color    = new Color(r:036, g:073, b:255); 
readonly Color Battery_Input_Color     = new Color(r:109, g:109, b:255); 
readonly Color Battery_Output_Color    = new Color(r:036, g:255, b:146); 
readonly Color Current_Solar_Color     = new Color(r:219, g:182, b:036); 
readonly Color Max_Solar_Color         = new Color(r:219, g:109, b:036); 
readonly Color Reactor_Output_Color    = new Color(r:073, g:146, b:073); 
readonly Color H2PowerGen_Output_Color = new Color(r:182, g:036, b:073); 
readonly Color Wind_Used_Color         = new Color(r:036, g:146, b:219); 
readonly Color Wind_Generating_Color   = new Color(r:073, g:036, b:219); 
readonly Color Stored_Oxygen_Color     = new Color(r:146, g:219, b:219); 
readonly Color Stored_Hydrogen_Color   = new Color(r:219, g:182, b:109); 
 
//========================= 
// -- Advanced Settings -- 
//========================= 
 
readonly string[] animFrames = new[] { 
    "Power Graphs |---", 
    "Power Graphs -|--", 
    "Power Graphs --|-", 
    "Power Graphs ---|", 
    "Power Graphs --|-", 
    "Power Graphs -|--" 
}; 
 
//===================================== 
//       -- END OF SETTINGS -- 
// -- Do NOT modify past this point! -- 
//===================================== 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
//======================================= 
//           SCROLL BACK UP 
//======================================= 
 
string Lcd_Name_Tag = "[PowerGraph]"; 
string Sensor_Name_Tag = "[LCDTrigger]"; 
double Data_Update_Interval_In_Seconds = 60; 
double LCD_Refresh_Interval_In_Seconds = 10; 
bool Auto_Cycle_Graphs = true; 
double Auto_Cycle_Interval_In_Seconds = 10; 
bool Zero_Fill_Missing_Values = true; 
bool Display_On_LCDs_Of_Attached_Grids = false; 
bool Display_On_LCDs_Of_Grids_Attached_Via_Connectors = false; 
bool Include_Data_From_Grids_Attached_Via_Connectors = false; 
bool Include_Data_From_Attached_Small_Grids = false; 
bool Include_Data_From_Attached_Large_Grids = false; 
bool Measure_Stored_Gas = false; 
bool Enable_Long_History = false; 
bool Show_Script_Status_On_PB_Display = true; 
int Script_Status_Display_Surface_Index = 0; 
float Color_Brightness = 0.5f; 
float Margin = 0; 
 
const int shortHistoryValueCount = 100; 
const int longHistoryValueCount = 200; 
 
readonly DateTime[] sampleTimes; 
readonly bool[] validSamples; 
 
readonly float[] powerUsageValues; 
readonly float[] powerReqValues; 
float maxPowerReqValue; 
 
readonly float[] batteryUsageInValues; 
 
readonly float[] batteryChargeValues; 
float batteryMaxChargeValue; 
readonly float[] batteryInputValues; 
float batteryMaxInputValue; 
readonly float[] batteryOutputValues; 
float batteryMaxOutputValue; 
readonly float[] batteryInOutValues; 
 
readonly float[] solarMaxOutputValues; 
readonly float[] solarOutputValues; 
readonly float[] reactorOutputValues; 
float reactorOutputMaxValue; 
readonly float[] h2PowerGenOutputValues; 
float h2GenMaxOutputValue; 
readonly float[] turbineMaxOutputValues; 
readonly float[] turbineOutputValues; 
 
readonly float[] storedOxygenValues; 
float maxStoredOxygenValue; 
readonly float[] storedHydrogenValues; 
float maxStoredHydrogenValue; 
 
[Flags] 
enum DisplayTypes 
{ 
    Auto             = 0, 
    PowerUsage       = 1, 
    PowerUsageScaled = 2, 
    BatteryCharge    = 4, 
    BatteryIn        = 8, 
    BatteryOut       = 16, 
    BatteryInOut     = 32, 
    BatteryInOutAuto = 64, 
    SolarOutput      = 128, 
    ReactorOutput    = 256, 
    H2PowerGenOutput = 512, 
    TurbineOutput    = 1024, 
    StoredOxygen     = 2048, 
    StoredHydrogen   = 4096, 
 
    First = 1, 
    NumTypes = 13, 
    Invalid = 8192, 
} 
 
DisplayTypes displayType = DisplayTypes.First; 
 
public enum GraphType { Single, Double, Triple } 
 
struct GraphSettings 
{ 
    public GraphType Type; 
    public string Name; 
    public float ValueScale; 
    public string Unit; 
    public string Label1; 
    public Color Color1; 
    public string Label2; 
    public Color Color2; 
    public string Label3; 
    public Color Color3; 
    public bool Symmetric; 
 
    public GraphSettings(GraphType type, string name, float valueScale, string unit, string label1, Color color1, 
        string label2 = null, Color color2 = new Color(), string label3 = null, Color color3 = new Color()) 
    { 
        Type = type; 
        Name = name; 
        ValueScale = valueScale; 
        Label1 = label1; 
        Color1 = color1; 
        Label2 = label2; 
        Color2 = color2; 
        Label3 = label3; 
        Color3 = color3; 
        Unit = unit; 
        Symmetric = false; 
    } 
} 
 
GraphSettings[] graphSettings; 
 
const UpdateType updateTypes = UpdateType.Terminal | UpdateType.Trigger | UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100; 
 
readonly List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); 
 
int usageBlockCount; 
 
readonly List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>(); 
readonly List<IMySolarPanel> solarPanels = new List<IMySolarPanel>(); 
readonly List<IMyReactor> reactors = new List<IMyReactor>(); 
readonly List<IMyPowerProducer> h2PowerGens = new List<IMyPowerProducer>(); 
readonly List<IMyPowerProducer> turbines = new List<IMyPowerProducer>(); 
readonly List<IMyGasTank> gasTanks = new List<IMyGasTank>(); 
 
int oxygenTankCount; 
int hydrogenTankCount; 
 
readonly List<IMyTerminalBlock> panels = new List<IMyTerminalBlock>(); 
readonly List<IMySensorBlock> sensors = new List<IMySensorBlock>(); 
readonly List<MyDetectedEntityInfo> detectedEntities = new List<MyDetectedEntityInfo>(); 
readonly HashSet<IMyTextSurfaceProvider> handledPanels = new HashSet<IMyTextSurfaceProvider>(); 
readonly StringBuilder stringBuilder = new StringBuilder(); 
readonly StringBuilder measureStringBuilder = new StringBuilder(); 
readonly StringBuilder statusStringBuilder = new StringBuilder(); 
 
readonly List<DisplayConfig> parsedDisplayConfigs = new List<DisplayConfig>(); 
 
readonly List<string> errors = new List<string>(); 
int prevErrorCount; 
 
bool isFirstRun; 
DateTime lastDataUpdate; 
DateTime lastRefresh; 
DateTime lastDisplayCycle; 
DateTime lastStatusUpdate; 
 
struct LCDStatus 
{ 
    public IMyTextSurfaceProvider LCD; 
    public DateTime LastUpdate; 
    public DateTime LastCycle; 
    public bool DisplayEnabled; 
    public bool IsCleared; 
    public int SurfaceToUpdate; 
    public int TrackedSurfacesMask; 
 
    public DisplayConfig[] DisplayConfigs; 
} 
 
struct DisplayConfig 
{ 
    public int SurfaceIndex; 
    public DisplayTypes DisplayTypes; 
    public DisplayTypes CurrentDisplayType; 
 
    public DisplayConfig(int surfaceIndex, DisplayTypes displayTypes, DisplayTypes currentDisplayType) 
    { 
        SurfaceIndex = surfaceIndex; 
        DisplayTypes = displayTypes; 
        CurrentDisplayType = currentDisplayType; 
    } 
} 
 
readonly List<LCDStatus> lcdStatuses = new List<LCDStatus>(); 
 
struct CachedImage 
{ 
    public DateTime LastUpdate; 
    public List<MySprite> Sprites; 
} 
 
struct CachedImageKey : IEquatable<CachedImageKey> 
{ 
    public DisplayTypes DisplayType; 
    public Vector2 TextureSize; 
    public Vector2 SurfaceSize; 
 
    public bool Equals(CachedImageKey other) 
    { 
        return DisplayType == other.DisplayType 
            && TextureSize == other.TextureSize 
            && SurfaceSize == other.SurfaceSize; 
    } 
} 
 
readonly Dictionary<CachedImageKey, CachedImage> cachedImages = new Dictionary<CachedImageKey, CachedImage>(); 
 
bool imageDrawnThisFrame; 
 
int runningFrame; 
 
double avgRuntime; 
double maxRuntime; 
 
double[] runtimes = new double[10]; 
 
#region Closure Items 
 
readonly IMyCubeGrid BaseGrid; 
readonly Func<IMyTerminalBlock, bool> BlockCollectorFunc; 
readonly Func<IMyTerminalBlock, bool> LCDCollectorFunc; 
readonly Func<IMySensorBlock, bool> SensorCollectorFunc; 
 
bool IsValidBlock(IMyTerminalBlock block) 
{ 
    var blockGrid = block.CubeGrid; 
 
    if (blockGrid == BaseGrid) 
        return true; 
 
    if (!Include_Data_From_Grids_Attached_Via_Connectors && !blockGrid.IsSameConstructAs(BaseGrid)) 
        return false; 
 
    return Include_Data_From_Attached_Small_Grids && blockGrid.GridSizeEnum == MyCubeSize.Small 
        || Include_Data_From_Attached_Large_Grids && blockGrid.GridSizeEnum == MyCubeSize.Large; 
} 
 
bool IsValidLCD(IMyTerminalBlock block) 
{ 
    if (!(block is IMyTextSurfaceProvider)) 
        return false; 
 
    var lcdGrid = block.CubeGrid; 
 
    if (!block.IsWorking) 
        return false; 
 
    if (Display_On_LCDs_Of_Attached_Grids) 
    { 
        if (!Display_On_LCDs_Of_Grids_Attached_Via_Connectors && !lcdGrid.IsSameConstructAs(BaseGrid)) 
            return false; 
    } 
    else if (lcdGrid != BaseGrid) 
    { 
        return false; 
    } 
 
    if (block.CustomName.Contains(Lcd_Name_Tag)) 
        return true; 
 
    int index = block.CustomData.IndexOf(Lcd_Name_Tag); 
 
    if (index == -1) 
        return false; 
 
    if (index != 0 && block.CustomData[index - 1] != '\n') 
        return false; 
 
    return true; 
} 
 
bool IsValidSensorBlock(IMySensorBlock sensor) 
{ 
    var sensorGrid = sensor.CubeGrid; 
 
    if (!sensor.IsWorking) 
        return false; 
 
    if (string.IsNullOrWhiteSpace(sensor.CustomData)) 
        return false; 
 
    if ((Display_On_LCDs_Of_Attached_Grids & !Display_On_LCDs_Of_Grids_Attached_Via_Connectors) && !sensorGrid.IsSameConstructAs(BaseGrid)) 
        return false; 
 
    return (Display_On_LCDs_Of_Attached_Grids || sensorGrid == BaseGrid) && sensor.CustomName.Contains(Sensor_Name_Tag); 
} 
 
#endregion 
 
public Program() 
{ 
    Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100; 
 
    BaseGrid = Me.CubeGrid; 
    BlockCollectorFunc = IsValidBlock; 
    LCDCollectorFunc = IsValidLCD; 
    SensorCollectorFunc = IsValidSensorBlock; 
 
    LoadConfig(); 
    SaveConfig(); 
 
    int historyValueCount = Enable_Long_History ? longHistoryValueCount : shortHistoryValueCount; 
 
    sampleTimes = new DateTime[historyValueCount]; 
    validSamples = new bool[historyValueCount]; 
    powerUsageValues = new float[historyValueCount]; 
    powerReqValues = new float[historyValueCount]; 
    batteryUsageInValues = new float[historyValueCount]; 
    batteryChargeValues = new float[historyValueCount]; 
    batteryInputValues = new float[historyValueCount]; 
    batteryOutputValues = new float[historyValueCount]; 
    batteryInOutValues = new float[historyValueCount]; 
    solarMaxOutputValues = new float[historyValueCount]; 
    solarOutputValues = new float[historyValueCount]; 
    reactorOutputValues = new float[historyValueCount]; 
    h2PowerGenOutputValues = new float[historyValueCount]; 
    turbineMaxOutputValues = new float[historyValueCount]; 
    turbineOutputValues = new float[historyValueCount]; 
    storedOxygenValues = new float[historyValueCount]; 
    storedHydrogenValues = new float[historyValueCount]; 
 
    var now = DateTime.Now; 
 
    if (Storage != "") 
    { 
        LoadGraphData(Storage); 
    } 
    else 
    { 
        for (int i = 0; i < sampleTimes.Length; i++) 
            sampleTimes[i] = now; 
    } 
 
    //Storage = ""; 
 
    graphSettings = new[] { 
        new GraphSettings(GraphType.Triple, "Power Usage", 1E6f, "W", "Battery Recharge: ", Battery_Input_Color, "Required Total: ", Required_Power_Color, "Current Used: ", Current_Usage_Color), 
        new GraphSettings(GraphType.Triple, "Power Usage S", 1E6f, "W", "Battery Recharge: ", Battery_Input_Color, "Required Total: ", Required_Power_Color, "Current Used: ", Current_Usage_Color), 
        new GraphSettings(GraphType.Single, "Batteries", 1E6f, "Wh", "Charge: ", Battery_Charge_Color), 
        new GraphSettings(GraphType.Single, "Batteries", 1E6f, "W", "Input: ", Battery_Input_Color), 
        new GraphSettings(GraphType.Single, "Batteries", 1E6f, "W", "Output: ", Battery_Output_Color), 
        new GraphSettings(GraphType.Single, "Batteries", 1E6f, "W", "In ^ / Out v: ", Battery_Input_Color) { Symmetric = true }, 
        new GraphSettings(GraphType.Single, "Batteries", 1E6f, "W", "In ^ / Out v: ", Battery_Input_Color) { Symmetric = true }, 
        new GraphSettings(GraphType.Double, "Solar Panels", 1E6f, "W", "Generating: ", Max_Solar_Color, "Used: ", Current_Solar_Color), 
        new GraphSettings(GraphType.Single, "Reactors", 1E6f, "W", "Output: ", Reactor_Output_Color), 
        new GraphSettings(GraphType.Single, "H2 Engines", 1E6f, "W", "Output: ", H2PowerGen_Output_Color), 
        new GraphSettings(GraphType.Double, "Wind Turbines", 1E6f, "W", "Generating: ", Wind_Generating_Color, "Used: ", Wind_Used_Color), 
        new GraphSettings(GraphType.Single, "Oxygen Tanks", 1, "L", "Stored gas: ", Stored_Oxygen_Color), 
        new GraphSettings(GraphType.Single, "Hydrogen Tanks", 1, "L", "Stored Gas: ", Stored_Hydrogen_Color) 
    }; 
 
    RemapColors(); 
 
    lastDataUpdate = now - TimeSpan.FromSeconds(Data_Update_Interval_In_Seconds); 
    isFirstRun = true; 
} 
 
void RemapColors() 
{ 
    Text_Color             = new Color(Text_Color.ToVector3()             * Color_Brightness); 
    Text_Dim_Color         = new Color(Text_Dim_Color.ToVector3()         * Color_Brightness); 
    Error_Background_Color = new Color(Error_Background_Color.ToVector3() * Color_Brightness); 
    Graph_Axes_Color       = new Color(Graph_Axes_Color.ToVector3()       * Color_Brightness); 
    Max_Capacity_Color     = new Color(Max_Capacity_Color.ToVector3()     * Color_Brightness); 
 
    for (int i = 0; i < graphSettings.Length; i++) 
    { 
        var gs = graphSettings[i]; 
        gs.Color1 = new Color(gs.Color1.ToVector3() * Color_Brightness); 
        gs.Color2 = new Color(gs.Color2.ToVector3() * Color_Brightness); 
        gs.Color3 = new Color(gs.Color3.ToVector3() * Color_Brightness); 
        graphSettings[i] = gs; 
    } 
} 
 
void LoadGraphData(string storage) 
{ 
    var deserializer = Deserializer.LoadFromData(storage, Echo); 
 
    if (!deserializer.IsValid) 
    { 
        Echo("ERROR: Invalid saved data"); 
        return; 
    } 
 
    if (!deserializer.LoadFloatArray("powerUsageValues", powerUsageValues)) 
        Echo("Warning: Failed to load powerUsageValues array"); 
 
    if (!deserializer.LoadFloatArray("powerReqValues", powerReqValues)) 
        Echo("Warning: Failed to load powerReqValues array"); 
 
    deserializer.LoadFloatArray("batteryUsageInValues", batteryUsageInValues); 
 
    if (!deserializer.LoadFloatArray("batteryChargeValues", batteryChargeValues)) 
        Echo("Warning: Failed to load batteryChargeValues array"); 
 
    deserializer.LoadFloatArray("batteryInputValues", batteryInputValues); 
    deserializer.LoadFloatArray("batteryOutputValues", batteryOutputValues); 
 
    for (int i = 0; i < batteryInOutValues.Length; i++) 
        batteryInOutValues[i] = batteryInputValues[i] - batteryOutputValues[i]; 
 
    deserializer.LoadFloatArray("solarMaxOutputValues", solarMaxOutputValues); 
 
    if (!deserializer.LoadFloatArray("solarOutputValues", solarOutputValues)) 
        Echo("Warning: Failed to load solarOutputValues array"); 
 
    deserializer.LoadFloatArray("reactorOutputValues", reactorOutputValues); 
    deserializer.LoadFloatArray("h2PowerGenOutputValues", h2PowerGenOutputValues); 
    deserializer.LoadFloatArray("turbineMaxOutputValues", turbineMaxOutputValues); 
    deserializer.LoadFloatArray("turbineOutputValues", turbineOutputValues); 
 
    if (Measure_Stored_Gas) 
    { 
        deserializer.LoadFloatArray("storedOxygenValues", storedOxygenValues); 
        deserializer.LoadFloatArray("storedHydrogenValues", storedHydrogenValues); 
    } 
 
    var storedSampleTimes = deserializer.LoadLongArray("sampleTimes"); 
 
    if (storedSampleTimes != null) 
    { 
        if (sampleTimes.Length > storedSampleTimes.Length) 
        { 
            int offset = sampleTimes.Length - storedSampleTimes.Length; 
            var now = DateTime.Now; 
 
            for (int i = 0; i < offset; i++) 
                sampleTimes[i] = now; 
 
            for (int i = 0; i < storedSampleTimes.Length; i++) 
                sampleTimes[offset + i] = DateTime.FromBinary(storedSampleTimes[i]); 
        } 
        else if (storedSampleTimes.Length > sampleTimes.Length) 
        { 
            int offset = storedSampleTimes.Length - sampleTimes.Length; 
 
            for (int i = 0; i < sampleTimes.Length; i++) 
                sampleTimes[i] = DateTime.FromBinary(storedSampleTimes[offset + i]); 
        } 
        else 
        { 
            for (int i = 0; i < storedSampleTimes.Length; i++) 
                sampleTimes[i] = DateTime.FromBinary(storedSampleTimes[i]); 
        } 
    } 
    else 
    { 
        Echo("Warning: Failed to load sampleTimes array"); 
    } 
 
    var storedValidSamples = deserializer.LoadIntArray("validSamples"); 
    bool resetValidSamples = false; 
 
    if (storedValidSamples != null) 
    { 
        var validSampleCountArr = deserializer.LoadIntArray("validSampleCount"); 
 
        if (validSampleCountArr != null) 
        { 
            int storedValidSampleCount = validSampleCountArr[0]; 
 
            if (storedValidSampleCount < validSamples.Length) 
            { 
                int offset = validSamples.Length - storedValidSampleCount; 
 
                for (int i = 0; i < storedValidSampleCount; i++) 
                    validSamples[i + offset] = ((storedValidSamples[i / 32] >> (i % 32)) & 1) == 1; 
            } 
            else 
            { 
                int offset = storedValidSampleCount - validSamples.Length; 
 
                for (int i = 0; i < validSamples.Length; i++) 
                    validSamples[i] = ((storedValidSamples[(i + offset) / 32] >> ((i + offset) % 32)) & 1) == 1; 
            } 
        } 
        else 
        { 
            Echo("Warning: Failed to load validSampleCount"); 
            resetValidSamples = true; 
        } 
    } 
    else 
    { 
        Echo("Warning: Failed to load validSamples array"); 
        resetValidSamples = true; 
    } 
 
    if (resetValidSamples) 
    { 
        for (int i = 0; i < validSamples.Length; i++) 
            validSamples[i] = true; 
    } 
} 
 
public void Save() 
{ 
    var serializer = Serializer.Create(); 
 
    serializer.AddArray("powerUsageValues", powerUsageValues); 
    serializer.AddArray("powerReqValues", powerReqValues); 
    serializer.AddArray("batteryUsageInValues", batteryUsageInValues); 
    serializer.AddArray("batteryChargeValues", batteryChargeValues); 
    serializer.AddArray("batteryInputValues", batteryInputValues); 
    serializer.AddArray("batteryOutputValues", batteryOutputValues); 
    serializer.AddArray("solarMaxOutputValues", solarMaxOutputValues); 
    serializer.AddArray("solarOutputValues", solarOutputValues); 
    serializer.AddArray("reactorOutputValues", reactorOutputValues); 
    serializer.AddArray("h2PowerGenOutputValues", h2PowerGenOutputValues); 
    serializer.AddArray("turbineMaxOutputValues", turbineMaxOutputValues); 
    serializer.AddArray("turbineOutputValues", turbineOutputValues); 
 
    if (Measure_Stored_Gas) 
    { 
        serializer.AddArray("storedOxygenValues", storedOxygenValues); 
        serializer.AddArray("storedHydrogenValues", storedHydrogenValues); 
    } 
 
    var binarySampleTimes = new long[sampleTimes.Length]; 
 
    for (int i = 0; i < binarySampleTimes.Length; i++) 
        binarySampleTimes[i] = sampleTimes[i].ToBinary(); 
 
    serializer.AddArray("sampleTimes", binarySampleTimes); 
 
    // TODO: Support single values 
    serializer.AddArray("validSampleCount", new int[] { validSamples.Length }); 
 
    var packedValidSamples = new int[(validSamples.Length + 31) / 32]; 
 
    for (int i = 0; i < validSamples.Length; i++) 
        packedValidSamples[i / 32] |= (validSamples[i] ? 1 : 0) << i % 32; 
 
    serializer.AddArray("validSamples", packedValidSamples); 
 
    Storage = serializer.Save(); 
} 
 
void LoadConfig() 
{ 
    const string section = "PowerGraphs"; 
 
    var ini = new MyIni(); 
    ini.TryParse(Me.CustomData); 
 
    InitValue(ini.Get(section, "Lcd_Name_Tag"), ref Lcd_Name_Tag); 
    InitValue(ini.Get(section, "Sensor_Name_Tag"), ref Sensor_Name_Tag); 
    InitValue(ini.Get(section, "Data_Update_Interval_In_Seconds"), ref Data_Update_Interval_In_Seconds); 
    InitValue(ini.Get(section, "LCD_Refresh_Interval_In_Seconds"), ref LCD_Refresh_Interval_In_Seconds); 
    InitValue(ini.Get(section, "Auto_Cycle_Graphs"), ref Auto_Cycle_Graphs); 
    InitValue(ini.Get(section, "Auto_Cycle_Interval_In_Seconds"), ref Auto_Cycle_Interval_In_Seconds); 
    InitValue(ini.Get(section, "Zero_Fill_Missing_Values"), ref Zero_Fill_Missing_Values); 
    InitValue(ini.Get(section, "Display_On_LCDs_Of_Attached_Grids"), ref Display_On_LCDs_Of_Attached_Grids); 
    InitValue(ini.Get(section, "Display_On_LCDs_Of_Grids_Attached_Via_Connectors"), ref Display_On_LCDs_Of_Grids_Attached_Via_Connectors); 
    InitValue(ini.Get(section, "Include_Data_From_Grids_Attached_Via_Connectors"), ref Include_Data_From_Grids_Attached_Via_Connectors); 
    InitValue(ini.Get(section, "Include_Data_From_Attached_Small_Grids"), ref Include_Data_From_Attached_Small_Grids); 
    InitValue(ini.Get(section, "Include_Data_From_Attached_Large_Grids"), ref Include_Data_From_Attached_Large_Grids); 
    InitValue(ini.Get(section, "Measure_Stored_Gas"), ref Measure_Stored_Gas); 
    InitValue(ini.Get(section, "Enable_Long_History"), ref Enable_Long_History); 
    InitValue(ini.Get(section, "Show_Script_Status_On_PB_Display"), ref Show_Script_Status_On_PB_Display); 
    InitValue(ini.Get(section, "Script_Status_Display_Surface_Index"), ref Script_Status_Display_Surface_Index); 
    InitValue(ini.Get(section, "Color_Brightness"), ref Color_Brightness); 
    InitValue(ini.Get(section, "Margin"), ref Margin); 
} 
 
static void InitValue(MyIniValue iniValue, ref string value) 
{ 
    string tempValue; 
    if (!iniValue.IsEmpty && iniValue.TryGetString(out tempValue)) 
        value = tempValue; 
} 
 
static void InitValue(MyIniValue iniValue, ref float value) 
{ 
    float tempValue; 
    if (!iniValue.IsEmpty && iniValue.TryGetSingle(out tempValue)) 
        value = tempValue; 
} 
 
static void InitValue(MyIniValue iniValue, ref double value) 
{ 
    double tempValue; 
    if (!iniValue.IsEmpty && iniValue.TryGetDouble(out tempValue)) 
        value = tempValue; 
} 
 
static void InitValue(MyIniValue iniValue, ref bool value) 
{ 
    bool tempValue; 
    if (!iniValue.IsEmpty && iniValue.TryGetBoolean(out tempValue)) 
        value = tempValue; 
} 
 
static void InitValue(MyIniValue iniValue, ref int value) 
{ 
    int tempValue; 
    if (!iniValue.IsEmpty && iniValue.TryGetInt32(out tempValue)) 
        value = tempValue; 
} 
 
void SaveConfig() 
{ 
    var stringBuilder = new StringBuilder(1024) 
    .Append(";  IMPORTANT! The script must be recompiled to\n;  apply any configuration changes.\n\n") 
    .Append("[PowerGraphs]\n") 
 
    .Append(";  The text LCD names must contain to be used\n") 
    .Append("Lcd_Name_Tag=").Append(Lcd_Name_Tag).Append("\n\n") 
 
    .Append(";  The text sensor names must contain if\n;  they are to be used as LCD triggers\n") 
    .Append("Sensor_Name_Tag=").Append(Sensor_Name_Tag).Append("\n\n") 
 
    .Append(";  How often current power stats are collected\n") 
    .Append("Data_Update_Interval_In_Seconds=").Append(Data_Update_Interval_In_Seconds).Append("\n\n") 
 
    .Append(";  How often LCDs are redrawn\n;  WARNING: Times below one second may impact game performance\n") 
    .Append("LCD_Refresh_Interval_In_Seconds=").Append(LCD_Refresh_Interval_In_Seconds).Append("\n\n") 
 
    .Append(";  Should LCDs without a specific display\n;  type cycle through all display types\n") 
    .Append("Auto_Cycle_Graphs=").Append(Auto_Cycle_Graphs).Append("\n\n") 
 
    .Append(";  How often LCDs cycle between assigned graph types\n") 
    .Append("Auto_Cycle_Interval_In_Seconds=").Append(Auto_Cycle_Interval_In_Seconds).Append("\n\n") 
 
    .Append(";  When enabled the script will insert empty sections after a power outage\n;  for the duration of the outage.\n;  NOTE: Pausing the game will have the same effect as a power outage.\n;  If you aren't likely to experience frequent power outages and you pause\n;  the game often you may want to set this to false.\n") 
    .Append("Zero_Fill_Missing_Values=").Append(Zero_Fill_Missing_Values).Append("\n\n") 
 
    .Append(";  Should the script update LCDs on attached grids (through rotors,\n;  connectors, pistons, etc.)\n") 
    .Append("Display_On_LCDs_Of_Attached_Grids=").Append(Display_On_LCDs_Of_Attached_Grids).Append("\n\n") 
 
    .Append(";  Should the script update LCDs on grids attached via connectors\n") 
    .Append("Display_On_LCDs_Of_Grids_Attached_Via_Connectors=").Append(Display_On_LCDs_Of_Grids_Attached_Via_Connectors).Append("\n\n") 
 
    .Append(";  Should power and gas be included from grids attached via connectors\n") 
    .Append("Include_Data_From_Grids_Attached_Via_Connectors=").Append(Include_Data_From_Grids_Attached_Via_Connectors).Append("\n\n") 
 
    .Append(";  Should power and gas of attached small grids be read\n") 
    .Append("Include_Data_From_Attached_Small_Grids=").Append(Include_Data_From_Attached_Small_Grids).Append("\n\n") 
 
    .Append(";  Should power and gas of attached large grids be read\n") 
    .Append("Include_Data_From_Attached_Large_Grids=").Append(Include_Data_From_Attached_Large_Grids).Append("\n\n") 
 
    .Append(";  Set to true to enable collecting of gas tank information\n") 
    .Append("Measure_Stored_Gas=").Append(Measure_Stored_Gas).Append("\n\n") 
 
    .Append(";  Set to true to record more data points (2x). These will only be\n;  shown on wide LCDs. This will use more memory and performance.\n") 
    .Append("Enable_Long_History=").Append(Enable_Long_History).Append("\n\n") 
 
    .Append(";  Set to false to disable the script status display output on the\n;  programmable blocks screen\n") 
    .Append("Show_Script_Status_On_PB_Display=").Append(Show_Script_Status_On_PB_Display).Append("\n\n") 
 
    //.Append(";  \n") 
    .Append("Script_Status_Display_Surface_Index=").Append(Script_Status_Display_Surface_Index).Append("\n\n") 
 
    //.Append(";  \n") 
    .Append("Color_Brightness=").Append(Color_Brightness).Append("\n\n") 
 
    .Append(";  Extra padding around the edge of displays. Try 15 as an example.\n") 
    .Append("Margin=").Append(Margin).Append("\n"); 
 
    Me.CustomData = stringBuilder.ToString(); 
} 
 
public void Main(string argument, UpdateType updateType) 
{ 
    if ((updateType & updateTypes) == 0) 
        return; 
 
    double lastRuntime = Runtime.LastRunTimeMs; 
 
    AddValueToArray(runtimes, lastRuntime); 
 
    avgRuntime = 0; 
 
    for (int i = 0; i < runtimes.Length; i++) 
        avgRuntime += runtimes[i]; 
 
    avgRuntime /= runtimes.Length; 
 
    if (lastRuntime > maxRuntime) 
        maxRuntime = lastRuntime; 
 
    statusStringBuilder.AppendLine(animFrames[runningFrame]); 
    statusStringBuilder.Append("Tracking ").Append(lcdStatuses.Count).AppendLine(" display blocks"); 
    //statusStringBuilder.Append("Last runtime ").Append(Math.Round(lastRuntime, 2)).AppendLine(" ms"); 
    //statusStringBuilder.Append("Avg runtime ").Append(Math.Round(avgRuntime, 2)).AppendLine(" ms"); 
    // TODO: Find out a good way to not count JIT time 
    //statusStringBuilder.Append("Max runtime ").Append(Math.Round(maxRuntime, 2)).AppendLine(" ms"); 
 
    bool collectBlocks = isFirstRun || (updateType & UpdateType.Update100) != 0; 
 
    if (collectBlocks) 
        GridTerminalSystem.GetBlocksOfType(blocks, BlockCollectorFunc); 
 
    var now = DateTime.Now; 
 
    double secondsSinceLastUpdate = (now - lastDataUpdate).TotalSeconds; 
 
    secondsSinceLastUpdate = Math.Min(secondsSinceLastUpdate, Data_Update_Interval_In_Seconds * sampleTimes.Length); 
 
    if (Zero_Fill_Missing_Values && !isFirstRun) 
    { 
        while (secondsSinceLastUpdate > Data_Update_Interval_In_Seconds * 2) 
        { 
            AddValueToArray(sampleTimes, now - TimeSpan.FromSeconds(secondsSinceLastUpdate)); 
            AddZeroValues(); 
            secondsSinceLastUpdate -= Data_Update_Interval_In_Seconds; 
        } 
    } 
 
    bool recordValues = !collectBlocks && secondsSinceLastUpdate >= Data_Update_Interval_In_Seconds; 
 
    if (recordValues) 
    { 
        RecordValues(blocks); 
        AddValueToArray(sampleTimes, now); 
        lastDataUpdate = now; 
    } 
 
    if (argument.Equals("resetData", StringComparison.OrdinalIgnoreCase)) 
    { 
        ResetData(); 
    } 
    else if (argument == "togglestatus") 
    { 
        CycleDisplayType(now); 
    } 
    else if (Auto_Cycle_Graphs && (now - lastDisplayCycle).TotalSeconds >= Auto_Cycle_Interval_In_Seconds) 
    { 
        CycleDisplayType(now); 
    } 
 
    prevErrorCount = errors.Count; 
 
    if (!collectBlocks && !recordValues) 
    { 
        bool needsUpdate = UpdateSensors(); 
 
        double secondsSinceLastRefresh = (now - lastRefresh).TotalSeconds; 
 
        if (needsUpdate || secondsSinceLastRefresh >= LCD_Refresh_Interval_In_Seconds) 
        { 
            errors.Clear(); 
            UpdateLCDs(now); 
 
            if (!imageDrawnThisFrame) 
                lastRefresh = now; 
        } 
    } 
 
    //statusStringBuilder.Append("Last run time: ").Append(Math.Round(Runtime.LastRunTimeMs, 2)).AppendLine("ms"); 
 
    if (errors.Count > 0) 
        statusStringBuilder.AppendLine("\nErrors:"); 
 
    foreach (var item in errors) 
        statusStringBuilder.AppendLine(item); 
 
    if ((now - lastStatusUpdate).TotalSeconds >= 1) 
    { 
        if (Show_Script_Status_On_PB_Display) 
        { 
            IMyTextSurface selfSurface; 
 
            if (Me.SurfaceCount > Script_Status_Display_Surface_Index 
                && (selfSurface = Me.GetSurface(Script_Status_Display_Surface_Index)) != null) 
            { 
                selfSurface.ContentType = ContentType.TEXT_AND_IMAGE; 
                selfSurface.Script = ""; 
                selfSurface.WriteText(statusStringBuilder); 
            } 
        } 
 
        Echo(statusStringBuilder.ToString()); 
        runningFrame = (runningFrame + 1) % animFrames.Length; 
        lastStatusUpdate = now; 
    } 
 
    statusStringBuilder.Clear(); 
    isFirstRun = false; 
} 
 
void ResetData() 
{ 
    for (int i = 0; i < sampleTimes.Length; i++) 
        sampleTimes[i] = DateTime.Now; 
 
    Array.Clear(validSamples, 0, validSamples.Length); 
    Array.Clear(powerUsageValues, 0, powerUsageValues.Length); 
    Array.Clear(powerReqValues, 0, powerReqValues.Length); 
    Array.Clear(batteryUsageInValues, 0, batteryUsageInValues.Length); 
    Array.Clear(batteryChargeValues, 0, batteryChargeValues.Length); 
    Array.Clear(batteryInputValues, 0, batteryInputValues.Length); 
    Array.Clear(batteryOutputValues, 0, batteryOutputValues.Length); 
    Array.Clear(batteryInOutValues, 0, batteryInOutValues.Length); 
    Array.Clear(solarMaxOutputValues, 0, solarMaxOutputValues.Length); 
    Array.Clear(solarOutputValues, 0, solarOutputValues.Length); 
    Array.Clear(reactorOutputValues, 0, reactorOutputValues.Length); 
    Array.Clear(h2PowerGenOutputValues, 0, h2PowerGenOutputValues.Length); 
    Array.Clear(turbineMaxOutputValues, 0, turbineMaxOutputValues.Length); 
    Array.Clear(turbineOutputValues, 0, turbineOutputValues.Length); 
    Array.Clear(storedOxygenValues, 0, storedOxygenValues.Length); 
    Array.Clear(storedHydrogenValues, 0, storedHydrogenValues.Length); 
} 
 
bool UpdateSensors() 
{ 
    GridTerminalSystem.GetBlocksOfType(sensors, SensorCollectorFunc); 
 
    if (sensors.Count <= 0) 
        return false; 
 
    string sensorStatusText = $"Tracking {sensors.Count.ToString()} sensors"; 
 
    statusStringBuilder.AppendLine(sensorStatusText); 
 
    bool needsUpdate = false; 
 
    foreach (var sensor in sensors) 
    { 
        sensor.DetectedEntities(detectedEntities); 
        bool sensorActive = detectedEntities.Count > 0; 
 
        // TODO: Custom struct enumerator 
        var lcdNames = sensor.CustomData.SplitOnChar('\n'); 
 
        foreach (var item in lcdNames) 
        { 
            int statusIndex = FindLCDStatusIndex(lcdStatuses, item); 
 
            if (statusIndex == -1) 
                continue; 
 
            var status = lcdStatuses[statusIndex]; 
 
            if (!status.DisplayEnabled && sensorActive) 
                needsUpdate = true; 
 
            status.DisplayEnabled = sensorActive; 
            lcdStatuses[statusIndex] = status; 
 
            handledPanels.Add(status.LCD); 
        } 
    } 
 
    for (int i = 0; i < lcdStatuses.Count; i++) 
    { 
        var status = lcdStatuses[i]; 
 
        if (!status.DisplayEnabled && !handledPanels.Contains(status.LCD)) 
        { 
            status.DisplayEnabled = true; 
            lcdStatuses[i] = status; 
        } 
    } 
 
    handledPanels.Clear(); 
 
    return needsUpdate; 
} 
 
int FindLCDStatusIndex(List<LCDStatus> statuses, StringSegment name) 
{ 
    for (int i = 0; i < statuses.Count; i++) 
    { 
        if (name.Equals(((IMyTerminalBlock)statuses[i].LCD).CustomName)) 
            return i; 
    } 
 
    return -1; 
} 
 
int FindLCDStatusIndex(List<LCDStatus> statuses, IMyTextSurfaceProvider surfaceProvider) 
{ 
    for (int i = 0; i < statuses.Count; i++) 
    { 
        if (statuses[i].LCD == surfaceProvider) 
            return i; 
    } 
 
    return -1; 
} 
 
void UpdateLCDs(DateTime now) 
{ 
    imageDrawnThisFrame = false; 
 
    GridTerminalSystem.GetBlocksOfType(panels, LCDCollectorFunc); 
 
    foreach (var item in panels) 
        UpdatePanel(item, now); 
 
    for (int i = 0; i < lcdStatuses.Count; i++) 
    { 
        if (!panels.Contains((IMyTerminalBlock)lcdStatuses[i].LCD)) 
            lcdStatuses.RemoveAt(i--); 
    } 
} 
 
void UpdatePanel(IMyTerminalBlock block, DateTime now) 
{ 
    var surfaceProvider = (IMyTextSurfaceProvider)block; 
    var panel = surfaceProvider as IMyTextPanel; // Workaround for game issue 
 
    int statusIndex = FindLCDStatusIndex(lcdStatuses, surfaceProvider); 
 
    parsedDisplayConfigs.Clear(); 
 
    int surfacesMask; 
    int minSurfaceIndex; 
    int maxSurfaceIndex; 
 
    ParseSurfaceDisplayConfigs(block.CustomData, surfaceProvider.SurfaceCount, 
        parsedDisplayConfigs, out surfacesMask, out minSurfaceIndex, out maxSurfaceIndex); 
 
    int trackedSurfaceCount = parsedDisplayConfigs.Count; 
 
    LCDStatus status; 
    bool resetDisplayTypes = false; 
 
    if (statusIndex == -1) 
    { 
        statusIndex = lcdStatuses.Count; 
 
        lcdStatuses.Add(status = new LCDStatus { 
            DisplayEnabled = true, 
            IsCleared = true, 
            LCD = surfaceProvider, 
            LastUpdate = now - TimeSpan.FromSeconds(LCD_Refresh_Interval_In_Seconds), 
            SurfaceToUpdate = minSurfaceIndex, 
            TrackedSurfacesMask = surfacesMask, 
            DisplayConfigs = new DisplayConfig[trackedSurfaceCount] 
        }); 
 
        resetDisplayTypes = true; 
    } 
    else 
    { 
        status = lcdStatuses[statusIndex]; 
 
        if (status.TrackedSurfacesMask != surfacesMask) 
        { 
            for (int i = 0; i < surfaceProvider.SurfaceCount; i++) 
            { 
                if ((surfacesMask & (1 << i)) != 0 || (status.TrackedSurfacesMask & (1 << i)) == 0) 
                    continue; 
 
                var surface = surfaceProvider.GetSurface(i); 
 
                surface.ContentType = ContentType.TEXT_AND_IMAGE; 
                surface.Font = "Debug"; 
                surface.FontSize = 1f; 
                surface.BackgroundColor = Color.Black; 
                surface.FontColor = Color.White; 
 
                if (panel != null) 
                    panel.WriteText(""); 
                else 
                    surface.WriteText(""); 
            } 
        } 
 
        status.TrackedSurfacesMask = surfacesMask; 
 
        if (status.DisplayConfigs.Length != trackedSurfaceCount) 
        { 
            status.DisplayConfigs = new DisplayConfig[trackedSurfaceCount]; 
            resetDisplayTypes = true; 
        } 
    } 
 
    if (resetDisplayTypes) 
    { 
        for (int i = 0; i < status.DisplayConfigs.Length; i++) 
            status.DisplayConfigs[i].CurrentDisplayType = DisplayTypes.Invalid; 
    } 
 
    for (int i = 0; i < status.DisplayConfigs.Length; i++) 
    { 
        var config = status.DisplayConfigs[i]; 
        var parsedConfig = parsedDisplayConfigs[i]; 
 
        if (parsedConfig.SurfaceIndex != config.SurfaceIndex || parsedConfig.DisplayTypes != config.DisplayTypes) 
            config.CurrentDisplayType = DisplayTypes.Invalid; 
 
        config.SurfaceIndex = parsedConfig.SurfaceIndex; 
        config.DisplayTypes = parsedConfig.DisplayTypes; 
 
        status.DisplayConfigs[i] = config; 
    } 
 
    if (imageDrawnThisFrame && (now - status.LastUpdate).TotalSeconds < LCD_Refresh_Interval_In_Seconds) 
    { 
        lcdStatuses[statusIndex] = status; 
        return; 
    } 
 
    if (status.DisplayEnabled) 
    { 
        UpdatePanel(now, surfaceProvider, panel, surfacesMask, minSurfaceIndex, maxSurfaceIndex, ref status); 
    } 
    else if (!status.IsCleared) 
    { 
        for (int i = 0; i < maxSurfaceIndex + 1; i++) 
        { 
            if ((surfacesMask & (1 << i)) == 0) 
                continue; 
 
            var surface = surfaceProvider.GetSurface(i); 
 
            if (panel != null) 
                panel.WriteText(""); 
            else 
                surface.WriteText(""); 
 
            surface.ContentType = ContentType.TEXT_AND_IMAGE; 
        } 
 
        status.IsCleared = true; 
    } 
 
    lcdStatuses[statusIndex] = status; 
} 
 
void UpdatePanel(DateTime now, IMyTextSurfaceProvider surfaceProvider, IMyTextPanel panel, 
    int surfacesMask, int minSurfaceIndex, int maxSurfaceIndex, ref LCDStatus status) 
{ 
    int statusIndex = -1; 
 
    for (int i = 0; i < status.DisplayConfigs.Length; i++) 
    { 
        if (status.DisplayConfigs[i].SurfaceIndex == status.SurfaceToUpdate) 
        { 
            statusIndex = i; 
            break; 
        } 
    } 
 
    var currentDisplayType = status.DisplayConfigs[statusIndex].CurrentDisplayType; 
    bool wasInvalid = currentDisplayType == DisplayTypes.Invalid; 
 
    if (wasInvalid || (now - status.LastCycle).TotalSeconds > Auto_Cycle_Interval_In_Seconds) 
    { 
        var displayTypes = parsedDisplayConfigs[statusIndex].DisplayTypes; 
        currentDisplayType = GetNextDisplayType(currentDisplayType, displayTypes); 
 
        status.DisplayConfigs[statusIndex].CurrentDisplayType = currentDisplayType; 
        status.LastCycle = now; 
    } 
 
    if (status.IsCleared || (wasInvalid && currentDisplayType != DisplayTypes.Invalid)) 
        status.IsCleared = false; 
 
    var surface = surfaceProvider.GetSurface(status.SurfaceToUpdate); 
 
    if (currentDisplayType == DisplayTypes.Invalid) 
    { 
        surface.ContentType = ContentType.TEXT_AND_IMAGE; 
        surface.Font = "Debug"; 
        surface.FontSize = 1f; 
        surface.BackgroundColor = Color.Black; 
        surface.FontColor = Color.White; 
 
        if (panel != null) 
            panel.WriteText("Config error: Invalid graph types\nin CustomData\nNOTE: LCDs may take a few seconds to\nupdate after changes"); 
        else 
            surface.WriteText("Config error: Invalid graph types\nin CustomData\nNOTE: LCDs may take a few seconds to\nupdate after changes"); 
    } 
    else 
    { 
        surface.ScriptBackgroundColor = Background_Color; 
        surface.ScriptForegroundColor = Color.White; 
        surface.ContentType = ContentType.SCRIPT; 
        surface.Script = ""; 
 
        if (wasInvalid) 
            surface.WriteText(""); 
 
        bool isWide = Enable_Long_History && (surface.TextureSize.X / surface.TextureSize.Y >= 1.5f); 
 
        UpdateDisplay(surface, currentDisplayType, isWide, now); 
    } 
 
    if (status.SurfaceToUpdate == maxSurfaceIndex) 
    { 
        status.LastUpdate = now; 
        status.SurfaceToUpdate = minSurfaceIndex; 
    } 
    else 
    { 
        for (int i = status.SurfaceToUpdate + 1; i <= maxSurfaceIndex; i++) 
        { 
            if ((surfacesMask & (1 << i)) == 0) 
                continue; 
 
            status.SurfaceToUpdate = i; 
            break; 
        } 
    } 
} 
 
static DisplayTypes GetNextDisplayType(DisplayTypes current, DisplayTypes choices) 
{ 
    if (choices == DisplayTypes.Invalid) 
        return DisplayTypes.Invalid; 
 
    if (choices == DisplayTypes.Auto) 
        return DisplayTypes.Auto; 
 
    if (current == DisplayTypes.Invalid || current == DisplayTypes.Auto) 
        current = DisplayTypes.First; 
 
    do 
    { 
        current = (DisplayTypes)((int)current << 1); 
 
        if (!IsDisplayTypeValid(current)) 
            current = DisplayTypes.First; 
    } 
    while ((choices & current) == 0); 
 
    return current; 
} 
 
void CycleDisplayType(DateTime now) 
{ 
    do 
    { 
        displayType = (DisplayTypes)((int)displayType << 1); 
 
        if (!IsDisplayTypeValid(displayType)) 
            displayType = DisplayTypes.First; 
    } 
    while (!IsDisplayTypeRelevant(displayType)); 
 
    lastDisplayCycle = now; 
} 
 
static bool IsDisplayTypeValid(DisplayTypes displayType) 
{ 
    switch (displayType) 
    { 
    case DisplayTypes.Auto: 
    case DisplayTypes.PowerUsage: 
    case DisplayTypes.PowerUsageScaled: 
    case DisplayTypes.BatteryCharge: 
    case DisplayTypes.BatteryIn: 
    case DisplayTypes.BatteryOut: 
    case DisplayTypes.BatteryInOut: 
    case DisplayTypes.BatteryInOutAuto: 
    case DisplayTypes.SolarOutput: 
    case DisplayTypes.ReactorOutput: 
    case DisplayTypes.H2PowerGenOutput: 
    case DisplayTypes.TurbineOutput: 
    case DisplayTypes.StoredOxygen: 
    case DisplayTypes.StoredHydrogen: 
        return true; 
    default: 
        return false; 
    } 
} 
 
bool IsDisplayTypeRelevant(DisplayTypes displayType) 
{ 
    switch (displayType) 
    { 
    case DisplayTypes.BatteryCharge: 
    case DisplayTypes.BatteryIn: 
    case DisplayTypes.BatteryOut: 
    case DisplayTypes.BatteryInOut: 
    case DisplayTypes.BatteryInOutAuto: 
        return batteries.Count > 0; 
 
    case DisplayTypes.SolarOutput: return solarPanels.Count > 0; 
    case DisplayTypes.ReactorOutput: return reactors.Count > 0; 
    case DisplayTypes.H2PowerGenOutput: return h2PowerGens.Count > 0; 
    case DisplayTypes.TurbineOutput: return turbines.Count > 0; 
    case DisplayTypes.StoredOxygen: return oxygenTankCount > 0; 
    case DisplayTypes.StoredHydrogen: return hydrogenTankCount > 0; 
    } 
 
    return true; 
} 
 
void ParseSurfaceDisplayConfigs(string text, int surfaceCount, List<DisplayConfig> configs, out int surfacesMask, out int minSurfaceIndex, out int maxSurfaceIndex) 
{ 
    surfacesMask = 0; 
    minSurfaceIndex = int.MaxValue; 
    maxSurfaceIndex = int.MinValue; 
 
    int textStartIndex = text.IndexOf(Lcd_Name_Tag); 
 
    var textPtr = textStartIndex == -1 ? new TextPtr(text) : new TextPtr(text, textStartIndex + Lcd_Name_Tag.Length).FindEndOfLine(true); 
 
    while (!textPtr.IsOutOfBounds()) 
    { 
        if (!textPtr.IsStartOfLine()) 
            break; 
 
        textPtr = textPtr.SkipWhitespace(); 
 
        var sepPtr = textPtr.FindInLine('='); 
 
        if (sepPtr.IsOutOfBounds()) 
            sepPtr = textPtr.FindInLine(':'); 
 
        var eol = textPtr.FindEndOfLine(); 
        var displayType = GetDisplayType(new StringSegment(text, textPtr.Index, (sepPtr.IsOutOfBounds() ? eol.Index : sepPtr.Index) - textPtr.Index)); 
 
        int surfaceIndex; 
 
        if (!sepPtr.IsOutOfBounds()) 
        { 
            sepPtr += 1; 
 
            var numText = sepPtr.Content.Substring(sepPtr, eol.Index - sepPtr.Index); 
 
            if (!int.TryParse(numText, out surfaceIndex)) 
                displayType = DisplayTypes.Invalid; 
        } 
        else 
        { 
            surfaceIndex = 0; 
        } 
 
        textPtr = eol.FindEndOfLine(true); 
 
        if (surfaceIndex < 0 || surfaceIndex >= 32) 
            continue; 
 
        if (surfaceIndex >= surfaceCount) 
        { 
            if (surfaceCount != 1) 
                continue; 
 
            displayType = DisplayTypes.Invalid; 
            surfaceIndex = 0; 
        } 
 
        AddDisplayConfig(configs, displayType, surfaceIndex); 
 
        minSurfaceIndex = Math.Min(minSurfaceIndex, surfaceIndex); 
        maxSurfaceIndex = Math.Max(maxSurfaceIndex, surfaceIndex); 
        surfacesMask |= 1 << surfaceIndex; 
    } 
 
    if (configs.Count == 0) 
    { 
        configs.Add(new DisplayConfig(0, DisplayTypes.Auto, DisplayTypes.Auto)); 
        minSurfaceIndex = 0; 
        maxSurfaceIndex = 0; 
        surfacesMask = 1; 
    } 
    else if (minSurfaceIndex == int.MaxValue) 
    { 
        minSurfaceIndex = 0; 
    } 
} 
 
static void AddDisplayConfig(List<DisplayConfig> configs, DisplayTypes displayType, int surfaceIndex) 
{ 
    bool replaced = false; 
 
    for (int i = 0; i < configs.Count; i++) 
    { 
        var config = configs[i]; 
 
        if (config.SurfaceIndex != surfaceIndex) 
            continue; 
 
        config.DisplayTypes |= displayType; 
        configs[i] = config; 
        replaced = true; 
    } 
 
    if (!replaced) 
        configs.Add(new DisplayConfig(surfaceIndex, displayType, DisplayTypes.Invalid)); 
} 
 
static DisplayTypes GetDisplayType(StringSegment typeString) 
{ 
    if      (typeString.EqualsIgnoreCase("usage"))            return DisplayTypes.PowerUsage; 
    if      (typeString.EqualsIgnoreCase("usageScaled"))      return DisplayTypes.PowerUsageScaled; 
    else if (typeString.EqualsIgnoreCase("battery"))          return DisplayTypes.BatteryCharge; 
    else if (typeString.EqualsIgnoreCase("batteryIn"))        return DisplayTypes.BatteryIn; 
    else if (typeString.EqualsIgnoreCase("batteryOut"))       return DisplayTypes.BatteryOut; 
    else if (typeString.EqualsIgnoreCase("batteryInOut"))     return DisplayTypes.BatteryInOut; 
    else if (typeString.EqualsIgnoreCase("batteryInOutAuto")) return DisplayTypes.BatteryInOutAuto; 
    else if (typeString.EqualsIgnoreCase("solar"))            return DisplayTypes.SolarOutput; 
    else if (typeString.EqualsIgnoreCase("reactor"))          return DisplayTypes.ReactorOutput; 
    else if (typeString.EqualsIgnoreCase("h2engine"))         return DisplayTypes.H2PowerGenOutput; 
    else if (typeString.EqualsIgnoreCase("wind"))             return DisplayTypes.TurbineOutput; 
    else if (typeString.EqualsIgnoreCase("storedOxygen"))     return DisplayTypes.StoredOxygen; 
    else if (typeString.EqualsIgnoreCase("storedHydrogen"))   return DisplayTypes.StoredHydrogen; 
    else return DisplayTypes.Invalid; 
} 
 
void UpdateDisplay(IMyTextSurface textSurface, DisplayTypes type, bool isWide, DateTime now) 
{ 
    if (type == DisplayTypes.Auto) 
        type = displayType; 
 
    var cacheKey = new CachedImageKey { 
        DisplayType = type, 
        TextureSize = textSurface.TextureSize, 
        SurfaceSize = textSurface.SurfaceSize 
    }; 
 
    int maxValueCount = isWide && Enable_Long_History ? longHistoryValueCount : shortHistoryValueCount; 
 
    CachedImage image; 
 
    if (cachedImages.TryGetValue(cacheKey, out image)) 
    { 
        if (!imageDrawnThisFrame && (now - image.LastUpdate).TotalSeconds > Math.Max(Data_Update_Interval_In_Seconds, LCD_Refresh_Interval_In_Seconds)) 
        { 
            DrawImage(textSurface, image, type, now, maxValueCount); 
            image.LastUpdate = now; 
            cachedImages[cacheKey] = image; 
        } 
    } 
    else 
    { 
        cachedImages[cacheKey] = image = new CachedImage { Sprites = new List<MySprite>(), LastUpdate = now }; 
        DrawImage(textSurface, image, type, now, maxValueCount); 
    } 
 
    using (var frame = textSurface.DrawFrame()) 
        frame.AddRange(image.Sprites); 
} 
 
void DrawImage(IMyTextSurface surface, CachedImage image, DisplayTypes displayType, DateTime now, int valueCount) 
{ 
    image.Sprites.Clear(); 
 
    var pos = (surface.TextureSize - surface.SurfaceSize) / 2 + Margin; 
    var areaSize = surface.SurfaceSize - Margin * 2; 
 
    float lineHeight = MeasureString(surface, " ", 1).Y; 
 
    if (errors.Count > 0) 
    { 
        FillRectangle(image.Sprites, new RectangleF(0, 0, surface.SurfaceSize.X, lineHeight + 2), Error_Background_Color); 
        float strWidth = MeasureString(surface, "CHECK ERRORS IN PB", 1).X; 
        DrawString(surface, image.Sprites, new Vector2((surface.SurfaceSize.X - strWidth) / 2, lineHeight / 2), "CHECK ERRORS IN PB", 1, Text_Color); 
 
        pos.Y += lineHeight + 2; 
        areaSize.Y -= lineHeight + 2; 
    } 
 
    int n = (int)displayType - 1; 
    int si = 0; 
 
    while (n > 0) 
    { 
        si++; 
        n >>= 1; 
    } 
 
    var graphArea = new RectangleF(pos.X + 10, pos.Y + lineHeight * 2 + 10, areaSize.X - 15, areaSize.Y - lineHeight * 3 - 15); 
 
    switch (graphSettings[si].Type) 
    { 
    case GraphType.Double: 
        graphArea.Y += (int)lineHeight * 2; 
        graphArea.Height -= (int)lineHeight * 2; 
        break; 
    case GraphType.Triple: 
        graphArea.Y += (int)lineHeight * 2; 
        graphArea.Height -= (int)lineHeight * 2; 
        break; 
    } 
 
    if (graphSettings[si].Symmetric) 
    { 
        var bl = graphArea.BottomLeft(); 
        DrawLine(surface, image.Sprites, bl, graphArea.Position, Graph_Axes_Color); 
 
        var halfHeight = new Vector2(0, graphArea.Height / 2); 
        DrawLine(surface, image.Sprites, graphArea.Position + halfHeight, graphArea.TopRight() + halfHeight, Graph_Axes_Color); 
    } 
    else 
    { 
        DrawGraphAxes(surface, image.Sprites, graphArea, Graph_Axes_Color); 
    } 
 
    graphArea.X += 3; // Adjust position of curves 
 
    int blockCount; 
    float maxValue1 = 0, maxValue2 = 0; 
    float[] values1, values2 = null, values3 = null; 
 
    switch (displayType) 
    { 
    case DisplayTypes.PowerUsage: 
        blockCount = usageBlockCount; 
        values1 = batteryUsageInValues; 
        values2 = powerReqValues; 
        values3 = powerUsageValues; 
        break; 
    case DisplayTypes.PowerUsageScaled: 
        blockCount = usageBlockCount; 
        values1 = batteryUsageInValues; 
        values2 = powerReqValues; 
        values3 = powerUsageValues; 
        maxValue1 = maxPowerReqValue; 
        break; 
    case DisplayTypes.BatteryCharge: 
        blockCount = batteries.Count; 
        maxValue1 = batteryMaxChargeValue; 
        values1 = batteryChargeValues; 
        break; 
    case DisplayTypes.BatteryIn: 
        blockCount = batteries.Count; 
        maxValue1 = batteryMaxInputValue; 
        values1 = batteryInputValues; 
        break; 
    case DisplayTypes.BatteryOut: 
        blockCount = batteries.Count; 
        maxValue1 = batteryMaxOutputValue; 
        values1 = batteryOutputValues; 
        break; 
    case DisplayTypes.BatteryInOut: 
        blockCount = batteries.Count; 
        maxValue1 = Math.Max(batteryMaxInputValue, batteryMaxOutputValue); 
        values1 = batteryInOutValues; 
        break; 
    case DisplayTypes.BatteryInOutAuto: 
        blockCount = batteries.Count; 
        values1 = batteryInOutValues; 
        break; 
    case DisplayTypes.SolarOutput: 
        blockCount = solarPanels.Count; 
        values1 = solarMaxOutputValues; 
        values2 = solarOutputValues; 
        break; 
    case DisplayTypes.ReactorOutput: 
        blockCount = reactors.Count; 
        maxValue1 = reactorOutputMaxValue; 
        values1 = reactorOutputValues; 
        break; 
    case DisplayTypes.H2PowerGenOutput: 
        blockCount = h2PowerGens.Count; 
        maxValue1 = h2GenMaxOutputValue; 
        values1 = h2PowerGenOutputValues; 
        break; 
    case DisplayTypes.TurbineOutput: 
        blockCount = turbines.Count; 
        values1 = turbineMaxOutputValues; 
        values2 = turbineOutputValues; 
        break; 
    case DisplayTypes.StoredOxygen: 
        blockCount = oxygenTankCount; 
        maxValue1 = maxStoredOxygenValue; 
        values1 = storedOxygenValues; 
        break; 
    case DisplayTypes.StoredHydrogen: 
        blockCount = hydrogenTankCount; 
        maxValue1 = maxStoredHydrogenValue; 
        values1 = storedHydrogenValues; 
        break; 
    default: 
        errors.Add($"SCRIPT ERROR:\nInvalid display type, '{displayType.ToString()}'\n\nPlease copy the error and report it\non the script workshop page"); 
        return; 
    } 
 
    switch (graphSettings[si].Type) 
    { 
    case GraphType.Single: 
        DrawValueGraph(surface, image.Sprites, pos, graphArea, blockCount, valueCount, values1, maxValue1, ref graphSettings[si]); 
        break; 
    case GraphType.Double: 
        DrawDualValueGraph(surface, image.Sprites, pos, graphArea, blockCount, valueCount, values1, maxValue1, values2, maxValue2, ref graphSettings[si]); 
        break; 
    case GraphType.Triple: 
        DrawTriValueGraph(surface, image.Sprites, pos, graphArea, blockCount, valueCount, maxValue1, values1, values2, values3, ref graphSettings[si]); 
        break; 
    } 
 
    graphArea.X--; 
 
    if (!IsDisplayTypeRelevant(displayType)) 
    { 
        // TODO: Add help text "If the blocks are on another grid check the allowed grid connectivity in the script configuration" 
        float strWidth = MeasureString(surface, "Data not measured", 1).X; 
        DrawString(surface, image.Sprites, new Vector2((surface.SurfaceSize.X - strWidth) / 2, (areaSize.Y - lineHeight) / 2), "Data not measured", 1, Text_Color); 
    } 
    else 
    { 
        var textPos = new Vector2(graphArea.X, graphArea.Bottom() + 2); 
        DrawString(surface, image.Sprites, ref textPos, GetTimeSpanString(now - sampleTimes[0]), 1, Text_Color); 
        DrawString(surface, image.Sprites, textPos, " ago", 1, Text_Color); 
        DrawString(surface, image.Sprites, new Vector2(graphArea.Right() - MeasureString(surface, "now", 1).X, graphArea.Bottom() + 2), "now", 1, Text_Color); 
    } 
 
    imageDrawnThisFrame = true; 
} 
 
void DrawGraphAxes(IMyTextSurface surface, List<MySprite> sprites, RectangleF graphArea, Color color) 
{ 
    var bl = graphArea.BottomLeft(); 
 
    DrawLine(surface, sprites, bl, graphArea.BottomRight(), color); 
    DrawLine(surface, sprites, bl, graphArea.Position, color); 
} 
 
void DrawValueGraph(IMyTextSurface surface, List<MySprite> sprites, Vector2 pos, RectangleF graphArea, 
    int blockCount, int maxValueCount, float[] values, float currentMaxValue, ref GraphSettings settings) 
{ 
    float rightEdge = pos.X + surface.SurfaceSize.X - Margin * 2; 
    DrawString(surface, sprites, new Vector2(rightEdge - MeasureString(surface, settings.Name, 1).X - 5, pos.Y + 1), settings.Name, 1, Text_Color); 
 
    float lineHeight = MeasureString(surface, " ", 1).Y; 
 
    var numberString = stringBuilder.Append(blockCount); 
    float numberOffset = rightEdge - MeasureString(surface, numberString, 1).X - 5; 
    DrawString(surface, sprites, new Vector2(numberOffset - MeasureString(surface, "x", 1).X - 3, pos.Y + 2 + lineHeight), "x", 1, Text_Color); 
    DrawString(surface, sprites, new Vector2(numberOffset, pos.Y + 2 + lineHeight), numberString.ToString(), 1, Text_Color); 
    stringBuilder.Clear(); 
 
    float peakValue = GetLargestValue(values, maxValueCount); 
    float maxValue = Math.Max(Math.Abs(peakValue), currentMaxValue); 
    float scale = 1f / maxValue; 
 
    if (currentMaxValue > 0 && currentMaxValue < maxValue) 
    { 
        float maxValueLineY = graphArea.Y + (graphArea.Height - currentMaxValue * scale * graphArea.Height); 
        DrawLine(surface, sprites, new Vector2(graphArea.X, maxValueLineY), new Vector2(graphArea.Right(), maxValueLineY), Max_Capacity_Color); 
    } 
 
    DrawCurve(surface, sprites, values, maxValueCount, graphArea, settings.Symmetric, scale, settings.Color1); 
 
    var textPos = new Vector2(pos.X + 5, pos.Y + 2); 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    DrawGraphValues(surface, sprites, textPos, values, settings.ValueScale, settings.Label1, settings.Color1, 1, settings.Unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    const float fontSize = 0.75f; 
 
    DrawString(surface, sprites, ref textPos, "Peak: ", fontSize, Text_Dim_Color); 
    DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, peakValue * settings.ValueScale, settings.Unit); 
    DrawString(surface, sprites, ref textPos, "; Max: ", fontSize, Text_Dim_Color); 
 
    if (currentMaxValue > 0) 
        DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, currentMaxValue * settings.ValueScale, settings.Unit); 
    else 
        DrawString(surface, sprites, ref textPos, "Auto", fontSize, Text_Color); 
} 
 
void DrawDualValueGraph(IMyTextSurface surface, List<MySprite> sprites, Vector2 pos, RectangleF graphArea, int blockCount, 
    int maxValueCount, float[] values1, float currentMaxValue1, float[] values2, float currentMaxValue2, ref GraphSettings settings) 
{ 
    float rightEdge = pos.X + surface.SurfaceSize.X - Margin * 2; 
    DrawString(surface, sprites, new Vector2(rightEdge - MeasureString(surface, settings.Name, 1).X - 5, pos.Y + 1), settings.Name, 1, Text_Color); 
 
    float valueScale = settings.ValueScale; 
    var unit = settings.Unit; 
 
    float lineHeight = MeasureString(surface, " ", 1).Y; 
 
    var numberString = blockCount.ToString(); 
    float numberOffset = rightEdge - MeasureString(surface, numberString, 1).X - 5; 
    DrawString(surface, sprites, new Vector2(numberOffset - MeasureString(surface, "x", 1).X, pos.Y + 2 + lineHeight), "x", 1, Text_Color); 
    DrawString(surface, sprites, new Vector2(numberOffset, pos.Y + 2 + lineHeight), numberString, 1, Text_Color); 
 
    float peakValue1 = GetLargestValue(values1, maxValueCount); 
    float peakValue2 = GetLargestValue(values2, maxValueCount); 
    float maxValue = Math.Max(Math.Max(peakValue1, currentMaxValue1), Math.Max(peakValue2, currentMaxValue2)); 
    float scale = 1f / maxValue; 
 
    DrawCurve(surface, sprites, values1, maxValueCount, graphArea, false, scale, settings.Color1); 
    DrawCurve(surface, sprites, values2, maxValueCount, graphArea, false, scale, settings.Color2); 
 
    var textPos = new Vector2(pos.X + 5, pos.Y + 2); 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    DrawGraphValues(surface, sprites, textPos, values1, valueScale, settings.Label1, settings.Color1, 1, unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    const float fontSize = 0.75f; 
    lineHeight = MeasureString(surface, " ", fontSize).Y; 
 
    DrawString(surface, sprites, ref textPos, "Peak: ", fontSize, Text_Dim_Color); 
    DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, peakValue1 * valueScale, unit); 
    DrawString(surface, sprites, ref textPos, "; Max: ", fontSize, Text_Dim_Color); 
 
    if (currentMaxValue1 > 0) 
        DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, currentMaxValue1 * valueScale, unit); 
    else 
        DrawString(surface, sprites, ref textPos, "Auto", fontSize, Text_Color); 
 
    textPos.X = pos.X + 5; 
    textPos.Y += lineHeight; 
 
    DrawGraphValues(surface, sprites, textPos, values2, valueScale, settings.Label2, settings.Color2, 1, unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    DrawString(surface, sprites, ref textPos, "Peak: ", fontSize, Text_Dim_Color); 
    DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, peakValue2 * valueScale, unit); 
    DrawString(surface, sprites, ref textPos, "; Max: ", fontSize, Text_Dim_Color); 
 
    if (currentMaxValue2 > 0) 
        DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, currentMaxValue2 * valueScale, unit); 
    else 
        DrawString(surface, sprites, ref textPos, "Auto", fontSize, Text_Color); 
} 
 
void DrawTriValueGraph(IMyTextSurface surface, List<MySprite> sprites, Vector2 pos, RectangleF graphArea, int blockCount, 
    int maxValueCount, float currentMaxValue, float[] values1, float[] values2, float[] values3, ref GraphSettings settings) 
{ 
    float rightEdge = pos.X + surface.SurfaceSize.X - Margin * 2; 
    float fontSize = 0.9f; 
 
    DrawString(surface, sprites, new Vector2(rightEdge - MeasureString(surface, settings.Name, fontSize).X - 5, pos.Y + 1), settings.Name, fontSize, Text_Color); 
 
    float valueScale = settings.ValueScale; 
    var unit = settings.Unit; 
 
    float lineHeight = MeasureString(surface, " ", fontSize).Y; 
 
    var numberString = blockCount.ToString(); 
    float numberOffset = rightEdge - MeasureString(surface, numberString, fontSize).X - 5; 
    DrawString(surface, sprites, new Vector2(numberOffset - MeasureString(surface, "x", fontSize).X, pos.Y + 2 + lineHeight), "x", fontSize, Text_Color); 
    DrawString(surface, sprites, new Vector2(numberOffset, pos.Y + 2 + lineHeight), numberString, fontSize, Text_Color); 
 
    float peakValue1 = GetLargestValue(values1, maxValueCount); 
    float peakValue2 = GetLargestValue(values2, maxValueCount); 
    float peakValue3 = GetLargestValue(values3, maxValueCount); 
    float peakValue = Math.Max(peakValue1, Math.Max(peakValue2, peakValue3)); 
    float maxValue = Math.Max(peakValue, currentMaxValue); 
    float scale = 1f / maxValue; 
 
    DrawCurve(surface, sprites, values1, maxValueCount, graphArea, false, scale, settings.Color1); 
    DrawCurve(surface, sprites, values2, maxValueCount, graphArea, false, scale, settings.Color2); 
    DrawCurve(surface, sprites, values3, maxValueCount, graphArea, false, scale, settings.Color3); 
 
    var textPos = new Vector2(pos.X + 5, pos.Y + 2); 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    DrawGraphValues(surface, sprites, textPos, values1, valueScale, settings.Label1, settings.Color1, fontSize, unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    DrawGraphValues(surface, sprites, textPos, values2, valueScale, settings.Label2, settings.Color2, fontSize, unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    DrawGraphValues(surface, sprites, textPos, values3, valueScale, settings.Label3, settings.Color3, fontSize, unit, lineHeight, scaleMultiplier); 
    textPos.Y += lineHeight; 
 
    fontSize = 0.75f; 
 
    DrawString(surface, sprites, ref textPos, "Peak: ", fontSize, Text_Dim_Color); 
    DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, maxValue * valueScale, unit); 
    DrawString(surface, sprites, ref textPos, "; Max: ", fontSize, Text_Dim_Color); 
 
    if (currentMaxValue > 0) 
        DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, currentMaxValue * valueScale, unit); 
    else 
        DrawString(surface, sprites, ref textPos, "Auto", fontSize, Text_Color); 
} 
 
void DrawGraphValues(IMyTextSurface surface, List<MySprite> sprites, Vector2 textPos, float[] values, float valueScale, 
                     string label, Color color, float fontSize, string unit, float lineHeight, float scaleMultiplier) 
{ 
    FillRectangle(sprites, new RectangleF(textPos + new Vector2(0, (lineHeight - 15 * scaleMultiplier) / 2), new Vector2(15, 15) * scaleMultiplier), color); 
    textPos.X += 20 * scaleMultiplier; 
    DrawString(surface, sprites, ref textPos, label, fontSize, Text_Dim_Color); 
    DrawValueAmount(surface, sprites, ref textPos, Text_Color, fontSize, values[values.Length - 1] * valueScale, unit); 
} 
 
void DrawValueAmount(IMyTextSurface surface, List<MySprite> sprites, ref Vector2 pos, Color color, float fontSize, float value, string unit) 
{ 
    float absValue = Math.Abs(value); 
 
    if (absValue == 0) 
    { 
        DrawString(surface, sprites, ref pos, "0", fontSize, color); 
    } 
    else if (absValue > 1E9f) 
    { 
        var a = value / 1E9f; 
        DrawString(surface, sprites, ref pos, Math.Round(a, 1).ToString(), fontSize, color); 
        DrawString(surface, sprites, ref pos, "G", fontSize, color); 
    } 
    else if (absValue > 1E6f) 
    { 
        var a = value / 1E6f; 
        DrawString(surface, sprites, ref pos, Math.Round(a, 1).ToString(), fontSize, color); 
        DrawString(surface, sprites, ref pos, "M", fontSize, color); 
    } 
    else if (absValue > 1000) 
    { 
        var a = value / 1000; 
        DrawString(surface, sprites, ref pos, Math.Round(a, 1).ToString(), fontSize, color); 
        DrawString(surface, sprites, ref pos, "K", fontSize, color); 
    } 
    else if (absValue < 0.1f) 
    { 
        DrawString(surface, sprites, ref pos, "<0.1", fontSize, color); 
    } 
    else 
    { 
        DrawString(surface, sprites, ref pos, Math.Round(value, 1).ToString(), fontSize, color); 
    } 
 
    DrawString(surface, sprites, ref pos, unit, fontSize, color); 
} 
 
static void FillRectangle(List<MySprite> sprites, RectangleF rectangle, Color color) 
{ 
    var center = rectangle.Position + rectangle.Size * 0.5f; 
    sprites.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", center, rectangle.Size, color)); 
} 
 
Vector2 MeasureString(IMyTextSurface surface, string text, float scale) 
{ 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
    var size = surface.MeasureStringInPixels(measureStringBuilder.Append(text), "Debug", scale); 
    measureStringBuilder.Clear(); 
 
    return size * scaleMultiplier; 
} 
 
Vector2 MeasureString(IMyTextSurface surface, StringBuilder text, float scale) 
{ 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    return surface.MeasureStringInPixels(text, "Debug", scale) * scaleMultiplier; 
} 
 
void DrawString(IMyTextSurface surface, List<MySprite> sprites, Vector2 pos, string text, float scale, Color color) 
{ 
    DrawString(surface, sprites, ref pos, text, scale, color); 
} 
 
void DrawString(IMyTextSurface surface, List<MySprite> sprites, ref Vector2 pos, string text, float scale, Color color) 
{ 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    var size = MeasureString(surface, text, scale); 
    var sprite = MySprite.CreateText(text, "Debug", color, scale * scaleMultiplier, TextAlignment.LEFT); 
    sprite.Position = pos; 
    sprites.Add(sprite); 
    pos.X += size.X; 
} 
 
static void DrawLine(IMyTextSurface surface, List<MySprite> sprites, Vector2 pos1, Vector2 pos2, Color color) 
{ 
    float scaleMultiplier = GetSurfaceScalingMultiplier(surface); 
 
    var delta = pos2 - pos1; 
    var pos = pos1 + delta * 0.5f; 
    float rotation = (float)Math.Atan2(pos1.Y - pos2.Y, pos2.X - pos1.X); 
 
    sprites.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", pos, new Vector2(delta.Length(), 3 * scaleMultiplier), color, rotation: rotation)); 
} 
 
void DrawCurve(IMyTextSurface surface, List<MySprite> sprites, float[] values, int maxValueCount, RectangleF area, bool symmetric, float heightScale, Color color) 
{ 
    int valueOffset = 0; 
 
    if (values.Length > maxValueCount) 
        valueOffset = values.Length - maxValueCount; 
 
    int valueCount = Math.Min(values.Length, maxValueCount); 
    float xScale = area.Width / maxValueCount; 
 
    float height = area.Height; 
    float minValue = 0; 
 
    if (symmetric) 
    { 
        height /= 2; 
        minValue = -1; 
    } 
 
    for (int i = 1; i < valueCount; i++) 
    { 
        if (!validSamples[i - 1] || !validSamples[i]) 
            continue; 
 
        float v = MathHelper.Clamp(values[valueOffset + i] * heightScale, minValue, 1); 
        float pv = MathHelper.Clamp(values[valueOffset + i - 1] * heightScale, minValue, 1); 
        float y = Math.Min(v * height, height); 
        float py = Math.Min(pv * height, height); 
 
        DrawLine(surface, sprites, area.Position + new Vector2((i - 1) * xScale, height - y), 
                                   area.Position + new Vector2(i * xScale, height - py), color); 
    } 
} 
 
static float GetSurfaceScalingMultiplier(IMyTextSurface surface) 
{ 
    var surfaceSize = surface.SurfaceSize; 
    return surfaceSize.X / 1024f + surfaceSize.Y / 1024f; 
} 
 
static void AddValueToArray<T>(T[] array, T value) 
{ 
    Array.Copy(array, 1, array, 0, array.Length - 1); 
 
    array[array.Length - 1] = value; 
} 
 
static float GetLargestValue(float[] array, int maxCount) 
{ 
    float largest = 0; 
 
    for (int i = array.Length - maxCount; i < array.Length; i++) 
        largest = Math.Abs(array[i]) > Math.Abs(largest) ? array[i] : largest; 
 
    return largest; 
} 
 
string GetTimeSpanString(TimeSpan timeSpan) 
{ 
    var text = stringBuilder.AppendTimeSpan(timeSpan).ToString(); 
    stringBuilder.Clear(); 
    return text; 
} 
 
readonly MyDefinitionId electricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity"); 
readonly MyDefinitionId oxygenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen"); 
readonly MyDefinitionId hydrogenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen"); 
 
void AddZeroValues() 
{ 
    AddValueToArray(validSamples, false); 
 
    AddValueToArray(powerUsageValues, 0); 
    AddValueToArray(powerReqValues, 0); 
    AddValueToArray(batteryUsageInValues, 0); 
 
    AddValueToArray(batteryChargeValues, 0); 
    batteryMaxChargeValue = 0; 
    AddValueToArray(batteryInputValues, 0); 
    batteryMaxInputValue = 0; 
    AddValueToArray(batteryOutputValues, 0); 
    batteryMaxOutputValue = 0; 
    AddValueToArray(batteryInOutValues, 0); 
 
    AddValueToArray(solarMaxOutputValues, 0); 
    AddValueToArray(solarOutputValues, 0); 
    AddValueToArray(reactorOutputValues, 0); 
    reactorOutputMaxValue = 0; 
    AddValueToArray(h2PowerGenOutputValues, 0); 
    h2GenMaxOutputValue = 0; 
    AddValueToArray(turbineMaxOutputValues, 0); 
    AddValueToArray(turbineOutputValues, 0); 
 
    if (Measure_Stored_Gas) 
    { 
        AddValueToArray(storedOxygenValues, 0); 
        maxStoredOxygenValue = 0; 
        AddValueToArray(storedHydrogenValues, 0); 
        maxStoredHydrogenValue = 0; 
    } 
} 
 
void RecordValues(List<IMyTerminalBlock> blocks) 
{ 
    AddValueToArray(validSamples, true); 
 
    float currentPowerUsage, currentPowerRequired, maxPowerRequired; 
 
    GetPowerStateAndFilterBlockTypes(blocks, 
        out currentPowerUsage, out currentPowerRequired, out maxPowerRequired); 
 
    maxPowerReqValue = maxPowerRequired; 
 
    AddValueToArray(powerUsageValues, currentPowerUsage); 
    AddValueToArray(powerReqValues, currentPowerRequired); 
 
    float storedPower, maxStoredPower; 
    float batteryInput, maxBatteryInput; 
    float batteryOutput, maxBatteryOutput; 
 
    GetBatteryValues(out storedPower, out maxStoredPower, 
                     out batteryInput, out maxBatteryInput, 
                     out batteryOutput, out maxBatteryOutput); 
 
    batteryMaxChargeValue = maxStoredPower; 
    batteryMaxInputValue = maxBatteryInput; 
    batteryMaxOutputValue = maxBatteryOutput; 
 
    AddValueToArray(batteryChargeValues, storedPower); 
    AddValueToArray(batteryInputValues, batteryInput); 
    AddValueToArray(batteryOutputValues, batteryOutput); 
    AddValueToArray(batteryInOutValues, batteryInput - batteryOutput); 
    AddValueToArray(batteryUsageInValues, Math.Max(0, batteryInput - batteryOutput)); 
 
    float solarMaxPower, solarPower; 
    GetSolarOutput(out solarMaxPower, out solarPower); 
    AddValueToArray(solarMaxOutputValues, solarMaxPower); 
    AddValueToArray(solarOutputValues, solarPower); 
 
    float reactorPower; 
    GetReactorOutput(out reactorPower, out reactorOutputMaxValue); 
    AddValueToArray(reactorOutputValues, reactorPower); 
 
    float generatorPower; 
    GetGeneratorOutput(out generatorPower, out h2GenMaxOutputValue); 
    AddValueToArray(h2PowerGenOutputValues, generatorPower); 
 
    float windMaxPower, windPower; 
    GetTurbineOutput(out windMaxPower, out windPower); 
    AddValueToArray(turbineMaxOutputValues, windMaxPower); 
    AddValueToArray(turbineOutputValues, windPower); 
 
    if (Measure_Stored_Gas) 
    { 
        float oxygenCapacity, storedOxygen, hydrogenCapacity, storedHydrogen; 
        GetStoredGas(out oxygenCapacity, out storedOxygen, out hydrogenCapacity, out storedHydrogen); 
 
        AddValueToArray(storedOxygenValues, storedOxygen); 
        maxStoredOxygenValue = oxygenCapacity; 
        AddValueToArray(storedHydrogenValues, storedHydrogen); 
        maxStoredHydrogenValue = hydrogenCapacity; 
    } 
} 
 
void GetPowerStateAndFilterBlockTypes(List<IMyTerminalBlock> blocks, 
    out float currentPowerUsage, out float currentPowerRequired, out float maxPowerRequired) 
{ 
    currentPowerUsage = 0; 
    currentPowerRequired = 0; 
    maxPowerRequired = 0; 
    usageBlockCount = 0; 
 
    batteries.Clear(); 
    solarPanels.Clear(); 
    reactors.Clear(); 
    h2PowerGens.Clear(); 
    turbines.Clear(); 
    gasTanks.Clear(); 
 
    foreach (var item in blocks) 
    { 
        if (item is IMyBatteryBlock) 
        { 
            batteries.Add((IMyBatteryBlock)item); 
            continue; 
        } 
        else if (item is IMySolarPanel) 
            solarPanels.Add((IMySolarPanel)item); 
        else if (item is IMyReactor) 
            reactors.Add((IMyReactor)item); 
        else if (item is IMyPowerProducer && item.GetType().Name == "MyHydrogenEngine") 
            h2PowerGens.Add((IMyPowerProducer)item); 
        else if (item is IMyPowerProducer && item.GetType().Name == "MyWindTurbine") 
            turbines.Add((IMyPowerProducer)item); 
        else if (item is IMyGasTank) 
            gasTanks.Add((IMyGasTank)item); 
 
        MyResourceSinkComponent sink; 
 
        if (item.Components.TryGet(out sink) && sink.AcceptsResourceType(electricityId)) 
        { 
            currentPowerUsage += sink.CurrentInputByType(electricityId); 
            currentPowerRequired += sink.RequiredInputByType(electricityId); 
            maxPowerRequired += sink.MaxRequiredInputByType(electricityId); 
            usageBlockCount++; 
        } 
    } 
} 
 
void GetBatteryValues( 
    out float storedPower, out float maxStoredPower, 
    out float input, out float maxInput, 
    out float output, out float maxOutput) 
{ 
    storedPower = 0; 
    maxStoredPower = 0; 
    input = 0; 
    maxInput = 0; 
    output = 0; 
    maxOutput = 0; 
 
    foreach (var item in batteries) 
    { 
        storedPower += item.CurrentStoredPower; 
        maxStoredPower += item.MaxStoredPower; 
        input += item.CurrentInput; 
        maxInput += item.MaxInput; 
        output += item.CurrentOutput; 
        maxOutput += item.MaxOutput; 
    } 
} 
 
void GetSolarOutput(out float solarMaxPower, out float solarPower) 
{ 
    solarMaxPower = 0; 
    solarPower = 0; 
 
    foreach (var item in solarPanels) 
    { 
        solarMaxPower += item.MaxOutput; 
        solarPower += item.CurrentOutput; 
    } 
} 
 
void GetReactorOutput(out float reactorPower, out float maxOutput) 
{ 
    reactorPower = 0; 
    maxOutput = 0; 
 
    foreach (var item in reactors) 
    { 
        reactorPower += item.CurrentOutput; 
        maxOutput += item.MaxOutput; 
    } 
} 
 
void GetGeneratorOutput(out float generatorPower, out float maxOutput) 
{ 
    generatorPower = 0; 
    maxOutput = 0; 
 
    foreach (var item in h2PowerGens) 
    { 
        generatorPower += item.CurrentOutput; 
        maxOutput += item.MaxOutput; 
    } 
} 
 
void GetTurbineOutput(out float windMaxPower, out float windPower) 
{ 
    windMaxPower = 0; 
    windPower = 0; 
 
    foreach (var item in turbines) 
    { 
        windMaxPower += item.MaxOutput; 
        windPower += item.CurrentOutput; 
    } 
} 
 
void GetStoredGas(out float oxygenCapacity, out float storedOxygen, out float hydrogenCapacity, out float storedHydrogen) 
{ 
    oxygenCapacity = 0; 
    storedOxygen = 0; 
    hydrogenCapacity = 0; 
    storedHydrogen = 0; 
 
    oxygenTankCount = 0; 
    hydrogenTankCount = 0; 
 
    foreach (var item in gasTanks) 
    { 
        MyResourceSinkComponent resourceSink; 
 
        if (!item.Components.TryGet(out resourceSink)) 
            continue; 
 
        float storedGas = (float)(item.Capacity * item.FilledRatio); 
 
        if (resourceSink.AcceptsResourceType(oxygenId)) 
        { 
            oxygenCapacity += item.Capacity; 
            storedOxygen += storedGas; 
            oxygenTankCount++; 
        } 
        else if (resourceSink.AcceptsResourceType(hydrogenId)) 
        { 
            hydrogenCapacity += item.Capacity; 
            storedHydrogen += storedGas; 
            hydrogenTankCount++; 
        } 
    } 
} 
} 
 
#region DataChunks 
 
struct DataChunk 
{ 
public string Name; 
public int DataOffset; 
public int DataLength; 
 
public DataChunk(string name, int dataOffset, int dataLength) 
{ 
    Name = name; 
    DataOffset = dataOffset; 
    DataLength = dataLength; 
} 
} 
 
public struct Serializer 
{ 
List<DataChunk> chunks; 
List<byte[]> dataArrays; 
int dataOffset; 
 
public static Serializer Create() 
{ 
    return new Serializer { 
        chunks = new List<DataChunk>(), 
        dataArrays = new List<byte[]>() 
    }; 
} 
 
void AddArray(string name, Array array, int elementSize) 
{ 
    int dataLength = array.Length * elementSize; 
    chunks.Add(new DataChunk(name, dataOffset, dataLength)); 
 
    var data = new byte[dataLength]; 
    Buffer.BlockCopy(array, 0, data, 0, dataLength); 
    dataArrays.Add(data); 
 
    dataOffset += dataLength; 
} 
 
public void AddArray(string name, ushort[] array) => AddArray(name, array, sizeof(ushort)); 
public void AddArray(string name, int[] array) => AddArray(name, array, sizeof(int)); 
public void AddArray(string name, long[] array) => AddArray(name, array, sizeof(long)); 
public void AddArray(string name, float[] array) => AddArray(name, array, sizeof(float)); 
 
public string Save() 
{ 
    var bytes = new List<byte>(); 
    var intArray = new int[1]; 
    var intBytes = new byte[sizeof(int)]; 
 
    dataOffset = 0; 
 
    bytes.AddArray(Encoding.Unicode.GetBytes("DCH2")); // Version 
    dataOffset += sizeof(char) * 4; 
 
    WriteInt(bytes, intArray, intBytes, chunks.Count); 
    dataOffset += sizeof(int); 
 
    for (int i = 0; i < chunks.Count; i++) 
        dataOffset += sizeof(int) + chunks[i].Name.Length * sizeof(char) + sizeof(int) + sizeof(int); 
 
    for (int i = 0; i < chunks.Count; i++) 
    { 
        var chunk = chunks[i]; 
        WriteInt(bytes, intArray, intBytes, chunk.Name.Length); 
        bytes.AddArray(Encoding.Unicode.GetBytes(chunk.Name)); 
        WriteInt(bytes, intArray, intBytes, chunk.DataOffset + dataOffset); 
        WriteInt(bytes, intArray, intBytes, chunk.DataLength); 
    } 
 
    for (int i = 0; i < dataArrays.Count; i++) 
        bytes.AddArray(dataArrays[i]); 
 
    return Convert.ToBase64String(bytes.ToArray()); 
} 
 
static void WriteInt(List<byte> bytes, int[] intArray, byte[] intBytes, int value) 
{ 
    intArray[0] = value; 
    Buffer.BlockCopy(intArray, 0, intBytes, 0, sizeof(int)); 
    bytes.AddArray(intBytes); 
} 
} 
 
public struct Deserializer 
{ 
byte[] data; 
DataChunk[] chunks; 
 
public bool IsValid => chunks != null; 
 
public static Deserializer LoadFromData(string data, Action<string> echo) 
{ 
    byte[] bytes; 
 
    if (data.StartsWith("DCH1", StringComparison.Ordinal)) 
    { 
        bytes = CharsToBytes(data.ToCharArray()); 
    } 
    else 
    { 
        try 
        { 
            bytes = Convert.FromBase64String(data); 
        } 
        catch (FormatException) 
        { 
            bytes = null; 
        } 
    } 
 
    if (bytes == null) 
        return default(Deserializer); 
 
    const int maxChunks = 50; 
 
    var chunks = LoadDataChunks(bytes, maxChunks, echo); 
 
    return new Deserializer { 
        data = bytes, 
        chunks = chunks 
    }; 
} 
 
static byte[] CharsToBytes(char[] chars) 
{ 
    var bytes = new byte[chars.Length * sizeof(char)]; 
    Buffer.BlockCopy(chars, 0, bytes, 0, bytes.Length); 
 
    return bytes; 
} 
 
static DataChunk[] LoadDataChunks(byte[] data, int maxChunks, Action<string> echo) 
{ 
    var intArray = new int[1]; 
    int byteOffset = 0; 
    var version = ReadVersion(data, ref byteOffset); 
 
    if (version != "DCH1" && version != "DCH2") 
    { 
        echo($"Invalid version '{version}'"); 
        return null; 
    } 
 
    int chunkCount = ReadInt(data, intArray, ref byteOffset); 
 
    if (chunkCount > maxChunks) 
    { 
        echo($"Invalid chunk count '{chunkCount}'"); 
        return null; 
    } 
 
    var chunks = new DataChunk[chunkCount]; 
 
    for (int i = 0; i < chunks.Length; i++) 
        chunks[i] = ReadDataChunk(data, intArray, ref byteOffset); 
 
    return chunks; 
} 
 
static string ReadVersion(byte[] data, ref int offset) 
{ 
    var version = Encoding.Unicode.GetString(data, offset, sizeof(char) * 4); 
    offset += sizeof(char) * 4; 
    return version; 
} 
 
static DataChunk ReadDataChunk(byte[] data, int[] intArray, ref int offset) 
{ 
    int nameLength = ReadInt(data, intArray, ref offset); 
    var name = Encoding.Unicode.GetString(data, offset, sizeof(char) * nameLength); 
 
    offset += nameLength * sizeof(char); 
 
    int dataOffset = ReadInt(data, intArray, ref offset); 
    int dataLength = ReadInt(data, intArray, ref offset); 
 
    return new DataChunk(name, dataOffset, dataLength); 
} 
 
static int ReadInt(byte[] bytes, int[] intArray, ref int offset) 
{ 
    Buffer.BlockCopy(bytes, offset, intArray, 0, sizeof(int)); 
 
    offset += sizeof(int); 
 
    return intArray[0]; 
} 
 
DataChunk? FindChunk(string name) 
{ 
    for (int i = 0; i < chunks.Length; i++) 
    { 
        if (chunks[i].Name == name) 
            return chunks[i]; 
    } 
 
    return null; 
} 
 
public bool HasChunk(string name) => FindChunk(name).HasValue; 
 
public bool LoadUShortArray(string name, ushort[] array) 
{ 
    var chunkOrNull = FindChunk(name); 
 
    if (chunkOrNull == null) 
        return false; 
 
    CopyChunkData(chunkOrNull.Value, array, sizeof(ushort)); 
 
    return true; 
} 
 
public int[] LoadIntArray(string name) 
{ 
    var chunkOrNull = FindChunk(name); 
 
    if (chunkOrNull == null) 
        return null; 
 
    var chunk = chunkOrNull.Value; 
    var array = new int[chunk.DataLength / sizeof(int)]; 
    CopyChunkData(chunk, array, sizeof(int)); 
 
    return array; 
} 
 
public long[] LoadLongArray(string name) 
{ 
    var chunkOrNull = FindChunk(name); 
 
    if (chunkOrNull == null) 
        return null; 
 
    var chunk = chunkOrNull.Value; 
    var array = new long[chunk.DataLength / sizeof(long)]; 
    CopyChunkData(chunk, array, sizeof(long)); 
 
    return array; 
} 
 
public bool LoadFloatArray(string name, float[] array) 
{ 
    var chunkOrNull = FindChunk(name); 
 
    if (chunkOrNull == null) 
        return false; 
 
    CopyChunkData(chunkOrNull.Value, array, sizeof(float)); 
 
    return true; 
} 
 
void CopyChunkData(DataChunk chunk, Array array, int elementSize) 
{ 
    int dataArrayLength = chunk.DataLength / elementSize; 
    int arrayOffset = 0; 
    int dataOffset = chunk.DataOffset; 
    int dataLength = chunk.DataLength; 
 
    if (array.Length > dataArrayLength) 
    { 
        arrayOffset = array.Length - dataArrayLength; 
    } 
    else if (dataArrayLength > array.Length) 
    { 
        dataOffset += (dataArrayLength - array.Length) * elementSize; 
        dataLength = array.Length * elementSize; 
    } 
 
    Buffer.BlockCopy(data, dataOffset, array, arrayOffset * elementSize, dataLength); 
} 
} 
 
#endregion 
 
static class Extensions 
{ 
public static Vector2 ToVector2(this Point point) => new Vector2(point.X, point.Y); 
public static int Right(this Rectangle rectangle) => rectangle.X + rectangle.Width; 
public static int Bottom(this Rectangle rectangle) => rectangle.Y + rectangle.Height; 
public static Point BottomLeft(this Rectangle rectangle) => new Point(rectangle.X, rectangle.Bottom); 
public static Point BottomRight(this Rectangle rectangle) => new Point(rectangle.Right, rectangle.Bottom); 
public static RectangleF ToRectangleF(this Rectangle rectangle) => new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height); 
 
public static float Right(this RectangleF rectangle) => rectangle.X + rectangle.Width; 
public static float Bottom(this RectangleF rectangle) => rectangle.Y + rectangle.Height; 
public static Vector2 TopRight(this RectangleF rectangle) => new Vector2(rectangle.X + rectangle.Width, rectangle.Y); 
public static Vector2 BottomLeft(this RectangleF rectangle) => new Vector2(rectangle.X, rectangle.Y + rectangle.Height); 
public static Vector2 BottomRight(this RectangleF rectangle) => new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height); 
public static Rectangle ToRectangle(this RectangleF rectangle) => new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height); 
 
// Space Engineers prohibits the Predicate Type >:( 
public static int FindIndex<T>(this List<T> list, Func<T, bool> predicate) 
{ 
    for (int i = 0; i < list.Count; i++) 
    { 
        if (predicate(list[i])) 
            return i; 
    } 
 
    return -1; 
} 
 
public static StringSegment[] SplitOnChar(this string _string, char separatorChar) 
{ 
    int separatorCount = 0; 
 
    for (int i = 0; i < _string.Length; i++) 
    { 
        if (_string[i] == separatorChar) 
            separatorCount++; 
    } 
 
    if (separatorCount == 0) 
        return new[] { new StringSegment(_string) }; 
 
    var parts = new StringSegment[separatorCount + 1]; 
 
    separatorCount = 0; 
    int prevIndex = -1; 
 
    for (int i = 0; i < _string.Length; i++) 
    { 
        if (_string[i] != separatorChar) 
            continue; 
 
        int start = prevIndex + 1; 
        parts[separatorCount++] = new StringSegment(_string, start, i - start); 
        prevIndex = i; 
    } 
 
    parts[separatorCount++] = new StringSegment(_string, prevIndex + 1, _string.Length - (prevIndex + 1)); 
 
    return parts; 
} 
 
public static bool AcceptsResourceType(this MyResourceSinkComponent sink, MyDefinitionId typeId) 
{ 
    foreach (var item in sink.AcceptedResources) 
    { 
        if (item == typeId) 
            return true; 
    } 
 
    return false; 
} 
 
// source.ResourceTypes is prohibited >:( 
//public static bool OutputsResourceType(this MyResourceSourceComponent source, MyDefinitionId typeId) 
//{ 
//    foreach (var item in source.ResourceTypes) 
//    { 
//        if (item == typeId) 
//            return true; 
//    } 
 
//    return false; 
//} 
 
public static void GetItemsOfType<T, TResult>(this List<T> list, List<TResult> outList) where TResult : T 
{ 
    outList.Clear(); 
 
    for (int i = 0; i < list.Count; i++) 
    { 
        var item = list[i]; 
 
        if (item is TResult) 
            outList.Add((TResult)item); 
    } 
} 
 
public static void GetItemsOfType<T, TResult>(this List<T> list, List<TResult> outList, Func<TResult, bool> predicate) where TResult : T 
{ 
    outList.Clear(); 
 
    for (int i = 0; i < list.Count; i++) 
    { 
        var item = list[i]; 
 
        if (item is TResult && predicate((TResult)item)) 
            outList.Add((TResult)item); 
    } 
} 
 
public static StringBuilder AppendTimeSpan(this StringBuilder sb, TimeSpan timeSpan) 
{ 
    if (timeSpan.Days > 0) 
    { 
        sb.Append((int)timeSpan.TotalDays); 
        sb.Append("d"); 
 
        if (timeSpan.Hours != 0 || timeSpan.Minutes != 0 || timeSpan.Seconds != 0) 
            sb.Append(" "); 
    } 
 
    if (timeSpan.Hours > 0 && timeSpan.Days < 2) 
    { 
        sb.Append(timeSpan.Hours); 
        sb.Append("h"); 
 
        if (timeSpan.Minutes == 0 || timeSpan.Seconds != 0) 
            sb.Append(" "); 
    } 
 
    if (timeSpan.Minutes > 0 && timeSpan.Days == 0) 
    { 
        sb.Append(timeSpan.Minutes); 
        sb.Append("m"); 
 
        if (timeSpan.Seconds != 0 && timeSpan.Hours == 0 && timeSpan.Days == 0) 
            sb.Append(" "); 
    } 
 
    if (timeSpan.Seconds != 0 && timeSpan.Hours == 0 && timeSpan.Days == 0) 
    { 
        sb.Append(timeSpan.Seconds); 
        sb.Append("s"); 
    } 
 
    return sb; 
} 
#if DEBUG 
}} 
#endif 
