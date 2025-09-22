using Microsoft.Xna.Framework.Graphics;
using SadConsole.Configuration;
using System;

namespace Primora.Screens.Helpers
{
    internal static class ScreenHelper
    {
        /// <summary>
        /// Creates a root console that fits the monitor while enforcing a minimum grid size.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="minWidth">Minimum console width in cells.</param>
        /// <param name="minHeight">Minimum console height in cells.</param>
        /// <param name="maxScreenFraction">Fraction of monitor to use (0.9 = leave margins).</param>
        public static (int width, int height) CreateRootWithMin(
            int minWidth, int minHeight, int glyphWidth, int glyphHeight, float maxScreenFraction = 0.9f)
        {
            var displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

            int screenWidth = displayMode.Width;
            int screenHeight = displayMode.Height;

            // Max cells that fit on this monitor
            int maxCellsX = (int)(screenWidth * maxScreenFraction / glyphWidth);
            int maxCellsY = (int)(screenHeight * maxScreenFraction / glyphHeight);

            // Clamp to minimum
            int rootWidth = Math.Max(minWidth, maxCellsX);
            int rootHeight = Math.Max(minHeight, maxCellsY);

            // Round down to nearest multiple of glyph size factor
            // Here we assume font glyphs are square multiples (8,16, etc.)
            rootWidth = rootWidth / 2 * 2;   // round down to nearest multiple of 2
            rootHeight = rootHeight / 2 * 2;

            return (rootWidth, rootHeight);
        }

        /// <summary>
        /// Helper to set window size in cells, but being able to use a func that is called once host is running.
        /// <br>This gives access to things from the host like GraphicsAdapter.</br>
        /// </summary>
        /// <param name="configBuilder"></param>
        /// <param name="sizeFunc"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static Builder SetWindowSizeInCells(this Builder configBuilder, Func<(int width, int height)> sizeFunc, bool zoom = false)
        {
            configBuilder.ConfigureWindow((screenConfig, configBuilder, host) =>
            {
                var (width, height) = sizeFunc.Invoke();
                screenConfig.SetWindowSizeInCells(width, height, zoom);
            });
            return configBuilder;
        }
    }
}
