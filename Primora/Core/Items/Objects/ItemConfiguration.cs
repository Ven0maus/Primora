using Primora.GameData.EditorObjects;
using Primora.GameData.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Primora.Core.Items.Objects
{
    /// <summary>
    /// Configuration holder, deserialized from items.json configuration
    /// </summary>
    public class ItemConfiguration
    {
        // Base configuration data
        public string Name { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        // Quick access variables
        public ItemRarity Rarity { get; set; }
        public ItemCategory Category { get; set; }
        public EquipmentSlot EquipmentSlot { get; set; }

        // Quick access stats
        public ItemStats ProvidedStats { get; set; }

        public ItemConfiguration(ItemObject itemObject)
        {
            // Set base data
            Name = itemObject.Name;
            Attributes = itemObject.Attributes;

            // Load quick access variables
            Rarity = GameDataLoader.GetAttribute<ItemRarity>(Attributes, nameof(ItemRarity));
            Category = GameDataLoader.GetAttribute<ItemCategory>(Attributes, nameof(ItemCategory));
            EquipmentSlot = GameDataLoader.GetAttribute<EquipmentSlot>(Attributes, nameof(EquipmentSlot));

            // Load quick access stats
            ProvidedStats = new ItemStats(Attributes);
        }

        // Static cache
        private static readonly Dictionary<ItemCategory, ItemConfiguration[]> _itemConfigurations;

        static ItemConfiguration()
        {
            _itemConfigurations = [];

            // Load game data into static cache
            foreach (var item in GameDataLoader.Load<ItemObject>(Constants.GameData.Items)
                .Select(a => new ItemConfiguration(a.Value))
                .GroupBy(a => a.Category))
            {
                _itemConfigurations[item.Key] = [.. item];
            }

            // Informative debugging
            foreach (var kvp in _itemConfigurations)
                Debug.WriteLine($"Loaded {kvp.Value.Length} \"{kvp.Key}\" items");
        }

        public static ItemConfiguration[] Get(ItemCategory category)
        {
            if (_itemConfigurations.TryGetValue(category, out var configuration))
                return configuration;
            throw new NotImplementedException($"No items available for category \"{category}\".");
        }
    }
}
