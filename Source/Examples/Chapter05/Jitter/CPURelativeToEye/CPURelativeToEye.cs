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
    sealed class CPURelativeToEye : IDisposable, IRenderable
    {
        public CPURelativeToEye(Context context, Vector3D[] positions, byte[] colors)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.CPURelativeToEye.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _modelViewPerspectiveMatrixRelativeToEye = (Uniform<Matrix4F>)(_sp.Uniforms["u_modelViewPerspectiveMatrixRelativeToEye"]);
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            _positions = new Vector3D[positions.Length];
            positions.CopyTo(_positions, 0);
            _positionsRelativeToEye = new Vector3S[_positions.Length];
            _eye = Vector3D.Zero;

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

                Matrix4D m = sceneState.ModelViewMatrix;
                Matrix4D mv = new Matrix4D(
                    m.Column0Row0, m.Column1Row0, m.Column2Row0, 0.0,
                    m.Column0Row1, m.Column1Row1, m.Column2Row1, 0.0,
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, 0.0,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);

                _modelViewPerspectiveMatrixRelativeToEye.Value = 
                    (sceneState.PerspectiveMatrix * mv).ToMatrix4F();

                for (int i = 0; i < _positions.Length; ++i)
                {
                    _positionsRelativeToEye[i] = (_positions[i] - eye).ToVector3S();
                }

                _positionBuffer.CopyFromSystemMemory(_positionsRelativeToEye);
            }

            _pointSize.Value = (float)(8.0 * sceneState.HighResolutionSnapScale);
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
        private readonly Uniform<Matrix4F> _modelViewPerspectiveMatrixRelativeToEye;
        private readonly Uniform<float> _pointSize;
        private readonly DrawState _drawState;

        private readonly VertexBuffer _positionBuffer;
        private readonly Vector3D[] _positions;
        private readonly Vector3S[] _positionsRelativeToEye;
        private Vector3D _eye;
    }
}