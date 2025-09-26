using Primora.Core.Npcs.AIModules;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface IDecisionModule : IAIModule
    {
        void Decide(Actor self, Actor target);
    }
}
