            /*<------------------------------------------------------->
         * РАДИОЛОКАЦИОННАЯ СИСТЕМА <<Изумруд-1>>, версия 1.85. | RADAR SYSTEM <<Emerald-1>>, version 1.85.
         * Сделано в Н.С.К.С. Androidом                        | Made in USSR by Android
         *                                                     |
         *                                                     |
         * 
         * Руководство пользователя || User Manual  https://steamcommunity.com/sharedfiles/filedetails/?id=2072454011
         */
        //
        // Названия блоков
        string RemName = "УДТ"; // Название ДУ || RemCon name
        string Proj1n = "Гол. прицел 1"; // Название обычного гол. прицела || Name of Projector of basic sight
        string Proj2n = "Гол. прицел 2"; // Название гол. прицела при поиске цели || Name of Projector active sight
        string BTAG = "SOVIETRADARTRANSMIT"; // Тег для радиосвязи (соединяемые должны иметь один тег) || Broadcast tag 
        string CamName = "Камеры РЛС"; // Название группы камер || Cameras Group Name
        string TurretCamName = "Камеры турелей"; // Название группы камер для защиты от дружественного огня управляемых турелей || Name for group of turret cameras for save against friendly-fire
        string TurretsName = "Турели ГШ-23-6"; // Название группы турелей || Turrets Group Name
        string TurretsRName = "Турели для ракет"; // Название группы турелей для ракет || Turrets for missiles group name
        string TurretsIRName = "Турели для ИК-Ракет"; // Название группы турелей для ИК-Ракет || Name of group for IR-Missiles
        string LCDName = "Дисплей РЛС 1"; // Название дисплея графики || Name of graphical display
        string LCDName2 = "Дисплей РЛС 2"; // Название дисплея главной информации и списка целей || Name of basic information and targets list display
        string LCDName3 = "Дисплей РЛС 3"; // Название дисплея предупреждения || Name of warnings display
        string LCDName4 = "Дисплей 4"; // Название дисплея статуса ракет || Name of missile status display
        string LCDName5 = "Дисплей 5"; // Название дисплея статуса готовности ракет || Name of missile ready status display
        string LLCDName = "R_TARGET"; // Название дисплея для вывода координат захваченной цели || Name of  display for GPS coods
        string LCDSightn = "Радиоприцел"; // Название дисплея для вывода радиоприцела || Name of display for radio-sight
        string SettingsLCD = "Дисплей 6";  // Название дисплея для вывода изменяемых настроек || Name of display for show settings what can be change by args
        string AntName = "Передатчик М-58"; // Название антенны || Antenna name
        string SensName = "Сенсоры"; // Название группы сенсоров || Sensor group name
        string RLSName = "Изумруд-1М"; // Название РЛС (декор) || Name of RADAR (decor)
        string BatsName = "Аккумуляторы ракет"; // Название группы батарей ракет || Name of Missiles Battery Group
        string TanksName = "Водородные баки ракет"; // Название топливных баков ракет || Name of Missiles Tanks Group
        string MissilePBName = "ЦП ЗУР-9"; // Название ПБ ракет группы Р-4 || Name of R-4 PB Missile System
        string MissileRdavsPBName = "ПБ1"; // Название ПБ ракет от Rdav's || Name of PB with Rdav's PvP HM script
        string MissileWhipsPBName = "ПБ2"; // Название ПБ Launch Control для ракет Whip's  || Name of PB with Whip's launch control script
        string MissileLidatPBName = "ПБ3"; // Название ПБ Launch Control для ракет Alysius || Name of PB with Alysius launch control script
        string R_FORWARDName = "R_FORWARD";
        string IR_FORWARDName = "IR_FORWARD #A#";
        string NotR_FORWARDName = "Турель";
        string ControlledTurretsN = "Контролируемые турели"; // Название группы контролируемых турелей || Name of controlled turrets group
        string TurretPBName = "ЦП П-9"; // Название ПБ контроля роторных турелей || Name of PB for control rotor turrets
        //
        // Названия ракет || Missiles Name
        string MissileName = "Р-4Р"; // R-4 System Name
        string Missile1Name = "Rdav's"; // Rdav's System Name
        string Missile2Name = "Whip's"; // Whip's System Name
        string Missile3Name = "Alysius"; // Alysius System Name
        //
        // Декорирование вида РЛС, цели. (у каждого символа в игре разная длина на дисплее, вы можете поломать изображение РЛС, если увеличите длину.)
        // Customization of RADAR graphic (each symbol in the game has a different length on the display, you can broke the radar graphic if you increase the length.)
string Targ = "@"; // Цель
string R1b = "###           |             ###"; // РЛС
string R2b = "#               |                #";
string R3b = "                  |                  ";
string R4b = "                  |                  ";
string R5b = "__________*__________";
string R6b = "                  |                  ";
string R7b = "                  |                 ";
string R8b = "#                |                #";
string R9b = "###            |           ###";
        //
        // Декорирование текста РЛС (русский)
        string Name = "РЛС ";
        string TargetsD = "Целей обнаружено: ";
        string TargetsN = "Целей не обнаружено.";
        string Mode = "Режим РЛС: ";
        string TargetedT = "<<ВЫБРАННАЯ ЦЕЛЬ>>";
        string TNam = "Название цели: ";
        string TSpe = "Скорость цели: ";
        string TDis = "Расстояние до цели: ";
        string TRel = "Принадлежность цели: ";
        string Alar = "ПРЕДУПРЕЖДЕНИЯ РЛС ";
        string AMode = "Автоматический запуск ракет: ";
        string TurrO = "Роторные турели: ";
        //
        // Customization of radar information (ENG)
        string EName = "RADAR ";
        string ETargetsD = "Targets: ";
        string ETargetsN = "Targets not found.";
        string EMode = "RADAR MODE: ";
        string ETargetedT = "<<SELECTED TARGET>>";
        string ETNam = "Target name: ";
        string ETSpe = "Target speed: ";
        string ETDis = "Distance to target: ";
        string ETRel = "Target relationship: ";
        string EAlar = "RADAR WARNINGS ";
        string EAmode = "Automated launch of missiles: ";
        string ETurrO = "Rotor turrets: ";
        //
        // Настройки переменых
        // Variables Settings
        bool BroadcastEn = true;
        static bool UseTurretsForScan = true; // Позволяет сканировать камерами в направлении турели в которую смотрит игрок || Allows you to scan with cameras in the direction of the turret that the player is looking at 
        static double RemoveRate = 10; // не ставьте больше, чем вам нужно! || don't set more than you need!
        static double MaxRange = 3500; 
        static double BroadcastRange = 2000; // Дальность обмена информацией, не ставьте больше, чем вам нужно! || Target broadcast range, don't set more than you need!
        static bool Debug = false; 
        static bool Auto = true; 
        static bool MouseSelector = false; 
        static bool TargetUpd = true; 
        bool FriendlyOff = false;
        static bool ENG = false; 
        static bool AdvAct = true; 
        int Frequency = 15; 
        static int count = 5;
        static int Emode = 2; // Режим ракет Alysius, поддерживаются: 1,2,9,3
                              // Alysius missiles homing mode, supported: 1,2,9,3
        static bool AllInf = true; //Передавать все данные о цели? || Broadcast all target info?
        static bool UseTimersInstead = false;
        //
        // Статус ракет
        // Missile Status 
        static bool StatusEnabled = true; 
        static bool EmergencyOff = true; 
        static double EmergencyCharge = 50; 
        static double EmergencyTanks = 50; 
        static bool RdavEn = true; 
        static bool WhipsEn = true; 
        static bool AlysiusEn = true;
        //
        // Автопуск ракет
        // Autolaunch missiles
        bool SAutoLaunch = false;
        bool LAutoLaunch = false;
        static double SAutoRange = 1000;
        static double LAutoRange = 1000;
        static bool ScanLaunch = false;
        static int MissilesCount = 1;
        static double IgnoredSize = 5;
        static double SRate = 10;
        static double LRate = 10;
        static int SAtype = 2; // 0-3
        static int LAtype = 2; // 0-3
        static bool IDNotType = false; // Идентифицировать размер корабля не по типу сетки? || Identificate ship size not by grid type?
        static double SizeS = 40; // Корабль размером выше этого значения считается большим || Ship have size more than this value identificated as big
        //
        // Контроль турелей
        // Turrets control
        static bool ControlTurr = false;
        static double ShellSpeed = 300;
        static double TRange = 800;
        static bool OwnDamage = false;
        static double MaxCamDist = 10;
        //
        // Контроль роторных турелей
        // Control of rotor turrets
        static bool SendAllTargets = true; // Передавать все цели? || Transmit all targets?
        //
        /****************************************************************************************************/
