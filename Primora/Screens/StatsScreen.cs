using Primora.Core.Npcs.Actors;
using Primora.Extensions;
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
            View.Clear();

            var stats = Player.Instance.Stats;
            var color = "#adadad".HexToColor();
            View.Print(1, 1, $"Health: {stats.Health}", color);
        }
    }
}
