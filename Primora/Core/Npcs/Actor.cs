using GoRogue.FOV;
using GoRogue.Pathing;
using Primora.Core.Npcs.EventArguments;
using Primora.Core.Npcs.Objects;
using Primora.Core.Procedural.Objects;
using SadConsole.Entities;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using System;
using Zone = Primora.Core.Procedural.WorldBuilding.Zone;

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
        /// The pathfinding algorithm that is used.
        /// </summary>
        public FastAStar Pathfinder { get; protected set; }
        /// <summary>
        /// Represents the field of view for the actor.
        /// </summary>
        public IFOV FieldOfView { get; protected set; }
        public Faction Faction { get; protected set; }

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
            // General
            Name = actorDefinition.Name;
            Faction = actorDefinition.Faction;

            // Handlers
            Stats = new ActorStats(this, actorDefinition);
            Equipment = new();

            // Positioning and location
            Location = location;
            Position = position;

            // World map entities do not need pathfinding
            if (Location is Zone)
            {
                var walkabilityView = new LambdaGridView<bool>(() => Location.Width, () => Location.Height,
                    (a) => Location.IsWalkable(a) && !ActorManager.ActorExistsAt(Location, a, out _));
                Pathfinder = new FastAStar(walkabilityView, Distance.Manhattan);

                var transparencyView = new LambdaGridView<bool>(() => Location.Width, () => Location.Height,
                    (a) => !Location.ObstructsView(a));
                FieldOfView = new RecursiveShadowcastingBooleanBasedFOV(transparencyView);
            }

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

        public bool IsHostileTowards(Actor target)
        {
            return IsHostileTowardsInternal(target, true);
        }

        private bool IsHostileTowardsInternal(Actor target, bool reverseLookup)
        {
            // Same faction will never fight eachother (unless neutral faction)
            if (Faction != Faction.Neutral && target.Faction == Faction)
                return false;

            if (AIController == null)
            {
                // Player, do a reverse lookup
                return target.IsHostileTowardsInternal(this, false);
            }

            // If we are considered aggressive
            if (AIController.IsAggressive) return true;
            // If our current target is the same target or visa versa
            if (AIController.CurrentTarget == target || target.AIController?.CurrentTarget == this) return true;
            // If we are a predator and hungry or target is prey
            if (AIController.IsPredator && (Stats.Hunger > 80 || (target.AIController != null && target.AIController.IsPrey))) return true;

            // Do a reverse lookup in the other direction
            if (reverseLookup)
                return target.IsHostileTowardsInternal(this, false);

            return false;
        }

        /// <summary>
        /// Returns the MANHATTAN distance towards the given position from the actor.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int DistanceTo(Point position)
        {
            int dx = Math.Abs(position.X - Position.X);
            int dy = Math.Abs(position.Y - Position.Y);

            // Manhattan distance
            return dx + dy;
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

            // Calculate field of view if present
            FieldOfView?.Calculate(Position, Stats.VisionRange);

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
