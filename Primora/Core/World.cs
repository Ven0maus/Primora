using Primora.Extensions;
using SadConsole;
using System;
using System.Collections.Generic;

namespace Primora.Core
{
    internal class World
    {
        private readonly Dictionary<Layer, ScreenSurface> _surfaces = [];
        public IReadOnlyDictionary<Layer, ScreenSurface> Surfaces => _surfaces;

        public readonly int Width, Height;
        
        public World(int width, int height, IFont.Sizes fontSize)
        {
            // Initialization
            InitSurfaces(width, height, fontSize);

            // Set width and height based on resized surfaces
            Width = Surfaces[Layer.Ground].Width;
            Height = Surfaces[Layer.Ground].Height;

            // Testing
            Surfaces[Layer.Ground].FillWithRandomGarbage(255);
        }

        private void InitSurfaces(int width, int height, IFont.Sizes fontSize)
        {
            var layers = Enum.GetValues<Layer>();
            foreach (var layer in layers)
            {
                var surface = new ScreenSurface(width, height);
                surface.ResizeToFitFontSize(fontSize);
                _surfaces[layer] = surface;
            }
        }

        public enum Layer
        {
            // Number value is the Z layer for rendering
            Ground = 0,
            Object = 1
        }
    }
}
