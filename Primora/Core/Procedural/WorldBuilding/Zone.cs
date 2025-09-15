using Primora.Core.Procedural.Common;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal class Zone
    {
        private readonly int _width, _height;
        private readonly Point _zonePosition;
        private readonly Random _random; 

        internal readonly Tilemap Tilemap;

        private static readonly Dictionary<Point, Zone> _zoneCache = [];

        internal Zone(Point zonePosition, int width, int height)
        {
            _zonePosition = zonePosition;
            _width = width;
            _height = height;
            _random = new Random(HashCode.Combine(Constants.General.GameSeed, _zonePosition.X, _zonePosition.Y));
            Tilemap = new Tilemap(width, height);
        }

        internal void Generate()
        {
            var tileInfo = World.Instance.WorldMap.GetTileInfo(_zonePosition);

            // Setup initial tilemap tiles
            InitTilemap();

            // Initial zone layout generation
            ZoneGenerator.Generate(Tilemap, tileInfo, _width, _height, _random);

            // TODO: Additionally generate areas of interest (chests, barrels, lost items, quests)

            // TODO: Generate NPCS
        }

        private void InitTilemap()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
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

        internal static Zone LoadZone(Point point)
        {
            if (_zoneCache.TryGetValue(point, out var zone))
                return zone;
            else
                return GenerateZone(point);
        }

        private static Zone GenerateZone(Point point)
        {
            var zone = new Zone(point, World.DefaultZoneWidth, World.DefaultZoneHeight);
            zone.Generate();

            // Add zone to the cache
            _zoneCache[point] = zone;
            return zone;
        }
    }
}
