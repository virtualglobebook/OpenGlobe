#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

public enum DayNightOutput
{
    Composite,
    DayBuffer,
    NightBuffer,
    BlendBuffer
}

namespace MiniGlobe.Scene
{
    public sealed class DayNightViewportQuad : IDisposable
    {
        public DayNightViewportQuad(Context context)
        {
            Verify.ThrowIfNull(context);

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.DepthTest.Enabled = false;

            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.ViewportQuadVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Globes.DayNight.Shaders.ViewportQuadFS.glsl"));
            _dayNightOutput = _sp.Uniforms["u_DayNightOutput"] as Uniform<int>;

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");
            Verify.ThrowInvalidOperationIfNull(BlendTexture, "BlendTexture");

            _geometry.Update(context, _sp);

            context.TextureUnits[0].Texture2D = DayTexture;
            context.TextureUnits[1].Texture2D = NightTexture;
            context.TextureUnits[2].Texture2D = BlendTexture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_geometry.VertexArray);
            context.Draw(PrimitiveType.TriangleStrip, sceneState);
        }

        public Texture2D DayTexture { get; set; }
        public Texture2D NightTexture { get; set; }
        public Texture2D BlendTexture { get; set; }

        public DayNightOutput DayNightOutput
        {
            get { return (DayNightOutput)_dayNightOutput.Value; }
            set { _dayNightOutput.Value = (int)value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _geometry.Dispose();
        }

        #endregion

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<int> _dayNightOutput;
        private readonly ViewportQuadGeometry _geometry;
    }
}