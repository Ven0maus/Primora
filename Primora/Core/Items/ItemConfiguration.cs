using GoRogue.DiceNotation;
using Primora.Core.Items.EditorObjects;
using Primora.Core.Items.Objects;
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

        public Dictionary<string, object> Attributes { get; set; }

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

        public ItemConfiguration(ItemObject itemObject)
        {
            Name = itemObject.Name;
            var data = itemObject.Attributes;
            Attributes = [];

            // TODO: Make a helper to parse JsonElement data for all items, attributes, npcs
            foreach (var kv in data)
            {
                var jsonElement = (JsonElement)kv.Value;
                object value = jsonElement.ValueKind switch
                {
                    JsonValueKind.String => jsonElement.GetString(),
                    JsonValueKind.Number => ConvertNumber(jsonElement),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Object => jsonElement, // or deserialize to Dictionary<string, object>
                    JsonValueKind.Array => jsonElement, // or deserialize to List<object>
                    _ => jsonElement
                };

                Attributes[kv.Key] = value;
            }

            // Load quick access variables:
            Rarity = Enum.Parse<ItemRarity>((string)Attributes["ItemRarity"], true);
            Category = Enum.Parse<ItemCategory>((string)Attributes["ItemCategory"], true);
            EquipmentSlot = Enum.Parse<EquipmentSlot>((string)Attributes["EquipmentSlot"], true);
        }

        private static object ConvertNumber(JsonElement element)
        {
            if (element.TryGetInt32(out int i))
                return i; // prefer int
            if (element.TryGetInt64(out long l))
                return l; // fallback to long
            return element.GetDouble(); // floating point
        }

        public static ItemConfiguration[] Get(ItemCategory category)
        {
            if (_itemConfigurations.TryGetValue(category, out var configuration))
                return configuration;
            throw new NotImplementedException($"No items available for category \"{category}\".");
        }

        private static IEnumerable<ItemConfiguration> LoadItems()
        {
            if (!File.Exists(Constants.GameData.Items))
                yield break;

            Dictionary<string, ItemObject> configs;
            try
            {
                var json = File.ReadAllText(Constants.GameData.Items);
                configs = JsonSerializer.Deserialize<Dictionary<string, ItemObject>>(json, Constants.General.SerializerOptions);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to process file \"{Constants.GameData.Items}\": {e.Message}", e);
            }

            foreach (var value in configs)
            {
                value.Value.Name = value.Key;
                yield return new ItemConfiguration(value.Value);
            }
        }
    }
}
