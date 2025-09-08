using Primora.Core.Procedural.Common;
using SadRogue.Primitives;
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
            var noiseMap = OpenSimplex.GenerateNoiseMap(_width, _height, 1337, 23f, 3, 0.37f, 0.9f);

            for (int x=0; x < _width; x++)
            {
                for (int y=0; y < _height; y++)
                {
                    var index = Point.ToIndex(x, y, _width);
                    if (noiseMap[index] > 0.6)
                    {
                        Tilemap.SetTile(x, y, TileRegistry.GetRandomVariant("Corrupted", "Grass").Id);
                    }
                    else
                        Tilemap.SetTile(x, y, TileRegistry.GetRandomVariant("Grassland", "Grass").Id);
                }
            }
        }
    }
}
