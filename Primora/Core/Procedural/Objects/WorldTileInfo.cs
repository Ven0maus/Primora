using SadRogue.Primitives;

namespace Primora.Core.Procedural.Objects
{
    internal class WorldTileInfo
    {
        internal Point Origin { get; set; }
        internal Biome Biome { get; set; }
        internal bool HasTreeResource { get; set; }
        internal bool HasWaterResource { get; set; }
        internal bool Walkable { get; set; } = true;
    }
}
