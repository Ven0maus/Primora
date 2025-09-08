using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora.Serialization
{
    internal class GlyphArrayJsonConverter : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<int>();

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Parse an array of glyphs
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    result.Add(ReadGlyphValue(ref reader));
                }
            }
            else
            {
                // Single value → wrap into list
                result.Add(ReadGlyphValue(ref reader));
            }

            return result;
        }

        private static int ReadGlyphValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var str = reader.GetString();
                    return string.IsNullOrEmpty(str) ? 0 : str[0];
                case JsonTokenType.Number:
                    return reader.GetInt32();
                default:
                    throw new JsonException($"Unexpected token {reader.TokenType} for Glyph");
            }
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            if (value.Count == 1)
            {
                WriteGlyph(writer, value[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var glyph in value)
                    WriteGlyph(writer, glyph);
                writer.WriteEndArray();
            }
        }

        private static void WriteGlyph(Utf8JsonWriter writer, int value)
        {
            if (value >= 32 && value <= 126)
                writer.WriteStringValue(((char)value).ToString());
            else
                writer.WriteNumberValue(value);
        }
    }
}
