using Primora.Serialization;
using SadRogue.Primitives;
using System.Text.Json.Serialization;

namespace Primora.Core.Npcs.Objects
{
    internal sealed class ActorDefinition
    {
        public Entities Entity { get; set; }
        public int MaxHealth { get; set; }
        public int Damage { get; set; }
        public int Armour { get; set; }
        public int Evasion { get; set; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Foreground { get; set; }

        [JsonConverter(typeof(GlyphJsonConverter))]
        public int Glyph { get; set; }
    }
}
