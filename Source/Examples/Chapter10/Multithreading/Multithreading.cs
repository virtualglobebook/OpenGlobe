//#define SINGLE_THREADED

#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    internal class ShapefileRequest
    {
        public ShapefileRequest(string filename, ShapefileAppearance appearance)
        {
            _filename = filename;
            _appearance = appearance;
        }

        public string Filename { get { return _filename; } }
        public ShapefileAppearance Appearance { get { return _appearance; } }

        private string _filename;
        private ShapefileAppearance _appearance;
    }

    internal class ShapefileWorker
    {
        public ShapefileWorker(Context context, Ellipsoid globeShape, MessageQueue doneQueue)
        {
            _context = context;
            _globeShape = globeShape;
            _doneQueue = doneQueue;
        }

        public void Process(object sender, MessageQueueEventArgs e)
        {
#if !SINGLE_THREADED
            _context.MakeCurrent();
#endif

            ShapefileRequest request = (ShapefileRequest)e.Message;
            ShapefileRenderer shapefile = new ShapefileRenderer(
                request.Filename, _context, _globeShape, request.Appearance);

#if !SINGLE_THREADED
            Fence fence = Device.CreateFence();
            while (fence.ClientWait(0) == ClientWaitResult.TimeoutExpired)
            {
                Thread.Sleep(10);   // Other work, etc.
            }
#endif

            _doneQueue.Post(shapefile);
        }

        private readonly Context _context;
        private readonly Ellipsoid _globeShape;
        private readonly MessageQueue _doneQueue;
    }
    
    sealed class Multithreading : IDisposable
    {
        public Multithreading()
        {
            Ellipsoid globeShape = Ellipsoid.ScaledWgs84;

            _workerWindow = Device.CreateWindow(1, 1);
            _window = Device.CreateWindow(800, 600, "Chapter 10:  Multithreading");
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
            _globe.UseAverageDepth = true;

            ///////////////////////////////////////////////////////////////////

            _doneQueue.MessageReceived += ProcessNewShapefile;

            _requestQueue.MessageReceived += new ShapefileWorker(_workerWindow.Context, globeShape, _doneQueue).Process;

            // 2ND_EDITION:  Draw order
            _requestQueue.Post(new ShapefileRequest("110m_admin_0_countries.shp", 
                new ShapefileAppearance()));
            _requestQueue.Post(new ShapefileRequest("110m_admin_1_states_provinces_lines_shp.shp", 
                new ShapefileAppearance()));
            _requestQueue.Post(new ShapefileRequest("airprtx020.shp", 
                new ShapefileAppearance() { Bitmap = new Bitmap("paper-plane--arrow.png") }));
            _requestQueue.Post(new ShapefileRequest("amtrakx020.shp", 
                new ShapefileAppearance() { Bitmap = new Bitmap("car-red.png") }));
            _requestQueue.Post(new ShapefileRequest("110m_populated_places_simple.shp", 
                new ShapefileAppearance() { Bitmap = new Bitmap("032.png") }));

#if SINGLE_THREADED
            _requestQueue.ProcessQueue();
#else
            _requestQueue.StartInAnotherThread();
#endif

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

            foreach (IRenderable shapefile in _shapefiles)
            {
                shapefile.Render(context, _sceneState);
            }
        }

        public void ProcessNewShapefile(object sender, MessageQueueEventArgs e)
        {
            _shapefiles.Add((IRenderable)e.Message);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (IRenderable shapefile in _shapefiles)
            {
                ((IDisposable)shapefile).Dispose();
            }

            _doneQueue.Dispose();
            _requestQueue.Dispose();
            _texture.Dispose();
            _globe.Dispose();
            _camera.Dispose();
            _window.Dispose();
            _workerWindow.Dispose();
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

        private readonly IList<IRenderable> _shapefiles = new List<IRenderable>();

        private readonly MessageQueue _requestQueue = new MessageQueue();
        private readonly MessageQueue _doneQueue = new MessageQueue();
        private readonly GraphicsWindow _workerWindow;
    }
}