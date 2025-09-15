using System.Drawing;

namespace Primora.Core.Procedural.Objects
{
    internal class ZoneTileInfo
    {
        public Point Origin { get; set; }
        public bool Walkable { get; set; } = true;
        public bool ObstructView { get; set; } = false;
    }
}
