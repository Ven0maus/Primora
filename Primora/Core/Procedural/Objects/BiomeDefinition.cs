using Primora.Serialization;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Text.Json.Serialization;

namespace Primora.Core.Procedural.Objects
{
    internal class BiomeDefinition
    {
        public string Name { get; set; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Color { get; set; }

        public float MinNoise { get; set; }
        public float MaxNoise { get; set; }

        private ColoredGlyph _appearance;
        [JsonIgnore]
        public ColoredGlyph Appearance => _appearance ??= new ColoredGlyph
        {
            Background = Color,
            Foreground = Color.Transparent,
            Glyph = 0
        };

        private Biome? _biome;
        [JsonIgnore]
        public Biome Biome => _biome ??= Enum.Parse<Biome>(Name, true);
    }
}
