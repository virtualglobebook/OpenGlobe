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
using OpenGlobe.Core.Geometry;
using OpenGlobe.Core.Tessellation;
using OpenGlobe.Renderer;
using System.Collections.Generic;

namespace OpenGlobe.Scene
{
    public sealed class LatitudeLongitudeGridGlobe : IDisposable
    {
        public LatitudeLongitudeGridGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeFS.glsl"));
            _gridWidth = sp.Uniforms["u_gridLineWidth"] as Uniform<Vector2S>;
            _gridResolution = sp.Uniforms["u_gridResolution"] as Uniform<Vector2S>;
            _globeOneOverRadiiSquared = sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>;

            _drawState = new DrawState();
            _drawState.ShaderProgram = sp;

            Shape = Ellipsoid.UnitSphere;
        }

        private void Clean(Context context)
        {
            if (_dirty)
            {
                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }

                Mesh mesh = GeographicGridEllipsoidTessellator.Compute(_shape, 64, 32, GeographicGridEllipsoidVertexAttributes.Position);
                _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _globeOneOverRadiiSquared.Value = _shape.OneOverRadiiSquared.ToVector3S();

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            if (GridResolutions == null)
            {
                throw new InvalidOperationException("GridResolutions");
            }

            Clean(context);

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

            context.TextureUnits[0].Texture2D = Texture;
            context.Draw(_primitiveType, _drawState, sceneState);
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
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<Vector2S> _gridWidth;
        private readonly Uniform<Vector2S> _gridResolution;
        private readonly Uniform<Vector3S> _globeOneOverRadiiSquared;

        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}