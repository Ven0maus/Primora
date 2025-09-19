using Primora.Screens;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;

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

            // Configure how SadConsole starts up
            Builder startup = new Builder()
                .SetWindowSizeInPixels(
                    Constants.General.DefaultWindowSize.width,
                    Constants.General.DefaultWindowSize.height)
                .SetStartingScreen<RootScreen>()
                .IsStartingScreenFocused(false)
                .EnableImGuiDebugger(Keys.F12)
                .ConfigureFonts((fc, gh) =>
                {
                    fc.UseCustomFont("Assets/font_16x16.font");
                });

            // Setup the engine and start the game
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}
