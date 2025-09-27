namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class CombatMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            // Attempt to stay in combat range, based on combat style
            if (self.AIController.CurrentTarget == null)
            {
                self.AIController.CurrentPath.Clear();
                return;
            }

            int dist = self.DistanceTo(self.AIController.CurrentTarget.Position);
            if (dist > self.Stats.AttackRange && self.AIController.CurrentPath.Count == 0)
            {
                EnqueuePath(self, self.AIController.CurrentTarget.Position);
            }

            // Stop moving when in attack range
            if (dist <= self.Stats.AttackRange)
                self.AIController.CurrentPath.Clear();

            MoveOnCurrentPath(self);
        }
    }
}
