using Primora.Core.Procedural.Objects;
using Primora.Extensions;
using Primora.Screens;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Primora.Core.Procedural.WorldBuilding
{
    /// <summary>
    /// The class that contains an assortment of objects that can be used to access various elements of the world.
    /// </summary>
    internal class World
    {
        internal static World Instance { get; private set; }

        /// <summary>
        /// The map of the overworld containing all the accessible zones.
        /// </summary>
        internal readonly WorldMap WorldMap;

        /// <summary>
        /// The zone that is currently loaded.
        /// </summary>
        internal Zone CurrentZone { get; private set; }
        internal static int DefaultZoneWidth { get; private set; }
        internal static int DefaultZoneHeight { get; private set; }

        internal World(int width, int height)
        {
            if (Instance != null)
                throw new Exception($"An instance of the {nameof(World)} already exists.");

            Instance = this;

            // World width/height is initialized at fontsize One.
            WorldMap = new WorldMap(width, height);

            // Just used as a helper to collect the zone width/height
            var surface = new ScreenSurface(width, height);
            surface.ResizeToFitFontSize(IFont.Sizes.Two);

            DefaultZoneWidth = surface.Width;
            DefaultZoneHeight = surface.Height;
        }

        /// <summary>
        /// Initial entrypoint for world generation.
        /// </summary>
        internal void Generate()
        {
            // Start initial world generation
            WorldMap.Generate();

            // Show the world map as default view
            ShowWorldMap();
        }

        internal void LoadZone(Point position)
        {
            CurrentZone = Zone.LoadZone(position);
            ShowCurrentZone();
        }

        /// <summary>
        /// Shows the world map on the rendering surface.
        /// </summary>
        internal void ShowWorldMap()
        {
            // Unloads the zone but remains in memory for X turns
            CurrentZone = null;

            // Worldmap has a regular fontsize
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(1f, true);
            WorldMap.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }

        /// <summary>
        /// Shows the current zone on the rendering surface.
        /// </summary>
        internal void ShowCurrentZone()
        {
            // Zone has a larger fontsize
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(2f, true);
            CurrentZone.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }
    }
}
