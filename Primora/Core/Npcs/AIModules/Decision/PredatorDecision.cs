using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class PredatorDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            // Find the closest prey
            Actor closestPrey = null;
            int closestDistance = int.MaxValue;

            // Only decide on a target if predator is hungry
            if (self.Stats.Hunger >= 50)
            {
                foreach (var target in detectedTargets)
                {
                    if (target.Stats.Health <= 0) continue;

                    // Never the same faction
                    if (self.Faction == target.Faction) continue;

                    var isHostile = self.IsHostileTowards(target);
                    var isPrey = target.AIController != null && target.AIController.IsPrey;

                    // Anything prey or hostile is a valid target
                    // However when really hungry anything is a valid target
                    if (!isPrey && !isHostile)
                    {
                        // If we are not hungry enough to be willing to attack this, then continue
                        if (self.Stats.Hunger < 80)
                            continue;
                    }

                    int dist = self.DistanceTo(target.Position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestPrey = target;
                    }
                }
            }

            if (closestPrey != null)
            {
                self.AIController.CurrentTarget = closestPrey;

                // If low health, then flee else if in attack range → combat, otherwise chase
                if (self.Stats.Health <= self.Stats.Health * 0.15 && (closestPrey.AIController == null || !closestPrey.AIController.IsPrey))
                    self.AIController.AIState = AIState.Flee;
                else if (closestDistance <= self.Stats.AttackRange)
                    self.AIController.AIState = AIState.Combat;
                else
                    self.AIController.AIState = AIState.Chase;
            }
            else
            {
                self.AIController.CurrentTarget = null;

                if (self.Stats.Hunger > 50)
                    self.AIController.AIState = AIState.Hunt;
                else
                    self.AIController.AIState = AIState.Wander;
            }
        }
    }
}
