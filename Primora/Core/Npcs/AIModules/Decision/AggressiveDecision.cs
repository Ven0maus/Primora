using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class AggressiveDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            Actor closestTarget = null;
            int closestDistance = int.MaxValue;

            foreach (var target in detectedTargets)
            {
                if (target.Stats.Health <= 0 || !target.IsHostileTowards(self))
                    continue;

                int dist = self.DistanceTo(target.Position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestTarget = target;
                }
            }

            if (closestTarget != null)
            {
                self.AIController.CurrentTarget = closestTarget;
                self.AIController.AIState =
                    closestDistance <= self.Stats.AttackRange
                        ? AIState.Combat
                        : AIState.Chase;
            }
            else
            {
                self.AIController.CurrentTarget = null;

                if (self.AIController.PatrolRoute != null && self.AIController.PatrolRoute.Count > 1)
                    self.AIController.AIState = AIState.Patrol; // Back to patrol
                else
                    self.AIController.AIState = AIState.Wander; // Wander around
            }
        }
    }
}
