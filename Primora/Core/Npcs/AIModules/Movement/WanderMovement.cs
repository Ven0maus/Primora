using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    /// <summary>
    /// Constant random wandering movement
    /// </summary>
    internal class WanderMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentPath.Count == 0)
            {
                // Shorter range then hunt
                EnqueuePath(self, RandomPositionWithinRange(self, 5));
            }

            // Skip ticks randomly
            if (Random.Shared.NextDouble() < 0.6)
                return;

            MoveOnCurrentPath(self);

            // Increase stamina when wandering by one
            self.AIController.RunningStamina = Math.Min(self.AIController.RunningStamina + 1, 100);
        }
    }
}
