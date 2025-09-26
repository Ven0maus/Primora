using Primora.Core.Npcs.Interfaces;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Awareness
{
    internal class SightAwareness : IAwarenessModule
    {
        public IEnumerable<Actor> Detect(Actor self)
        {
            yield break;
        }
    }
}
