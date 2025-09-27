using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class OpportunisticDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            Actor easiestTarget = null;
            int lowestHP = int.MaxValue;

            if (self.AIController.CurrentTarget == null)
            {
                foreach (var target in detectedTargets)
                {
                    if (target.Stats.Health <= 0 || target.Faction == self.Faction)
                        continue;

                    // If health is too high we don't want to consider this target as viable
                    if (target.Stats.Health > (int)Math.Round(target.Stats.MaxHealth * 0.5f))
                    {
                        // TODO: Check if the target is hostile and can sense us then chance to flee else skip
                        if (target.IsHostileTowards(self) && (target.AIController == null || target.AIController.IsAwareOf(self)))
                        {
                            if (Random.Shared.NextDouble() < 0.3)
                            {
                                self.AIController.CurrentTarget = target;
                                self.AIController.AIState = AIState.Flee;
                            }
                            continue;
                        }
                        else
                        {
                            // Sometimes take a risk
                            if (Random.Shared.NextDouble() >= 0.1)
                            {
                                continue;
                            }
                        }
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
            }

            if (self.AIController.CurrentTarget != null && self.AIController.CurrentTarget.Stats.Health <= 0)
                self.AIController.CurrentTarget = null;
        }
    }
}
