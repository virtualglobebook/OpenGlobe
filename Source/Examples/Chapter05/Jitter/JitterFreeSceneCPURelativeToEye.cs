#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using OpenTK;

namespace OpenGlobe.Examples
{
    sealed class JitterFreeSceneCPURelativeToEye : IDisposable, IRenderable
    {
        public JitterFreeSceneCPURelativeToEye(Context context, double xTranslation, double triangleDelta)
        {
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  layout(location = og_colorVertexLocation) in vec3 color;
                  out vec3 fsColor;
                  uniform mat4 u_modelViewPerspectiveMatrixRelativeToEye;

                  void main()                     
                  {
                        gl_PointSize = 8.0;
                        gl_Position = u_modelViewPerspectiveMatrixRelativeToEye * position; 
                        fsColor = color;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 fsColor;
                  out vec3 fragmentColor;

                  void main()
                  {
                      fragmentColor = fsColor;
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            _modelViewPerspectiveMatrixRelativeToEye = (Uniform<Matrix4>)(_sp.Uniforms["u_modelViewPerspectiveMatrixRelativeToEye"]);

            ///////////////////////////////////////////////////////////////////

            _positions = new Vector3D[]
            {
                new Vector3D(xTranslation, triangleDelta + 0, 0),            // Red triangle
                new Vector3D(xTranslation, triangleDelta + 1000000, 0),
                new Vector3D(xTranslation, triangleDelta + 0, 1000000),
                new Vector3D(xTranslation, -triangleDelta - 0, 0),           // Green triangle
                new Vector3D(xTranslation, -triangleDelta - 0, 1000000),
                new Vector3D(xTranslation, -triangleDelta - 1000000, 0),
                new Vector3D(xTranslation, 0, 0),                            // Blue point
            };
            _positionsRelativeToEye = new Vector3S[_positions.Length];
            _eye = Vector3D.Zero;

            byte[] colors = new byte[]
            {
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 0, 255
            };

            //
            // _positionBuffer is dynamic, and is written to when the camera moves.
            //
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.DynamicDraw, _positionsRelativeToEye.Length * SizeInBytes<Vector3S>.Value);

            VertexBuffer colorBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, colors.Length);
            colorBuffer.CopyFromSystemMemory(colors);

            _va = context.CreateVertexArray();
            _va.Attributes[_sp.VertexAttributes["position"].Location] =
                new VertexBufferAttribute(_positionBuffer, ComponentDatatype.Float, 3);
            _va.Attributes[_sp.VertexAttributes["color"].Location] =
                new VertexBufferAttribute(colorBuffer, ComponentDatatype.UnsignedByte, 3, true, 0, 0);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawState = new DrawState(renderState, _sp, _va);
        }

        private void Update(SceneState sceneState)
        {
            Vector3D eye = sceneState.Camera.Eye;

            if (_eye != eye)
            {
                _eye = eye;

                Matrix4d m = sceneState.ModelMatrix;
                m.M41 += eye.X;
                m.M42 += eye.Y;
                m.M43 += eye.Z;

                //TODO:  A transpose is wrong somewhere.  The above should be this:
                //m.M14 += eye.X;
                //m.M24 += eye.Y;
                //m.M34 += eye.Z;

                _modelViewPerspectiveMatrixRelativeToEye.Value = Conversion.ToMatrix4(
                    m * sceneState.ViewMatrix * sceneState.PerspectiveMatrix);

                for (int i = 0; i < _positions.Length; ++i)
                {
                    _positionsRelativeToEye[i] = (_positions[i] - eye).ToVector3S();
                }

                _positionBuffer.CopyFromSystemMemory(_positionsRelativeToEye);
            }
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            Update(sceneState);

            context.Draw(PrimitiveType.Triangles, 0, 6, _drawState, sceneState);
            context.Draw(PrimitiveType.Points, 6, 1, _drawState, sceneState);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _va.Dispose();
            _sp.Dispose();
        }

        #endregion

        private readonly VertexArray _va;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Matrix4> _modelViewPerspectiveMatrixRelativeToEye;
        private readonly DrawState _drawState;

        private readonly VertexBuffer _positionBuffer;
        private readonly Vector3D[] _positions;
        private readonly Vector3S[] _positionsRelativeToEye;
        private Vector3D _eye;
    }
}