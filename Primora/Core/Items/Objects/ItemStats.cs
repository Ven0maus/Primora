using System;
using System.Collections.Generic;

namespace Primora.Core.Items.Objects
{
    public class ItemStats
    {
        public int Damage { get; set; }
        public int Armour { get; set; }
        public int Evasion { get; set; }

        // For deepclone access
        private ItemStats()
        { }

        public ItemStats(Dictionary<string, object> attributes)
        {
            Damage = Read<int>(attributes, nameof(Damage));
            Armour = Read<int>(attributes, nameof(Armour));
            Evasion = Read<int>(attributes, nameof(Evasion));
        }

        /// <summary>
        /// Makes a deep clone of the item stats.
        /// </summary>
        /// <returns></returns>
        public ItemStats DeepClone()
        {
            return new ItemStats
            {
                Damage = Damage,
                Armour = Armour,
                Evasion = Evasion
            };
        }

        /// <summary>
        /// Read helper to parse attributes to the correct type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributes"></param>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static T Read<T>(Dictionary<string, object> attributes, string attributeKey)
        {
            if (attributes != null && attributes.TryGetValue(attributeKey, out var value))
            {
                // Direct type support (int, long, double)
                if (value is not string && value is T tValue)
                    return tValue;

                var type = typeof(T);

                // Double conversion support
                if (value is double fValue)
                {
                    if (type == typeof(float))
                        return (T)Convert.ChangeType((float)fValue, type);

                    throw new Exception($"Not support double conversion to type \"{type.Name}\".");
                }

                // Enum support
                if (value is string sValue && !string.IsNullOrWhiteSpace(sValue) && type == typeof(Enum))
                {
                    return (T)Enum.Parse(type, sValue, true);
                }

                throw new Exception($"Not supported attribute value conversion from \"{value.GetType().Name}\" to \"{type.Name}\".");
            }
            return default;
        }
    }
}
