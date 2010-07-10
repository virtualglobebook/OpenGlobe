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

using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Core.Tessellation;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples.Chapter9
{
    internal class ShapefileWorker
    {
        public ShapefileWorker(MessageQueue doneQueue)
        {
            _doneQueue = doneQueue;
        }

        public void Process(object sender, MessageQueueEventArgs e)
        {
            int value = (int)e.Message;
            value *= value;

            _doneQueue.Post(value);
        }

        private readonly MessageQueue _doneQueue;
    }
    
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

            ///////////////////////////////////////////////////////////////////
            
            _doneQueue = new MessageQueue();
            _doneQueue.MessageReceived += ProcessNewShapefile;

            _worker = new ShapefileWorker(_doneQueue);

            _requestQueue = new MessageQueue();
            _requestQueue.MessageReceived += _worker.Process;
            _requestQueue.StartInAnotherThread();

            ///////////////////////////////////////////////////////////////////

            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            _doneQueue.ProcessQueue();

            Context context = _window.Context;
            context.Clear(_clearState);
            _globe.Render(context, _sceneState);

            if (_haveRequest)
            {
                int value = 5;
                _requestQueue.Post(value);

                _haveRequest = false;
            }
        }

        public void ProcessNewShapefile(object sender, MessageQueueEventArgs e)
        {
            int squaredValue = (int)e.Message;
            System.Diagnostics.Debug.Assert(squaredValue == 25);
        }

        private bool _haveRequest = true;   // TODO:  remove temp

        #region IDisposable Members

        public void Dispose()
        {
            _doneQueue.Dispose();
            _requestQueue.Dispose();
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

        private readonly MessageQueue _requestQueue;
        private readonly MessageQueue _doneQueue;
        private readonly ShapefileWorker _worker;
    }
}