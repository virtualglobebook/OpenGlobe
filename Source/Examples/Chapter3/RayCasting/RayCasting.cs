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

using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Chapter3
{
    sealed class RayCasting : IDisposable
    {
        public RayCasting()
        {
            Ellipsoid globeShape = Ellipsoid.UnitSphere;

            _window = Device.CreateWindow(800, 600, "Chapter 3:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);

            _window.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == KeyboardKey.P)
                {
                    CenterCameraOnPoint();
                }
                else if (e.Key == KeyboardKey.C)
                {
                    CenterCameraOnGlobeCenter();
                }
            };

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _globe = new RayCastedGlobe(_window.Context);
            _globe.Shape = globeShape;
            _globe.Texture = _texture;
            _globe.ShowWireframeBoundingBox = true;

            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);
            //CenterCameraOnPoint();
            //CenterCameraOnGlobeCenter();

            PersistentView.Execute(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.png";
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
            _globe.Render(_sceneState);
        }

        private void CenterCameraOnPoint()
        {
            _camera.ViewPoint(new Cartographic3D(Trig.ToRadians(-75.697), Trig.ToRadians(40.039), 0.0));
            _camera.Azimuth = 0.0;
            _camera.Elevation = Math.PI / 4.0;
            _camera.Range = _camera.Ellipsoid.MaximumRadius * 3.0;
        }

        private void CenterCameraOnGlobeCenter()
        {
            _camera.CenterPoint = Vector3D.Zero;
            _camera.FixedToLocalRotation = Matrix3d.Identity;
            _camera.Azimuth = 0.0;
            _camera.Elevation = 0.0;
            _camera.Range = _camera.Ellipsoid.MaximumRadius * 3.0;
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
            using (RayCasting example = new RayCasting())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly RayCastedGlobe _globe;
        private readonly Texture2D _texture;
    }
}