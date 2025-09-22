using SadConsole;
using SadConsole.Components;
using SadConsole.Input;
using SadRogue.Primitives;
using System;

namespace Primora.Components
{
    /// <summary>
    /// Enables dragging a scrollable surface around by mouse.
    /// </summary>
    public class MouseDragViewPortCustom : MouseConsoleComponent
    {
        bool _isDragging = false;
        Point _grabWorldPosition = Point.Zero;
        Point _grabOriginalPosition = Point.Zero;
        bool _previousMouseExclusiveDrag;

        public enum MouseButtonType
        {
            Left,
            Right
        }

        /// <summary>
        /// When true, enables this component.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Which mouse button to use for registering the dragging. Default: Left
        /// </summary>
        public MouseButtonType MouseButtonForDragging { get; set; }

        /// <inheritdoc/>
        /// <exception cref="Exception">Raised when the host this component is added to doesn't implement <see cref="IScreenSurface"/>.</exception>
        public override void OnAdded(IScreenObject host)
        {
            if (host is not IScreenSurface)
                throw new Exception($"Component requires {nameof(IScreenSurface)}");
        }

        /// <inheritdoc/>
        public override void ProcessMouse(IScreenObject host, MouseScreenObjectState state, out bool handled)
        {
            var localHost = (IScreenSurface)host;
            handled = false;

            var mouseButtonState = MouseButtonForDragging == MouseButtonType.Left ?
                state.Mouse.LeftButtonDown : state.Mouse.RightButtonDown;
            var durationButtonState = MouseButtonForDragging == MouseButtonType.Left ?
                state.Mouse.LeftButtonDownDuration : state.Mouse.RightButtonDownDuration;

            // Disabled or surface can't even scroll
            if (!IsEnabled || !localHost.Surface.IsScrollable)
                return;

            // Stopped dragging
            else if (_isDragging && !mouseButtonState)
            {
                _isDragging = false;
                localHost.IsExclusiveMouse = _previousMouseExclusiveDrag;
                handled = true;
            }

            // Dragging
            else if (_isDragging)
            {
                localHost.Surface.ViewPosition = _grabOriginalPosition + (_grabWorldPosition - state.WorldCellPosition);
                handled = true;
            }

            // Not dragging, check if we should
            else if (state.IsOnScreenObject && !_isDragging && mouseButtonState && durationButtonState == TimeSpan.Zero)
            {
                _grabWorldPosition = state.WorldCellPosition;
                _grabOriginalPosition = localHost.Surface.ViewPosition;
                _isDragging = true;
                _previousMouseExclusiveDrag = localHost.IsExclusiveMouse;
                localHost.IsExclusiveMouse = true;
                handled = true;
            }
        }
    }
}