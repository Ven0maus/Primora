using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class PatrolMovement : MovementBase
    {
        private const int DefaultPauseDuration = 3; // turns to wait at each patrol point

        public override void UpdateMovement(Actor self)
        {
            // Don't get stuck in some weird 1 position route, or empty route
            if (self.AIController.PatrolRoute == null || self.AIController.PatrolRoute.Count <= 1)
            {
                self.AIController.PatrolRoute = null;
                return;
            }

            // Increase stamina when patrolling by one
            self.AIController.RunningStamina = Math.Min(self.AIController.RunningStamina + 1, 100);

            // If we're waiting at a patrol point, decrement pause counter
            if (self.AIController.PatrolPauseCounter > 0)
            {
                self.AIController.PatrolPauseCounter--;
                return; // stay idle this tick
            }

            if (self.AIController.CurrentPath.Count == 0)
            {
                var target = self.AIController.PatrolRoute[self.AIController.CurrentPatrolIndex];

                // Advance index only if we’re at the target tile
                if (self.Position == target)
                {
                    self.AIController.PatrolPauseCounter = DefaultPauseDuration;

                    // Advance patrol index
                    self.AIController.CurrentPatrolIndex++;

                    // Wrap index if we go past the end
                    if (self.AIController.CurrentPatrolIndex >= self.AIController.PatrolRoute.Count)
                    {
                        // Randomly reverse or loop
                        if (Random.Shared.NextDouble() < 0.5)
                            self.AIController.PatrolRoute.Reverse();

                        // Set index to the first point that is not the current position
                        self.AIController.CurrentPatrolIndex =
                            self.AIController.PatrolRoute[0] == self.Position ? 1 : 0;
                    }
                    return;
                }

                EnqueuePath(self, target);
            }

            MoveOnCurrentPath(self);
        }
    }
}
