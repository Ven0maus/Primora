namespace Primora.Core.Npcs.AIModules.Movement
{
    internal class HuntMovement : MovementBase
    {
        public override void UpdateMovement(Actor self)
        {
            if (self.AIController.CurrentPath.Count == 0)
            {
                // Enqueue a new path, larger than wander range
                EnqueuePath(self, RandomPositionWithinRange(self, 15));
            }

            MoveOnCurrentPath(self);
        }
    }
}
