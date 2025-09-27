using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class GrazeMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentPath.Count == 0)
            {
                // Mostly stand still, sometimes move slightly
                if (Random.Shared.NextDouble() < 0.8)
                    return;

                var target = RandomPositionWithinRange(self, 1);
                EnqueuePath(self, target);
            }

            MoveOnCurrentPath(self);
        }
    }
}
