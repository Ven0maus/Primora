using Primora.Core.Npcs.EventArguments;
using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.Objects;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace Primora.Core.Npcs
{
    internal abstract class Actor : Entity
    {
        /// <summary>
        /// The controller that handles AI modules.
        /// </summary>
        public AIController AIController { get; protected set; }
        /// <summary>
        /// Contains the modifiable stats of the actor.
        /// </summary>
        public ActorStats Stats { get; }
        /// <summary>
        /// Contains all the equipment the actor is wielding.
        /// </summary>
        public EquipmentManager Equipment { get; }

        /// <summary>
        /// Returns the zone or worldmap that the actor is active in.
        /// </summary>
        public ILocation Location { get; protected set; }

        public Actor(ILocation location, Point position, ActorDefinition actorDefinition) : 
            base(foreground: actorDefinition.Color,
                  background: Color.Transparent,
                  glyph: actorDefinition.Glyph,
                  zIndex: Constants.Npcs.NpcZIndex)
        {
            // Handlers
            AIController = new(this, actorDefinition.AIModules);
            Stats = new ActorStats(this, actorDefinition);
            Equipment = new();

            // Positioning and location
            Location = location;
            Position = position;

            // Register in manager on creation after position is defined
            ActorManager.Register(this);
            Stats.OnDeath += Actor_OnDeath;
        }

        public Actor(ILocation location, Point position, string npc) :
            this(location, position, ActorDefinition.Get(npc))
        { }

        private void Actor_OnDeath(object sender, DeathArgs e)
        {
            // Unregister from manager and eventlistener
            Stats.OnDeath -= Actor_OnDeath;
            ActorManager.Unregister(this);
        }

        public virtual void EndTurn()
        {
            AIController?.Update();
        }

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
