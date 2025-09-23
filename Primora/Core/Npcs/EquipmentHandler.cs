using Primora.Core.Items;
using Primora.Core.Items.Interfaces;
using Primora.Core.Npcs.Actors;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Primora.Core.Npcs
{
    internal class EquipmentHandler : IEnumerable<(EquipmentSlot slot, IEquipable item)>
    {
        private readonly Dictionary<EquipmentSlot, IEquipable> _equipedItems = new(Enum.GetValues<EquipmentSlot>().Length);

        public IEquipable GetEquipment(EquipmentSlot slot)
            => _equipedItems.GetValueOrDefault(slot);

        public void Equip<T>(T item) where T : Item, IEquipable
        {
            if (item == null) return;
            if (!_equipedItems.ContainsKey(item.EquipmentSlot))
            {
                _equipedItems[item.EquipmentSlot] = item;
            }
            else
            {
                Unequip(item.EquipmentSlot);
                _equipedItems[item.EquipmentSlot] = item;
            }

            // Add provided item stats
            if (item.ProvidedStats != null)
                Player.Instance.Stats.AddItemStats(item.ProvidedStats);
        }

        public void Unequip(EquipmentSlot slot)
        {
            if (_equipedItems.TryGetValue(slot, out var equipedItem))
            {
                // TODO: Add a check for inventory space

                _equipedItems.Remove(slot);

                // Remove provided item stats
                if (equipedItem.ProvidedStats != null)
                    Player.Instance.Stats.RemoveItemStats(equipedItem.ProvidedStats);

                // TODO: Add item back to inventory
            }
        }

        public void UnequipAll()
        {
            // TODO: Add a check for inventory space

            foreach (var slot in _equipedItems.Keys)
                Unequip(slot);
        }

        public IEnumerator<(EquipmentSlot slot, IEquipable item)> GetEnumerator()
        {
            foreach (var item in _equipedItems)
                yield return (item.Key, item.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
