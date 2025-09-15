using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using Primora.Extensions;
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
        private readonly TileInfo[] _tiles;
        internal readonly Tilemap Tilemap;

        internal WorldMap(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new TileInfo[width * height];
            Tilemap = new Tilemap(width, height);
        }

        #region Accessors
        /// <summary>
        /// Returns the tile information at the specified coordinate on the world map.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal TileInfo GetTileInfo(int x, int y)
        {
            return _tiles[Point.ToIndex(x, y, _width)];
        }

        internal TileInfo GetTileInfo(Point position)
            => GetTileInfo(position.X, position.Y);
        #endregion

        #region World Generation

        internal void Generate()
        {
            var random = new Random(Constants.General.GameSeed);

            // Define the biomes of the world
            GenerateBiomes(random, out var heightmap);

            // Define the details of the biomes of the world
            GenerateDetails(random, heightmap);
        }

        private void GenerateBiomes(Random random, out float[] heightmap)
        {
            // Step 0: Generate base noise maps
            heightmap = GenerateHeightMap(random);
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

        private void GenerateDetails(Random random, float[] heightMap)
        {
            bool[,] treeMask = CreateTreeMask(random);

            // Step 3: Set base biome glyphs
            var grassTiles = new[] { ';', '.', ',', '"', '\'', ':' };
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var tile = Tilemap.GetTile(x, y);
                    var tileInfo = GetTileInfo(x, y);
                    var biome = tileInfo.Biome;

                    if (treeMask[x, y])
                    {
                        tile.Glyph = 6; // Tree glyph
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, biome, random);
                        tileInfo.Biome = Biome.Forest; // Turn into forest, regardless of biome
                        tileInfo.HasTreeResource = true;
                    }
                    else if ((biome == Biome.Hills || biome == Biome.Mountains) && random.Next(100) < 20)
                    {
                        tile.Glyph = random.Next(2) == 0 ? 94 : 30;
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, biome, random);
                    }
                    else if ((biome == Biome.Grassland || biome == Biome.Woodland || biome == Biome.Forest) && random.Next(100) < 20)
                    {
                        tile.Glyph = grassTiles[random.Next(grassTiles.Length)];
                        tile.Foreground = GetBiomeGlyphColor(tile.Background, biome, random);
                        tileInfo.Biome = Biome.Grassland; // When no trees, woodland and forest becomes grassland
                    }
                }
            }

            CreateRiver(random, heightMap);
            CreateSettlementsAndRoads(random, heightMap);
        }

        #endregion


        #region Noise Map Generators

        private float[] GenerateHeightMap(Random rand) =>
            [.. OpenSimplex.GenerateNoiseMap(
                _width, _height,
                seed: rand.Next(),
                scale: 200f,          // ↑ slightly bigger scale → smoother continents
                octaves: 5,
                persistance: 0.55f,
                lacunarity: 1.9f
            ).Select(h => Math.Clamp(h * 0.85f + 0.1f, 0f, 1f))];

        private float[] GenerateTemperatureMap(Random rand) =>
            OpenSimplex.GenerateNoiseMap(_width, _height, seed: rand.Next(), scale: 300f, octaves: 2, persistance: 0.6f, lacunarity: 2f);

        private float[] GenerateMoistureMap(Random rand) =>
            [.. OpenSimplex.GenerateNoiseMap(
                _width, _height,
                seed: rand.Next(),
                scale: 180f,
                octaves: 4,
                persistance: 0.55f,
                lacunarity: 2f
            ).Select(m => Math.Clamp(m * 0.7f + 0.3f, 0f, 1f))];

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
                        Constants.General.GameSeed,
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
                    _tiles[Point.ToIndex(x, y, _width)] = new TileInfo { Biome = biomeMap[x, y] };
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

                    colorMap[x, y] = new Color(r, g, b);
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
                    var appearance = BiomeRegistry.Get(GetTileInfo(x, y).Biome).Appearance.Clone();
                    appearance.Background = colorMap[x, y]; // Adjust biome coloring to be more accurate
                    Tilemap.SetTile(x, y, (ColoredGlyph)appearance);
                }
            }
        }

        #endregion

        #region Tree Generation
        private bool[,] CreateTreeMask(Random random)
        {
            // Step 1: Build a mask of where forests should be
            bool[,] forestMask = new bool[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var biome = GetTileInfo(x, y).Biome;
                    if (biome == Biome.Woodland || biome == Biome.Forest)
                    {
                        // Small random chance to seed a new forest
                        if (random.Next(100) < 1 && !forestMask[x, y])
                        {
                            // Grow a forest from this seed
                            var localForest = GrowForestFromSeed(new Point(x, y), random, 50);

                            // Merge into main forest mask
                            for (int xx = 0; xx < _width; xx++)
                            {
                                for (int yy = 0; yy < _height; yy++)
                                {
                                    if (localForest[xx, yy])
                                        forestMask[xx, yy] = true;
                                }
                            }
                        }
                    }
                }
            }

            // Step 2: Apply smoothing with CA
            forestMask = SmoothForest(forestMask, iterations: 3);
            return forestMask;
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

        private static bool[,] SmoothForest(bool[,] mask, int iterations = 2)
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
                        int neighbors = CountTreeNeighbors(mask, x, y);

                        if (mask[x, y])
                        {
                            // Tree survives if enough neighbors
                            newMask[x, y] = neighbors >= 3;
                        }
                        else
                        {
                            // Empty grows a tree if surrounded by many
                            newMask[x, y] = neighbors >= 5;
                        }
                    }
                }

                mask = newMask;
            }

            return mask;
        }

        private static int CountTreeNeighbors(bool[,] mask, int cx, int cy)
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


        private bool[,] GrowForestFromSeed(Point seed, Random rand, int maxSize = 200)
        {
            var mask = new bool[_width, _height];
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
                    if (nx < 0 || ny < 0 || nx >= _width || ny >= _height)
                        continue;

                    if (mask[nx, ny]) continue; // already forest

                    var biome = GetTileInfo(nx, ny).Biome;

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
        #endregion

        #region Rivers
        private void CreateRiver(Random random, float[] heightMap)
        {
            // Collect random city locations and roads
            var roadPoints = RiverNetworkHelper.BuildMajorRiver(heightMap, _width, _height, random, out var riverDistances);

            // Draw roads between cities
            var glyphPositions = DefineLineGlyphsByPositions(roadPoints);
            foreach (var (coordinate, _) in glyphPositions)
            {
                var tile = Tilemap.GetTile(coordinate);
                tile.Glyph = 0;

                Color biome = tile.Background;
                Color deepBlue = "#061558".HexToColor();
                Color shallowBlue = "#102373".HexToColor();

                // Look up distance
                int distSq = riverDistances.TryGetValue(coordinate, out var d) ? d : 0;
                float dist = MathF.Sqrt(distSq);

                // Map distance → blend
                // 0 = center (deep), radius = edge (light)
                float t = Math.Clamp(dist / 2f, 0f, 1f); // if radius=2

                // Interpolate between deep blue and shallow blue
                Color riverColor = Color.Lerp(deepBlue, shallowBlue, t);

                // Blend with biome color
                float blend = 0.45f;
                tile.Background = Color.Lerp(riverColor, biome, blend);

                var tileInfo = GetTileInfo(coordinate);
                tileInfo.Biome = Biome.River;
                tileInfo.HasWaterResource = true;
            }
        }
        #endregion

        #region Settlements
        private void CreateSettlementsAndRoads(Random random, float[] heightMap)
        {
            // Collect random city locations and roads
            var cityPositions = GetCityPositions(random, heightMap, _width, _height);
            var roadPoints = RoadNetworkHelper.BuildRoadNetwork(cityPositions, heightMap, _width, _height, random);

            // Draw roads between cities
            var glyphPositions = DefineLineGlyphsByPositions(roadPoints);
            foreach (var (coordinate, glyph) in glyphPositions)
            {
                var tile = Tilemap.GetTile(coordinate);
                tile.Glyph = glyph;
                tile.Foreground = GetBiomeGlyphColor("#A1866F".HexToColor(), Biome.Road, random);

                var tileInfo = GetTileInfo(coordinate);
                if (tileInfo.Biome == Biome.River)
                    tileInfo.Biome = Biome.Bridge;
                else
                    tileInfo.Biome = Biome.Road;
            }

            // Set cities
            foreach (var coordinate in cityPositions)
            {
                var tile = Tilemap.GetTile(coordinate);
                tile.Glyph = 127;
                tile.Foreground = Color.White;

                var tileInfo = GetTileInfo(coordinate);
                tileInfo.Biome = Biome.Settlement;
            }
        }

        private List<Point> GetCityPositions(Random random,
             float[] heightMap, int width, int height,
             int cityCount = 8, int minDistance = 30,
             int borderMargin = 10) // new parameter for border margin
        {
            var cities = new List<Point>();

            // Step 1: Collect candidate points based on height (avoid mountains and water)
            var candidates = new List<Point>();
            for (int y = borderMargin; y < height - borderMargin; y++) // exclude border rows
            {
                for (int x = borderMargin; x < width - borderMargin; x++) // exclude border columns
                {
                    float h = heightMap[Point.ToIndex(x, y, width)];

                    // realistic city terrain: avoid extremes
                    if (h >= 0.25f && h <= 0.7f && GetTileInfo(x, y).Biome != Biome.River)
                        candidates.Add(new Point(x, y));
                }
            }

            if (candidates.Count == 0)
                return cities; // no valid terrain

            // Step 2: Pick cities one by one
            while (cities.Count < cityCount && candidates.Count > 0)
            {
                // Random candidate
                var candidate = candidates[random.Next(candidates.Count)];

                // Check minimum distance from existing cities
                bool tooClose = cities.Any(c => EuclideanDistanceSquared(c, candidate) < minDistance * minDistance);
                if (!tooClose)
                {
                    cities.Add(candidate);
                }

                // Remove candidate to avoid repeated selection
                candidates.Remove(candidate);
            }

            return cities;
        }

        // Distance helper (squared to avoid sqrt)
        private static int EuclideanDistanceSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }
        #endregion

        #region Utility Functions
        /// <summary>
        /// Defines the correct box-line style glyphs for the entire path.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static List<(Point coordinate, int glyph)> DefineLineGlyphsByPositions(HashSet<Point> positions)
        {
            var glyphs = new List<(Point coordinate, int glyph)>();
            foreach (var point in positions)
            {
                // Check each neighbor to define the correct glyph for this point
                bool left = positions.Contains(new Point(point.X - 1, point.Y));
                bool right = positions.Contains(new Point(point.X + 1, point.Y));
                bool up = positions.Contains(new Point(point.X, point.Y - 1));
                bool down = positions.Contains(new Point(point.X, point.Y + 1));

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

        private static Biome SelectOrganicBiome(
            int seed,
            float height,
            float temperature,
            float moisture,
            int x, int y,
            ICollection<BiomeDefinition> biomeRanges,
            Random rand)
        {
            float edgeNoise = (float)OpenSimplex.Noise2(seed, x * 0.05f, y * 0.05f);
            float adjustedHeight = height + edgeNoise * 0.05f;

            var candidates = new List<(Biome biome, float weight)>();
            foreach (var b in biomeRanges)
            {
                bool inHeight = adjustedHeight >= b.MinHeight && adjustedHeight <= b.MaxHeight;
                bool inTemp = temperature >= b.MinTemp && temperature <= b.MaxTemp;
                bool inMoisture = moisture >= b.MinMoisture && moisture <= b.MaxMoisture;

                if (inHeight && inTemp && inMoisture)
                {
                    float hCenter = (b.MinHeight + b.MaxHeight) / 2f;
                    float tCenter = (b.MinTemp + b.MaxTemp) / 2f;
                    float mCenter = (b.MinMoisture + b.MaxMoisture) / 2f;

                    float hWeight = 1f - Math.Abs(adjustedHeight - hCenter) / ((b.MaxHeight - b.MinHeight) / 2f);
                    float tWeight = 1f - Math.Abs(temperature - tCenter) / ((b.MaxTemp - b.MinTemp) / 2f);
                    float mWeight = 1f - Math.Abs(moisture - mCenter) / ((b.MaxMoisture - b.MinMoisture) / 2f);

                    float weight = Math.Clamp((hWeight + tWeight + mWeight) / 3f, 0f, 1f);

                    // 🌲 Forest bias → multiply weight
                    if (b.Name.Contains("Forest") || b.Name.Contains("Woodland"))
                        weight *= 1.5f;

                    candidates.Add((b.Biome, weight));
                }
            }

            // 3. Apply forest bias before weighted random
            for (int ci = 0; ci < candidates.Count; ci++)
            {
                var (biome, weight) = candidates[ci];
                float bias = 1f;

                if (biome == Biome.Forest || biome == Biome.Woodland || biome == Biome.Grassland)  // or explicit check: (c.biome == Biome.Forest || c.biome == Biome.TropicalForest)
                    bias = 2.2f; // forests ~2x more likely

                candidates[ci] = (biome, weight * bias);
            }

            // 3. Pick biome using weighted random
            if (candidates.Count == 0)
            {
                // fallback to nearest by height only
                var nearest = biomeRanges.OrderBy(b => Math.Abs(adjustedHeight - (b.MinHeight + b.MaxHeight) / 2f)).First();
                return nearest.Biome;
            }

            // Weighted random pick
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
                        var (min, max, biome, color) = bcolors.First(b => b.biome == nb);
                        neighborInfo[nb] = (min, max, color);
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

                var (min, max, color) = neighborInfo[nbBiome];
                float center = (min + max) * 0.5f;
                float halfRange = Math.Max((max - min) * 0.5f, 0.0001f);
                // 1.0 when near center, 0.0 when >= one half-range away
                float heightSimilarity = 1f - Math.Clamp(Math.Abs(h - center) / halfRange, 0f, 1f);

                // OPTIONAL: emphasize height similarity more (power >1)
                //heightSimilarity = (float)Math.Pow(heightSimilarity, 1.0);

                float weight = neighborFraction * heightSimilarity;

                if (weight > bestWeight)
                {
                    bestWeight = weight;
                    bestBiome = nbBiome;
                    bestColor = color;
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
