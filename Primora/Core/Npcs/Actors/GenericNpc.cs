using Primora.Core.Procedural.Objects;
using SadRogue.Primitives;

namespace Primora.Core.Npcs.Actors
{
    internal class GenericNpc : Actor
    {
        public GenericNpc(ILocation location, Point position, string npc) 
            : base(location, position, npc)
        { }
    }
}
