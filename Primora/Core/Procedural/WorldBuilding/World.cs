using Primora.Core.Npcs;
using Primora.Core.Npcs.Actors;
using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using Primora.Extensions;
using Primora.Screens.Main;
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
        /// The current date and time in the world.
        /// </summary>
        internal DateTime Clock { get; private set; }

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

        /// <summary>
        /// The player's entity on the worldmap.
        /// </summary>
        internal WorldMapEntity PlayerWorldMapEntity { get; private set; }

        /// <summary>
        /// Contains all settlements in the world
        /// </summary>
        internal Dictionary<Point, Settlement> Settlements { get; private set; }

        private static readonly TickDictionary<Point, Zone> _zoneCache = [];

        internal World()
        {
            if (Instance != null)
                throw new Exception($"An instance of the {nameof(World)} already exists.");

            Instance = this;

            // Data
            WorldMap = new WorldMap(Constants.Worldmap.DefaultWidth, Constants.Worldmap.DefaultHeight);
            Settlements = [];

            // Setup a random medieval clock
            Clock = GenerateRandomMedievalDate(new Random(Constants.General.GameSeed));
        }

        static World()
        {
            _zoneCache.OnExpire += ZoneCache_OnExpire;
        }

        /// <summary>
        /// Initial entrypoint for world generation.
        /// </summary>
        internal void Generate()
        {
            // Start initial world generation
            WorldMap.Generate(Settlements);

            // Finds a suitable easy early-game zone for the player to spawn in
            SpawnPlayerActorInRandomZone();
        }

        internal void EndTurn()
        {
            _zoneCache.Tick();
        }

        /// <summary>
        /// Shows the world map on the rendering surface.
        /// </summary>
        internal void ShowWorldMap()
        {
            if (CurrentZone != null)
                CurrentZone.IsDisplayed = false;

            var worldScreen = RootScreen.Instance.WorldScreen;
            worldScreen.AdaptScreenForWorldMap();

            // Render world map
            WorldMap.Tilemap.Render(worldScreen);
            WorldMap.IsDisplayed = true;

            // Render also the entities
            ActorManager.RenderLocation(WorldMap);
        }

        /// <summary>
        /// Shows the current zone on the rendering surface.
        /// </summary>
        internal void ShowCurrentZone()
        {
            WorldMap.IsDisplayed = false;

            var worldScreen = RootScreen.Instance.WorldScreen;
            worldScreen.AdaptScreenForZone();

            // Render zone
            CurrentZone.Tilemap.Render(worldScreen);
            CurrentZone.IsDisplayed = true;

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
        internal Zone OpenZone(Point worldPosition, bool setAsCurrentZone = true, bool cacheZone = true)
        {
            if (!_zoneCache.TryGetValue(worldPosition, out var zone))
            {
                // Create new zone and generate it.
                zone = new Zone(worldPosition, Constants.Zone.DefaultWidth, Constants.Zone.DefaultHeight);
                zone.Generate(cacheZone);

                // Add to cache
                if (cacheZone)
                {
                    _zoneCache[worldPosition, Constants.Zone.ZoneCacheTTLInTurns] = zone;

                    // Remove lowest ttl from cache ones atleast 2 zones are in cache
                    // Current and previous zone is the only ones cached
                    if (_zoneCache.Count > 2)
                        _zoneCache.RemoveLowestTTL();
                }
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
            // TODO: when no valid location can be found, fallback to a random grassland biome location.

            if (Settlements.Count == 0)
                throw new Exception("There are no settlements on the map!");

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
            var zone = OpenZone(candidateWorldPositions[random.Next(candidateWorldPositions.Count)], false, false);

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

            // Open and store in cache + set as current zone but don't show yet
            zone = OpenZone(zone.WorldPosition, false, true);
            CurrentZone = zone;

            // Player entity itself
            Player = new Player(zone, candidateZonePositions[random.Next(candidateZonePositions.Count)]);

            // Player's WorldMap Entity
            PlayerWorldMapEntity = new WorldMapEntity(Player.Location.WorldPosition, Entities.Player);
            PlayerWorldMapEntity.PositionChanged += PlayerWorldMapEntity_PositionChanged;
        }

        private void PlayerWorldMapEntity_PositionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            if (!WorldMap.IsDisplayed) return;

            // Center view on player world map entity
            RootScreen.Instance.WorldScreen.ViewPosition = PlayerWorldMapEntity.Position - new Point(
                RootScreen.Instance.WorldScreen.ViewWidth / 2,
                RootScreen.Instance.WorldScreen.ViewHeight / 2);
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

        private static DateTime GenerateRandomMedievalDate(Random rng = null)
        {
            rng ??= new Random();

            // Medieval era
            int year = rng.Next(1000, 1501); // 1000–1500 inclusive
            int month = rng.Next(1, 13);     // 1–12

            // Simplify day calculation to 1–28 to avoid month-length checks
            int day = rng.Next(1, 29);

            return new DateTime(year, month, day, Constants.General.GameStartHour, 0, 0);
        }

        private static void ZoneCache_OnExpire(object sender, TickDictionary<Point, Zone>.ExpireArgs e)
        {
            if (e.Value == Player.Instance.Location)
            {
                // Re-add player zone to the cache, since it is not allowed to be uncached
                _zoneCache[e.Key, 120] = e.Value;
            }
        }
    }
}
