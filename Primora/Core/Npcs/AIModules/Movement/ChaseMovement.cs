namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class ChaseMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentTarget == null)
            {
                self.AIController.CurrentPath.Clear();
                return;
            }

            // Update path if target moves, or we reached destination
            if (self.AIController.CurrentPath.Count == 0 ||
                self.AIController.CurrentTarget.Position != self.AIController.LastKnownTargetPosition)
            {
                self.AIController.CurrentPath.Clear();
                EnqueuePath(self, self.AIController.CurrentTarget.Position);
                self.AIController.LastKnownTargetPosition = self.AIController.CurrentTarget.Position;
            }

            MoveOnCurrentPath(self);
        }
    }
}
