using Primora.Core.Procedural.Objects;
using Primora.GameData.EditorObjects;
using Primora.GameData.Helpers;
using Primora.Serialization;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Primora.Core.Npcs.Objects
{
    internal sealed class ActorDefinition
    {
        public string Name { get; set; }
        public int MaxHealth { get; set; }
        public int Damage { get; set; }
        public int Armour { get; set; }
        public int Evasion { get; set; }
        public Color Color { get; set; }
        public int Glyph { get; set; }
        public Rarity Rarity { get; set; }
        public Biome[] SpawnInBiomes { get; set; }

        public ActorDefinition(NpcObject npcObject)
        {
            Name = npcObject.Name;
            MaxHealth = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(MaxHealth));
            Damage = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Damage));
            Armour = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Armour));
            Evasion = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Evasion));
            Color = GameDataLoader.GetAttribute<Color>(npcObject.Attributes, nameof(Color));
            Glyph = GameDataLoader.GetAttribute<char>(npcObject.Attributes, nameof(Glyph));
            Rarity = GameDataLoader.GetAttribute<Rarity>(npcObject.Attributes, nameof(Rarity));
            SpawnInBiomes = GameDataLoader.GetAttribute<Biome[]>(npcObject.Attributes, nameof(SpawnInBiomes));
        }

        // Static cache
        private static readonly Dictionary<string, ActorDefinition> _actorDefinitions;

        static ActorDefinition()
        {
            _actorDefinitions = [];

            // Load game data into static cache
            foreach (var item in GameDataLoader.Load<NpcObject>(Constants.GameData.Npcs)
                .Select(a => new ActorDefinition(a.Value)))
            {
                _actorDefinitions[item.Name] = item;
            }

            // Informative debugging
            var groups = _actorDefinitions.Values
                .SelectMany(e =>
                    (e.SpawnInBiomes != null
                        ? e.SpawnInBiomes.Cast<Biome?>()
                        : [null])
                    .Select(b => new { Biome = b, ActorDefinition = e }))
                .GroupBy(x => x.Biome, x => x.ActorDefinition);

            Debug.WriteLine("Total unique npcs loaded: " + _actorDefinitions.Count);
            foreach (var group in groups)
                Debug.WriteLine($"Loaded {group.Count()} npcs for Biome type \"{(group.Key == null ? "undefined" : group.Key)}\".");
        }

        /// <summary>
        /// Gets the cached actor definition for the specified entity.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ActorDefinition Get(string npc)
        {
            if (_actorDefinitions.TryGetValue(npc, out var definition))
                return definition;
            throw new NotImplementedException($"Entity \"{npc}\" is not implemented.");
        }

        /// <summary>
        /// Gets the cached actor definition for the specified entity.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ActorDefinition[] Get(Biome biome)
        {
            return [.. _actorDefinitions.Values.Where(a => a.SpawnInBiomes != null && a.SpawnInBiomes.Contains(biome))];
        }
    }
}
