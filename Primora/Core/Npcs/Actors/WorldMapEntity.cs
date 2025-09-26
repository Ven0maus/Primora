using Primora.Core.Procedural.WorldBuilding;
using SadRogue.Primitives;
using System;

namespace Primora.Core.Npcs.Actors
{
    internal class WorldMapEntity : Actor
    {
        [Obsolete("World map entities do not have stats.", true)]
        public new ActorStats Stats => null;

        public WorldMapEntity(Point position, string npc) : 
            base(World.Instance.WorldMap, position, npc)
        {
        }
    }
}
