using Primora.Core.Npcs.Objects;

namespace Primora.Core.Npcs
{
    internal sealed class ActorStats
    {
        public int MaxHealth { get; set; }
        public int Health { get; set; }

        public ActorStats(ActorDefinition actorDefinition)
        {
            MaxHealth = actorDefinition.MaxHealth;
            Health = MaxHealth;
        }
    }
}
