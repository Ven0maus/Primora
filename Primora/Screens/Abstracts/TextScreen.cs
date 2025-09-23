using Primora.Extensions;
using SadConsole;
using SadRogue.Primitives;

namespace Primora.Screens.Abstracts
{
    internal abstract class TextScreen : ScreenSurface
    {
        public readonly ScreenSurface View;
        public TextScreen(string title, int width, int height, IFont font = null) : 
            base(width, height)
        {
            var parentFontSize = FontSize;
            var fontToBeUsed = font ?? Game.Instance.Fonts["Cheepicus_12x12"];
            var fontToBeUsedSize = fontToBeUsed.GetFontSize(IFont.Sizes.One);

            int nW = (width - 2) * parentFontSize.X / fontToBeUsedSize.X;
            int nH = (height - 2) * parentFontSize.Y / fontToBeUsedSize.Y;

            View = new ScreenSurface(nW, nH)
            {
                Font = fontToBeUsed,
                Position = new Point(1, 1)
            };
            Children.Add(View);

            Surface.DrawBorder(LineThickness.Thin, title, Color.Gray, Color.White);
        }

        public abstract void UpdateDisplay();
    }
}
