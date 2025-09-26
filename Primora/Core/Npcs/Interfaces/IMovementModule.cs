using Primora.Core.Npcs.AIModules;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface IMovementModule : IAIModule
    {
        void UpdateMovement(Actor self, Actor target);
    }
}
