using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class SettlementHelper
    {
        private class Building
        {
            public Rectangle Bounds;
            public Point Door;
            public string Type;
        }

        public static void GenerateSettlement(Zone zone)
        {
            int width = zone.Width;
            int height = zone.Height;
            Random rnd = zone.Random;

            // STEP 1: Define town center plaza
            int plazaSize = 5;
            Rectangle plaza = new Rectangle(width / 2 - plazaSize / 2, height / 2 - plazaSize / 2, plazaSize, plazaSize);
            CarvePlaza(zone, plaza);

            // STEP 3: Carve main north-south and east-west roads through center
            CarveMainRoads(zone, plaza);

            var buildings = new List<Building>();

            // STEP 2: Town hall and key landmark near center
            var townHall = PlaceBuilding(zone, plaza.Center.X - 4, plaza.MinExtentY - 6, 9, 6, "TownHall", buildings);
            if (townHall != null) buildings.Add(townHall);

            var temple = PlaceBuilding(zone, plaza.Center.X + 5, plaza.Center.Y - 4, 7, 6, "Temple", buildings);
            if (temple != null) buildings.Add(temple);

            // STEP 4: Place districts
            PlaceDistrict(zone, "Residential", 8, plaza, buildings, rnd);
            PlaceDistrict(zone, "Market", 3, plaza, buildings, rnd);
            PlaceDistrict(zone, "Crafts", 2, plaza, buildings, rnd);
            PlaceDistrict(zone, "Farms", 3, plaza, buildings, rnd);

            // STEP 5: Connect all doors to plaza
            foreach (var b in buildings)
                CarveAStarRoad(zone, b.Door, plaza.Center, plaza);

            // STEP 6: Perimeter wall with towers + gates
            DrawPerimeterWall(zone, buildings, 3);
        }

        // ---------------- DISTRICTS ----------------

        private static void PlaceDistrict(Zone zone, string district, int count, Rectangle plaza, List<Building> buildings, Random rnd)
        {
            int attempts = 0;
            while (count > 0 && attempts < 500)
            {
                int w = rnd.Next(4, 9);
                int h = rnd.Next(4, 8);

                // Bias location by district type
                int x, y;
                switch (district)
                {
                    case "Residential":
                        x = rnd.Next(plaza.MinExtentX - 20, plaza.MaxExtentX + 20);
                        y = rnd.Next(plaza.MinExtentY - 8, plaza.MaxExtentY + 8);
                        break;
                    case "Market":
                        x = rnd.Next(plaza.MinExtentX - 8, plaza.MaxExtentX + 8);
                        y = rnd.Next(plaza.MinExtentY - 8, plaza.MaxExtentY + 8);
                        break;
                    case "Crafts":
                        x = rnd.Next(plaza.MaxExtentX + 4, plaza.MaxExtentX + 4);
                        y = rnd.Next(plaza.MinExtentY - 4, plaza.MaxExtentY + 4);
                        break;
                    case "Farms":
                        x = rnd.Next(plaza.MaxExtentX + 7, plaza.MaxExtentX + 7);
                        y = rnd.Next(plaza.MinExtentY - 7, plaza.MaxExtentY + 7);
                        break;
                    default:
                        x = rnd.Next(plaza.MaxExtentX + 12, plaza.MaxExtentX + 12);
                        y = rnd.Next(plaza.MinExtentY - 8, plaza.MaxExtentY + 8);
                        break;
                }

                string type = PickBuildingType(district, rnd);
                var b = PlaceBuilding(zone, x, y, w, h, type, buildings);
                if (b != null)
                {
                    buildings.Add(b);
                    count--;
                }

                attempts++;
            }
        }

        private static string PickBuildingType(string district, Random rnd)
        {
            return district switch
            {
                "Residential" => "House",
                "Market" => rnd.NextDouble() < 0.7 ? "Shop" : "Inn",
                "Crafts" => rnd.NextDouble() < 0.5 ? "Blacksmith" : "Warehouse",
                "Farms" => "Farm",
                _ => "House"
            };
        }

        // ---------------- BUILDINGS ----------------

        private static Building PlaceBuilding(Zone zone, int x, int y, int w, int h, string type, List<Building> existing)
        {
            Rectangle rect = new Rectangle(x, y, w, h);
            Random rnd = zone.Random;

            // Skip if outside bounds
            if (rect.MinExtentX < 2 || rect.MinExtentY < 2 || rect.MaxExtentX >= zone.Width - 2 || rect.MaxExtentY >= zone.Height - 2)
                return null;

            // Extra check in PlaceBuilding before drawing
            for (int dx = x; dx < x + w; dx++)
            {
                for (int dy = y; dy < y + h; dy++)
                {
                    if (zone.Tilemap.GetTile(dx, dy).Glyph == '=')
                        return null; // don't place building across a road
                }
            }

            // Pick door position
            int side = rnd.Next(4);
            int doorX = x, doorY = y;
            switch (side)
            {
                case 0: doorX = x + 1 + rnd.Next(w - 2); doorY = y; break;
                case 1: doorX = x + 1 + rnd.Next(w - 2); doorY = y + h - 1; break;
                case 2: doorX = x; doorY = y + 1 + rnd.Next(h - 2); break;
                case 3: doorX = x + w - 1; doorY = y + 1 + rnd.Next(h - 2); break;
            }

            Point doorPoint = new Point(doorX, doorY);

            // Check overlap with existing buildings
            foreach (var b in existing)
            {
                var expanded = new Rectangle(b.Bounds.MinExtentX - 1, b.Bounds.MinExtentY - 1, b.Bounds.Width + 2, b.Bounds.Height + 2);
                if (rect.Intersects(expanded))
                    return null;
            }

            // Draw building
            for (int dx = x; dx < x + w; dx++)
            {
                for (int dy = y; dy < y + h; dy++)
                {
                    bool border = dx == x || dy == y || dx == x + w - 1 || dy == y + h - 1;
                    if (border)
                        zone.Tilemap.SetTile(dx, dy, WallTile(zone, dx, dy));
                    else
                        zone.Tilemap.SetTile(dx, dy, FloorTile(zone, dx, dy));
                }
            }

            zone.Tilemap.SetTile(doorX, doorY, DoorTile(zone, doorX, doorY));

            return new Building { Bounds = rect, Door = doorPoint, Type = type };
        }

        private static void CarvePlaza(Zone zone, Rectangle plaza)
        {
            for (int x = plaza.MinExtentX; x <= plaza.MaxExtentX; x++)
            {
                for (int y = plaza.MinExtentY; y <= plaza.MaxExtentY; y++)
                {
                    var tile = zone.Tilemap.GetTile(x, y);
                    tile.Glyph = '.';
                    tile.Foreground = Color.DarkKhaki;
                    tile.Background = Color.Black;
                }
            }
        }

        private static void CarveMainRoads(Zone zone, Rectangle plaza)
        {
            int cx = plaza.Center.X;
            int cy = plaza.Center.Y;

            // Vertical road
            for (int y = 0; y < zone.Height; y++)
                zone.Tilemap.SetTile(cx, y, RoadTile(zone, cx, y));

            // Horizontal road
            for (int x = 0; x < zone.Width; x++)
                zone.Tilemap.SetTile(x, cy, RoadTile(zone, x, cy));
        }

        // ---------------- ROADS ----------------

        private static readonly Point[] _directions = { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };

        private static void CarveAStarRoad(Zone zone, Point start, Point goal, Rectangle plaza)
        {
            var open = new SortedSet<(int f, int t, Point p)>(Comparer<(int f, int t, Point p)>.Create((a, b) =>
            {
                int cmp = a.f.CompareTo(b.f);
                if (cmp == 0) cmp = a.t.CompareTo(b.t);
                if (cmp == 0) cmp = a.p.X.CompareTo(b.p.X);
                if (cmp == 0) cmp = a.p.Y.CompareTo(b.p.Y);
                return cmp;
            }));

            var gScore = new Dictionary<Point, int> { [start] = 0 };
            var cameFrom = new Dictionary<Point, Point>();
            int counter = 0;
            open.Add((Heuristic(start, goal), counter++, start));
            HashSet<Point> closed = new();

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);
                Point pt = current.p;

                if (pt == goal) break;
                closed.Add(pt);

                foreach (var dir in _directions)
                {
                    Point next = pt + dir;
                    if (next.X < 0 || next.Y < 0 || next.X >= zone.Width || next.Y >= zone.Height) continue;
                    if (closed.Contains(next)) continue;

                    var tile = zone.Tilemap.GetTile(next.X, next.Y);

                    // Block walls, floors, and doors so roads don't carve through buildings
                    bool isBuildingFloor = tile.Glyph == '.' && !plaza.Contains(next);
                    if (tile.Glyph == '#' || isBuildingFloor)
                        continue;

                    // Allow goal door, but not other doors
                    if (tile.Glyph == '+' && next != goal)
                        continue;

                    int cost = tile.Glyph == '=' ? 1 : 3;
                    int tentative = gScore[pt] + cost;

                    if (!gScore.ContainsKey(next) || tentative < gScore[next])
                    {
                        gScore[next] = tentative;
                        cameFrom[next] = pt;
                        open.Add((tentative + Heuristic(next, goal), counter++, next));
                    }
                }
            }

            Point cur = goal;
            while (cameFrom.ContainsKey(cur))
            {
                zone.Tilemap.SetTile(cur.X, cur.Y, RoadTile(zone, cur.X, cur.Y));
                cur = cameFrom[cur];
            }
        }

        private static int Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        // ---------------- WALLS ----------------

        private static void DrawPerimeterWall(Zone zone, List<Building> buildings, int padding)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var b in buildings)
            {
                minX = Math.Min(minX, b.Bounds.MinExtentX);
                minY = Math.Min(minY, b.Bounds.MinExtentY);
                maxX = Math.Max(maxX, b.Bounds.MaxExtentX);
                maxY = Math.Max(maxY, b.Bounds.MaxExtentY);
            }

            minX = Math.Max(1, minX - padding);
            minY = Math.Max(1, minY - padding);
            maxX = Math.Min(zone.Width - 2, maxX + padding);
            maxY = Math.Min(zone.Height - 2, maxY + padding);

            // Wall outline
            for (int x = minX; x <= maxX; x++)
            {
                if (zone.Tilemap.GetTile(x, minY).Glyph != '=')
                    zone.Tilemap.SetTile(x, minY, WallTile(zone, x, minY));
                if (zone.Tilemap.GetTile(x, maxY).Glyph != '=')
                    zone.Tilemap.SetTile(x, maxY, WallTile(zone, x, maxY));
            }
            for (int y = minY; y <= maxY; y++)
            {
                if (zone.Tilemap.GetTile(minX, y).Glyph != '=')
                    zone.Tilemap.SetTile(minX, y, WallTile(zone, minX, y));
                if (zone.Tilemap.GetTile(maxX, y).Glyph != '=')
                    zone.Tilemap.SetTile(maxX, y, WallTile(zone, maxX, y));
            }

            // Towers at corners
            zone.Tilemap.SetTile(minX, minY, TowerTile(zone, minX, minY));
            zone.Tilemap.SetTile(maxX, minY, TowerTile(zone, maxX, minY));
            zone.Tilemap.SetTile(minX, maxY, TowerTile(zone, minX, maxY));
            zone.Tilemap.SetTile(maxX, maxY, TowerTile(zone, maxX, maxY));
        }

        // ---------------- TILES ----------------

        private static ColoredGlyph RoadTile(Zone zone, int x, int y)
        {
            var tile = zone.Tilemap.GetTile(x, y);
            tile.Glyph = '=';
            tile.Foreground = Color.DarkGray;
            tile.Background = "#2e2626".HexToColor();
            return tile;
        }

        private static ColoredGlyph WallTile(Zone zone, int x, int y)
        {
            var tile = zone.Tilemap.GetTile(x, y);
            tile.Glyph = '#';
            tile.Foreground = Color.Gray;
            tile.Background = "#1c130f".HexToColor();
            return tile;
        }

        private static ColoredGlyph TowerTile(Zone zone, int x, int y)
        {
            var tile = zone.Tilemap.GetTile(x, y);
            tile.Glyph = 'O';
            tile.Foreground = Color.DarkSlateGray;
            tile.Background = "#1c130f".HexToColor();
            return tile;
        }

        private static ColoredGlyph DoorTile(Zone zone, int x, int y)
        {
            var tile = zone.Tilemap.GetTile(x, y);
            tile.Glyph = '+';
            tile.Foreground = Color.Goldenrod;
            tile.Background = "#1c130f".HexToColor();
            return tile;
        }

        private static ColoredGlyph FloorTile(Zone zone, int x, int y)
        {
            var tile = zone.Tilemap.GetTile(x, y);
            tile.Glyph = '.';
            tile.Foreground = "#120d0b".HexToColor();
            tile.Background = "#1c130f".HexToColor();
            return tile;
        }
    }
}
