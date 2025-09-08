using Primora.Serialization;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Primora.Core.Procedural.Objects
{
    internal class TileDefinition
    {
        public string Name { get; init; }
        public string Key { get; init; }
        public Obstruction Obstruction { get; init; }
        public TileVariantGroup Default { get; init; }
        public Dictionary<string, TileVariantGroup> BiomeOverrides { get; init; } = [];

        public class TileVariantGroup
        {
            [JsonConverter(typeof(GlyphArrayJsonConverter))]
            public List<int> Glyph { get; set; }

            [JsonConverter(typeof(ColorArrayJsonConverter))]
            public List<Color> Foreground { get; set; }

            [JsonConverter(typeof(ColorArrayJsonConverter))]
            public List<Color> Background { get; set; }
        }
    }
}
