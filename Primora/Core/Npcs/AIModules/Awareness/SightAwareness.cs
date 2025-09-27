using Primora.Core.Npcs.Interfaces;
using System.Collections.Generic;

namespace Primora.Core.Npcs.AIModules.Awareness
{
    internal class SightAwareness : IAwarenessModule
    {
        public IEnumerable<Actor> Detect(Actor self)
        {
            // Simple FOV based detection
            var visiblePositions = self.FieldOfView.CurrentFOV;
            foreach (var position in visiblePositions)
            {
                // Check if an actor exists at the position in the FOV
                if (!ActorManager.ActorExistsAt(self.Location, position, out Actor actor) || actor == self)
                    continue;

                yield return actor;
            }
        }
    }
}
