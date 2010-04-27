#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Diagnostics;
using MiniGlobe.Renderer.GL3x;
using OpenTK.Graphics.OpenGL;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MiniGlobe.Renderer
{
    public enum WindowType
    {
        Default = 0,
        FullScreen = 1
    }

    public static class Device
    {
        public static MiniGlobeWindow CreateWindow(int width, int height)
        {
            return CreateWindow(width, height, "");
        }

        public static MiniGlobeWindow CreateWindow(int width, int height, string title)
        {
            return CreateWindow(width, height, title, WindowType.Default);
        }

        public static MiniGlobeWindow CreateWindow(int width, int height, string title, WindowType windowType)
        {
            return new MiniGlobeWindowGL3x(width, height, title, windowType);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL3x(vertexShaderSource, fragmentShaderSource);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL3x(vertexShaderSource, geometryShaderSource, fragmentShaderSource);
        }

        public static VertexBuffer CreateVertexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new VertexBufferGL3x(usageHint, sizeInBytes);
        }

        public static IndexBuffer CreateIndexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new IndexBufferGL3x(usageHint, sizeInBytes);
        }

        public static UniformBuffer CreateUniformBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new UniformBufferGL3x(usageHint, sizeInBytes);
        }

        public static WritePixelBuffer CreateWritePixelBuffer(WritePixelBufferHint usageHint, int sizeInBytes)
        {
            return new WritePixelBufferGL3x(usageHint, sizeInBytes);
        }

        public static Texture2D CreateTexture2D(Texture2DDescription description)
        {
            return new Texture2DGL3x(description, TextureTarget.Texture2D);
        }

        public static Texture2D CreateTexture2D(Bitmap bitmap, TextureFormat format, bool generateMipmaps)
        {
            return CreateTexture2DFromBitmap(bitmap, format, generateMipmaps, TextureTarget.Texture2D);
        }

        public static Texture2D CreateTexture2DRectangle(Texture2DDescription description)
        {
            return new Texture2DGL3x(description, TextureTarget.TextureRectangle);
        }

        public static Texture2D CreateTexture2DRectangle(Bitmap bitmap, TextureFormat format)
        {
            return CreateTexture2DFromBitmap(bitmap, format, false, TextureTarget.TextureRectangle);
        }

        private static Texture2D CreateTexture2DFromBitmap(Bitmap bitmap, TextureFormat format, bool generateMipmaps, TextureTarget textureTarget)
        {
            const int bitsPerByte = 8;
            int sizeInBytes = bitmap.Width * bitmap.Height * (Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / bitsPerByte);
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, sizeInBytes))
            {
                pixelBuffer.CopyFromBitmap(bitmap);

                Texture2DDescription description = new Texture2DDescription(bitmap.Width, bitmap.Height, format, generateMipmaps);
                Texture2D texture = new Texture2DGL3x(description, textureTarget);
                texture.CopyFromBuffer(pixelBuffer,
                    TextureUtility.ImagingPixelFormatToImageFormat(bitmap.PixelFormat),
                    TextureUtility.ImagingPixelFormatToDataType(bitmap.PixelFormat));

                return texture;
            }
        }

        public static Bitmap CreateBitmapFromText(string text, Font font)
        {
            Bitmap tmpbitmap = new Bitmap(1, 1);
            Graphics tmpGraphics = Graphics.FromImage(tmpbitmap);
            SizeF size = tmpGraphics.MeasureString(text, font);
            tmpGraphics.Dispose();
            tmpbitmap.Dispose();

            Bitmap bitmap = new Bitmap(
                (int)Math.Ceiling(size.Width),
                (int)Math.Ceiling(size.Height),
                ImagingPixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            Brush brush = new SolidBrush(Color.White);
            graphics.DrawString(text, font, brush, new PointF());

            brush.Dispose();
            graphics.Dispose();

            return bitmap;
        }

        public static Bitmap CreateBitmapFromPoint(int diameter)
        {
            if (diameter < 1)
            {
                throw new ArgumentOutOfRangeException("diameter");
            }

            Bitmap bitmap = new Bitmap(diameter, diameter, ImagingPixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            Brush brush = new SolidBrush(Color.White);
            graphics.FillEllipse(new SolidBrush(Color.White), 0, 0, diameter, diameter);

            brush.Dispose();
            graphics.Dispose();

            return bitmap;
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
                return new ExtensionsGL3x();
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
            drawAutomaticUniformFactories.Add(new ModelViewOrthographicProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new OrthographicProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportOrthographicProjectionMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportTransformationMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelZToClipCoordinatesUniformFactory());
            drawAutomaticUniformFactories.Add(new WindowToWorldNearPlaneUniformFactory());
            drawAutomaticUniformFactories.Add(new Wgs84AltitudeUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveNearPlaneDistanceUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveFarPlaneDistanceUniformFactory());
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
