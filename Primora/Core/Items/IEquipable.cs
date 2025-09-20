namespace Primora.Core.Items
{
    internal interface IEquipable
    {
        EquipmentSlot EquipmentSlot { get; }
        ItemStats ProvidedStats { get; }
    }
}
