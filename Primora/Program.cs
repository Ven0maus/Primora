using Primora.Screens;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
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
            Builder startup = new Builder()
                .SetWindowSizeInCells(80, 50)
                .SetStartingScreen<RootScreen>()
                .IsStartingScreenFocused(false)
                .EnableImGuiDebugger(Keys.F12)
                .OnStart(OnStart)
                .ConfigureFonts((fc, gh) =>
                {
                    fc.UseCustomFont("Assets/font_16x16.font");
                    fc.AddExtraFonts("Assets/Cheepicus_12x12.font");
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
