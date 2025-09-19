using Primora.Core.Npcs.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Primora.Core.Npcs.Registries
{
    internal static class ActorRegistry
    {
        private static readonly Dictionary<Entities, ActorDefinition> _actorDefinitionsCache;

        static ActorRegistry()
        {
            var json = File.ReadAllText(Constants.GameData.ActorDefinitions);

            List<ActorDefinition> biomeDefinitions;
            try
            {
                biomeDefinitions = JsonSerializer.Deserialize<List<ActorDefinition>>(json, Constants.General.SerializerOptions);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to load file \"{Constants.GameData.ActorDefinitions}\": {e.Message}", e);
            }

            _actorDefinitionsCache = biomeDefinitions.ToDictionary(a => a.Entity);
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
