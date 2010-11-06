#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class Jitter : IDisposable
    {
        public Jitter()
        {
            Ellipsoid globeShape = Ellipsoid.Wgs84;

            _window = Device.CreateWindow(800, 600, "Chapter 5:  Jitter");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _clearState = new ClearState();

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_modelViewPerspectiveMatrix;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 
                  }";

            string fs =
                @"#version 330
                 
                  out vec3 fragmentColor;
                  uniform vec3 u_color;

                  void main()
                  {
                      fragmentColor = u_color;
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);
            _color = (Uniform<Vector3S>)sp.Uniforms["u_color"];

            ///////////////////////////////////////////////////////////////////
            
            Mesh mesh = new Mesh();

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", 3);
            mesh.Attributes.Add(positionsAttribute);

            double delta = 1;

            IList<Vector3D> positions = positionsAttribute.Values;
            positions.Add(new Vector3D(globeShape.Radii.X, delta + 0, 0));
            positions.Add(new Vector3D(globeShape.Radii.X, delta + 1000000, 0));
            positions.Add(new Vector3D(globeShape.Radii.X, delta + 0, 1000000));

            positions.Add(new Vector3D(globeShape.Radii.X, -delta - 0, 0));
            positions.Add(new Vector3D(globeShape.Radii.X, -delta - 0, 1000000));
            positions.Add(new Vector3D(globeShape.Radii.X, -delta - 1000000, 0));

            VertexArray va = _window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;

            _drawState = new DrawState(renderState, sp, va);

            ///////////////////////////////////////////////////////////////////

            Vector3D localOrigin = Vector3D.UnitX * globeShape.Radii.X;

            _billboards = new BillboardCollection(_window.Context);
            _billboards.DepthTestEnabled = false;
            _billboards.DepthWrite = false;
            _billboards.Texture = Device.CreateTexture2D(Device.CreateBitmapFromPoint(5), TextureFormat.RedGreenBlueAlpha8, false);

            Billboard billboard = new Billboard();
            billboard.Position = localOrigin;
            billboard.Color = Color.Blue;
            _billboards.Add(billboard);
           
            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Black;

            ///////////////////////////////////////////////////////////////////

            Camera camera = _sceneState.Camera;
            camera.PerspectiveNearPlaneDistance = 0.1;
            camera.PerspectiveFarPlaneDistance = 1000000;
            camera.Target = localOrigin;
            camera.Eye = localOrigin * 1.1;

            _camera = new CameraLookAtPoint(camera, _window, globeShape);
            _camera.Range = (camera.Eye - camera.Target).Magnitude;
            _camera.MinimumZoomRate = 1;
            _camera.MaximumZoomRate = Double.MaxValue;
            _camera.ZoomFactor = 10;
            _camera.ZoomRateRangeAdjustment = 0;
        }

        private void UpdateHUD()
        {
            string text = "Distance: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _camera.Range);

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            UpdateHUD();

            Context context = _window.Context;
            context.Clear(_clearState);

            _color.Value = new Vector3S(1, 0, 0);
            context.Draw(PrimitiveType.Triangles, 0, 3, _drawState, _sceneState);
            _color.Value = new Vector3S(0, 1, 0);
            context.Draw(PrimitiveType.Triangles, 3, 3, _drawState, _sceneState);

            _billboards.Render(context, _sceneState);
            _hud.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.VertexArray.Dispose();
            _drawState.ShaderProgram.Dispose();
            _billboards.Texture.Dispose();
            _billboards.Dispose();
            _camera.Dispose();
            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (Jitter example = new Jitter())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly ClearState _clearState;
        private readonly DrawState _drawState;
        private readonly Uniform<Vector3S> _color;

        private readonly BillboardCollection _billboards;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private readonly CameraLookAtPoint _camera;
    }
}