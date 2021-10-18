﻿/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/D2RAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/
using D2RAssist.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static D2RAssist.MapData;
using System.Text.RegularExpressions;

namespace D2RAssist.Helpers
{
    static class MapRenderer
    {
        public static Point LineEnd = new Point(0, 0);
        public static Bitmap CachedBackground;
        public static string TombID = "0";

        public static class Icons
        {
            public static readonly Bitmap DoorNext = CreateFilledRectangle(Settings.Map.Colors.DoorNext, 10, 10);
            public static readonly Bitmap DoorPrevious = CreateFilledRectangle(Settings.Map.Colors.DoorPrevious, 10, 10);
            public static readonly Bitmap Waypoint = CreateFilledRectangle(Settings.Map.Colors.Waypoint, 10, 10);
            public static readonly Bitmap Player = CreateFilledEllipse(Settings.Map.Colors.Player, 10, 10);
            public static readonly Bitmap SuperChest = CreateFilledEllipse(Settings.Map.Colors.SuperChest, 10, 10);
            public static readonly Bitmap Chest = CreateFilledEllipse(Settings.Map.Colors.Chest, 10, 10);
        }

        public static class MapPointsOfInterest
        {
            public static readonly string[] Chests = { "5", "6", "87", "104", "105", "106", "107", "143", "140", "141", "144", "146", "147", "148", "176", "177", "181", "183", "198", "240", "241", "242", "243", "329", "330", "331", "332", "333", "334", "335", "336", "354", "355", "356", "371", "387", "389", "390", "391", "397", "405", "406", "407", "413", "420", "424", "425", "430", "431", "432", "433", "454", "455", "501", "502", "504", "505", "580", "581", };
            public static readonly string[] Quests = { "61", "152", "266","357", "356", "354", "355", "376", "462" };
            public static readonly string[] Waypoints = { "182", "298", "119", "145", "156", "157", "238", "237", "288", "323", "324", "398", "402", "429", "494", "496", "511", "539", "59", "60", "100" };
            public static readonly string[] TalTombs = { "66", "67", "68", "69", "70", "71", "72" };
        }
            
        private static Bitmap CreateFilledRectangle(Color color, int width, int height)
        {
            Bitmap rectangle = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(rectangle);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return rectangle;
        }

