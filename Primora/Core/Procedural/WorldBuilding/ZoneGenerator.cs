using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using System;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class ZoneGenerator
    {
        /// <summary>
        /// Generate the correct zone layout based on the biome of the tile info.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="tileInfo"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="random"></param>
        internal static void Generate(Tilemap tilemap, TileInfo tileInfo, int width, int height, Random random)
        {
            switch (tileInfo.Biome)
            {
                case Biome.Grassland:
                    GenerateGrassland(tilemap, tileInfo, width, height, random);
                    break;

                case Biome.Forest:
                    GenerateForest(tilemap, tileInfo, width, height, random);
                    break;
            }
        }

        private static void GenerateGrassland(Tilemap tilemap, TileInfo tileInfo, int width, int height, Random random)
        {
            // Generate scattered grass glyphs, bushes, twigs, flowers
            // Generate also patches of tall grass that cannot be tresspassed and blocks view
        }

        private static void GenerateForest(Tilemap tilemap, TileInfo tileInfo, int width, int height, Random random)
        {
            // Generate scattered grass glyphs, bushes, twigs, flowers
            // Generate also patches of tall grass that cannot be tresspassed and blocks view
        }
    }
}
