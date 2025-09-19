using Primora.Core.Npcs;
using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using Primora.Extensions;
using Primora.Screens;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary>
        /// The player character object.
        /// </summary>
        internal Player Player { get; private set; }

        internal Dictionary<Point, Settlement> Settlements { get; private set; }

        private readonly int _defaultZoneWidth, _defaultZoneHeight;
        private static readonly TickDictionary<Point, Zone> _zoneCache = [];

        internal World(int width, int height)
        {
            if (Instance != null)
                throw new Exception($"An instance of the {nameof(World)} already exists.");

            Instance = this;

            // TODO: Rework worldmap and zone width/height defining
            // World width/height is initialized at fontsize One.
            WorldMap = new WorldMap(width, height);

            // Just used as a helper to collect the zone width/height
            var surface = new ScreenSurface(width, height);
            surface.ResizeToFitFontSize(Constants.Zone.ZoneSizeModifier);

            _defaultZoneWidth = surface.Width;
            _defaultZoneHeight = surface.Height;

            // Settlement data
            Settlements = [];
        }

        /// <summary>
        /// Initial entrypoint for world generation.
        /// </summary>
        internal void Generate()
        {
            // Start initial world generation
            WorldMap.Generate();

            // Finds a suitable easy early-game zone for the player to spawn in
            SpawnPlayerActorInRandomZone();

            // TODO: Make sure player is visible on world map
            // TODO: Make sure player zone is never removed from the cache.
        }

        /// <summary>
        /// Shows the world map on the rendering surface.
        /// </summary>
        internal void ShowWorldMap()
        {
            // Worldmap has a regular fontsize (1 size)
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(1f, true);

            // Render world map
            WorldMap.Tilemap.Render(RootScreen.Instance.RenderingSurface);

            // Render also the entities
            ActorManager.RenderLocation(CurrentZone);
        }

        /// <summary>
        /// Shows the current zone on the rendering surface.
        /// </summary>
        internal void ShowCurrentZone()
        {
            // Zone has a larger fontsize
            RootScreen.Instance.RenderingSurface.ResizeToFitFontSize(Constants.Zone.ZoneSizeModifier, true);

            // Render zone
            CurrentZone.Tilemap.Render(RootScreen.Instance.RenderingSurface);

            // Render also the entities
            ActorManager.RenderLocation(CurrentZone);
        }

        /// <summary>
        /// Opens a zone, and caches it for 120 turns (starts ticking when zone is unloaded.)
        /// <br>When TTL is reached, zone is removed from the cache. Next time the same zone is opened it generates a new one from a new seed.</br>
        /// <br>The generated zones remain deterministic per gameseed (so generating the same zone 5 times will have different generate "different" zones, 
        /// but when starting a new game with the same seed will guarantee the same layouts to be generated in the same order.)</br>
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="setAsCurrentZone"></param>
        internal Zone OpenZone(Point worldPosition, bool setAsCurrentZone = true)
        {
            if (!_zoneCache.TryGetValue(worldPosition, out var zone))
            {
                // Create new zone and generate it.
                zone = new Zone(worldPosition, _defaultZoneWidth, _defaultZoneHeight);
                zone.Generate();

                // Add to cache
                _zoneCache[worldPosition, Constants.Zone.ZoneCacheTTLInTurns] = zone;
            }

            if (setAsCurrentZone)
            {
                CurrentZone = zone;
                ShowCurrentZone();
            }

            return zone;
        }

        private void SpawnPlayerActorInRandomZone()
        {
            var random = new Random(Constants.General.GameSeed);

            // Find a grassland biome between min and max tiles from any settlement.
            var settlementPositions = Settlements.Values
                .Select(a => a.Position)
                .ToHashSet();

            var candidateWorldPositions = new List<Point>();
            var (min, max) = Constants.Npcs.PlayerSpawnDistanceFromSettlements;
            var minSqr = min * min;
            var maxSqr = max * max;

            for (int x = 0; x < WorldMap.Width; x++)
            {
                for (int y = 0; y < WorldMap.Height; y++)
                {
                    var worldTileInfo = WorldMap.GetTileInfo(x, y);
                    if (worldTileInfo.Biome != Objects.Biome.Grassland) continue;

                    if (settlementPositions.Any(s =>
                    {
                        int distSq = s.DistanceSquared(new Point(x, y));
                        return distSq >= minSqr &&
                               distSq <= maxSqr;
                    }))
                        candidateWorldPositions.Add(new Point(x, y));
                }
            }

            if (candidateWorldPositions.Count == 0)
                throw new Exception("Could not find a valid zone within the world map for player generation.");

            // Open the zone
            var zone = OpenZone(candidateWorldPositions[random.Next(candidateWorldPositions.Count)], false);

            // Find a position within the zone that has no obstructions in its immediate area (4 radius)
            var candidateZonePositions = new List<Point>();
            for (int x = 0; x < zone.Width; x++)
            {
                for (int y = 0; y < zone.Height; y++)
                {
                    if (IsClearAround(zone, x, y, Constants.Npcs.PlayerSpawnZoneClearRadius))
                        candidateZonePositions.Add(new Point(x, y));
                }
            }

            if (candidateZonePositions.Count == 0)
                throw new Exception("Could not find a valid position within the player designated spawn zone.");

            Player = new Player(zone, candidateZonePositions[random.Next(candidateZonePositions.Count)]);
        }

        private static bool IsClearAround(Zone zone, int x, int y, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (!zone.InBounds(nx, ny))
                        continue;

                    if (!zone.GetTileInfo(nx, ny).Walkable)
                        return false;
                }
            }
            return true;
        }
    }
}
