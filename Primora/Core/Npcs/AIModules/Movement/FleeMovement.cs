using SadRogue.Primitives;
using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class FleeMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentTarget == null)
            {
                self.AIController.CurrentPath.Clear();
                return;
            }

            if (self.AIController.CurrentPath.Count == 0)
            {
                var dx = self.Position.X - self.AIController.CurrentTarget.Position.X;
                var dy = self.Position.Y - self.AIController.CurrentTarget.Position.Y;

                // Bias away from target, but still constrained
                var fleeTarget = new Point(
                    self.Position.X + Math.Sign(dx) * Random.Shared.Next(5, 10),
                    self.Position.Y + Math.Sign(dy) * Random.Shared.Next(5, 10)
                );

                // clamp into valid bounds
                fleeTarget = new Point(
                    Math.Clamp(fleeTarget.X, 0, self.Location.Width - 1),
                    Math.Clamp(fleeTarget.Y, 0, self.Location.Height - 1)
                );

                EnqueuePath(self, fleeTarget);
            }

            MoveOnCurrentPath(self);
        }
    }
}
