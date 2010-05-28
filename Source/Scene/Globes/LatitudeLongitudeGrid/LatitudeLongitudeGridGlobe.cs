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
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using System.Collections.Generic;

namespace MiniGlobe.Scene
{
    public sealed class LatitudeLongitudeGridGlobe : IDisposable
    {
        public LatitudeLongitudeGridGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _context = context;

            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeFS.glsl"));
            _gridWidth = _sp.Uniforms["u_gridLineWidth"] as Uniform<Vector2S>;
            _gridResolution = _sp.Uniforms["u_gridResolution"] as Uniform<Vector2S>;
            _globeOneOverRadiiSquared = _sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>;

            _renderState = new RenderState();

            Shape = Ellipsoid.UnitSphere;
        }

        private void Clean()
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                }

                Mesh mesh = GeographicGridEllipsoidTessellator.Compute(_shape, 64, 32, GeographicGridEllipsoidVertexAttributes.Position);
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _globeOneOverRadiiSquared.Value = _shape.OneOverRadiiSquared.ToVector3S();

                _dirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            if (GridResolutions == null)
            {
                throw new InvalidOperationException("GridResolutions");
            }

            Clean();

            //
            // This could be improved to exploit temporal coherence as described in section x.x.
            //
            double altitude = sceneState.Camera.Altitude(_shape);
            for (int i = 0; i < GridResolutions.Count; ++i)
            {
                if (GridResolutions[i].Interval.Contains(altitude))
                {
                    _gridResolution.Value = GridResolutions[i].Resolution.ToVector2S();
                    break;
                }
            }

            float width = (float)sceneState.HighResolutionSnapScale;
            _gridWidth.Value = new Vector2S(width, width);

            _context.TextureUnits[0].Texture2D = Texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(_primitiveType, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public Texture2D Texture { get; set; }

        public GridResolutionCollection GridResolutions { get; set; }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector2S> _gridWidth;
        private readonly Uniform<Vector2S> _gridResolution;
        private readonly Uniform<Vector3S> _globeOneOverRadiiSquared;

        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}