using Primora.Core;
using SadConsole;
using Game = SadConsole.Game;

namespace Primora.Screens
{
    internal class RootScreen : ScreenObject
    {
        public readonly World World;

        public RootScreen()
        {
            World = new World(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY, IFont.Sizes.Two);
            Children.Add(World.TileGrid.Surface);

            // Entry
            World.StartWorldGeneration();
        }
    }
}
