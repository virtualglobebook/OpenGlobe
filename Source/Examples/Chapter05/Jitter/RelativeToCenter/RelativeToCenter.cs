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

namespace OpenGlobe.Examples
{
    sealed class RelativeToCenter : IDisposable, IRenderable
    {
        public RelativeToCenter(Context context, Vector3D[] positions, byte[] colors)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.RelativeToCenter.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _modelViewPerspectiveMatrixRelativeToCenter = (Uniform<Matrix4S>)(_sp.Uniforms["u_modelViewPerspectiveMatrixRelativeToCenter"]);
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            VertexAttributeFloatVector3 positionsAttribute = new VertexAttributeFloatVector3("position", positions.Length);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", positions.Length);
            mesh.Attributes.Add(positionsAttribute);
            mesh.Attributes.Add(colorAttribute);

            _center = new AxisAlignedBoundingBox(positions).Center;
            for (int i = 0; i < positions.Length; ++i)
            {
                positionsAttribute.Values.Add((positions[i] - _center).ToVector3S());
            }

            for (int i = 0; i < colors.Length; ++i)
            {
                colorAttribute.Values.Add(colors[i]);
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);

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

            //
            // Eye change means view matrix changed, so recompute
            // model-view-perspective relative to center.
            //
            if (_eye != eye)
            {
                _eye = eye;

                Matrix4D m = sceneState.ModelViewMatrix;
                Vector4D centerEye = m * new Vector4D(_center, 1.0);
                Matrix4D mv = new Matrix4D(
                    m.Column0Row0, m.Column1Row0, m.Column2Row0, centerEye.X,
                    m.Column0Row1, m.Column1Row1, m.Column2Row1, centerEye.Y,
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, centerEye.Z,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);

                _modelViewPerspectiveMatrixRelativeToCenter.Value =
                    (sceneState.PerspectiveMatrix * mv).ToMatrix4S();
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
        private readonly Uniform<Matrix4S> _modelViewPerspectiveMatrixRelativeToCenter;
        private readonly Uniform<float> _pointSize;
        private readonly DrawState _drawState;

        private readonly Vector3D _center;
        private Vector3D _eye;
    }
}