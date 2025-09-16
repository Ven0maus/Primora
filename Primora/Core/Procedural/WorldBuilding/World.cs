using Primora.Core.Procedural.Common;
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

        private static readonly TickDictionary<Point, Zone> _zoneCache = [];

        internal World(int width, int height)
        {
            if (Instance != null)
                throw new Exception($"An instance of the {nameof(World)} already exists.");

            Instance = this;

            // World width/height is initialized at fontsize One.
            WorldMap = new WorldMap(width, height);

            // Just used as a helper to collect the zone width/height
            var surface = new ScreenSurface(width, height);
            surface.ResizeToFitFontSize(Constants.Zone.ZoneSizeModifier);

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
        }

        /// <summary>
        /// Shows the world map on the rendering surface.
        /// </summary>
        internal void ShowWorldMap()
        {
            // Unloads the zone but remains in memory for X turns
            CurrentZone = null;

            // Worldmap has a regular fontsize (1 size)
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(1f, true);
            WorldMap.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }

        /// <summary>
        /// Shows the current zone on the rendering surface.
        /// </summary>
        internal void ShowCurrentZone()
        {
            // Zone has a larger fontsize
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(Constants.Zone.ZoneSizeModifier, true);
            CurrentZone.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }

        /// <summary>
        /// Opens a zone, and caches it for 120 turns (starts ticking when zone is unloaded.)
        /// <br>When TTL is reached, zone is removed from the cache. Next time the same zone is opened it generates a new one from a new seed.</br>
        /// <br>The generated zones remain deterministic per gameseed (so generating the same zone 5 times will have different generate "different" zones, 
        /// but when starting a new game with the same seed will guarantee the same layouts to be generated in the same order.)</br>
        /// </summary>
        /// <param name="worldPosition"></param>
        internal void OpenZone(Point worldPosition)
        {
            if (!_zoneCache.TryGetValue(worldPosition, out var zone))
            {
                // Create new zone and generate it.
                zone = new Zone(worldPosition, DefaultZoneWidth, DefaultZoneHeight);
                zone.Generate();

                // Add to cache
                _zoneCache[worldPosition, Constants.Zone.ZoneCacheTTLInTurns] = zone;
            }
            CurrentZone = zone;
            ShowCurrentZone();
        }
    }
}
