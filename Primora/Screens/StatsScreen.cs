using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens
{
    internal class StatsScreen : ScreenSurface
    {
        private const string Title = "Stats";

        public StatsScreen(int width, int height) : 
            base(width, height)
        {
            Surface.DrawBorder(SurfaceExtensions.LineThickness.Thin, Title, Color.Gray, Color.White);
        }
    }
}
