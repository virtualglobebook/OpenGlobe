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
using MiniGlobe.Core;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Examples.Chapter7
{
    sealed class VectorData : IDisposable
    {
        public VectorData()
        {
            Ellipsoid globeShape = Ellipsoid.UnitSphere;

            _window = Device.CreateWindow(800, 600, "Chapter 7:  Vector Data");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);

            _frameBuffer = _window.Context.CreateFrameBuffer();
            _quad = new DayNightViewportQuad(_window.Context);

            _globe = new DayNightGlobe(_window.Context);
            _globe.Shape = globeShape;
            _globe.UseAverageDepth = true;
            _globe.DayTexture = Device.CreateTexture2D(new Bitmap("NE2_50M_SR_W_4096.jpg"), TextureFormat.RedGreenBlue8, false);
            _globe.NightTexture = Device.CreateTexture2D(new Bitmap("land_ocean_ice_lights_2048.jpg"), TextureFormat.RedGreenBlue8, false);

            _countries = new ShapefileGraphics(_window.Context, globeShape, "110m_admin_0_countries.shp");
            _countries.PolylineWidth = 1;
            _countries.PolylineOutlineWidth = 1;
            _states = new ShapefileGraphics(_window.Context, globeShape, "110m_admin_1_states_provinces_lines_shp.shp");
            _states.PolylineWidth = 1;
            _states.PolylineOutlineWidth = 1;
            _rivers = new ShapefileGraphics(_window.Context, globeShape, "50m-rivers-lake-centerlines.shp", Color.LightBlue, Color.LightBlue);
            _rivers.PolylineWidth = 1;
            _rivers.PolylineOutlineWidth = 0;

            _sceneState.DiffuseIntensity = 0.5f;
            _sceneState.SpecularIntensity = 0.1f;
            _sceneState.AmbientIntensity = 0.4f;
            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            PersistentView.Execute(@"E:\Test.xml", _window, _sceneState.Camera);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;

            UpdateFrameBufferAttachments();
        }

        private void UpdateFrameBufferAttachments()
        {
            DisposeFrameBufferAttachments();
            _dayTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlueAlpha8, false));
            _nightTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlueAlpha8, false));
            _blendTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.Red32f, false));
            _depthTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.Depth24, false));
            
            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("dayColor")] = _dayTexture;
            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("nightColor")] = _nightTexture;
            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("blendAlpha")] = _blendTexture;
            _frameBuffer.DepthAttachment = _depthTexture;

            _quad.DayTexture = _dayTexture;
            _quad.NightTexture = _nightTexture;
            _quad.BlendTexture = _blendTexture;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;

            //
            // Render to frame buffer
            //
            context.Bind(_frameBuffer);
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.Black, 1, 0);
            _globe.Render(_sceneState);

            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("nightColor")] = null;
            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("blendAlpha")] = null;
            
            _countries.Render(_sceneState);
            _states.Render(_sceneState);
            _rivers.Render(_sceneState);

            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("nightColor")] = _nightTexture;
            _frameBuffer.ColorAttachments[_globe.FragmentOutputs("blendAlpha")] = _blendTexture;

            //
            // Render viewport quad to show contents of frame buffer's color buffer
            //
            context.Bind(null as FrameBuffer);
            _quad.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            DisposeFrameBufferAttachments();
            _frameBuffer.Dispose();
            _quad.Dispose();
            _globe.DayTexture.Dispose();
            _globe.NightTexture.Dispose();
            _globe.Dispose();
            _countries.Dispose();
            _states.Dispose();
            _rivers.Dispose();
            _window.Dispose();
        }

        #endregion

        private void DisposeFrameBufferAttachments()
        {
            if (_dayTexture != null)
            {
                _dayTexture.Dispose();
                _dayTexture = null;
            }

            if (_nightTexture != null)
            {
                _nightTexture.Dispose();
                _nightTexture = null;
            }

            if (_blendTexture != null)
            {
                _blendTexture.Dispose();
                _blendTexture = null;
            }

            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (VectorData example = new VectorData())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;

        private Texture2D _dayTexture;
        private Texture2D _nightTexture;
        private Texture2D _blendTexture;
        private Texture2D _depthTexture;
        private readonly FrameBuffer _frameBuffer;
        private readonly DayNightViewportQuad _quad;

        private readonly DayNightGlobe _globe;
        private readonly ShapefileGraphics _countries;
        private readonly ShapefileGraphics _states;
        private readonly ShapefileGraphics _rivers;
    }
}