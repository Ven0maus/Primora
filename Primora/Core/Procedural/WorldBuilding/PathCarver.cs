using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class PathCarver
    {
        /// <summary>
        /// Defines the correct box-line style glyphs for the entire path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<(Point coordinate, int glyph)> DefineGlyphs(HashSet<Point> path)
        {
            var glyphs = new List<(Point coordinate, int glyph)>();
            foreach (var point in path)
            {
                // Check each neighbor to define the correct glyph for this point
                bool left = path.Contains(new Point(point.X - 1, point.Y));
                bool right = path.Contains(new Point(point.X + 1, point.Y));
                bool up = path.Contains(new Point(point.X, point.Y - 1));
                bool down = path.Contains(new Point(point.X, point.Y + 1));

                int glyph;

                // Decide glyph based on neighbors
                if (left && right && up && down) glyph = 197;        // ┼
                else if (left && right && up) glyph = 193;           // ┴
                else if (left && right && down) glyph = 194;         // ┬
                else if (up && down && left) glyph = 180;            // ┤
                else if (up && down && right) glyph = 195;           // ├
                else if (left && right) glyph = 196;                 // ─
                else if (up && down) glyph = 179;                    // │
                else if (down && right) glyph = 218;                 // ┌
                else if (down && left) glyph = 191;                  // ┐
                else if (up && right) glyph = 192;                   // └
                else if (up && left) glyph = 217;                    // ┘
                else if (left) glyph = 196;                          // lone horizontal
                else if (right) glyph = 196;
                else if (up) glyph = 179;
                else if (down) glyph = 179;
                else glyph = 250; // middle dot for isolated tile

                glyphs.Add((point, glyph));
            }
            return glyphs;
        }

        internal static void CarveRiver(Point start, float[] heightmap, int width, int height)
        {
            // Height based river carving
        }

        public static HashSet<Point> BuildRoadNetwork(List<Point> cities, float[] heightMap, int width, int height, Random random)
        {
            var roadPoints = new HashSet<Point>();
            var remainingCities = new List<Point>(cities);

            // Start by connecting a random city to the network
            var startCity = remainingCities[random.Next(remainingCities.Count)];
            var connectedCities = new HashSet<Point> { startCity };
            remainingCities.Remove(startCity);

            while (remainingCities.Count > 0)
            {
                Point cityToConnect = remainingCities[random.Next(remainingCities.Count)];
                Point? closestConnectedCity = FindClosestCity(cityToConnect, connectedCities);
                if (closestConnectedCity == null)
                    throw new Exception("No closest city found");

                BuildRoad(cityToConnect, closestConnectedCity.Value, cities, roadPoints, heightMap, width, height);
                connectedCities.Add(cityToConnect);
                remainingCities.Remove(cityToConnect);
            }

            // --- Ensure full connectivity ---

            // Step 1: Find connected components
            var components = GetConnectedComponents(roadPoints, width, height);
            if (components.Count > 1)
            {
                // Step 2: Designate the largest component as main
                var mainComponent = components.OrderByDescending(c => c.Count).First();
                var otherComponents = components.Where(c => c != mainComponent).ToArray();

                // Step 3: Connect each other component to the main
                foreach (var component in otherComponents)
                {
                    (Point pA, Point pB) = FindClosestPointBetweenSets(component, mainComponent);

                    BuildRoad(pA, pB, cities, roadPoints, heightMap, width, height);
                }
            }

            // Removes dead-end points
            var deadEnds = roadPoints.Where(a => IsDeadEnd(cities, a, roadPoints));
            foreach (var deadEnd in deadEnds)
                roadPoints.Remove(deadEnd);

            // Remove boxes (roads that create box like roads)
            RemoveRedundantRoads(roadPoints, cities, width, height);

            return roadPoints;
        }

        private static void RemoveRedundantRoads(HashSet<Point> roadPoints, List<Point> cities, int width, int height)
        {
            // Start with all non-city tiles as candidates
            var candidates = new Queue<Point>(roadPoints.Where(p => !cities.Contains(p)));
            var directions = new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

            while (candidates.Count > 0)
            {
                var p = candidates.Dequeue();

                // Skip if already removed
                if (!roadPoints.Contains(p)) continue;

                // Count neighbors
                var neighbors = directions
                    .Select(d => new Point(p.X + d.dx, p.Y + d.dy))
                    .Where(n => roadPoints.Contains(n))
                    .ToList();

                // Temporarily remove
                roadPoints.Remove(p);

                if (AllCitiesConnected(roadPoints, cities, width, height))
                {
                    // Tile removed successfully
                    // Add its neighbors as new candidates in case new redundancies appeared
                    foreach (var n in neighbors)
                    {
                        if (!cities.Contains(n))
                            candidates.Enqueue(n);
                    }
                }
                else
                {
                    // Restore if removing breaks connectivity
                    roadPoints.Add(p);
                }
            }
        }

        /// <summary>
        /// Checks if all cities are connected via the road network.
        /// </summary>
        private static bool AllCitiesConnected(HashSet<Point> roadPoints, List<Point> cities, int width, int height)
        {
            if (cities.Count == 0) return true;

            var visited = new HashSet<Point>();
            var queue = new Queue<Point>();

            // Start BFS from the first city
            queue.Enqueue(cities[0]);
            visited.Add(cities[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in GetNeighbors(current, width, height))
                {
                    // Only traverse through road points
                    if ((roadPoints.Contains(neighbor) || cities.Contains(neighbor)) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Check if all cities are visited
            return cities.All(city => visited.Contains(city));
        }

        // Helper: find all connected components
        private static List<HashSet<Point>> GetConnectedComponents(HashSet<Point> roadPoints, int width, int height)
        {
            var remaining = new HashSet<Point>(roadPoints);
            var components = new List<HashSet<Point>>();

            while (remaining.Count > 0)
            {
                var start = remaining.First();
                var queue = new Queue<Point>();
                var component = new HashSet<Point>();

                queue.Enqueue(start);
                component.Add(start);
                remaining.Remove(start);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    foreach (var neighbor in GetNeighbors(current, width, height))
                    {
                        if (remaining.Contains(neighbor))
                        {
                            remaining.Remove(neighbor);
                            component.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                components.Add(component);
            }

            return components;
        }

        // Helper: find closest point between two sets
        private static (Point a, Point b) FindClosestPointBetweenSets(HashSet<Point> setA, HashSet<Point> setB)
        {
            Point bestA = Point.Zero;
            Point bestB = Point.Zero;
            float minDist = float.MaxValue;

            foreach (var a in setA)
            {
                foreach (var b in setB)
                {
                    float dx = a.X - b.X;
                    float dy = a.Y - b.Y;
                    float dist = dx * dx + dy * dy;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestA = a;
                        bestB = b;
                    }
                }
            }

            return (bestA, bestB);
        }

        private static Point? FindClosestCity(Point city, HashSet<Point> connected)
        {
            Point? closest = null;
            float minDist = float.MaxValue;

            foreach (var c in connected)
            {
                float dx = city.X - c.X;
                float dy = city.Y - c.Y;
                float dist = dx * dx + dy * dy; // squared distance
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = c;
                }
            }

            return closest;
        }

        private static void BuildRoad(Point start, Point end, List<Point> cities, HashSet<Point> roadPoints, float[] heightMap, int width, int height)
        {
            var openSet = new PriorityQueue<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, float> { [start] = 0 };
            var fScore = new Dictionary<Point, float> { [start] = Heuristic(start, end) };
            var openSetPoints = new HashSet<Point> { start };

            openSet.Enqueue(start, fScore[start]);

            int maxIterations = width * height * 10;
            int iterations = 0;

            while (openSet.Count > 0 && iterations++ < maxIterations)
            {
                var current = openSet.Dequeue();
                openSetPoints.Remove(current);

                if (current == end)
                {
                    ReconstructPath(cameFrom, current, roadPoints);
                    return;
                }

                foreach (var neighbor in GetNeighbors(current, width, height)) // cardinal only
                {
                    bool isCity = cities.Contains(neighbor);
                    float heightCost = HeightCost(neighbor, heightMap, width);
                    float tentativeG = gScore[current] + Math.Max(1f, heightCost); // always at least 1

                    if (!isCity && roadPoints.Contains(neighbor))
                        tentativeG *= 0.9f; // prefer existing roads

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + Heuristic(neighbor, end); // normal A* heuristic
                        fScore[neighbor] = f;

                        if (!openSetPoints.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, f);
                            openSetPoints.Add(neighbor);
                        }
                    }
                }
            }

            // Always fallback if path not found
            StraightLinePath(start, end, roadPoints);
        }


        // Cardinal neighbors only
        private static IEnumerable<Point> GetNeighbors(Point p, int width, int height)
        {
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < dx.Length; i++)
            {
                int nx = p.X + dx[i];
                int ny = p.Y + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    yield return new Point(nx, ny);
            }
        }

        // Terrain cost helper
        private static float HeightCost(Point p, float[] heightMap, int width)
        {
            float h = heightMap[p.Y * width + p.X];
            return 1 + Math.Max(0, (h - 0.7f) * 10);
        }

        // Fallback straight-line path
        private static void StraightLinePath(Point start, Point end, HashSet<Point> roadPoints)
        {
            Point current = start;

            while (current.X != end.X)
            {
                roadPoints.Add(current);
                current = new Point(current.X + Math.Sign(end.X - current.X), current.Y);
            }

            while (current.Y != end.Y)
            {
                roadPoints.Add(current);
                current = new Point(current.X, current.Y + Math.Sign(end.Y - current.Y));
            }

            roadPoints.Add(end);
        }

        // Squared Euclidean distance heuristic
        private static float Heuristic(Point a, Point b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }


        private static bool IsDeadEnd(List<Point> cityPoints, Point position, HashSet<Point> road)
        {
            int neighborCount = 0;

            // Cardinal neighbors
            int[] dx = [-1, 1, 0, 0];
            int[] dy = [0, 0, -1, 1];

            bool hasCity = cityPoints.Contains(position);
            for (int i = 0; i < 4; i++)
            {
                Point neighbor = new(position.X + dx[i], position.Y + dy[i]);
                if (road.Contains(neighbor))
                {
                    if (cityPoints.Contains(neighbor))
                        hasCity = true;
                    neighborCount++;
                }
            }

            // A dead-end has exactly one connected neighbor
            return neighborCount == 1 && !hasCity;
        }

        private static void ReconstructPath(Dictionary<Point, Point> cameFrom, Point current, HashSet<Point> roadPoints)
        {
            var visited = new HashSet<Point>(); // prevent cycles

            while (cameFrom.ContainsKey(current) && !visited.Contains(current))
            {
                roadPoints.Add(current);
                visited.Add(current);
                current = cameFrom[current];
            }

            roadPoints.Add(current); // include the start point
        }
    }
}
