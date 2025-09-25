using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Items
{
    /// <summary>
    /// Configuration holder, deserialized from item's json configuration
    /// </summary>
    public class ItemConfiguration
    {
        // Shared configuration
        public string Name { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemCategory Category { get; set; }

        // Equipment configuration
        public EquipmentSlot EquipmentSlot { get; set; }
        public ItemStats ProvidedStats { get; set; }

        private static readonly Dictionary<ItemCategory, ItemConfiguration[]> _itemConfigurations;

        static ItemConfiguration()
        {
            _itemConfigurations = [];

            // Insert item configurations into cache
            foreach (var item in LoadItems().GroupBy(a => a.Category))
                _itemConfigurations[item.Key] = [.. item];

            // Informative debugging
            foreach (var kvp in _itemConfigurations)
                Debug.WriteLine($"Loaded {kvp.Value.Length} \"{kvp.Key}\" items");
        }

        public static ItemConfiguration[] Get(ItemCategory category)
        {
            if (_itemConfigurations.TryGetValue(category, out var configuration))
                return configuration;
            throw new NotImplementedException($"Category \"{category}\" has not yet been implemented.");
        }

        private static List<ItemConfiguration> LoadItems()
        {
            if (!File.Exists(Constants.GameData.Items))
                return [];

            try
            {
                var json = File.ReadAllText(Constants.GameData.Items);
                var configs = JsonSerializer.Deserialize<List<ItemConfiguration>>(json, Constants.General.SerializerOptions);
                return configs;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to process file \"{Constants.GameData.Items}\": {e.Message}", e);
            }
        }
    }
}
