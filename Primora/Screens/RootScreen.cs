using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Diagnostics;

namespace Primora.Screens
{
    internal class RootScreen : ScreenObject
    {
        /// <summary>
        /// The instance of this screen object.
        /// </summary>
        internal static RootScreen Instance { get; private set; }

        /// <summary>
        /// The main rendering surface.
        /// </summary>
        internal readonly ScreenSurface RenderingSurface;

        /// <summary>
        /// The entire world
        /// </summary>
        internal readonly World World;

        private Point? _currentHoverTile;

        public RootScreen()
        {
            if (Instance != null) 
                throw new Exception($"An instance of the {nameof(RootScreen)} already exists.");

            Instance = this;

            // Create the main rendering surface and add it to the RootScreen tree
            RenderingSurface = new ScreenSurface(
                Constants.General.DefaultWindowSize.width,
                Constants.General.DefaultWindowSize.height);
            RenderingSurface.ResizeToFitFontSize(1f, true);
            Children.Add(RenderingSurface);

            // Setup the world elements
            World = new World(RenderingSurface.Width, RenderingSurface.Height);
            Debug.WriteLine("Seed: " + Constants.General.GameSeed);
#if DEBUG
            // Add a glyph selector popup for development purposes
            SadConsole.UI.Windows.GlyphSelectPopup.AddRootComponent(SadConsole.Input.Keys.F11);
#endif
            RenderingSurface.MouseMove += RenderingSurface_MouseMove;
            RenderingSurface.MouseExit += RenderingSurface_MouseExit;
            RenderingSurface.MouseButtonClicked += RenderingSurface_MouseButtonClicked;

            // Testing:
            StartGame();
        }

        private void RenderingSurface_MouseExit(object sender, SadConsole.Input.MouseScreenObjectState e)
        {
            // Clear previous
            var prev = _currentHoverTile;
            if (prev != null)
            {
                RenderingSurface.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
                _currentHoverTile = null;
            }
        }

        private void RenderingSurface_MouseMove(object sender, SadConsole.Input.MouseScreenObjectState e)
        {
            var pos = e.SurfaceCellPosition;

            // Clear previous
            var prev = _currentHoverTile;
            if (prev != null)
            {
                RenderingSurface.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
                _currentHoverTile = null;
            }

            if (_currentHoverTile != null && pos == _currentHoverTile) return;

            if (pos.X >= 0 && pos.Y >= 0 && pos.X < RenderingSurface.Width && pos.Y < RenderingSurface.Height)
            {
                // Set current
                _currentHoverTile = pos;
                RenderingSurface.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Black, 255, Mirror.None));
            }
        }

        private void RenderingSurface_MouseButtonClicked(object sender, SadConsole.Input.MouseScreenObjectState e)
        {
            if (World.CurrentZone != null)
            {
                if (e.Mouse.RightClicked)
                {
                    // Go back to world map
                    World.ShowWorldMap();
                }
                return;
            }
            if (!e.Mouse.LeftClicked) return;

            var pos = e.SurfaceCellPosition;

            var worldTileInfo = World.WorldMap.GetTileInfo(pos);
            Debug.WriteLine($"Loading zone{pos} biome({worldTileInfo.Biome})");
            World.LoadZone(pos);
        }

        /// <summary>
        /// Entrypoint into gameplay, enters into the character creation screen.
        /// </summary>
        internal void StartGame()
        {
            // TODO: Open character creation screen
            GenerateWorld();
        }

        internal void GenerateWorld()
        {
            // TODO: Show a fancy loading bar?
            World.Generate();
        }
    }
}
