using Primora.Extensions;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primora.Screens.Helpers
{
    /// <summary>
    /// A helper class to quickly build screens to display.
    /// </summary>
    internal class ScreenBuilder
    {
        private string _title;
        private bool _enableXButton = false;
        private bool _surroundWithBorder = false;
        private LineThickness _lineThickness;
        private Point _desiredPosition = Point.Zero;
        private readonly List<string> _texts = [];
        private readonly List<(string Label, Action OnClick)> _buttons = [];

        public ScreenBuilder AddTitle(string title)
        {
            _title = title;
            return this;
        }

        public ScreenBuilder AppendTextLine(string text)
        {
            _texts.Add(text);
            return this;
        }

        public ScreenBuilder SurroundWithBorder(LineThickness lineThickness = LineThickness.Thin)
        {
            _lineThickness = lineThickness;
            _surroundWithBorder = true;
            return this;
        }

        public ScreenBuilder AddButton(string label, Action onClick)
        {
            _buttons.Add((label, onClick));
            return this;
        }

        public ScreenBuilder EnableXButton() 
        {
            _enableXButton = true;
            return this; 
        }

        /// <summary>
        /// Will attempt to position the surface above the given position. 
        /// If not possible, will stay within the parent's bounds.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ScreenBuilder Position(Point position)
        {
            _desiredPosition = position;
            return this;
        }

        /// <summary>
        /// Returns a screensurface build based on the configured settings.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configureScreen">Executes to adapt settings on the screen.</param>
        /// <returns></returns>
        public ControlsConsole Build(IScreenObject parent, Action<ControlsConsole> configureScreen = null, Action onClose = null)
        {
            // --- measure content ---
            int width = 0;

            if (!string.IsNullOrEmpty(_title))
                width = Math.Max(width, _title.Length + 4);

            if (_texts.Count > 0)
                width = Math.Max(width, _texts.Max(t => t.Length) + 3); // 3 because start is offset by 1, and X comes on top of last

            if (_buttons.Count > 0)
                width = Math.Max(width, _buttons.Max(b => b.Label.Length + 4));

            width = Math.Max(width, 20); // minimum width

            int height = 1; // title row
            height += _texts.Count;
            height += _buttons.Count > 0 ? _buttons.Count + 1 : 0; // buttons + spacing
            height += _enableXButton ? 1 : 0;

            if (_surroundWithBorder)
            {
                width += 2;
                height += 2;
            }

            // --- create console ---
            var console = new ControlsConsole(width, height)
            {
                Parent = parent,
                Font = Game.Instance.Fonts["IBM_8x16"],
                FontSize = new(8, 16)
            };

            // --- adjust position so popup fits inside parent ---
            if (console.Parent is IScreenSurface screenSurface)
            {
                var parentWidth = screenSurface.Surface.ViewWidth;
                var parentHeight = screenSurface.Surface.ViewHeight;

                // How many times larger is the parent font compared to the "base font"?
                float scaleX = screenSurface.FontSize.X / 8f;  // the screenbuilder windows are always 8x16
                float scaleY = screenSurface.FontSize.Y / 16f;

                int px = _desiredPosition.X;
                int py = _desiredPosition.Y;

                // Push left by half width, up by height (scaled)
                px -= (int)(width / (2f * scaleX));
                py -= (int)(height / scaleY + 1);

                // Clamp in tile space (not pixels!)
                if (px < 0) px = 0;
                if (py < 0) py = 0;

                if (px + (int)(width / scaleX) > parentWidth)
                    px = parentWidth - (int)(width / scaleX);

                if (py + (int)(height / scaleY) > parentHeight)
                    py = parentHeight - (int)(height / scaleY);

                // Apply scale correction (for "8x16 issue")
                px = (int)(px * scaleX);
                py = (int)(py * scaleY);

                // Clamp at least to 0
                px = Math.Max(0, px);
                py = Math.Max(0, py);

                console.Position = new Point(px, py);
            }
            else
            {
                console.Position = _desiredPosition;
            }

            // --- draw border ---
            if (_surroundWithBorder)
            {
                console.Surface.DrawBox(
                    new Rectangle(0, 0, width, height),
                    ShapeParameters.CreateStyledBox(_lineThickness == LineThickness.Thin ? 
                        ICellSurface.ConnectedLineThin : ICellSurface.ConnectedLineThick, 
                        new ColoredGlyph(Color.White, Color.Black)));
            }

            int cursorY = _surroundWithBorder ? 1 : 0;

            // --- title ---
            if (!string.IsNullOrEmpty(_title))
            {
                console.Print((width - _title.Length) / 2, cursorY, _title, Color.Yellow);
                cursorY += 2;
            }

            // --- text ---
            foreach (var line in _texts)
            {
                console.Print(2, cursorY, line, Color.White);
                cursorY++;
            }

            cursorY++; // spacing before buttons

            // --- buttons ---
            foreach (var (label, onClick) in _buttons)
            {
                var button = new Button(label.Length + 4, 1)
                {
                    Text = label,
                    Position = new Point((width - (label.Length + 4)) / 2, cursorY)
                };
                button.Click += (s, e) =>
                {
                    onClick?.Invoke();
                    console.Parent?.Children.Remove(console);
                    console.IsEnabled = false;
                    onClose?.Invoke();
                };

                console.Controls.Add(button);
                cursorY += 2;
            }

            // --- X Button ---
            if (_enableXButton)
            {
                var xBtn = new Button(3, 1)
                {
                    Text = "X",
                    Position = new Point(width - 4, _surroundWithBorder ? 1 : 0)
                };
                xBtn.Click += (s, e) =>
                {
                    console.Parent?.Children.Remove(console);
                    console.IsEnabled = false;
                    onClose?.Invoke();
                };
                console.Controls.Add(xBtn);
            }

            configureScreen?.Invoke(console);
            return console;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">The parent screen object.</param>
        /// <param name="configureScreen">Executes to adapt settings on the screen.</param>
        public ControlsConsole BuildAndParent(IScreenObject parent, Action<ControlsConsole> configureScreen = null, Action onClose = null)
        {
            var build = Build(parent, configureScreen, onClose);
            parent.Children.Add(build);
            return build;
        }
    }
}
