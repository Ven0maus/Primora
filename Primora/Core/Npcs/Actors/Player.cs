using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.WorldBuilding;
using SadRogue.Primitives;
using System;

namespace Primora.Core.Npcs.Actors
{
    internal sealed class Player : Actor
    {
        public static Player Instance { get; private set; }

        /// <summary>
        /// Returns the current player zone.
        /// </summary>
        public new Zone Location => (Zone)base.Location;

        public Player(Zone zone, Point position) : 
            base(zone, position, Entities.Player)
        {
            if (Instance != null)
                throw new Exception("An instance of the player already exists.");
            Instance = this;
        }
    }
}
