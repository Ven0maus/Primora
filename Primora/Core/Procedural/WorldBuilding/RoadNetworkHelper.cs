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
            Point current = start;

            while (current != end)
            {
                roadPoints.Add(current);

                // Bridge logic
                if (IsRiver(current))
                {
                    var bridgeEnd = FindBestBridge(current, end, width, height);
                    if (bridgeEnd != null && bridgeEnd.Value != current)
                    {
                        foreach (var tile in WalkStraight(current, bridgeEnd.Value))
                        {
                            roadPoints.Add(tile);
                            current = tile;
                        }
                        continue;
                    }
                }

                Point next = ChooseNextTileWithCostVariation(current, end, heightMap, width, random);

                // Safety check
                if (next == current) break;

                current = next;
            }

            roadPoints.Add(end);
        }

        private static IEnumerable<Point> WalkStraight(Point start, Point end)
        {
            int dx = Math.Sign(end.X - start.X);
            int dy = Math.Sign(end.Y - start.Y);

            // Only horizontal or vertical allowed
            if (dx != 0 && dy != 0)
                throw new InvalidOperationException("Bridge must be straight along one axis");

            Point current = start;

            while (current != end)
            {
                current = new Point(current.X + dx, current.Y + dy);
                yield return current;
            }
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

        private static Point? FindBestBridge(Point current, Point end, int width, int height)
        {
            var candidates = new List<(Point endTile, int length)>();

            // Horizontal bridge
            int y = current.Y;
            int dirX = Math.Sign(end.X - current.X);
            if (dirX != 0)
            {
                int x = current.X;
                int length = 0;
                // Move along river tiles
                while (x >= 0 && x < width && IsRiver(new Point(x, y)))
                {
                    length++;
                    x += dirX;
                }

                // x is now first land tile after river (opposite bank)
                if (length > 0 && x >= 0 && x < width)
                    candidates.Add((new Point(x, y), length));
            }

            // Vertical bridge
            int x2 = current.X;
            int dirY = Math.Sign(end.Y - current.Y);
            if (dirY != 0)
            {
                int y2 = current.Y;
                int length = 0;
                while (y2 >= 0 && y2 < height && IsRiver(new Point(x2, y2)))
                {
                    length++;
                    y2 += dirY;
                }

                if (length > 0 && y2 >= 0 && y2 < height)
                    candidates.Add((new Point(x2, y2), length));
            }

            if (candidates.Count == 0)
                return null;

            // Choose the shortest bridge
            var best = candidates.OrderBy(c => c.length).First();

            // Make sure it actually moves forward
            if (best.endTile == current) return null;

            return best.endTile; // now guaranteed to be the first land tile on opposite bank
        }

        private static Point ChooseNextTileWithCostVariation(Point current, Point end, float[] heightMap, int width, Random random)
        {
            var neighbors = new List<Point>();

            int dx = end.X - current.X;
            int dy = end.Y - current.Y;

            if (dx != 0) neighbors.Add(new Point(current.X + Math.Sign(dx), current.Y));
            if (dy != 0) neighbors.Add(new Point(current.X, current.Y + Math.Sign(dy)));

            var weightedNeighbors = new List<(Point point, float weight)>();
            foreach (var n in neighbors)
            {
                float cost = GetTileCost(n, heightMap, width);
                float variation = (float)(random.NextDouble() * 0.2); // small random tweak
                float weight = (float)Math.Exp(-cost) * (1 + variation); // cheaper tiles get exponentially higher weight
                weightedNeighbors.Add((n, weight));
            }

            float totalWeight = weightedNeighbors.Sum(w => w.weight);
            float pick = (float)(random.NextDouble() * totalWeight);

            float cumulative = 0f;
            foreach (var w in weightedNeighbors)
            {
                cumulative += w.weight;
                if (pick <= cumulative)
                    return w.point;
            }

            return current; // fallback
        }

        private static float GetTileCost(Point p, float[] heightMap, int width)
        {
            float cost = HeightCost(p, heightMap, width);
            if (IsRiver(p)) cost *= 5f; // penalize river tiles heavily
            return cost;
        }
    }
}
