using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class OpportunisticDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            Actor easiestTarget = null;
            int lowestHP = int.MaxValue;

            // Clear target if no longer in detection
            if (self.AIController.CurrentTarget != null && detectedTargets.Contains(self.AIController.CurrentTarget))
            {
                if (self.AIController.CurrentTarget.Stats.Health <= 0)
                    self.AIController.CurrentTarget = null;
                else
                    return;
            }
            else if (self.AIController.CurrentTarget != null)
                self.AIController.CurrentTarget = null;

            foreach (var target in detectedTargets)
            {
                if (target.Stats.Health <= 0 || target.Faction == self.Faction)
                    continue;

                // If health is too high we don't want to consider this target as viable
                if (target.Stats.Health > (int)Math.Round(target.Stats.MaxHealth * 0.5f))
                {
                    // Check if the target is hostile and can sense us then chance to flee else skip
                    if (target.IsHostileTowards(self) && (target.AIController == null || target.AIController.IsAwareOf(self)))
                    {
                        self.AIController.CurrentTarget = target;
                        self.AIController.AIState = AIState.Flee;
                        return;
                    }
                    continue;
                }

                // Start sorting
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

            if (self.AIController.CurrentTarget != null && self.AIController.CurrentTarget.Stats.Health <= 0)
                self.AIController.CurrentTarget = null;
        }
    }
}
