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
            _itemConfigurations = new Dictionary<ItemCategory, ItemConfiguration[]>
            {
                // Load data of each item category
                [ItemCategory.Consumable] = LoadConsumables(),
                [ItemCategory.Equipment] = LoadEquipment()
            };

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

        private static ItemConfiguration[] LoadConsumables()
        {
            var consumableFiles = new[] { "Potions.json" };

            var configurations = new List<ItemConfiguration>();
            foreach (var filename in consumableFiles)
            {
                var filePath = Path.Combine(Constants.GameData.ItemDataFolder, "Consumables", filename);
                if (!File.Exists(filePath)) continue;

                try
                {
                    var json = File.ReadAllText(filePath);
                    var configs = JsonSerializer.Deserialize<List<ItemConfiguration>>(json, Constants.General.SerializerOptions);

                    // Set correct category
                    foreach (var config in configs)
                        config.Category = ItemCategory.Consumable;

                    configurations.AddRange(configs);
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to process file \"{filePath}\": {e.Message}", e);
                }
            }

            return [.. configurations];
        }

        private static ItemConfiguration[] LoadEquipment()
        {
            var equipmentSlots = Enum.GetValues<EquipmentSlot>()
                .Where(a => a != EquipmentSlot.None)
                .ToArray();

            var configurations = new List<ItemConfiguration>();
            foreach (var equipmentSlot in equipmentSlots)
            {
                var filePath = Path.Combine(Constants.GameData.ItemDataFolder, "Equipment", equipmentSlot.ToString() + ".json");
                if (!File.Exists(filePath)) continue;

                try
                {
                    var json = File.ReadAllText(filePath);
                    var configs = JsonSerializer.Deserialize<List<ItemConfiguration>>(json, Constants.General.SerializerOptions);

                    // Set correct equipment slot and category
                    foreach (var config in configs)
                    {
                        config.Category = ItemCategory.Equipment;
                        config.EquipmentSlot = equipmentSlot;
                    }

                    configurations.AddRange(configs);
                }
                catch (Exception e)
                {
                    throw new Exception($"Unable to process file \"{filePath}\": {e.Message}", e);
                }
            }
            return [.. configurations];
        }
    }
}
