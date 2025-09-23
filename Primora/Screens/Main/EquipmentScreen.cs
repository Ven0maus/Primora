using Primora.Core.Items;
using Primora.Core.Npcs.Actors;
using Primora.Extensions;
using Primora.Screens.Abstracts;
using SadConsole;
using System;

namespace Primora.Screens.Main
{
    internal class EquipmentScreen : TextScreen
    {
        private const string Title = "Equipment";

        public EquipmentScreen(int width, int height) : 
            base(Title, width, height)
        { }

        public override void UpdateDisplay()
        {
            View.Clear();

            // Define for each slot its item(s)
            var player = Player.Instance;
            var slots = Enum.GetValues<EquipmentSlot>();
            int row = 1;
            foreach (var slot in slots)
            {
                // Slot name
                View.Print(1, row++, $"[{slot.ToString().Replace("_", " ")} Slot]", "#303538".HexToColor());

                var equipment = player.Equipment.GetEquipment(slot);
                if (equipment != null)
                {
                    // Equipment item name
                    View.Print(1, row++, $"- {((Item)equipment).Name}", "#adadad".HexToColor());
                }

                row++;
            }
        }
    }
}
