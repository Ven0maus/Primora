using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding.Helpers
{
    /// <summary>
    /// Helper to generate a worldmap "road" tile, as a zone.
    /// </summary>
    internal static class RoadZoneHelper
    {
        public static void GenerateRoad(Zone zone)
        {
            var w = zone.Width;
            var h = zone.Height;

            var worldTile = World.Instance.WorldMap.Tilemap.GetTile(zone.WorldPosition);
            if (_roadTemplates.TryGetValue(worldTile.Glyph, out var roadTemplate))
            {
                int templateHeight = roadTemplate.GetLength(0);
                int templateWidth = roadTemplate.GetLength(1);

                // Scale (integer division might drop remainder, so use double)
                double scaleY = (double)h / templateHeight;
                double scaleX = (double)w / templateWidth;

                // Actual rendered size (might be slightly smaller than zone due to rounding)
                int renderHeight = (int)(templateHeight * scaleY);
                int renderWidth = (int)(templateWidth * scaleX);

                // Center offsets
                int offsetY = (h - renderHeight) / 2;
                int offsetX = (w - renderWidth) / 2;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        // Only render inside the centered area
                        if (y >= offsetY && y < offsetY + renderHeight &&
                            x >= offsetX && x < offsetX + renderWidth)
                        {
                            // Map to template coords
                            int ty = (int)((y - offsetY) / scaleY);
                            int tx = (int)((x - offsetX) / scaleX);

                            ty = Math.Min(ty, templateHeight - 1);
                            tx = Math.Min(tx, templateWidth - 1);

                            if (roadTemplate[ty, tx] == '=')
                            {
                                zone.Tilemap.SetTile(x, y, RoadTile(zone));
                            }
                        }
                    }
                }
            }
        }

        private static ColoredGlyph RoadTile(Zone zone)
        {
            return new ColoredGlyph
            {
                Glyph = '"',
                Foreground = "#0d0b09".HexToColor(),
                Background = WorldMap.GetVariationGlyphColor("#241a10".HexToColor(), zone.Random),
            };
        }

        // Cross ┼ (all four)
        private static readonly char[,] CrossTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'=', '=', '=', '=', '=', '=', '='},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // Horizontal ─
        private static readonly char[,] VerticalTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // Vertical │
        private static readonly char[,] HorizontalTemplate =
        {
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'=', '=', '=', '=', '=', '=', '='},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
        };

        // Corner ┌ (down + right)
        private static readonly char[,] CornerDownLeftTemplate =
        {
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'=', '=', '=', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // Corner ┐ (down + left)
        private static readonly char[,] CornerDownRightTemplate =
        {
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '=', '=', '=', '='},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // Corner └ (up + right)
        private static readonly char[,] CornerUpLeftTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'=', '=', '=', '=', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
        };

        // Corner ┘ (up + left)
        private static readonly char[,] CornerUpRightTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '=', '=', '='},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
        };

        // T ┴ (up + left + right)
        private static readonly char[,] TJunctionDownTemplate =
        {
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'=', '=', '=', '=', '=', '=', '='},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // T ┬ (down + left + right)
        private static readonly char[,] TJunctionUpTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'=', '=', '=', '=', '=', '=', '='},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
            {'.', '.', '.', '.', '.', '.', '.'},
        };

        // T ┤ (up + down + left)
        private static readonly char[,] TJunctionLeftTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'=', '=', '=', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // T ├ (up + down + right)
        private static readonly char[,] TJunctionRightTemplate =
        {
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '=', '=', '='},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
            {'.', '.', '.', '=', '.', '.', '.'},
        };

        // This declaration must be on the bottom, because the statics load in the same order as defined.
        /// <summary>
        /// All possible road templates as minimal version
        /// </summary>
        private static readonly Dictionary<int, char[,]> _roadTemplates = new()
        {
            { 197, CrossTemplate },             // ┼
            { 193, TJunctionUpTemplate },       // ┴
            { 194, TJunctionDownTemplate },     // ┬
            { 180, TJunctionLeftTemplate },     // ┤
            { 195, TJunctionRightTemplate },    // ├
            { 196, HorizontalTemplate },        // ─
            { 179, VerticalTemplate },          // │
            { 218, CornerDownRightTemplate },   // ┌
            { 191, CornerDownLeftTemplate },    // ┐
            { 192, CornerUpRightTemplate },     // └
            { 217, CornerUpLeftTemplate },      // ┘
        };
    }
}
