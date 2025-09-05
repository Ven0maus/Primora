using Primora.Serialization;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Text.Json.Serialization;

namespace Primora.Core
{
    internal class TileType
    {
        /// <summary>
        /// The unique id for the tiletype.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The name of the tiletype.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The foreground color defined by this tiletype.
        /// </summary>
        public Color Foreground { get; set; }
        /// <summary>
        /// The background color defined by this tiletype.
        /// </summary>
        public Color Background { get; set; }
        /// <summary>
        /// The glyph character defined by this tiletype.
        /// </summary>
        [JsonConverter(typeof(GlyphJsonConverter))]
        public int Glyph { get; set; }
        /// <summary>
        /// The type of obstruction defined by this tiletype.
        /// </summary>
        public Obstruction Obstruction { get; set; }

        private ColoredGlyphBase _cellAppearance;
        /// <summary>
        /// The object used for rendering the appearance of this tiletype.
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
