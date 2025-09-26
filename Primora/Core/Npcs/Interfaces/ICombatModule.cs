using Primora.Core.Npcs.AIModules;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface ICombatModule : IAIModule
    {
        void UpdateCombat(Actor self, Actor target);
    }
}
