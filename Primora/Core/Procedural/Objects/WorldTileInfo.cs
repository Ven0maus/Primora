using SadRogue.Primitives;

namespace Primora.Core.Procedural.Objects
{
    internal struct WorldTileInfo(Point origin, Biome biome)
    {
        internal Point Origin = origin;
        internal Biome Biome = biome;
        internal bool HasTreeResource = false;
        internal bool HasWaterResource = false;
        internal bool Walkable = true;
        internal double Weight = 1;
    }
}
