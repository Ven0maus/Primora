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
            internal static int GameSeed = 1924984455; // new Random().Next();

            internal static readonly JsonSerializerOptions SerializerOptions = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        internal static class Zone
        {
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
