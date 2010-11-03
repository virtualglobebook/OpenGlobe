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
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class OutlinedPolylineTexture : IDisposable
    {
        public OutlinedPolylineTexture()
        {
            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.Blending.Enabled = true;
            renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            _drawState = new DrawState();
            _drawState.RenderState = renderState;

            Width = 3;
            OutlineWidth = 2;
        }

        public void Set(Context context, Mesh mesh)
        {
            Verify.ThrowIfNull(context);

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

            if (_drawState.ShaderProgram == null)
            {
                _drawState.ShaderProgram = Device.CreateShaderProgram(
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineVS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineGS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineTexture.PolylineFS.glsl"));
                _distance = (Uniform<float>)_drawState.ShaderProgram.Uniforms["u_distance"];
            }

            ///////////////////////////////////////////////////////////////////
            _meshBuffers = Device.CreateMeshBuffers(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        private void Update(Context context, SceneState sceneState)
        {
            if (_meshBuffers != null)
            {
                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }

                _drawState.VertexArray = context.CreateVertexArray(_meshBuffers);
                _meshBuffers.Dispose();
                _meshBuffers = null;
            }

            ///////////////////////////////////////////////////////////////////

            int width = (int)Math.Ceiling(Width * sceneState.HighResolutionSnapScale);
            int outlineWidth = (int)Math.Ceiling(OutlineWidth * sceneState.HighResolutionSnapScale);

            int textureWidth = width + outlineWidth + outlineWidth + 2;

            if ((_texture == null) || (_texture.Description.Width != textureWidth))
            {
                int textureResolution = textureWidth * 2;

                float[] texels = new float[textureResolution];

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

                WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream,
                    sizeof(float) * textureResolution);
                pixelBuffer.CopyFromSystemMemory(texels);

                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }

                // TODO:  Why does only Float or HalfFloat work here?
                _texture = Device.CreateTexture2D(new Texture2DDescription(textureWidth, 1, TextureFormat.RedGreen8));
                _texture.CopyFromBuffer(pixelBuffer, ImageFormat.RedGreen, ImageDatatype.Float);
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            if (_drawState.ShaderProgram != null)
            {
                Update(context, sceneState);

                _distance.Value = (float)(((Width * 0.5) + OutlineWidth + 1) * sceneState.HighResolutionSnapScale);

                context.TextureUnits[0].Texture = _texture;
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
                context.Draw(_primitiveType, _drawState, sceneState);
            }
        }

        public double Width { get; set; }

        public double OutlineWidth { get; set; }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthWrite
        {
            get { return _drawState.RenderState.DepthMask; }
            set { _drawState.RenderState.DepthMask = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_drawState.ShaderProgram != null)
            {
                _drawState.ShaderProgram.Dispose();
            }

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
            }

            if (_texture != null)
            {
                _texture.Dispose();
            }
        }

        #endregion

        private readonly DrawState _drawState;
        private Uniform<float> _distance;
        private PrimitiveType _primitiveType;
        private Texture2D _texture;
        private MeshBuffers _meshBuffers;       // For passing between threads
    }
}