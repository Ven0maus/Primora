using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

internal static class RiverNetworkHelper
{
    public static HashSet<Point> BuildMajorRiver(
        float[] heightMap,
        int width,
        int height,
        Random random,
        int minLength = 50,
        int maxAttempts = 10)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var river = new HashSet<Point>();

            // 1. Pick a source (highland)
            var highCandidates = GetPointsAbove(heightMap, width, height, 0.75f);
            if (highCandidates.Count == 0) return river;
            var source = highCandidates[random.Next(highCandidates.Count)];

            // 2. Pick an outlet far away from source
            Point outlet = PickFarOutlet(source, width, height, random);

            // 3. Trace river
            TraceMajorRiver(source, outlet, heightMap, width, height, random, river);

            // 4. Accept only if long enough
            if (river.Count >= minLength)
                return river;
        }

        // fallback: return empty if no good river found
        return [];
    }

    private static Point PickFarOutlet(Point source, int width, int height, Random random)
    {
        // choose an edge farthest from source
        var candidates = new List<Point>();
        for (int x = 0; x < width; x++)
        {
            candidates.Add(new Point(x, 0));
            candidates.Add(new Point(x, height - 1));
        }
        for (int y = 0; y < height; y++)
        {
            candidates.Add(new Point(0, y));
            candidates.Add(new Point(width - 1, y));
        }

        // maximize distance from source
        return candidates
            .OrderByDescending(p => (source.X - p.X) * (source.X - p.X) + (source.Y - p.Y) * (source.Y - p.Y))
            .Take(5) // pick one of the farthest few, so it's not always the same edge
            .ElementAt(random.Next(5));
    }

    private static void TraceMajorRiver(
        Point start,
        Point outlet,
        float[] heightMap,
        int width,
        int height,
        Random random,
        HashSet<Point> river)
    {
        Point current = start;
        var visited = new HashSet<Point>();

        for (int steps = 0; steps < width + height; steps++) // cutoff
        {
            if (!visited.Add(current)) break;
            river.Add(current);

            if (current == outlet ||
                current.X == 0 || current.Y == 0 ||
                current.X == width - 1 || current.Y == height - 1)
                break; // reached destination

            // Bias next step toward outlet but also downhill
            var neighbors = GetNeighbors(current, width, height).ToList();

            // Sort neighbors by two factors:
            // 1. Height (lower = better)
            // 2. Distance to outlet (closer = better)
            neighbors = neighbors
                .OrderBy(n =>
                {
                    float h = heightMap[n.Y * width + n.X];
                    float dist = DistanceSquared(n, outlet);
                    return h * 2 + dist * 0.001f; // tweak weights
                })
                .ToList();

            Point next = neighbors.First();

            current = next;
        }
    }

    private static float DistanceSquared(Point a, Point b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

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

    private static List<Point> GetPointsAbove(float[] heightMap, int width, int height, float threshold)
    {
        var list = new List<Point>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (heightMap[y * width + x] >= threshold)
                    list.Add(new Point(x, y));
        return list;
    }
}