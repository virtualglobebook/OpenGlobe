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
    sealed class JitterFreeSceneGPURelativeToEye : IDisposable, IRenderable
    {
        public JitterFreeSceneGPURelativeToEye(Context context, double xTranslation, double triangleDelta)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.JitterFreeSceneGPURelativeToEye.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _cameraEyeHigh = (Uniform<Vector3S>)_sp.Uniforms["u_cameraEyeHigh"];
            _cameraEyeLow = (Uniform<Vector3S>)_sp.Uniforms["u_cameraEyeLow"];
            _modelViewPerspectiveMatrixRelativeToEye = (Uniform<Matrix4>)(_sp.Uniforms["u_modelViewPerspectiveMatrixRelativeToEye"]);
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            VertexAttributeFloatVector3 positionsHighAttribute = new VertexAttributeFloatVector3("positionHigh", 7);
            VertexAttributeFloatVector3 positionsLowAttribute = new VertexAttributeFloatVector3("positionLow", 7);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", 7);
            mesh.Attributes.Add(positionsHighAttribute);
            mesh.Attributes.Add(positionsLowAttribute);
            mesh.Attributes.Add(colorAttribute);

            Vector3D[] positions = new Vector3D[]
            {
                new Vector3D(xTranslation, triangleDelta + 0, 0),            // Red triangle
                new Vector3D(xTranslation, triangleDelta + 1000000, 0),
                new Vector3D(xTranslation, triangleDelta + 0, 1000000),
                new Vector3D(xTranslation, -triangleDelta - 0, 0),           // Green triangle
                new Vector3D(xTranslation, -triangleDelta - 0, 1000000),
                new Vector3D(xTranslation, -triangleDelta - 1000000, 0),
                new Vector3D(xTranslation, 0, 0)                             // Blue point
            };

            for (int i = 0; i < positions.Length; ++i)
            {
                Vector3S positionHigh;
                Vector3S positionLow;
                Vector3DToTwoVector3S(positions[i], out positionHigh, out positionLow);

                positionsHighAttribute.Values.Add(positionHigh);
                positionsLowAttribute.Values.Add(positionLow);
            }

            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.FromArgb(0, 255, 0));
            colorAttribute.AddColor(Color.FromArgb(0, 255, 0));
            colorAttribute.AddColor(Color.FromArgb(0, 255, 0));
            colorAttribute.AddColor(Color.Blue);

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawState = new DrawState(renderState, _sp, _va);
        }

        private static void DoubleToTwoFloats(double value, out float high, out float low)
        {
            if (value >= 0.0)
            {
                double doubleHigh = Math.Floor(value / 65536.0) * 65536.0;
                high = (float)doubleHigh;
                low = (float)(value - high);
            }
            else
            {
                double doubleHigh = Math.Floor(-value / 65536.0) * 65536.0;
                high = (float)-doubleHigh;
                low = (float)(value + high);
            }
        }

        private static void Vector3DToTwoVector3S(Vector3D value, out Vector3S high, out Vector3S low)
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

            high = new Vector3S(highX, highY, highZ);
            low = new Vector3S(lowX, lowY, lowZ);
        }

        private void Update(SceneState sceneState)
        {
            Vector3D eye = sceneState.Camera.Eye;

            if (_eye != eye)
            {
                _eye = eye;

                Vector3S eyeHigh;
                Vector3S eyeLow;
                Vector3DToTwoVector3S(eye, out eyeHigh, out eyeLow);
                _cameraEyeHigh.Value = eyeHigh;
                _cameraEyeLow.Value = eyeLow;

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
        private readonly Uniform<Vector3S> _cameraEyeHigh;
        private readonly Uniform<Vector3S> _cameraEyeLow;
        private readonly Uniform<Matrix4> _modelViewPerspectiveMatrixRelativeToEye;
        private readonly Uniform<float> _pointSize;
        private readonly DrawState _drawState;

        private Vector3D _eye;
    }
}