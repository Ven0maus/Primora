using Primora.Screens;

namespace Primora.Core.Procedural.WorldBuilding
{
    /// <summary>
    /// The class that contains an assortment of objects that can be used to access various elements of the world.
    /// </summary>
    internal class World
    {
        /// <summary>
        /// The map of the overworld containing all the accessible zones.
        /// </summary>
        internal readonly WorldMap WorldMap;

        /// <summary>
        /// The zone that is currently loaded.
        /// </summary>
        internal Zone CurrentZone { get; private set; }

        internal World(int width, int height)
        {
            WorldMap = new WorldMap(width, height);
        }

        /// <summary>
        /// Initial entrypoint for world generation.
        /// </summary>
        internal void Generate()
        {
            WorldMap.Generate();
            ShowWorldMap();
        }

        /// <summary>
        /// Will fast travel to the designated zone.
        /// <br>If the zone is known and explored in the past 4 in-game hours it will load a cached version.</br>
        /// <br>If the zone was not known, or not visited in the past 4 in-game hour it will generate a new procedural zone.</br>
        /// </summary>
        /// <param name="zone"></param>
        internal void TravelToZone(Zone zone)
        {
            if (CurrentZone == zone) return;

            zone.GetFromCacheOrGenerate();
            CurrentZone = zone;

            // Render the zone
            ShowCurrentZone();
        }

        /// <summary>
        /// Shows the world map on the rendering surface.
        /// </summary>
        internal void ShowWorldMap()
        {
            WorldMap.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }

        /// <summary>
        /// Shows the current zone on the rendering surface.
        /// </summary>
        internal void ShowCurrentZone()
        {
            CurrentZone.Tilemap.Render(RootScreen.Instance.RenderingSurface);
        }
    }
}
