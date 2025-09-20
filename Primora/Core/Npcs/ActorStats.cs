using Primora.Core.Items;
using Primora.Core.Npcs.EventArguments;
using Primora.Core.Npcs.Objects;
using System;

namespace Primora.Core.Npcs
{
    internal sealed class ActorStats
    {
        public Actor Actor { get; }
        public int MaxHealth { get; set; }
        public int Health { get; private set; }

        /// <summary>
        /// Raised when the actor dies.
        /// </summary>
        public event EventHandler<DeathArgs> OnDeath;

        public ActorStats(Actor actor, ActorDefinition actorDefinition)
        {
            Actor = actor;
            MaxHealth = actorDefinition.MaxHealth;
            Health = MaxHealth;
        }

        /// <summary>
        /// Method to apply damage and correctly trigger the related events.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        public void ApplyDamage(int damage, Actor attacker)
        {
            if (damage <= 0 || Health <= 0)
                return;

            int previousHealth = Health;
            Health = Math.Max(0, previousHealth - damage);

            if (previousHealth > 0 && Health <= 0)
                OnDeath?.Invoke(this, new DeathArgs(Actor, attacker));
        }

        internal void AddItemStats(ItemStats providedStats)
        {

        }

        internal void RemoveItemStats(ItemStats providedStats)
        {

        }
    }
}
