using Primora.Core.Npcs.AIModules;
using System.Collections.Generic;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface IAwarenessModule : IAIModule
    {
        Actor Detect(Actor self, IEnumerable<Actor> potentionTargets);
    }
}
