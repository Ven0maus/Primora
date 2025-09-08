using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora.Serialization
{
    internal class ColorArrayJsonConverter : JsonConverter<List<Color>>
    {
        private static readonly Dictionary<string, Color> _namedColors;

        static ColorArrayJsonConverter()
        {
            _namedColors = typeof(Color)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Color))
                .ToDictionary(f => f.Name, f => (Color)f.GetValue(null), StringComparer.OrdinalIgnoreCase);
        }

        public override List<Color> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<Color>();

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Parse an array of colors
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    result.Add(ReadColorValue(ref reader));
                }
            }
            else
            {
                // Single value → wrap into list
                result.Add(ReadColorValue(ref reader));
            }

            return result;
        }

        private static Color ReadColorValue(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Unexpected token {reader.TokenType} for Color");

            var hex = reader.GetString();

            if (string.IsNullOrWhiteSpace(hex))
                return Color.Transparent;

            if (hex.StartsWith('#'))
                hex = hex[1..];

            // Named color?
            if (_namedColors.TryGetValue(hex, out var color))
                return color;

            if (hex.Length != 6)
                throw new JsonException($"Invalid color string: {hex}");

            var r = Convert.ToByte(hex[..2], 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);

            return new Color(r, g, b);
        }

        public override void Write(Utf8JsonWriter writer, List<Color> value, JsonSerializerOptions options)
        {
            if (value.Count == 1)
            {
                WriteColor(writer, value[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var color in value)
                    WriteColor(writer, color);
                writer.WriteEndArray();
            }
        }

        private static void WriteColor(Utf8JsonWriter writer, Color color)
        {
            var hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            writer.WriteStringValue(hex);
        }
    }
}
