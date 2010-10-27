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

namespace OpenGlobe.Scene
{
    public sealed class RayCastedGlobe : IDisposable
    {
        public RayCastedGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            string vs = EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.GlobeVS.glsl");

            ShaderProgram sp = Device.CreateShaderProgram(vs, EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.GlobeFS.glsl"));
            _cameraEyeSquared = sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;
            _useAverageDepth = sp.Uniforms["u_useAverageDepth"] as Uniform<bool>;

            ShaderProgram solidSP = Device.CreateShaderProgram(vs, EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.SolidShadedGlobeFS.glsl"));
            _cameraEyeSquaredSolid = solidSP.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;
            _useAverageDepthSolid = solidSP.Uniforms["u_useAverageDepth"] as Uniform<bool>;

            _drawState = new DrawState(_renderState, sp, null);
            _drawStateSolid = new DrawState(_renderState, solidSP, null);

            Shape = Ellipsoid.ScaledWgs84;
            Shade = true;
            ShowGlobe = true;
        }

        private void Clean(Context context)
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                    _va = null;
                    _drawState.VertexArray = null;
                    _drawStateSolid.VertexArray = null;
                }

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _va = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _drawState.VertexArray = _va;
                _drawStateSolid.VertexArray = _va;
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                (_drawState.ShaderProgram.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();
                (_drawStateSolid.ShaderProgram.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                    _wireframe = null;
                }
                _wireframe = new Wireframe(context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Clean(context);

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3S cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3S();

                if (Shade)
                {
                    context.TextureUnits[0].Texture2D = Texture;
                    context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
                    _cameraEyeSquared.Value = cameraEyeSquared;
                    context.Draw(_primitiveType, _drawState, sceneState);
                }
                else
                {
                    _cameraEyeSquaredSolid.Value = cameraEyeSquared;
                    context.Draw(_primitiveType, _drawStateSolid, sceneState);
                }
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(context, sceneState);
            }
        }

        public bool UseAverageDepth
        {
            get { return _useAverageDepth.Value; }
            set 
            { 
                _useAverageDepth.Value = value;
                _useAverageDepthSolid.Value = value;
            }
        }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        public bool Shade { get; set; }
        public bool ShowGlobe { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawStateSolid.ShaderProgram.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3S> _cameraEyeSquared;
        private readonly DrawState _drawStateSolid;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSolid;

        private readonly Uniform<bool> _useAverageDepth;
        private readonly Uniform<bool> _useAverageDepthSolid;
        
        private readonly RenderState _renderState;
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}