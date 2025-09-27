using Primora.Core.Procedural.Objects;
using Primora.GameData.EditorObjects;
using Primora.GameData.Helpers;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Primora.Core.Npcs.Objects
{
    internal sealed class ActorDefinition
    {
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public AwarenessType[] AwarenessTypes { get; set; }
        public DecisionType DecisionType { get; set; }
        public Faction Faction { get; set; }
        public Race Race { get; set; }

        // Constitution
        public int MaxHealth { get; set; }

        // Offensive
        public int Damage { get; set; }

        // Defensive
        public int VisionRange { get; set; }
        public int Armour { get; set; }
        public int Evasion { get; set; }

        // Rendering
        public Color Color { get; set; }
        public int Glyph { get; set; }

        // Spawning / Rarity / RollWeights
        public Rarity Rarity { get; set; }
        public Biome[] SpawnInBiomes { get; set; }
        public int RollWeight { get; set; }

        public ActorDefinition(NpcObject npcObject)
        {
            Name = npcObject.Name;
            Attributes = npcObject.Attributes;
            Faction = GameDataLoader.GetAttribute<Faction>(npcObject.Attributes, nameof(Faction));
            Race = GameDataLoader.GetAttribute<Race>(npcObject.Attributes, nameof(Race));

            // Constitution
            MaxHealth = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(MaxHealth));

            // Offensive
            Damage = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Damage));

            // Defensive
            Armour = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Armour));
            Evasion = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(Evasion));
            VisionRange = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(VisionRange));

            // Rendering
            Color = GameDataLoader.GetAttribute<Color>(npcObject.Attributes, nameof(Color));
            Glyph = GameDataLoader.GetAttribute<char>(npcObject.Attributes, nameof(Glyph));

            // Spawning, rarity, rollweights
            Rarity = GameDataLoader.GetAttribute<Rarity>(npcObject.Attributes, nameof(Rarity));
            SpawnInBiomes = GameDataLoader.GetAttribute<Biome[]>(npcObject.Attributes, nameof(SpawnInBiomes));
            RollWeight = GameDataLoader.GetAttribute<int>(npcObject.Attributes, nameof(RollWeight));

            // AI Modules
            AwarenessTypes = GameDataLoader.GetAttribute<AwarenessType[]>(npcObject.Attributes, nameof(AwarenessTypes));
            DecisionType = GameDataLoader.GetAttribute<DecisionType>(npcObject.Attributes, nameof(DecisionType));
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
