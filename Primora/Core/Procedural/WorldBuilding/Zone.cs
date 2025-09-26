using Primora.Core.Procedural.Common;
using Primora.Core.Procedural.Objects;
using Primora.Extensions;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.WorldBuilding
{
    internal class Zone : ILocation, IEquatable<Zone>
    {
        private readonly ZoneTileInfo[] _zoneTileInfo;

        public int Width { get; }
        public int Height { get; }

        internal bool IsDisplayed = false;
        internal readonly Point WorldPosition;
        internal readonly Random Random; 
        internal readonly Tilemap Tilemap;

        /// <summary>
        /// Keeps a counter, so each distinct generation will be unique. But still based on the game seed.
        /// <br>This guarantees multiple games on the same seed to generate in the same order.</br>
        /// </summary>
        private static readonly Dictionary<Point, int> _zoneLoadCount = [];

        internal Zone(Point zonePosition, int width, int height)
        {
            WorldPosition = zonePosition;
            _zoneTileInfo = new ZoneTileInfo[width * height];

            // Generate the random based on times we visited this zone (completely regenerated, not from cache)
            Random = new Random(MathUtils.Fnv1aHash(Constants.General.GameSeed, WorldPosition.X, WorldPosition.Y,
                _zoneLoadCount.TryGetValue(zonePosition, out var count) ? count : 0));

            Width = width;
            Height = height;
            Tilemap = new Tilemap(width, height);
        }

        internal void Generate(bool cached = true)
        {
            if (cached)
            {
                _zoneLoadCount.TryGetValue(WorldPosition, out var count);
                _zoneLoadCount[WorldPosition] = ++count;
            }

            // Setup initial tilemap tiles
            InitTilemap();

            // Initial zone layout generation
            ZoneGenerator.Generate(this);

            // TODO: Additionally generate areas of interest (chests, barrels, lost items, quests)


            // TODO: Generate NPCS
        }

        internal double GetLowestWeight()
        {
            double weight = double.MaxValue;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var weightValue = GetTileInfo(x, y).Weight;
                    if (weightValue < weight)
                        weight = weightValue;
                }
            }
            return weight;
        }

        internal ZoneTileInfo GetTileInfo(int x, int y)
        {
            return _zoneTileInfo[Point.ToIndex(x, y, Width)];
        }

        internal ZoneTileInfo GetTileInfo(Point position)
            => GetTileInfo(position.X, position.Y);

        internal void SetTileInfo(int x, int y, ZoneTileInfo zoneTileInfo)
        {
            if (!InBounds(x, y)) return;
            _zoneTileInfo[Point.ToIndex(x, y, Width)] = zoneTileInfo;
        }

        internal void SetTileInfo(Point position, ZoneTileInfo zoneTileInfo)
            => SetTileInfo(position.X, position.Y, zoneTileInfo);

        internal bool InBounds(int x, int y)
            => x >= 0 && y >= 0 && x < Width && y < Height;

        internal bool InBounds(Point position)
            => InBounds(position.X, position.Y);

        public bool IsWalkable(Point position)
            => InBounds(position) && GetTileInfo(position).Walkable;

        private void InitTilemap()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetTileInfo(x, y, new ZoneTileInfo(new Point(x, y)));
                }
            }
        }

        public bool Equals(Zone other)
        {
            return other != null && (ReferenceEquals(this, other) || other.WorldPosition == WorldPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is Zone zone && Equals(zone);
        }

        public override int GetHashCode()
        {
            return WorldPosition.GetHashCode();
        }

        public static bool operator ==(Zone left, Zone right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Zone left, Zone right) => !(left == right);
    }
}
