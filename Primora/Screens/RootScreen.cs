using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using SadConsole;
using SadConsole.Components;
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

        public RootScreen()
        {
            if (Instance != null) 
                throw new Exception($"An instance of the {nameof(RootScreen)} already exists.");

            Instance = this;

            // Setup the world as it does not directly interact with screens on construction
            World = new World();

            const int MaxScreenWidth = 80;
            const int MaxScreenHeight = 50;

            // Create screen layout for zone
            var borderSurface = new ScreenSurface(60, 40);
            WorldScreen = new WorldScreen(borderSurface, 
                (borderSurface.Width - 2, borderSurface.Height - 2), 
                (MaxScreenWidth - 2, MaxScreenHeight - 2)) 
            { 
                Position = (0, 0) 
            };
            WorldScreen.Position = new Point(1, 1);
            borderSurface.Children.Add(WorldScreen);

            LogScreen = new LogScreen(40, 10) { Position = (0, 40) };
            StatsScreen = new StatsScreen(20, 25) { Position = (60, 0) };
            EquipmentScreen = new EquipmentScreen(20, 25) { Position = (60, 25) };
            AbilitiesScreen = new AbilityScreen(20, 10) { Position = (40, 40) };

            Children.Add(LogScreen);
            Children.Add(StatsScreen);
            Children.Add(EquipmentScreen);
            Children.Add(AbilitiesScreen);

            // World screen needs to be last in the render,
            // This so that the "worldmap" can be rendered on top of all others
            Children.Add(borderSurface);

            // Entity manager component
            EntityManager = new EntityManager
            {
                SkipExistsChecks = true,
                DoEntityUpdate = false
            };
            WorldScreen.IsFocused = true;
            WorldScreen.SadComponents.Add(EntityManager);

            // Testing:
            StartGame();
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
            World.OpenZone(Player.Instance.WorldPosition);
        }
    }
}
