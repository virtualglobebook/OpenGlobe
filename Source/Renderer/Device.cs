#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.Diagnostics;
using MiniGlobe.Renderer.GL32;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer
{
    public static class Device
    {
        public static MiniGlobeWindow CreateWindow(int width, int height)
        {
            return CreateWindow(width, height, "");
        }

        public static MiniGlobeWindow CreateWindow(int width, int height, string title)
        {
            return new MiniGlobeWindowGL32(width, height, title);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL32(vertexShaderSource, fragmentShaderSource);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL32(vertexShaderSource, geometryShaderSource, fragmentShaderSource);
        }

        public static VertexBuffer CreateVertexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new VertexBufferGL32(usageHint, sizeInBytes);
        }

        public static IndexBuffer CreateIndexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new IndexBufferGL32(usageHint, sizeInBytes);
        }

        public static UniformBuffer CreateUniformBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new UniformBufferGL32(usageHint, sizeInBytes);
        }

        public static WritePixelBuffer CreateWritePixelBuffer(WritePixelBufferHint usageHint, int sizeInBytes)
        {
            return new WritePixelBufferGL32(usageHint, sizeInBytes);
        }

        public static Texture2D CreateTexture2D(Texture2DDescription description)
        {
            return new Texture2DGL32(description);
        }

        public static Texture2D CreateTexture2D(Bitmap bitmap, TextureFormat format, bool generateMipmaps)
        {
            const int bitsPerByte = 8;
            int sizeInBytes = bitmap.Width * bitmap.Height * (Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / bitsPerByte);
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, sizeInBytes);
            pixelBuffer.CopyFromBitmap(bitmap);

            Texture2DDescription description = new Texture2DDescription(bitmap.Width, bitmap.Height, format, generateMipmaps);
            Texture2D texture = Device.CreateTexture2D(description);
            texture.CopyFromBuffer(pixelBuffer,
                TextureUtility.ImagingPixelFormatToImageFormat(bitmap.PixelFormat),
                TextureUtility.ImagingPixelFormatToDataType(bitmap.PixelFormat));

            return texture;
        }

        public static Extensions Extensions
        {
            get { return s_extensions; }
        }

        public static ShaderCache ShaderCache
        {
            get { return _shaderCache; }
        }

        internal static LinkAutomaticUniformCollection LinkAutomaticUniforms
        {
            get { return s_linkAutomaticUniforms; }
        }

        internal static DrawAutomaticUniformFactoryCollection DrawAutomaticUniformFactories
        {
            get { return s_drawAutomaticUniformFactories; }
        }

        private static Extensions CreateExtensions()
        {
            using (MiniGlobeWindow window = CreateWindow(1, 1))
            {
                return new ExtensionsGL32();
            }
        }

        private static LinkAutomaticUniformCollection CreateLinkAutomaticUniforms()
        {
            LinkAutomaticUniformCollection linkAutomaticUniforms = new LinkAutomaticUniformCollection();

            using (MiniGlobeWindow window = CreateWindow(1, 1))
            {
                for (int i = 0; i < window.Context.TextureUnits.Count; ++i)
                {
                    linkAutomaticUniforms.Add(new TextureUniform(i));
                }
            }

            return linkAutomaticUniforms;
        }

        private static DrawAutomaticUniformFactoryCollection CreateDrawAutomaticUniforms()
        {
            DrawAutomaticUniformFactoryCollection drawAutomaticUniformFactories = new DrawAutomaticUniformFactoryCollection();

            drawAutomaticUniformFactories.Add(new SunPositionUniformFactory());
            drawAutomaticUniformFactories.Add(new LightPropertiesUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraLightPositionUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraEyeUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewPerspectiveProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new OrthographicProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportTransformationMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelZToClipCoordinatesUniformFactory());
            drawAutomaticUniformFactories.Add(new WindowToWorldNearPlaneUniformFactory());
            drawAutomaticUniformFactories.Add(new Wgs84AltitudeUniformFactory());
            drawAutomaticUniformFactories.Add(new HighResolutionSnapScaleUniformFactory());
                                    
            return drawAutomaticUniformFactories;
        }

        // TODO:  Protect shader cache with lock?
        private static Extensions s_extensions = CreateExtensions();
        private static ShaderCache _shaderCache = new ShaderCache();
        private static LinkAutomaticUniformCollection s_linkAutomaticUniforms = CreateLinkAutomaticUniforms();
        private static DrawAutomaticUniformFactoryCollection s_drawAutomaticUniformFactories = CreateDrawAutomaticUniforms();
    }
}
