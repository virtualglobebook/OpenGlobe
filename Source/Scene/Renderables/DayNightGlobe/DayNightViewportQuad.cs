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

namespace MiniGlobe.Scene
{
    public sealed class DayNightViewportQuad : IDisposable
    {
        public DayNightViewportQuad(Context context)
        {
            Verify.ThrowIfNull(context);

            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.DepthTest.Enabled = false;

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec2 textureCoordinates;

                  out vec2 fsTextureCoordinates;

                  uniform mat4 mg_viewportOrthographicProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_viewportOrthographicProjectionMatrix * position;
                      fsTextureCoordinates = textureCoordinates;
                  }";
            string fs =
                @"#version 150
                 
                  in vec2 fsTextureCoordinates;

                  out vec4 fragmentColor;

                  uniform sampler2D mg_texture0;    // Day
                  uniform sampler2D mg_texture1;    // Night
                  uniform sampler2D mg_texture2;    // Blend

                  void main()
                  {
                      vec4 dayColor = texture(mg_texture0, fsTextureCoordinates);
                      vec4 nightColor = texture(mg_texture1, fsTextureCoordinates);
                      float blend = texture(mg_texture2, fsTextureCoordinates).r;

                      fragmentColor = mix(nightColor, dayColor, blend);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");
            Verify.ThrowInvalidOperationIfNull(BlendTexture, "BlendTexture");

            _geometry.Update(_context, _sp);

            _context.TextureUnits[0].Texture2D = DayTexture;
            _context.TextureUnits[1].Texture2D = NightTexture;
            _context.TextureUnits[2].Texture2D = BlendTexture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_geometry.VertexArray);
            _context.Draw(PrimitiveType.TriangleStrip, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public Texture2D DayTexture { get; set; }
        public Texture2D NightTexture { get; set; }
        public Texture2D BlendTexture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _geometry.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly ViewportQuadGeometry _geometry;
    }
}