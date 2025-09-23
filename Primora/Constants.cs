using System.Text.Json;
using System.Text.Json.Serialization;

namespace Primora
{
    internal static class Constants
    {
        internal static class General
        {
            internal const int FontGlyphWidth = 16;
            internal const int FontGlyphHeight = 16;

            internal const string GameTitle = "Primora";
            internal static int GameSeed = new System.Random().Next();
            internal const int GameStartHour = 12; // 24 hour format

            internal static readonly JsonSerializerOptions SerializerOptions = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        internal static class Worldmap
        {
            internal const int DefaultWidth = 240;
            internal const int DefaultHeight = 120;
            internal const int TurnsPerTile_FastTravel = 3; // This is affected by terrain weights too
            internal const double FoodConsumptionPerTurn_FastTravel = 0.1;
        }

        internal static class Zone
        {
            internal const int DefaultWidth = 75;
            internal const int DefaultHeight = 50;
            internal const float ZoneSizeModifier = 1.5f; // increase by half original
            internal const int ZoneCacheTTLInTurns = 30;
        }

        internal static class Npcs
        {
            internal const int NpcZIndex = 1;
            internal static readonly (int min, int max) PlayerSpawnDistanceFromSettlements = (10, 20); // How far from a settlement to spawn
            internal const int PlayerSpawnZoneClearRadius = 4; // How many tiles radius there is no obstruction on player spawn.
        }

        internal static class GameData
        {
            internal const string Biomes = "GameData/Biomes.json";
            internal const string ActorDefinitions = "GameData/ActorDefinitions.json";
        }
    }
}
