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
    public sealed class DayNightGlobe : IDisposable
    {
        public DayNightGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.GlobeFS.glsl"));
            _cameraEyeSquaredSP = sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;
            _useAverageDepth = sp.Uniforms["u_useAverageDepth"] as Uniform<bool>;

            float blendDurationScale = 0.1f;
            (sp.Uniforms["u_blendDuration"] as Uniform<float>).Value = blendDurationScale;
            (sp.Uniforms["u_blendDurationScale"] as Uniform<float>).Value = 1 / (2 * blendDurationScale);

            _drawState = new DrawState();
            _drawState.ShaderProgram = sp;

            Shape = Ellipsoid.UnitSphere;
            ShowGlobe = true;
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

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _drawState.RenderState.FacetCulling.Face = CullFace.Front;
                _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                (_drawState.ShaderProgram.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
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
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");

            Clean(context);

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3S cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3S();
                _cameraEyeSquaredSP.Value = cameraEyeSquared;

                context.TextureUnits[0].Texture2D = DayTexture;
                context.TextureUnits[1].Texture2D = NightTexture;
                context.Draw(_primitiveType, _drawState, sceneState);
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(context, sceneState);
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
            return _drawState.ShaderProgram.FragmentOutputs[name];
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSP;
        private readonly Uniform<bool> _useAverageDepth;
        
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}