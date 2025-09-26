using Primora.Core.Procedural.WorldBuilding;
using SadRogue.Primitives;

namespace Primora.Core.Npcs.Actors
{
    internal class WorldMapEntity : Actor
    {
        public WorldMapEntity(Point position, string npc) : 
            base(World.Instance.WorldMap, position, npc)
        { }
    }
}
