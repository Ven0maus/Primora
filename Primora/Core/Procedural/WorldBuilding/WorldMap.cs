using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding
{
    /// <summary>
    /// Contains all accessible zones in the world.
    /// </summary>
    internal class WorldMap
    {
        private readonly int _width, _height;
        private readonly Dictionary<Point, Zone> _zones;
        private readonly Biome[] _biomes;

        internal readonly Tilemap Tilemap;

        internal WorldMap(int width, int height)
        {
            _width = width;
            _height = height;
            _zones = [];
            _biomes = new Biome[width * height];

            Tilemap = new Tilemap(width, height);
        }

        #region World Generation

        internal void Generate()
        {
            var random = Constants.General.Random;

            // Define the biomes op the world
            GenerateBiomes(random);

            // Define the details of the biomes of the world
            GenerateDetails(random);
        }

        /// <summary>
        /// Returns the biome at the specified coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal Biome GetBiome(int x, int y)
        {
            return _biomes[Point.ToIndex(x, y, _width)];
        }

        private void GenerateBiomes(Random random)
        {
            // Step 0: Generate base noise maps
            var heightmap = GenerateHeightMap(random);
            var tempMap = GenerateTemperatureMap(random);
            var moistureMap = GenerateMoistureMap(random);

            ApplyLatitudeAdjustment(tempMap);

            // Step 1: Determine biome map
            var biomes = BiomeRegistry.GetAll(); // Assumes ascending NoiseLevel
            var biomeMap = GenerateBiomeMap(heightmap, tempMap, moistureMap, biomes, random);

            // Step 2: Smooth biome transitions and record biomes into world map
            biomeMap = SmoothBiomes(biomeMap);
            RecordBiomesIntoWorldMap(biomeMap);

            // Step 3: Create random variation map
            var variationMap = GenerateVariationMap(random);

            // Step 4: Define colors
            var colorMap = GenerateColorMap(heightmap, biomeMap, biomes, variationMap);

            // Step 5: Apply directional shading
            ApplyDirectionalShading(colorMap, heightmap);

            // Step 6: Smooth final colors
            var smoothed = SmoothColors(colorMap);

            // Step 7: Assign colors to tiles
            ApplyColorsToTilemap(smoothed);
        }

        private void GenerateDetails(Random random)
        {
            for (int x=0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // TODO
                }
            }
        }

        #endregion

        #region Noise Map Generators

        private float[] GenerateHeightMap(Random rand) =>
            OpenSimplex.GenerateNoiseMap(_width, _height, seed: rand.Next(), scale: 180f, octaves: 6, persistance: 0.5f, lacunarity: 2.0f);

        private float[] GenerateTemperatureMap(Random rand) =>
            OpenSimplex.GenerateNoiseMap(_width, _height, seed: rand.Next(), scale: 300f, octaves: 2, persistance: 0.6f, lacunarity: 2f);

        private float[] GenerateMoistureMap(Random rand) =>
            OpenSimplex.GenerateNoiseMap(_width, _height, seed: rand.Next(), scale: 180f, octaves: 4, persistance: 0.55f, lacunarity: 2f);

        private void ApplyLatitudeAdjustment(float[] tempMap)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int i = Point.ToIndex(x, y, _width);
                    float latitude = (float)y / _height;
                    tempMap[i] = tempMap[i] * 0.7f + (1f - Math.Abs(latitude - 0.5f) * 2f) * 0.3f;
                }
            }
        }

        #endregion

        #region Biome Generation

        private Biome[,] GenerateBiomeMap(float[] heightmap, float[] tempMap, float[] moistureMap, ICollection<BiomeDefinition> biomes, Random rand)
        {
            var biomeMap = new Biome[_width, _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int i = Point.ToIndex(x, y, _width);
                    biomeMap[x, y] = SelectOrganicBiome(
                        Constants.General.Seed,
                        heightmap[i],
                        tempMap[i],
                        moistureMap[i],
                        x, y,
                        biomes,
                        rand
                    );
                }
            }

            return biomeMap;
        }

        private Biome[,] SmoothBiomes(Biome[,] biomeMap)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var newBiomes = new Biome[_width, _height];

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        var counts = new Dictionary<Biome, int>();

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;

                                var b = biomeMap[nx, ny];
                                if (!counts.ContainsKey(b)) counts[b] = 0;
                                counts[b]++;
                            }
                        }

                        newBiomes[x, y] = counts.OrderByDescending(kvp => kvp.Value).First().Key;
                    }
                }

                biomeMap = newBiomes;
            }

            return biomeMap;
        }

        public void RecordBiomesIntoWorldMap(Biome[,] biomeMap)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y=0; y < _height; y++)
                {
                    _biomes[Point.ToIndex(x, y, _width)] = biomeMap[x, y];
                }
            }
        }

        #endregion

        #region Variation Map

        private int[,] GenerateVariationMap(Random rand)
        {
            var variationMap = new int[_width, _height];

            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    variationMap[x, y] = rand.Next(-30, 31);

            // Smooth variation
            for (int pass = 0; pass < 3; pass++)
            {
                var newMap = new int[_width, _height];

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

            return variationMap;
        }

        #endregion

        #region Color Map

        private Color[,] GenerateColorMap(float[] heightmap, Biome[,] biomeMap, ICollection<BiomeDefinition> biomes, int[,] variationMap)
        {
            var colorMap = new Color[_width, _height];
            var biomeColors = biomes
                .Select(a => (a.MinHeight, a.MaxHeight, a.Biome, color: BiomeRegistry.Get(a.Biome).Color))
                .ToList();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int i = Point.ToIndex(x, y, _width);
                    float height = heightmap[i];

                    int heightVariation = (int)((height - 0.5f) * 60f);
                    int noiseVariation = variationMap[x, y];
                    int totalVariation = heightVariation + noiseVariation;

                    var currentIndex = biomeColors.FindIndex(b => b.Biome == biomeMap[x, y]);
                    var baseColor = biomeColors[currentIndex].color;

                    var neighborBlended = BlendTowardBestNeighbor(
                        x, y, baseColor, biomeMap, biomeColors, heightmap, _width, _height, maxBlend: 0.4f, radius: 2
                    );

                    int r = Math.Clamp(neighborBlended.R + totalVariation, 0, 255);
                    int g = Math.Clamp(neighborBlended.G + totalVariation, 0, 255);
                    int b = Math.Clamp(neighborBlended.B + totalVariation, 0, 255);

                    colorMap[x, y] = AdjustForElevation(new Color(r, g, b), height, biomeMap[x, y]);
                }
            }

            return colorMap;
        }

        #endregion

        #region Shading & Smoothing

        private void ApplyDirectionalShading(Color[,] colorMap, float[] heightmap)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 1; x < _width; x++)
                {
                    int i = Point.ToIndex(x, y, _width);
                    int leftIndex = Point.ToIndex(x - 1, y, _width);

                    float h = heightmap[i];
                    float hLeft = heightmap[leftIndex];

                    var c = colorMap[x, y];
                    int delta = (int)((hLeft - h) * 40f);

                    int r = Math.Clamp(c.R + delta, 0, 255);
                    int g = Math.Clamp(c.G + delta, 0, 255);
                    int b = Math.Clamp(c.B + delta, 0, 255);

                    colorMap[x, y] = new Color(r, g, b);
                }
            }
        }

        private Color[,] SmoothColors(Color[,] colorMap)
        {
            var smoothed = new Color[_width, _height];
            float smoothingFactor = 0.3f;

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

                    var avgR = sumR / count;
                    var avgG = sumG / count;
                    var avgB = sumB / count;

                    var original = colorMap[x, y];

                    smoothed[x, y] = new Color(
                        (int)(original.R * (1 - smoothingFactor) + avgR * smoothingFactor),
                        (int)(original.G * (1 - smoothingFactor) + avgG * smoothingFactor),
                        (int)(original.B * (1 - smoothingFactor) + avgB * smoothingFactor)
                    );
                }
            }

            return smoothed;
        }

        #endregion

        #region Apply to Tilemap

        private void ApplyColorsToTilemap(Color[,] colorMap)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var appearance = BiomeRegistry.Get(_biomes[Point.ToIndex(x, y, _width)]).Appearance.Clone();
                    appearance.Background = colorMap[x, y]; // Adjust biome coloring to be more accurate
                    Tilemap.SetTile(x, y, (ColoredGlyph)appearance);
                }
            }
        }

        #endregion

        #region Utility Functions
        private static Biome SelectOrganicBiome(
            int seed, 
            float height,
            float temperature,
            float moisture,
            int x, int y,
            ICollection<BiomeDefinition> biomeRanges,
            Random rand)
        {
            // 1. Apply low-frequency noise to height to wiggle borders
            float edgeNoise = (float)OpenSimplex.Noise2(seed, x * 0.05f, y * 0.05f);
            float adjustedHeight = height + edgeNoise * 0.05f; // tweak 0.05 for more/less wiggling

            // 2. Collect candidate biomes based on ranges
            var candidates = new List<(Biome biome, float weight)>();
            foreach (var b in biomeRanges)
            {
                bool inHeight = adjustedHeight >= b.MinHeight && adjustedHeight <= b.MaxHeight;
                bool inTemp = temperature >= b.MinTemp && temperature <= b.MaxTemp;
                bool inMoisture = moisture >= b.MinMoisture && moisture <= b.MaxMoisture;

                if (inHeight && inTemp && inMoisture)
                {
                    // Weight based on how close the tile is to biome center (height, temp, moisture)
                    float hCenter = (b.MinHeight + b.MaxHeight) / 2f;
                    float tCenter = (b.MinTemp + b.MaxTemp) / 2f;
                    float mCenter = (b.MinMoisture + b.MaxMoisture) / 2f;

                    float hWeight = 1f - Math.Abs(adjustedHeight - hCenter) / ((b.MaxHeight - b.MinHeight) / 2f);
                    float tWeight = 1f - Math.Abs(temperature - tCenter) / ((b.MaxTemp - b.MinTemp) / 2f);
                    float mWeight = 1f - Math.Abs(moisture - mCenter) / ((b.MaxMoisture - b.MinMoisture) / 2f);

                    float weight = Math.Clamp((hWeight + tWeight + mWeight) / 3f, 0f, 1f);
                    candidates.Add((b.Biome, weight));
                }
            }

            // 3. Pick biome using weighted random
            if (candidates.Count == 0)
            {
                // fallback to nearest by height only
                var nearest = biomeRanges.OrderBy(b => Math.Abs(adjustedHeight - (b.MinHeight + b.MaxHeight) / 2f)).First();
                return nearest.Biome;
            }

            float totalWeight = candidates.Sum(c => c.weight);
            float pick = (float)(rand.NextDouble() * totalWeight);
            float running = 0f;

            foreach (var (biome, weight) in candidates)
            {
                running += weight;
                if (pick <= running) return biome;
            }

            return candidates[0].biome; // fallback
        }

        private static Color AdjustForElevation(
                    Color c,
                    float height,
                    Biome biome)
        {
            float h, s, l;
            RgbToHsl(c.R, c.G, c.B, out h, out s, out l);

            float factor = Math.Clamp(height, 0f, 1f);

            if (biome == Biome.Snow || biome == Biome.Glacial)
            {
                // Don't adjust, these are already fine
            }
            else if (factor < 0.3f)
            {
                float lowFactor = 1f - (factor / 0.3f);
                s *= 1f + 0.3f * lowFactor;
                h = (h + 0.03f * lowFactor) % 1f;
                l *= 1f + 0.2f * lowFactor;
            }
            else if (factor > 0.85f)
            {
                float snowFactor = (factor - 0.85f) / 0.15f;
                snowFactor = Math.Clamp(snowFactor, 0f, 1f);
                float targetS = 0.15f;
                float targetL = 0.95f;
                float targetH = 0f;
                s = s * (1f - snowFactor) + targetS * snowFactor;
                l = l * (1f - snowFactor) + targetL * snowFactor;
                h = h * (1f - snowFactor) + targetH * snowFactor;
            }
            else if (factor > 0.6f)
            {
                float highFactor = (factor - 0.6f) / 0.3f;
                s *= 1f - 0.5f * highFactor;
                l *= 1f - 0.3f * highFactor;
                h = (h - 0.02f * highFactor + 1f) % 1f;
            }
            else
            {
                float midFactor = 1f - Math.Abs((factor - 0.5f) / 0.1f);
                midFactor = Math.Clamp(midFactor, 0f, 1f);
                s *= 1f + 0.4f * midFactor;
                h = (h + 0.04f * midFactor) % 1f;
                l *= 1f + 0.1f * midFactor;
            }

            s = Math.Clamp(s, 0f, 1f);
            l = Math.Clamp(l, 0f, 1f);

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

            h = 0f;
            s = 0f;
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

        // Helper: neighbor-aware blend limited by height-similarity and maxBlend
        private static Color BlendTowardBestNeighbor(
            int x, int y,
            Color baseColor,
            Biome[,] biomeMap,
            List<(float min, float max, Biome biome, SadRogue.Primitives.Color color)> bcolors,
            float[] heightmap,
            int width,
            int height,
            float maxBlend = 0.30f, // tweak: 0.15 = subtle, 0.30 = visible
            int radius = 1)
        {
            int cx = x, cy = y;
            float h = heightmap[Point.ToIndex(cx, cy, width)];

            // Count neighbors per biome and record their base color & biome info
            var neighborCounts = new Dictionary<Biome, int>();
            var neighborInfo = new Dictionary<Biome, (float min, float max, SadRogue.Primitives.Color color)>();

            int availableNeighbors = 0;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cx + dx, ny = cy + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    availableNeighbors++;

                    var nb = biomeMap[nx, ny];
                    if (!neighborCounts.ContainsKey(nb)) neighborCounts[nb] = 0;
                    neighborCounts[nb]++;

                    if (!neighborInfo.ContainsKey(nb))
                    {
                        var bc = bcolors.First(b => b.biome == nb);
                        neighborInfo[nb] = (bc.min, bc.max, bc.color);
                    }
                }
            }

            if (availableNeighbors == 0 || neighborCounts.Count == 0)
                return baseColor; // no neighbors to blend toward

            // Evaluate weights: neighborFraction * heightSimilarity
            float bestWeight = 0f;
            Biome bestBiome = default;
            SadRogue.Primitives.Color bestColor = baseColor;

            foreach (var kv in neighborCounts)
            {
                var nbBiome = kv.Key;
                int count = kv.Value;
                float neighborFraction = (float)count / (float)availableNeighbors; // 0..1

                var info = neighborInfo[nbBiome];
                float center = (info.min + info.max) * 0.5f;
                float halfRange = Math.Max((info.max - info.min) * 0.5f, 0.0001f);
                // 1.0 when near center, 0.0 when >= one half-range away
                float heightSimilarity = 1f - Math.Clamp(Math.Abs(h - center) / halfRange, 0f, 1f);

                // OPTIONAL: emphasize height similarity more (power >1)
                //heightSimilarity = (float)Math.Pow(heightSimilarity, 1.0);

                float weight = neighborFraction * heightSimilarity;

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestBiome = nbBiome;
                    bestColor = info.color;
                }
            }

            if (bestWeight <= 0f)
                return baseColor;

            float blend = Math.Clamp(maxBlend * bestWeight, 0f, maxBlend); // final blend fraction
                                                                           // Linear interpolate colors
            int r = (int)(baseColor.R * (1 - blend) + bestColor.R * blend);
            int g = (int)(baseColor.G * (1 - blend) + bestColor.G * blend);
            int b = (int)(baseColor.B * (1 - blend) + bestColor.B * blend);

            return new Color(r, g, b);
        }
        #endregion
    }
}
