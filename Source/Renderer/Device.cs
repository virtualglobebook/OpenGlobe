#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using OpenGlobe.Core;
using OpenGlobe.Renderer.GL3x;
using OpenTK.Graphics.OpenGL;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Threading;

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
                GL.GetInteger(GetPName.MaxVertexAttribs, out s_maximumVertexAttributes);
                GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out s_numberOfTextureUnits);
                GL.GetInteger(GetPName.MaxColorAttachments, out s_maximumColorAttachments);

                GL.GetInteger(GetPName.MaxTransformFeedbackInterleavedComponents, out s_maximumTransformFeedbackInterleavedComponents);
                GL.GetInteger(GetPName.MaxTransformFeedbackSeparateAttribs, out s_maximumTransformFeedbackSeparateAttributes);
                GL.GetInteger(GetPName.MaxTransformFeedbackSeparateComponents, out s_maximumTransformFeedbackSeparateComponents);

                ///////////////////////////////////////////////////////////////

                s_shaderCache = new ShaderCache();
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
                drawAutomaticUniformFactories.Add(new CameraEyeHighUniformFactory());
                drawAutomaticUniformFactories.Add(new CameraEyeLowUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewPerspectiveMatrixRelativeToEyeUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewMatrixRelativeToEyeUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewPerspectiveMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewOrthographicMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelViewMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ModelMatrixUniformFactory());
                drawAutomaticUniformFactories.Add(new ViewMatrixUniformFactory());
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
            return new ShaderProgramGL3x(vertexShaderSource, string.Empty, 
                fragmentShaderSource, null, TransformFeedbackAttributeLayout.Separate);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL3x(vertexShaderSource, geometryShaderSource,
                fragmentShaderSource, null, TransformFeedbackAttributeLayout.Separate);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource,
            IEnumerable<string> transformFeedbackOutputs,
            TransformFeedbackAttributeLayout transformFeedbackAttributeLayout)
        {
            return new ShaderProgramGL3x(vertexShaderSource, geometryShaderSource,
                fragmentShaderSource, transformFeedbackOutputs, transformFeedbackAttributeLayout);
        }

        public static ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string fragmentShaderSource,
            IEnumerable<string> transformFeedbackOutputs,
            TransformFeedbackAttributeLayout transformFeedbackAttributeLayout)
        {
            return new ShaderProgramGL3x(vertexShaderSource, string.Empty,
                fragmentShaderSource, transformFeedbackOutputs, transformFeedbackAttributeLayout);
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
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (shaderAttributes == null)
            {
                throw new ArgumentNullException("shaderAttributes");
            }

            MeshBuffers meshBuffers = new MeshBuffers();

            if (mesh.Indices != null)
            {
                if (mesh.Indices.Datatype == IndicesType.UnsignedShort)
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
                else if (mesh.Indices.Datatype == IndicesType.UnsignedInt)
                {
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
                else
                {
                    throw new NotSupportedException("mesh.Indices.Datatype " +
                        mesh.Indices.Datatype.ToString() + " is not supported.");
                }
            }

            //
            // Emulated double precision vectors are a special case:  one mesh vertex attribute
            // yields two shader vertex attributes.  As such, these are handled separately before
            // normal attributes.
            //
            HashSet<string> ignoreAttributes = new HashSet<string>();

            foreach (VertexAttribute attribute in mesh.Attributes)
            {
                if (attribute is VertexAttributeDoubleVector3)
                {
                    VertexAttributeDoubleVector3 emulated = (VertexAttributeDoubleVector3)attribute;

                    int highLocation = -1;
                    int lowLocation = -1;

                    foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
                    {
                        if (shaderAttribute.Name == emulated.Name + "High")
                        {
                            highLocation = shaderAttribute.Location;
                        }
                        else if (shaderAttribute.Name == emulated.Name + "Low")
                        {
                            lowLocation = shaderAttribute.Location;
                        }

                        if ((highLocation != -1) && (lowLocation != -1))
                        {
                            break;
                        }
                    }

                    if ((highLocation == -1) && (lowLocation == -1))
                    {
                        //
                        // The shader did not have either attribute.  No problem.
                        //
                        continue;
                    }
                    else if ((highLocation == -1) || (lowLocation == -1))
                    {
                        throw new ArgumentException("An emulated double vec3 mesh attribute requires both " + emulated.Name + "High and " + emulated.Name + "Low vertex attributes, but the shader only contains one matching attribute.");
                    }

                    //
                    // Copy both high and low parts into a single vertex buffer.
                    //
                    IList<Vector3D> values = ((VertexAttribute<Vector3D>)attribute).Values;

                    Vector3F[] vertices = new Vector3F[2 * values.Count];

                    int j = 0;
                    for (int i = 0; i < values.Count; ++i)
                    {
                        EmulatedVector3D v = new EmulatedVector3D(values[i]);
                        vertices[j++] = v.High;
                        vertices[j++] = v.Low;
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(vertices));
                    vertexBuffer.CopyFromSystemMemory(vertices);

                    int stride = 2 * SizeInBytes<Vector3F>.Value;
                    meshBuffers.Attributes[highLocation] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3, false, 0, stride);
                    meshBuffers.Attributes[lowLocation] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3, false, SizeInBytes<Vector3F>.Value, stride);

                    ignoreAttributes.Add(emulated.Name + "High");
                    ignoreAttributes.Add(emulated.Name + "Low");
                }
            }

            // TODO:  Not tested exhaustively
            foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
            {
                if (ignoreAttributes.Contains(shaderAttribute.Name))
                {
                    continue;
                }

                if (!mesh.Attributes.Contains(shaderAttribute.Name))
                {
                    throw new ArgumentException("Shader requires vertex attribute \"" + shaderAttribute.Name + "\", which is not present in mesh.");
                }

                VertexAttribute attribute = mesh.Attributes[shaderAttribute.Name];


                if (attribute.Datatype == VertexAttributeType.EmulatedDoubleVector3)
                {
                    IList<Vector3D> values = ((VertexAttribute<Vector3D>)attribute).Values;

                    Vector3F[] valuesArray = new Vector3F[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector3F();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(valuesArray));
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloat)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Half>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.Float)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<float>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.UnsignedByte)
                {
                    if (attribute is VertexAttributeRGBA)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 4, true, 0, 0);
                    }

                    else if (attribute is VertexAttributeRGB)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 3, true, 0, 0);
                    }
                    else
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

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

        private static VertexBuffer CreateVertexBuffer<T>(IList<T> values, BufferHint usageHint) where T : struct
        {
            T[] valuesArray = new T[values.Count];
            values.CopyTo(valuesArray, 0);

            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(valuesArray));
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

        public static Bitmap CreateBitmapFromPoint(int radiusInPixels)
        {
            if (radiusInPixels < 1)
            {
                throw new ArgumentOutOfRangeException("radius");
            }

            int diameter = radiusInPixels * 2;
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

        // TODO:  Add Number of uniform buffers and read/write pixel buffers?
        public static int NumberOfShaderProgramsCreated { get { return ShaderProgramCount; } }
        public static int NumberOfVertexBuffersCreated { get { return VertexBufferCount; } }
        public static int NumberOfIndexBuffersCreated { get { return IndexBufferCount; } }
        public static int NumberOfTexturesCreated { get { return TextureCount; } }
        public static int NumberOfFencesCreated { get { return FenceCount; } }
        public static int NumberOfVertexArraysCreated { get { return VertexArrayCount; } }
        public static int NumberOfFramebuffersCreated { get { return FramebufferCount; } }

        internal static int ShaderProgramCount;
        internal static int VertexBufferCount;
        internal static int IndexBufferCount;
        internal static int TextureCount;
        internal static int FenceCount;
        internal static int VertexArrayCount;
        internal static int FramebufferCount;

        public static int VertexBufferMemoryUsedInBytes { get { return VertexBufferMemoryCount; } }
        public static int IndexBufferMemoryUsedInBytes { get { return IndexBufferMemoryCount; } }
        public static int TextureMemoryUsedInBytes { get { return TextureMemoryCount; } }

        internal static int VertexBufferMemoryCount;
        internal static int IndexBufferMemoryCount;
        internal static int TextureMemoryCount;

        public static ShaderCache ShaderCache
        {
            get { return s_shaderCache; }
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

        public static int MaximumVertexAttributes
        {
            get { return s_maximumVertexAttributes;  }
        }

        public static int NumberOfTextureUnits
        {
            get { return s_numberOfTextureUnits; }
        }

        public static int MaximumColorAttachments
        { 
            get { return s_maximumColorAttachments; } 
        }

        public static int MaximumTransformFeedbackInterleavedComponents
        {
            get { return s_maximumTransformFeedbackInterleavedComponents; }
        }

        public static int MaximumTransformFeedbackSeparateAttributes
        {
            get { return s_maximumTransformFeedbackSeparateAttributes; }
        }

        public static int MaximumTransformFeedbackSeparateComponents
        {
            get { return s_maximumTransformFeedbackSeparateComponents; }
        }

        private static int s_maximumVertexAttributes;
        private static int s_numberOfTextureUnits;
        private static int s_maximumColorAttachments;

        private static int s_maximumTransformFeedbackInterleavedComponents;
        private static int s_maximumTransformFeedbackSeparateAttributes;
        private static int s_maximumTransformFeedbackSeparateComponents;

        private static ShaderCache s_shaderCache;
        private static Extensions s_extensions;
        private static LinkAutomaticUniformCollection s_linkAutomaticUniforms;
        private static DrawAutomaticUniformFactoryCollection s_drawAutomaticUniformFactories;

        private static TextureSamplers s_textureSamplers;
    }
}
