using SadConsole;
using SadRogue.Primitives;

namespace Primora.Extensions
{
    internal static class SurfaceExtensions
    {
        internal static void DrawBorder(this ICellSurface surface, LineThickness lineStyle, string title, Color borderColor, Color titleColor)
        {
            var style = lineStyle == LineThickness.Thin ? 
                ICellSurface.ConnectedLineThin : ICellSurface.ConnectedLineThick;

            // Draw borders
            var shapeParams = ShapeParameters.CreateStyledBox(style, new ColoredGlyph(borderColor), ignoreBorderBackground: true);
            surface.DrawBox(new Rectangle(0, 0, surface.Width, surface.Height), shapeParams);

            // Print title
            surface.Print(surface.Width / 2 - title.Length / 2, 0, new ColoredString(title, titleColor, Color.Transparent));
        }
    }

    public enum LineThickness
    {
        Thin,
        Thick
    }
}
