using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core
{
    /// <summary>
    /// Contains all methods to handle generation of the world layers.
    /// </summary>
    internal class WorldGeneration(World world)
    {
        private readonly World _world = world;

        public void GenerateGrounds()
        {
            // TODO: Generate noise-like grass ground

        }

        public void GenerateFauna()
        {
            // Generate forests
            int forestCount = (int)Math.Ceiling(_world.Width * _world.Height / 130d);
            int forestDensity = 50; // tweak for forest density

            for (int i = 0; i < forestCount; i++)
            {
                int startX = Constants.General.Random.Next(0, _world.Width);
                int startY = Constants.General.Random.Next(0, _world.Height);

                int clusterSize = Constants.General.Random.Next(10, forestDensity);

                GrowCluster(startX, startY, clusterSize, 3);
            }

            // Generate rocks
            int rockCount = (int)Math.Ceiling(_world.Width * _world.Height / 200d);
            int rockDensity = 20; // tweak for rock density

            for (int i = 0; i < rockCount; i++)
            {
                int startX = Constants.General.Random.Next(0, _world.Width);
                int startY = Constants.General.Random.Next(0, _world.Height);

                int clusterSize = Constants.General.Random.Next(5, rockDensity);

                GrowCluster(startX, startY, clusterSize, 4);
            }

            // Generate berry bushes
            var bushesCount = (int)Math.Ceiling(_world.Width * _world.Height / 100d * 5);
            for (int i=0; i < bushesCount; i++)
            {
                int x = 0, y = 0, counter = 0;
                while (_world.TileGrid.GetTile(x, y).Id != 0)
                {
                    x = Constants.General.Random.Next(0, _world.Width);
                    y = Constants.General.Random.Next(0, _world.Height);
                    counter++;
                    if (counter >= 500) 
                        break;
                }

                if (counter >= 500) 
                    break;

                _world.TileGrid.SetTile(x, y, 2, true);
            }
        }

        public void GenerateTribes()
        {
            // TODO: Spawn player and AI hut, and define a way to set a unique color for player and AI
            _world.TileGrid.SetTile(_world.Width / 2, _world.Height / 2, 5, true);
        }

        private void GrowCluster(int x, int y, int clusterSize, int tileId)
        {
            var toVisit = new Queue<(int X, int Y)>();
            var visited = new HashSet<(int, int)>();

            toVisit.Enqueue((x, y));

            while (toVisit.Count > 0 && clusterSize > 0)
            {
                var (cx, cy) = toVisit.Dequeue();

                if (!_world.TileGrid.InBounds(cx, cy)) continue;
                if (!visited.Add((cx, cy))) continue; // skip already processed
                if (_world.TileGrid.GetTile(cx, cy).Id != 0) continue; // Skip when not grass

                _world.TileGrid.SetTile(cx, cy, tileId, true);
                clusterSize--;

                // Randomly push neighbors to spread
                foreach (var (X, Y) in GetRandomizedDirections())
                {
                    if (Constants.General.Random.NextDouble() < 0.6) // ~60% chance to spread
                        toVisit.Enqueue((cx + X, cy + Y));
                }
            }
        }

        private static readonly (int X, int Y)[] _directions =
        [
            (1, 0), (-1, 0), (0, 1), (0, -1),
            (1, 1), (1, -1), (-1, 1), (-1, -1)
        ];

        private static IEnumerable<(int X, int Y)> GetRandomizedDirections()
        {
            return _directions.OrderBy(_ => Constants.General.Random.Next());
        }
    }
}
