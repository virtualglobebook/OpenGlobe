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
    sealed class RelativeToWorld : IDisposable, IRenderable
    {
        public RelativeToWorld(Context context, Vector3D[] positions, byte[] colors)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.RelativeToWorld.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", positions.Length);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", positions.Length);
            mesh.Attributes.Add(positionsAttribute);
            mesh.Attributes.Add(colorAttribute);

            for (int i = 0; i < positions.Length; ++i)
            {
                positionsAttribute.Values.Add(positions[i]);
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

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            _pointSize.Value = (float)(8.0 * sceneState.HighResolutionSnapScale);
            
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
        private readonly Uniform<float> _pointSize;
        private readonly DrawState _drawState;
    }
}