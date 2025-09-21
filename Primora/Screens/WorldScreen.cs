using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
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
        /// Screen size for zone display
        /// </summary>
        public readonly (int width, int height) ZoneScreenSize;
        /// <summary>
        /// Screen size for worldmap display
        /// </summary>
        public readonly (int width, int height) WorldMapScreenSize;

        private readonly ScreenSurface _borderSurface;

        public WorldScreen(ScreenSurface borderSurface,
            (int width, int height) zoneSize, 
            (int width, int height) worldMapSize) : 
            base(zoneSize.width, zoneSize.height, Constants.Worldmap.DefaultWidth, Constants.Worldmap.DefaultHeight)
        {
            _borderSurface = borderSurface;

            // Store screen sizes
            ZoneScreenSize = zoneSize;
            WorldMapScreenSize = worldMapSize;

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
                            LogScreen.Add(LogEntry.New($"Player moved {key.Value.Type}"));
                            World.Instance.EndTurn();
                            return true;
                        }
                    }
                }
            }
            return base.ProcessKeyboard(keyboard);
        }

        public void AdaptScreenForWorldMap()
        {
            // Screen is already adapted for worldmap
            if (_borderSurface.Width == WorldMapScreenSize.width + 2 && _borderSurface.Height == WorldMapScreenSize.height + 2)
                return;

            // Resize also the border surface
            _borderSurface.Resize(
                WorldMapScreenSize.width + 2,
                WorldMapScreenSize.height + 2,
                true);
            _borderSurface.Surface.DrawBorder(LineThickness.Thin, "Worldmap", Color.Gray, Color.White, Color.Black);

            // Resize screensize for worldmap display
            Resize(
                WorldMapScreenSize.width,
                WorldMapScreenSize.height,
                Constants.Worldmap.DefaultWidth,
                Constants.Worldmap.DefaultHeight,
                false);

            // Reset fontsize to default
            FontSize = new Point(
                Constants.General.FontGlyphWidth,
                Constants.General.FontGlyphHeight);

            // Center view on player
            ViewPosition = Player.Instance.WorldPosition - new Point(
                ViewWidth / 2,
                ViewHeight / 2);
        }

        public void AdaptScreenForZone()
        {
            // Screen is already adapted for zones
            if (_borderSurface.Width == ZoneScreenSize.width + 2 && _borderSurface.Height == ZoneScreenSize.height + 2)
                return;

            // Resize also the border surface
            _borderSurface.Resize(
                ZoneScreenSize.width + 2,
                ZoneScreenSize.height + 2,
                true);
            _borderSurface.Surface.DrawBorder(LineThickness.Thin, "World", Color.Gray, Color.White, Color.Black);

            // Resize back to the screensize for zone display
            Resize(
                ZoneScreenSize.width / 2,
                ZoneScreenSize.height / 2,
                Constants.Zone.DefaultWidth,
                Constants.Zone.DefaultHeight,
                false);

            // Increase the fontsize by two
            FontSize *= 2;

            // Center view on player
            ViewPosition = Player.Instance.Position - new Point(
                ViewWidth / 2,
                ViewHeight / 2);
        }
    }
}
