using SadConsole;
using SadRogue.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Extensions
{
    internal static class SurfaceExtensions
    {
        internal static void DrawBorder(this ICellSurface surface, LineThickness lineStyle, string title, Color borderColor, Color titleColor, Color? background = null)
        {
            var style = lineStyle == LineThickness.Thin ? 
                ICellSurface.ConnectedLineThin : ICellSurface.ConnectedLineThick;

            // Draw borders
            var shapeParams = ShapeParameters.CreateStyledBox(style, new ColoredGlyph(borderColor, background ?? Color.Transparent), ignoreBorderBackground: background == null);
            surface.DrawBox(new Rectangle(0, 0, surface.Width, surface.Height), shapeParams);

            // Print title
            surface.Print(surface.Width / 2 - title.Length / 2, 0, new ColoredString(title, titleColor, Color.Transparent));
        }

        /// <summary>
        /// Defines the correct box-line style glyphs for the entire path.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static List<(Point coordinate, int glyph)> DefineLineGlyphsByPositions(this IEnumerable<Point> positions)
        {
            var glyphs = new List<(Point coordinate, int glyph)>();
            var hashset = positions as HashSet<Point> ?? [.. positions];
            foreach (var point in hashset)
            {
                // Check each neighbor to define the correct glyph for this point
                bool left = hashset.Contains(new Point(point.X - 1, point.Y));
                bool right = hashset.Contains(new Point(point.X + 1, point.Y));
                bool up = hashset.Contains(new Point(point.X, point.Y - 1));
                bool down = hashset.Contains(new Point(point.X, point.Y + 1));

                int glyph;

                // Decide glyph based on neighbors
                if (left && right && up && down) glyph = 197;        // ┼
                else if (left && right && up) glyph = 193;           // ┴
                else if (left && right && down) glyph = 194;         // ┬
                else if (up && down && left) glyph = 180;            // ┤
                else if (up && down && right) glyph = 195;           // ├
                else if (left && right) glyph = 196;                 // ─
                else if (up && down) glyph = 179;                    // │
                else if (down && right) glyph = 218;                 // ┌
                else if (down && left) glyph = 191;                  // ┐
                else if (up && right) glyph = 192;                   // └
                else if (up && left) glyph = 217;                    // ┘
                else if (left) glyph = 196;
                else if (right) glyph = 196;
                else if (up) glyph = 179;
                else if (down) glyph = 179;
                else glyph = 250; // middle dot for isolated tile

                glyphs.Add((point, glyph));
            }
            return glyphs;
        }
    }

    public enum LineThickness
    {
        Thin,
        Thick
    }
}
