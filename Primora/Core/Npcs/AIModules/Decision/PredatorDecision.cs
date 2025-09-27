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

            // TODO: Implement that if there are hostile enemies near prey, then maybe decide not to attack
            foreach (var target in detectedTargets)
            {
                // If not prey and its not hostile towards me
                // (Eg only hunt other predators that are hostile)
                // TODO: Fix so issues like Goblin and Goblin Brute won't attack eachother
                // (also can't use race, because human want to fight other humans)
                if (target.Name == self.Name) continue;

                // Is the target hostile towards me?
                var canAttack = target.AIController.IsPrey || target.IsHostileTowards(self);
                if (!canAttack)
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

            // If we have found prey, but we are not hungry and they are not attacking us at the moment, then skip
            if (closestPrey != null && self.Stats.Hunger < 50 && closestPrey.AIController.CurrentTarget != self)
                closestPrey = null;

            if (closestPrey != null)
            {
                self.AIController.CurrentTarget = closestPrey;

                // If low health, then flee else if in attack range → combat, otherwise chase
                if (self.Stats.Health <= self.Stats.Health * 0.15 && !closestPrey.AIController.IsPrey)
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
