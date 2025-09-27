using SadRogue.Primitives;

namespace Primora.Core.Procedural.Objects
{
    internal interface ILocation
    {
        int Width { get; }
        int Height { get; }
        bool IsWalkable(Point position);
        bool ObstructsView(Point a);
    }
}
