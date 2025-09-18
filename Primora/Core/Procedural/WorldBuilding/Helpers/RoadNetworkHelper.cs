using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding.Helpers
{
    internal static class RoadNetworkHelper
    {
        private static readonly Point[] _cardinalDirections =
        [
            new Point(-1, 0),
            new Point(1, 0),
            new Point(0, -1),
            new Point(0, 1)
        ];

        public static HashSet<Point> BuildRoadNetwork(List<Point> cities, float[] heightMap, int width, int height, Random random)
        {
            var roadPoints = new HashSet<Point>();
            var remainingCities = new List<Point>(cities);

            // Start by connecting a random city to the network
            var startCity = remainingCities[random.Next(remainingCities.Count)];
            var connectedCities = new HashSet<Point> { startCity };
            remainingCities.Remove(startCity);

            float[] terrainCosts = new float[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    terrainCosts[y * width + x] = HeightCost(new Point(x, y), heightMap, width);

            // Connect each remaining city to the network
            while (remainingCities.Count > 0)
            {
                Point cityToConnect = remainingCities[random.Next(remainingCities.Count)];
                Point closestConnectedCity = FindClosestCity(cityToConnect, connectedCities).Value;

                BuildRoad(cityToConnect, closestConnectedCity, roadPoints, terrainCosts, width, height, random);

                connectedCities.Add(cityToConnect);
                remainingCities.Remove(cityToConnect);
            }

            // Ensure full connectivity of all cities
            ConnectDisconnectedComponents(roadPoints, cities, width, height, terrainCosts, random);

            // Remove dead-end roads safely
            var hashSetCities = cities.ToHashSet();
            RemoveDeadEnds(roadPoints, hashSetCities, width, height);

            // Remove redundant roads while preserving connectivity
            RemoveRedundantRoadsSafe(roadPoints, hashSetCities, width, height);

            return roadPoints;
        }

        // Ensures any disconnected component is linked to the main network
        private static void ConnectDisconnectedComponents(HashSet<Point> roadPoints, List<Point> cities, int width, int height, float[] terrainCosts, Random random)
        {
            var components = GetConnectedComponentsIncludingCities(roadPoints, cities, width, height);
            if (components.Count <= 1) return;

            var mainComponent = components.OrderByDescending(c => c.Count).First();
            foreach (var component in components.Where(c => c != mainComponent))
            {
                var (pA, pB) = FindClosestPointBetweenSets(component, mainComponent);
                BuildRoad(pA, pB, roadPoints, terrainCosts, width, height, random);
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
        private static void RemoveDeadEnds(HashSet<Point> roadPoints, HashSet<Point> cities, int width, int height)
        {
            int deadEndIterations = 0;
            int maxDeadEndIterations = width * height;
            bool removed;
            do
            {
                deadEndIterations++;
                if (deadEndIterations > maxDeadEndIterations) break;

                removed = false;
                var deadEnds = new HashSet<Point>();
                foreach (var p in roadPoints)
                {
                    if (!cities.Contains(p) && IsDeadEnd(cities, p, roadPoints))
                        deadEnds.Add(p);
                }

                foreach (var deadEnd in deadEnds)
                {
                    roadPoints.Remove(deadEnd);
                    removed = true;
                }
            } while (removed);
        }

        // Safe redundant road removal using BFS
        private static void RemoveRedundantRoadsSafe(HashSet<Point> roadPoints, HashSet<Point> cities, int width, int height)
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
        private static bool AllCitiesConnected(HashSet<Point> roadPoints, HashSet<Point> cities, int width, int height)
        {
            if (cities.Count == 0) return true;

            var visited = new HashSet<Point>();
            var queue = new Queue<Point>();
            var firstCity = cities.First();
            queue.Enqueue(firstCity);
            visited.Add(firstCity);

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

        private static void BuildRoad(Point start, Point end, HashSet<Point> roadPoints, float[] terrainCosts, int width, int height, Random random)
        {
            var openSet = new PriorityQueue<Point, float>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, float> { [start] = 0f };
            int maxIterations = width * height * 10;
            int iterations = 0;

            openSet.Enqueue(start, 0f);

            while (openSet.Count > 0)
            {
                iterations++;
                if (iterations > maxIterations) break;

                var current = openSet.Dequeue();

                if (current == end) break;

                foreach (var neighbor in GetNeighbors(current, width, height))
                {
                    float cost = GetTileCostWithOptionalBridges(current, neighbor, width, height, roadPoints, terrainCosts, random);

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
                path.Insert(0, temp);
                if (!cameFrom.TryGetValue(temp, out temp)) break;
            }
            path.Insert(0, start);

            foreach (var p in path)
                roadPoints.Add(p);
        }

        // Adjusted tile cost function
        private static float GetTileCostWithOptionalBridges(Point from, Point to, int width, int height, HashSet<Point> existingRoads, float[] terrainCosts, Random random)
        {
            float baseCost;
            float toHeightCost = terrainCosts[Point.ToIndex(to.X, to.Y, width)];
            float fromHeightCost = terrainCosts[Point.ToIndex(from.X, from.Y, width)];

            if (IsRiver(to))
            {
                // Compute a feasible bridge from 'from' to 'to'
                var bridgePath = ComputeBridgePath(to, from, width, height);
                if (bridgePath != null && bridgePath.Contains(to))
                {
                    baseCost = 1.5f; // bridge tiles are moderate cost
                }
                else
                {
                    baseCost = 10f; // detour needed, very expensive
                }
            }
            else
            {
                baseCost = toHeightCost;
            }

            // Slope & terrain
            float slope = Math.Abs(toHeightCost - fromHeightCost);
            float valleyBonus = 0.5f + 0.5f * (float)Math.Exp(-slope);
            baseCost /= valleyBonus;

            float terrainPreference = 1 - Math.Max(0, (toHeightCost - 1) * 0.5f);
            baseCost /= terrainPreference;

            // Road-following bonus
            if (existingRoads.Contains(to)) baseCost *= 0.5f;
            if (existingRoads.Contains(from) && !existingRoads.Contains(to)) baseCost += 0.25f;

            int nearbyRoads = GetNearbyRoadCount(to, existingRoads, width, height, 5);
            baseCost *= 1f - Math.Clamp(0.15f * nearbyRoads, 0, 0.6f);

            float nearestRoadDistSq = DistanceToNearestRoadSquared(to, existingRoads, width, height, 5);
            float roadBonus = nearestRoadDistSq == float.MaxValue ? 0f : Math.Clamp(1f / (nearestRoadDistSq + 1), 0, 0.5f);
            baseCost *= 1f - roadBonus;

            baseCost *= BorderPenalty(to, width, height, 10);

            // Organic randomness
            baseCost *= 1 + (float)(random.NextDouble() * 0.2 - 0.1);

            return baseCost;
        }

        // Compute bridge path (straight line, can be extended for diagonals)
        private static List<Point> ComputeBridgePath(Point riverTile, Point target, int width, int height)
        {
            var path = new List<Point>();

            int dirX = Math.Sign(target.X - riverTile.X);
            if (dirX != 0)
            {
                int x = riverTile.X;
                int y = riverTile.Y;
                while (x >= 0 && x < width && IsRiver(new Point(x, y)))
                {
                    path.Add(new Point(x, y));
                    x += dirX;
                }
                if (x >= 0 && x < width) return path;
                path.Clear();
            }

            int dirY = Math.Sign(target.Y - riverTile.Y);
            if (dirY != 0)
            {
                int x = riverTile.X;
                int y = riverTile.Y;
                while (y >= 0 && y < height && IsRiver(new Point(x, y)))
                {
                    path.Add(new Point(x, y));
                    y += dirY;
                }
                if (y >= 0 && y < height) return path;
            }

            return null; // no bridge found
        }

        // Compute a simple straight bridge path from river tile to nearest land in direction of target
        // Heuristic: Manhattan distance
        private static float Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static float BorderPenalty(Point p, int width, int height, int radius = 5)
        {
            int distLeft = p.X;
            int distRight = width - 1 - p.X;
            int distTop = p.Y;
            int distBottom = height - 1 - p.Y;

            int minDist = Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));

            if (minDist >= radius)
                return 1f; // no penalty
            else
            {
                // Linear penalty: closer to edge → higher multiplier
                float factor = 1f + (radius - minDist) * 0.5f; // adjust 0.5f for strength
                return factor;
            }
        }

        // Count how many existing road tiles are within a given radius
        private static int GetNearbyRoadCount(Point p, HashSet<Point> existingRoads, int width, int height, int radius)
        {
            int count = 0;
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = p.X + dx;
                if (nx < 0 || nx >= width) continue;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    int ny = p.Y + dy;
                    if (ny < 0 || ny >= height) continue;
                    if (dx == 0 && dy == 0) continue;

                    // Use Point struct without allocation
                    if (existingRoads.Contains(new Point(nx, ny)))
                        count++;
                }
            }
            return count;
        }

        private static float DistanceToNearestRoadSquared(Point p, HashSet<Point> existingRoads, int width, int height, int maxRadius)
        {
            int closestSq = int.MaxValue;

            for (int dx = -maxRadius; dx <= maxRadius; dx++)
            {
                for (int dy = -maxRadius; dy <= maxRadius; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = p.X + dx;
                    int ny = p.Y + dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (existingRoads.Contains(new Point(nx, ny)))
                        {
                            int distSq = dx * dx + dy * dy;
                            if (distSq < closestSq) closestSq = distSq;
                        }
                    }
                }
            }

            return closestSq == int.MaxValue ? float.MaxValue : closestSq; // **return squared distance**
        }

        private static bool IsRiver(Point p) => World.Instance.WorldMap.GetTileInfo(p).Biome == Objects.Biome.River;

        // Cardinal neighbors only
        private static IEnumerable<Point> GetNeighbors(Point p, int width, int height)
        {
            foreach (var dir in _cardinalDirections)
            {
                int nx = p.X + dir.X;
                int ny = p.Y + dir.Y;
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

        private static bool IsDeadEnd(HashSet<Point> cityPoints, Point position, HashSet<Point> road)
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
    }
}
