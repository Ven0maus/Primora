using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.Objects;
using Primora.Screens;
using SadRogue.Primitives;
using SadRogue.Primitives.SpatialMaps;
using System;
using System.Collections.Generic;

namespace Primora.Core.Npcs
{
    internal static class ActorManager
    {
        private static readonly Dictionary<ILocation, AutoSyncSpatialMap<Actor>> _actorMaps = [];

        /// <summary>
        /// Register a new actor within the manager for tracking.
        /// </summary>
        /// <param name="actor"></param>
        public static void Register(Actor actor)
        {
            if (!_actorMaps.TryGetValue(actor.Location, out var actorMap))
                _actorMaps[actor.Location] = actorMap = [];
            actorMap.Add(actor);
        }

        /// <summary>
        /// Unregister an actor from the manager.
        /// </summary>
        /// <param name="actor"></param>
        public static void Unregister(Actor actor)
        {
            if (!_actorMaps.TryGetValue(actor.Location, out var actorMap))
                return;

            actorMap.Remove(actor);

            // Remove memory map if no actors remain inside
            if (actorMap.Count == 0) 
                _actorMaps.Remove(actor.Location);
        }

        /// <summary>
        /// Determine if an actor exists in the given location at the specified position.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="position"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static bool ActorExistsAt(ILocation location, Point position, out Actor actor)
        {
            actor = null;
            if (!_actorMaps.TryGetValue(location, out var actorMap)) return false;
            actor = actorMap.GetItemOrDefault(position);
            return actor != null;
        }

        /// <summary>
        /// Returns all the actors around a specified position within the specified radius.
        /// <br>(Optionally can include the player.)</br>
        /// </summary>
        /// <param name="source">The source position</param>
        /// <param name="radius">Radius of the search</param>
        /// <param name="includePlayerActor">Should the player be included in the search?</param>
        /// <returns></returns>
        public static ICollection<Actor> GetActorsAround(ILocation location, Point source, int radius, bool includePlayerActor = false)
        {
            if (!_actorMaps.TryGetValue(location, out var actorMap)) 
                return Array.Empty<Actor>();

            List<Actor> actors = null;
            for (int x = source.X - radius; x < source.X + radius; x++)
            {
                for (int y = source.Y - radius; x < source.Y + radius; y++)
                {
                    var actor = actorMap.GetItemOrDefault(new Point(x, y));
                    if (actor != null)
                    {
                        if (!includePlayerActor && actor is Player)
                            continue;

                        actors ??= [];
                        actors.Add(actor);
                    }
                }
            }
            return actors;
        }

        public static void RenderLocation(ILocation location)
        {
            var entityManager = RootScreen.Instance.EntityManager;
            entityManager.Clear();

            if (!_actorMaps.TryGetValue(location, out var actorMap)) return;

            entityManager.AddRange(actorMap.Items);
        }
    }
}
