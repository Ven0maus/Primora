using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens
{
    internal class EquipmentScreen : ScreenSurface
    {
        private const string Title = "Equipment";

        public EquipmentScreen(int width, int height) : 
            base(width, height)
        {
            Surface.DrawBorder(SurfaceExtensions.LineThickness.Thin, Title, Color.Gray, Color.White);
        }
    }
}
