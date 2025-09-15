namespace Primora.Core.Procedural.Objects
{
    internal enum Biome
    {
        // Base biome
        Grassland,

        // Woodland is used purely for generation, but is eventually turned to forest or grassland and no longer available in the world
        Woodland, 
        Forest,

        // Higher elevation biomes
        Hills,
        Mountains,

        // Special biomes
        River,
        Road,
        Bridge,
        Settlement
    }
}