bool update = true;
public string IsMode(bool Mode)
{
if (!ENG)
{
if (Mode == true)
{
if (!Scan)
{
return "Активный режим";
}
else
{
return "Активный режим, идёт захват цели";
}
}
if (Mode == false)
{
if (!Scan)
{
return "Пассивный режим";
}
else
{
return "Пассивный режим, идёт захват цели";
}
}
}
if (ENG)
{
if (Mode == true)
{
if (!Scan)
{
return "Active search mode";
}
else
{
return "Active search mode, scanning...";
}
}
if (Mode == false)
{
if (!Scan)
{
return "Passive search mode";
}
else
{
return "Passive search mode, scanning...";
}
}
}

return "";
}
public string IsMissiles(int Mode)
{
if (!ENG)
{
if (Mode == 0)
{
return "Выбраны ракеты " + MissileName;
}
if (Mode == 1)
{
return "Выбраны ракеты " + Missile1Name;
}
if (Mode == 2)
{
return "Выбраны ракеты " + Missile2Name;
}
if (Mode == 3)
{
return "Выбраны ракеты " + Missile3Name;
}
}
else
{
if (Mode == 0)
{
return "Selected Missiles " + MissileName;
}
if (Mode == 1)
{
return "Selected Missiles " + Missile1Name;
}
if (Mode == 2)
{
return "Selected Missiles " + Missile2Name;
}
if (Mode == 3)
{
return "Selected Missiles " + Missile3Name;
}
}
return "";
}
public string IsEnabled()
{
if (!ENG)
{
if (TPB != null) return "Подключены";
if (TPB == null) return "Не подключены";
}
else
{
if (TPB != null) return "Connected";
if (TPB == null) return "Not connected";
}
return "";
}
public string IsCharge(bool Mode)
{
if (!ENG)
{
if (Mode && Cameras.Count > 0)
{
double Charge = 0;
foreach (IMyCameraBlock cam in Cameras)
{
Charge += cam.AvailableScanRange;
}
return ("Заряд камер: " + Charge / Cameras.Count);
}
if (Mode && Cameras.Count <= 0)
{
return ("Камер нет, РЛС работает в пассивном режиме");
}
}
if (ENG)
{
if (Mode && Cameras.Count > 0)
{
double Charge = 0;
foreach (IMyCameraBlock cam in Cameras)
{
Charge += cam.AvailableScanRange;
}
return ("Charge of cameras: " + Charge / Cameras.Count);
}
if (Mode && Cameras.Count <= 0)
{
return ("No cameras, RADAR in passive scanning mode");
}
}
return "<---------------->";
}
public string IsAMode(bool small, bool large)
{
if (!ENG)
{
if (small || large)
{
return "Включён";
}
else return "Выключен";
}
else
{
if (small || large)
{
return "Online";
}
else return "Offline";
}
}
public bool IsRemote(IMyTerminalBlock block)
{
IMyRemoteControl rc = block as IMyRemoteControl;
if (rc != null) return true;
return false;
}
public bool IsLCD(IMyTerminalBlock block)
{
IMyTextPanel lcd = block as IMyTextPanel;
if (lcd != null) return true;
return false;
}
IMyTextPanel LCD2;
IMyTextPanel LCD3;
IMyTextPanel LCD4;
IMyTextPanel LCD5;
IMyTextPanel LLCD;
IMyTextPanel LCDSight;
IMyTextPanel DLCD;
IMyProjector Proj1;
IMyProjector Proj2;
IMyRadioAntenna ANT;
IMyBlockGroup Sensorsg;
IMyProgrammableBlock RRPB;
IMyProgrammableBlock RPB;
IMyProgrammableBlock WPB;
IMyProgrammableBlock EPB;
IMyProgrammableBlock TPB;
IMyTimerBlock RT;
IMyTimerBlock WT;
IMyTimerBlock ET;
IMyTimerBlock RRT;
IMyBlockGroup Turretg;
IMyBlockGroup CamerasG;
IMyBlockGroup gBats;
IMyBlockGroup gTanks;
StringBuilder sb = new StringBuilder();
List<string> RLSArray = new List<string>();
MyDetectedEntityInfo TargetedEnt = new MyDetectedEntityInfo();
List<Vector3D> TrTargets = new List<Vector3D>();
IMyBroadcastListener listener;
List<IMyLargeTurretBase> ControlledTurrets = new List<IMyLargeTurretBase>();
List<string> times = new List<string>();
List<string> targettimes = new List<string>();
List<IMyTerminalBlock> RCs = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
List<MyDetectedEntityInfo> TargetsBroad = new List<MyDetectedEntityInfo>();
List<MyDetectedEntityInfo> LTargets = new List<MyDetectedEntityInfo>();
List<IMyBatteryBlock> Bats = new List<IMyBatteryBlock>();
List<IMyGasTank> Tanks = new List<IMyGasTank>();
List<IMyProgrammableBlock> DPBs = new List<IMyProgrammableBlock>();
List<IMyCameraBlock> usedcams = new List<IMyCameraBlock>();
List<IMyLargeTurretBase> IRTurrets = new List<IMyLargeTurretBase>();
List<IMyLargeTurretBase> RTurrets = new List<IMyLargeTurretBase>();
List<MyDetectedEntityInfo> ATTargets = new List<MyDetectedEntityInfo>();
List<MyDetectedEntityInfo> Targets4 = new List<MyDetectedEntityInfo>();
List<MyDetectedEntityInfo> Targets = new List<MyDetectedEntityInfo>();
List<IMySensorBlock> Sensors = new List<IMySensorBlock>();
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();
List<IMyCameraBlock> TCameras = new List<IMyCameraBlock>();
List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();
bool Checked = false;
bool finished = false;
bool ActiveMode = false;
bool HasAntenna = false;
bool Scan = false;
bool start = false;
int MissileType = 0; // 0 - Р-4, 1 - рдав, 2 - вип, 3 - изи
Vector3D EVEL = new Vector3D();
int Frequency1 = 0;
int SelectedTarget = 0;
Program()
{
if (BroadcastEn)
{
listener = IGC.RegisterBroadcastListener(BTAG);
listener.SetMessageCallback(BTAG);
}
}
void Main(String args)
{
if (Auto)
{
Runtime.UpdateFrequency |= UpdateFrequency.Update10;
}
if (!update) Checked = true;
if (!Checked)
{
update = false;
if (StatusEnabled)
{
Tanks.Clear();
Bats.Clear();
}
ControlledTurrets.Clear();
IRTurrets.Clear();
Sensors.Clear();
Cameras.Clear();
TCameras.Clear();
Turrets.Clear();
RTurrets.Clear();
HasAntenna = false;
List<IMyRadioAntenna> ants = new List<IMyRadioAntenna>();
GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(ants);
if (ants.Count > 0) HasAntenna = true;
List<IMyLaserAntenna> ants1 = new List<IMyLaserAntenna>();
GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna>(ants1);
if (ants1.Count > 0) HasAntenna = true;
TPB = GridTerminalSystem.GetBlockWithName(TurretPBName) as IMyProgrammableBlock;
Proj1 = GridTerminalSystem.GetBlockWithName(Proj1n) as IMyProjector;
Proj2 = GridTerminalSystem.GetBlockWithName(Proj2n) as IMyProjector;
LCD5 = GridTerminalSystem.GetBlockWithName(LCDName5) as IMyTextPanel;
LLCD = GridTerminalSystem.GetBlockWithName(LLCDName) as IMyTextPanel;
DLCD = GridTerminalSystem.GetBlockWithName(SettingsLCD) as IMyTextPanel;
LCDSight = GridTerminalSystem.GetBlockWithName(LCDSightn) as IMyTextPanel;
ANT = GridTerminalSystem.GetBlockWithName(AntName) as IMyRadioAntenna;
GridTerminalSystem.SearchBlocksOfName(LCDName, LCDs, IsLCD);
GridTerminalSystem.SearchBlocksOfName(RemName, RCs, IsRemote);
LCD2 = GridTerminalSystem.GetBlockWithName(LCDName2) as IMyTextPanel;
LCD3 = GridTerminalSystem.GetBlockWithName(LCDName3) as IMyTextPanel;
LCD4 = GridTerminalSystem.GetBlockWithName(LCDName4) as IMyTextPanel;
RRPB = GridTerminalSystem.GetBlockWithName(MissilePBName) as IMyProgrammableBlock;
RPB = GridTerminalSystem.GetBlockWithName(MissileRdavsPBName) as IMyProgrammableBlock;
WPB = GridTerminalSystem.GetBlockWithName(MissileWhipsPBName) as IMyProgrammableBlock;
EPB = GridTerminalSystem.GetBlockWithName(MissileLidatPBName) as IMyProgrammableBlock;
if (UseTimersInstead)
{
RT = GridTerminalSystem.GetBlockWithName(MissileRdavsPBName) as IMyTimerBlock;
WT = GridTerminalSystem.GetBlockWithName(MissileWhipsPBName) as IMyTimerBlock;
ET = GridTerminalSystem.GetBlockWithName(MissileLidatPBName) as IMyTimerBlock;
RRT = GridTerminalSystem.GetBlockWithName(MissilePBName) as IMyTimerBlock;
}
Turretg = GridTerminalSystem.GetBlockGroupWithName(TurretsName);
CamerasG = GridTerminalSystem.GetBlockGroupWithName(CamName);
Sensorsg = GridTerminalSystem.GetBlockGroupWithName(SensName);
if (StatusEnabled)
{
gBats = GridTerminalSystem.GetBlockGroupWithName(BatsName);
gTanks = GridTerminalSystem.GetBlockGroupWithName(TanksName);
}
if ((Debug || BroadcastEn) && ANT != null)
{
ANT.Enabled = true;
}
if (CamerasG != null)
{
CamerasG.GetBlocksOfType(Cameras);
}
if (Sensorsg != null)
{
Sensorsg.GetBlocksOfType(Sensors);
}
if (Turretg != null)
{
Turretg.GetBlocksOfType(Turrets);
}
Turretg = GridTerminalSystem.GetBlockGroupWithName(TurretsRName);
if (Turretg != null)
{
Turretg.GetBlocksOfType(RTurrets);
}
Turretg = GridTerminalSystem.GetBlockGroupWithName(ControlledTurretsN);
if (Turretg != null)
{
Turretg.GetBlocksOfType(ControlledTurrets);
}
Turretg = GridTerminalSystem.GetBlockGroupWithName(TurretsIRName);
if (Turretg != null)
{
Turretg.GetBlocksOfType(IRTurrets);
}
CamerasG = GridTerminalSystem.GetBlockGroupWithName(TurretCamName);
if (CamerasG != null)
{
CamerasG.GetBlocksOfType(TCameras);
}
if (StatusEnabled)
{
if (gBats != null)
{
gBats.GetBlocksOfType(Bats);
}
if (gTanks != null)
{
gTanks.GetBlocksOfType(Tanks);
}
}
foreach (IMyLargeTurretBase turret in Turrets)
{
if (!turret.Enabled)
{
Turrets.Remove(turret);
break;
}
}
foreach (IMyLargeTurretBase turret in RTurrets)
{
if (!turret.Enabled)
{
RTurrets.Remove(turret);
break;
}
}
foreach (IMyCameraBlock turret in Cameras)
{
if (!turret.Enabled)
{
Cameras.Remove(turret);
break;
}
}
foreach (IMySensorBlock turret in Sensors)
{
if (!turret.Enabled)
{
Sensors.Remove(turret);
break;
}
}
if (RCs.Count > 0)
{
if (LCDs.Count <= 0)
{
if (!ENG)
{
Echo("Дисплей РЛС не обнаружен, РЛС работает...");
}
else
{
Echo("Display of RADAR not found, RADAR is working...");
}
}
else
{
foreach (IMyTextPanel lcd in LCDs)
{
lcd.Enabled = true;
}
}
if (LCD2 == null)
{
if (!ENG)
{
Echo("Дисплей информации о цели не обнаружен, РЛС работает...");
}
else
{
Echo("Display of RADAR info not found, RADAR is working...");
}
}
else
{
LCD2.Enabled = true;
}
if (LCD3 == null)
{
if (!ENG)
{
Echo("Дисплей вывода предупреждений не обнаружен, РЛС работает...");
}
else
{
Echo("Display of RADAR warnings not found, RADAR is working...");
}
}
else
{
LCD3.Enabled = true;
}
if (LCD4 == null)
{
if (!ENG)
{
Echo("Дисплей статуса ракет не обнаружен, РЛС работает...");
}
else
{
Echo("Display of missile status not found, RADAR is working...");
}
}
if (LCD5 == null && StatusEnabled)
{
if (!ENG)
{
Echo("Дисплей статуса готовности ракет не обнаружен, РЛС работает...");
}
else
{
Echo("Display of ready missiles status not found, RADAR is working...");
}
}
else
{
LCD4.Enabled = true;
}
if (LLCD != null)
{
LLCD.Enabled = true;
}
if (LCD5 != null)
{
LCD5.Enabled = true;
}
if (LCDSight != null)
{
LCDSight.Enabled = true;
}
if (DLCD != null)
{
DLCD.Enabled = true;
}
if (ANT == null)
{
if (!ENG && (!HasAntenna && BroadcastEn))
{
Echo("Передатчик не обнаружен, радиосвязь невозможна, РЛС работает...");
}
if (ENG && (!HasAntenna && BroadcastEn))
{
Echo("Antenna not found, broadcasting impossible, RADAR is working...");
}
}
if (Turrets.Count <= 0)
{
if (!ENG)
{
Echo("Турели не обнаружены, РЛС работает...");
}
else
{
Echo("Turrets not found, RADAR is working...");
}
}
if (Cameras.Count <= 0)
{
if (!ENG)
{
Echo("Камеры не обнаружены, РЛС работает...");
}
else
{
Echo("Cameras not found, RADAR is working...");
}
}
if (Sensors.Count <= 0)
{
if (!ENG)
{
Echo("Сенсоры не обнаружены, РЛС работает...");
}
else
{
Echo("Sensors not found, RADAR is working...");
}
}
if (RPB != null)
{
RPB.Enabled = true;
}
if (ControlledTurrets.Count <= 0 && ControlTurr)
{
if (!ENG)
{
Echo("Турели для управления не найдены, РЛС работает...");
}
else
{
Echo("Turrets for control not found, RADAR is working...");
}
}
if (Cameras.Count <= 0 && Turrets.Count <= 0 && Sensors.Count <= 0)
{
if (!ENG)
{
Echo("Ни камеры ни турели ни сенсоры не обнаружены, каким образом я должен работать?");
Checked = false;
}
else
{
Echo("Cameras, turrets and sensors not found, how i must work?");
Checked = false;
}
}
else
{
Checked = true;
}
}
else
{
if (!ENG)
{
Echo("Блок ДУ не обнаружен, работа невозможна");
Checked = false;
}
else
{
Echo("Remote Control not found, RADAR offline");
Checked = false;
}
}

}
if (RRPB != null || RPB != null || WPB != null || EPB != null || RT != null || RRT != null || WT != null || ET != null)
{
if ((RRPB == null || RRT == null) && MissileType == 0)
{
MissileType++;
}
if ((RPB == null || RT == null) && MissileType == 1)
{
MissileType++;
}
if ((WPB == null || WT == null) && MissileType == 2)
{
MissileType++;
}
if ((EPB == null || ET == null) && MissileType == 3)
{
MissileType = 0;
}
}
if (Checked)
{
if (!start)
{
if (StatusEnabled)
{
DPBs.Clear();
if (EmergencyOff)
{
if (RdavEn && RPB != null)
{
DPBs.Add(RPB);
}
if (WhipsEn && WPB != null)
{
DPBs.Add(WPB);
}
if (AlysiusEn && EPB != null)
{
DPBs.Add(EPB);
}
}
}

if (RRPB != null || RRT != null)
{
MissileType = 0;
}
else
{
if (RPB != null || RT != null)
{
MissileType = 1;
}
else
{
if (WPB != null || WT != null)
{
MissileType = 2;
}
else
{
if (EPB != null || ET != null)
{
MissileType = 3;
}
else
{
MissileType = 0;
}
}
}
}
Frequency1 = Frequency;
foreach (IMyCameraBlock cam in Cameras)
{
cam.EnableRaycast = true;
}
if (ENG)
{
Name = EName;
TargetsD = ETargetsD;
TargetsN = ETargetsN;
Mode = EMode;
TargetedT = ETargetedT;
TNam = ETNam;
TSpe = ETSpe;
TDis = ETDis;
TRel = ETRel;
Alar = EAlar;
AMode = EAmode;
TurrO = ETurrO;
}
foreach (IMyCameraBlock cam in TCameras)
{
cam.EnableRaycast = true;
}
start = true;
}
//
if (!finished)
{
float Angle = 0;
float Pitch = 0;
if (TrTargets.Count > 0 && Cameras.Count > 0)
{
foreach (Vector3D target in TrTargets)
{
for (int i = Cameras.Count - 1; i >= 0; i--)
{
IMyCameraBlock cam = Cameras[i];
if (!usedcams.Contains(cam) && cam.CanScan(target))
{
MyDetectedEntityInfo n;
n = cam.Raycast(target);
if (n.Name != "Планета" && n.Name != "Planet" && n.Name != Me.CubeGrid.CustomName && n.Type.ToString() != "Planet" && !n.IsEmpty() && ((n.Relationship.ToString() != "Owner" && n.Relationship.ToString() != "Friendly") || !FriendlyOff))
{
usedcams.Add(cam);
Targets.Add(n);
break;
}
else
{
continue;
}
}
else
{
continue;
}
}
}
}
TrTargets.Clear();
if (Cameras.Count > 0 && TargetUpd)
{
if (LTargets.Count > 0)
{
foreach (MyDetectedEntityInfo target in LTargets)
{
Targets.Add(target);
}
LTargets.Clear();
}
if (Targets.Count > 0)
{
foreach (MyDetectedEntityInfo target in Targets)
{
bool Contains = false;
foreach (MyDetectedEntityInfo target1 in Targets4)
{
if (target.EntityId == target1.EntityId)
{
Contains = true;
break;
}
}
if (!Contains)
{
bool exist = false;
Vector3D vector = target.Position + (Vector3D)target.Velocity / 5;
Vector3D dir = target.Position - Me.CubeGrid.GetPosition();
MyDetectedEntityInfo n = new MyDetectedEntityInfo();
for (int cam = Cameras.Count - 1; cam >= 0; cam--)
{
if ((Cameras[cam].CanScan(vector) && dir.Length() <= MaxRange && Targets.Count != Targets4.Count) && ((target.Relationship.ToString() != "Owner" && target.Relationship.ToString() != "Friendly") || !FriendlyOff))
{
n = Cameras[cam].Raycast(vector);
if (n.EntityId == Me.CubeGrid.EntityId) continue;
usedcams.Add(Cameras[cam]);
foreach (MyDetectedEntityInfo targett in Targets)
{
if (n.EntityId == targett.EntityId)
{
exist = true;
break;
}
}
break;
}
}
if (exist)
{
Targets4.Add(n);
continue;
}
else
{
if (n.IsEmpty())
{
for (int count1 = count; count1 >= 0; count1--)
{
Random rand = new Random();
MyDetectedEntityInfo n1 = new MyDetectedEntityInfo();
for (int i = Cameras.Count - 1; i >= 0; i--)
{
IMyCameraBlock cam1 = Cameras[i];
Vector3D vector1 = target.Position;
if (cam1.CanScan(vector1) && !usedcams.Contains(cam1))
{
n1 = cam1.Raycast(vector1);
usedcams.Add(cam1);
break;
}
}
for (int i = Cameras.Count - 1; i >= 0; i--)
{
if (!n1.IsEmpty()) break;
IMyCameraBlock cam1 = Cameras[i];
Vector3D vector1 = new Vector3D(target.Position.X, target.Position.Y, target.Position.Z + rand.Next(-7, 7));
if (cam1.CanScan(vector1) && !usedcams.Contains(cam1))
{
n1 = cam1.Raycast(vector1);
usedcams.Add(cam1);
break;
}
}
for (int i = Cameras.Count - 1; i >= 0; i--)
{
if (!n1.IsEmpty()) break;
IMyCameraBlock cam1 = Cameras[i];
Vector3D vector1 = new Vector3D(target.Position.X, target.Position.Y + rand.Next(-7, 7), target.Position.Z);
if (cam1.CanScan(vector1) && !usedcams.Contains(cam1))
{
n1 = cam1.Raycast(vector1);
usedcams.Add(cam1);
break;
}
}
for (int i = Cameras.Count - 1; i >= 0; i--)
{
if (!n1.IsEmpty()) break;
IMyCameraBlock cam1 = Cameras[i];
Vector3D vector1 = new Vector3D(target.Position.X + rand.Next(-7, 7), target.Position.Y, target.Position.Z);
if (cam1.CanScan(vector1) && !usedcams.Contains(cam1))
{
n1 = cam1.Raycast(vector1);
usedcams.Add(cam1);
break;
}
}
exist = false;
if (!n1.IsEmpty())
{
foreach (MyDetectedEntityInfo targett in Targets)
{
if (n1.EntityId == targett.EntityId)
{
exist = true;
break;
}
}
if (exist)
{
Targets4.Add(n1);
break;
}
}
}
}
//else break;
}
}
}
}
}
else
{
Targets.Clear();
}
if (Targets4.Count > 0)
{
Targets.Clear();
foreach (MyDetectedEntityInfo n in Targets4)
{
Targets.Add(n);
}
}
else
{
Targets.Clear();
}
if (TargetsBroad.Count > 0)
{
foreach (MyDetectedEntityInfo n in TargetsBroad)
{
if (Targets.Count <= 0) { Targets.Add(n); AddAuto(n); }

if (Targets.Count > 0)
{
bool exist = false;
foreach (MyDetectedEntityInfo v in Targets)
{
if (n.EntityId == v.EntityId)
{
exist = true;
break;
}
}
if (!exist)
{
Targets.Add(n);
AddAuto(n);
}
}
}
}
foreach (IMyRemoteControl RC in RCs)
{
if (RC.IsSameConstructAs(Me))
{
if (Scan && Cameras.Count > 0)
{
for (int ca = Cameras.Count - 1; ca >= 0; ca--)
{
IMyCameraBlock cam = Cameras[ca];
if (cam.AvailableScanRange > MaxRange * 3 && !usedcams.Contains(cam))
{
if (Proj1 != null && Proj2 != null)
{
Proj1.Enabled = false;
Proj2.Enabled = true;
}
MyDetectedEntityInfo d;
Vector3D vector = RC.GetPosition() + RC.WorldMatrix.Forward * MaxRange;
if (Turrets.Count > 0 && UseTurretsForScan)
{
IMyLargeTurretBase Cturret = Turrets[0];
foreach (IMyLargeTurretBase turret in Turrets)
{
if (turret.IsUnderControl)
{
Vector3D vector0;
Vector3D.CreateFromAzimuthAndElevation(-turret.Azimuth, -turret.Elevation, out vector0);
vector = turret.GetPosition() + vector0 * MaxRange;
break;
}
}
}
if (cam.CanScan(vector)) d = cam.Raycast(vector);
else continue;
if (!d.IsEmpty() && d.Name != "Планета" && d.Name != "Planet" && d.Name != cam.CubeGrid.CustomName && d.Type.ToString() != "Planet" && ((d.Relationship.ToString() != "Owner" && d.Relationship.ToString() != "Friendly") || !FriendlyOff))
{
if (Targets.Count <= 0)
{
if (ScanLaunch)
{
AddAuto(d);
}
Targets.Add(d);
Scan = false;
break;
}
if (Targets.Count > 0 && !Targets.Contains(d))
{
foreach (MyDetectedEntityInfo n in Targets)
{
if (n.EntityId != d.EntityId)
{
if (ScanLaunch)
{
AddAuto(d);
}
Targets.Add(d);
Scan = false;
break;
}
}
}
}
}
}
}
if (!Scan)
{
if (Proj1 != null && Proj2 != null)
{
Proj1.Enabled = true;
Proj2.Enabled = false;
}
}
break;
}
}
if (Cameras.Count > 0 && ActiveMode)
{
//
Frequency--;
int q = Cameras.Count;
if (Frequency <= 0)
{
while (!finished)
{
q--;
Echo(q.ToString());
IMyCameraBlock n = Cameras[q];
if (!usedcams.Contains(Cameras[q]))
{
MyDetectedEntityInfo target = new MyDetectedEntityInfo();
if (!AdvAct)
{
target = n.Raycast(MaxRange, 0, 0);
}
else
{
Random rand = new Random();
Angle = rand.Next(-45, 45);
Pitch = rand.Next(-45, 45);
target = n.Raycast(MaxRange, Angle, Pitch);
}
if (!Targets.Contains(target) && Targets.Count > 0)
{

for (int i1 = Targets.Count - 1; i1 >= 0; i1--)
{
MyDetectedEntityInfo i = Targets[i1];
if (i.EntityId != target.EntityId && !target.IsEmpty() && target.Name != "Планета" && target.Name != Me.CubeGrid.CustomName && target.Name != "Planet" && target.Type.ToString() != "Planet")
{
if (i1 == 0)
{
Targets.Add(target);
AddAuto(target);
break;
}
}
else
{
break;
}

}

}
if (Targets.Count == 0)
{
if (target.IsEmpty() == false && target.Name != "Планета" && target.Name != Me.CubeGrid.CustomName && target.Name != "Planet" && target.Type.ToString() != "Planet")
{
Targets.Add(target);
AddAuto(target);
}

}
if (q == 0)
{
if (Sensors.Count <= 0 && Turrets.Count <= 0)
{
finished = true;
}
else
{
break;
}
}
}
}
Frequency = Frequency1;
}
else
{
if (Sensors.Count <= 0 && Turrets.Count <= 0)
{
finished = true;
}
}
}
//
if (Turrets.Count > 0)
{

int i = Turrets.Count;
while (!finished)
{
i--;
Echo(i.ToString());
IMyLargeTurretBase n = Turrets[i];
if (!Targets.Contains(n.GetTargetedEntity()) && Targets.Count > 0)
{

foreach (MyDetectedEntityInfo z in Targets)
{
if (z.EntityId != n.GetTargetedEntity().EntityId && n.GetTargetedEntity().IsEmpty() == false && !Targets.Contains(n.GetTargetedEntity()))
{
Targets.Add(n.GetTargetedEntity());
AddAuto(n.GetTargetedEntity());
break;
}

}

}
if (Targets.Count <= 0)
{
if (n.GetTargetedEntity().IsEmpty() == false)
{
Targets.Add(n.GetTargetedEntity());
AddAuto(n.GetTargetedEntity());
}

}
if (i == 0)
{
if (Sensors.Count <= 0)
{
finished = true;
}
else
{
break;
}
}
}
}
//
if (Sensors.Count > 0)
{
int i1 = Sensors.Count;
while (!finished)
{
i1--;
List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();
Echo(i1.ToString());
IMySensorBlock n = Sensors[i1];
n.DetectedEntities(targets);
foreach (MyDetectedEntityInfo target in targets)
{
if (!Targets.Contains(target) && Targets.Count > 0)
{
bool exist = false;
foreach (MyDetectedEntityInfo z in Targets)
{
if (z.EntityId != target.EntityId && targets.Count > 0) exist = true;
}
if (!exist)
{
Targets.Add(target);
AddAuto(target);
break;
}
}
if (Targets.Count == 0)
{
if (target.IsEmpty() == false)
{
Targets.Add(target);
AddAuto(target);
}

}
}
if (i1 == 0)
{
finished = true;
}
}
}
//
if (Sensors.Count <= 0 && Turrets.Count <= 0 && !ActiveMode)
{
finished = true;
}
}
//
if (finished)
{
if (!MouseSelector && Targets.Count > 0)
{
if (SelectedTarget < 0)
{
SelectedTarget = 0;
}
if (SelectedTarget > Targets.Count - 1)
{
SelectedTarget = 0;
}
if (TargetedEnt.IsEmpty())TargetedEnt = Targets[SelectedTarget];
}
foreach (IMyRemoteControl RC in RCs)
{
if (Targets.Count > 0)
{
if (MouseSelector && RC.IsSameConstructAs(Me))
{
if (Targets.Count > 1)
{
for (int z = Targets.Count - 1; z >= 1; z--)
{
Vector3D Direction2 = Targets[z].Position - RC.CubeGrid.GetPosition();
Vector3D NeedFDirection2 = RC.WorldMatrix.Forward - Vector3D.Normalize(Direction2);
Vector3D Direction21 = Targets[z - 1].Position - RC.CubeGrid.GetPosition();
Vector3D NeedFDirection21 = RC.WorldMatrix.Forward - Vector3D.Normalize(Direction21);
for (int q = Targets.Count - 1; q >= 0; q--)
{
if (NeedFDirection2.Length() <= NeedFDirection21.Length())
{
TargetedEnt = Targets[z];
}
else
{
TargetedEnt = Targets[z - 1];
}
}
}
}
if (Targets.Count == 1)
{
TargetedEnt = Targets[0];
}
}
if (!MouseSelector)
{
foreach (MyDetectedEntityInfo n in Targets)
{
if (n.EntityId == TargetedEnt.EntityId)
{
TargetedEnt = n;
break;
}
}
}
}
foreach (IMyTextPanel LCD in LCDs)
{
string R1 = R1b;
string R2 = R2b;
string R3 = R3b;
string R4 = R4b;
string R5 = R5b;
string R6 = R6b;
string R7 = R7b;
string R8 = R8b;
string R9 = R9b;
RLSArray.Add(R1);
RLSArray.Add(R2);
RLSArray.Add(R3);
RLSArray.Add(R4);
RLSArray.Add(R5);
RLSArray.Add(R6);
RLSArray.Add(R7);
RLSArray.Add(R8);
RLSArray.Add(R9);
if (Targets.Count > 0)
{
foreach (MyDetectedEntityInfo n in Targets)
{
Vector3D EnemyPos2 = new Vector3D();
if (EnemyPos2 == new Vector3D())
{
EnemyPos2 = n.Position;
}
Vector3D EnemyPos = n.Position;
Vector3D OurPos = RC.CubeGrid.GetPosition();
Vector3D Direction = EnemyPos - OurPos;
EVEL = EnemyPos - EnemyPos2;
double VertMapDir = Vector3D.Dot(RC.WorldMatrix.Forward, Vector3D.Normalize(Direction)) * Direction.Length();
double HorRast = Vector3D.Dot(RC.WorldMatrix.Left, Vector3D.Normalize(Direction)) * Direction.Length();
if (VertMapDir > MaxRange)
{
VertMapDir = MaxRange;
}
if (VertMapDir < -MaxRange)
{
VertMapDir = -MaxRange;
}
if (HorRast > MaxRange)
{
HorRast = MaxRange;
}
if (HorRast < -MaxRange)
{
HorRast = -MaxRange;
}
//
// Верт. - 9
// Гор. - 20
int Hor = 0;
int Vert = 0;
int Vert1 = 0;
double VertNeed = Math.Abs(VertMapDir / MaxRange * 100);
double HorNeed = Math.Abs(HorRast / MaxRange * 100);
// максимум - 100%, т.е. граница дисплея
if (VertMapDir < 0)
{
for (Vert = 4; VertNeed >= 25; VertNeed -= 25)
{
Vert++;
}
}
if (VertMapDir > 0)
{
for (Vert = 4; VertNeed >= 25; VertNeed -= 25)
{
Vert--;
}
}
Vert1 = Vert;
if (Vert1 > 9)
{
Vert1 = 9;
}
if (Vert1 < 0)
{
Vert1 = 0;
}
if (HorRast > 0)
{
for (Hor = RLSArray[Vert].Length / 2; HorNeed >= RLSArray[Vert].Length / 2; HorNeed -= RLSArray[Vert].Length / 2)
{
Hor--;
}
}
if (HorRast < 0)
{
for (Hor = RLSArray[Vert].Length / 2; HorNeed >= RLSArray[Vert].Length / 2; HorNeed -= RLSArray[Vert].Length / 2)
{
Hor++;
}
}
if (Hor < 0)
{
Hor = 0;
}
if (Hor > RLSArray[Vert].Length)
{
Hor = RLSArray[Vert].Length;
}
RLSArray[Vert1] = RLSArray[Vert1].Remove(Hor, 1);
RLSArray[Vert1] = RLSArray[Vert1].Insert(Hor, Targ);
if (TargetedEnt.EntityId.Equals(n.EntityId))
{
if (Hor <= RLSArray[Vert1].Length - 1)
{
RLSArray[Vert1] = RLSArray[Vert1].Remove((Hor + 1), 1);
RLSArray[Vert1] = RLSArray[Vert1].Insert((Hor + 1), ">");
}
if (Hor >= 1)
{
RLSArray[Vert1] = RLSArray[Vert1].Remove((Hor - 1), 1);
RLSArray[Vert1] = RLSArray[Vert1].Insert((Hor - 1), "<");
}
}

if (Debug && ANT != null)
{
ANT.HudText = (count.ToString());
}
EnemyPos2 = EnemyPos;
//
}
}
LCD.FontSize = 2;
sb.Clear();
foreach (string b in RLSArray)
{
sb.Append(b);
sb.AppendLine();
}
LCD.WriteText(sb.ToString());
LCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
RLSArray.Clear();
}
}
//
//
if (LCD2 != null)
{
sb.Clear();
if (Targets.Count > 0 && finished)
{
sb.Append(Name + RLSName);
sb.AppendLine();
sb.Append(TargetsD + Targets.Count);
sb.AppendLine();
sb.Append(Mode + IsMode(ActiveMode));
sb.AppendLine();
sb.Append(IsCharge(ActiveMode));
sb.AppendLine();
sb.Append(AMode + IsAMode(SAutoLaunch, LAutoLaunch));
sb.AppendLine();
sb.Append(TurrO + IsEnabled());
sb.AppendLine();
if (!TargetedEnt.IsEmpty())
{
sb.Append(TargetedT);
sb.AppendLine();
Vector3D Dist1 = TargetedEnt.Position;
Vector3D Dist2 = Me.CubeGrid.GetPosition();
Vector3D Dist3 = Dist2 - Dist1;
sb.Append(TNam + TargetedEnt.Name);
sb.AppendLine();
sb.Append(TSpe + TargetedEnt.Velocity.Length());
sb.AppendLine();
sb.Append(TDis + Dist3.Length());
sb.AppendLine();
sb.Append(TRel + TargetedEnt.Relationship);
sb.AppendLine();
}
for (int i = Targets.Count - 1; i >= 0; i--)
{
MyDetectedEntityInfo n = Targets[i];
if (TargetedEnt.EntityId.Equals(n.EntityId))
{
if (i > 0)
{
i--;
n = Targets[i];
}
else break;
}
Vector3D Dist1 = n.Position;
Vector3D Dist2 = Me.CubeGrid.GetPosition();
Vector3D Dist3 = Dist2 - Dist1;
sb.Append(TNam + n.Name);
sb.AppendLine();
sb.Append(TSpe + n.Velocity.Length());
sb.AppendLine();
sb.Append(TDis + Dist3.Length());
sb.AppendLine();
sb.Append(TRel + n.Relationship);
sb.AppendLine();
}
}
else
{
sb.Append(Name + RLSName);
sb.AppendLine();
sb.Append(TargetsN);
sb.AppendLine();
sb.Append(Mode + IsMode(ActiveMode));
sb.AppendLine();
sb.Append(IsCharge(ActiveMode));
sb.AppendLine();
sb.Append(AMode + IsAMode(SAutoLaunch, LAutoLaunch));
sb.AppendLine();
if (!ENG)
{
if (RPB != null || RT != null)
{
if (RTurrets.Count < 0)
{
sb.Append("Ракеты Rdav's подключены, но нет турелей");
sb.AppendLine();
}
if (RTurrets.Count > 0)
{
sb.Append("Ракеты Rdav's подключены");
sb.AppendLine();
}
}
if (WPB != null || WT != null)
{
if (RTurrets.Count < 0)
{
sb.Append("Ракеты Whip's подключены, но нет турели");
sb.AppendLine();
}
if (RTurrets.Count > 0)
{
sb.Append("Ракеты Whip's подключены");
sb.AppendLine();
}
}
if (EPB != null || ET != null)
{
if (LLCD == null)
{
sb.Append("Ракеты Alysius подключены, но нет дисплея");
sb.AppendLine();
}
if (LLCD != null)
{
sb.Append("Ракеты Alysius подключены");
sb.AppendLine();
}
}
if (RRPB != null)
{
if (LLCD != null)
{
sb.Append("Ракеты системы Р-4 подключены");
sb.AppendLine();
}
}
}
if (ENG)
{
if (RPB != null)
{
if (RTurrets.Count < 0)
{
sb.Append("Rdav's missiles connected, but no turrets");
sb.AppendLine();
}
if (RTurrets.Count > 0)
{
sb.Append("Rdav's missiles connected and ready");
sb.AppendLine();
}
}
if (WPB != null)
{
if (RTurrets.Count < 0)
{
sb.Append("Whip's missiles connected, but no turret");
sb.AppendLine();
}
if (RTurrets.Count > 0)
{
sb.Append("Whip's missiles connected");
sb.AppendLine();
}
}
if (EPB != null || ET != null)
{
if (LLCD == null)
{
sb.Append("Alysius missiles connected, but no display");
sb.AppendLine();
}
if (LLCD != null)
{
sb.Append("Alysius missiles connected");
sb.AppendLine();
}
}
if (RRPB != null)
{
if (LLCD != null)
{
sb.Append("Missile R-4 system connected");
sb.AppendLine();
}
}
}

}
LCD2.WriteText(sb.ToString());
LCD2.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}
if (LCD3 != null)
{
sb.Clear();
sb.Append(Alar + RLSName);
sb.AppendLine();
if (Cameras.Count > 0 && Targets.Count > 0 && TargetUpd)
{
double Charge = 0;
foreach (IMyCameraBlock cam in Cameras)
{
Charge += cam.AvailableScanRange;
}
if (Charge / Cameras.Count <= MaxRange + 300)
{
if (!ENG)
{
sb.Append("Заряд камер близок к растрате, цели могут сорваться");
sb.AppendLine();
}
else
{
sb.Append("Charge of cameras near to zero, targets can be lost");
sb.AppendLine();
}
}
}
//
if (Cameras.Count > 0 && Cameras.Count <= MaxRange /2000)
{
if (!ENG)
{
sb.Append("Камер недостаточно для эффективного сканирования");
sb.AppendLine();
sb.Append("на такую дистанцию. Увеличьте число камер.");
sb.AppendLine();
}
else
{
sb.Append("Not enough cameras for effective scanning");
sb.AppendLine();
sb.Append("on such distance. Increase amount of cameras");
sb.AppendLine();
}
}
if (Cameras.Count > 0 && Cameras.Count < Targets.Count && TargetUpd)
{
if (!ENG)
{
sb.Append("Камер недостаточно для ведения такого кол-ва целей");
sb.AppendLine();
}
else
{
sb.Append("Not enough cameras for lock-on this amount of targets");
sb.AppendLine();
}
}
if (Turrets.Count <= 0 && Cameras.Count > 0)
{
if (!ENG)
{
sb.Append("В РЛС отсутствуют турели при наличии камер.");
sb.AppendLine();
sb.Append("Турели позволят гарантированно обнаружить цель на малых");
sb.AppendLine();
sb.Append("расстояниях, а так же повысят надёжность ведения цели.");
sb.AppendLine();
}
else
{
sb.Append("Radar not have turrets.");
sb.AppendLine();
sb.Append("Turrets will help with detect targets on small distances");
sb.AppendLine();
sb.Append("and will increase the reliability");
sb.AppendLine();
}
}
if (ActiveMode)
{
if (!ENG)
{
sb.Append("Активный режим РЛС может вызывать лаги");
sb.AppendLine();
if (Frequency1 < 15)
{
sb.Append("Частота сканирования очень высокая, лучше уменьшить её");
sb.AppendLine();
}
}
else
{
sb.Append("Active search RADAR mode can cause lags");
sb.AppendLine();
if (Frequency1 < 15)
{
sb.Append("Scan frequency is too big, better to reduce it");
sb.AppendLine();
}
}
}
if (Targets.Count > 0 && TargetUpd && Cameras.Count > 0)
{
foreach(MyDetectedEntityInfo target in Targets)
{
if (target.BoundingBox.Size.Length() <= 25)
{
if (!ENG)
{
sb.Append("Цель " + target.Name);
sb.AppendLine();
sb.Append("Имеет малые размеры, захват ненадёжен.");
sb.AppendLine();
}
if (ENG)
{
sb.Append("Target " + target.Name);
sb.AppendLine();
sb.Append("Has small size, lock-on unsafe.");
sb.AppendLine();
}
}
}
}
if (MouseSelector)
{
if (!ENG)
{
sb.Append("Включён автоматический приоритет целей.");
sb.AppendLine();
sb.Append("Уберите его, если хотите выбрать цель вне");
sb.AppendLine();
sb.Append("зависимости от своей ориентации.");
sb.AppendLine();
}
else
{
sb.Append("Auto priority of target selection online");
sb.AppendLine();
sb.Append("Turn off, if you want select target manually");
sb.AppendLine();
sb.Append("and without depend from your direction");
sb.AppendLine();
}
}
if (LAutoLaunch || SAutoLaunch)
{
if (!ENG)
{
sb.Append("Автозапуск ракет включён, вы НЕ можете");
sb.AppendLine();
sb.Append("выбирать цели вручную при атаке целей им!");
sb.AppendLine();
}
else
{
sb.Append("Autolaunch of missiles online, you CAN'T");
sb.AppendLine();
sb.Append("select target manually for attack them by it!");
sb.AppendLine();
}
if (Cameras.Count <= 0)
{
if (!ENG)
{
sb.Append("Автозапуск ракет включён, но нет камер. Убедитесь, что");
sb.AppendLine();
sb.Append("это вам не помешает");
sb.AppendLine();
}
else
{
sb.Append("Autolaunch of missiles online, but cameras not found. I hope you");
sb.AppendLine();
sb.Append("sure what it's can't broke your idea");
sb.AppendLine();
}
}
if ((!UseTimersInstead && (RPB == null && WPB == null && EPB == null)) || (UseTimersInstead && (RT == null && WT == null && ET == null)))
{
if (!ENG)
{
sb.Append("Автозапуск ракет включён, но нет ни одной точки");
sb.AppendLine();
sb.Append("доступа к оным. Как и что я буду запускать?");
sb.AppendLine();
}
else
{
sb.Append("Autolaunch of missiles online, but not found any");
sb.AppendLine();
sb.Append("PB or timer for them. How and what i will launch?");
sb.AppendLine();
}
}
if (RTurrets.Count <= 0 && ((SAtype != 3 && Emode != 1 && SAtype != 0 && SAutoLaunch) || (LAtype != 3 && Emode != 1) && LAtype != 0 && LAutoLaunch))
{
if (!ENG)
{
sb.Append("Автозапуск ракет включён, но нет турелей для");
sb.AppendLine();
sb.Append("ведения атакуемой цели. Установите, если нужна");
sb.AppendLine();
}
else
{
sb.Append("Autolaunch enabled, but no turrets for guidance");
sb.AppendLine();
sb.Append("not found. Build them if you need!");
sb.AppendLine();
}
}
if (LLCD == null && ((SAutoLaunch && SAtype == 3 && Emode == 1) || (LAtype == 3 && Emode == 1 && LAutoLaunch)))
{
if (!ENG)
{
sb.Append("Автозапуск ракет включён, но нет дисплея для");
sb.AppendLine();
sb.Append("ведения атакуемой цели. Установите или измените режим ракеты!");
sb.AppendLine();
}
else
{
sb.Append("Autolaunch enabled, but no display for guidance");
sb.AppendLine();
sb.Append("not found. Build it or change launch mode!");
sb.AppendLine();
}
}
}
if (BroadcastEn && ANT != null)
{
if (!ENG)
{
sb.Append("Включено вещание. Противники будут видеть вас на дистанции");
sb.AppendLine();
sb.Append("вещания.");
sb.AppendLine();
if (!AllInf)
{
sb.Append("Кроме того, союзникам нужны камеры для подтверждения");
sb.AppendLine();
sb.Append("дислокации передаваемой цели");
sb.AppendLine();
}
}
else
{
sb.Append("Broadcast enabled. Enemies will see you on distance");
sb.AppendLine();
sb.Append("of broadcast.");
sb.AppendLine();
if (!AllInf)
{
sb.Append("By the way, allies must have cameras for accept");
sb.AppendLine();
sb.Append("transmited target dislocation");
sb.AppendLine();
}
}
}
if (ControlTurr && !OwnDamage && TCameras.Count <= ControlledTurrets.Count * 5)
{
if (!ENG)
{
sb.Append("Не хватает камер для текущего количества турелей.");
sb.AppendLine();
sb.Append("В связи с настройками турели не будет стрелять там");
sb.AppendLine();
sb.Append("где она не получила разрешение от камер");
sb.AppendLine();
}
else
{
sb.Append("Not enough cameras for such amount of turrets.");
sb.AppendLine();
sb.Append("In case of settings turrets will be not fire if");
sb.AppendLine();
sb.Append("they haven't confirm from cameras");
sb.AppendLine();
}
}
LCD3.WriteText(sb.ToString());
LCD3.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}
if (!Targets.Contains(TargetedEnt) && !TargetedEnt.IsEmpty())
{
TargetedEnt = new MyDetectedEntityInfo();
}
if (ATTargets.Count > 0)
{
foreach(MyDetectedEntityInfo target in Targets)
{
for (int i = ATTargets.Count - 1; i >= 0; i--)
{
MyDetectedEntityInfo target1 = ATTargets[i];
if (target.EntityId == target1.EntityId)
{
ATTargets.RemoveAt(i);
ATTargets.Insert(i, target);
}
}
}
}
if (LCD4 != null)
{
sb.Clear();
}
if (RTurrets.Count > 0)
{
foreach (IMyLargeTurretBase turret in RTurrets)
{
if (RTurrets.Count > 1)
{
foreach (IMyLargeTurretBase turret1 in RTurrets)
{
turret1.CustomName = NotR_FORWARDName;
}
}
if (((turret.HasTarget && turret.GetTargetedEntity().Equals(TargetedEnt) && !TargetedEnt.IsEmpty()) || turret.IsAimed) && MissileType != 1)
{
turret.CustomName = R_FORWARDName;
if (!ENG)
{
sb.Append("РАКЕТЫ ИМЕЮТ НАВЕДЕНИЕ");
sb.AppendLine();
}
break;
}
if (turret.HasTarget && turret.GetTargetedEntity().Equals(TargetedEnt) && !TargetedEnt.IsEmpty() && MissileType == 1 && RPB != null)
{
if (!ENG)
{
sb.Append("РАКЕТЫ ИМЕЮТ НАВЕДЕНИЕ");
sb.AppendLine();
}
break;
}
}
}
if (IRTurrets.Count > 0)
{
foreach (IMyLargeTurretBase turret in IRTurrets)
{
turret.CustomName = NotR_FORWARDName;
if (!TargetedEnt.IsEmpty())
{
if (turret.HasTarget && turret.GetTargetedEntity().EntityId == TargetedEnt.EntityId)
{
turret.CustomName = IR_FORWARDName;
if (!ENG)
{
sb.Append("ИК-ТУРЕЛИ ВИДЯТ ЦЕЛЬ");
sb.AppendLine();
}
else
{
sb.Append("IR-TURRETS HAVE GUIDANCE");
sb.AppendLine();
}
}
else
{
turret.ResetTargetingToDefault();
}
}
}
}
if (!ENG)
{
sb.Append(IsMissiles(MissileType));
sb.AppendLine();
}
if (ENG)
{
if (RTurrets.Count > 0)
{
foreach (IMyLargeTurretBase turret in RTurrets)
{
if (RTurrets.Count > 1)
{
turret.CustomName = NotR_FORWARDName;
}
if ((turret.HasTarget && turret.GetTargetedEntity().Equals(TargetedEnt) && !TargetedEnt.IsEmpty()) || turret.IsAimed)
{
turret.CustomName = R_FORWARDName;
if (MissileType != 1)
{
sb.Append("TURRET-GUIDED MISSILES HAVE GUIDANCE");
sb.AppendLine();
}
break;
}
if (turret.HasTarget && turret.GetTargetedEntity().Equals(TargetedEnt) && !TargetedEnt.IsEmpty() && MissileType == 1 && RPB != null)
{
sb.Append("TURRET-GUIDED MISSILES HAVE GUIDANCE");
sb.AppendLine();
break;
}
}
}
sb.Append(IsMissiles(MissileType));
sb.AppendLine();
}
if (LCD4 != null) { LCD4.WriteText(sb.ToString()); LCD4.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE; };

if (LLCD != null)
{
sb.Clear();
if (!TargetedEnt.IsEmpty())
{
sb.Append("GPS:" + RLSName + " #111:" + TargetedEnt.Position.X + ":" + TargetedEnt.Position.Y + ":" + TargetedEnt.Position.Z + ":");
sb.AppendLine();
}
LLCD.WriteText(sb.ToString());
LLCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}
if ((RPB != null || WPB != null || EPB != null || RT != null || WT != null || ET != null) && RTurrets.Count > 0)
{
if (!TargetedEnt.IsEmpty())
{
foreach (IMyLargeTurretBase turret in RTurrets)
{
turret.SetTarget(TargetedEnt.Position + TargetedEnt.Velocity);
}
}
else
{
foreach (IMyLargeTurretBase turret in RTurrets)
{
turret.ResetTargetingToDefault();
}
}
}
if (StatusEnabled)
{
double TanksFull = 0;
double BatsCharge = 0;
if (Tanks.Count <= 0) TanksFull = 1000;
if (Bats.Count <= 0) BatsCharge = 1000;
if (Tanks.Count > 0)
{
foreach (IMyGasTank tank in Tanks)
{
TanksFull += tank.FilledRatio * 100 / Tanks.Count;
}
foreach (IMyGasTank tank in Tanks)
{
if (TanksFull < EmergencyTanks) tank.Stockpile = true;
if (TanksFull > EmergencyTanks) tank.Stockpile = false;
}
}
if (Bats.Count > 0)
{
foreach (IMyBatteryBlock bat in Bats)
{
BatsCharge += bat.CurrentStoredPower/ bat.MaxStoredPower * 100 / Bats.Count;
}
foreach (IMyBatteryBlock bat in Bats)
{
if (BatsCharge < EmergencyCharge) bat.ChargeMode = ChargeMode.Recharge;
if (BatsCharge > EmergencyCharge) bat.ChargeMode = ChargeMode.Auto;
}

}
if (EmergencyOff)
{
foreach (IMyProgrammableBlock pb in DPBs)
{
if (BatsCharge < EmergencyCharge || TanksFull < EmergencyTanks)
{
pb.Enabled = false;
}
}
}
if (LCD5 != null && StatusEnabled)
{
sb.Clear();
if (!ENG)
{
sb.Append("Статус готовности ракет:");
sb.AppendLine();
if (Bats.Count > 0) sb.Append("срзнч Заряда Батарей: " + BatsCharge);
sb.AppendLine();
if (Tanks.Count > 0) sb.Append("срзнч Заполненности баков: " + TanksFull);
sb.AppendLine();
if (EmergencyOff)
{
foreach (IMyProgrammableBlock pb in DPBs)
{
if (!pb.Enabled) { sb.Append("Ракеты не готовы к пуску"); break; }
if (pb.Enabled) { sb.Append("Ракеты готовы к пуску"); break; }
}
}
else
{
sb.Append("Ракеты готовы к пуску");
}
}
if (ENG)
{
sb.Append("Missile Status:");
sb.AppendLine();
if (Bats.Count > 0) sb.Append("Bat Charge%: " + BatsCharge);
sb.AppendLine();
if (Tanks.Count > 0) sb.Append("Tanks Fuel%: " + TanksFull);
sb.AppendLine();
if (EmergencyOff)
{
foreach (IMyProgrammableBlock pb in DPBs)
{
if (!pb.Enabled) { sb.Append("Missiles not ready"); break; }
if (pb.Enabled) { sb.Append("Missiles ready"); break; }
}
}
else
{
sb.Append("Missiles ready");
}
}
LCD5.WriteText(sb.ToString());
LCD5.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}
}
if (BroadcastEn && !HasAntenna)
{
TargetsBroad.Clear();
targettimes.Clear();
}
if (BroadcastEn && HasAntenna)
{
//
for (int i = targettimes.Count - 1; i >= 0; i--)
{
string targed = targettimes[i];
string[] targ = targed.Split('b');
long id1;
double rate;
long.TryParse(targ[1], out id1);
double.TryParse(targ[0], out rate);
rate--;
if (rate > 0)
{
targettimes[i] = (rate.ToString() + "b" + id1.ToString() + "b");
}
if (rate <= 0)
{
foreach (MyDetectedEntityInfo target in TargetsBroad)
{
if (target.EntityId == id1)
{
TargetsBroad.Remove(target);
targettimes.Remove(targed);
LTargets.Add(target);
break;
}
}
}
}
//
TrTargets.Clear();
if (listener != null)
{
if (TargetsBroad.Count > 0)
{
for (int i = Targets.Count - 1; i >= 0; i--)
{
MyDetectedEntityInfo target = Targets[i];
foreach (MyDetectedEntityInfo target1 in TargetsBroad)
{
if (target.EntityId == target1.EntityId) { Targets.Remove(target); }
}
}
}
if (Targets.Count > 0)
{
foreach (MyDetectedEntityInfo target in Targets)
{
if (!AllInf)
{
IGC.SendBroadcastMessage(BTAG, target.Position + ((Vector3D)target.Velocity / 5) + "notinf", (TransmissionDistance)BroadcastRange);
}
else
{
string targetpos = target.Position.ToString() + "b";
string targetvel = target.Velocity.ToString() + "b";
string targetrelate = target.Relationship.ToString() + "b";
string targetsize = target.BoundingBox.Max.ToString() + "b";
string targetsize1 = target.BoundingBox.Min.ToString() + "b";
string targetname = target.Name.ToString() + "b";
string targetid = target.EntityId.ToString() + "b";
string targettype = target.Type.ToString() + "b";
string targets = targetpos + targetvel + targetrelate + targetsize + targetname + targetid + targettype + targetsize1;
IGC.SendBroadcastMessage(BTAG, targets, (TransmissionDistance)BroadcastRange);
}
}
}
if (RemoveRate <= 0)
{
TargetsBroad.Clear();
}
while (listener.HasPendingMessage)
{
if (args.ToString() == BTAG)
{
object TrTarget = listener.AcceptMessage().Data;
if (TrTarget != null)
{
string[] targetinfo = null;
if (!TrTarget.ToString().EndsWith("notinf"))
{
char b = 'b';
targetinfo = TrTarget.ToString().Split(b);
if (targetinfo != null)
{
Vector3D position;
Vector3D.TryParse(targetinfo[0], out position);
Vector3D veloc;
Vector3D.TryParse(targetinfo[1], out veloc);
string name = targetinfo[4];
long targetid;
long.TryParse(targetinfo[5], out targetid);
MyDetectedEntityType targettype;
MyDetectedEntityType.TryParse(targetinfo[6], out targettype);
Vector3D max;
Vector3D.TryParse(targetinfo[3], out max);
Vector3D min;
Vector3D.TryParse(targetinfo[7], out min);
MyDetectedEntityInfo target1 = new MyDetectedEntityInfo();
if (targetid != 0) { target1 = new MyDetectedEntityInfo(targetid, name, targettype, position, new MatrixD(), veloc, MyRelationsBetweenPlayerAndBlock.NoOwnership, new BoundingBoxD(min, max), new long()); }
bool exist2 = false;
bool exist = false;
foreach (MyDetectedEntityInfo target in TargetsBroad)
{
if (target.EntityId == targetid)
{
exist2 = true;
break;
}
}
foreach (MyDetectedEntityInfo target in Targets)
{
if (target.EntityId == targetid)
{
exist = true;
break;
}
}
if (targetid != 0)
{
if (!exist2 && !exist) TargetsBroad.Add(target1);
if (exist2 && !exist)
{
foreach (MyDetectedEntityInfo targetd in TargetsBroad)
{
if (targetd.EntityId == targetid)
{
TargetsBroad.Remove(targetd);
break;
}
}
TargetsBroad.Add(target1);
}
bool exist3 = false;
foreach (string target in targettimes)
{
string[] targ = target.Split('b');
long id1;
long.TryParse(targ[1], out id1);
if (id1 == targetid)
{
exist3 = true;
break;
}
}
if (RemoveRate > 0 && !exist2 && !exist3 && !exist)
targettimes.Add(RemoveRate.ToString() + "b" + targetid.ToString() + "b");
if (RemoveRate > 0 && exist2 && exist3 && !exist)
{
foreach (string targed in targettimes)
{
string[] targ = targed.Split('b');
long id1;
long.TryParse(targ[1], out id1);
if (targetid == id1)
{
targettimes.Remove(targed);
targettimes.Add(RemoveRate.ToString() + "b" + targetid.ToString() + "b");
break;
}
}
}
}

}
}
else
{
TrTarget = TrTarget.ToString().Remove(TrTarget.ToString().Length - 6, 6);
bool exist = false;
Vector3D TargetsCoord;
Vector3D.TryParse(TrTarget.ToString(), out TargetsCoord);
if (Targets.Count > 0)
{
foreach (MyDetectedEntityInfo target in Targets)
{
if (target.Position == TargetsCoord)
{
exist = true;
break;
}
}
}
if (TargetsCoord != new Vector3D() && TargetsCoord != null && !exist) TrTargets.Add(TargetsCoord);
}
}
}
}
foreach (MyDetectedEntityInfo n in TargetsBroad)
{
if (Targets.Count <= 0) { Targets.Add(n); AddAuto(n); }

if (Targets.Count > 0)
{
bool exist = false;
foreach (MyDetectedEntityInfo v in Targets)
{
if (n.EntityId == v.EntityId)
{
exist = true;
break;
}
}
if (!exist)
{
Targets.Add(n);
AddAuto(n);
}
}
}
}

}
foreach (IMyRemoteControl RC in RCs)
{
if (RC.IsSameConstructAs(Me))
{
if (LCDSight != null)
{
sb.Clear();
if (!TargetedEnt.IsEmpty())
{
LCDSight.FontSize = (float)3.5;
string S1 = "----------------------------------";
string S2 = "----------------*-----------------";
string S3 = "----------------------------------";
double NeedU = 0;
double NeedL = 0;
Vector3D Direction = TargetedEnt.Position - RC.CubeGrid.GetPosition();
NeedU = Vector3D.Dot(RC.WorldMatrix.Up, Direction);
NeedL = Vector3D.Dot(RC.WorldMatrix.Left, Direction);
if (NeedU > 10)
{
S1 = S1.Remove(17, 1);
S1 = S1.Insert(17, "^");
}
if (NeedU < 10)
{
S3 = S3.Remove(17, 1);
S3 = S3.Insert(17, "v");
}
if (NeedL > 10)
{
S2 = S2.Remove(16, 1);
S2 = S2.Insert(16, "<");
}
if (NeedL < 10)
{
S2 = S2.Remove(18, 1);
S2 = S2.Insert(18, ">");
}
sb.Append(S1);
sb.AppendLine();
sb.Append(S2);
sb.AppendLine();
sb.Append(S3);
sb.AppendLine();
}
else
{
sb.AppendLine();
if (!ENG)
{
sb.Append("------ЦЕЛЬ НЕ ВЫБРАНА------------");
}
else
{
sb.Append("-----TARGET NOT SELECTED---------");
}
sb.AppendLine();
}
LCDSight.WriteText(sb.ToString());
LCDSight.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}

if (SAutoLaunch || LAutoLaunch)
{
if (Debug && ANT != null) ANT.HudText = times.Count.ToString();
if (times.Count > 0)
{
for (int i = times.Count - 1; i >= 0; i--)
{
string targed = times[i];
string[] targ = targed.Split('b');
long id1;
long.TryParse(targ[1], out id1);
double rate;
double.TryParse(targ[0], out rate);
rate--;
if (rate > 0)
{
times[i] = (rate.ToString() + "b" + id1.ToString() + "b");
}
if (rate == 1)
{
foreach (MyDetectedEntityInfo target in Targets)
{
if (target.EntityId == id1) { TargetedEnt = target; break; }
}
}
if (rate <= 0)
{
bool large = false;
foreach (MyDetectedEntityInfo target in Targets)
{
if (target.EntityId == id1) { TargetedEnt = target; if ((target.Type.ToString() == "LargeGrid" && !IDNotType) || (IDNotType && target.BoundingBox.Size.Length() > SizeS)) large = true; break; }
}
for (int z = MissilesCount; z > 0; z--)
{
double Distance = (Me.CubeGrid.GetPosition() - TargetedEnt.Position).Length();
if ((SAtype == 0 && !large && SAutoLaunch) || (LAtype == 0 && large && LAutoLaunch))
{
if (!UseTimersInstead && RRPB != null) RRPB.TryRun("FIRE");
if (UseTimersInstead && RRT != null) RRT.Trigger();
}
if ((SAtype == 1 && !large && SAutoLaunch && SAutoRange >= Distance) || (LAtype == 1 && large && LAutoLaunch && LAutoRange >= Distance))
{
if (!UseTimersInstead && RPB != null) RPB.TryRun("Fire");
if (UseTimersInstead && RT != null) RT.Trigger();
}
if ((SAtype == 2 && !large && SAutoLaunch && SAutoRange >= Distance) || (LAtype == 2 && large && LAutoLaunch && LAutoRange >= Distance))
{
if (!UseTimersInstead && WPB != null) WPB.TryRun("fire");
if (UseTimersInstead && WT != null) WT.Trigger();
}
if ((SAtype == 3 && !large && SAutoLaunch && SAutoRange >= Distance) || (LAtype == 3 && large && LAutoLaunch && LAutoRange >= Distance))
{
if (!UseTimersInstead && EPB != null) EPB.TryRun("MODE:" + Emode);
if (UseTimersInstead && ET != null) ET.Trigger();
}
}
if (large)
{
rate = LRate;
times[i] = ((rate + 2).ToString() + "b" + id1.ToString() + "b");
}
else
{
rate = SRate;
times[i] = ((rate + 2).ToString() + "b" + id1.ToString() + "b");
}
}
}
foreach (string targed in times)
{
string[] targ = targed.Split('b');
long id1;
long.TryParse(targ[1], out id1);
List<long> ids = new List<long>();
foreach (MyDetectedEntityInfo target in Targets)
{
ids.Add(target.EntityId);
}
if (ids.Count == Targets.Count)
{
if (!ids.Contains(id1))
{
times.Remove(targed);
break;
}
}
}
}
}
if (ControlTurr && ControlledTurrets.Count > 0)
{
foreach (IMyLargeTurretBase turret in ControlledTurrets)
{
if (TargetedEnt.IsEmpty() || !turret.IsAimed || Targets.Count <= 0)
turret.ApplyAction("Shoot_Off", null);
}
}
if (Targets.Count > 0 && !TargetedEnt.IsEmpty() && ControlTurr && ControlledTurrets.Count > 0)
{
foreach (IMyLargeTurretBase turret in ControlledTurrets)
{
double Distance = (TargetedEnt.Position - turret.GetPosition()).Length();
Vector3D vector = TargetedEnt.Position + (Vector3D)(TargetedEnt.Velocity - (Vector3I)RC.GetShipVelocities().LinearVelocity) * (Distance / ShellSpeed);
turret.SetTarget(vector);
if (turret.IsAimed)
{
Vector3D Direction = Vector3D.Normalize(TargetedEnt.Position - turret.GetPosition());
RayD sight = new RayD((Vector3D)turret.GetPosition() + Direction * Me.CubeGrid.GridSize, Direction /*vector*/);
Vector3I MIN = Me.CubeGrid.Min;
Vector3I MAX = Me.CubeGrid.Max;
BoundingBoxD BOX = new BoundingBoxD(MIN, MAX);
bool confirmed = false;
if (TCameras.Count > 0 && Distance < TRange && !OwnDamage)
{
for (int i = TCameras.Count - 1; i >= 0; i--)
{
IMyCameraBlock camera = TCameras[i];
double dist = (camera.GetPosition() - turret.GetPosition()).Length();
if (camera != null && camera.CanScan(vector) && dist < MaxCamDist)
{
MyDetectedEntityInfo info = camera.Raycast(vector);
if (info.Relationship != MyRelationsBetweenPlayerAndBlock.Owner && info.EntityId != Me.CubeGrid.EntityId && ((info.Relationship != MyRelationsBetweenPlayerAndBlock.Friends && info.Relationship != MyRelationsBetweenPlayerAndBlock.FactionShare) || FriendlyOff)) confirmed = true;
}
}
}
if (Distance > TRange)
{
foreach (IMyCameraBlock cam in TCameras)
{
if (!Cameras.Contains(cam)) Cameras.Add(cam);
}
}
else
{
foreach (IMyCameraBlock cam in TCameras)
{
if (Cameras.Contains(cam)) Cameras.Remove(cam);
}
}
if ((confirmed || OwnDamage) && Distance < TRange)
{
turret.ApplyAction("Shoot_On", null);
}
}
}
}
break;
}
}
if (DLCD != null)
{
sb.Clear();
sb.Append("BroadcastEn " + BroadcastEn.ToString());
sb.AppendLine();
sb.Append("ActiveMode " + ActiveMode.ToString());
sb.AppendLine();
sb.Append("Scan " + Scan.ToString());
sb.AppendLine();
sb.Append("MouseSelector " + MouseSelector.ToString());
sb.AppendLine();
sb.Append("SAutoLaunch " + SAutoLaunch.ToString());
sb.AppendLine();
sb.Append("LAutoLaunch " + LAutoLaunch.ToString());
sb.AppendLine();
sb.Append("FriendlyOff " + FriendlyOff.ToString());
sb.AppendLine();
DLCD.WriteText(sb.ToString());
DLCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
}
if (TPB != null)
{
if (Targets.Count > 0 && SendAllTargets)
{
foreach (MyDetectedEntityInfo target in Targets)
{
TPB.TryRun(target.Position + "b" + target.Velocity + "b" + target.BoundingBox.Min + "b" + target.BoundingBox.Max + "b" + target.EntityId + "b" + "TARGET");
}
}
if (!TargetedEnt.IsEmpty())
{
TPB.TryRun(TargetedEnt.Position + "b" + TargetedEnt.Velocity + "b" + TargetedEnt.BoundingBox.Min + "b" + TargetedEnt.BoundingBox.Max + "b" + TargetedEnt.EntityId + "b" + "TTARGET");
}

}
if (RRPB != null)
{
RRPB.TryRun(TargetedEnt.Position + "b" + TargetedEnt.Velocity + "b" + TargetedEnt.BoundingBox.Min + "b" + TargetedEnt.BoundingBox.Max + "b" + TargetedEnt.EntityId + "b" + "TARGET");
foreach (MyDetectedEntityInfo tg in Targets) { if (tg.EntityId != TargetedEnt.EntityId)RRPB.TryRun(tg.Position + "b" + tg.Velocity + "b" + tg.BoundingBox.Min + "b" + tg.BoundingBox.Max + "b" + tg.EntityId + "b" + "TTARGET"); }
}
switch (args)
{
case "MODE": ActiveMode = !ActiveMode; break;
case "SCAN": Scan = !Scan; break;
case "TARGET":
if (!MouseSelector)
{
TargetedEnt = new MyDetectedEntityInfo();
if (SelectedTarget < Targets.Count - 1)
{
SelectedTarget++;
}
else
{
SelectedTarget = 0;
}
}
; break;

case "RESET":
Targets.Clear();
Targets4.Clear();
LTargets.Clear();
TargetedEnt = new MyDetectedEntityInfo();
break;
case "SELECT":
MissileType++;
if (MissileType == 4) MissileType = 0;
break;
// battle mode
case "LAUNCH":
if (!TargetedEnt.IsEmpty() && MissileType == 0)
{
if (RRPB != null) RRPB.Enabled = true;
if (RRPB != null && !UseTimersInstead)RRPB.TryRun("FIRE");
if (RRT != null && UseTimersInstead) RRT.Trigger();
}
if (RPB != null && MissileType == 1)
{
if (RPB != null && !StatusEnabled || !RdavEn) RPB.Enabled = true;
if (RPB != null && !UseTimersInstead) RPB.TryRun("Fire");
if (UseTimersInstead && RT != null) RT.Trigger();
}
if (MissileType == 2)
{
if (WPB != null && !StatusEnabled || !WhipsEn) WPB.Enabled = true;
if (WPB != null && !UseTimersInstead) WPB.TryRun("fire");
if (UseTimersInstead && WT != null) WT.Trigger();
}
if (EPB != null && MissileType == 3)
{
if (EPB != null && !StatusEnabled || !AlysiusEn) EPB.Enabled = true;
if (EPB != null && !UseTimersInstead) EPB.TryRun("MODE:" + Emode);
if (UseTimersInstead && ET != null) ET.Trigger();
}
; break;
//
case "BROADCAST": BroadcastEn = !BroadcastEn; break;
case "SMALLGRIDA": SAutoLaunch = !SAutoLaunch; break;
case "LARGEGRIDA": LAutoLaunch = !LAutoLaunch; break;
case "FRIENDLYFIRE": FriendlyOff = !FriendlyOff; break;
case "UPDATE": update = true;break;
}
finished = false;
Checked = false;
if (TargetsBroad.Count > 0)
{
for (int i = Targets.Count - 1; i >= 0; i--)
{
MyDetectedEntityInfo target = Targets[i];
foreach (MyDetectedEntityInfo target1 in TargetsBroad)
{
if (target.EntityId == target1.EntityId) { Targets.Remove(target); }
}
}
}
}
}
sb.Clear();
usedcams.Clear();
Targets4.Clear();
}
public void AddAuto(MyDetectedEntityInfo d)
{
double Distance = (d.Position - Me.CubeGrid.GetPosition()).Length();
if (SAutoLaunch && d.BoundingBox.Size.Length() > IgnoredSize && ((d.Type.ToString() == "SmallGrid" && !IDNotType) || (IDNotType && d.BoundingBox.Size.Length() <= SizeS)))
{
string oh = SRate.ToString() + "b" + d.EntityId.ToString() + "b";
if (times.Count <= 0) times.Add(oh);
else
{
bool exist = false;
foreach (string targed in times)
{
string[] targ = targed.Split('b');
long id1;
long.TryParse(targ[1], out id1);
if (id1 == d.EntityId)
{
exist = true;
break;
}
}
if (!exist)
{
times.Add(oh);
}
}
}
if (LAutoLaunch && d.BoundingBox.Size.Length() > IgnoredSize && ((d.Type.ToString() == "LargeGrid" && !IDNotType) || (IDNotType && d.BoundingBox.Size.Length() > SizeS)))
{
string oh = LRate.ToString()+ "b" + d.EntityId.ToString() + "b";
if (times.Count <= 0) times.Add(oh);
else
{
bool exist = false;
foreach (string targed in times)
{
string[] targ = targed.Split('b');
long id1;
long.TryParse(targ[1], out id1);
if (id1 == d.EntityId)
{
exist = true;
break;
}
}
if (!exist)
{
times.Add(oh);
}
}
}
}