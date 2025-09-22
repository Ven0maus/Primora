using SadRogue.Primitives;

namespace Primora.Core.Procedural.Objects
{
    internal struct ZoneTileInfo(Point origin)
    {
        public Point Origin = origin;
        public bool Walkable = true;
        public bool ObstructView = false;
        public double Weight = 1;
    }
}
