         /* Экспортный корабельный Компьютер Н.С.К.С || Export onboard computer from N.U.C.C, version 1.0
         * Дверь будет игнорироваться, если имеет символ @ в названии || Door will be ignored if have @ symbol in his name
         * В отличии от оригинального бортового компьютера имеет скромный функционал.
         * Для взаимодействия с компьютером требуется дисплей в кабине или снаружи. 
         * Компьютер позволяет с бОльшим умом использовать курсовое вооружение, посадить корабль без лишней траты топлива, 
         * выпустить парашюты или дать звук при подозрении на угрозу столкновения, закрыть двери при разгерметизации, заправить корабль при стыковке, 
         * записать координаты базы и вернуться к ней, закрыть двери в случае боевой тревоги, активировать таймеры или ПБ с заданной частотой, отслеживать их статус.
         */
        string point = " <";
        string cockpitname = "Кресло пилота М-58"; // Название кабины || Cockpit name
        string batname = "АКБ"; // Название аккумулятора (каждого) || battery name (every battery)
        string RCN = "УДТ"; // название ДУ || remcon name
        string LCDN = "Инфо борт. компьютера"; // название дисплея (не нужно если используется дисплей кабины) || display name (not used if dipslay in cockpit)
        string PBTag = "ЦП"; //Тег для компьютеров (начало назавния) || PB name tag (in start of name)
        string ventn = "Вентиляция М-58"; // Название каждой вентиляции || name of every vent
        string ReactorN = "Реактор корабля"; // Название реактора (каждого) || name of every reactor
        string thrustn = "Ускорители"; // Название группы ускорителей || name of thrusters group
        string GyroName = "Стаб. гироскоп"; // Название гироскопа || name of gyro 
        string connname = "Коннектор базы";  // Название коннектора || name of connector 
        string triggers = "@"; // Должен быть так же тег PBTag, справа должна быть прибавлена ещё и цифра, которая обозначает частоту вызова, 0 = одновременно с ЦП корабля. 1 = через один вызов этого скрипта!
        string TimerN = "Аварийн. таймер"; // Название таймера, который будет запускаться пока скрипт жив (сработает по его смерти)
        string gguns = "ГШ-23-6"; // Заряжаемые пулемёты, группа || gatlings for load, group
        string Cargo = "Большой контейнер"; // Название контейнера корабля || ship cargo name
        string BaseCargo = "Контейнер базы"; // Название контейнера базы || base cargo name
        string ShipName = "фрегата М-58"; 
        string RLSName = "ЦП РЛС"; // Название РЛС изумруд (ПБ) || name of RADAR emerald-1 (PB)
        string DynName = "Динамик"; // Название динамика для тревоги || alarm soundblock name
        string gungroupname = "НУРСы"; // Оружие стреляемое по очереди, группа || Name of salvo group 
        string gunseekname = "НАР Оса 1"; // Оружие отслеживаемое для стрельбы (если не нужно ничего не пишите, нужно для одновременной стрельбы салво-группы и другого оружия) || gun for check fire pilot or not, use it if you want fire salvo group with your other weapon
        //
        double Missiles = 0; // Кол-во ракет в ящике
        double Bullets = 10; // Кол-во пул. лент в ящике
        double LoadedBullets = 3; // Кол-во пул. лент в пулемёте
        double uranium = 0; // Кол-во урана в ящике
        double loadeduranium = 10; // Кол-во урана в реакторах
        double ice = 2000; // Кол-во льда в ящике
        double chutes = 10; // Кол-во парашютных куполов
        double CruiseSpeed = 100; // Скорость круиз-контроля
        double tick = 10; // Время тиков таймера эвакуации
        double SCoeff = 1.05; // Чувствительность гасителя безопасности (чем больше тем меньше)
        //
        bool CockpitLCD = false; // Дисплей в кабине? || Display in cockpit?     
        bool OwnRadar = false; // Использовать Встроенный радар (турели)? || Use Included turret radar ?
        bool battlemode = false; 
        bool AutoRefill = true; 
        bool AutoBattle = true;
        bool AutoDoor = true;
        bool Safety = true;
        bool cruise = false;
        bool Landing = false;
        bool TurnThrustOnMerge = true; // Выключать ускорители при пристыковке?
        bool ENG = false; // English translation?
        static bool Auto = true; // Автозапуск скрипта? || Auto-launch of script?

        //RU
        string mn1 = "ЦП ";
        string mn2 = " Н.С.К.С ";
        string mn3 = "<--Главное меню--------------------->";
        string mn4 = "-Настройки-";
        string mn5 = "-Статус-";
        string mn6 = "-Ручное управление-";
        string mn7 = "-Отчёт-";
        string mn8 = "<--Настройки----------------------->";
        string mn9 = "-Встроенная РЛС ";
        string mn10 = "-Авт. боевая тревога ";
        string mn11 = "-Контроль шлюза ";
        string mn12 = "-Автозаправщик ";
        string mn13 = "-Аварийный гаситель ";
        string mn14 = "-Бортовая РЛС ";
        string mn15 = "-Выход-";
        string mn16 = "<--Статус--------------------------->";
        string mn17 = "Сгоревшие ПБ: ";
        string mn18 = "Запускаемых ПБ: ";
        string mn19 = "Встроенная РЛС тревоги включена";
        string mn20 = "Боевая тревога: ";
        string mn21 = "<--Ручное управление------------------------>";
        string mn22 = "-Отключить заправку, отсоедениться-";
        string mn23 = "-Выключить перехват гироскопов-";
        string mn24 = "-Посадить корабль-";
        string mn25 = "-Возврат домой-";
        string mn26 = "-Круиз-контроль-";
        string mn27 = "-Отменить все задачи-";
        string mn28 = "<--Отчёт----------------------------------->";
        //
        //ENG
        string emn1 = "PB ";
        string emn2 = " N.U.C.C ";
        string emn3 = "<--Main menu--------------------->";
        string emn4 = "-Settings-";
        string emn5 = "-Status-";
        string emn6 = "-Main control-";
        string emn7 = "-Report-";
        string emn8 = "<--Settings----------------------->";
        string emn9 = "-Included RADAR ";
        string emn10 = "-Auto battle alarm ";
        string emn11 = "-Pressure control ";
        string emn12 = "-Auto refill ";
        string emn13 = "-Emergency brake ";
        string emn14 = "-Included RADAR ";
        string emn15 = "-Exit-";
        string emn16 = "<--Status--------------------------->";
        string emn17 = "Destroyed PB: ";
        string emn18 = "Launching PB: ";
        string emn19 = "Included RADAR of alarm is offline";
        string emn20 = "Battle alarm: ";
        string emn21 = "<--Main control------------------------>";
        string emn22 = "-Stop refill, disconnect-";
        string emn23 = "-Turn off gyroscoped percentage-";
        string emn24 = "-Land ship-";
        string emn25 = "-Back to home-";
        string emn26 = "-Cruise control-";
        string emn27 = "-Cancel all missions-";
        string emn28 = "<--Report----------------------------------->";
        //
        //
        /******************************************/
        public bool IsBat(IMyTerminalBlock block)
        {
            IMyBatteryBlock bat = block as IMyBatteryBlock;
            if (bat != null || !bat.IsSameConstructAs(Me)) return true;
            return false;
        }

        public bool IsVent(IMyTerminalBlock block)
        {
            IMyAirVent vent = block as IMyAirVent;
            if (vent != null) return true;
            return false;
        }
        public bool IsPB(IMyTerminalBlock block)
        {
            IMyProgrammableBlock pb = block as IMyProgrammableBlock;
            if (pb != null || !pb.IsSameConstructAs(Me)) return true;
            return false;
        }
        public bool IsReact(IMyTerminalBlock block)
        {
            IMyReactor reactor = block as IMyReactor;
            if (reactor != null || !reactor.IsSameConstructAs(Me)) return true;
            return false;
        }
        public string IsEnabled(bool woah)
        {
            if (woah && !ENG) return "ВКЛ";
            if (woah && ENG) return "ON";
            if (!ENG) return "ВЫКЛ";
            else return "OFF";
        }
        public string IsLoad(string load)
        {
            if (load == "|") return "/";
            if (load == "/") return "_";
            if (load == "_") return "<";
            if (load == "<") return "|";
            return "";
        }
        public string IsWork(IMyTerminalBlock block)
        {
            if (block != null && block.IsWorking && !ENG) return "ВКЛ";
            if (block != null && block.IsWorking && ENG) return "ON";
            if (!ENG) return "ВЫКЛ";
            else return "OFF";
        }
        /**/
        int number = 0;
        IMyTextPanel LCD;
        IMyRemoteControl RC;
        IMyCockpit cockpit;
        IMyShipConnector baseconn;
        IMyTimerBlock ExtraTimer; /* Таймер эваукации */
        IMyBlockGroup gatlings;
        IMyBlockGroup cannonsg;
        IMyTerminalBlock seeker;
        IMyCargoContainer cargo;
        IMyProgrammableBlock RLS;
        IMySoundBlock dynam;
        /**/
        List<IMyTimerBlock> timers = new List<IMyTimerBlock>();
        List<IMySmallGatlingGun> guns = new List<IMySmallGatlingGun>();
        List<IMyThrust> thrusts = new List<IMyThrust>();
        List<IMyTerminalBlock> reactors = new List<IMyTerminalBlock>(); /* Пока нет интерфейса для водородных двигателей здесь будут только реакторы.  */
        List<IMyTerminalBlock> bats = new List<IMyTerminalBlock>();
        List<IMyGyro> gyros = new List<IMyGyro>();
        List<IMyTerminalBlock> PB = new List<IMyTerminalBlock>();
        List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
        List<IMyParachute> parachutes = new List<IMyParachute>();
        List<IMyProductionBlock> produce = new List<IMyProductionBlock>();
        List<IMyTerminalBlock> doors = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> vents = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> cannons = new List<IMyTerminalBlock>();
        List<string> stimers = new List<string>();
        List<string> spb = new List<string>();
        /**/
        int Menu = 0;
        int StrinG = 0;
        double DistToEarth = 0;
        bool ToHome = false;
        bool Alarm = false;
        /**/
        StringBuilder sb = new StringBuilder();
        string load = "|";
        Program()
        {
            if (ENG)
            {
                mn1 = emn1;
                mn2 = emn2; 
                mn3 = emn3; 
                mn4 = emn4; 
                mn5 = emn5; 
                mn6 = emn6; 
                mn7 = emn7; 
                mn8 = emn8; 
                mn9 = emn9; 
                mn10 = emn10;
                mn11 = emn11;
                mn12 = emn12;
                mn13 = emn13;
                mn14 = emn14;
                mn15 = emn15;
                mn16 = emn16;
                mn17 = emn17;
                mn18 = emn18;
                mn19 = emn19;
                mn20 = emn20;
                mn21 = emn21;
                mn22 = emn22;
                mn23 = emn23;
                mn24 = emn24;
                mn25 = emn25; 
                mn26 = emn26;
                mn27 = emn27; 
                mn28 = emn28; 
            }
            if (Auto) Runtime.UpdateFrequency |= UpdateFrequency.Update10;
        }
        void Main(String args)
        {
            load = IsLoad(load);
            IMyBlockGroup thrustg;
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(produce);
            IMyShipConnector conn = GridTerminalSystem.GetBlockWithName(connname) as IMyShipConnector;
            if (conn != null) baseconn = conn;
            conn = null;
            dynam = GridTerminalSystem.GetBlockWithName(DynName) as IMySoundBlock;
            RLS = GridTerminalSystem.GetBlockWithName(RLSName) as IMyProgrammableBlock;
            GridTerminalSystem.SearchBlocksOfName(ventn, vents, IsVent);
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);
            if (doors.Count > 0)
            {
                for (int i = doors.Count - 1; i >= 0; i--)
                {
                    IMyTerminalBlock door = doors[i];
                    if (door.CustomName.Contains("@")) doors.Remove(door);
                }
            }
            GridTerminalSystem.SearchBlocksOfName(batname, bats, IsBat);
            GridTerminalSystem.GetBlocksOfType(turrets);
            cannonsg = GridTerminalSystem.GetBlockGroupWithName(gungroupname);
            if (cannonsg != null) cannonsg.GetBlocks(cannons);
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(blocks);
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block.CustomName.Contains(PBTag)) PB.Add(block);
            }
            blocks.Clear();
            GridTerminalSystem.GetBlocksOfType(parachutes);
            gatlings = GridTerminalSystem.GetBlockGroupWithName(gguns);
            if (gatlings != null) gatlings.GetBlocksOfType(guns);
            GridTerminalSystem.SearchBlocksOfName(ReactorN, reactors, IsReact);
            thrustg = GridTerminalSystem.GetBlockGroupWithName(thrustn);
            if (thrustg != null) thrustg.GetBlocksOfType(thrusts);
            thrustg = null;
            GridTerminalSystem.GetBlocksOfType(gyros);
            GridTerminalSystem.GetBlocksOfType(timers);
            cargo = GridTerminalSystem.GetBlockWithName(Cargo) as IMyCargoContainer;
            cockpit = GridTerminalSystem.GetBlockWithName(cockpitname) as IMyCockpit;
            RC = GridTerminalSystem.GetBlockWithName(RCN) as IMyRemoteControl;
            LCD = GridTerminalSystem.GetBlockWithName(LCDN) as IMyTextPanel;
            ExtraTimer = GridTerminalSystem.GetBlockWithName(TimerN) as IMyTimerBlock;
            if (ExtraTimer != null) { ExtraTimer.TriggerDelay = (float)tick; ExtraTimer.StartCountdown(); }
            if (cannons.Count > 0)
            {
                if (gunseekname != "") seeker = GridTerminalSystem.GetBlockWithName(gunseekname);
                foreach (IMyTerminalBlock cannon in cannons)
                {
                    if (cannon.CustomName != gunseekname) cannon.ApplyAction("OnOff_Off");
                }
                if (seeker != null)
                {
                    IMySmallMissileLauncher mgun = seeker as IMySmallMissileLauncher;
                    IMySmallGatlingGun ggun = seeker as IMySmallGatlingGun;
                    if ((mgun != null && mgun.IsShooting) || (ggun != null && ggun.IsShooting))
                    {
                        number--;
                        if (number > cannons.Count - 1) number = 0;
                        if (number < 0) number = cannons.Count - 1;
                        for (; number >= 0;)
                        {
                            if (!cannons[number].IsFunctional) continue;
                            cannons[number].ApplyAction("OnOff_On");
                            cannons[number].ApplyAction("ShootOnce");
                            break;
                        }
                    }
                }
                else
                {
                    IMySmallMissileLauncher mgun = cannons[number] as IMySmallMissileLauncher;
                    IMySmallGatlingGun ggun = cannons[number] as IMySmallGatlingGun;
                    if (mgun != null || ggun != null)
                    {
                        number--;
                        if (number > cannons.Count - 1) number = 0;
                        if (number < 0) number = cannons.Count - 1;
                        if ((mgun != null && mgun.IsShooting) || (ggun != null && ggun.IsShooting))
                            for (; number >= 0;)
                            {
                                if (!cannons[number].IsFunctional) continue;
                                cannons[number].ApplyAction("ShootOnce");
                                break;
                            }
                        cannons[number].ApplyAction("OnOff_On");
                    }
                }
            }
            foreach (IMyLargeTurretBase turret in turrets)
            {
                turret.Enabled = true;
                if (OwnRadar)
                {
                    MyDetectedEntityInfo target = turret.GetTargetedEntity();
                    if (!target.IsEmpty())
                    {
                        Alarm = true;
                    }
                }
            }
            Vector3D grav = cockpit.GetNaturalGravity();
            double elev;
            cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out elev);
            foreach (IMyGyro gyro in gyros)
            {
                if (gyro.CustomName == GyroName)
                {
                    gyro.GyroOverride = false;
                }
            }
            
            if (Landing || Safety)
            {
                if (Landing) { cruise = false;}
                if (elev > 40 && (DistToEarth / SCoeff > elev || Landing))
                {
                    if (Landing)
                    {
                        foreach (IMyGyro gyro in gyros)
                        {
                            if (gyro.CustomName == GyroName)
                            {
                                gyro.GyroOverride = true;
                                Vector3D vector = cockpit.GetNaturalGravity();
                                float gForward = (float)vector.Dot(gyro.WorldMatrix.Forward);
                                float gRight = (float)vector.Dot(gyro.WorldMatrix.Left);
                                float gUp = (float)vector.Dot(gyro.WorldMatrix.Up);
                                gyro.Pitch = (float)Math.Atan2(gForward, gUp);
                                gyro.Roll = (float)Math.Atan2(-gRight, gUp);
                            }
                        }
                    }
                    double Power = 0;
                    foreach (IMyThrust thrust in thrusts)
                    {
                        if (thrust.WorldMatrix.Backward == cockpit.WorldMatrix.Up)
                        {
                            Power += thrust.MaxEffectiveThrust;
                            if (Landing) thrust.Enabled = false;
                        }
                    }
                    double spd = cockpit.GetShipSpeed();
                    if (spd < 10) foreach (IMyParachute para in parachutes) { if (para.OpenRatio == 1) para.CloseDoor(); }
                    if (spd > 0)
                    {
                        double mass = cockpit.CalculateShipMass().PhysicalMass;
                        double ac = Power / mass;
                        if (elev / spd <= spd / ac)
                        {
                            cockpit.DampenersOverride = true;
                            Landing = false;
                            foreach (IMyThrust thrust in thrusts) { thrust.Enabled = true; if (Safety) { if (dynam != null) dynam.Play(); if (elev / spd <= spd / ac / 1.2) foreach (IMyParachute para in parachutes) { if (para.OpenRatio == 0) para.OpenDoor(); } } }
                        }
                    }
                }
                else
                {
                    Landing = false;
                    foreach (IMyThrust thrust in thrusts) { thrust.Enabled = true; }
                }
                cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out DistToEarth);
            }
            foreach (IMyThrust thrust in thrusts)
            {
                if (cruise)
                {
                    if (thrust.WorldMatrix.Backward == cockpit.WorldMatrix.Forward && cockpit.GetShipSpeed() < CruiseSpeed) thrust.ThrustOverridePercentage = 1;
                    else thrust.ThrustOverridePercentage = 0;
                    if (thrust.WorldMatrix.Forward == cockpit.WorldMatrix.Forward && cockpit.GetShipSpeed() < CruiseSpeed) thrust.Enabled = false;
                }
                else
                {
                    if (cockpit.GetShipSpeed() > 0 && !Landing) thrust.Enabled = true;
                    thrust.ThrustOverridePercentage = 0;
                }
            }
            if (baseconn != null && ToHome && RC != null)
            {
                cruise = false;
                Landing = false;
                RC.ClearWaypoints();
                string gps0 = "GPS:" + "BASE" + " #1:" + baseconn.GetPosition().X + ":" + baseconn.GetPosition().Y + ":" + baseconn.GetPosition().Z + ":";
                MyWaypointInfo gps;
                MyWaypointInfo.TryParse(gps0, out gps);
                RC.AddWaypoint(gps);
                RC.SetAutoPilotEnabled(true);
                RC.SetCollisionAvoidance(true);
                RC.FlightMode = FlightMode.OneWay;
                if ((RC.GetPosition() - baseconn.GetPosition()).Length() < 100)
                {
                    RC.SetAutoPilotEnabled(false);
                }
            }
            else
            {
                if (RC != null)
                {
                    RC.SetAutoPilotEnabled(false);
                }
            }
            foreach (IMyTimerBlock timer in timers)
            {
                if (timer.CustomName.Contains(triggers))
                {
                    bool woh = false;
                    foreach (string str in stimers)
                    {
                        string[] spl = str.Split('b');
                        if (long.Parse(spl[1]) == timer.EntityId)
                        {
                            int count = int.Parse(spl[0]);
                            count--;
                            if (count <= 0) { timer.Trigger(); stimers.Remove(str); break; }
                            stimers.Remove(str);
                            stimers.Add(count + "b" + spl[1]);
                            woh = true;
                            break;
                        }
                    }
                    if (woh) continue;
                    string[] wow = timer.CustomName.Split(triggers.ToCharArray());
                    stimers.Add(wow[1] + "b" + timer.EntityId);
                }
            }
            foreach (IMyProgrammableBlock pb in PB)
            {
                if (pb.CustomName.Contains(triggers))
                {
                    bool woh = false;
                    foreach (string str in spb)
                    {
                        string[] spl = str.Split('b');
                        if (long.Parse(spl[1]) == pb.EntityId)
                        {
                            int count = int.Parse(spl[0]);
                            count--;
                            if (count <= 0) { pb.TryRun(""); spb.Remove(str); break; }
                            spb.Remove(str);
                            spb.Add(count + "b" + spl[1]);
                            woh = true;
                            break;
                        }
                    }
                    if (woh) continue;
                    string[] wow = pb.CustomName.Split(triggers.ToCharArray());
                    spb.Add(wow[1] + "b" + pb.EntityId);
                }
            }
            bool bulletsf = false;
            bool missilef = false;
            bool batf = true;
            bool icef = false;
            bool reactorf = false;
            bool chutef = false;
            if (AutoRefill && baseconn != null && baseconn.Status == MyShipConnectorStatus.Connected && baseconn.OtherConnector.IsSameConstructAs(cockpit))
            {
                if (TurnThrustOnMerge)
                    foreach (IMyThrust thrust in thrusts)
                    {
                        thrust.Enabled = false;
                    }
                IMyCargoContainer basecargo = GridTerminalSystem.GetBlockWithName(BaseCargo) as IMyCargoContainer;
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                if (basecargo != null)
                {
                    foreach (IMyReactor reactor in reactors)
                    {
                        reactor.UseConveyorSystem = false;
                        items.Clear();
                        reactor.GetInventory().GetItems(items);
                        foreach (MyInventoryItem item in items)
                        {
                            if (item.Type.SubtypeId.Equals("Uranium"))
                            {
                                if (item.Amount > (MyFixedPoint)loadeduranium) basecargo.GetInventory().TransferItemTo(reactor.GetInventory(), item, (VRage.MyFixedPoint)(item.Amount - (MyFixedPoint)loadeduranium));
                                if (item.Amount <= (MyFixedPoint)loadeduranium) basecargo.GetInventory().TransferItemTo(reactor.GetInventory(), item, (VRage.MyFixedPoint)((MyFixedPoint)loadeduranium - item.Amount));
                            }
                        }
                    }
                    foreach (IMyBatteryBlock bat in bats)
                    {
                        bat.ChargeMode = ChargeMode.Recharge;
                        if (bat.CurrentStoredPower < bat.MaxStoredPower) batf = false;
                    }
                    foreach (IMySmallGatlingGun gun in guns)
                    {
                        double COUNT = 0;
                        gun.Enabled = false;
                        items.Clear();
                        gun.GetInventory().GetItems(items);
                        foreach (MyInventoryItem item in items)
                        {
                            if (item.Type.SubtypeId.Equals("NATO_25x184mm")) COUNT = (double)item.Amount;
                        }
                        items.Clear();
                        basecargo.GetInventory().GetItems(items);
                        foreach (MyInventoryItem item in items)
                        {
                            if (item.Type.SubtypeId.Equals("NATO_25x184mm") && COUNT < LoadedBullets)
                            {
                                if (COUNT > LoadedBullets) gun.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(COUNT - LoadedBullets));
                                if (COUNT <= LoadedBullets) basecargo.GetInventory().TransferItemTo(gun.GetInventory(), item, (VRage.MyFixedPoint)(LoadedBullets - COUNT));
                            }
                        }
                        items.Clear();
                    }
                    if (cargo != null)
                    {
                        double countammo = 0;
                        double countmissile = 0;
                        double counturanium = 0;
                        double icecount = 0;
                        double parachutecount = 0;
                        cargo.GetInventory().GetItems(items);
                        foreach (MyInventoryItem item in items)
                        {
                            if (item.Type.SubtypeId.Equals("NATO_25x184mm"))
                            {
                                countammo = (double)item.Amount;
                            }
                            if (item.Type.SubtypeId.Equals("Missile200mm"))
                            {
                                countmissile = (double)item.Amount;
                            }
                            if (item.Type.SubtypeId.Equals("Uranium"))
                            {
                                counturanium = (double)item.Amount;
                            }
                            if (item.Type.SubtypeId.Equals("Ice"))
                            {
                                icecount = (double)item.Amount;
                            }
                            if (item.Type.SubtypeId.Contains("Canvas"))
                            {
                                parachutecount = (double)item.Amount;
                            }
                        }
                        foreach (IMyReactor reactor in reactors)
                        {
                            double reactoramount = 0;
                            items.Clear();
                            reactor.GetInventory().GetItems(items);
                            foreach (MyInventoryItem item in items)
                            {
                                if (item.Type.SubtypeId.Contains("Uranium"))
                                {
                                    reactoramount = (double)item.Amount;
                                }
                            }
                            items.Clear();
                            basecargo.GetInventory().GetItems(items);
                            foreach (MyInventoryItem item in items)
                            {
                                if (item.Type.SubtypeId.Contains("Uranium"))
                                {
                                    if (reactoramount > uranium) reactor.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(reactoramount - uranium));
                                    if (reactoramount < uranium) basecargo.GetInventory().TransferItemTo(reactor.GetInventory(), item, (VRage.MyFixedPoint)(uranium - reactoramount));
                                    if (reactoramount >= uranium) reactorf = true;
                                    else reactorf = false;
                                }
                            }
                        }
                        items.Clear();
                        basecargo.GetInventory().GetItems(items);
                        foreach (MyInventoryItem item in items)
                        {
                            if (item.Type.SubtypeId.Equals("NATO_25x184mm"))
                            {
                                if (countammo > Bullets) cargo.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(countammo - Bullets));
                                if (countammo < Bullets) basecargo.GetInventory().TransferItemTo(cargo.GetInventory(), item, (VRage.MyFixedPoint)(Bullets - countammo));
                                if (countammo >= Bullets) bulletsf = true;
                            }
                            if (item.Type.SubtypeId.Equals("Missile200mm"))
                            {
                                if (countmissile > Missiles) cargo.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(countmissile - Missiles));
                                if (countmissile < Missiles) basecargo.GetInventory().TransferItemTo(cargo.GetInventory(), item, (VRage.MyFixedPoint)(Missiles - countmissile));
                                if (countmissile >= Missiles) missilef = true;
                            }
                            if (item.Type.SubtypeId.Contains("Uranium"))
                            {
                                if (counturanium > uranium) cargo.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(counturanium - uranium));
                                if (counturanium < uranium) basecargo.GetInventory().TransferItemTo(cargo.GetInventory(), item, (VRage.MyFixedPoint)(uranium - counturanium));
                                if (counturanium != uranium) reactorf = false;
                            }
                            if (item.Type.SubtypeId.Equals("Ice"))
                            {
                                if (icecount > ice) cargo.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(icecount - ice));
                                if (icecount < ice) basecargo.GetInventory().TransferItemTo(cargo.GetInventory(), item, (VRage.MyFixedPoint)(ice - icecount));
                                if (icecount >= ice) icef = true;
                            }
                            if (item.Type.SubtypeId.Contains("Canvas"))
                            {
                                if (parachutecount > chutes) cargo.GetInventory().TransferItemTo(basecargo.GetInventory(), item, (VRage.MyFixedPoint)(parachutecount - chutes));
                                if (parachutecount < chutes) basecargo.GetInventory().TransferItemTo(cargo.GetInventory(), item, (VRage.MyFixedPoint)(chutes - parachutecount));
                                if (parachutecount == chutes) chutef = true;
                            }
                        }
                        items.Clear();
                    }
                }
            }
            bool Pressure = true;
            if (AutoDoor)
            {
                foreach (IMyAirVent vent in vents)
                {
                    if (!vent.CanPressurize)
                    {
                        foreach (IMyDoor door in doors)
                        {
                            door.CloseDoor();
                        }
                        Pressure = false;
                        break;
                    }
                }
            }
            if (AutoBattle && Alarm)
            {
                foreach (IMyDoor door in doors)
                {
                    door.CloseDoor();
                }
                foreach (IMyAirtightHangarDoor door in doors)
                {
                    door.CloseDoor();
                }
                Alarm = false;
            }
            List<string> infos = new List<string>();
            sb.Append(mn1 + ShipName + mn2 + load);
            sb.AppendLine();
            if (Menu == 0)
            {
                sb.Append(mn3);
                sb.AppendLine();
                infos.Add(mn4);
                infos.Add(mn5);
                infos.Add(mn6);
                infos.Add(mn7);
            }
            if (Menu == 1)
            {
                sb.Append(mn8);
                sb.AppendLine();
                infos.Add(mn9 + IsEnabled(OwnRadar) + "-");
                infos.Add(mn10 + IsEnabled(AutoBattle) + "-");
                infos.Add(mn11 + IsEnabled(AutoDoor) + "-");
                infos.Add(mn12 + IsEnabled(AutoRefill) + "-");
                infos.Add(mn13 + IsEnabled(Safety) + "-");
                if (RLS != null) infos.Add(mn14 + IsWork(RLS) + "-");
                infos.Add(mn15);
            }
            if (Menu == 2)
            {
                sb.Append(mn16);
                sb.AppendLine();
                sb.Append(mn17);
                sb.AppendLine();
                bool allf = true;
                int count = 0;
                foreach (IMyProgrammableBlock pb in PB)
                {
                    if (!pb.IsWorking) { sb.Append(pb.CustomName + " ВЫКЛ"); sb.AppendLine(); allf = false; }
                    if (!pb.IsFunctional) { sb.Append(" И СГОРЕЛ"); sb.AppendLine(); }
                    if (pb.CustomName.Contains(triggers)) count++;
                }
                if (allf) { sb.Append("Нет таких"); sb.AppendLine(); }

                sb.Append(mn18 + count);
                sb.AppendLine();
                if (OwnRadar)
                {
                    sb.Append(mn19);
                    sb.AppendLine();
                }
                sb.Append(mn20 + IsEnabled(battlemode));
                sb.AppendLine();
                infos.Add(mn15);
            }
            if (Menu == 3)
            {
                sb.Append(mn21);
                sb.AppendLine();
                infos.Add(mn22);
                infos.Add(mn23);
                infos.Add(mn24);
                infos.Add(mn25);
                infos.Add(mn26);
                infos.Add(mn27);
                infos.Add(mn15);
            }
            if (Menu == 4)
            {
                sb.Append(mn28);
                sb.AppendLine();
                if (baseconn != null && baseconn.Status == MyShipConnectorStatus.Connected && baseconn.OtherConnector.IsSameConstructAs(cockpit))
                {
                    if (!ENG)
                    {
                        sb.Append("КОРАБЛЬ ПРИСТЫКОВАН");
                        sb.AppendLine();
                        if (AutoRefill)
                        {
                            sb.Append("Статус заправки:");
                            sb.AppendLine();
                            if (!bulletsf) { sb.Append("Пулемётные ленты не заряжены"); sb.AppendLine(); }
                            if (!missilef) { sb.Append("Ракеты не заряжены"); sb.AppendLine(); }
                            if (!batf) { sb.Append("Аккумуляторы не заряжены"); sb.AppendLine(); }
                            if (!icef) { sb.Append("Лёд не заправлен"); sb.AppendLine(); }
                            if (!reactorf) { sb.Append("Уран не заправлен"); sb.AppendLine(); }
                            if (!chutef) { sb.Append("Парашюты не заправлены"); sb.AppendLine(); }
                            if (bulletsf && missilef && batf && icef && reactorf && chutef) { sb.Append("Заправка завершена!"); sb.AppendLine(); }
                        }
                    }
                    else
                    {
                        sb.Append("SHIP CONNECTED");
                        sb.AppendLine();
                        if (AutoRefill)
                        {
                            sb.Append("Refill status: ");
                            sb.AppendLine();
                            if (!bulletsf) { sb.Append("Machineguns ammo not loaded"); sb.AppendLine(); }
                            if (!missilef) { sb.Append("Missiles not loaded"); sb.AppendLine(); }
                            if (!batf) { sb.Append("Batteries on recharge"); sb.AppendLine(); }
                            if (!icef) { sb.Append("Ice not loaded"); sb.AppendLine(); }
                            if (!reactorf) { sb.Append("Uranium not loaded"); sb.AppendLine(); }
                            if (!chutef) { sb.Append("Canvas not loaded"); sb.AppendLine(); }
                            if (bulletsf && missilef && batf && icef && reactorf && chutef) { sb.Append("Refill completed!"); sb.AppendLine(); }
                        }
                    }
                }
                if (AutoDoor && vents.Count > 0)
                {
                    if (!ENG)
                    {
                        if (Pressure) sb.Append("Герметизация - есть");
                        else sb.Append("Герметизация - отсутствует");
                        sb.AppendLine();
                    }
                    else
                    {
                        if (Pressure) sb.Append("Pressure - true");
                        else sb.Append("Pressure - false");
                        sb.AppendLine();
                    }
                }
                if (!ENG)
                {
                    if (baseconn != null) { sb.Append("Координаты базы записаны!"); sb.AppendLine(); }
                    else { sb.Append("Координаты базы отсутствуют"); sb.AppendLine(); }
                    List<IMyTerminalBlock> blocg = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocks(blocg);
                    bool hasdamage = false;
                    foreach (IMyTerminalBlock block in blocg) { if (!block.IsFunctional) { hasdamage = true; break; } }
                    if (hasdamage) sb.Append("Есть повреждения");
                    if (!hasdamage) sb.Append("Повреждений нет");
                    blocg.Clear();
                }
                else
                {
                    if (baseconn != null) { sb.Append("Base coords exists!"); sb.AppendLine(); }
                    else { sb.Append("Base coords is empty"); sb.AppendLine(); }
                    List<IMyTerminalBlock> blocg = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocks(blocg);
                    bool hasdamage = false;
                    foreach (IMyTerminalBlock block in blocg) { if (!block.IsFunctional) { hasdamage = true; break; } }
                    if (hasdamage) sb.Append("Ship damaged");
                    if (!hasdamage) sb.Append("No damaged blocks");
                    blocg.Clear();
                }
                sb.AppendLine();
                infos.Add(mn15);
            }
            InfoToSb(infos);
            if (LCD != null)
            {
                LCD.WriteText(sb.ToString());
                LCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                LCD.Enabled = true;
            }
            if (cockpit != null && CockpitLCD)
            {
                cockpit.GetSurface(0).WriteText(sb.ToString());
                cockpit.GetSurface(0).ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            }
            sb.Clear();
            Alarm = false;
            switch (args)
            {
                case "ALARM": Alarm = true; break;
                case "UP": StrinG--; break;
                case "DOWN": StrinG++; break;
                case "ENTER":
                    string b = infos[StrinG];
                    if (b == mn15) Menu = 0;
                    if (b == mn4) { Menu = 1; StrinG = 0; }
                    if (b == mn5) { Menu = 2; StrinG = 0; }
                    if (b == mn6) { Menu = 3; StrinG = 0; }
                    if (b == mn7) { Menu = 4; StrinG = 0; }
                    if (b == mn9 + IsEnabled(OwnRadar) + "-") OwnRadar = !OwnRadar;
                    if (b == mn10 + IsEnabled(AutoBattle) + "-") AutoBattle = !AutoBattle;
                    if (b == mn11 + IsEnabled(AutoDoor) + "-") AutoDoor = !AutoDoor;
                    if (b == mn12 + IsEnabled(AutoRefill) + "-") AutoRefill = !AutoRefill;
                    if (b == mn13 + IsEnabled(Safety) + "-") Safety = !Safety;
                    if (b == mn26) cruise = !cruise;
                    if (b == mn15)
                    {
                        foreach (IMyGyro gyro in gyros)
                        {
                            gyro.GyroOverride = false;
                        }
                    }
                    if (b == mn27) { cruise = false; ToHome = false; }
                    if (b == mn24) Landing = true;
                    if (b == mn25) ToHome = !ToHome;
                    if (b == mn22)
                    {
                        if (baseconn != null && baseconn.Status == MyShipConnectorStatus.Connected && baseconn.OtherConnector.IsSameConstructAs(cockpit))
                        {
                            foreach (IMyBatteryBlock bat in bats)
                            {
                                bat.ChargeMode = ChargeMode.Auto;
                            }
                            foreach (IMySmallGatlingGun gun in guns)
                            {
                                gun.Enabled = true;
                            }
                            foreach (IMyThrust thrust in thrusts)
                            {
                                thrust.Enabled = true;
                            }
                            foreach (IMyReactor reactor in reactors)
                            {
                                reactor.UseConveyorSystem = true;
                            }
                            baseconn.Disconnect();
                        }
                    }
                    if (b == mn14 + IsWork(RLS) + "-") RLS.Enabled = !RLS.Enabled;
                    //if (b == )
                    ; break;
                case "BACK": Menu = 0; break;
            }

            bats.Clear();
            timers.Clear();
            reactors.Clear();
            gyros.Clear();
            PB.Clear();
            turrets.Clear();
            parachutes.Clear();
            produce.Clear();
            vents.Clear();
            doors.Clear();
            guns.Clear();
            cannons.Clear();
        }
        void InfoToSb(List<string> infos)
        {
            if (StrinG >= infos.Count) StrinG = 0;
            if (StrinG < 0) StrinG = infos.Count - 1;
            foreach (string str in infos)
            {
                sb.Append(str);
                if (str == infos[StrinG]) sb.Append(point);
                sb.AppendLine();
            }
        }