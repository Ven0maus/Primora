using Primora.Extensions;
using Primora.Screens.Abstracts;
using SadConsole;

namespace Primora.Screens.Main
{
    internal class AbilityScreen : TextScreen
    {
        private const string Title = "Abilities";

        public AbilityScreen(int width, int height) : 
            base(Title, width, height)
        {

        }

        public override void UpdateDisplay()
        {
            View.Clear();

            var color = "#adadad".HexToColor();
            View.Print(1, 1, "1. No ability set.", color);
            View.Print(1, 3, "2. No ability set.", color);
            View.Print(1, 5, "3. No ability set.", color);
            View.Print(1, 7, "4. No ability set.", color);
            View.Print(1, 9, "5. No ability set.", color);
        }
    }
}
