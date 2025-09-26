using System.Collections.Generic;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface IDecisionModule
    {
        void Decide(Actor self, IEnumerable<Actor> detectedTargets);
    }
}
