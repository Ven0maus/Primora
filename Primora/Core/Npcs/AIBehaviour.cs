using Primora.Core.Npcs.AIModules.Awareness;
using Primora.Core.Npcs.AIModules.Combat;
using Primora.Core.Npcs.AIModules.Decision;
using Primora.Core.Npcs.AIModules.Movement;
using Primora.Core.Npcs.Interfaces;
using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;

namespace Primora.Core.Npcs
{
    internal static class AIBehaviour
    {
        /// <summary>
        /// Unique movement modules for each state
        /// </summary>
        private static readonly Dictionary<AIState, IMovementModule> _movementModules = new()
        {
            { AIState.Wander, new WanderMovement() }
        };

        private static readonly Dictionary<DecisionType, IDecisionModule> _decisionModules = new()
        {
            // Animalistic behaviours
            { DecisionType.Predator, new PredatorDecision() },
            { DecisionType.Prey, new PreyDecision() },

            // Humanoid behaviours
            { DecisionType.Opportunistic, new OpportunisticDecision() },
            { DecisionType.Aggressive, new AggressiveDecision() }
        };

        private static readonly Dictionary<AwarenessType, IAwarenessModule> _awarenessModules = new()
        {
            { AwarenessType.Sight, new SightAwareness() }
        };

        /// <summary>
        /// Standard generic combat module
        /// </summary>
        public static readonly ICombatModule HybridCombatModule = new HybridCombat();

        internal static IMovementModule GetMovementModule(AIState aiState)
        {
            if (aiState == AIState.Idle) return null;
            if (_movementModules.TryGetValue(aiState, out var module))
                return module;
            throw new NotImplementedException($"No MovementModule for AIState \"{aiState}\" implemented yet.");
        }

        internal static IDecisionModule GetDecisionModule(DecisionType decisionType)
        {
            if (_decisionModules.TryGetValue(decisionType, out var module))
                return module;
            throw new NotImplementedException($"No DecisionModule \"{decisionType}\" implemented yet.");
        }

        internal static IAwarenessModule GetAwarenessModule(AwarenessType awarenessType)
        {
            if (_awarenessModules.TryGetValue(awarenessType, out var module))
                return module;
            throw new NotImplementedException($"No AwarenessModule \"{awarenessType}\" implemented yet.");
        }
    }
}
