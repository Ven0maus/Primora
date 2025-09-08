using Primora.Core.Procedural.Objects;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Primora.Core.Procedural.Common
{
    /// <summary>
    /// The class that contains all methods to modify the world.
    /// </summary>
    internal class Tilemap
    {
        internal readonly int Width, Height;

        private readonly int[] _tiles;

        private static readonly Point[] _cardinalDirections =
        [
            new(0, -1), // North
            new(1, 0),  // East
            new(0, 1),  // South
            new(-1, 0), // West
        ];

        private static readonly Point[] _diagonalDirections =
        [
            new(-1, -1), // NW
            new(1, -1),  // NE
            new(1, 1),   // SE
            new(-1, 1),  // SW
        ];

        internal Tilemap(int width, int height) 
        {
            // Set width and height based on the surface (it could be resized)
            Width = width;
            Height = height;

            // Setup internal tiles array
            _tiles = new int[Width * Height];
        }

        /// <summary>
        /// Returns true if the position is within the bounds of the world.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Returns true if the position is within the bounds of the world.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal bool InBounds(Point point)
            => InBounds(point.X, point.Y);

        /// <summary>
        /// Returns the tile id at the specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal TileVariant GetTile(int x, int y)
        {
            if (!InBounds(x, y))
                throw new Exception($"Point ({x}, {y}) is out of bounds of the world.");

            return TileRegistry.GetVariant(_tiles[Point.ToIndex(x, y, Width)]);
        }

        /// <summary>
        /// Returns the tile id at the specified coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal TileVariant GetTile(Point point)
            => GetTile(point.X, point.Y);

        /// <summary>
        /// Sets the specified tile at the specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tileId"></param>
        /// <exception cref="ArgumentException"></exception>
        internal void SetTile(int x, int y, int tileId)
        {
            if (!InBounds(x, y))
                throw new Exception($"Point ({x}, {y}) is out of bounds of the world.");

            var index = Point.ToIndex(x, y, Width);

            // Return when tile is not modified
            var currentTile = _tiles[index];
            if (currentTile == tileId) return;

            // Verify if tile type exists
            if (!TileRegistry.Exists(tileId))
                throw new ArgumentException($"Provided {nameof(tileId)} \"{tileId}\" does not match with a known tile type.", nameof(tileId));

            // Set new tile
            _tiles[index] = tileId;
        }

        /// <summary>
        /// Sets the specified tile at the specified coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tileId"></param>
        internal void SetTile(Point point, int tileId)
            => SetTile(point.X, point.Y, tileId);

        /// <summary>
        /// Renders the entire tilemap onto a screensurface.
        /// </summary>
        /// <param name="surface"></param>
        internal void Render(ScreenSurface surface)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var appearance = TileRegistry.GetVariant(_tiles[Point.ToIndex(x, y, Width)])?.CellAppearance;
                    if (appearance == null)
                        surface.Clear(x, y);
                    else
                        surface.SetCellAppearance(x, y, appearance);
                }
            }
        }

        /// <summary>
        /// Returns all the neighbors of the specified position.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="includeDiagonals"></param>
        /// <returns></returns>
        internal IEnumerable<(Point Position, TileVariant TileType)> GetNeighbors(Point point, bool includeDiagonals = false)
        {
            foreach (var dir in _cardinalDirections)
            {
                var neighbor = point + dir;
                if (InBounds(neighbor))
                    yield return (neighbor, GetTile(neighbor));
            }

            if (includeDiagonals)
            {
                foreach (var dir in _diagonalDirections)
                {
                    var neighbor = point + dir;
                    if (InBounds(neighbor))
                        yield return (neighbor, GetTile(neighbor));
                }
            }
        }
    }
}
