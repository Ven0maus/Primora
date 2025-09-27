using Primora.GameData.Helpers;
using System.Collections.Generic;

namespace Primora.Core.Items.Objects
{
    public class ItemStats
    {
        // Offensive stats
        public int AttackRange { get; set; }
        public int Damage { get; set; }

        // Defensive stats
        public int Armour { get; set; }
        public int Evasion { get; set; }

        // For deepclone access
        private ItemStats()
        { }

        public ItemStats(Dictionary<string, object> attributes)
        {
            // Offensive
            AttackRange = GameDataLoader.GetAttribute<int>(attributes, nameof(AttackRange));
            Damage = GameDataLoader.GetAttribute<int>(attributes, nameof(Damage));

            // Defensive
            Armour = GameDataLoader.GetAttribute<int>(attributes, nameof(Armour));
            Evasion = GameDataLoader.GetAttribute<int>(attributes, nameof(Evasion));
        }

        /// <summary>
        /// Makes a deep clone of the item stats.
        /// </summary>
        /// <returns></returns>
        public ItemStats DeepClone()
        {
            return new ItemStats
            {
                AttackRange = AttackRange,
                Damage = Damage,
                Armour = Armour,
                Evasion = Evasion
            };
        }
    }
}
