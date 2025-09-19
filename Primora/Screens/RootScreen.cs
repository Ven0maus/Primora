using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using SadConsole;
using SadConsole.Entities;
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
        internal readonly WorldScreen WorldScreen;

        /// <summary>
        /// The entire world
        /// </summary>
        internal readonly World World;

        internal readonly EntityManager EntityManager;

        private Point? _currentHoverTile;

        public RootScreen()
        {
            if (Instance != null) 
                throw new Exception($"An instance of the {nameof(RootScreen)} already exists.");

            Instance = this;

            // Create the main rendering surface and add it to the RootScreen tree
            WorldScreen = new WorldScreen(
                Constants.General.DefaultWindowSize.width,
                Constants.General.DefaultWindowSize.height);
            WorldScreen.ResizeToFitFontSize(1f, true);
            Children.Add(WorldScreen);

            // Entity manager component
            EntityManager = new EntityManager
            {
                SkipExistsChecks = true,
                DoEntityUpdate = true
            };
            WorldScreen.IsFocused = true;
            WorldScreen.SadComponents.Add(EntityManager);

            // Setup the world elements
            World = new World(WorldScreen.Width, WorldScreen.Height);
            Debug.WriteLine("Game Seed: " + Constants.General.GameSeed);
#if DEBUG
            // Add a glyph selector popup for development purposes
            SadConsole.UI.Windows.GlyphSelectPopup.AddRootComponent(SadConsole.Input.Keys.F11);
#endif
            WorldScreen.MouseMove += RenderingSurface_MouseMove;
            WorldScreen.MouseExit += RenderingSurface_MouseExit;
            WorldScreen.MouseButtonClicked += RenderingSurface_MouseButtonClicked;

            // Testing:
            StartGame();
        }

        private void RenderingSurface_MouseExit(object sender, SadConsole.Input.MouseScreenObjectState e)
        {
            // Clear previous
            var prev = _currentHoverTile;
            if (prev != null)
            {
                WorldScreen.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
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
                WorldScreen.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
                _currentHoverTile = null;
            }

            if (_currentHoverTile != null && pos == _currentHoverTile) return;

            if (pos.X >= 0 && pos.Y >= 0 && pos.X < WorldScreen.Width && pos.Y < WorldScreen.Height)
            {
                // Set current
                _currentHoverTile = pos;
                WorldScreen.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Black, 255, Mirror.None));
            }
        }

        private void RenderingSurface_MouseButtonClicked(object sender, SadConsole.Input.MouseScreenObjectState e)
        {
            // TODO: Rework into fast travel, properly going into zones, this is just for testing purposes at the moment.
            if (World.CurrentZone != null && World.CurrentZone.IsDisplayed)
            {
                if (e.Mouse.RightClicked)
                {
                    // Go back to world map
                    World.ShowWorldMap();
                }
                return; 
            }
            if (!e.Mouse.LeftClicked) return;

            var coordinate = e.SurfaceCellPosition;
            _ = World.OpenZone(coordinate);
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
            World.ShowWorldMap();
        }
    }
}
