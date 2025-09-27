using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class ChaseMovement : MovementBase
    {
        private const float StaminaRegainChance = 0.25f;
        private const float ChanceToLoseStaminaWhenChasing = 0.8f;
        private const int StaminaDrainFleeing = 4;
        private const int StaminaRecoveryWhenResting = 2;

        public override void UpdateMovement(Actor self)
        {
            var target = self.AIController.CurrentTarget;
            if (target == null)
            {
                self.AIController.CurrentPath.Clear();
                return;
            }

            // Check if exhausted then stop chasing
            if (self.AIController.RunningStamina < StaminaDrainFleeing)
            {
                // Chance to regain stamina
                if (Random.Shared.NextDouble() < StaminaRegainChance)
                {
                    self.AIController.RunningStamina += StaminaRecoveryWhenResting;
                }
                return;
            }

            // Update path if target moves, or we reached destination
            if (self.AIController.CurrentPath.Count == 0 ||
                target.Position != self.AIController.LastKnownTargetPosition)
            {
                self.AIController.CurrentPath.Clear();
                EnqueuePath(self, target.Position);
                self.AIController.LastKnownTargetPosition = target.Position;
            }

            // Reduce stamina if we can traverse the path, and chance hits
            if (self.AIController.CurrentPath.Count != 0 && Random.Shared.NextDouble() < ChanceToLoseStaminaWhenChasing)
                self.AIController.RunningStamina = Math.Min(Math.Max(0, self.AIController.RunningStamina - StaminaDrainFleeing), 100);

            MoveOnCurrentPath(self);
        }
    }
}
