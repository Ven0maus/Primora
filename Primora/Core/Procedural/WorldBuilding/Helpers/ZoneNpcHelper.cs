using Primora.Core.Npcs;
using Primora.Core.Npcs.Actors;
using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.Objects;
using Primora.GameData.EditorObjects;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Primora.Core.Procedural.WorldBuilding.Helpers
{
    /// <summary>
    /// Helper class to generate and assign npcs within a zone.
    /// </summary>
    internal static class ZoneNpcHelper
    {
        private static readonly Dictionary<Rarity, int> _probabilities = new()
        {
            { Rarity.Common, 60 },
            { Rarity.Uncommon, 25 },
            { Rarity.Rare, 10 },
            { Rarity.Epic, 4 },
            { Rarity.Mythic, 1 }
        };

        public static void GenerateNpcs(Zone zone)
        {
            int zoneTiles = zone.Width * zone.Height;

            // 1. Roll density range for biome
            var (minDensity, maxDensity) = GetZoneDensity(zone);
            double density = zone.Random.NextDouble() * (maxDensity - minDensity) + minDensity;
            int npcCount = (int)Math.Round(zoneTiles / 100.0d * density);

            var actorDefinitions = ActorDefinition.Get(zone.WorldTileInfo.Biome)
                .GroupBy(a => a.Rarity)
                .ToDictionary(a => a.Key, a => a.ToArray());
            var validRarityBuckets = actorDefinitions.Keys.ToHashSet();

            int succes = 0;
            for (int i = 0; i < npcCount; i++)
            {
                // 2. Pick rarity bucket first
                var rarity = RollRarityBucket(zone.Random, validRarityBuckets);

                // 3. Filter NPCs that match biome + rarity
                if (!actorDefinitions.TryGetValue(rarity, out var definitions) || definitions.Length == 0)
                    continue;

                // 4. Pick from candidates using weights
                var actorDefinition = PickWeightedActorDefinition(definitions, zone.Random);
                if (actorDefinition == null) continue;

                // Automatically cached in ActorManager
                _ = new GenericNpc(zone, GetValidNpcPosition(zone), actorDefinition);
                succes++;
            }

            Debug.WriteLine($"Generated \"{succes}/{npcCount}\" npcs in zone{zone.WorldPosition} with density \"{density}\".");
        }

        private static Point GetValidNpcPosition(Zone zone)
        {
            var totalIndexLength = zone.Width * zone.Height;
            var index = zone.Random.Next(0, totalIndexLength);
            var pos = Point.FromIndex(index, zone.Width);
            var valid = zone.GetTileInfo(pos).Walkable && !ActorManager.ActorExistsAt(zone, pos, out _);

            int maxAttempts = (zone.Width * zone.Height) * 2;
            int attempts = 0;
            while (!valid && attempts < maxAttempts)
            {
                index = zone.Random.Next(0, totalIndexLength);
                pos = Point.FromIndex(index, zone.Width);
                valid = zone.GetTileInfo(pos).Walkable && !ActorManager.ActorExistsAt(zone, pos, out _);
            }

            if (!valid)
                throw new Exception("No valid spawn position found.");

            return pos;
        }

        private static (float min, float max) GetZoneDensity(Zone zone)
        {
            return zone.WorldTileInfo.Biome switch
            {
                Biome.Grassland => (0.05f, 0.3f),
                Biome.Forest => (0.25f, 0.5f),
                Biome.Mountains => (0.05f, 0.15f),
                Biome.Hills => (0.05f, 0.2f),
                Biome.River => (0.0125f, 0.125f),
                _ => (0.1f, 1.0f)
            };
        }

        private static Rarity RollRarityBucket(Random random, HashSet<Rarity> validRarities)
        {
            var valid = _probabilities
                    .Where(kv => validRarities.Contains(kv.Key))
                    .ToArray();

            if (valid.Length == 0)
                return Rarity.Common; // or throw

            int roll = random.Next(0, valid.Sum(kv => kv.Value));
            int cumulative = 0;

            foreach (var kv in valid)
            {
                cumulative += kv.Value;
                if (roll < cumulative)
                    return kv.Key;
            }

            return valid.Last().Key; // should never happen
        }

        public static ActorDefinition PickWeightedActorDefinition(ActorDefinition[] candidates, Random random)
        {
            int totalWeight = candidates.Sum(n => n.RollWeight);
            if (totalWeight <= 0)
                return null;

            int roll = random.Next(0, totalWeight);
            int cumulative = 0;

            foreach (var npc in candidates)
            {
                cumulative += npc.RollWeight;
                if (roll < cumulative)
                    return npc;
            }

            // Fallback
            return candidates[random.Next(candidates.Length)];
        }
    }
}
