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
using System.Collections.Generic;

namespace MiniGlobe.Terrain
{
    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec2 textureCoordinate;

                  out vec2 fsTextureCoordinate;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                      fsTextureCoordinate = textureCoordinate;
                  }";
            string fs =
                @"#version 150
                 
                  in vec2 fsTextureCoordinate;

                  out vec3 fragmentColor;
                  uniform sampler2DRect mg_texture0;

                  void main()
                  {
                      fragmentColor = vec3(texture(mg_texture0, fsTextureCoordinate).r / 15.0, 0.0, 0.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Vector3D radii = new Vector3D(
                tile.Extent.East - tile.Extent.West,
                tile.Extent.North - tile.Extent.South,
                tile.MaximumHeight - tile.MinimumHeight);

            Mesh mesh = BoxTessellator.Compute(radii);

            VertexAttributeFloatVector2 textureCoordinatesAttribute = new VertexAttributeFloatVector2("textureCoordinate", 8);
            mesh.Attributes.Add(textureCoordinatesAttribute);
            IList<Vector2S> textureCoordinates = textureCoordinatesAttribute.Values;
            for (int i = 0; i < 2; ++i)
            {
                textureCoordinates.Add(new Vector2S(0, 0));
                textureCoordinates.Add(new Vector2S(tile.Size.Width, 0));
                textureCoordinates.Add(new Vector2S(tile.Size.Width, tile.Size.Height));
                textureCoordinates.Add(new Vector2S(0, tile.Size.Height));
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            //_renderState.RasterizationMode = RasterizationMode.Line;

            ///////////////////////////////////////////////////////////////////

            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.Width, tile.Size.Height, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;
        }

        public void Render(SceneState sceneState)
        {
            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
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
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
        private readonly RenderState _renderState;
    }
}