using Primora.Core.Npcs.AIModules.Decision;
using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Core.Npcs
{
    internal class AIController
    {
        /// <summary>
        /// The actor this controller is controlling.
        /// </summary>
        public Actor Actor { get; }
        /// <summary>
        /// The current state the AI is in.
        /// </summary>
        public AIState AIState { get; set; }

        /// <summary>
        /// If there is a target in awareness of actor, this is the closest target.
        /// </summary>
        public Actor CurrentTarget { get; set; }
        /// <summary>
        /// Used for tracking target with pathfinding updates in AIState.Chase
        /// </summary>
        public Point LastKnownTargetPosition { get; set; }
        /// <summary>
        /// If pathfinding, this is the path it is currently going.
        /// </summary>
        public Queue<Point> CurrentPath { get; }
        /// <summary>
        /// Used for patrolling area when in AIState.Patrol
        /// </summary>
        public int CurrentPatrolIndex { get; set; } = 0;
        /// <summary>
        /// Defines how many turns to wait before continueing with patrol.
        /// </summary>
        public int PatrolPauseCounter { get; set; } = 0;
        /// <summary>
        /// The current route the actor is patrolling.
        /// </summary>
        public List<Point> PatrolRoute { get; set; }
        /// <summary>
        /// Helper to track stamina for flee and chase states.
        /// <br>This prevents infinite fleeing and chasing.</br>
        /// </summary>
        public int RunningStamina { get; set; } = 100;

        /// <summary>
        /// Defines if the current AIController behaves as a prey.
        /// </summary>
        public bool IsPrey => _decisionModule is PreyDecision;
        /// <summary>
        /// Defines if the current AIController behaves as a predator.
        /// </summary>
        public bool IsPredator => _decisionModule is PredatorDecision;
        /// <summary>
        /// Defines if the current AIController behaves aggressively.
        /// </summary>
        public bool IsAggressive => _decisionModule is AggressiveDecision;

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
            CurrentPath = [];

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
                // We are changing our movement logic, reset current path
                if (CurrentPath.Count > 0)
                    CurrentPath.Clear();

                _movementModule = AIBehaviour.GetMovementModule(AIState);
            }

            // Step 4: Execute movement logic
            _movementModule?.UpdateMovement(Actor);

            // Step 5: Execute combat logic if in combat state
            if (AIState == AIState.Combat)
                _combatModule?.UpdateCombat(Actor);
        }

        /// <summary>
        /// Check if the npc is aware of the specified actor.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool IsAwareOf(Actor actor)
        {
            return _awarenessModules.Any(a => a.Detect(Actor).Contains(actor));
        }
    }
}
