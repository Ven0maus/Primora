using Primora.Core.Npcs.Objects;
using Primora.Core.Npcs.Registries;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace Primora.Core.Npcs
{
    internal abstract class Actor : Entity
    {
        /// <summary>
        /// Contains the modifiable stats of the actor.
        /// </summary>
        public ActorStats Stats { get; }

        private Actor(ActorDefinition actorDefinition) : 
            base(foreground: actorDefinition.Foreground,
                  background: Color.Transparent,
                  glyph: actorDefinition.Glyph,
                  zIndex: Constants.Npcs.NpcZIndex)
        {
            Stats = new ActorStats(actorDefinition);
        }

        public Actor(Entities npc) : this(ActorRegistry.Get(npc))
        { }

        // TODO: Implement movement method and CanMove validation method
    }
}
