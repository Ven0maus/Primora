using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using PointF = System.Drawing.PointF;

namespace Primora.Core
{
    /// <summary>
    /// Contains all methods to handle generation of the world layers.
    /// </summary>
    internal class WorldGeneration(World world)
    {
        private readonly World _world = world;

        public void GenerateGrounds()
        {
            var noiseMap = GenerateNoiseMap(_world.Width, _world.Height, 1337, 23f, 3, 0.37f, 0.9f, PointF.Empty);
            // TODO: Generate noise-like grass ground
            for (int x=0; x < _world.Width; x++)
            {
                for (int y=0; y < _world.Height; y++)
                {
                    var noise = noiseMap[Point.ToIndex(x, y, _world.Width)];
                    if (noise < 0.65)
                    {
                        _world.TileGrid.SetTile(x, y, 0, false);
                    }
                    else
                    {
                        _world.TileGrid.SetTile(x, y, 1, false);
                    }
                }
            }
        }

        public void GenerateFauna()
        {
            // Generate forests in forest areas
            int forestCount = (int)Math.Ceiling(_world.Width * _world.Height / 130d);
            int forestDensity = 50; // tweak for forest density

            for (int i = 0; i < forestCount; i++)
            {
                int startX = Constants.General.Random.Next(0, _world.Width);
                int startY = Constants.General.Random.Next(0, _world.Height);
                var tile = _world.TileGrid.GetTile(startX, startY);

                int count = 0;
                while (tile.Id != 1)
                {
                    if (count >= 500)
                    {
                        break;
                    }
                    count++;
                    startX = Constants.General.Random.Next(0, _world.Width);
                    startY = Constants.General.Random.Next(0, _world.Height);
                    tile = _world.TileGrid.GetTile(startX, startY);
                }
                if (count >= 500) break;

                int clusterSize = Constants.General.Random.Next(10, forestDensity);
                GrowCluster(startX, startY, clusterSize, 3, 1);
            }

            // Generate rocks in grasslands
            int rockCount = (int)Math.Ceiling(_world.Width * _world.Height / 200d);
            int rockDensity = 20; // tweak for rock density

            for (int i = 0; i < rockCount; i++)
            {
                int startX = Constants.General.Random.Next(0, _world.Width);
                int startY = Constants.General.Random.Next(0, _world.Height);
                var tile = _world.TileGrid.GetTile(startX, startY);

                int count = 0;
                while (tile.Id != 0)
                {
                    if (count >= 500)
                    {
                        break;
                    }
                    count++;
                    startX = Constants.General.Random.Next(0, _world.Width);
                    startY = Constants.General.Random.Next(0, _world.Height);
                    tile = _world.TileGrid.GetTile(startX, startY);
                }
                if (count >= 500) break;

                int clusterSize = Constants.General.Random.Next(5, rockDensity);
                GrowCluster(startX, startY, clusterSize, 4, 0);
            }

            // Generate berry bushes on grasslands
            var bushesCount = (int)Math.Ceiling(_world.Width * _world.Height / 100d * 7);
            for (int i=0; i < bushesCount; i++)
            {
                int x = 0, y = 0, counter = 0;
                var tile = _world.TileGrid.GetTile(x, y);
                while (tile.Id != 0)
                {
                    x = Constants.General.Random.Next(0, _world.Width);
                    y = Constants.General.Random.Next(0, _world.Height);
                    tile = _world.TileGrid.GetTile(x, y);
                    counter++;
                    if (counter >= 500) 
                        break;
                }

                if (counter >= 500) 
                    break;

                _world.TileGrid.SetTile(x, y, 2, true);
            }
        }

        public void GenerateTribes()
        {
            // TODO: Spawn player and AI hut, and define a way to set a unique color for player and AI
            //_world.TileGrid.SetTile(_world.Width / 2, _world.Height / 2, 5, true);
        }

        private void GrowCluster(int x, int y, int clusterSize, int tileId, int onlyOnTileId)
        {
            var toVisit = new Queue<(int X, int Y)>();
            var visited = new HashSet<(int, int)>();

            toVisit.Enqueue((x, y));

            while (toVisit.Count > 0 && clusterSize > 0)
            {
                var (cx, cy) = toVisit.Dequeue();

                if (!_world.TileGrid.InBounds(cx, cy)) continue;
                if (!visited.Add((cx, cy))) continue; // skip already processed
                var tile = _world.TileGrid.GetTile(cx, cy);
                if (tile.Id != onlyOnTileId) continue; // Skip when not on "onlyOnTileId"

                _world.TileGrid.SetTile(cx, cy, tileId, true);
                clusterSize--;

                // Randomly push neighbors to spread
                foreach (var (X, Y) in GetRandomizedDirections())
                {
                    if (Constants.General.Random.NextDouble() < 0.6) // ~60% chance to spread
                        toVisit.Enqueue((cx + X, cy + Y));
                }
            }
        }

        private static readonly (int X, int Y)[] _directions =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        private static IEnumerable<(int X, int Y)> GetRandomizedDirections()
        {
            return _directions.OrderBy(_ => Constants.General.Random.Next());
        }

        public static float[] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, PointF offset)
        {
            float[] noiseMap = new float[mapWidth * mapHeight];

            var random = new Random(seed);

            // We need atleast one octave
            if (octaves < 1)
            {
                octaves = 1;
            }

            PointF[] octaveOffsets = new PointF[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + offset.X;
                float offsetY = random.Next(-100000, 100000) + offset.Y;
                octaveOffsets[i] = new PointF(offsetX, offsetY);
            }

            if (scale <= 0f)
            {
                scale = 0.0001f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // When changing noise scale, it zooms from top-right corner
            // This will make it zoom from the center
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int x = 0, y; x < mapWidth; x++)
            {
                for (y = 0; y < mapHeight; y++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].X;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].Y;

                        // Use unity's implementation of perlin noise
                        float perlinValue = OpenSimplex2S.Noise2(seed, sampleX, sampleY) * 2 - 1;

                        noiseHeight += perlinValue * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight)
                        minNoiseHeight = noiseHeight;

                    noiseMap[y * mapWidth + x] = noiseHeight;
                }
            }

            for (int x = 0, y; x < mapWidth; x++)
            {
                for (y = 0; y < mapHeight; y++)
                {
                    // Returns a value between 0f and 1f based on noiseMap value
                    // minNoiseHeight being 0f, and maxNoiseHeight being 1f
                    noiseMap[y * mapWidth + x] = InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[y * mapWidth + x]);
                }
            }
            return noiseMap;
        }

        private static float InverseLerp(float a, float b, float value)
        {
            if (a < b)
            {
                if (value <= a) return 0f;
                if (value >= b) return 1f;
            }
            else
            {
                if (value <= b) return 1f;
                if (value >= a) return 0f;
            }

            return (value - a) / (b - a);
        }
    }
}
