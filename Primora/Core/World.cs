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
            Width = width;
            Height = height;

            // Initialization
            InitSurfaces(fontSize);

            // Testing
            Surfaces[Layer.Ground].FillWithRandomGarbage(255);
        }

        private void InitSurfaces(IFont.Sizes fontSize)
        {
            var layers = Enum.GetValues<Layer>();
            foreach (var layer in layers)
            {
                var surface = new ScreenSurface(Width, Height);
                surface.ResizeToFitFontSize(fontSize);
                _surfaces[layer] = surface;
            }
        }

        public enum Layer
        {
            Ground,
            Object,
            Npc
        }
    }
}
