using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding
{
    /// <summary>
    /// Contains all accessible zones in the world.
    /// </summary>
    internal class WorldMap
    {
        private readonly int _width, _height;
        private readonly Dictionary<Point, Zone> _zones;

        internal readonly Tilemap Tilemap;

        internal WorldMap(int width, int height)
        {
            _width = width;
            _height = height;
            _zones = [];

            Tilemap = new Tilemap(width, height);
        }

        internal void Generate()
        {
            var globalRand = Constants.General.Random;
            var heightmap = OpenSimplex.GenerateNoiseMap(_width, _height,
                seed: globalRand.Next(),
                scale: 100f,
                octaves: 2,
                persistance: 0.5f,
                lacunarity: 2.0f);

            // Get sorted biomes by NoiseLevel
            var biomes = BiomeRegistry.GetBiomesByNoise(); // Assumes ascending NoiseLevel

            // Step 1: Initial variation
            int[,] variationMap = new int[_width, _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    variationMap[x, y] = globalRand.Next(0, 31); // 0..30
                }
            }

            // Step 2: Smooth with CA-style blending
            for (int pass = 0; pass < 3; pass++)
            {
                int[,] newMap = new int[_width, _height];

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int sum = 0, count = 0;

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;

                                sum += variationMap[nx, ny];
                                count++;
                            }
                        }

                        int avg = sum / count;

                        // Blend self + neighbor average (don’t overwrite completely!)
                        newMap[x, y] = (int)(variationMap[x, y] * 0.6f + avg * 0.4f);
                    }
                }

                variationMap = newMap; // overwrite with smoothed version
            }

            // Step 3: Apply to tiles
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int i = Point.ToIndex(x, y, _width);
                    float height = heightmap[i];

                    Biome selectedBiome = Biome.Grassland;
                    foreach (var biomeDef in biomes)
                    {
                        if (height <= biomeDef.NoiseLevel)
                        {
                            selectedBiome = biomeDef.Biome;
                            break;
                        }
                    }

                    var appearance = BiomeRegistry.Get(selectedBiome).Appearance.Clone();
                    var baseColor = appearance.Background;

                    // Shift variation to be -15..+15
                    int variation = variationMap[x, y] - 15;

                    int r = Math.Clamp(baseColor.R + variation, 0, 255);
                    int g = Math.Clamp(baseColor.G + variation, 0, 255);
                    int b = Math.Clamp(baseColor.B + variation, 0, 255);

                    appearance.Background = new SadRogue.Primitives.Color(r, g, b);
                    Tilemap.SetTile(x, y, (ColoredGlyph)appearance);
                }
            }
        }
    }
}
