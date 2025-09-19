using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.Objects;
using Primora.Core.Procedural.WorldBuilding;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;

namespace Primora.Core.Npcs.Actors
{
    internal sealed class Player : Actor
    {
        private readonly Dictionary<Keys, Direction> _moveDirections = new()
        {
            { Keys.Z, Direction.Up },
            { Keys.S, Direction.Down },
            { Keys.Q, Direction.Left },
            { Keys.D, Direction.Right },
        };

        /// <summary>
        /// Returns the current player zone.
        /// </summary>
        public new Zone Location => (Zone)base.Location;

        public Player(ILocation location, Point position) : 
            base(location, position, Entities.Player)
        { }

        // Implement keyboard input for movement
        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            foreach (var key in _moveDirections)
            {
                if (keyboard.IsKeyPressed(key.Key))
                {
                    if (Move(key.Value))
                        return true;
                }
            }
            return base.ProcessKeyboard(keyboard);
        }
    }
}
