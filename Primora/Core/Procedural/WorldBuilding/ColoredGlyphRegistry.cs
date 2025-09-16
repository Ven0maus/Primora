using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class ColoredGlyphRegistry
    {
        private static readonly Dictionary<GlyphKey, ColoredGlyph> _registry = [];

        /// <summary>
        /// How often a new glyph was created and cached.
        /// </summary>
        internal static int NewlyCreated { get; private set; }
        /// <summary>
        /// How often a glyph was retrieved from the cache.
        /// </summary>
        internal static int CachedRetrievals { get; private set; }

        /// <summary>
        /// Gets a cached ColoredGylph similar to the provided one, or creates a new one.
        /// <br>Color values will be quantized to 256 shade range instead of 16 million for memory reduction.</br>
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="mirror"></param>
        /// <returns></returns>
        internal static ColoredGlyph GetOrCreate(int glyph, Color foreground, Color background, Mirror mirror = Mirror.None)
        {
            var key = AsGlyphKey(glyph, foreground, background, mirror);
            return GetOrCreate(key);
        }

        /// <summary>
        /// Gets a cached ColoredGylph similar to the provided one, or creates a new one.
        /// <br>Color values will be quantized to 256 shade range instead of 16 million for memory reduction.</br>
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="mirror"></param>
        /// <returns></returns>
        internal static ColoredGlyph GetOrCreate(GlyphKey key)
        {
            if (!_registry.TryGetValue(key, out var coloredGlyph))
            {
                ++NewlyCreated;
                _registry[key] = coloredGlyph = new ColoredGlyph(key.Foreground, key.Background, key.Glyph, key.Mirror);
            }
            else
            {
                ++CachedRetrievals;
            }
            return coloredGlyph;
        }

        internal static GlyphKey AsGlyphKey(int glyph, Color foreground, Color background, Mirror mirror = Mirror.None)
        {
            var quantizedFg = Quantize(foreground);
            var quantizedBg = Quantize(background);

            return new GlyphKey(glyph, quantizedFg, quantizedBg, mirror);
        }

        internal static GlyphKey AsGlyphKey(ColoredGlyph coloredGlyph)
        {
            var quantizedFg = Quantize(coloredGlyph.Foreground);
            var quantizedBg = Quantize(coloredGlyph.Background);

            return new GlyphKey(coloredGlyph.Glyph, quantizedFg, quantizedBg, coloredGlyph.Mirror);
        }

        /// <summary>
        /// Instead of 16 million RGB colors, reduce to e.g. 256 allowed shades per biome.
        /// That way, many “almost green” colors collapse into the same cached entry, same goes for all colors.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private static Color Quantize(Color input, int step = 8)
        {
            return new Color(
                (input.R / step) * step,
                (input.G / step) * step,
                (input.B / step) * step
            );
        }
    }

    internal readonly struct GlyphKey(int glyph, Color fg, Color bg, Mirror mirror) : IEquatable<GlyphKey>
    {
        public int Glyph { get; } = glyph;
        public Color Foreground { get; } = fg;
        public Color Background { get; } = bg;
        public Mirror Mirror { get; } = mirror;

        public GlyphKey() : this(0, Color.Transparent, Color.Black, Mirror.None)
        { }

        public bool Equals(GlyphKey other) =>
            Glyph == other.Glyph &&
            Foreground.Equals(other.Foreground) &&
            Background.Equals(other.Background) &&
            Mirror == other.Mirror;

        public override bool Equals(object obj) =>
            obj is GlyphKey other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Glyph, Foreground, Background, Mirror);
    }
}
