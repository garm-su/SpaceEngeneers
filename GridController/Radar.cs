
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
using VRage.Game.GUI.TextPanel;
using SpaceEngineers.UWBlockPrograms.Grid;
using SpaceEngineers.UWBlockPrograms.LogLibrary;
using static SpaceEngineers.UWBlockPrograms.LogLibrary.Program;

// Change this namespace for each script you create.
namespace SpaceEngineers.UWBlockPrograms.GridStatusRadar //@remove
{ //@remove
    public class Program : GridStatusLcd.Program //@remove
    { //@remove

        public class gridPosition
        {
            public bool isEnemy;
            public string gridName;
            public Vector3D position;
            public int type; //0 - static, 1 - large, 2 - small and characters

            public gridPosition(double x, double y, double z, bool enemyFlag, string gridType, string name = "")
            {
                gridName = name;
                position = new Vector3D(x, y, z);
                isEnemy = enemyFlag;
                if (gridType.Contains("Static"))
                {
                    type = 0;
                }
                else if (gridType.Contains("Large"))
                {
                    type = 1;
                }
                else
                {
                    type = 2;
                }
            }
            public override string ToString()
            {
                return gridName + ":" + position.ToString();
            }

            public void drawGrid2D(MySpriteDrawFrame frame, float maxRange, Vector2 surfaceSize, gridPosition myGrid, MatrixD mat, Color baseColor, Color borderColor)
            {
                var rPos = position - myGrid.position;
                var distance = rPos.Length();
                var rDir = Vector3D.Normalize(rPos); //world direction to grid from myGrid

                //Convert worldDirection into a local direction
                Vector3D rVector = Vector3D.TransformNormal(rDir, MatrixD.Transpose(mat)) * distance; //note that we transpose to go from world -> body
                if (distance == 0)
                {
                    rVector = new Vector3D(0f, 0f, 0f);
                }
                double spriteX = rVector.X * (surfaceSize.X / (2 * maxRange)) + surfaceSize.X / 2;
                double lineHeight = rVector.Y * (surfaceSize.Y / (2 * maxRange));
                double spriteY = rVector.Z * (surfaceSize.Y / (2 * maxRange)) + surfaceSize.Y / 2;

                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2((float)spriteX, (float)spriteY + (float)lineHeight / 4), new Vector2(1f, (float)lineHeight / 2), new Color(50, 50, 50), null, TextAlignment.CENTER, 0f));
                if (distance <= maxRange)
                {
                    switch (type)
                    {
                        case 0: //square
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2((float)spriteX, (float)spriteY), new Vector2(20f, 20f), borderColor, null, TextAlignment.CENTER, 0f));
                            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2((float)spriteX, (float)spriteY), new Vector2(17f, 17f), baseColor, null, TextAlignment.CENTER, 0f));
                            break;
                        case 1: //triangle
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2((float)spriteX, (float)spriteY), new Vector2(20f, 20f), borderColor, null, TextAlignment.CENTER, 0f));
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Triangle", new Vector2((float)spriteX, (float)spriteY), new Vector2(17f, 17f), baseColor, null, TextAlignment.CENTER, 0f));
                            break;
                        case 2: //circle
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2((float)spriteX, (float)spriteY), new Vector2(20f, 20f), borderColor, null, TextAlignment.CENTER, 0f));
                            frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", new Vector2((float)spriteX, (float)spriteY), new Vector2(17f, 17f), baseColor, null, TextAlignment.CENTER, 0f));
                            break;
                    }
                }
                else
                {
                    //todo - bordermarkers for ranged targets?
                }
            }

        }

        void drawMapBorder(MySpriteDrawFrame frame, float maxRange, Vector2 surfaceSize, Color baseColor, Color borderColor)
        {
            int marksCount = (int)Math.Round(maxRange / 100);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(surfaceSize.X / 2, surfaceSize.Y / 2), new Vector2(surfaceSize.X - 10f, surfaceSize.Y - 10f), baseColor, null, TextAlignment.CENTER, 0f));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(surfaceSize.X / 2, surfaceSize.Y / 2), new Vector2(surfaceSize.X - 12f, surfaceSize.Y - 12f), new Color(0, 0, 0), null, TextAlignment.CENTER, 0f));
            for (var i = 0; i < marksCount; i++)
            {
                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(5f + i * (surfaceSize.X - 10f) / marksCount, surfaceSize.Y / 2), new Vector2(1f, surfaceSize.Y), borderColor, null, TextAlignment.CENTER, 0f));
                frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(surfaceSize.X / 2, 5f + i * (surfaceSize.Y - 10f) / marksCount), new Vector2(surfaceSize.X, 1f), borderColor, null, TextAlignment.CENTER, 0f));
            }
        }

        void drawMapText(MySpriteDrawFrame frame, double maxRange, Vector2 surfaceSize, gridPosition myGrid, List<Color> colorScheme, int enemyCount, int allyCount)
        {
            var sprite = MySprite.CreateText("Range:" + ((float)maxRange / 1000).ToString() + "km", "Debug", colorScheme[0], 1f, TextAlignment.CENTER);
            sprite.Position = new Vector2(surfaceSize.X / 2, 15f);
            frame.Add(sprite);
            if (allyCount > 0)
            {
                sprite = MySprite.CreateText("Ally:" + allyCount.ToString(), "Debug", colorScheme[1], 1.2f, TextAlignment.LEFT);
                sprite.Position = new Vector2(10f, surfaceSize.Y - 75f);
                frame.Add(sprite);
            }

            if (enemyCount > 0)
            {
                sprite = MySprite.CreateText("Enemy:" + enemyCount.ToString(), "Debug", colorScheme[2], 1.2f, TextAlignment.RIGHT);
                sprite.Position = new Vector2(surfaceSize.X - 10f, surfaceSize.Y - 75f);
                frame.Add(sprite);
            }

            string gps = "GPS:";
            gps += String.Format("{0:0.00}", myGrid.position.X);
            gps += ":";
            gps += String.Format("{0:0.00}", myGrid.position.Y);
            gps += ":";
            gps += String.Format("{0:0.00}", myGrid.position.Z);
            sprite = MySprite.CreateText(gps, "Debug", colorScheme[0], 1f, TextAlignment.CENTER);
            sprite.Position = new Vector2(surfaceSize.X / 2, surfaceSize.Y - 40f);
            frame.Add(sprite);
        }

        public void drawMap(double maxRange)
        {
            string allyJson = "[" + string.Join(",", allyPositions) + "]";
            string targetsJson = "";
            List<IMyTerminalBlock> displays = new List<IMyTerminalBlock>();
            List<IMyShipController> controls = new List<IMyShipController>();
            reScanObjectGroupLocal(displays, radarTag);
            reScanObjects(controls);
            Vector3D myPosition = Me.GetPosition();
            List<gridPosition> allyGrids = new List<gridPosition>();
            gridPosition myGrid = new gridPosition(myPosition.X, myPosition.Y, myPosition.Z, false, "Large");
            IMyTerminalBlock refBlock = Me;

            allyGrids = parseGridPositions(allyJson, false); //todo: get from status withour LCD
            //enemyGrids = parseGridPositions(targetsJson, false);
            if (controls.Count() > 0)
            {
                refBlock = (IMyTerminalBlock)controls[0];
                foreach (var control in controls)
                {
                    if (control.IsUnderControl)
                    {
                        refBlock = (IMyTerminalBlock)control;
                    }
                }
            }

            foreach (var display in displays)
            {
                var e = display as IMyTextSurfaceProvider;
                var elem = e.GetSurface(0);
                initDrawSurface(elem);
                using (var frame = elem.DrawFrame())
                {
                    drawMapBorder(frame, (float)maxRange, elem.SurfaceSize, new Color(0, 150, 0), new Color(0, 20, 0));
                    List<Color> cSheme = new List<Color>();
                    cSheme.Add(new Color(0, 200, 0));
                    cSheme.Add(new Color(0, 0, 200));
                    cSheme.Add(new Color(200, 0, 0));
                    drawMapText(frame, (float)maxRange, elem.SurfaceSize, myGrid, cSheme, 1, allyGrids.Count()); //enemyGrids.Count()
                    foreach (var ally in allyGrids)
                    {
                        ally.drawGrid2D(frame, (float)maxRange, elem.SurfaceSize, myGrid, refBlock.WorldMatrix, new Color(0, 0, 200), new Color(0, 0, 50));
                    }
                    /*foreach(var enemy in enemyGrids)
                    {
                        enemy.drawGrid2D(frame, (float)maxRange, elem.SurfaceSize, myGrid, refBlock.WorldMatrix, new Color(200,0,0), new Color(50,0,0));
                    }*/
                    myGrid.drawGrid2D(frame, (float)maxRange, elem.SurfaceSize, myGrid, refBlock.WorldMatrix, new Color(200, 200, 200), new Color(50, 50, 50));
                }
            }
        }

        List<gridPosition> parseGridPositions(string positionsList, bool isEnemy)
        {
            List<gridPosition> result = new List<gridPosition>();
            JsonList jsonData;

            if (positionsList == "[]")
            {
                return result;
            }

            try
            {
                jsonData = (new JSON(positionsList)).Parse() as JsonList;
            }
            catch (Exception e) // in case something went wrong (either your json is wrong or my library has a bug :P)
            {
                Echo("There's somethign wrong with your json: " + e.Message);
                return null;
            }

            foreach (var elem in jsonData)
            {
                JsonObject temp;
                temp = (JsonObject)elem;
                var jsonPosition = ((JsonObject)temp["Position"]);
                //todo: add grid type
                result.Add(new gridPosition(((JsonPrimitive)jsonPosition["X"]).GetValue<double>(), ((JsonPrimitive)jsonPosition["Y"]).GetValue<double>(), ((JsonPrimitive)jsonPosition["Z"]).GetValue<double>(), isEnemy, "Small"));
            }
            return result;
        }

        public JsonList getEnemyTargetsData()
        {
            JsonList result = new JsonList("Targets");
            foreach (MyDetectedEntityInfo target in targets)
            {
                var t = new JsonObject("");
                t.Add(new JsonPrimitive("Name", target.Name));
                t.Add(new JsonPrimitive("Type", target.Type.ToString()));
                t.Add(new JsonPrimitive("Position", target.Position.ToString()));
                result.Add(t);
            }
            return result;
        }

        public List<MyDetectedEntityInfo> getTurretsTargets()
        {
            List<MyDetectedEntityInfo> result = new List<MyDetectedEntityInfo>();
            List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
            reScanObjects(turrets);
            foreach (IMyLargeTurretBase t in turrets)
            {
                if (t.HasTarget)
                {
                    MyDetectedEntityInfo trg = t.GetTargetedEntity();
                    result.Add(trg);
                }
            }
            return result;
        }

        public List<MyDetectedEntityInfo> getScannedTargets()
        {
            List<MyDetectedEntityInfo> result = new List<MyDetectedEntityInfo>();

            return result;
        }
        public List<MyDetectedEntityInfo> getSensorsTargets()
        {
            List<MyDetectedEntityInfo> result = new List<MyDetectedEntityInfo>();
            foreach (var s in gridSensors)
            {
                if (!s.Closed)
                {
                    List<MyDetectedEntityInfo> s_targets = new List<MyDetectedEntityInfo>();
                    s.DetectedEntities(s_targets);
                    foreach (var t in s_targets)
                    {
                        if ((t.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) && (!result.Any(n => n.EntityId == t.EntityId)))
                        {
                            result.Add(t);
                        }
                    }
                }
            }
            return result;
        }

        public List<MyDetectedEntityInfo> updateLocalTargets()
        {
            List<MyDetectedEntityInfo> result = getTurretsTargets();
            List<MyDetectedEntityInfo> localSens = getSensorsTargets();
            List<MyDetectedEntityInfo> localScan = getScannedTargets(); //todo

            foreach (var elem in localSens)
            {
                if (!result.Any(n => n.EntityId == elem.EntityId))
                {
                    result.Add(elem);
                }
            }

            return result;//dummy, rework
        }

        public void updateTargets()
        {
            targets.Clear();
            targets = updateLocalTargets();
            //return targets;

            //todo: get json targets from net to string networkTargets
            //send local targets to network channel
        }

    }  //@remove
}  //@remove