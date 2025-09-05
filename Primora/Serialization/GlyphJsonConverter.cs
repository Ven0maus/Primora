using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora.Serialization
{
    internal class GlyphJsonConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var str = reader.GetString();
                    if (string.IsNullOrEmpty(str))
                        return 0;

                    // Take first character
                    return str[0];

                case JsonTokenType.Number:
                    return reader.GetInt32();

                default:
                    throw new JsonException($"Unexpected token {reader.TokenType} for Glyph");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            // By default, write glyphs as a single char if printable, else as number
            if (value >= 32 && value <= 126)
                writer.WriteStringValue(((char)value).ToString());
            else
                writer.WriteNumberValue(value);
        }
    }
}
