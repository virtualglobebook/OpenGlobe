#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public enum DayNightOutput
    {
        Composite,
        DayBuffer,
        NightBuffer,
        BlendBuffer
    }

    public sealed class DayNightViewportQuad : IDisposable
    {
        public DayNightViewportQuad(Context context)
        {
            Verify.ThrowIfNull(context);

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.DayNight.Shaders.ViewportQuadVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.DayNight.Shaders.ViewportQuadFS.glsl"));
            _dayNightOutput = sp.Uniforms["u_DayNightOutput"] as Uniform<int>;

            _drawState = new DrawState();
            _drawState.RenderState = renderState;
            _drawState.ShaderProgram = sp;

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");
            Verify.ThrowInvalidOperationIfNull(BlendTexture, "BlendTexture");

            _geometry.Update(context, _drawState.ShaderProgram);

            context.TextureUnits[0].Texture = DayTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
            context.TextureUnits[1].Texture = NightTexture;
            context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
            context.TextureUnits[2].Texture = BlendTexture;
            context.TextureUnits[2].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
            _drawState.VertexArray = _geometry.VertexArray;

            context.Draw(PrimitiveType.TriangleStrip, _drawState, sceneState);
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
            _drawState.ShaderProgram.Dispose();
            _geometry.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<int> _dayNightOutput;
        private readonly ViewportQuadGeometry _geometry;
    }
}