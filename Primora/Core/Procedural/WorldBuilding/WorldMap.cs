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

            var biomes = BiomeRegistry.GetBiomesByNoise(); // Assumes ascending NoiseLevel

            // ----------------------------
            // Step 0: Random noise variation map
            // ----------------------------
            int[,] variationMap = new int[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    variationMap[x, y] = globalRand.Next(-30, 31); // -30..30
                }
            }

            // Smooth the variation a few passes
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
                        newMap[x, y] = (int)(variationMap[x, y] * 0.55f + avg * 0.45f);
                    }
                }
                variationMap = newMap;
            }

            // ----------------------------
            // Step 1: Base color (height + noise variation)
            // ----------------------------
            var colorMap = new SadRogue.Primitives.Color[_width, _height];

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

                    var baseColor = BiomeRegistry.Get(selectedBiome).Appearance.Background;

                    // Height-based variation [-30..30]
                    int heightVariation = (int)((height - 0.5f) * 60f);
                    // Noise variation (already -30..30 from map)
                    int noiseVariation = variationMap[x, y];
                    int totalVariation = heightVariation + noiseVariation;

                    int r = Math.Clamp(baseColor.R + totalVariation, 0, 255);
                    int g = Math.Clamp(baseColor.G + totalVariation, 0, 255);
                    int b = Math.Clamp(baseColor.B + totalVariation, 0, 255);

                    colorMap[x, y] = new SadRogue.Primitives.Color(r, g, b);
                }
            }

            // ----------------------------
            // Step 2: Shading (directional light)
            // ----------------------------
            for (int y = 0; y < _height; y++)
            {
                for (int x = 1; x < _width; x++) // start at 1 so we can look left
                {
                    int i = Point.ToIndex(x, y, _width);
                    int leftIndex = Point.ToIndex(x - 1, y, _width);

                    float h = heightmap[i];
                    float hLeft = heightmap[leftIndex];

                    var c = colorMap[x, y];

                    // If left tile is lower → lighten, if higher → darken
                    int delta = (int)((hLeft - h) * 40f); // tweak 40 for stronger/weaker shading
                    int r = Math.Clamp(c.R + delta, 0, 255);
                    int g = Math.Clamp(c.G + delta, 0, 255);
                    int b = Math.Clamp(c.B + delta, 0, 255);

                    colorMap[x, y] = new SadRogue.Primitives.Color(r, g, b);
                }
            }

            // ----------------------------
            // Step 3: Smooth by blending with neighbors
            // ----------------------------
            var smoothed = new SadRogue.Primitives.Color[_width, _height];
            float smoothingFactor = 0.3f; // lower = crisper, higher = blurrier

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int sumR = 0, sumG = 0, sumB = 0, count = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;
                            var nc = colorMap[nx, ny];
                            sumR += nc.R;
                            sumG += nc.G;
                            sumB += nc.B;
                            count++;
                        }
                    }

                    int avgR = sumR / count;
                    int avgG = sumG / count;
                    int avgB = sumB / count;

                    var original = colorMap[x, y];

                    smoothed[x, y] = new SadRogue.Primitives.Color(
                        (int)(original.R * (1 - smoothingFactor) + avgR * smoothingFactor),
                        (int)(original.G * (1 - smoothingFactor) + avgG * smoothingFactor),
                        (int)(original.B * (1 - smoothingFactor) + avgB * smoothingFactor)
                    );
                }
            }

            // ----------------------------
            // Final: Assign to tiles
            // ----------------------------
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var appearance = BiomeRegistry.Get(Biome.Grassland).Appearance.Clone();
                    appearance.Background = smoothed[x, y];
                    Tilemap.SetTile(x, y, (ColoredGlyph)appearance);
                }
            }
        }

    }
}
