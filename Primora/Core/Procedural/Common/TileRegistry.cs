using Primora.Core.Procedural.Objects;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Procedural.Common
{
    internal static class TileRegistry
    {
        private static readonly Dictionary<string, List<TileVariant>> _lookup = [];
        private static readonly Dictionary<int, TileVariant> _tileVariants = [];

        static TileRegistry()
        {
            // Load and cache all variants
            _lookup = CreateTileVariants(LoadTileDefinitions());

            // Cache variants by id
            foreach (var variant in _lookup.Values.SelectMany(a => a))
                _tileVariants[variant.Id] = variant;
        }

        /// <summary>
        /// Returns true if the specified tile variant id exists in the cache.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Exists(int id) => id == 0 || _tileVariants.ContainsKey(id);

        /// <summary>
        /// Gets the cached tile variant by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TileVariant GetVariant(int id)
        {
            if (id == 0) return null; // Void tile
            if (_tileVariants.TryGetValue(id, out var variant)) return variant;
            throw new Exception($"No tile variant exists with id \"{id}\".");
        }

        /// <summary>
        /// Returns a random tile variant out of the cache based on the given tile definition key and biome.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="tileDefinitionKey"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TileVariant GetRandomVariant(string biome, string tileDefinitionKey, Random random = null)
        {
            if (string.IsNullOrWhiteSpace(biome))
                throw new ArgumentNullException(nameof(biome));
            if (string.IsNullOrWhiteSpace(tileDefinitionKey))
                throw new ArgumentNullException(nameof(tileDefinitionKey));

            random ??= Constants.General.Random;
            if (_lookup.TryGetValue($"{biome}_{tileDefinitionKey}", out var values))
                return values[random.Next(0, values.Count)];

            throw new Exception($"No tile variants found by tile definition key \"{tileDefinitionKey}\".");
        }

        private static Dictionary<string, List<TileVariant>> CreateTileVariants(Dictionary<string, TileDefinition> tileDefinitions)
        {
            var result = new Dictionary<string, List<TileVariant>>();
            var biomes = Enum.GetValues<Biome>();
            int idCounter = 0;

            foreach (var kvp in tileDefinitions)
            {
                var def = kvp.Value;
                var variants = new List<TileVariant>();

                // Default variants for each biome
                foreach (var biome in biomes)
                    ExpandGroup(def.Default, def, ref idCounter, variants, biome);

                // Biome overrides
                foreach (var biomeKvp in def.BiomeOverrides)
                {
                    if (!Enum.TryParse(biomeKvp.Key, true, out Biome biome))
                        throw new Exception($"Invalid biome \"{biomeKvp.Key}\".");

                    // Remove default for this biome, as it has an override
                    variants.RemoveAll(a => a.Biome == biome);

                    // Add new variants for this biome
                    ExpandGroup(biomeKvp.Value, def, ref idCounter, variants, biome);
                }

                // Setup for each biome a unique key
                var groups = variants.GroupBy(a => a.Biome);
                foreach (var group in groups)
                    result[$"{group.Key}_{def.Key}"] = [.. group];
            }

            return result;
        }

        private static void ExpandGroup(TileDefinition.TileVariantGroup group, TileDefinition def,
            ref int idCounter, List<TileVariant> output, Biome biome)
        {
            if (group == null)
                return;

            // Ensure atleast one value
            group.Glyph ??= [0];
            if (group.Glyph.Count == 0)
                group.Glyph.Add(0);

            // Ensure atleast one value
            group.Foreground ??= [Color.Transparent];
            if (group.Foreground.Count == 0)
                group.Foreground.Add(Color.Transparent);

            // Ensure atleast one value
            group.Background ??= [Color.Transparent];
            if (group.Background.Count == 0)
                group.Background.Add(Color.Transparent);

            foreach (var glyph in group.Glyph)
            {
                foreach (var fg in group.Foreground)
                {
                    foreach (var bg in group.Background)
                    {
                        var variant = new TileVariant
                        {
                            Id = ++idCounter,
                            Key = def.Key,
                            Name = def.Name,
                            Glyph = glyph,
                            Foreground = fg,
                            Background = bg,
                            Obstruction = def.Obstruction,
                            Biome = biome
                        };

                        output.Add(variant);
                    }
                }
            }
        }

        private static Dictionary<string, TileDefinition> LoadTileDefinitions()
        {
            var tileTypesPath = Constants.GameData.TileTypesDataPath;
            if (!File.Exists(tileTypesPath))
                throw new Exception($"Missing game data file \"{Path.GetFileName(tileTypesPath)}\" at path \"{tileTypesPath}\".");
            try
            {
                var tileDefinitions = JsonSerializer.Deserialize<List<TileDefinition>>(File.ReadAllText(tileTypesPath), Constants.General.SerializerOptions);
                return tileDefinitions.ToDictionary(a => a.Key, a => a);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to deserialize game data file \"{Path.GetFileName(tileTypesPath)}\", game data is corrupted:\n{e.Message}");
            }
        }
    }
}
