using Primora.Core.Items.Objects;

namespace Primora.Core.Items
{
    /// <summary>
    /// Absolute base for all items
    /// </summary>
    internal abstract class Item
    {
        public string Name { get; set; }
        public ItemCategory Category { get; set; }
        public ItemRarity Rarity { get; set; }

        public Item(ItemConfiguration configuration) 
        { 
            Name = configuration.Name;
            Category = configuration.Category;
            Rarity = configuration.Rarity;
        }
    }
}
