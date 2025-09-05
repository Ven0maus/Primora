using Primora.Screens;
using SadConsole;
using SadConsole.Configuration;

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
                .IsStartingScreenFocused(true)
                .ConfigureFonts((fc, gh) =>
                {
                    fc.UseCustomFont("Assets/font_12x12.font");
                });

            // Setup the engine and start the game
            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}
