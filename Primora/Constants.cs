using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora
{
    internal static class Constants
    {
        internal static class General
        {
            internal const string GameTitle = "Primora";
            internal static readonly (int width, int height) DefaultWindowSize = (1920, 1080);
            internal static readonly Random Random = new();
            internal static readonly JsonSerializerOptions SerializerOptions = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        internal static class GameData
        {
            internal const string Biomes = "GameData/Biomes.json";
        }
    }
}
