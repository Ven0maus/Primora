using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
using System;

namespace Primora.Screens
{
    internal class RootScreen : ScreenObject
    {
        /// <summary>
        /// The instance of this screen object.
        /// </summary>
        internal static RootScreen Instance { get; private set; }

        #region Screens
        /// <summary>
        /// The main surface for rendering the world visuals.
        /// </summary>
        internal readonly WorldScreen WorldScreen;
        /// <summary>
        /// The main surface for rendering the game's log messages.
        /// </summary>
        internal readonly LogScreen LogScreen;
        /// <summary>
        /// The main screen for rendering the player stats.
        /// </summary>
        internal readonly StatsScreen StatsScreen;
        /// <summary>
        /// The main screen for rendering the player equipment.
        /// </summary>
        internal readonly EquipmentScreen EquipmentScreen;
        /// <summary>
        /// The main screen for rendering the entities in the surrounding environment of the player.
        /// </summary>
        internal readonly AbilityScreen AbilitiesScreen;
        #endregion

        /// <summary>
        /// The entire world
        /// </summary>
        internal readonly World World;
        /// <summary>
        /// The EntityManager component used to draw entities to the world screen.
        /// </summary>
        internal readonly EntityManager EntityManager;

        private Point? _currentHoverTile;

        public RootScreen()
        {
            if (Instance != null) 
                throw new Exception($"An instance of the {nameof(RootScreen)} already exists.");

            Instance = this;

            // Screen is 80 x 50
            // Create screen layout
            var borderSurface = new ScreenSurface(60, 40);
            borderSurface.Surface.DrawBorder(LineThickness.Thin, "World", Color.Gray, Color.White);

            WorldScreen = new WorldScreen(borderSurface.Width - 2, borderSurface.Height - 2) { Position = (0, 0) };
            WorldScreen.Position = new Point(1, 1);
            borderSurface.Children.Add(WorldScreen);

            LogScreen = new LogScreen(40, 10) { Position = (0, 40) };
            StatsScreen = new StatsScreen(20, 25) { Position = (60, 0) };
            EquipmentScreen = new EquipmentScreen(20, 25) { Position = (60, 25) };
            AbilitiesScreen = new AbilityScreen(20, 10) { Position = (40, 40) };

            Children.Add(borderSurface);
            Children.Add(LogScreen);
            Children.Add(StatsScreen);
            Children.Add(EquipmentScreen);
            Children.Add(AbilitiesScreen);

            // Entity manager component
            EntityManager = new EntityManager
            {
                SkipExistsChecks = true,
                DoEntityUpdate = false
            };
            WorldScreen.IsFocused = true;
            WorldScreen.SadComponents.Add(EntityManager);

            WorldScreen.MouseMove += RenderingSurface_MouseMove;
            WorldScreen.MouseExit += RenderingSurface_MouseExit;
            WorldScreen.MouseButtonClicked += RenderingSurface_MouseButtonClicked;

            // Setup the world with a much larger size
            World = new World();

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
            var pos = e.SurfaceCellPosition + WorldScreen.ViewPosition;

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

            var coordinate = e.SurfaceCellPosition + WorldScreen.ViewPosition;
            _ = World.OpenZone(coordinate);
        }

        /// <summary>
        /// Entrypoint into gameplay, enters into the character creation screen.
        /// </summary>
        internal void StartGame()
        {
            // TODO: Open character creation screen
            GenerateWorld();

            // Update all the displays
            UpdateDisplays();
        }

        internal void UpdateDisplays()
        {
            StatsScreen.UpdateDisplay();
            EquipmentScreen.UpdateDisplay();
            AbilitiesScreen.UpdateDisplay();
            LogScreen.UpdateDisplay();
        }

        private void GenerateWorld()
        {
            // TODO: Show a fancy loading bar?
            World.Generate();
            World.ShowWorldMap();
        }
    }
}
