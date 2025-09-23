using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;
using System;

namespace Primora.Screens.Abstracts
{
    internal sealed class PopupScreen : ControlsConsole
    {
        private bool _viewPortSyncEnabled = false;
        private Point _previousViewPosition;
        private Point _basePosition;

        public PopupScreen(int width, int height, IScreenSurface parent) : base(width, height)
        {
            Parent = parent;
        }

        public void SetBasePosition(Point basePosition, Point viewPosition)
        {
            if (Parent is not IScreenSurface parentSurface)
            {
                Position = basePosition;
                return;
            }

            var parentFont = parentSurface.FontSize;
            var childFont = FontSize;

            // --- Step 1: compute scale factors ---
            float scaleX = (float)parentFont.X / childFont.X;
            float scaleY = (float)parentFont.Y / childFont.Y;

            // Width and height of popup in parent tile space
            int popupWidthInParentTiles = (int)Math.Ceiling(Width / scaleX);
            int popupHeightInParentTiles = (int)Math.Ceiling(Height / scaleY);

            int parentWidth = parentSurface.Surface.ViewWidth;
            int parentHeight = parentSurface.Surface.ViewHeight;

            // --- Step 2: ideal position (centered above tile) ---
            int px = basePosition.X - popupWidthInParentTiles / 2;
            int py = basePosition.Y - popupHeightInParentTiles - 1; // 1-tile gap above

            // --- Step 3: clamp to parent viewport ---
            px = Math.Clamp(px, 0, parentWidth - popupWidthInParentTiles);
            py = Math.Clamp(py, 0, parentHeight - popupHeightInParentTiles);

            // --- Step 4: convert to world pixels ---
            var worldPixels = new Point(
                (px + viewPosition.X) * parentFont.X,
                (py + viewPosition.Y) * parentFont.Y
            );

            // --- Step 5: store _basePosition in child tiles ---
            _basePosition = new Point(
                worldPixels.X / childFont.X,
                worldPixels.Y / childFont.Y
            );

            // --- Step 6: initial placement relative to viewport ---
            var scaledView = new Point(
                viewPosition.X * parentFont.X / childFont.X,
                viewPosition.Y * parentFont.Y / childFont.Y
            );

            Position = _basePosition - scaledView;
        }

        public void EnableSync()
        {
            if (!_viewPortSyncEnabled && Parent != null && Parent is IScreenSurface screenSurface)
            {
                _previousViewPosition = screenSurface.Surface.ViewPosition;
                screenSurface.Surface.IsDirtyChanged += Surface_IsDirtyChanged;
                _viewPortSyncEnabled = true;
            }
        }

        public void DisableSync()
        {
            if (_viewPortSyncEnabled && Parent != null && Parent is IScreenSurface screenSurface)
            {
                screenSurface.Surface.IsDirtyChanged -= Surface_IsDirtyChanged;
                _viewPortSyncEnabled = false;
            }
        }

        private void Surface_IsDirtyChanged(object sender, System.EventArgs e)
        {
            var surface = (ICellSurface)sender;
            if (!_viewPortSyncEnabled) return;
            if (_previousViewPosition == surface.ViewPosition) return;

            _previousViewPosition = surface.ViewPosition;

            var parentSurface = (IScreenSurface)Parent;
            var parentFont = parentSurface.FontSize; // e.g., 16x16
            var childFont = FontSize;                // e.g., 8x16

            // Convert parent view position to child tiles
            var viewPixels = new Point(
                surface.ViewPosition.X * parentFont.X,
                surface.ViewPosition.Y * parentFont.Y
            );

            var scaledView = new Point(
                viewPixels.X / childFont.X,
                viewPixels.Y / childFont.Y
            );

            // Adjust popup position in child tiles
            Position = _basePosition - scaledView;
        }
    }
}
