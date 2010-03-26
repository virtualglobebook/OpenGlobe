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
            _sceneState.Camera.PerspectiveFarPlaneDistance = 128;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);

            ///////////////////////////////////////////////////////////////////
            GeodeticExtent extent = new GeodeticExtent(0, 0, 6, 6); // TODO:  Must match texture dimensions for now
            Size size = new Size(6, 6);
            float[] heights = new float[]
            {
                0f,    0.25f, 0.35f, 0.4f,  0.2f,  0.1f,
                0.25f, 0.35f, 0.35f, 0.45f, 0.35f, 0.2f,
                0.35f, 0.35f, 0.35f, 0.45f, 0.5f,  0.4f,
                0.45f, 0.5f,  0.5f,  0.5f,  0.5f,  0.4f,
                0.25f, 0.4f,  0.3f,  0.5f,  0.4f,  0.2f,
                0.15f, 0.3f,  0.2f,  0.1f,  0f,    0f
            };

            _tile = new RayCastedTerrainTile(_window.Context,
                new TerrainTile(extent, size, heights, 0, 0.5f));

            ///////////////////////////////////////////////////////////////////

            _axes = new Axes(_window.Context);
            _axes.Length = 25;
            _axes.Width = 3;
            _sceneState.Camera.ZoomToTarget(15);

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
            _axes.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _tile.Dispose();
            _axes.Dispose();
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
        private readonly Axes _axes;
    }
}