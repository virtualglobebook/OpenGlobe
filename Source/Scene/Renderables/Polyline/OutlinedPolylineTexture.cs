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
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class OutlinedPolylineTexture : IDisposable
    {
        public OutlinedPolylineTexture(Context context)
        {
            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            
            Width = 3;
            OutlineWidth = 2;
        }

        public void Set(Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (mesh.PrimitiveType != PrimitiveType.Lines &&
                mesh.PrimitiveType != PrimitiveType.LineLoop &&
                mesh.PrimitiveType != PrimitiveType.LineStrip)
            {
                throw new ArgumentException("mesh.PrimitiveType must be Lines, LineLoop, or LineStrip.", "mesh");
            }

            if (!mesh.Attributes.Contains("position") &&
                !mesh.Attributes.Contains("color") &&
                !mesh.Attributes.Contains("outlineColor"))
            {
                throw new ArgumentException("mesh.Attributes should contain attributes named \"position\", \"color\", and \"outlineColor\".", "mesh");
            }

            if (_sp == null)
            {
                _sp = Device.CreateShaderProgram(
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineVS.glsl"),
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineGS.glsl"),
                    EmbeddedResources.GetText("MiniGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineFS.glsl"));
                _distance = _sp.Uniforms["u_distance"] as Uniform<float>;
            }

            ///////////////////////////////////////////////////////////////////
            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        private void Update(SceneState sceneState)
        {
            int width = (int)Math.Ceiling(Width * sceneState.HighResolutionSnapScale);
            int outlineWidth = (int)Math.Ceiling(OutlineWidth * sceneState.HighResolutionSnapScale);

            int textureWidth = width + outlineWidth + outlineWidth + 2;

            if ((_texture == null) || (_texture.Description.Width != textureWidth))
            {
                int textureSize = textureWidth * 2;

                float[] texels = new float[textureSize];

                int k = 3;
                for (int i = 1; i < textureWidth - 1; ++i)
                {
                    texels[k] = 1;                  // Alpha (stored in Green channel)
                    k += 2;
                }

                int j = (outlineWidth + 1) * 2;
                for (int i = 0; i < width; ++i)
                {
                    texels[j] = 1;                  // Fill/Outline (stored in Red channel)
                    j += 2;
                }

                WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                    sizeof(float) * textureSize);
                pixelBuffer.CopyFromSystemMemory(texels);

                if (_texture != null)
                {
                    _texture.Dispose();
                }
                _texture = Device.CreateTexture2D(new Texture2DDescription(textureWidth, 1, TextureFormat.RedGreen8));
                _texture.CopyFromBuffer(pixelBuffer, ImageFormat.RedGreen, ImageDataType.Float);
                _texture.Filter = Texture2DFilter.LinearClampToEdge;
            }
        }

        public void Render(SceneState sceneState)
        {
            if (_sp != null)
            {
                Update(sceneState);

                _distance.Value = (float)(((Width * 0.5) + OutlineWidth + 1) * sceneState.HighResolutionSnapScale);

                _context.TextureUnits[0].Texture2D = _texture;
                _context.Bind(_renderState);
                _context.Bind(_sp);
                _context.Bind(_va);
                _context.Draw(_primitiveType, sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Width { get; set; }

        public double OutlineWidth { get; set; }

        public bool Wireframe
        {
            get { return _renderState.RasterizationMode == RasterizationMode.Line; }
            set { _renderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthWrite
        {
            get { return _renderState.DepthWrite; }
            set { _renderState.DepthWrite = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_sp != null)
            {
                _sp.Dispose();
            }

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_texture != null)
            {
                _texture.Dispose();
            }
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private ShaderProgram _sp;
        private Uniform<float> _distance;
        private VertexArray _va;
        private PrimitiveType _primitiveType;
        private Texture2D _texture;
    }
}