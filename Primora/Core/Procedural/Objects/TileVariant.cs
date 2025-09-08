using Primora.Serialization;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Text.Json.Serialization;

namespace Primora.Core.Procedural.Objects
{
    internal class TileVariant
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public Biome Biome { get; set; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Background { get; set; }

        [JsonConverter(typeof(ColorJsonConverter))]
        public Color Foreground { get; set; }

        public Obstruction Obstruction { get; set; }

        [JsonConverter(typeof(GlyphJsonConverter))]
        public int Glyph { get; set; }

        private ColoredGlyphBase _cellAppearance;
        /// <summary>
        /// The object used for rendering the appearance of this tile.
        /// </summary>
        [JsonIgnore]
        public ColoredGlyphBase CellAppearance => _cellAppearance ??= new ColoredGlyph(Foreground, Background, Glyph);
    }

    [Flags]
    public enum Obstruction
    {
        None = 0,
        NoVision = 1 << 0,
        NotWalkable = 1 << 1
    }
}
