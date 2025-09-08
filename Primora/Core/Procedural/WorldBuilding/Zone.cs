using Primora.Core.Procedural.Common;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal class Zone
    {
        private readonly int _width, _height;
        internal readonly Tilemap Tilemap;

        internal Zone(int width, int height)
        {
            _width = width;
            _height = height;
            Tilemap = new Tilemap(width, height);
        }

        internal void GetFromCacheOrGenerate()
        {

        }
    }
}
