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
            // Draw borders
            var shapeParams = ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Gray), ignoreBorderBackground: true);
            Surface.DrawBox(new Rectangle(0, 0, width, height), shapeParams);

            // Print title
            Surface.Print(width / 2 - Title.Length / 2, 0, new ColoredString(Title, Color.White, Color.Transparent));
        }
    }
}
