using SadRogue.Primitives;

namespace Primora.Core.Npcs.Objects
{
    internal sealed class ActorDefinition
    {
        public Entities Entity { get; set; }
        public int MaxHealth { get; set; }
        public Color Foreground { get; set; }
        public int Glyph { get; set; }
    }
}
