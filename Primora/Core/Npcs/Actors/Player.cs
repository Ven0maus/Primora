using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.WorldBuilding;
using Primora.Screens.Main;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Primora.Core.Npcs.Actors
{
    internal sealed class Player : Actor
    {
        public static Player Instance { get; private set; }

        /// <summary>
        /// Returns the current player zone.
        /// </summary>
        public new Zone Location
        {
            get => (Zone)base.Location;
            private set => base.Location = value;
        }

        /// <summary>
        /// Returns the position of the player on the world map.
        /// </summary>
        public Point WorldPosition
        {
            get => World.Instance.PlayerWorldMapEntity.Position;
        }

        /// <summary>
        /// True if the player is in aiming mode.
        /// </summary>
        public bool IsAiming { get; set; } = false;

        public bool IsFastTraveling { get; private set; } = false;

        public Player(Zone zone, Point position) : 
            base(zone, position, nameof(Player))
        {
            if (Instance != null)
                throw new Exception("An instance of the player already exists.");
            Instance = this;
            AIController = null; // Player doesn't need an AI Controller.

            PositionChanged += Player_PositionChanged;
        }

        /// <summary>
        /// Travels the player into a new zone.
        /// </summary>
        /// <param name="worldPosition"></param>
        public async Task Travel(Point[] steps)
        {
            if (steps.Length == 0) return;

            IsFastTraveling = true;
            RootScreen.Instance.WorldScreen.DisableWorldMapDrag = true;

            var world = World.Instance;
            var worldMapEntity = World.Instance.PlayerWorldMapEntity;
            double remainingTurns = 0;

            // Compute per-tile normalized weights
            double maxWeight = steps.Max(p => world.WorldMap.Weights[p]);
            var normalizedWeights = steps.Select(p => world.WorldMap.Weights[p] / maxWeight).ToArray();

            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];

                // Move player visually but don't open any zones or change player's location yet
                worldMapEntity.Position = step;

                // Calculate turn cost for this tile
                double costThisTile = normalizedWeights[i] * Constants.Worldmap.TurnsPerTile_FastTravel;
                remainingTurns += costThisTile;
                if (remainingTurns < 0)
                    remainingTurns = 1;

                // Spend whole turns
                while (remainingTurns >= 1)
                {
                    world.EndTurn();
                    remainingTurns -= 1;
                }

                // Wait for next step
                await Task.Delay(50);
            }

            // Set new zone as current location
            var prevLocationPosition = Location.WorldPosition;
            Location = world.OpenZone(steps.Last(), movePlayerEntity: true);

            // Update zone position
            Position = FindSuitableZoneSpawnPositionFromTravel(steps, prevLocationPosition);

            // No longer fast traveling
            IsFastTraveling = false;
        }

        private Point FindSuitableZoneSpawnPositionFromTravel(Point[] steps, Point prevLocationPosition)
        {
            Point fromTile = steps.Length > 1 ? steps[^2] : prevLocationPosition;
            Point toTile = steps[^1];

            // Direction vector (normalized to -1, 0, 1)
            Point dir = new Point(
                Math.Sign(toTile.X - fromTile.X),
                Math.Sign(toTile.Y - fromTile.Y)
            );

            // Candidate positions along the border depending on direction
            // TODO: Make sure there is atleast a 1 tile radius around the candidates walkable
            List<Point> candidates = [];

            if (dir.X == -1) // entering from left → spawn on right border
                for (int y = 0; y < Location.Height; y++)
                    candidates.Add(new Point(Location.Width - 1, y));
            else if (dir.X == 1) // entering from right → spawn on left border
                for (int y = 0; y < Location.Height; y++)
                    candidates.Add(new Point(0, y));

            if (dir.Y == -1) // entering from top → spawn on bottom border
                for (int x = 0; x < Location.Width; x++)
                    candidates.Add(new Point(x, Location.Height - 1));
            else if (dir.Y == 1) // entering from bottom → spawn on top border
                for (int x = 0; x < Location.Width; x++)
                    candidates.Add(new Point(x, 0));

            // Remove all non-walkable tiles from candidates
            candidates.RemoveAll(a => !Location.GetTileInfo(a).Walkable);

            // TODO: Add more valid candidates
            if (candidates.Count == 0)
                throw new Exception("No valid player spawn position tile found.");

            Point spawnTile = candidates[Location.Random.Next(candidates.Count)];
            return spawnTile;
        }

        private void Player_PositionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            if (World.Instance.WorldMap.IsDisplayed) return;

            // Center view on player
            RootScreen.Instance.WorldScreen.ViewPosition = Position - new Point(
                RootScreen.Instance.WorldScreen.ViewWidth / 2,
                RootScreen.Instance.WorldScreen.ViewHeight / 2);
        }
    }
}
