using Primora.Core.Npcs.EventArguments;
using Primora.Core.Npcs.Objects;
using Primora.Core.Npcs.Registries;
using Primora.Core.Procedural.Objects;
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
        /// <summary>
        /// Contains all the equipment the actor is wielding.
        /// </summary>
        public EquipmentHandler Equipment { get; }

        /// <summary>
        /// Returns the zone or worldmap that the actor is active in.
        /// </summary>
        public ILocation Location { get; }

        private Actor(ILocation location, Point position, ActorDefinition actorDefinition) : 
            base(foreground: actorDefinition.Foreground,
                  background: Color.Transparent,
                  glyph: actorDefinition.Glyph,
                  zIndex: Constants.Npcs.NpcZIndex)
        {
            Stats = new ActorStats(this, actorDefinition);
            Equipment = new();
            Location = location;
            Position = position;

            // Register in manager on creation after position is defined
            ActorManager.Register(this);
            Stats.OnDeath += Actor_OnDeath;
        }

        private void Actor_OnDeath(object sender, DeathArgs e)
        {
            // Unregister from manager and eventlistener
            Stats.OnDeath -= Actor_OnDeath;
            ActorManager.Unregister(this);
        }

        public Actor(ILocation location, Point position, Entities npc) : 
            this(location, position, ActorRegistry.Get(npc))
        { }

        /// <summary>
        /// Move the actor to the specified position.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="checkCanMove"></param>
        /// <returns></returns>
        public virtual bool Move(Point targetPos, bool checkCanMove = true)
        {
            if (!InBounds(targetPos)) return false;
            if (checkCanMove && !CanMove(targetPos)) return false;
            Position = targetPos;
            return true;
        }

        /// <summary>
        /// Move the actor towards the specified direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="checkCanMove"></param>
        /// <returns></returns>
        public bool Move(Direction direction, bool checkCanMove = true)
        {
            var targetPos = Position + direction;
            return Move(targetPos, checkCanMove);
        }

        /// <summary>
        /// Execute a validation to test if the actor can move to the specified position.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        protected virtual bool CanMove(Point targetPos)
        {
            return Location.IsWalkable(targetPos) && !ActorManager.ActorExistsAt(Location, targetPos, out _);
        }

        /// <summary>
        /// Check if the position is within the walkable bounds of the location the actor resides in.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        private bool InBounds(Point targetPos)
            => targetPos.X >= 0 && targetPos.Y >= 0 && targetPos.X < Location.Width && targetPos.Y < Location.Height;
    }
}