        private static Bitmap CreateFilledEllipse(Color color, int width, int height)
        {
            Bitmap ellipse = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(ellipse);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillEllipse(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return ellipse;
        }

        public static void Clear()
        {
            CachedBackground = null;
            LineEnd = new Point(0, 0);
        }

        public static Bitmap FromMapData(MapData mapData)
        {

            Graphics CachedBackgroundGraphics;
            Bitmap updatedMap;

            if (CachedBackground == null)
            {
                var uncroppedBackground = new Bitmap(mapData.mapRows[0].Length, mapData.mapRows.Length, PixelFormat.Format32bppArgb);

                for (int x = 0; x < mapData.mapRows.Length; x++)
                {
                    for (int y = 0; y < mapData.mapRows[x].Length; y++)
                    {
                        int type = mapData.mapRows[x][y];
                        Color? typeColor = Settings.Map.Colors.LookupMapColor(type);
                        if (typeColor != null)
                        {
                            uncroppedBackground.SetPixel(y, x, (Color)typeColor);
                        }
                    }
                }
                CachedBackground = ImageManipulation.CropBitmap(uncroppedBackground);
            }

            double biggestDimension = CachedBackground.Width > CachedBackground.Height ? CachedBackground.Width : CachedBackground.Height;

            if (biggestDimension == 0)
            {
                biggestDimension = 1;
            }

            double multiplier = Settings.Map.Size / biggestDimension;

            if (multiplier == 0)
            {
                multiplier = 1;
            }

            int miniMapPlayerX = (int)((Globals.CurrentGameData.PlayerX - mapData.levelOrigin.x) * multiplier);
            int miniMapPlayerY = (int)((Globals.CurrentGameData.PlayerY - mapData.levelOrigin.y) * multiplier);
            updatedMap = ImageManipulation.ResizeImage((Bitmap)CachedBackground.Clone(), (int)(CachedBackground.Width * multiplier), (int)(CachedBackground.Height * multiplier));

            CachedBackgroundGraphics = Graphics.FromImage(updatedMap);
            Globals.MinimapPlayerPosition = new Point (miniMapPlayerX, miniMapPlayerY);
            Globals.MinimapBaseSize = new Point ((int)(CachedBackground.Width * multiplier), (int)(CachedBackground.Height * multiplier));
            DrawArrows (miniMapPlayerX, miniMapPlayerY, CachedBackgroundGraphics, multiplier, mapData);

            int counter = 0;
            int originX = mapData.levelOrigin.x;
            int originY = mapData.levelOrigin.y;

            foreach (KeyValuePair<string, AdjacentLevel> i in mapData.adjacentLevels)
            {
                if (mapData.adjacentLevels[i.Key].exits.Length == 0)
                {
                    continue;
                }
                int x = mapData.adjacentLevels[i.Key].exits[0].x;
                int y = mapData.adjacentLevels[i.Key].exits[0].y;
                int coordX = (int)(multiplier * (x - originX));
                int coordY = (int)(multiplier * (y - originY));
                var nextLevelPoint = LineEnd = new Point(coordX, coordY);
                if (counter == 0)
                {
                    CachedBackgroundGraphics.DrawImage(Icons.DoorPrevious, nextLevelPoint);
                }
                else
                {
                    CachedBackgroundGraphics.DrawImage(Icons.DoorNext, nextLevelPoint);
                }
                counter++;
            }

            List<Point> chestPoints = new List<Point>();
            foreach (KeyValuePair<string, XY[]> mapObject in mapData.objects)
            {
                int coordX = MultiplyIntByDouble(mapData.objects[mapObject.Key][0].x - originX, multiplier);
                int coordY = MultiplyIntByDouble(mapData.objects[mapObject.Key][0].y - originY, multiplier);
                Point mapObjectPoint = new Point(coordX, coordY);
                string mapObjectKey = (string)mapObject.Key;
                if (MapPointsOfInterest.Waypoints.Contains(mapObject.Key))
                {
                    CachedBackgroundGraphics.DrawImage(Icons.Waypoint, mapObjectPoint);
                }
                else if (MapPointsOfInterest.Waypoints.Contains(mapObject.Key))
                {
                    CachedBackgroundGraphics.DrawImage(Icons.DoorNext, mapObjectPoint);
                }
                else if (MapPointsOfInterest.Chests.Contains(mapObjectKey))
                {
                    foreach (XY coordinates in mapData.objects[mapObject.Key])
                    {
                        coordX = MultiplyIntByDouble (coordinates.x - originX, multiplier);
                        coordY = MultiplyIntByDouble (coordinates.y - originY, multiplier);
                        mapObjectPoint = new Point (coordX, coordY);

                        if (Globals.CurrentGameData.AreaId == Game.Area.LowerKurast) {
                            chestPoints.Add (mapObjectPoint);
                        } else {
                            CachedBackgroundGraphics.DrawImage (Icons.Chest, mapObjectPoint);
                        }
                    }
                }
            }

            if (Globals.CurrentGameData.AreaId == Game.Area.LowerKurast) {
                List<int> objectIndexToDraw = new List<int>();
                for (int i = 0 ; i < chestPoints.Count ; i++) {
                    if (objectIndexToDraw.Contains (i)) {
                        continue;
                    }
                    Point p1 = chestPoints[i];
                    for (int j = 0 ; j < chestPoints.Count ; j++) {
                        if (objectIndexToDraw.Contains (j))
                            continue;
                        Point p2 = chestPoints[j];
                        double distance = Math.Sqrt(Math.Pow(p1.X-p2.X,2)+Math.Pow(p1.Y-p2.Y,2));
                        Console.WriteLine (distance);
                        if (distance > 10 && distance < 28) {
                            objectIndexToDraw.Add (i);
                            objectIndexToDraw.Add (j);
                        }
                    }
                    if (objectIndexToDraw.Contains (i)) {
                        CachedBackgroundGraphics.DrawImage (Icons.SuperChest, chestPoints[i]);
                    }
                }
            }

            CachedBackgroundGraphics.DrawImage(Icons.Player, Globals.MinimapPlayerPosition);

            if (Settings.Map.Rotate != 0)
            {
                updatedMap = ImageManipulation.RotateImage (updatedMap, Settings.Map.Rotate, true, false, Color.Transparent);
            }

            return updatedMap;
        }

        private static int MultiplyIntByDouble(int _int, double _double)
        {
            return (int)(_int * _double);
        }

        private static void DrawArrows(int miniMapPlayerX, int miniMapPlayerY, Graphics minimap, double multiplier, MapData mapData)
        {
            int originX = mapData.levelOrigin.x;
            int originY = mapData.levelOrigin.y;
            // Setting up resuable pen
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(Settings.Map.ArrowThickness, Settings.Map.ArrowThickness);
            Pen pen = new Pen(Color.Transparent, Settings.Map.ArrowThickness);
            pen.CustomEndCap = bigArrow;

            // Exits
            foreach (KeyValuePair<string, AdjacentLevel> i in mapData.adjacentLevels)
            {
                if (mapData.adjacentLevels[i.Key].exits.Length == 0 || !Settings.Map.DrawExitArrow)
                {
                    continue;
                }
                if (MapPointsOfInterest.TalTombs.Contains(i.Key))
                {
                    TombCheck(i.Key);
                    if (TombID == "0" || i.Key != TombID)
                    {
                        continue;
                    }
                }
                int x = mapData.adjacentLevels[i.Key].exits[0].x;
                int y = mapData.adjacentLevels[i.Key].exits[0].y;
                int coordX = (int)((x - originX) * multiplier);
                int coordY = (int)((y - originY) * multiplier);

                // Draw arrow
                pen.Color = Settings.Map.Colors.ArrowExit;
                if (i.Key.toArea ().IsForward()) {
                    minimap.DrawLine(pen, miniMapPlayerX, miniMapPlayerY, coordX, coordY);
                }
                // Draw label
                DrawLabelAt("Area", i.Key, coordX, coordY, minimap);
            }
            
            bool waypointFound = false;
            foreach (KeyValuePair<string, XY[]> mapObject in mapData.objects)
            {
                if (Settings.Map.DrawWaypointArrow && !waypointFound && MapPointsOfInterest.Waypoints.Contains(mapObject.Key))
                {
                    Point destPoint = GetMapObjectPoint(mapData, mapObject.Key, originX, originY, multiplier);
                    // Draw arrow
                    pen.Color = Settings.Map.Colors.ArrowWaypoint;
                    minimap.DrawLine(pen, miniMapPlayerX, miniMapPlayerY, destPoint.X, destPoint.Y);
                    // Draw label
                    DrawLabelAt("GameObject", mapObject.Key, destPoint.X, destPoint.Y, minimap);
                    waypointFound = true;
                }

                if (Settings.Map.DrawQuestArrow && (MapPointsOfInterest.Quests.Contains(mapObject.Key) || MapPointsOfInterest.TalTombs.Contains(mapObject.Key)))
                {
                    Point destPoint = GetMapObjectPoint(mapData, mapObject.Key, originX, originY, multiplier);
                    // Draw arrow
                    pen.Color = Settings.Map.Colors.ArrowQuest;
                    minimap.DrawLine(pen, miniMapPlayerX, miniMapPlayerY, destPoint.X, destPoint.Y);
                    // Draw label
                    DrawLabelAt("GameObject", mapObject.Key, destPoint.X, destPoint.Y, minimap);
                }
            }
        }

        private async static void TombCheck(string mapObjectKey)
        {
            Game.Area tomb = (Game.Area)Int32.Parse(mapObjectKey);
            MapData tombMapData = await MapApi.GetMapData(tomb);
            if (tombMapData.objects.ContainsKey("152"))
            {
                TombID = mapObjectKey;
            }
        }

        private static Point GetMapObjectPoint(MapData mapData, string mapObjectKey, int originX, int originY, double multiplier)
        {
            return new Point((int)((mapData.objects[mapObjectKey][0].x - originX) * multiplier), (int)((mapData.objects[mapObjectKey][0].y - originY) * multiplier));
        }

        private static void DrawLabelAt(string objectType, string objectKey, int posx, int posy, Graphics minimap)
        {
            minimap.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            // Create font and brush.
            Font drawFont = new Font(Settings.Map.LabelFont, 10);
            SolidBrush drawBrush = new SolidBrush(Settings.Map.Colors.LabelColor);
            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.NoClip;
            // Draw label line
            int adjustment = 20;
            Pen p = new Pen(Settings.Map.Colors.LabelColor, 1);
            minimap.DrawLine(p, posx + adjustment, posy - (adjustment), posx, posy);
            // Label box
            RectangleF rectangleF = new RectangleF(posx + adjustment, posy - (2 * adjustment), 100, 100);
            if (objectType == "Area")
            {
                string objName = Enum.GetName(typeof(Game.Area), Int32.Parse(objectKey));
                objName = Game.AreaName[Int32.Parse(objectKey)];
                minimap.DrawString(objName, drawFont, drawBrush, rectangleF, drawFormat);
            }
            if (objectType == "GameObject")
            {
                string objName = Enum.GetName(typeof(Game.GameObject), Int32.Parse(objectKey));
                if (objName.Contains("Waypoint")) objName = "Waypoint";
                minimap.DrawString(objName, drawFont, drawBrush, rectangleF, drawFormat);
            }
            if (objectType == "Npc") {
                
            }
        }
    }
}