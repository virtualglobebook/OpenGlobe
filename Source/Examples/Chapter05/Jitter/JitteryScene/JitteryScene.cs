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
    sealed class JitteryScene : IDisposable, IRenderable
    {
        public JitteryScene(Context context, double xTranslation, double triangleDelta)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.JitteryScene.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", 7);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", 7);
            mesh.Attributes.Add(positionsAttribute);
            mesh.Attributes.Add(colorAttribute);

            IList<Vector3D> positions = positionsAttribute.Values;
            positions.Add(new Vector3D(xTranslation, triangleDelta + 0, 0));            // Red triangle
            positions.Add(new Vector3D(xTranslation, triangleDelta + 1000000, 0));
            positions.Add(new Vector3D(xTranslation, triangleDelta + 0, 1000000));
            positions.Add(new Vector3D(xTranslation, -triangleDelta - 0, 0));           // Green triangle
            positions.Add(new Vector3D(xTranslation, -triangleDelta - 0, 1000000));
            positions.Add(new Vector3D(xTranslation, -triangleDelta - 1000000, 0));
            positions.Add(new Vector3D(xTranslation, 0, 0));                            // Blue point
            
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