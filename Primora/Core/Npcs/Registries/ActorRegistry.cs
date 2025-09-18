using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Npcs.Registries
{
    internal static class ActorRegistry
    {
        private static readonly Dictionary<Entities, ActorDefinition> _actorDefinitionsCache;

        static ActorRegistry()
        {
            // Read and cache from the data file.
            var actorDefinitions = JsonSerializer.Deserialize<List<ActorDefinition>>(
                Constants.GameData.ActorDefinitions, Constants.General.SerializerOptions);
            _actorDefinitionsCache = actorDefinitions.ToDictionary(a => a.Entity);
        }

        /// <summary>
        /// Gets the cached actor definition for the specified entity.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ActorDefinition Get(Entities npc)
        {
            if (_actorDefinitionsCache.TryGetValue(npc, out var definition)) 
                return definition;
            throw new NotImplementedException("Entity \"{npc}\" is not implemented.");
        }
    }
}
