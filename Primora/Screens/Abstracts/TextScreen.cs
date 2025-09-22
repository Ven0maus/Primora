using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens.Abstracts
{
    internal abstract class TextScreen : ScreenSurface
    {
        public readonly ScreenSurface View;
        public TextScreen(string title, int width, int height) : 
            base(width, height)
        {
            var nW = width - 2;
            var nH = height - 2;

            nW = (int)(nW * 1.4); // 1.4 because font is 4 pixels smaller than base font
            nH = (int)(nH * 1.4);
            View = new ScreenSurface(nW, nH)
            {
                Font = Game.Instance.Fonts["Cheepicus_12x12"],
                Position = new Point(1, 1)
            };
            Children.Add(View);

            Surface.DrawBorder(LineThickness.Thin, title, Color.Gray, Color.White);
        }

        public abstract void UpdateDisplay();
    }
}
