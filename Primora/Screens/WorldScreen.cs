using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.WorldBuilding;
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

        /// <summary>
        /// Original view width
        /// </summary>
        public readonly int OriginalWidth;

        /// <summary>
        /// Original view height
        /// </summary>
        public readonly int OriginalHeight;

        public WorldScreen(int width, int height) : 
            base(width, height, Constants.Worldmap.DefaultWidth, Constants.Worldmap.DefaultHeight)
        { 
            OriginalWidth = width;
            OriginalHeight = height;

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
                        {
                            World.Instance.EndTurn();
                            return true;
                        }
                    }
                }
            }
            return base.ProcessKeyboard(keyboard);
        }
    }
}
