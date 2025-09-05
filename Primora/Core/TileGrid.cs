using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core
{
    /// <summary>
    /// The class that contains all methods to modify the world.
    /// </summary>
    internal class TileGrid
    {
        public readonly int Width, Height;

        private readonly int[] _tiles;
        private readonly Dictionary<int, TileType> _tileTypes;

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

        /// <summary>
        /// The rendering surface of the tile grid.
        /// </summary>
        public readonly ScreenSurface Surface;

        public TileGrid(int width, int height, IFont.Sizes fontSize) 
        {
            // Load and cache reference to all tile types
            _tileTypes = LoadTileTypeData();

            // Setup the rendering surface
            Surface = new ScreenSurface(width, height);
            if (fontSize != IFont.Sizes.One)
                Surface.ResizeToFitFontSize(fontSize);

            // Set width and height based on the surface (it could be resized)
            Width = Surface.Width;
            Height = Surface.Height;

            // Setup internal tiles array
            _tiles = new int[Width * Height];

            // Initial appearance rendering
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Surface.SetCellAppearance(x, y, GetTile(x, y).CellAppearance);
        }

        /// <summary>
        /// Returns true if the position is within the bounds of the world.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        /// <summary>
        /// Returns true if the position is within the bounds of the world.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool InBounds(Point point)
            => InBounds(point.X, point.Y);

        /// <summary>
        /// Returns the tile id at the specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TileType GetTile(int x, int y)
        {
            if (!InBounds(x, y))
                throw new Exception($"Point ({x}, {y}) is out of bounds of the world.");

            return _tileTypes[_tiles[Point.ToIndex(x, y, Width)]];
        }

        /// <summary>
        /// Returns the tile id at the specified coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TileType GetTile(Point point)
            => GetTile(point.X, point.Y);

        /// <summary>
        /// Sets the specified tile at the specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tileId"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetTile(int x, int y, int tileId, bool asDecorator = false)
        {
            if (!InBounds(x, y))
                throw new Exception($"Point ({x}, {y}) is out of bounds of the world.");

            var index = Point.ToIndex(x, y, Width);

            // Return when tile is not modified
            var currentTile = _tiles[index];
            if (currentTile == tileId) return;

            // Verify if tile type exists
            if (!_tileTypes.TryGetValue(tileId, out var tileType))
                throw new ArgumentException($"Provided {nameof(tileId)} \"{tileId}\" does not match with a known tile type.", nameof(tileId));

            // Set new tile
            _tiles[index] = tileId;

            // Set appearance on the rendering surface
            var appearance = tileType.CellAppearance;
            if (asDecorator)
                Surface.SetDecorator(index, new CellDecorator(appearance.Foreground, appearance.Glyph, Mirror.None));
            else
            {
                Surface.SetDecorator(index, null);
                Surface.SetCellAppearance(x, y, appearance);
            }
        }

        /// <summary>
        /// Sets the specified tile at the specified coordinates.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tileId"></param>
        public void SetTile(Point point, int tileId, bool asDecorator = false)
            => SetTile(point.X, point.Y, tileId, asDecorator);

        /// <summary>
        /// Returns all the neighbors of the specified position.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="includeDiagonals"></param>
        /// <returns></returns>
        public IEnumerable<(Point Position, TileType TileType)> GetNeighbors(Point point, bool includeDiagonals = false)
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

        private static Dictionary<int, TileType> LoadTileTypeData()
        {
            var tileTypesPath = Constants.GameData.TileTypesDataPath;
            if (!File.Exists(tileTypesPath))
                throw new Exception($"Missing game data file \"{Path.GetFileName(tileTypesPath)}\" at path \"{tileTypesPath}\".");
            try
            {
                var tiles = JsonSerializer.Deserialize<List<TileType>>(File.ReadAllText(tileTypesPath), Constants.General.SerializerOptions);
                return tiles.ToDictionary(a => a.Id, a => a);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to deserialize game data file \"{Path.GetFileName(tileTypesPath)}\", game data is corrupted:\n{e.Message}");
            }
        }
    }
}
