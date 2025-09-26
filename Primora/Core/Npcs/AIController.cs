using Primora.Core.Npcs.AIModules;
using Primora.Core.Npcs.Interfaces;
using System;
using System.Collections.Generic;

namespace Primora.Core.Npcs
{
    internal class AIController
    {
        public Actor Actor { get; }

        private readonly List<IMovementModule> _movementModules = [];
        private readonly List<IAwarenessModule> _awarenessModules = [];
        private readonly List<ICombatModule> _combatModules = [];
        private readonly List<IDecisionModule> _decisionModules = [];

        public IReadOnlyCollection<IMovementModule> MovementModules => _movementModules;
        public IReadOnlyCollection<IAwarenessModule> AwarenessModules => _awarenessModules;
        public IReadOnlyCollection<ICombatModule> CombatModules => _combatModules;
        public IReadOnlyCollection<IDecisionModule> DecisionModules => _decisionModules;

        public Actor CurrentTarget { get; private set; }

        // Cached hashset for performance
        private readonly HashSet<Actor> _detectedTargets = [];

        public AIController(Actor actor, HashSet<IAIModule> aiModules)
        {
            Actor = actor;

            // Assign all AI Modules properly
            foreach (var module in aiModules ?? [])
                AssignModule(module);
        }

        public void Update()
        {
            // Step 0: Collect all nearby targets, TODO: adjust based on aggro range?
            var nearbyActors = ActorManager.GetActorsAround(Actor.Location, Actor.Position, 4, true);

            // Step 1: Awareness — collect targets
            _detectedTargets.Clear();
            foreach (var module in AwarenessModules)
            {
                var target = module.Detect(Actor, nearbyActors);
                if (target != null)
                    _detectedTargets.Add(target);
            }

            // Step 2: Decision — choose actions and steer modules
            foreach (var decision in DecisionModules)
                decision.Decide(Actor, SelectClosestTarget(_detectedTargets));

            // Step 3: Movement — follow the currently active movement module
            foreach (var movement in MovementModules)
                movement.UpdateMovement(Actor, CurrentTarget);

            // Step 4: Combat — attack if in range
            foreach (var combat in CombatModules)
                combat.UpdateCombat(Actor, CurrentTarget);
        }

        private Actor SelectClosestTarget(IEnumerable<Actor> targets)
        {
            Actor closest = null;
            int bestDistance = int.MaxValue;

            foreach (var target in targets)
            {
                int dx = Math.Abs(target.Position.X - Actor.Position.X);
                int dy = Math.Abs(target.Position.Y - Actor.Position.Y);

                int manhattanDistance = dx + dy; // Manhattan distance
                if (manhattanDistance < bestDistance)
                {
                    bestDistance = manhattanDistance;
                    closest = target;
                }
            }

            return closest;
        }

        private void AssignModule(IAIModule module)
        {
            if (module is IMovementModule movementModule)
                _movementModules.Add(movementModule);
            else if (module is IAwarenessModule awarenessModule)
                _awarenessModules.Add(awarenessModule);
            else if(module is ICombatModule combatModule)
                _combatModules.Add(combatModule);
            else if(module is IDecisionModule decisionModule)
                _decisionModules.Add(decisionModule);
        }
    }
}
