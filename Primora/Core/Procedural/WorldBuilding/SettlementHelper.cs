using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class SettlementHelper
    {
        private class Building
        {
            public Rectangle Bounds;
            public Point Door;
        }

        public static void GenerateSettlement(Zone zone)
        {
            SetBackgroundGrass(zone);

            int width = zone.Width;
            int height = zone.Height;

            Random rnd = zone.Random;

            // Step 1: Town center
            Point townCenter = new Point(width / 2, height / 2);
            zone.Tilemap.SetTile(townCenter.X, townCenter.Y, new ColoredGlyph { Glyph = '*', Foreground = Color.Yellow });

            // Step 2: Town Hall in front of plaza
            var buildings = new List<Building>();
            var townHall = PlaceBuilding(zone, width / 2 - 4, height / 2 - 6, 9, 6, "TownHall", buildings);
            buildings.Add(townHall);

            // Step 3: Populate the town
            int numBuildings = 20;
            int attempts = 0;
            while (buildings.Count < numBuildings && attempts < 300)
            {
                int w = rnd.Next(4, 10);
                int h = rnd.Next(4, 8);
                int x = rnd.Next(5, width - w - 5);
                int y = rnd.Next(5, height - h - 5);

                var b = PlaceBuilding(zone, x, y, w, h, "House", buildings);
                if (b != null)
                    buildings.Add(b);

                attempts++;
            }

            // Step 4: Connect all doors to town center using A*
            foreach (var b in buildings)
                CarveAStarRoad(zone, b.Door, townCenter);

            // Step 5: Build perimeter wall around all buildings
            DrawPerimeterWall(zone, buildings, 2);

            ConnectGatesToRoads(zone, buildings, 2);
        }

        // ---------------- BUILDING PLACEMENT ----------------

        private static Building PlaceBuilding(Zone zone, int x, int y, int w, int h, string type, List<Building> existing)
        {
            Rectangle rect = new Rectangle(x, y, w, h);
            Random rnd = zone.Random;

            // Determine door location first
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

            // Check overlap with existing buildings + door + 1-tile spacing
            foreach (var b in existing)
            {
                // Reserve the door tile with buffer
                var doorRect = new Rectangle(b.Door.X - 1, b.Door.Y - 1, 3, 3);

                // Expand existing building bounds by 1 for spacing
                var expandedBounds = new Rectangle(b.Bounds.MinExtentX - 1, b.Bounds.MinExtentY - 1, b.Bounds.Width + 2, b.Bounds.Height + 2);

                if (rect.Intersects(expandedBounds) || rect.Intersects(doorRect))
                    return null;
            }

            // Draw building outline
            for (int dx = x; dx < x + w; dx++)
                for (int dy = y; dy < y + h; dy++)
                {
                    bool border = dx == x || dy == y || dx == x + w - 1 || dy == y + h - 1;
                    char glyph = border ? '#' : '.';
                    Color color = border ? Color.SaddleBrown : Color.Green;
                    zone.Tilemap.SetTile(dx, dy, new ColoredGlyph { Glyph = glyph, Foreground = color });
                }

            // Place door
            zone.Tilemap.SetTile(doorX, doorY, new ColoredGlyph { Glyph = '+', Foreground = Color.Yellow });

            return new Building { Bounds = rect, Door = doorPoint };
        }



        // ---------------- ROADS ----------------

        private static readonly Point[] Directions = { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };

        private static void CarveAStarRoad(Zone zone, Point start, Point goal)
        {
            var open = new SortedSet<(int fScore, int tieBreak, Point pt)>(Comparer<(int fScore, int tieBreak, Point pt)>.Create((a, b) =>
            {
                int cmp = a.fScore.CompareTo(b.fScore);
                if (cmp == 0) cmp = a.tieBreak.CompareTo(b.tieBreak);
                if (cmp == 0) cmp = a.pt.X.CompareTo(b.pt.X);
                if (cmp == 0) cmp = a.pt.Y.CompareTo(b.pt.Y);
                return cmp;
            }));

            var gScore = new Dictionary<Point, int> { [start] = 0 };
            var cameFrom = new Dictionary<Point, Point>();
            int counter = 0;
            open.Add((Heuristic(start, goal), counter++, start));
            HashSet<Point> closed = new HashSet<Point>();

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);
                Point pt = current.pt;

                if (pt == goal) break;
                closed.Add(pt);

                foreach (var dir in Directions)
                {
                    Point next = pt + dir;
                    if (next.X < 0 || next.Y < 0 || next.X >= zone.Width || next.Y >= zone.Height) continue;
                    if (closed.Contains(next)) continue;

                    var tile = zone.Tilemap.GetTile(next.X, next.Y);
                    int cost;

                    if (tile.Glyph == '#') continue;    // cannot pass through walls
                    else if (tile.Glyph == '=') cost = 1; // existing road preferred
                    else cost = 3;                      // grass/empty cost a bit more

                    int tentativeG = gScore[pt] + cost;
                    if (!gScore.ContainsKey(next) || tentativeG < gScore[next])
                    {
                        gScore[next] = tentativeG;
                        cameFrom[next] = pt;
                        open.Add((tentativeG + Heuristic(next, goal), counter++, next));
                    }
                }
            }

            // Retrace path
            Point cur = goal;
            while (cameFrom.ContainsKey(cur))
            {
                var tile = zone.Tilemap.GetTile(cur.X, cur.Y);
                if (tile.Glyph == '.' || tile.Glyph == '#')
                    zone.Tilemap.SetTile(cur.X, cur.Y, RoadTile());
                cur = cameFrom[cur];
            }
        }


        private static int Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static ColoredGlyph RoadTile() => new ColoredGlyph { Glyph = '=', Foreground = Color.Gray };

        // ---------------- WALL ----------------

        private static void DrawPerimeterWall(Zone zone, List<Building> buildings, int padding)
        {
            // Find outermost extents of buildings
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var b in buildings)
            {
                minX = Math.Min(minX, b.Bounds.MinExtentX);
                minY = Math.Min(minY, b.Bounds.MinExtentY);
                maxX = Math.Max(maxX, b.Bounds.MaxExtentX);
                maxY = Math.Max(maxY, b.Bounds.MaxExtentY);
            }

            // Expand by padding radius
            minX = Math.Max(1, minX - padding);
            minY = Math.Max(1, minY - padding);
            maxX = Math.Min(zone.Width - 2, maxX + padding);
            maxY = Math.Min(zone.Height - 2, maxY + padding);

            // Draw continuous wall
            for (int x = minX; x <= maxX; x++)
            {
                zone.Tilemap.SetTile(x, minY, new ColoredGlyph { Glyph = '#', Foreground = Color.SaddleBrown });
                zone.Tilemap.SetTile(x, maxY, new ColoredGlyph { Glyph = '#', Foreground = Color.SaddleBrown });
            }
            for (int y = minY + 1; y < maxY; y++)
            {
                zone.Tilemap.SetTile(minX, y, new ColoredGlyph { Glyph = '#', Foreground = Color.SaddleBrown });
                zone.Tilemap.SetTile(maxX, y, new ColoredGlyph { Glyph = '#', Foreground = Color.SaddleBrown });
            }

            // Gates N/W/S/E aligned with center
            int centerX = (minX + maxX) / 2;
            int centerY = (minY + maxY) / 2;
            zone.Tilemap.SetTile(centerX, minY, RoadTile()); // North gate
            zone.Tilemap.SetTile(centerX, maxY, RoadTile()); // South gate
            zone.Tilemap.SetTile(minX, centerY, RoadTile()); // West gate
            zone.Tilemap.SetTile(maxX, centerY, RoadTile()); // East gate
        }

        private static void ConnectGatesToRoads(Zone zone, List<Building> buildings, int padding)
        {
            // Compute wall bounds (same as your wall)
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

            int centerX = (minX + maxX) / 2;
            int centerY = (minY + maxY) / 2;

            List<Point> gates = new()
            {
                new Point(centerX, minY), // North
                new Point(centerX, maxY), // South
                new Point(minX, centerY), // West
                new Point(maxX, centerY)  // East
            };

            // Find a suitable target inside village (town center or closest building door)
            Point villageCenter = new Point(zone.Width / 2, zone.Height / 2);

            foreach (var gate in gates)
            {
                // Carve a path from gate directly to village center using your A* function
                CarveAStarRoad(zone, gate, villageCenter);
            }
        }

        // ---------------- HELPERS ----------------

        private static void SetBackgroundGrass(Zone zone)
        {
            for (int x = 0; x < zone.Width; x++)
                for (int y = 0; y < zone.Height; y++)
                    zone.Tilemap.SetTile(x, y, new ColoredGlyph { Glyph = '.', Foreground = Color.Green });
        }
    }
}
