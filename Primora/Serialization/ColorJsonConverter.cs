using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora.Serialization
{
    internal class ColorJsonConverter : JsonConverter<Color>
    {
        private static readonly Dictionary<string, Color> _namedColors;

        static ColorJsonConverter()
        {
            _namedColors = typeof(Color)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Color))
                .ToDictionary(f => f.Name, f => (Color)f.GetValue(null), StringComparer.OrdinalIgnoreCase);
        }

        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var hex = reader.GetString();

            if (string.IsNullOrWhiteSpace(hex))
                return Color.Transparent; // or some default

            // Accept "#RRGGBB" or "RRGGBB"
            if (hex.StartsWith('#'))
                hex = hex[1..];

            // Support both named colors and hex colors
            if (_namedColors.TryGetValue(hex, out var color))
                return color;
            else if (hex.Length != 6)
                throw new JsonException($"Invalid color string: {hex}");

            var r = Convert.ToByte(hex[..2], 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);

            return new Color(r, g, b);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            var hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            writer.WriteStringValue(hex);
        }
    }
}
