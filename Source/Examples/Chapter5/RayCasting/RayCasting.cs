#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class RayCasting : IDisposable
    {
        public RayCasting()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _sceneState.Camera.ZoomToTarget(1);

            _tile = new RayCastedTerrainTile(_window.Context,
                new TerrainTile(new GeodeticExtent(0, 0, 1, 1), 0, 1));

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\TerrainRendering\Figures\GPURayCasting.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _tile.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _tile.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (RayCasting example = new RayCasting())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly RayCastedTerrainTile _tile;
    }
}