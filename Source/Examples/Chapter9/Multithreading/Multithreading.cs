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

using OpenGlobe.Core.Geometry;
using OpenGlobe.Core.Tessellation;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples.Chapter9
{
    sealed class Multithreading : IDisposable
    {
        public Multithreading()
        {
            Ellipsoid globeShape = Ellipsoid.ScaledWgs84;
                        
            _window = Device.CreateWindow(800, 600, "Chapter 9:  Multithreading");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);
            _clearState = new ClearState();

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _globe = new RayCastedGlobe(_window.Context);
            _globe.Shape = globeShape;
            _globe.Texture = _texture;

            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);
            _globe.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _globe.Dispose();
            _camera.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (Multithreading example = new Multithreading())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly RayCastedGlobe _globe;
        private readonly Texture2D _texture;

    }
}