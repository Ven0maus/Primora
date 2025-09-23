using Primora.Extensions;
using Primora.Screens.Abstracts;
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
        private bool _syncViewportPosition = false;
        private LineThickness _lineThickness;
        private Point _desiredPosition = Point.Zero;
        private readonly List<string> _texts = [];
        private readonly List<(string Label, Action OnClick)> _buttons = [];

        public ScreenBuilder AddTitle(string title)
        {
            _title = title;
            return this;
        }

        public ScreenBuilder AddTextLine(string text)
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
        public ScreenBuilder SetAnchorPosition(Point position)
        {
            _desiredPosition = position;
            return this;
        }

        /// <summary>
        /// Will keep the position synced with the parent's viewport movement.
        /// </summary>
        /// <returns></returns>
        internal ScreenBuilder SyncWithViewport()
        {
            _syncViewportPosition = true;
            return this;
        }

        /// <summary>
        /// Builds and parents the console to the given parent (this will also clamp positioning within parent if set).
        /// </summary>
        /// <param name="parent">The parent screen object.</param>
        /// <param name="configureScreen">Executes to adapt settings on the screen.</param>
        public PopupScreen BuildAndParent(IScreenSurface parent, Action<ControlsConsole> configureScreen = null, Action onClose = null)
        {
            var build = Build(parent, configureScreen, onClose);
            parent.Children.Add(build);
            return build;
        }

        /// <summary>
        /// Returns a screensurface build based on the configured settings.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configureScreen">Executes to adapt settings on the screen.</param>
        /// <returns></returns>
        public PopupScreen Build(IScreenSurface parent, Action<ControlsConsole> configureScreen = null, Action onClose = null)
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

            int buttonMargin = 1; // at least 1 cell from each border
            int availableWidth = width - (_surroundWithBorder ? 2 : 0) - (buttonMargin * 2);

            // Split buttons into rows
            var rows = new List<List<(string label, Action onClick)>>();
            var currentRow = new List<(string, Action)>();
            int rowWidth = 0;

            foreach (var (label, onClick) in _buttons)
            {
                int btnWidth = label.Length + 4; // button width
                if (rowWidth > 0 && rowWidth + 1 + btnWidth > availableWidth) // wrap if needed
                {
                    rows.Add(currentRow);
                    currentRow = new List<(string, Action)>();
                    rowWidth = 0;
                }

                currentRow.Add((label, onClick));
                rowWidth += (rowWidth > 0 ? 1 : 0) + btnWidth; // +1 for spacing
            }

            if (currentRow.Count > 0)
                rows.Add(currentRow);

            int height = 1; // title row
            height += _texts.Count;
            height += rows.Count * 2;
            height += _enableXButton ? 1 : 0;

            if (_surroundWithBorder)
            {
                width += 2;
                height += 2;
            }

            // --- create console ---
            var console = new PopupScreen(width, height, parent)
            {
                Font = Game.Instance.Fonts["IBM_8x16"],
                FontSize = new(8, 16)
            };
            console.SetBasePosition(_desiredPosition, parent.Surface.ViewPosition);

            // Viewport sync
            if (_syncViewportPosition)
                console.EnableSync();

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

            foreach (var row in rows)
            {
                int rowTotalWidth = row.Sum(b => b.label.Length + 4) + (row.Count - 1);

                // center inside the reduced available space, then add margin + border offset
                int cursorX = (availableWidth - rowTotalWidth) / 2
                              + (_surroundWithBorder ? 1 : 0)
                              + buttonMargin;

                foreach (var (label, onClick) in row)
                {
                    int btnWidth = label.Length + 4;

                    var button = new Button(btnWidth, 1)
                    {
                        Text = label,
                        Position = new Point(cursorX, cursorY)
                    };
                    button.Click += (s, e) =>
                    {
                        onClick?.Invoke();
                        console.DisableSync();
                        console.Parent?.Children.Remove(console);
                        console.IsEnabled = false;
                        onClose?.Invoke();     
                    };

                    console.Controls.Add(button);

                    cursorX += btnWidth + 1; // move right with spacing
                }

                cursorY += 2; // move down for next row
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
                    console.DisableSync();
                    console.Parent?.Children.Remove(console);
                    console.IsEnabled = false;
                    onClose?.Invoke();
                };
                console.Controls.Add(xBtn);
            }

            configureScreen?.Invoke(console);
            return console;
        }
    }
}
