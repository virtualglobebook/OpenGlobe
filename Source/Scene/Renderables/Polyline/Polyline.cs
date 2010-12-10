#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class Polyline : IDisposable
    {
        public Polyline()
        {
            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.Enabled = false;

            Width = 1;
        }

        public void Set(Context context, Mesh mesh)
        {
            Verify.ThrowIfNull(context);

            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (mesh.PrimitiveType != PrimitiveType.Lines &&
                mesh.PrimitiveType != PrimitiveType.LineLoop &&
                mesh.PrimitiveType != PrimitiveType.LineStrip)
            {
                throw new ArgumentException("mesh.PrimitiveType must be Lines, LineLoop, or LineStrip.", "mesh");
            }

            if (!mesh.Attributes.Contains("position") &&
                !mesh.Attributes.Contains("color"))
            {
                throw new ArgumentException("mesh.Attributes should contain attributes named \"position\" and \"color\".", "mesh");
            }

            if (_drawState.ShaderProgram == null)
            {
                _drawState.ShaderProgram = Device.CreateShaderProgram(
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.Polyline.PolylineVS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.Polyline.PolylineGS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.Polyline.PolylineFS.glsl"));
                _fillDistance = (Uniform<float>)_drawState.ShaderProgram.Uniforms["u_fillDistance"];
            }
            
            ///////////////////////////////////////////////////////////////////
            _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            if (_drawState.ShaderProgram != null)
            {
                _fillDistance.Value = (float)(Width * 0.5 * sceneState.HighResolutionSnapScale);

                context.Draw(_primitiveType, _drawState, sceneState);
            }
        }

        public double Width { get; set; }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthTestEnabled
        {
            get { return _drawState.RenderState.DepthTest.Enabled; }
            set { _drawState.RenderState.DepthTest.Enabled = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_drawState.ShaderProgram != null)
            {
                _drawState.ShaderProgram.Dispose();
            }

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
            }
        }

        #endregion

        private Uniform<float> _fillDistance;
        private readonly DrawState _drawState;
        private PrimitiveType _primitiveType;
    }
}