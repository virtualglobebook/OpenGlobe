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

namespace MiniGlobe.Scene
{
    public sealed class DayNightGlobe : IDisposable
    {
        public DayNightGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _context = context;

            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.GlobeFS.glsl"));
            _cameraEyeSquaredSP = _sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;
            _useAverageDepth = _sp.Uniforms["u_useAverageDepth"] as Uniform<bool>;

            float blendDurationScale = 0.1f;
            (_sp.Uniforms["u_blendDuration"] as Uniform<float>).Value = blendDurationScale;
            (_sp.Uniforms["u_blendDurationScale"] as Uniform<float>).Value = 1 / (2 * blendDurationScale);

            _renderState = new RenderState();

            Shape = Ellipsoid.UnitSphere;
            ShowGlobe = true;
        }

        private void Clean()
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                }

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.Face = CullFace.Front;
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                }
                _wireframe = new Wireframe(_context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");

            Clean();

            if (ShowGlobe || ShowWireframeBoundingBox)
            {
                _context.Bind(_va);
            }

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3S cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3S();
                _cameraEyeSquaredSP.Value = cameraEyeSquared;

                _context.TextureUnits[0].Texture2D = DayTexture;
                _context.TextureUnits[1].Texture2D = NightTexture;
                _context.Bind(_sp);
                _context.Bind(_renderState);
                _context.Draw(_primitiveType, sceneState);
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
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

        public bool ShowGlobe { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D DayTexture { get; set; }
        public Texture2D NightTexture { get; set; }

        public bool UseAverageDepth
        {
            get { return _useAverageDepth.Value; }
            set { _useAverageDepth.Value = value; }
        }

        public int FragmentOutputs(string name)
        {
            return _sp.FragmentOutputs[name];
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();

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

        private readonly Context _context;

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSP;
        private readonly Uniform<bool> _useAverageDepth;
        
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}