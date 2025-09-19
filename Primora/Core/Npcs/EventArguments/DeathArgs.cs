using System;

namespace Primora.Core.Npcs.EventArguments
{
    internal class DeathArgs(Actor actor, Actor killedBy) : EventArgs
    {
        public Actor KilledBy { get; } = killedBy;
        public Actor Actor { get; } = actor;
    }
}
