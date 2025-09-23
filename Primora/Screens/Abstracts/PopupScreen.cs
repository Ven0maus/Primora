using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;

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

        public void SetBasePosition(Point position, Point viewPosition)
        {
            _basePosition = position + viewPosition;
            Position = position;
        }

        public void EnableSync()
        {
            if (!_viewPortSyncEnabled && Parent != null)
            {
                var parent = (IScreenSurface)Parent;
                _previousViewPosition = parent.Surface.ViewPosition;
                parent.Surface.IsDirtyChanged += Surface_IsDirtyChanged;
                _viewPortSyncEnabled = true;
            }
        }

        public void DisableSync()
        {
            if (_viewPortSyncEnabled && Parent != null)
            {
                var parent = (IScreenSurface)Parent;
                parent.Surface.IsDirtyChanged -= Surface_IsDirtyChanged;
                _viewPortSyncEnabled = false;
            }
        }

        private void Surface_IsDirtyChanged(object sender, System.EventArgs e)
        {
            var surface = (ICellSurface)sender;
            if (_viewPortSyncEnabled && _previousViewPosition != surface.ViewPosition)
            {
                // View position was changed
                _previousViewPosition = surface.ViewPosition;

                // Adjust position of popupscreen
                Position = _basePosition - surface.ViewPosition;
            }
        }
    }
}
