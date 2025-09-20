using Primora.Screens;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
using System;
using System.Diagnostics;

namespace Primora
{
    /// <summary>
    /// Entrypoint class
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Entrypoint method
        /// </summary>
        private static void Main()
        {
            Settings.WindowTitle = Constants.General.GameTitle;
            Settings.ResizeMode = Settings.WindowResizeOptions.Scale;

            // Configure how SadConsole starts up
            int width = 0, height = 0;
            Builder startup = new Builder()
                //.SetWindowSizeInCells(60, 40)
                .SetWindowSizeInCells(() => (width, height) = ScreenHelper.CreateRootWithMin(60, 40, 16, 16))
                .SetStartingScreen((gh) => new RootScreen(width, height))
                .IsStartingScreenFocused(false)
                .EnableImGuiDebugger(Keys.F12)
                .OnStart(OnStart)
                .ConfigureFonts((fc, gh) =>
                {
                    fc.UseCustomFont("Assets/font_16x16.font");
                });

            // Setup the engine and start the game
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void OnStart(object sender, GameHost e)
        {
#if DEBUG
            Debug.WriteLine("Game Seed: " + Constants.General.GameSeed);

            // Add a glyph selector popup for development purposes
            SadConsole.UI.Windows.GlyphSelectPopup.AddRootComponent(SadConsole.Input.Keys.F11);
#endif
        }
    }
}
