using Primora.Core.Items.Interfaces;

namespace Primora.Core.Items.Types
{
    internal class Equipment : Item, IEquipable
    {
        public EquipmentSlot EquipmentSlot { get; }
        public ItemStats ProvidedStats { get; }

        public Equipment(ItemConfiguration configuration) : base(configuration)
        {
            EquipmentSlot = configuration.EquipmentSlot;
        }
    }
}
