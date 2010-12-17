#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class ViewportQuad : IDisposable
    {
        public ViewportQuad(Context context, string fs)
        {
            Verify.ThrowIfNull(context);

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;

            _drawState = new DrawState();
            _drawState.RenderState = renderState;
            _drawState.ShaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.ViewportQuad.Shaders.ViewportQuadVS.glsl"),
                fs == null ? EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.ViewportQuad.Shaders.ViewportQuadFS.glsl") : fs);

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            _geometry.Update(context, _drawState.ShaderProgram);

            context.TextureUnits[0].Texture = Texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            _drawState.VertexArray = _geometry.VertexArray;

            context.Draw(PrimitiveType.TriangleStrip, _drawState, sceneState);
        }

        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _geometry.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly ViewportQuadGeometry _geometry;
    }
}