using Primora.Core.Items;
using Primora.Core.Npcs.Actors;
using Primora.Extensions;
using SadConsole;
using System;
using System.Collections.Generic;

namespace Primora.Screens
{
    internal class EquipmentScreen : TextScreen
    {
        private const string Title = "Equipment";
        private readonly Dictionary<EquipmentSlot, Item> _equipedItems = new(Enum.GetValues<EquipmentSlot>().Length);

        public EquipmentScreen(int width, int height) : 
            base(Title, width, height)
        { }

        public Item GetEquipment(EquipmentSlot slot) 
            => _equipedItems.GetValueOrDefault(slot);

        public void Equip<T>(T item) where T : Item, IEquipable
        {
            if (item == null) return;
            if (!_equipedItems.ContainsKey(item.EquipmentSlot))
            {
                _equipedItems[item.EquipmentSlot] = item;

                // Add provided item stats
                Player.Instance.Stats.AddItemStats(item.ProvidedStats);
            }
        }

        public void Unequip<T>(T item) where T : Item, IEquipable
        {
            if (item == null) return;
            if (_equipedItems.TryGetValue(item.EquipmentSlot, out var equipedItem) &&
                equipedItem.Equals(item))
            {
                _equipedItems.Remove(item.EquipmentSlot);

                // Remove provided item stats
                Player.Instance.Stats.RemoveItemStats(item.ProvidedStats);
            }
        }

        public override void UpdateDisplay()
        {
            View.Clear();

            // Define for each slot its item(s)
            var slots = Enum.GetValues<EquipmentSlot>();
            int row = 1;
            foreach (var slot in slots)
            {
                // Slot name
                View.Print(1, row++, $"[{slot.ToString().Replace("_", " ")} Slot]", "#303538".HexToColor());

                if (_equipedItems.TryGetValue(slot, out var equipment))
                {
                    // Equipment item name
                    View.Print(1, row++, $"- {equipment.Name}", "#adadad".HexToColor());
                }

                row++;
            }
        }
    }
}
