#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using System.Collections.Generic;

namespace MiniGlobe.Terrain
{
    public sealed class VertexDisplacementMapTerrainTile : IDisposable
    {
        public VertexDisplacementMapTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec2 position;
                  
                  out float height;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform float u_heightExaggeration;

                  void main()
                  {
                      vec4 displacedPosition = vec4(position.xy, 
                          texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                      height = displacedPosition.z;
                  }";
            string fs =
                @"#version 150
                 
                  in float height;

                  out vec3 fragmentColor;

                  uniform float u_minimumHeight;
                  uniform float u_maximumHeight;

                  void main()
                  {
                      fragmentColor = vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            _tileSize = tile.Size;
            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = _sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _minimumHeight = _sp.Uniforms["u_minimumHeight"] as Uniform<float>;
            _maximumHeight = _sp.Uniforms["u_maximumHeight"] as Uniform<float>;
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.X, tile.Size.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;

            ///////////////////////////////////////////////////////////////////

            
            Mesh mesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.5, 0.5), 
                new Vector2D((double)tile.Size.X - 0.5, (double)tile.Size.Y - 0.5)), 
                tile.Size.X - 1, tile.Size.Y - 1);
            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
        }

        public void Render(SceneState sceneState)
        {
            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration must be greater than zero.");
                }

                if (_heightExaggeration.Value != value)
                {
                    //
                    // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                    // scale will incorrectly move the entire tile.
                    //
                    _heightExaggeration.Value = value;
                    _minimumHeight.Value = _tileMinimumHeight * value;
                    _maximumHeight.Value = _tileMaximumHeight * value;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;

        private readonly Vector2I _tileSize;
        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private readonly Texture2D _texture;
        private readonly RenderState _renderState;

        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;
    }
}