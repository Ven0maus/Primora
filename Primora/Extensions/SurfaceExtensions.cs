using SadConsole;
using SadRogue.Primitives;
using System;

namespace Primora.Extensions
{
    /// <summary>
    /// Contains extensions related to surfaces.
    /// </summary>
    internal static class SurfaceExtensions
    {
        /// <summary>
        /// Translate the given width and height to a new fontsize.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        internal static (int width, int height) TranslateSize(this (int width, int height) size, Point fontSize)
        {
            return ((int)Math.Ceiling((double)size.width / fontSize.X), (int)Math.Ceiling((double)size.height / fontSize.Y));
        }

        /// <summary>
        /// Returns the translated width and height of the surface based on its fontsize.
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        internal static Point TranslatedSize(this IScreenSurface surface)
        {
            return new Point(
                (int)Math.Ceiling((double)Constants.General.DefaultWindowSize.width / surface.FontSize.X), 
                (int)Math.Ceiling((double)Constants.General.DefaultWindowSize.height / surface.FontSize.Y));
        }

        /// <summary>
        /// Resizes the surfaces to maintain its original width/height translated to the specified size.
        /// <br>If no size specified, uses the size of the font of the surface.</br>
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="clear">Clear the entire surface</param>
        /// <returns></returns>
        internal static void ResizeToFitFontSize(this ScreenSurface surface, IFont.Sizes? size = null, bool clear = false)
        {
            // Adjust fontsize
            if (size != null)
                surface.FontSize = surface.Font.GetFontSize(size.Value);

            var translatedSize = surface.TranslatedSize();
            surface.Resize(translatedSize.X, translatedSize.Y, clear);
        }
    }
}
