using System.Collections.Generic;

namespace Primora.Core.Npcs.Interfaces
{
    internal interface IAwarenessModule
    {
        IEnumerable<Actor> Detect(Actor self);
    }
}
