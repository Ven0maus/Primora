using Primora.Core.Npcs.Interfaces;
using SadRogue.Primitives;
using System;

namespace Primora.Core.Npcs.AIModules
{
    internal abstract class MovementBase : IMovementModule
    {
        public abstract void UpdateMovement(Actor self);

        protected static Point RandomPositionWithinRange(Actor actor, int range)
        {
            int minX = Math.Max(0, actor.Position.X - range);
            int maxX = Math.Min(actor.Location.Width - 1, actor.Position.X + range);

            int minY = Math.Max(0, actor.Position.Y - range);
            int maxY = Math.Min(actor.Location.Height - 1, actor.Position.Y + range);

            return new Point(
                Random.Shared.Next(minX, maxX + 1),
                Random.Shared.Next(minY, maxY + 1)
            );
        }

        protected static void EnqueuePath(Actor self, Point target)
        {
            var path = self.Pathfinder.ShortestPath(self.Position, target);
            if (path != null)
            {
                foreach (var step in path.Steps)
                    self.AIController.CurrentPath.Enqueue(step);
            }
        }

        protected static void MoveOnCurrentPath(Actor self)
        {
            if (self.AIController.CurrentPath.Count != 0)
            {
                if (!self.Move(self.AIController.CurrentPath.Dequeue()))
                    self.AIController.CurrentPath.Clear();
            }
        }
    }
}
