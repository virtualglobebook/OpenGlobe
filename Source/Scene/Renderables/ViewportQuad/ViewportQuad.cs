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
    public sealed class ViewportQuad : IDisposable
    {
        public ViewportQuad(Context context)
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

                  uniform sampler2D mg_texture0;

                  void main()
                  {
                      fragmentColor = texture(mg_texture0, fsTextureCoordinates);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            _geometry = new ViewportQuadGeometry();
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            _geometry.Update(_context, _sp);

            _context.TextureUnits[0].Texture2D = Texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_geometry.VertexArray);
            _context.Draw(PrimitiveType.TriangleStrip, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public Texture2D Texture { get; set; }

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