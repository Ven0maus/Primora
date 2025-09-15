using Primora.Core.Procedural.Common;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class RoadNetworkHelper
    {
        public static HashSet<Point> BuildRoadNetwork(List<Point> cities, float[] heightMap, int width, int height, Random random)
        {
            var roadPoints = new HashSet<Point>();
            var remainingCities = new List<Point>(cities);

            // Start by connecting a random city to the network
            var startCity = remainingCities[random.Next(remainingCities.Count)];
            var connectedCities = new HashSet<Point> { startCity };
            remainingCities.Remove(startCity);

            // Connect each remaining city to the network
            while (remainingCities.Count > 0)
            {
                Point cityToConnect = remainingCities[random.Next(remainingCities.Count)];
                Point closestConnectedCity = FindClosestCity(cityToConnect, connectedCities).Value;

                BuildRoad(cityToConnect, closestConnectedCity, roadPoints, heightMap, width, height, random);

                connectedCities.Add(cityToConnect);
                remainingCities.Remove(cityToConnect);
            }

            // Ensure full connectivity of all cities
            ConnectDisconnectedComponents(roadPoints, cities, width, height, heightMap, random);

            // Remove dead-end roads safely
            RemoveDeadEnds(roadPoints, cities);

            // Remove redundant roads while preserving connectivity
            RemoveRedundantRoadsSafe(roadPoints, cities, width, height);

            return roadPoints;
        }

        // Ensures any disconnected component is linked to the main network
        private static void ConnectDisconnectedComponents(HashSet<Point> roadPoints, List<Point> cities, int width, int height, float[] heightMap, Random random)
        {
            var components = GetConnectedComponentsIncludingCities(roadPoints, cities, width, height);
            if (components.Count <= 1) return;

            var mainComponent = components.OrderByDescending(c => c.Count).First();
            foreach (var component in components.Where(c => c != mainComponent))
            {
                var (pA, pB) = FindClosestPointBetweenSets(component, mainComponent);
                BuildRoad(pA, pB, roadPoints, heightMap, width, height, random);
            }
        }

        // BFS that includes cities
        private static List<HashSet<Point>> GetConnectedComponentsIncludingCities(HashSet<Point> roadPoints, List<Point> cities, int width, int height)
        {
            var visited = new HashSet<Point>();
            var allPoints = new HashSet<Point>(roadPoints.Concat(cities));
            var components = new List<HashSet<Point>>();

            foreach (var start in allPoints)
            {
                if (visited.Contains(start)) continue;

                var queue = new Queue<Point>();
                var component = new HashSet<Point>();

                queue.Enqueue(start);
                component.Add(start);
                visited.Add(start);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    foreach (var neighbor in GetNeighbors(current, width, height))
                    {
                        if (allPoints.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            component.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                components.Add(component);
            }

            return components;
        }

        // Remove dead ends but preserve city connections
        private static void RemoveDeadEnds(HashSet<Point> roadPoints, List<Point> cities)
        {
            bool removed;
            do
            {
                removed = false;
                var deadEnds = roadPoints
                    .Where(p => !cities.Contains(p) && IsDeadEnd(cities, p, roadPoints))
                    .ToList();

                foreach (var deadEnd in deadEnds)
                {
                    roadPoints.Remove(deadEnd);
                    removed = true;
                }
            } while (removed);
        }

        // Safe redundant road removal using BFS
        private static void RemoveRedundantRoadsSafe(HashSet<Point> roadPoints, List<Point> cities, int width, int height)
        {
            var candidates = new Queue<Point>(roadPoints.Where(p => !cities.Contains(p)));
            while (candidates.Count > 0)
            {
                var p = candidates.Dequeue();
                if (!roadPoints.Contains(p)) continue;

                roadPoints.Remove(p);
                if (!AllCitiesConnected(roadPoints, cities, width, height))
                {
                    roadPoints.Add(p); // restore if removal breaks connectivity
                }
            }
        }

        // Updated BFS to traverse through roads and cities
        private static bool AllCitiesConnected(HashSet<Point> roadPoints, List<Point> cities, int width, int height)
        {
            if (cities.Count == 0) return true;

            var visited = new HashSet<Point>();
            var queue = new Queue<Point>();
            queue.Enqueue(cities[0]);
            visited.Add(cities[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in GetNeighbors(current, width, height))
                {
                    if (!visited.Contains(neighbor) && (roadPoints.Contains(neighbor) || cities.Contains(neighbor)))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return cities.All(c => visited.Contains(c));
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

        private static void BuildRoad(Point start, Point end, HashSet<Point> roadPoints, float[] heightMap, int width, int height, Random random)
        {
            var openSet = new PriorityQueue<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, float> { [start] = 0f };

            openSet.Enqueue(start, 0f);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == end)
                    break;

                foreach (var neighbor in GetNeighbors(current, width, height))
                {
                    // Use enhanced cost function that prefers existing roads & avoids rivers
                    float cost = GetTileCostWithBridges(current, neighbor, heightMap, width, height, roadPoints, end, random);

                    // Turn penalty for organic curves
                    if (cameFrom.TryGetValue(current, out var prev))
                    {
                        int dirX1 = current.X - prev.X;
                        int dirY1 = current.Y - prev.Y;
                        int dirX2 = neighbor.X - current.X;
                        int dirY2 = neighbor.Y - current.Y;

                        if (dirX1 != dirX2 || dirY1 != dirY2)
                            cost += 0.3f;
                    }

                    float tentativeG = gScore[current] + cost;

                    if (!gScore.TryGetValue(neighbor, out var existingG) || tentativeG < existingG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        // Heuristic with slight randomness for organic look
                        float heuristic = Heuristic(neighbor, end) * (1 + (float)(random.NextDouble() * 0.2 - 0.1));
                        openSet.Enqueue(neighbor, tentativeG + heuristic);
                    }
                }
            }

            // Reconstruct path
            var path = new List<Point>();
            var temp = end;
            while (temp != start)
            {
                path.Add(temp);
                if (!cameFrom.TryGetValue(temp, out temp)) break; // safety
            }
            path.Add(start);
            path.Reverse();

            foreach (var p in path)
                roadPoints.Add(p);
        }

        // Heuristic: Manhattan distance
        private static float Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        // Cost function that prefers land but allows bridges
        private static float GetTileCostWithBridges(
            Point from,
            Point to,
            float[] heightMap,
            int width,
            int height,
            HashSet<Point> existingRoads,
            Point goal,
            Random random)
        {
            // 1️⃣ Base terrain cost
            float baseCost = HeightCost(to, heightMap, width);

            // 2️⃣ River avoidance first
            if (IsRiver(to))
            {
                var bridgeEnd = FindBestBridge(to, goal, width, height, heightMap);
                if (bridgeEnd != null)
                {
                    // Crossing via bridge is allowed, cheaper than swimming
                    baseCost = Math.Min(baseCost * 5f, 1.5f);
                }
                else
                {
                    // No bridge, very expensive
                    baseCost *= 10f;
                }
            }

            // 3️⃣ Gentle slope preference (valleys)
            float slope = Math.Abs(HeightCost(to, heightMap, width) - HeightCost(from, heightMap, width));
            float valleyBonus = 0.5f + 0.5f * (float)Math.Exp(-slope);
            baseCost /= valleyBonus;

            // 4️⃣ Terrain preference (lowlands preferred)
            float terrainPreference = 1 - Math.Max(0, (HeightCost(to, heightMap, width) - 1) * 0.5f);
            baseCost /= terrainPreference;

            // 5️⃣ Existing road bonus
            if (existingRoads.Contains(to))
                baseCost *= 0.5f; // stepping on road is cheaper
            if (existingRoads.Contains(from) && !existingRoads.Contains(to))
                baseCost += 0.25f; // leaving road is slightly more expensive

            // 6️⃣ Proximity to existing roads
            float nearestRoadDist = DistanceToNearestRoad(to, existingRoads);
            baseCost *= 1f - Math.Clamp(0.5f / (nearestRoadDist + 1), 0, 0.5f);

            // 7️⃣ Directional bias toward goal
            float dx = Math.Abs(to.X - goal.X);
            float dy = Math.Abs(to.Y - goal.Y);
            float distToGoal = dx + dy;
            baseCost *= 1 + distToGoal * 0.01f; // small bias to encourage forward progress

            // 8️⃣ Slight random variation for organic look
            baseCost *= 1 + (float)(random.NextDouble() * 0.1 - 0.05);

            return baseCost;
        }

        private static float DistanceToNearestRoad(Point p, HashSet<Point> existingRoads)
        {
            if (existingRoads.Count == 0)
                return float.MaxValue; // no roads yet

            float minDistSq = float.MaxValue;

            foreach (var roadPoint in existingRoads)
            {
                // Squared distance for efficiency
                float dx = p.X - roadPoint.X;
                float dy = p.Y - roadPoint.Y;
                float distSq = dx * dx + dy * dy;

                if (distSq < minDistSq)
                    minDistSq = distSq;
            }

            // Return Euclidean distance
            return (float)Math.Sqrt(minDistSq);
        }

        private static bool IsRiver(Point p) => World.Instance.WorldMap.GetTileInfo(p).Biome == Objects.Biome.River;

        // Cardinal neighbors only
        private static IEnumerable<Point> GetNeighbors(Point p, int width, int height)
        {
            int[] dx = [-1, 1, 0, 0];
            int[] dy = [0, 0, -1, 1];

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

        private static Point? FindBestBridge(Point start, Point end, int width, int height, float[] heightMap)
        {
            var candidates = new List<(Point endTile, float totalCost)>();

            // Try horizontal bridge
            int dirX = Math.Sign(end.X - start.X);
            if (dirX != 0)
            {
                int x = start.X;
                int y = start.Y;
                float cost = 0;
                while (x >= 0 && x < width && IsRiver(new Point(x, y)))
                {
                    cost += 1f; // base river crossing cost
                    x += dirX;
                }

                if (x >= 0 && x < width)
                {
                    // Add land cost on opposite bank
                    cost += HeightCost(new Point(x, y), heightMap, width);
                    candidates.Add((new Point(x, y), cost));
                }
            }

            // Try vertical bridge
            int dirY = Math.Sign(end.Y - start.Y);
            if (dirY != 0)
            {
                int x = start.X;
                int y = start.Y;
                float cost = 0;
                while (y >= 0 && y < height && IsRiver(new Point(x, y)))
                {
                    cost += 1f;
                    y += dirY;
                }

                if (y >= 0 && y < height)
                {
                    cost += HeightCost(new Point(x, y), heightMap, width);
                    candidates.Add((new Point(x, y), cost));
                }
            }

            if (candidates.Count == 0)
                return null;

            // Choose the bridge with **lowest total cost**, favoring land
            var (endTile, totalCost) = candidates.OrderBy(c => c.totalCost).First();

            return endTile;
        }
    }
}
