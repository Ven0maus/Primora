using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class OpportunisticDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            Actor easiestTarget = null;
            int lowestHP = int.MaxValue;

            foreach (var target in detectedTargets)
            {
                if (target.Stats.Health <= 0 || !self.IsHostileTowards(target))
                    continue;

                if (target.Stats.Health < lowestHP)
                {
                    lowestHP = target.Stats.Health;
                    easiestTarget = target;
                }
            }

            if (easiestTarget != null)
            {
                self.AIController.CurrentTarget = easiestTarget;
                self.AIController.AIState = self.DistanceTo(easiestTarget.Position) <= self.Stats.AttackRange
                        ? AIState.Combat : AIState.Chase;
            }
            else
            {
                self.AIController.CurrentTarget = null;
                self.AIController.AIState = AIState.Wander;
            }
        }
    }
}
