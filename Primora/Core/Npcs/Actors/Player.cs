using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.WorldBuilding;
using Primora.Screens;
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

        /// <summary>
        /// Returns the position of the player on the world map.
        /// </summary>
        public Point WorldPosition => Location.WorldPosition;

        /// <summary>
        /// True if the player is in aiming mode.
        /// </summary>
        public bool IsAiming { get; set; } = false;

        public Player(Zone zone, Point position) : 
            base(zone, position, Entities.Player)
        {
            if (Instance != null)
                throw new Exception("An instance of the player already exists.");
            Instance = this;

            PositionChanged += Player_PositionChanged;
        }

        private void Player_PositionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            var pos = World.Instance.WorldMap.IsDisplayed ? WorldPosition : Position;

            // Center view on player
            RootScreen.Instance.WorldScreen.ViewPosition = pos - new Point(
                RootScreen.Instance.WorldScreen.ViewWidth / 2,
                RootScreen.Instance.WorldScreen.ViewHeight / 2);
        }
    }
}
