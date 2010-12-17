#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    /// <summary>
    /// The same as GPURelativeToEyeDSFUN90, except uses
    /// precision LOD, so only 32-bit positions are required until the 
    /// viewer is zoomed in close.
    /// </summary>
    sealed class SceneGPURelativeToEyeLOD : IDisposable, IRenderable
    {
        public SceneGPURelativeToEyeLOD(Context context, Vector3D[] positions, byte[] colors)
        {
            _spHigh = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.GPURelativeToEyeLOD.Shaders.HighPrecisionVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _cameraEyeHigh = (Uniform<Vector3F>)_spHigh.Uniforms["u_cameraEyeHigh"];
            _cameraEyeLow = (Uniform<Vector3F>)_spHigh.Uniforms["u_cameraEyeLow"];
            _modelViewPerspectiveMatrixRelativeToEye = (Uniform<Matrix4F>)(_spHigh.Uniforms["u_modelViewPerspectiveMatrixRelativeToEye"]);
            _pointSizeHigh = (Uniform<float>)_spHigh.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            Vector3F[] positionsHigh = new Vector3F[positions.Length];
            Vector3F[] positionsLow = new Vector3F[positions.Length];

            for (int i = 0; i < positions.Length; ++i)
            {
                Vector3DToTwoVector3F(positions[i], out positionsHigh[i], out positionsLow[i]);
            }
            _center = positions[6];

            _positionBufferHigh = Device.CreateVertexBuffer(BufferHint.StaticDraw, ArraySizeInBytes.Size(positions));
            _positionBufferLow = Device.CreateVertexBuffer(BufferHint.StaticDraw, ArraySizeInBytes.Size(positions));
            _colorBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, colors.Length);

            _positionBufferHigh.CopyFromSystemMemory(positionsHigh);
            _positionBufferLow.CopyFromSystemMemory(positionsLow);
            _colorBuffer.CopyFromSystemMemory(colors);

            _vaHigh = context.CreateVertexArray();
            _vaHigh.Attributes[_spHigh.VertexAttributes["positionHigh"].Location] =
                new VertexBufferAttribute(_positionBufferHigh, ComponentDatatype.Float, 3);
            _vaHigh.Attributes[_spHigh.VertexAttributes["positionLow"].Location] =
                new VertexBufferAttribute(_positionBufferLow, ComponentDatatype.Float, 3);
            _vaHigh.Attributes[_spHigh.VertexAttributes["color"].Location] =
                new VertexBufferAttribute(_colorBuffer, ComponentDatatype.UnsignedByte, 3, true, 0, 0);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawStateHigh = new DrawState(renderState, _spHigh, _vaHigh);

            ///////////////////////////////////////////////////////////////////
            // Low Precision

            _spLow = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.GPURelativeToEyeLOD.Shaders.LowPrecisionVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _pointSizeLow = (Uniform<float>)_spLow.Uniforms["u_pointSize"];

            _vaLow = context.CreateVertexArray();
            _vaLow.Attributes[_spLow.VertexAttributes["position"].Location] =
                new VertexBufferAttribute(_positionBufferHigh, ComponentDatatype.Float, 3);
            _vaLow.Attributes[_spLow.VertexAttributes["color"].Location] =
                new VertexBufferAttribute(_colorBuffer, ComponentDatatype.UnsignedByte, 3, true, 0, 0);

            _drawStateLow = new DrawState(renderState, _spLow, _vaLow);
        }

        private static void DoubleToTwoFloats(double value, out float high, out float low)
        {
            high = (float)value;
            low = (float)(value - high);
        }

        private static void Vector3DToTwoVector3F(Vector3D value, out Vector3F high, out Vector3F low)
        {
            float highX;
            float highY;
            float highZ;

            float lowX;
            float lowY;
            float lowZ;

            DoubleToTwoFloats(value.X, out highX, out lowX);
            DoubleToTwoFloats(value.Y, out highY, out lowY);
            DoubleToTwoFloats(value.Z, out highZ, out lowZ);

            high = new Vector3F(highX, highY, highZ);
            low = new Vector3F(lowX, lowY, lowZ);
        }

        private void UpdateHigh(SceneState sceneState)
        {
            Vector3D eye = sceneState.Camera.Eye;

            if (_eye != eye)
            {
                _eye = eye;

                Vector3F eyeHigh;
                Vector3F eyeLow;
                Vector3DToTwoVector3F(eye, out eyeHigh, out eyeLow);
                _cameraEyeHigh.Value = eyeHigh;
                _cameraEyeLow.Value = eyeLow;

                Matrix4D m = sceneState.ModelViewMatrix;
                Matrix4D mv = new Matrix4D(
                    m.Column0Row0, m.Column1Row0, m.Column2Row0, 0.0,
                    m.Column0Row1, m.Column1Row1, m.Column2Row1, 0.0,
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, 0.0,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);

                _modelViewPerspectiveMatrixRelativeToEye.Value =
                    (sceneState.PerspectiveMatrix * mv).ToMatrix4F();
            }

            _pointSizeHigh.Value = (float)(8.0 * sceneState.HighResolutionSnapScale);
        }

        private void UpdateLow(SceneState sceneState)
        {
            _pointSizeLow.Value = (float)(8.0 * sceneState.HighResolutionSnapScale);
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            //
            // Select LOD
            //
            DrawState ds = null;

            if ((sceneState.Camera.Eye - _center).Magnitude < 100.0)
            {
                ds = _drawStateHigh;
                UpdateHigh(sceneState);
            }
            else
            {
                ds = _drawStateLow;
                UpdateLow(sceneState);
            }

            context.Draw(PrimitiveType.Triangles, 0, 6, ds, sceneState);
            context.Draw(PrimitiveType.Points, 6, 1, ds, sceneState);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _positionBufferHigh.Dispose();
            _positionBufferLow.Dispose();
            _colorBuffer.Dispose();

            _vaHigh.Dispose();
            _spHigh.Dispose();

            _vaLow.Dispose();
            _spLow.Dispose();
        }

        #endregion

        private readonly VertexBuffer _positionBufferHigh;
        private readonly VertexBuffer _positionBufferLow;
        private readonly VertexBuffer _colorBuffer;

        private readonly VertexArray _vaHigh;
        private readonly ShaderProgram _spHigh;
        private readonly Uniform<Vector3F> _cameraEyeHigh;
        private readonly Uniform<Vector3F> _cameraEyeLow;
        private readonly Uniform<Matrix4F> _modelViewPerspectiveMatrixRelativeToEye;
        private readonly Uniform<float> _pointSizeHigh;
        private readonly DrawState _drawStateHigh;

        ///////////////////////////////////////////////////////////////////////

        private readonly VertexArray _vaLow;
        private readonly ShaderProgram _spLow;
        private readonly Uniform<float> _pointSizeLow;
        private readonly DrawState _drawStateLow;

        ///////////////////////////////////////////////////////////////////////

        private readonly Vector3D _center;
        private Vector3D _eye;
    }
}