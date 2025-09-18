using Primora.Core.Procedural.Objects;
using Primora.Core.Procedural.WorldBuilding.Helpers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class ZoneGenerator
    {
        /// <summary>
        /// Generate the correct zone layout based on the biome of the tile info.
        /// </summary>
        /// <param name="zone"></param>
        internal static void Generate(Zone zone)
        {
            var worldTileInfo = World.Instance.WorldMap.GetTileInfo(zone.WorldPosition);
            switch (worldTileInfo.Biome)
            {
                case Biome.Grassland:
                    GenerateGrassland(zone);
                    break;

                case Biome.Forest:
                    GenerateForest(zone);
                    break;

                case Biome.Hills:
                case Biome.Mountains:
                    GenerateMountains(zone);
                    break;

                case Biome.River:
                    GenerateRiver(zone);
                    break;

                case Biome.Bridge:
                    GenerateBridge(zone);
                    break;

                case Biome.Road:
                    GenerateRoad(zone);
                    break;

                case Biome.Settlement:
                    GenerateSettlement(zone);
                    break;

                default:
                    // Fallback to grassland, if biome unknown or unhandled
                    GenerateGrassland(zone);
                    break;
            }
        }

        private static void GenerateGrassland(Zone zone)
        {
            SetBackgroundAsOriginBiomeColor(zone);

            var grassTiles = new[] { ';', '.', ',', '"', '\'', ':' };
            var worldTileInfo = World.Instance.WorldMap.GetTileInfo(zone.WorldPosition);
            var random = zone.Random;

            // Tall grass mask
            var tallGrassClustersMask = CreateClustersMask(random, zone.Width, zone.Height);

            // Initial grass background setup + glyphs randomized
            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    var tile = zone.Tilemap.GetTile(x, y);
                    var zoneTileInfo = zone.GetTileInfo(x, y);
                    var chance = random.Next(100);

                    // Set foreground as a variation on biome color
                    tile.Foreground = GetBiomeGlyphColor(tile.Background, worldTileInfo.Biome, random);

                    if (tallGrassClustersMask[x, y])
                    {
                        tile.Glyph = 157;
                        zoneTileInfo.Walkable = false;
                        zoneTileInfo.ObstructView = true;
                    }
                    else if (chance < 25)
                    {
                        tile.Glyph = grassTiles[random.Next(grassTiles.Length)];
                    }

                    zone.Tilemap.SetTile(x, y, tile);
                    zone.SetTileInfo(x, y, zoneTileInfo);
                }
            }
        }

        private static void GenerateForest(Zone zone)
        {
            // Generate an initial grassland biome
            GenerateGrassland(zone);

            var worldTileInfo = World.Instance.WorldMap.GetTileInfo(zone.WorldPosition);
            var random = zone.Random;

            // Adjust grassland and make it represent a forest
            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    var tile = zone.Tilemap.GetTile(x, y);
                    var zoneTileInfo = zone.GetTileInfo(x, y);

                    // Remove tall grass
                    if (tile.Glyph == 157)
                    {
                        tile.Glyph = 0;
                        zoneTileInfo.Walkable = true;
                        zoneTileInfo.ObstructView = false;
                    }

                    // Random chance for a tree
                    var chance = random.Next(100);
                    if (chance < 10)
                    {
                        tile.Glyph = 6;
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, worldTileInfo.Biome, random);
                        zoneTileInfo.Walkable = false;
                        zoneTileInfo.ObstructView = true;
                    }

                    zone.Tilemap.SetTile(x, y, tile);
                    zone.SetTileInfo(x, y, zoneTileInfo);
                }
            }
        }

        private static void GenerateMountains(Zone zone)
        {
            SetBackgroundAsOriginBiomeColor(zone);

            var worldTileInfo = World.Instance.WorldMap.GetTileInfo(zone.WorldPosition);
            var random = zone.Random;

            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    var tile = zone.Tilemap.GetTile(x, y);
                    var zoneTileInfo = zone.GetTileInfo(x, y);

                    // Random chance for a rock
                    var chance = random.Next(100);
                    if (chance < 10)
                    {
                        tile.Glyph = 30;
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, worldTileInfo.Biome, random);
                        zoneTileInfo.Walkable = false;
                        zoneTileInfo.ObstructView = true;
                    }

                    zone.Tilemap.SetTile(x, y, tile);
                    zone.SetTileInfo(x, y, zoneTileInfo);
                }
            }
        }

        private static void GenerateRiver(Zone zone)
        {
            SetBackgroundAsOriginBiomeColor(zone);

            var worldTileInfo = World.Instance.WorldMap.GetTileInfo(zone.WorldPosition);
            var random = zone.Random;

            var waterGlyphs = new[] { 247, 126, 248, 249, 250 };
            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    var tile = zone.Tilemap.GetTile(x, y);
                    var zoneTileInfo = zone.GetTileInfo(x, y);

                    // Random chance for a river tile
                    var chance = random.Next(100);
                    if (chance < 25)
                    {
                        tile.Glyph = waterGlyphs[random.Next(waterGlyphs.Length)];
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, worldTileInfo.Biome, random);
                        zoneTileInfo.Walkable = false;
                        zoneTileInfo.ObstructView = true;
                    }

                    zone.Tilemap.SetTile(x, y, tile);
                    zone.SetTileInfo(x, y, zoneTileInfo);
                }
            }
        }

        private static void GenerateSettlement(Zone zone)
        {
            // TODO: Fix no variation because not always same biome neighbors
            SetBackgroundAsOriginBiomeColor(zone);

            SettlementHelper.GenerateSettlement(zone);
        }

        private static void GenerateRoad(Zone zone)
        {
            // TODO: Fix no variation because not always same biome neighbors
            SetBackgroundAsOriginBiomeColor(zone);

            // Define in which direction the road is (can be possible 8 ways)
            RoadZoneHelper.GenerateRoad(zone);
        }

        private static void GenerateBridge(Zone zone)
        {
            // Very custom
        }

        private static void SetBackgroundAsOriginBiomeColor(Zone zone)
        {
            var worldMap = World.Instance.WorldMap;
            var worldTileInfo = worldMap.GetTileInfo(zone.WorldPosition);
            var random = zone.Random;
            var neighborBackgrounds = CollectBiomeNeighborBackgrounds(worldMap, worldTileInfo.Origin, worldTileInfo.Biome, radius: 5);

            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    // Set background as the worldmap origin tile
                    var tile = zone.Tilemap.GetTile(x, y);
                    tile.Background = neighborBackgrounds[random.Next(neighborBackgrounds.Count)];
                    zone.Tilemap.SetTile(x, y, tile);
                }
            }
        }

        private static bool[,] CreateClustersMask(Random random, int width, int height)
        {
            // Step 1: Build a mask of where clusters should be
            bool[,] mainMask = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Small random chance to seed a new cluster
                    if (random.Next(100) < 2 && !mainMask[x, y])
                    {
                        // Grow an area from this seed
                        var localArea = GrowFromSeed(new Point(x, y), random, width, height, 30);

                        // Merge into main mask
                        for (int xx = 0; xx < width; xx++)
                        {
                            for (int yy = 0; yy < height; yy++)
                            {
                                if (localArea[xx, yy])
                                    mainMask[xx, yy] = true;
                            }
                        }
                    }
                }
            }

            // Step 2: Apply smoothing with CA
            mainMask = SmoothCluster(mainMask, iterations: 3);
            return mainMask;
        }

        private static bool[,] GrowFromSeed(Point seed, Random rand, int width, int height, int maxSize = 200)
        {
            var mask = new bool[width, height];
            var queue = new Queue<Point>();

            queue.Enqueue(seed);
            mask[seed.X, seed.Y] = true;

            int size = 1;

            while (queue.Count > 0 && size < maxSize)
            {
                var current = queue.Dequeue();

                // Explore neighbors (4 directions, can use 8 for more organic shapes)
                foreach (var dir in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
                {
                    int nx = current.X + dir.X;
                    int ny = current.Y + dir.Y;

                    // Stay inside bounds
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    int margin = 5;

                    // Stay inside bounds with margin
                    if (nx < margin || ny < margin || nx >= width - margin || ny >= height - margin)
                        continue;

                    if (mask[nx, ny]) continue; // already forest

                    var biome = World.Instance.WorldMap.GetTileInfo(nx, ny).Biome;

                    // Random growth chance
                    if ((biome == Biome.Woodland || biome == Biome.Forest) && rand.NextDouble() < 0.45) // 45% chance to expand here
                    {
                        mask[nx, ny] = true;
                        queue.Enqueue(new Point(nx, ny));
                        size++;
                    }
                }
            }

            return mask;
        }

        private static bool[,] SmoothCluster(bool[,] mask, int iterations = 2)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);

            for (int it = 0; it < iterations; it++)
            {
                var newMask = new bool[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int neighbors = CountNeighbors(mask, x, y);

                        if (mask[x, y])
                        {
                            // Cell survives if enough neighbors
                            newMask[x, y] = neighbors >= 3;
                        }
                        else
                        {
                            // Empty grows a new cell if surrounded by many
                            newMask[x, y] = neighbors >= 5;
                        }
                    }
                }

                mask = newMask;
            }

            return mask;
        }

        private static int CountNeighbors(bool[,] mask, int cx, int cy)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = cx + dx;
                    int ny = cy + dy;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (mask[nx, ny]) count++;
                    }
                }
            }

            return count;
        }

        private static Color GetBiomeGlyphColor(Color biomeColor, Biome biome, Random rand)
        {
            // Convert biome color to HSL
            RgbToHsl(biomeColor.R, biomeColor.G, biomeColor.B, out var h, out var s, out _);

            // Very subtle hue variation ±2%
            h += (float)(rand.NextDouble() * 0.04 - 0.02);

            // Slight saturation variation ±5%
            s = Math.Clamp(s * (0.95f + (float)rand.NextDouble() * 0.1f), 0f, 1f);

            float l;
            // Lightness based on biome
            switch (biome)
            {
                case Biome.Forest:
                    // Base lightness 0.25–0.35 (slightly lighter than before)
                    l = 0.25f + (float)rand.NextDouble() * 0.1f;
                    break;

                case Biome.Woodland:
                    // Base lightness 0.30–0.38 (slightly lighter than forest)
                    l = 0.30f + (float)rand.NextDouble() * 0.08f;
                    break;

                case Biome.Road:
                    // Slight hue variation ±3%
                    h += (float)(rand.NextDouble() * 0.06 - 0.03);

                    // Slight desaturation
                    s = Math.Clamp(s * (0.85f + (float)rand.NextDouble() * 0.1f), 0f, 1f);

                    // Slight lightness variation
                    l = 0.28f + (float)rand.NextDouble() * 0.10f;
                    break;

                default:
                    l = 0.30f + (float)rand.NextDouble() * 0.08f;
                    break;
            }

            // Convert back to RGB
            var (r, g, b) = HslToRgb(h, s, l);
            return new Color(r, g, b);
        }

        public static (int r, int g, int b) HslToRgb(float h, float s, float l)
        {
            float r, g, b;

            if (s <= 0.0001f)
            {
                // achromatic (gray)
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1f + s) : (l + s - l * s);
                float p = 2f * l - q;

                r = HueToRgb(p, q, h + 1f / 3f);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1f / 3f);
            }

            return (
                Math.Clamp((int)(r * 255f), 0, 255),
                Math.Clamp((int)(g * 255f), 0, 255),
                Math.Clamp((int)(b * 255f), 0, 255)
            );
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0f) t += 1f;
            if (t > 1f) t -= 1f;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        public static void RgbToHsl(int r, int g, int b, out float h, out float s, out float l)
        {
            float rf = r / 255f;
            float gf = g / 255f;
            float bf = b / 255f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            l = (max + min) / 2f;

            if (Math.Abs(max - min) < 0.0001f)
            {
                // achromatic (gray)
                h = 0f;
                s = 0f;
            }
            else
            {
                float d = max - min;
                s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

                if (Math.Abs(max - rf) < 0.0001f)
                    h = (gf - bf) / d + (gf < bf ? 6f : 0f);
                else if (Math.Abs(max - gf) < 0.0001f)
                    h = (bf - rf) / d + 2f;
                else
                    h = (rf - gf) / d + 4f;

                h /= 6f;
            }
        }

        /// <summary>
        /// Collects background colors from tiles around origin that share the same biome.
        /// </summary>
        private static List<Color> CollectBiomeNeighborBackgrounds(WorldMap worldMap, Point origin, Biome biome, int radius)
        {
            var results = new List<(Biome biome, Color color)>
            {
                (biome, worldMap.Tilemap.GetTile(origin).Background) // Include origin tile
            };

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // skip origin itself

                    var pos = new Point(origin.X + dx, origin.Y + dy);
                    if (!worldMap.InBounds(pos)) continue;

                    var neighborInfo = worldMap.GetTileInfo(pos);
                    var neighborTile = worldMap.Tilemap.GetTile(pos);

                    // Exceptional biomes
                    results.Add((neighborInfo.Biome, neighborTile.Background));
                }
            }

            var exceptionalCases = new[]
            {
                Biome.Settlement,
                Biome.Road,
            };

            // Some are exceptional cases
            if (exceptionalCases.Contains(biome))
            {
                // Take the colors of the most present biome that isn't one of the exceptionals
                return [.. results.GroupBy(a => a.biome)
                    .Where(a => !exceptionalCases.Contains(a.Key))
                    .OrderByDescending(a => a.Count())
                    .First()
                    .Select(a => a.color)];
            }

            return [.. results
                      .Where(a => a.biome == biome)
                      .Select(a => a.color)];
        }
    }
}
