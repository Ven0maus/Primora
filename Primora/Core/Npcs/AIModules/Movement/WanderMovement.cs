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
                var target = RandomPositionWithinRange(self, 5);
                var path = self.Pathfinder.ShortestPath(self.Position, target);
                foreach (var step in path.Steps)
                    self.AIController.CurrentPath.Enqueue(step);
            }
            if (self.AIController.CurrentPath.Count != 0)
            {
                if (!self.Move(self.AIController.CurrentPath.Dequeue()))
                    self.AIController.CurrentPath.Clear();
            }

            // Increase stamina when wandering by one
            self.AIController.RunningStamina = Math.Min(self.AIController.RunningStamina + 1, 100);
        }
    }
}
