using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens
{
    internal class LogScreen : ScreenSurface
    {
        public LogScreen(int width, int height) : 
            base(width, height)
        {
            Surface.Fill(background: Color.Blue);
        }
    }
}
