using Primora.Core.Procedural.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Procedural.WorldBuilding.Registries
{
    internal static class BiomeRegistry
    {
        private readonly static Dictionary<Biome, BiomeDefinition> _biomeGlyphs;

        static BiomeRegistry()
        {
            _biomeGlyphs = LoadBiomeGlyphDefinitions();
        }

        /// <summary>
        /// Returns the matching definition for the specified biome.
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public static BiomeDefinition Get(Biome biome) 
            => _biomeGlyphs[biome];

        /// <summary>
        /// Gets all biome definitions.
        /// </summary>
        /// <returns></returns>
        public static ICollection<BiomeDefinition> GetAll() 
            => _biomeGlyphs.Values;

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
