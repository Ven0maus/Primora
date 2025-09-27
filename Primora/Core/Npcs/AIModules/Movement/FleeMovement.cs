using SadRogue.Primitives;
using System;

namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class FleeMovement : MovementBase
    {
        private const float StaminaRegainChance = 0.4f;
        private const float ChanceToLoseStaminaWhenFleeing = 0.8f;
        private const int StaminaDrainFleeing = 5;
        private const int StaminaRecoveryWhenResting = 2;

        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentTarget == null)
            {
                return;
            }

            // Always recalculate flee path, it could change
            self.AIController.CurrentPath.Clear();

            // Rest when exhausted
            if (self.AIController.RunningStamina < StaminaDrainFleeing)
            {
                // Chance to regain stamina
                if (Random.Shared.NextDouble() < StaminaRegainChance)
                {
                    self.AIController.RunningStamina += StaminaRecoveryWhenResting;
                }
                return;
            }

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

            // Reduce stamina if we can traverse the path, and chance hits
            if (self.AIController.CurrentPath.Count != 0 && Random.Shared.NextDouble() < ChanceToLoseStaminaWhenFleeing)
                self.AIController.RunningStamina = Math.Min(Math.Max(0, self.AIController.RunningStamina - StaminaDrainFleeing), 100);

            MoveOnCurrentPath(self);
        }
    }
}
