namespace Primora.Core.Npcs.Objects
{
    internal enum DecisionType
    {
        // Animalistic behaviours
        Prey, // avoids combat, wanders, grazes
        Predator, // focuses on self preservation by hunting weaker enemies if hungry

        // Humanoid behaviours
        Opportunistic, // attacks if conditions look favorable (weaker target, group advantage, etc.).
        Aggressive // attacks as soon as target is acquired, does not flee
    }
}
