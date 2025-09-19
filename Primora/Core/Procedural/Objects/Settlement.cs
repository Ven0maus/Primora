using SadRogue.Primitives;

namespace Primora.Core.Procedural.Objects
{
    internal class Settlement
    {
        internal Point Position { get; init; }

        public Settlement(Point position)
        {
            Position = position;
        }
    }
}
