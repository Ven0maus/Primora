using SadConsole;

namespace Primora.Core
{
    /// <summary>
    /// The class that contains an assortment of objects that can be used to access various elements of the world.
    /// </summary>
    internal class World
    {
        /// <summary>
        /// The width of the world.
        /// </summary>
        public int Width => TileGrid.Width;
        /// <summary>
        /// The height of the world.
        /// </summary>
        public int Height => TileGrid.Height;
        /// <summary>
        /// The tile grid of the world. Contains methods to modify the world visuals.
        /// </summary>
        public readonly TileGrid TileGrid;
        
        public World(int width, int height, IFont.Sizes fontSize)
        {
            // Setup method container to be able to edit the world tiles
            TileGrid = new TileGrid(width, height, fontSize);
        }

        public void StartWorldGeneration()
        {
            var worldGen = new WorldGeneration(this);
            worldGen.GenerateGrounds();
            worldGen.GenerateFauna();
            worldGen.GenerateTribes();
        }
    }
}
