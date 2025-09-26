using Primora.Core.Items.Objects;
using Primora.Core.Items.Types;
using System;
using System.Linq;

namespace Primora.Core.Items
{
    /// <summary>
    /// Static helper used for loot generation.
    /// </summary>
    internal static class Loot
    {
        /// <summary>
        /// Generates a specific loot item from the specified item category, and optional specific criteria.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        internal static Item Generate(Random random, ItemCategory category, ItemRarity rarity, Func<ItemConfiguration, bool> criteria = null)
        {
            var configurations = ItemConfiguration.Get(category)
                .Where(a => a.Rarity == rarity)
                .ToList();

            // Further refine based on criteria
            if (criteria != null)
                configurations.RemoveAll(a => !criteria.Invoke(a));

            if (configurations.Count == 0)
                return null;

            var selectedConfiguration = configurations[random.Next(configurations.Count)];

            return category switch
            {
                ItemCategory.Consumable => new Consumable(selectedConfiguration),
                ItemCategory.Equipment => new Equipment(selectedConfiguration),
                ItemCategory.Material => new Material(selectedConfiguration),
                _ => throw new NotImplementedException($"Category \"{category}\" has no loot generation method available."),
            };
        }
    }
}
