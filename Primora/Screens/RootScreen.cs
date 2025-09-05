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
            foreach (var surface in World.Surfaces.Values)
                Children.Add(surface);
        }
    }
}
