using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Text.Json;

namespace Primora.Extensions
{
    internal static class MathUtils
    {
        public static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        public static float InverseLerp(float a, float b, float value)
        {
            if (Math.Abs(b - a) < 1e-6f) // avoid division by zero
                return 0f;

            return (value - a) / (b - a);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static Color HexToColor(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Color.Transparent; // or some default

            // Accept "#RRGGBB" or "RRGGBB"
            if (hex.StartsWith('#'))
                hex = hex[1..];

            // Support both named colors and hex colors
            if (hex.Length != 6)
                throw new JsonException($"Invalid color string: {hex}");

            var r = Convert.ToByte(hex[..2], 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);

            return new Color(r, g, b);
        }
    }
}
