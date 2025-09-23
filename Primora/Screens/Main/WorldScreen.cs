using GoRogue.Pathing;
using Primora.Components;
using Primora.Core.Npcs.Actors;
using Primora.Core.Procedural.WorldBuilding;
using Primora.Extensions;
using Primora.Screens.Helpers;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Primora.Screens.Main
{
    internal class WorldScreen : LayeredScreenSurface
    {
        private readonly Dictionary<Keys, Direction> _moveDirections = new()
        {
            { Keys.Z, Direction.Up },
            { Keys.S, Direction.Down },
            { Keys.Q, Direction.Left },
            { Keys.D, Direction.Right },
        };

        /// <summary>
        /// Screen size for zone display
        /// </summary>
        public readonly (int width, int height) ZoneScreenSize;
        /// <summary>
        /// Screen size for worldmap display
        /// </summary>
        public readonly (int width, int height) WorldMapScreenSize;

        private readonly ScreenSurface _borderSurface;
        private readonly MouseDragViewPortCustom _mouseDragViewPortComponent;

        // Pathfinding
        private readonly FastAStar _worldMapPathfinder;
        private readonly FastAStar _zonePathfinder;
        private readonly ScreenSurface _pathfindingSurface;

        private Path _currentWorldMapPath, _currentZonePath;
        private Point? _currentHoverTile, _currentZoneHoverTile;
        private ControlsConsole _previousTravelScreen;

        public WorldScreen(ScreenSurface borderSurface,
            (int width, int height) zoneSize, 
            (int width, int height) worldMapSize) : 
            base(zoneSize.width, zoneSize.height, Constants.Zone.DefaultWidth, Constants.Zone.DefaultHeight)
        {
            _borderSurface = borderSurface;

            // Add surface child for pathfinding this resizes automatically based on parent
            _pathfindingSurface = new ScreenSurface(zoneSize.width, zoneSize.height, Constants.Zone.DefaultWidth, Constants.Zone.DefaultHeight)
            {
                UseKeyboard = false,
                UseMouse = false
            };
            Layers.Add(_pathfindingSurface.Surface);

            // Store screen sizes
            ZoneScreenSize = zoneSize;
            WorldMapScreenSize = worldMapSize;

            SadComponents.Add(_mouseDragViewPortComponent = new MouseDragViewPortCustom() 
            {  
                ApplyToHierarchy = false,
                MouseButtonForDragging = MouseDragViewPortCustom.MouseButtonType.Right 
            });

            _mouseDragViewPortComponent.IsEnabled = false;
            UseKeyboard = true;
            UseMouse = true;
            IsFocused = true;

            MouseMove += RenderingSurface_MouseMove;
            MouseExit += RenderingSurface_MouseExit;

            // Setup the pathfinder for the worldmap
            var worldmap = World.Instance.WorldMap;
            _worldMapPathfinder = new FastAStar(worldmap.Walkability, Distance.Manhattan, worldmap.Weights, 1);

            // Setup the pathfinder for the zones
            // Since zones change dynamically, we need to use lambda view
            var zoneWalkabilityView = new LambdaGridView<bool>(Constants.Zone.DefaultWidth, Constants.Zone.DefaultHeight, 
                (p) => World.Instance.CurrentZone.GetTileInfo(p).Walkable);
            var zoneWeightsView = new LambdaGridView<double>(Constants.Zone.DefaultWidth, Constants.Zone.DefaultHeight,
                (p) => World.Instance.CurrentZone.GetTileInfo(p).Weight);
            _zonePathfinder = new FastAStar(zoneWalkabilityView, Distance.Manhattan, zoneWeightsView, 1);
        }

        /// <summary>
        /// Updates the lowest weight for the worldmap pathfinder.
        /// </summary>
        /// <param name="value"></param>
        internal void UpdateLowestWeightForWorldMap(double value)
            => _worldMapPathfinder.MinimumWeight = value;

        /// <summary>
        /// Updates the lowest weight for the zone pathfinder.
        /// </summary>
        /// <param name="value"></param>
        internal void UpdateLowestWeightForZone(double value)
            => _zonePathfinder.MinimumWeight = value;

        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            VisualizeWorldMapPath(state);
            VisualizeZonePath(state);
            return base.ProcessMouse(state);
        }

        private void VisualizeWorldMapPath(MouseScreenObjectState state)
        {
            if (state.Mouse.LeftClicked && World.Instance.WorldMap.IsDisplayed)
            {
                // Check if in bounds
                if (state.SurfaceCellPosition.X >= ViewWidth || state.SurfaceCellPosition.Y >= ViewHeight) return;

                if (_currentWorldMapPath != null)
                {
                    foreach (var p in _currentWorldMapPath.Steps)
                    {
                        _pathfindingSurface.Clear(p.X, p.Y);
                    }
                }

                var startPos = Player.Instance.WorldPosition;
                var endPos = state.SurfaceCellPosition + ViewPosition;
                var path = _worldMapPathfinder.ShortestPath(startPos, endPos, false);
                if (path != null)
                {
                    var steps = path.Steps.Append(startPos).DefineLineGlyphsByPositions();
                    foreach (var (coordinate, glyph) in steps)
                    {
                        if (coordinate == startPos) continue; // We added start pos so the correct glyph is generated
                        var glyphValue = coordinate == endPos ? 255 : glyph;
                        _pathfindingSurface.SetGlyph(coordinate.X, coordinate.Y, glyphValue, Color.Lerp(Color.White, Color.Transparent, 0.1f));
                    }

                    if (_previousTravelScreen != null)
                    {
                        _previousTravelScreen.Parent.Children.Remove(_previousTravelScreen);
                        _previousTravelScreen.IsEnabled = false;
                    }

                    var travelDistanceInTurns = World.Instance.WorldMap.CalculateTravelDistanceInTurns(path.Steps, Constants.Worldmap.TurnsPerTile_FastTravel);
                    var foodConsumption = (int)Math.Ceiling(travelDistanceInTurns * Constants.Worldmap.FoodConsumptionPerTurn_FastTravel); // 1 food every 10 turns
                    
                    _previousTravelScreen = new ScreenBuilder()
                        .AddTitle("Fast Travel")
                        .EnableXButton()
                        .AddTextLine($"Fast traveling here will take {travelDistanceInTurns} turns.")
                        .AddTextLine($"You will consume {foodConsumption} food during your journey.")
                        .Position(endPos - ViewPosition)
                        .AddButton("Travel", () => Debug.WriteLine("Travel!"))
                        .SurroundWithBorder()
                        .BuildAndParent(this, onClose: () =>
                        {
                            if (_currentWorldMapPath != null)
                            {
                                foreach (var p in _currentWorldMapPath.Steps)
                                {
                                    _pathfindingSurface.Clear(p.X, p.Y);
                                }
                                _currentWorldMapPath = null;
                            }
                            _previousTravelScreen = null;
                        });
                }

                _currentWorldMapPath = path;
            }
        }

        private void VisualizeZonePath(MouseScreenObjectState state)
        {
            if (World.Instance.WorldMap.IsDisplayed) return;

            // Only visualize zone pathing if aiming
            if (!Player.Instance.IsAiming) return;

            // Don't recalculate path if we didn't leave the current hover tile
            if (_currentZoneHoverTile != null && _currentZoneHoverTile.Value == state.CellPosition + ViewPosition)
                return;

            // Check if in bounds
            if (state.SurfaceCellPosition.X >= ViewWidth || state.SurfaceCellPosition.Y >= ViewHeight) return;

            // Set new hover tile position
            _currentZoneHoverTile = state.SurfaceCellPosition + ViewPosition;

            if (_currentZonePath != null)
            {
                foreach (var p in _currentZonePath.Steps)
                {
                    _pathfindingSurface.Clear(p.X, p.Y);
                }
            }

            var startPos = Player.Instance.Position;
            var endPos = state.SurfaceCellPosition + ViewPosition;
            var path = _zonePathfinder.ShortestPath(startPos, endPos, false);
            if (path != null)
            {
                var steps = path.Steps.Append(startPos).DefineLineGlyphsByPositions();
                foreach (var (coordinate, glyph) in steps)
                {
                    if (coordinate == startPos) continue; // We added start pos so the correct glyph is generated
                    var glyphValue = coordinate == endPos ? 255 : glyph;
                    _pathfindingSurface.SetGlyph(coordinate.X, coordinate.Y, glyphValue, Color.Lerp(Color.White, Color.Transparent, 0.3f));
                }
            }

            _currentZonePath = path;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (Player.Instance.Location.IsDisplayed)
            {
                // Player movement allowed only if it's zone is displayed
                foreach (var key in _moveDirections)
                {
                    if (keyboard.IsKeyPressed(key.Key))
                    {
                        if (Player.Instance.Move(key.Value))
                        {
                            LogScreen.Add(LogEntry.New($"Player moved {key.Value.Type}"));
                            World.Instance.EndTurn();
                            return true;
                        }
                    }
                }
            }

            if (keyboard.IsKeyPressed(Keys.M))
            {
                var world = World.Instance;
                if (world.WorldMap.IsDisplayed)
                    world.ShowCurrentZone();
                else
                    world.ShowWorldMap();
            }

            return base.ProcessKeyboard(keyboard);
        }

        private void OnScreenSwap()
        {
            if (_previousTravelScreen != null)
            {
                _previousTravelScreen.Parent?.Children.Remove(_previousTravelScreen);
                _previousTravelScreen.IsEnabled = false;
                _previousTravelScreen = null;
            }

            // Resize all children too
            ResizeChildren(Surface.ViewWidth, Surface.ViewHeight, Surface.Width, Surface.Height, ViewPosition, FontSize);
        }

        public void AdaptScreenForWorldMap()
        {
            // Screen is already adapted for worldmap
            if (FontSize == new Point(Constants.General.FontGlyphWidth, Constants.General.FontGlyphHeight) && 
                _borderSurface.Width == WorldMapScreenSize.width + 2 && 
                _borderSurface.Height == WorldMapScreenSize.height + 2)
                return;

            _currentZonePath = null;

            // Resize also the border surface
            _borderSurface.Resize(
                WorldMapScreenSize.width + 2,
                WorldMapScreenSize.height + 2,
                true);
            _borderSurface.Surface.DrawBorder(LineThickness.Thin, "Worldmap", Color.Gray, Color.White, Color.Black);

            // Resize screensize for worldmap display
            Resize(
                WorldMapScreenSize.width,
                WorldMapScreenSize.height,
                Constants.Worldmap.DefaultWidth,
                Constants.Worldmap.DefaultHeight,
                false);

            // Reset fontsize to default
            FontSize = new Point(
                Constants.General.FontGlyphWidth,
                Constants.General.FontGlyphHeight);

            // Center view on player
            ViewPosition = Player.Instance.WorldPosition - new Point(
                ViewWidth / 2,
                ViewHeight / 2);

            // Handle all shared screen swap logic
            OnScreenSwap();

            _mouseDragViewPortComponent.IsEnabled = true;
        }

        public void AdaptScreenForZone()
        {
            // Screen is already adapted for zones
            if (FontSize == new Point(Constants.General.FontGlyphWidth *2, Constants.General.FontGlyphHeight * 2) && 
                _borderSurface.Width == ZoneScreenSize.width + 2 && 
                _borderSurface.Height == ZoneScreenSize.height + 2)
                return;

            _currentWorldMapPath = null;

            // Resize also the border surface
            _borderSurface.Resize(
                ZoneScreenSize.width + 2,
                ZoneScreenSize.height + 2,
                true);
            _borderSurface.Surface.DrawBorder(LineThickness.Thin, "World", Color.Gray, Color.White, Color.Black);

            // Resize back to the screensize for zone display
            Resize(
                ZoneScreenSize.width / 2,
                ZoneScreenSize.height / 2,
                Constants.Zone.DefaultWidth,
                Constants.Zone.DefaultHeight,
                false);

            // Increase the fontsize by two
            FontSize *= 2;

            // Center view on player
            ViewPosition = Player.Instance.Position - new Point(
                ViewWidth / 2,
                ViewHeight / 2);

            // Handle all shared screen swap logic
            OnScreenSwap();

            _mouseDragViewPortComponent.IsEnabled = false;
        }

        private void ResizeChildren(int viewWidth, int viewHeight, int totalWidth, int totalHeight, Point viewPosition, Point fontSize)
        {
            foreach (var child in Children)
            {
                if (child is ScreenSurface sf)
                {
                    sf.Resize(viewWidth, viewHeight, totalWidth, totalHeight, true);
                    sf.ViewPosition = viewPosition;
                    sf.FontSize = fontSize;
                }
            }
        }

        private void RenderingSurface_MouseExit(object sender, MouseScreenObjectState e)
        {
            // Clear previous
            var prev = _currentHoverTile;
            if (prev != null)
            {
                this.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
                _currentHoverTile = null;
            }
        }

        private void RenderingSurface_MouseMove(object sender, MouseScreenObjectState e)
        {
            var pos = e.SurfaceCellPosition + ViewPosition;

            // Clear previous
            var prev = _currentHoverTile;
            if (prev != null)
            {
                this.ClearDecorators(prev.Value.X, prev.Value.Y, 1);
                _currentHoverTile = null;
            }

            if (_currentHoverTile != null && pos == _currentHoverTile) return;

            if (pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height)
            {
                // Set current
                _currentHoverTile = pos;
                this.SetDecorator(pos.X, pos.Y, 1, new CellDecorator(Color.Black, 255, Mirror.None));
            }
        }
    }
}
