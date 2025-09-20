using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens
{
    internal class StatsScreen : ScreenSurface
    {
        public StatsScreen(int width, int height) : 
            base(width, height)
        {
            Surface.Fill(background: Color.Yellow);
        }
    }
}
