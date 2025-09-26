using Primora.Core.Npcs.Interfaces;

namespace Primora.Core.Npcs.AIModules.Combat
{
    internal class HybridCombat : ICombatModule
    {
        public void UpdateCombat(Actor self)
        {
            // TODO:
            // Prefer ranged attacks if we have ranged weapon in inventory or equipped
            // and distance between self and target is > 1 tile

            // Otherwise fallback to melee attacks
        }
    }
}
