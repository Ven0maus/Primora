namespace Primora.Core.Items.Interfaces
{
    internal interface IEquipable
    {
        EquipmentSlot EquipmentSlot { get; }
        ItemStats ProvidedStats { get; }
    }
}
