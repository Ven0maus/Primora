using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Decision
{
    internal class PreyDecision : IDecisionModule
    {
        public void Decide(Actor self, IEnumerable<Actor> detectedTargets)
        {
            // Find the closest predator
            Actor closestThreat = null;
            int closestDistance = int.MaxValue;

            foreach (var target in detectedTargets)
            {
                if (target.Stats.Health <= 0) continue;

                // Consider non-ai neutral (player?)
                if (target.AIController == null) continue;

                // Always scared of predators
                if (!target.AIController.IsPredator && !target.IsHostileTowards(self)) continue;

                int dist = self.DistanceTo(target.Position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestThreat = target;
                }
            }

            if (closestThreat != null)
            {
                // Set AI to flee
                self.AIController.CurrentTarget = closestThreat;
                self.AIController.AIState = AIState.Flee;
            }
            else
            {
                // Nothing threatening nearby → graze or wander
                self.AIController.CurrentTarget = null;
                self.AIController.AIState = Random.Shared.NextDouble() < 0.1 ? 
                    AIState.Wander: AIState.Graze;
            }
        }
    }
}
