using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Npcs
{
    internal class AIController
    {
        public Actor Actor { get; }
        public Actor CurrentTarget { get; set; }
        public AIState AIState { get; set; }

        // Cached hashset for performance
        private readonly HashSet<Actor> _detectedTargets = [];

        // AI Modules
        private IMovementModule _movementModule;
        private readonly ICombatModule _combatModule;
        private readonly IDecisionModule _decisionModule;
        private readonly IAwarenessModule[] _awarenessModules;

        public AIController(Actor actor, ActorDefinition actorDefinition)
        {
            Actor = actor;

            // These modules don't change anymore
            _decisionModule = AIBehaviour.GetDecisionModule(actorDefinition.DecisionType);
            _awarenessModules = [.. actorDefinition.AwarenessTypes.Select(AIBehaviour.GetAwarenessModule)];
            _combatModule = AIBehaviour.HybridCombatModule;
        }

        public void Update()
        {
            // Step 1: Detect surroundings based on awareness
            _detectedTargets.Clear();
            foreach (var module in _awarenessModules)
            {
                var targets = module.Detect(Actor);
                foreach (var target in targets)
                    _detectedTargets.Add(target);
            }

            var prevState = AIState;

            // Step 2: Decide what to do with current information
            _decisionModule?.Decide(Actor, _detectedTargets);

            // Step 3: Assign movement module to be used based on state
            if (prevState != AIState)
            {
                _movementModule = AIBehaviour.GetMovementModule(AIState);
            }

            // Step 4: Execute movement logic
            _movementModule?.UpdateMovement(Actor);

            // Step 5: Execute combat logic if in combat state
            if (AIState == AIState.Combat)
                _combatModule?.UpdateCombat(Actor);
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
    }
}
