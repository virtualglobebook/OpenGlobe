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
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples.Chapter3.NumberPrecision
{
    sealed class NumberPrecision : IDisposable
    {
        public NumberPrecision()
        {
            _window = Device.CreateWindow(1, 1, "Chapter 4:  Number Precision", WindowType.FullScreen);
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, new Ellipsoid(0.001, 0.001, 0.001),
                new Vector3D(_viewCenterX, 0.0, 0.0));
            _camera.MouseEnabled = false;
            CenterCameraOnPoint();
            _clearState = new ClearState();

            //
            // Point shader
            //
            string vs =
                 @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_modelViewOrthographicnMatrix;

                  void main()                     
                  {
                        gl_PointSize = 4.0;
                        gl_Position = og_modelViewOrthographicnMatrix * position;
                  }";
            string fs =
                @"#version 330
                 
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(1.0, 0.0, 0.0, 0.0);
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);
            VertexArray va = _window.Context.CreateVertexArray();

            //
            // Points
            //
            const int numPoints = 20;
            float[] points = new float[numPoints * 3];

            points[0] = 131071.00f; points[1] = 0.0f; points[2] = 0.0f;
            points[3] = 131071.01f; points[4] = 0.0f; points[5] = 0.0f;
            points[6] = 131071.02f; points[7] = 0.0f; points[8] = 0.0f;
            points[9] = 131071.03f; points[10] = 0.0f; points[11] = 0.0f;
            points[12] = 131071.04f; points[13] = 0.0f; points[14] = 0.0f;
            points[15] = 131071.05f; points[16] = 0.0f; points[17] = 0.0f;
            points[18] = 131071.06f; points[19] = 0.0f; points[20] = 0.0f;
            points[21] = 131071.07f; points[22] = 0.0f; points[23] = 0.0f;
            points[24] = 131071.08f; points[25] = 0.0f; points[26] = 0.0f;
            points[27] = 131071.09f; points[28] = 0.0f; points[29] = 0.0f;

            points[30] = 131072.00f; points[31] = 0.0f; points[32] = 0.0f;
            points[33] = 131072.01f; points[34] = 0.0f; points[35] = 0.0f;
            points[36] = 131072.02f; points[37] = 0.0f; points[38] = 0.0f;
            points[39] = 131072.03f; points[40] = 0.0f; points[41] = 0.0f;
            points[42] = 131072.04f; points[43] = 0.0f; points[44] = 0.0f;
            points[45] = 131072.05f; points[46] = 0.0f; points[47] = 0.0f;
            points[48] = 131072.06f; points[49] = 0.0f; points[50] = 0.0f;
            points[51] = 131072.07f; points[52] = 0.0f; points[53] = 0.0f;
            points[54] = 131072.08f; points[55] = 0.0f; points[56] = 0.0f;
            points[57] = 131072.09f; points[58] = 0.0f; points[59] = 0.0f;

            int sizeInBytes = points.Length * sizeof(float);
            _positionVertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticCopy, sizeInBytes);
            _positionVertexBuffer.CopyFromSystemMemory(points);

            AttachedVertexBuffer attachedPositionVertexBuffer = new AttachedVertexBuffer(
                _positionVertexBuffer, VertexAttributeComponentType.Float, 3);

            int location = sp.VertexAttributes["position"].Location;
            va.VertexBuffers[location] = attachedPositionVertexBuffer;

            _camera.Camera.PerspectiveNearPlaneDistance = 0.005;
            _camera.Camera.PerspectiveFarPlaneDistance = 5.0;

            //
            // Set the orthographic projection matrix to the identity matrix.
            //
            _camera.Camera.OrthographicNearPlaneDistance = -1;
            _camera.Camera.OrthographicFarPlaneDistance = 1;
            _camera.Camera.OrthographicLeft = -1;
            _camera.Camera.OrthographicRight = 1;
            _camera.Camera.OrthographicBottom = -1;
            _camera.Camera.OrthographicTop = 1;

            //
            // Enable point size.
            //
            RenderState renderState = new RenderState();
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawState = new DrawState(renderState, sp, va);

            Font font = new Font("Arial", 24);
            _bg = new BillboardCollection(_window.Context);
            _bg.Texture = Device.CreateTexture2D(Device.CreateBitmapFromText("131072.00f", font),
                TextureFormat.RedGreenBlueAlpha8, false);
            _bg.Add(new Billboard()
                {
                    Position = new Vector3D(_viewCenterX, 0.0, 0.0),
                    Color = Color.Black
                });
            font.Dispose();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;

            //
            // Clear
            //
            context.Clear(_clearState);

            //
            // Points
            //
            context.Draw(PrimitiveType.Points, _drawState, _sceneState);
            
            //
            // Text
            //
            _bg.Render(context, _sceneState);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.Escape)
            {
                Environment.Exit(0);
            }
        }

        private void CenterCameraOnPoint()
        {
            _sceneState.Camera.Target = new Vector3D(_viewCenterX, 0.0, 0.0);
            _sceneState.Camera.Up = new Vector3D(0.0, 1.0, 0.0);
            _sceneState.Camera.Eye = new Vector3D(_viewCenterX, 0.0, 0.5);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
            _bg.Texture.Dispose();
            _bg.Dispose();
            _positionVertexBuffer.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (NumberPrecision example = new NumberPrecision())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly DrawState _drawState;
        private readonly BillboardCollection _bg;
        private VertexBuffer _positionVertexBuffer;
        private double _viewCenterX = 131071.50;
    }
}