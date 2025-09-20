using Primora.Core.Npcs.Actors;
using SadConsole;

namespace Primora.Screens
{
    internal class StatsScreen : TextScreen
    {
        private const string Title = "Stats";

        public StatsScreen(int width, int height) : 
            base(Title, width, height)
        {

        }

        public override void UpdateDisplay()
        {
            var stats = Player.Instance.Stats;

            View.Print(1, 1, $"Health: {stats.Health}");
        }
    }
}
