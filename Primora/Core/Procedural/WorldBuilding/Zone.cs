using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal class Zone
    {
        private readonly ZoneTileInfo[] _zoneTileInfo;

        internal readonly int Width, Height;
        internal readonly Point WorldPosition;
        internal readonly Random Random; 
        internal readonly Tilemap Tilemap;

        private static readonly TickDictionary<Point, Zone> _zoneCache = [];
        private static readonly Dictionary<Point, int> _zoneLoadCount = [];

        internal Zone(Point zonePosition, int width, int height)
        {
            WorldPosition = zonePosition;
            _zoneTileInfo = new ZoneTileInfo[width * height];

            // Generate the random based on times we visited this zone (completely regenerated, not from cache)
            Random = new Random(HashCode.Combine(Constants.General.GameSeed, WorldPosition.X, WorldPosition.Y,
                _zoneLoadCount.TryGetValue(zonePosition, out var count) ? count : 0));

            Width = width;
            Height = height;
            Tilemap = new Tilemap(width, height);
        }

        internal void Generate()
        {
            _zoneLoadCount.TryGetValue(WorldPosition, out var count);
            _zoneLoadCount[WorldPosition] = ++count;

            // Setup initial tilemap tiles
            InitTilemap();

            // Initial zone layout generation
            ZoneGenerator.Generate(this);

            // TODO: Additionally generate areas of interest (chests, barrels, lost items, quests)

            // TODO: Generate NPCS
        }

        internal ZoneTileInfo GetTileInfo(int x, int y)
        {
            return _zoneTileInfo[Point.ToIndex(x, y, Width)];
        }

        internal ZoneTileInfo GetTileInfo(Point position)
            => GetTileInfo(position.X, position.Y);

        private void InitTilemap()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tilemap.SetTile(x, y, new SadConsole.ColoredGlyph
                    {
                        Glyph = 0,
                        Foreground = Color.White,
                        Background = Color.Black
                    });
                }
            }
        }

        internal static Zone LoadZone(Point worldPosition)
        {
            if (_zoneCache.TryGetValue(worldPosition, out var zone))
            {
                Debug.WriteLine($"Retrieved zone from cache: {worldPosition}");
                return zone;
            }
            else
                return GenerateZone(worldPosition);
        }

        private static Zone GenerateZone(Point worldPosition)
        {
            // Create a complete new zone and generate it
            var zone = new Zone(worldPosition, World.DefaultZoneWidth, World.DefaultZoneHeight);
            zone.Generate();

            Debug.WriteLine($"Generated new zone: {worldPosition}");

            // Add zone to the cache for 120 turns
            _zoneCache[worldPosition, 120] = zone;
            return zone;
        }
    }
}
