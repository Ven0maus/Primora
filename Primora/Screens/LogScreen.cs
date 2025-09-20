using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens
{
    internal class LogScreen : ScreenSurface
    {
        private const string Title = "Event Log";

        public LogScreen(int width, int height) : 
            base(width, height)
        {
            Surface.DrawBorder(SurfaceExtensions.LineThickness.Thin, Title, Color.Gray, Color.White);
        }
    }
}
