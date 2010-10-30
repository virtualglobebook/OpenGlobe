#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer.GL3x;
using OpenTK.Graphics.OpenGL;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenGlobe.Renderer
{
    public enum WindowType
    {
        Default = 0,
        FullScreen = 1
    }

    public static class Device
    {
        static Device()
        {
            using (GraphicsWindow window = CreateWindow(1, 1))
            {
                GL.GetInteger(GetPName.MaxVertexAttribs, out s_maximumNumberOfVertexAttributes);
                GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out s_numberOfTextureUnits);
                GL.GetInteger(GetPName.MaxColorAttachments, out s_maximumNumberOfColorAttachments);

                ///////////////////////////////////////////////////////////////

                s_extensions = new ExtensionsGL3x();

                ///////////////////////////////////////////////////////////////

                LinkAutomaticUniformCollection linkAutomaticUniforms = new LinkAutomaticUniformCollection();

                for (int i = 0; i < window.Context.TextureUnits.Count; ++i)
                {
                    linkAutomaticUniforms.Add(new TextureUniform(i));
                }

                s_linkAutomaticUniforms = linkAutomaticUniforms;

                ///////////////////////////////////////////////////////////////

                DrawAutomaticUniformFactoryCollection drawAutomaticUniformFactories = new DrawAutomaticUniformFactoryCollection();

                drawAutomaticUniformFactories.Add(new SunPositionUniformFactory());
                drawAutomaticUniformFactories.Add(new LightPropertiesUniformFactory());
                drawAutomaticUniformFactories.Add(new CameraLightPositionUniformFactory());
                drawAutomaticUniformFactories.Add(new CameraEyeUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewPerspectiveMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewOrthographicMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new PerspectiveMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new OrthographicMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ViewportOrthographicMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ViewportUniformFactory());
                drawAutomaticUniformFactories.Add(new InverseViewportDimensionsUniformFactory());
                drawAutomaticUniformFactories.Add(new ViewportTransformationMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelZToClipCoordinatesUniformFactory());
                drawAutomaticUniformFactories.Add(new WindowToWorldNearPlaneUniformFactory());
                drawAutomaticUniformFactories.Add(new Wgs84HeightUniformFactory());
                drawAutomaticUniformFactories.Add(new PerspectiveNearPlaneDistanceUniformFactory());
                drawAutomaticUniformFactories.Add(new PerspectiveFarPlaneDistanceUniformFactory());
                drawAutomaticUniformFactories.Add(new HighResolutionSnapScaleUniformFactory());
                drawAutomaticUniformFactories.Add(new PixelSizePerDistanceUniformFactory());

                s_drawAutomaticUniformFactories = drawAutomaticUniformFactories;

                ///////////////////////////////////////////////////////////////

                s_textureSamplers = new TextureSamplers();
            }
        }

        public static GraphicsWindow CreateWindow(int width, int height)
        {
            return CreateWindow(width, height, "");
        }

        public static GraphicsWindow CreateWindow(int width, int height, string title)
        {
            return CreateWindow(width, height, title, WindowType.Default);
        }

        public static GraphicsWindow CreateWindow(int width, int height, string title, WindowType windowType)
        {
            return new GraphicsWindowGL3x(width, height, title, windowType);
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

        public static MeshBuffers CreateMeshBuffers(Mesh mesh, ShaderVertexAttributeCollection shaderAttributes, BufferHint usageHint)
        {
            MeshBuffers meshBuffers = new MeshBuffers();

            if (mesh.Indices != null)
            {
                if (mesh.Indices.Datatype == IndicesType.Byte)
                {
                    IList<byte> meshIndices = ((IndicesByte)mesh.Indices).Values;

                    byte[] indices = new byte[meshIndices.Count];
                    meshIndices.CopyTo(indices, 0);

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(byte));
                    indexBuffer.CopyFromSystemMemory(indices);
                    meshBuffers.IndexBuffer = indexBuffer;
                }
                else if (mesh.Indices.Datatype == IndicesType.UnsignedShort)
                {
                    IList<ushort> meshIndices = ((IndicesUnsignedShort)mesh.Indices).Values;

                    ushort[] indices = new ushort[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = meshIndices[j];
                    }

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(ushort));
                    indexBuffer.CopyFromSystemMemory(indices);
                    meshBuffers.IndexBuffer = indexBuffer;
                }
                else
                {
                    Debug.Assert(mesh.Indices.Datatype == IndicesType.UnsignedInt);

                    IList<uint> meshIndices = ((IndicesUnsignedInt)mesh.Indices).Values;

                    uint[] indices = new uint[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = meshIndices[j];
                    }

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(uint));
                    indexBuffer.CopyFromSystemMemory(indices);
                    meshBuffers.IndexBuffer = indexBuffer;
                }
            }

            // TODO:  Not tested exhaustively
            foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
            {
                if (!mesh.Attributes.Contains(shaderAttribute.Name))
                {
                    throw new ArgumentException("Shader requires vertex attribute \"" + shaderAttribute.Name + "\", which is not present in mesh.");
                }

                VertexAttribute attribute = mesh.Attributes[shaderAttribute.Name];

                if (attribute.Datatype == VertexAttributeType.Double)
                {
                    IList<double> values = ((VertexAttribute<double>)attribute).Values;

                    float[] valuesArray = new float[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = (float)values[i];
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * sizeof(float));
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.DoubleVector2)
                {
                    IList<Vector2D> values = ((VertexAttribute<Vector2D>)attribute).Values;

                    Vector2S[] valuesArray = new Vector2S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector2S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector2S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.DoubleVector3)
                {
                    IList<Vector3D> values = ((VertexAttribute<Vector3D>)attribute).Values;

                    Vector3S[] valuesArray = new Vector3S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector3S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector3S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.DoubleVector4)
                {
                    IList<Vector4D> values = ((VertexAttribute<Vector4D>)attribute).Values;

                    Vector4S[] valuesArray = new Vector4S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector4S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector4S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloat)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Half>)attribute).Values, SizeInBytes<Half>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2H>)attribute).Values, SizeInBytes<Vector2H>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3H>)attribute).Values, SizeInBytes<Vector3H>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4H>)attribute).Values, SizeInBytes<Vector4H>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.Float)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<float>)attribute).Values, sizeof(float), usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2S>)attribute).Values, SizeInBytes<Vector2S>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3S>)attribute).Values, SizeInBytes<Vector3S>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4S>)attribute).Values, SizeInBytes<Vector4S>.Value, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.UnsignedByte)
                {
                    if (attribute is VertexAttributeRGBA)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, sizeof(byte), usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 4, true, 0, 0);
                    }
                    else
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, sizeof(byte), usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 1);
                    }
                }
                else
                {
                    Debug.Fail("attribute.Datatype");
                }
            }

            return meshBuffers;
        }

        private static VertexBuffer CreateVertexBuffer<T>(IList<T> values, int SizeOfT, BufferHint usageHint) where T : struct
        {
            T[] valuesArray = new T[values.Count];
            values.CopyTo(valuesArray, 0);

            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeOfT);
            vertexBuffer.CopyFromSystemMemory(valuesArray);
            return vertexBuffer;
        }

        public static UniformBuffer CreateUniformBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new UniformBufferGL3x(usageHint, sizeInBytes);
        }

        public static WritePixelBuffer CreateWritePixelBuffer(PixelBufferHint usageHint, int sizeInBytes)
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
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, 
                BitmapAlgorithms.SizeOfPixelsInBytes(bitmap)))
            {
                pixelBuffer.CopyFromBitmap(bitmap);

                Texture2DDescription description = new Texture2DDescription(bitmap.Width, bitmap.Height, format, generateMipmaps);
                Texture2D texture = new Texture2DGL3x(description, textureTarget);
                texture.CopyFromBuffer(pixelBuffer,
                    TextureUtility.ImagingPixelFormatToImageFormat(bitmap.PixelFormat),
                    TextureUtility.ImagingPixelFormatToDatatype(bitmap.PixelFormat));

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

        public static TextureSampler CreateTexture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT)
        {
            return new TextureSamplerGL3x(
                minificationFilter,
                magnificationFilter,
                wrapS,
                wrapT,
                1);
        }

        public static TextureSampler CreateTexture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT,
            float maximumAnistropy)
        {
            return new TextureSamplerGL3x(
                minificationFilter,
                magnificationFilter,
                wrapS,
                wrapT,
                maximumAnistropy);
        }

        public static TextureSamplers TextureSamplers
        {
            get { return s_textureSamplers; }
        }

        public static Fence CreateFence()
        {
            return new FenceGL3x();
        }

        public static void Finish()
        {
            GL.Finish();
        }

        public static void Flush()
        {
            GL.Flush();
        }

        public static Extensions Extensions
        {
            get { return s_extensions; }
        }

        /// <summary>
        /// The collection is not thread safe.
        /// </summary>
        public static LinkAutomaticUniformCollection LinkAutomaticUniforms
        {
            get { return s_linkAutomaticUniforms; }
        }

        /// <summary>
        /// The collection is not thread safe.
        /// </summary>
        public static DrawAutomaticUniformFactoryCollection DrawAutomaticUniformFactories
        {
            get { return s_drawAutomaticUniformFactories; }
        }

        public static int MaximumNumberOfVertexAttributes
        {
            get { return s_maximumNumberOfVertexAttributes;  }
        }

        public static int NumberOfTextureUnits
        {
            get { return s_numberOfTextureUnits; }
        }

        public static int MaximumNumberOfColorAttachments
        { 
            get { return s_maximumNumberOfColorAttachments; } 
        }

        private static int s_maximumNumberOfVertexAttributes;
        private static int s_numberOfTextureUnits;
        private static int s_maximumNumberOfColorAttachments;
        
        private static Extensions s_extensions;
        private static LinkAutomaticUniformCollection s_linkAutomaticUniforms;
        private static DrawAutomaticUniformFactoryCollection s_drawAutomaticUniformFactories;

        private static TextureSamplers s_textureSamplers;
    }
}
