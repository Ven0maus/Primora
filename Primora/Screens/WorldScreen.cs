using Primora.Core.Npcs.Actors;
using SadConsole;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Collections.Generic;

namespace Primora.Screens
{
    internal class WorldScreen : ScreenSurface
    {
        private readonly Dictionary<Keys, Direction> _moveDirections = new()
        {
            { Keys.Z, Direction.Up },
            { Keys.S, Direction.Down },
            { Keys.Q, Direction.Left },
            { Keys.D, Direction.Right },
        };

        public WorldScreen(int width, int height) : 
            base(width, height)
        {
            UseKeyboard = true;
            UseMouse = true;
            IsFocused = true;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (Player.Instance.Location.IsDisplayed)
            {
                // Player movement allowed only if it's zone is displayed
                foreach (var key in _moveDirections)
                {
                    if (keyboard.IsKeyPressed(key.Key))
                    {
                        if (Player.Instance.Move(key.Value))
                            return true;
                    }
                }
            }
            return base.ProcessKeyboard(keyboard);
        }
    }
}
