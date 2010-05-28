#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class Polyline : IDisposable
    {
        public Polyline(Context context)
        {
            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            Width = 1;
        }

        public void Set(Mesh mesh)
        {
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

            if (_sp == null)
            {
                _sp = Device.CreateShaderProgram(
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.Polyline.PolylineVS.glsl"),
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.Polyline.PolylineGS.glsl"),
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.Polyline.PolylineFS.glsl"));
                _fillDistance = _sp.Uniforms["u_fillDistance"] as Uniform<float>;
            }

            ///////////////////////////////////////////////////////////////////
            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        public void Render(SceneState sceneState)
        {
            if (_sp != null)
            {
                _fillDistance.Value = (float)(Width * 0.5 * sceneState.HighResolutionSnapScale);

                _context.Bind(_renderState);
                _context.Bind(_sp);
                _context.Bind(_va);
                _context.Draw(_primitiveType, sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Width { get; set; }

        public bool Wireframe
        {
            get { return _renderState.RasterizationMode == RasterizationMode.Line; }
            set { _renderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_sp != null)
            {
                _sp.Dispose();
            }

            if (_va != null)
            {
                _va.Dispose();
            }
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private ShaderProgram _sp;
        private Uniform<float> _fillDistance;
        private VertexArray _va;
        private PrimitiveType _primitiveType;
    }
}