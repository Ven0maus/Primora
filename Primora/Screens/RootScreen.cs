using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using SadConsole;
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
        }

        internal void GenerateWorld()
        {
            // TODO: Show a fancy loading bar?
            World.Generate();
        }
    }
}
