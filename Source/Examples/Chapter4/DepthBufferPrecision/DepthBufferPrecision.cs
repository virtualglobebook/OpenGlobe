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
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class DepthBufferPrecision : IDisposable
    {
        public DepthBufferPrecision()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 4:  Depth Buffer Precision");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();

            _sceneState.Camera.PerspectiveFarPlaneDistance = 1;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;

            ///////////////////////////////////////////////////////////////////

            Ellipsoid globeShape = Ellipsoid.UnitSphere;
            _globe = new TessellatedGlobe(_window.Context);
            _globe.Shape = globeShape;
            _globe.Texture = Device.CreateTexture2D(new Bitmap("MapperWDB.jpg"), TextureFormat.RedGreenBlue8, false);

            _viewportQuad = new ViewportQuad(_window.Context);
            _viewportQuad.Texture = Device.CreateTexture2D(new Bitmap("MapperWDB.jpg"), TextureFormat.RedGreenBlue8, false);

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);
            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\DepthBufferPrecision\Figures\DepthBufferPrecision.png";
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

            _viewportQuad.Render(_sceneState);
            _globe.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _globe.Texture.Dispose();
            _globe.Dispose();
            _viewportQuad.Texture.Dispose();
            _viewportQuad.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (DepthBufferPrecision example = new DepthBufferPrecision())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TessellatedGlobe _globe;
        private readonly ViewportQuad _viewportQuad;
    }
}