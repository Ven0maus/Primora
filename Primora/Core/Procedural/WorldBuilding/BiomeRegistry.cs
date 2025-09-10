using Primora.Core.Procedural.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal static class BiomeRegistry
    {
        private readonly static Dictionary<Biome, BiomeDefinition> _biomeGlyphs;
        private readonly static List<(float min, float max, Biome biome)> _biomeNoiseLookup;

        static BiomeRegistry()
        {
            _biomeGlyphs = LoadBiomeGlyphDefinitions();
            _biomeNoiseLookup = [.. _biomeGlyphs.Values
                .OrderBy(a => a.MinNoise)
                .Select(a => (a.MinNoise, a.MaxNoise, a.Biome))];
        }

        /// <summary>
        /// Returns the matching definition for the specified biome.
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public static BiomeDefinition Get(Biome biome) 
            => _biomeGlyphs[biome];

        /// <summary>
        /// Gets all biome definitions ordered by noise level.
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<(float min, float max, Biome biome)> GetBiomesByNoise() => _biomeNoiseLookup;

        private static Dictionary<Biome, BiomeDefinition> LoadBiomeGlyphDefinitions()
        {
            var json = File.ReadAllText(Constants.GameData.Biomes);

            List<BiomeDefinition> biomeDefinitions;
            try
            {
                biomeDefinitions = JsonSerializer.Deserialize<List<BiomeDefinition>>(json, Constants.General.SerializerOptions);
            }
            catch(Exception e)
            {
                throw new Exception($"Unable to load file \"{Constants.GameData.Biomes}\": {e.Message}", e);
            }

            return biomeDefinitions.ToDictionary(a => Enum.Parse<Biome>(a.Name, true), a => a);
        }
    }
}
