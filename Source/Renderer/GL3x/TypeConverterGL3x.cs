#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal static class TypeConverterGL3x
    {
        public static ClearBufferMask To(ClearBuffers mask)
        {
            ClearBufferMask clearMask = 0;

            if ((mask & ClearBuffers.ColorBuffer) != 0)
            {
                clearMask |= ClearBufferMask.ColorBufferBit;
            }

            if ((mask & ClearBuffers.DepthBuffer) != 0)
            {
                clearMask |= ClearBufferMask.DepthBufferBit;
            }

            if ((mask & ClearBuffers.StencilBuffer) != 0)
            {
                clearMask |= ClearBufferMask.StencilBufferBit;
            }

            return clearMask;
        }

        public static ShaderVertexAttributeType To(ActiveAttribType type)
        {
            if (type == ActiveAttribType.Float)
            {
                return ShaderVertexAttributeType.Float;
            }
            else if (type == ActiveAttribType.FloatVec2)
            {
                return ShaderVertexAttributeType.FloatVector2;
            }
            else if (type == ActiveAttribType.FloatVec3)
            {
                return ShaderVertexAttributeType.FloatVector3;
            }
            else if (type == ActiveAttribType.FloatVec4)
            {
                return ShaderVertexAttributeType.FloatVector4;
            }
            else if (type == ActiveAttribType.FloatMat2)
            {
                return ShaderVertexAttributeType.FloatMatrix22;
            }
            else if (type == ActiveAttribType.FloatMat3)
            {
                return ShaderVertexAttributeType.FloatMatrix33;
            }
            else if (type == ActiveAttribType.FloatMat4)
            {
                return ShaderVertexAttributeType.FloatMatrix44;
            }
            else if (type == (ActiveAttribType)All.Int)
            {
                return ShaderVertexAttributeType.Int;
            }
            else if (type == (ActiveAttribType)All.IntVec2)
            {
                return ShaderVertexAttributeType.IntVector2;
            }
            else if (type == (ActiveAttribType)All.IntVec3)
            {
                return ShaderVertexAttributeType.IntVector3;
            }

            Debug.Assert(type == (ActiveAttribType)All.IntVec4);
            return ShaderVertexAttributeType.IntVector4;
        }

        public static UniformType To(ActiveUniformType type)
        {
            if (type == ActiveUniformType.Int)
            {
                return UniformType.Int;
            }
            else if (type == ActiveUniformType.Float)
            {
                return UniformType.Float;
            }
            else if (type == ActiveUniformType.FloatVec2)
            {
                return UniformType.FloatVector2;
            }
            else if (type == ActiveUniformType.FloatVec3)
            {
                return UniformType.FloatVector3;
            }
            else if (type == ActiveUniformType.FloatVec4)
            {
                return UniformType.FloatVector4;
            }
            else if (type == ActiveUniformType.IntVec2)
            {
                return UniformType.IntVector2;
            }
            else if (type == ActiveUniformType.IntVec3)
            {
                return UniformType.IntVector3;
            }
            else if (type == ActiveUniformType.IntVec4)
            {
                return UniformType.IntVector4;
            }
            else if (type == ActiveUniformType.Bool)
            {
                return UniformType.Bool;
            }
            else if (type == ActiveUniformType.BoolVec2)
            {
                return UniformType.BoolVector2;
            }
            else if (type == ActiveUniformType.BoolVec3)
            {
                return UniformType.BoolVector3;
            }
            else if (type == ActiveUniformType.BoolVec4)
            {
                return UniformType.BoolVector4;
            }
            else if (type == ActiveUniformType.FloatMat2)
            {
                return UniformType.FloatMatrix22;
            }
            else if (type == ActiveUniformType.FloatMat3)
            {
                return UniformType.FloatMatrix33;
            }
            else if (type == ActiveUniformType.FloatMat4)
            {
                return UniformType.FloatMatrix44;
            }
            else if (type == ActiveUniformType.Sampler1D)
            {
                return UniformType.Sampler1D;
            }
            else if (type == ActiveUniformType.Sampler2D)
            {
                return UniformType.Sampler2D;
            }
            else if (type == ActiveUniformType.Sampler2DRect)
            {
                return UniformType.Sampler2DRectangle;
            }
            else if (type == ActiveUniformType.Sampler2DRectShadow)
            {
                return UniformType.Sampler2DRectangleShadow;
            }
            else if (type == ActiveUniformType.Sampler3D)
            {
                return UniformType.Sampler3D;
            }
            else if (type == ActiveUniformType.SamplerCube)
            {
                return UniformType.SamplerCube;
            }
            else if (type == ActiveUniformType.Sampler1DShadow)
            {
                return UniformType.Sampler1DShadow;
            }
            else if (type == ActiveUniformType.Sampler2DShadow)
            {
                return UniformType.Sampler2DShadow;
            }
            else if (type == ActiveUniformType.FloatMat2x3)
            {
                return UniformType.FloatMatrix23;
            }
            else if (type == ActiveUniformType.FloatMat2x4)
            {
                return UniformType.FloatMatrix24;
            }
            else if (type == ActiveUniformType.FloatMat3x2)
            {
                return UniformType.FloatMatrix32;
            }
            else if (type == ActiveUniformType.FloatMat3x4)
            {
                return UniformType.FloatMatrix34;
            }
            else if (type == ActiveUniformType.FloatMat4x2)
            {
                return UniformType.FloatMatrix42;
            }
            else if (type == ActiveUniformType.FloatMat4x3)
            {
                return UniformType.FloatMatrix43;
            }
            else if (type == ActiveUniformType.Sampler1DArray)
            {
                return UniformType.Sampler1DArray;
            }
            else if (type == ActiveUniformType.Sampler2DArray)
            {
                return UniformType.Sampler2DArray;
            }
            else if (type == ActiveUniformType.Sampler1DArrayShadow)
            {
                return UniformType.Sampler1DArrayShadow;
            }
            else if (type == ActiveUniformType.Sampler2DArrayShadow)
            {
                return UniformType.Sampler2DArrayShadow;
            }
            else if (type == ActiveUniformType.SamplerCubeShadow)
            {
                return UniformType.SamplerCubeShadow;
            }
            else if (type == ActiveUniformType.IntSampler1D)
            {
                return UniformType.IntSampler1D;
            }
            else if (type == ActiveUniformType.IntSampler2D)
            {
                return UniformType.IntSampler2D;
            }
            else if (type == ActiveUniformType.IntSampler2DRect)
            {
                return UniformType.IntSampler2DRectangle;
            }
            else if (type == ActiveUniformType.IntSampler3D)
            {
                return UniformType.IntSampler3D;
            }
            else if (type == ActiveUniformType.IntSamplerCube)
            {
                return UniformType.IntSamplerCube;
            }
            else if (type == ActiveUniformType.IntSampler1DArray)
            {
                return UniformType.IntSampler1DArray;
            }
            else if (type == ActiveUniformType.IntSampler2DArray)
            {
                return UniformType.IntSampler2DArray;
            }
            else if (type == ActiveUniformType.UnsignedIntSampler1D)
            {
                return UniformType.UnsignedIntSampler1D;
            }
            else if (type == ActiveUniformType.UnsignedIntSampler2D)
            {
                return UniformType.UnsignedIntSampler2D;
            }
            else if (type == ActiveUniformType.UnsignedIntSampler2DRect)
            {
                return UniformType.UnsignedIntSampler2DRectangle;
            }
            else if (type == ActiveUniformType.UnsignedIntSampler3D)
            {
                return UniformType.UnsignedIntSampler3D;
            }
            else if (type == ActiveUniformType.UnsignedIntSamplerCube)
            {
                return UniformType.UnsignedIntSamplerCube;
            }
            else if (type == ActiveUniformType.UnsignedIntSampler1DArray)
            {
                return UniformType.UnsignedIntSampler1DArray;
            }

            Debug.Assert(type == ActiveUniformType.UnsignedIntSampler2DArray);
            return UniformType.UnsignedIntSampler2DArray;
        }

        public static BufferHint To(BufferUsageHint hint)
        {
            if (hint == BufferUsageHint.StreamDraw)
            {
                return BufferHint.StreamDraw;
            }
            else if (hint == BufferUsageHint.StreamRead)
            {
                return BufferHint.StreamRead;
            }
            else if (hint == BufferUsageHint.StreamCopy)
            {
                return BufferHint.StreamCopy;
            }
            else if (hint == BufferUsageHint.StaticDraw)
            {
                return BufferHint.StaticDraw;
            }
            else if (hint == BufferUsageHint.StaticRead)
            {
                return BufferHint.StaticRead;
            }
            else if (hint == BufferUsageHint.StaticCopy)
            {
                return BufferHint.StaticCopy;
            }
            else if (hint == BufferUsageHint.DynamicDraw)
            {
                return BufferHint.DynamicDraw;
            }
            else if (hint == BufferUsageHint.DynamicRead)
            {
                return BufferHint.DynamicRead;
            }

            Debug.Assert(hint == BufferUsageHint.DynamicCopy);
            return BufferHint.DynamicCopy;
        }

        public static BufferUsageHint To(BufferHint hint)
        {
            if (hint == BufferHint.StreamDraw)
            {
                return BufferUsageHint.StreamDraw;
            }
            else if (hint == BufferHint.StreamRead)
            {
                return BufferUsageHint.StreamRead;
            }
            else if (hint == BufferHint.StreamCopy)
            {
                return BufferUsageHint.StreamCopy;
            }
            else if (hint == BufferHint.StaticDraw)
            {
                return BufferUsageHint.StaticDraw;
            }
            else if (hint == BufferHint.StaticRead)
            {
                return BufferUsageHint.StaticRead;
            }
            else if (hint == BufferHint.StaticCopy)
            {
                return BufferUsageHint.StaticCopy;
            }
            else if (hint == BufferHint.DynamicDraw)
            {
                return BufferUsageHint.DynamicDraw;
            }
            else if (hint == BufferHint.DynamicRead)
            {
                return BufferUsageHint.DynamicRead;
            }

            Debug.Assert(hint == BufferHint.DynamicCopy);
            return BufferUsageHint.DynamicCopy;
        }

        public static VertexAttribPointerType To(ComponentDatatype type)
        {
            if (type == ComponentDatatype.Byte)
            {
                return VertexAttribPointerType.Byte;
            }
            else if (type == ComponentDatatype.UnsignedByte)
            {
                return VertexAttribPointerType.UnsignedByte;
            }
            else if (type == ComponentDatatype.Short)
            {
                return VertexAttribPointerType.Short;
            }
            else if (type == ComponentDatatype.UnsignedShort)
            {
                return VertexAttribPointerType.UnsignedShort;
            }
            else if (type == ComponentDatatype.Int)
            {
                return VertexAttribPointerType.Int;
            }
            else if (type == ComponentDatatype.UnsignedInt)
            {
                return VertexAttribPointerType.UnsignedInt;
            }
            else if (type == ComponentDatatype.Float)
            {
                return VertexAttribPointerType.Float;
            }

            Debug.Assert(type == ComponentDatatype.HalfFloat);
            return VertexAttribPointerType.HalfFloat;
        }

        public static BeginMode To(PrimitiveType type)
        {
            if (type == PrimitiveType.Points)
            {
                return BeginMode.Points;
            }
            else if (type == PrimitiveType.Lines)
            {
                return BeginMode.Lines;
            }
            else if (type == PrimitiveType.LineLoop)
            {
                return BeginMode.LineLoop;
            }
            else if (type == PrimitiveType.LineStrip)
            {
                return BeginMode.LineStrip;
            }
            else if (type == PrimitiveType.Triangles)
            {
                return BeginMode.Triangles;
            }
            else if (type == PrimitiveType.TriangleStrip)
            {
                return BeginMode.TriangleStrip;
            }
            else if (type == PrimitiveType.LinesAdjacency)
            {
                return BeginMode.LinesAdjacency;
            }
            else if (type == PrimitiveType.LineStripAdjacency)
            {
                return BeginMode.LineStripAdjacency;
            }
            else if (type == PrimitiveType.TrianglesAdjacency)
            {
                return BeginMode.TrianglesAdjacency;
            }
            else if (type == PrimitiveType.TriangleStripAdjacency)
            {
                return BeginMode.TriangleStripAdjacency;
            }

            Debug.Assert(type == PrimitiveType.TriangleFan);
            return BeginMode.TriangleFan;
        }

        public static DrawElementsType To(IndexBufferDatatype type)
        {
            if (type == IndexBufferDatatype.UnsignedShort)
            {
                return DrawElementsType.UnsignedShort;
            }

            Debug.Assert(type == IndexBufferDatatype.UnsignedInt);
            return DrawElementsType.UnsignedInt;
        }

        public static DepthFunction To(DepthTestFunction function)
        {
            if (function == DepthTestFunction.Never)
            {
                return DepthFunction.Never;
            }
            else if (function == DepthTestFunction.Less)
            {
                return DepthFunction.Less;
            }
            else if (function == DepthTestFunction.Equal)
            {
                return DepthFunction.Equal;
            }
            else if (function == DepthTestFunction.LessThanOrEqual)
            {
                return DepthFunction.Lequal;
            }
            else if (function == DepthTestFunction.Greater)
            {
                return DepthFunction.Greater;
            }
            else if (function == DepthTestFunction.NotEqual)
            {
                return DepthFunction.Notequal;
            }
            else if (function == DepthTestFunction.GreaterThanOrEqual)
            {
                return DepthFunction.Gequal;
            }

            Debug.Assert(function == DepthTestFunction.Always);
            return DepthFunction.Always;
        }

        public static CullFaceMode To(CullFace face)
        {
            Debug.Assert(
                (face == CullFace.Front) ||
                (face == CullFace.Back) ||
                (face == CullFace.FrontAndBack));

            return _cullFaceModes[(int)face];
        }

        public static FrontFaceDirection To(WindingOrder windingOrder)
        {
            Debug.Assert(
                (windingOrder == WindingOrder.Clockwise) || 
                (windingOrder == WindingOrder.Counterclockwise));

            return _frontFaceDirections[(int)windingOrder];
        }

        public static PolygonMode To(RasterizationMode mode)
        {
            Debug.Assert(
                (mode == RasterizationMode.Point) ||
                (mode == RasterizationMode.Line) ||
                (mode == RasterizationMode.Fill));

            return _polygonModes[(int)mode];
        }

        public static StencilOp To(StencilOperation operation)
        {
            Debug.Assert(
                (operation == StencilOperation.Zero) ||
                (operation == StencilOperation.Invert) ||
                (operation == StencilOperation.Keep) ||
                (operation == StencilOperation.Replace) ||
                (operation == StencilOperation.Increment) ||
                (operation == StencilOperation.Decrement) ||
                (operation == StencilOperation.IncrementWrap) ||
                (operation == StencilOperation.DecrementWrap));

            return _stencilOp[(int)operation];
        }

        public static StencilFunction To(StencilTestFunction function)
        {
            Debug.Assert(
                (function == StencilTestFunction.Never) ||
                (function == StencilTestFunction.Less) ||
                (function == StencilTestFunction.Equal) ||
                (function == StencilTestFunction.LessThanOrEqual) ||
                (function == StencilTestFunction.Greater) ||
                (function == StencilTestFunction.NotEqual) ||
                (function == StencilTestFunction.GreaterThanOrEqual) ||
                (function == StencilTestFunction.Always));

            return _stencilFunction[(int)function];
        }

        public static BlendEquationMode To(BlendEquation equation)
        {
            Debug.Assert(
                (equation == BlendEquation.Add) ||
                (equation == BlendEquation.Minimum) ||
                (equation == BlendEquation.Maximum) ||
                (equation == BlendEquation.Subtract) ||
                (equation == BlendEquation.ReverseSubtract));
            
            return _blendEquationModes[(int)equation];
        }

        public static BlendingFactorSrc To(SourceBlendingFactor factor)
        {
            Debug.Assert(
                (factor == SourceBlendingFactor.Zero) ||
                (factor == SourceBlendingFactor.One) ||
                (factor == SourceBlendingFactor.SourceAlpha) ||
                (factor == SourceBlendingFactor.OneMinusSourceAlpha) ||
                (factor == SourceBlendingFactor.DestinationAlpha) ||
                (factor == SourceBlendingFactor.OneMinusDestinationAlpha) ||
                (factor == SourceBlendingFactor.DestinationColor) ||
                (factor == SourceBlendingFactor.OneMinusDestinationColor) ||
                (factor == SourceBlendingFactor.SourceAlphaSaturate) ||
                (factor == SourceBlendingFactor.ConstantColor) ||
                (factor == SourceBlendingFactor.OneMinusConstantColor) ||
                (factor == SourceBlendingFactor.ConstantAlpha) ||
                (factor == SourceBlendingFactor.OneMinusConstantAlpha));

            return _blendingFactorSrc[(int)factor];
        }

        public static BlendingFactorDest To(DestinationBlendingFactor factor)
        {
            Debug.Assert(
                (factor == DestinationBlendingFactor.Zero) ||
                (factor == DestinationBlendingFactor.One) ||
                (factor == DestinationBlendingFactor.SourceColor) ||
                (factor == DestinationBlendingFactor.OneMinusSourceColor) ||
                (factor == DestinationBlendingFactor.SourceAlpha) ||
                (factor == DestinationBlendingFactor.OneMinusSourceAlpha) ||
                (factor == DestinationBlendingFactor.DestinationAlpha) ||
                (factor == DestinationBlendingFactor.OneMinusDestinationAlpha) ||
                (factor == DestinationBlendingFactor.DestinationColor) ||
                (factor == DestinationBlendingFactor.OneMinusDestinationColor) ||
                (factor == DestinationBlendingFactor.ConstantColor) ||
                (factor == DestinationBlendingFactor.OneMinusConstantColor) ||
                (factor == DestinationBlendingFactor.ConstantAlpha) ||
                (factor == DestinationBlendingFactor.OneMinusConstantAlpha));

            return _blendingFactorDest[(int)factor];
        }

        public static PixelInternalFormat To(TextureFormat format)
        {
            Debug.Assert(
                (format == TextureFormat.RedGreenBlue8) ||
                (format == TextureFormat.RedGreenBlue16) ||
                (format == TextureFormat.RedGreenBlueAlpha8) ||
                (format == TextureFormat.RedGreenBlue10A2) ||
                (format == TextureFormat.RedGreenBlueAlpha16) ||
                (format == TextureFormat.Depth16) ||
                (format == TextureFormat.Depth24) ||
                (format == TextureFormat.Red8) ||
                (format == TextureFormat.Red16) ||
                (format == TextureFormat.RedGreen8) ||
                (format == TextureFormat.RedGreen16) ||
                (format == TextureFormat.Red16f) ||
                (format == TextureFormat.Red32f) ||
                (format == TextureFormat.RedGreen16f) ||
                (format == TextureFormat.RedGreen32f) ||
                (format == TextureFormat.Red8i) ||
                (format == TextureFormat.Red8ui) ||
                (format == TextureFormat.Red16i) ||
                (format == TextureFormat.Red16ui) ||
                (format == TextureFormat.Red32i) ||
                (format == TextureFormat.Red32ui) ||
                (format == TextureFormat.RedGreen8i) ||
                (format == TextureFormat.RedGreen8ui) ||
                (format == TextureFormat.RedGreen16i) ||
                (format == TextureFormat.RedGreen16ui) ||
                (format == TextureFormat.RedGreen32i) ||
                (format == TextureFormat.RedGreen32ui) ||
                (format == TextureFormat.RedGreenBlueAlpha32f) ||
                (format == TextureFormat.RedGreenBlue32f) ||
                (format == TextureFormat.RedGreenBlueAlpha16f) ||
                (format == TextureFormat.RedGreenBlue16f) ||
                (format == TextureFormat.Depth24Stencil8) ||
                (format == TextureFormat.Red11fGreen11fBlue10f) ||
                (format == TextureFormat.RedGreenBlue9E5) ||
                (format == TextureFormat.SRedGreenBlue8) ||
                (format == TextureFormat.SRedGreenBlue8Alpha8) ||
                (format == TextureFormat.Depth32f) ||
                (format == TextureFormat.Depth32fStencil8) ||
                (format == TextureFormat.RedGreenBlueAlpha32ui) ||
                (format == TextureFormat.RedGreenBlue32ui) ||
                (format == TextureFormat.RedGreenBlueAlpha16ui) ||
                (format == TextureFormat.RedGreenBlue16ui) ||
                (format == TextureFormat.RedGreenBlueAlpha8ui) ||
                (format == TextureFormat.RedGreenBlue8ui) ||
                (format == TextureFormat.RedGreenBlueAlpha32i) ||
                (format == TextureFormat.RedGreenBlue32i) ||
                (format == TextureFormat.RedGreenBlueAlpha16i) ||
                (format == TextureFormat.RedGreenBlue16i) ||
                (format == TextureFormat.RedGreenBlueAlpha8i) ||
                (format == TextureFormat.RedGreenBlue8i));

            return _pixelInternalFormat[(int)format];
        }

        public static PixelFormat To(ImageFormat format)
        {
            Debug.Assert(
                (format == ImageFormat.StencilIndex) ||
                (format == ImageFormat.DepthComponent) ||
                (format == ImageFormat.Red) ||
                (format == ImageFormat.Green) ||
                (format == ImageFormat.Blue) ||
                (format == ImageFormat.RedGreenBlue) ||
                (format == ImageFormat.RedGreenBlueAlpha) ||
                (format == ImageFormat.BlueGreenRed) ||
                (format == ImageFormat.BlueGreenRedAlpha) ||
                (format == ImageFormat.RedGreen) ||
                (format == ImageFormat.RedGreenInteger) ||
                (format == ImageFormat.DepthStencil) ||
                (format == ImageFormat.RedInteger) ||
                (format == ImageFormat.GreenInteger) ||
                (format == ImageFormat.BlueInteger) ||
                (format == ImageFormat.RedGreenBlueInteger) ||
                (format == ImageFormat.RedGreenBlueAlphaInteger) ||
                (format == ImageFormat.BlueGreenRedInteger) ||
                (format == ImageFormat.BlueGreenRedAlphaInteger));

            return _pixelFormat[(int)format];
        }

        public static PixelType To(ImageDatatype type)
        {
            Debug.Assert(
                (type == ImageDatatype.Byte) ||
                (type == ImageDatatype.UnsignedByte) ||
                (type == ImageDatatype.Short) ||
                (type == ImageDatatype.UnsignedShort) ||
                (type == ImageDatatype.Int) ||
                (type == ImageDatatype.UnsignedInt) ||
                (type == ImageDatatype.Float) ||
                (type == ImageDatatype.HalfFloat) ||
                (type == ImageDatatype.UnsignedByte332) ||
                (type == ImageDatatype.UnsignedShort4444) ||
                (type == ImageDatatype.UnsignedShort5551) ||
                (type == ImageDatatype.UnsignedInt8888) ||
                (type == ImageDatatype.UnsignedInt1010102) ||
                (type == ImageDatatype.UnsignedByte233Reversed) ||
                (type == ImageDatatype.UnsignedShort565) ||
                (type == ImageDatatype.UnsignedShort565Reversed) ||
                (type == ImageDatatype.UnsignedShort4444Reversed) ||
                (type == ImageDatatype.UnsignedShort1555Reversed) ||
                (type == ImageDatatype.UnsignedInt8888Reversed) ||
                (type == ImageDatatype.UnsignedInt2101010Reversed) ||
                (type == ImageDatatype.UnsignedInt248) ||
                (type == ImageDatatype.UnsignedInt10F11F11FReversed) ||
                (type == ImageDatatype.UnsignedInt5999Reversed) ||
                (type == ImageDatatype.Float32UnsignedInt248Reversed));

            return _pixelType[(int)type];
        }

        public static PixelFormat TextureToPixelFormat(TextureFormat textureFormat)
        {
            // TODO:  Not tested exhaustively
            Debug.Assert(IsTextureFormatValid(textureFormat));
            return _textureToPixelFormats[(int)textureFormat];
        }

        public static PixelType TextureToPixelType(TextureFormat textureFormat)
        {
            // TODO:  Not tested exhaustively
            Debug.Assert(IsTextureFormatValid(textureFormat));
            return _textureToPixelTypes[(int)textureFormat];
        }

        private static bool IsTextureFormatValid(TextureFormat textureFormat)
        {
            return (textureFormat == TextureFormat.RedGreenBlue8) ||
                   (textureFormat == TextureFormat.RedGreenBlue16) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha8) ||
                   (textureFormat == TextureFormat.RedGreenBlue10A2) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha16) ||
                   (textureFormat == TextureFormat.Depth16) ||
                   (textureFormat == TextureFormat.Depth24) ||
                   (textureFormat == TextureFormat.Red8) ||
                   (textureFormat == TextureFormat.Red16) ||
                   (textureFormat == TextureFormat.RedGreen8) ||
                   (textureFormat == TextureFormat.RedGreen16) ||
                   (textureFormat == TextureFormat.Red16f) ||
                   (textureFormat == TextureFormat.Red32f) ||
                   (textureFormat == TextureFormat.RedGreen16f) ||
                   (textureFormat == TextureFormat.RedGreen32f) ||
                   (textureFormat == TextureFormat.Red8i) ||
                   (textureFormat == TextureFormat.Red8ui) ||
                   (textureFormat == TextureFormat.Red16i) ||
                   (textureFormat == TextureFormat.Red16ui) ||
                   (textureFormat == TextureFormat.Red32i) ||
                   (textureFormat == TextureFormat.Red32ui) ||
                   (textureFormat == TextureFormat.RedGreen8i) ||
                   (textureFormat == TextureFormat.RedGreen8ui) ||
                   (textureFormat == TextureFormat.RedGreen16i) ||
                   (textureFormat == TextureFormat.RedGreen16ui) ||
                   (textureFormat == TextureFormat.RedGreen32i) ||
                   (textureFormat == TextureFormat.RedGreen32ui) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha32f) ||
                   (textureFormat == TextureFormat.RedGreenBlue32f) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha16f) ||
                   (textureFormat == TextureFormat.RedGreenBlue16f) ||
                   (textureFormat == TextureFormat.Depth24Stencil8) ||
                   (textureFormat == TextureFormat.Red11fGreen11fBlue10f) ||
                   (textureFormat == TextureFormat.RedGreenBlue9E5) ||
                   (textureFormat == TextureFormat.SRedGreenBlue8) ||
                   (textureFormat == TextureFormat.SRedGreenBlue8Alpha8) ||
                   (textureFormat == TextureFormat.Depth32f) ||
                   (textureFormat == TextureFormat.Depth32fStencil8) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha32ui) ||
                   (textureFormat == TextureFormat.RedGreenBlue32ui) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha16ui) ||
                   (textureFormat == TextureFormat.RedGreenBlue16ui) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha8ui) ||
                   (textureFormat == TextureFormat.RedGreenBlue8ui) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha32i) ||
                   (textureFormat == TextureFormat.RedGreenBlue32i) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha16i) ||
                   (textureFormat == TextureFormat.RedGreenBlue16i) ||
                   (textureFormat == TextureFormat.RedGreenBlueAlpha8i) ||
                   (textureFormat == TextureFormat.RedGreenBlue8i);
        }

        public static TextureMinFilter To(TextureMinificationFilter filter)
        {
            Debug.Assert(
                (filter == TextureMinificationFilter.Nearest) ||
                (filter == TextureMinificationFilter.Linear) ||
                (filter == TextureMinificationFilter.NearestMipmapNearest) ||
                (filter == TextureMinificationFilter.LinearMipmapNearest) ||
                (filter == TextureMinificationFilter.NearestMipmapLinear) ||
                (filter == TextureMinificationFilter.LinearMipmapLinear));

            return _textureMinFilter[(int)filter];
        }

        public static TextureMagFilter To(TextureMagnificationFilter filter)
        {
            Debug.Assert(
                (filter == TextureMagnificationFilter.Nearest) ||
                (filter == TextureMagnificationFilter.Linear));

            return _textureMagFilter[(int)filter];
        }

        public static TextureWrapMode To(TextureWrap wrap)
        {
            Debug.Assert(
                (wrap == TextureWrap.Clamp) ||
                (wrap == TextureWrap.Repeat) ||
                (wrap == TextureWrap.MirroredRepeat));

            return _textureWrapMode[(int)wrap];
        }

        private static CullFaceMode[] _cullFaceModes =
        {
            CullFaceMode.Front,
            CullFaceMode.Back,
            CullFaceMode.FrontAndBack
        };

        private static FrontFaceDirection[] _frontFaceDirections =
        {
            FrontFaceDirection.Cw,
            FrontFaceDirection.Ccw
        };

        private static PolygonMode[] _polygonModes =
        {
            PolygonMode.Point,
            PolygonMode.Line,
            PolygonMode.Fill
        };

        private static StencilOp[] _stencilOp =
        {
            StencilOp.Zero,
            StencilOp.Invert,
            StencilOp.Keep,
            StencilOp.Replace,
            StencilOp.Incr,
            StencilOp.Decr,
            StencilOp.IncrWrap,
            StencilOp.DecrWrap
        };

        private static StencilFunction[] _stencilFunction =
        {
            StencilFunction.Never,
            StencilFunction.Less,
            StencilFunction.Equal,
            StencilFunction.Lequal,
            StencilFunction.Greater,
            StencilFunction.Notequal,
            StencilFunction.Gequal,
            StencilFunction.Always
        };

        private static BlendEquationMode[] _blendEquationModes =
        {
            BlendEquationMode.FuncAdd,
            BlendEquationMode.Min,
            BlendEquationMode.Max,
            BlendEquationMode.FuncSubtract,
            BlendEquationMode.FuncReverseSubtract
        };

        private static BlendingFactorSrc[] _blendingFactorSrc =
        {
            BlendingFactorSrc.Zero,
            BlendingFactorSrc.One,
            BlendingFactorSrc.SrcAlpha,
            BlendingFactorSrc.OneMinusSrcAlpha,
            BlendingFactorSrc.DstAlpha,
            BlendingFactorSrc.OneMinusDstAlpha,
            BlendingFactorSrc.DstColor,
            BlendingFactorSrc.OneMinusDstColor,
            BlendingFactorSrc.SrcAlphaSaturate,
            BlendingFactorSrc.ConstantColor,
            BlendingFactorSrc.OneMinusConstantColor,
            BlendingFactorSrc.ConstantAlpha,
            BlendingFactorSrc.OneMinusConstantAlpha
        };

        private static BlendingFactorDest[] _blendingFactorDest =
        {
            BlendingFactorDest.Zero,
            BlendingFactorDest.One,
            BlendingFactorDest.SrcColor,
            BlendingFactorDest.OneMinusSrcColor,
            BlendingFactorDest.SrcAlpha,
            BlendingFactorDest.OneMinusSrcAlpha,
            BlendingFactorDest.DstAlpha,
            BlendingFactorDest.OneMinusDstAlpha,
            BlendingFactorDest.DstColor,
            BlendingFactorDest.OneMinusDstColor,
            BlendingFactorDest.ConstantColor,
            BlendingFactorDest.OneMinusConstantColor,
            BlendingFactorDest.ConstantAlpha,
            BlendingFactorDest.OneMinusConstantAlpha
        };

        private static PixelInternalFormat[] _pixelInternalFormat = 
        {
            PixelInternalFormat.Rgb8,
            PixelInternalFormat.Rgb16,
            PixelInternalFormat.Rgba8,
            PixelInternalFormat.Rgb10A2,
            PixelInternalFormat.Rgba16,
            PixelInternalFormat.DepthComponent16,
            PixelInternalFormat.DepthComponent24,
            PixelInternalFormat.R8,
            PixelInternalFormat.R16,
            PixelInternalFormat.Rg8,
            PixelInternalFormat.Rg16,
            PixelInternalFormat.R16f,
            PixelInternalFormat.R32f,
            PixelInternalFormat.Rg16f,
            PixelInternalFormat.Rg32f,
            PixelInternalFormat.R8i,
            PixelInternalFormat.R8ui,
            PixelInternalFormat.R16i,
            PixelInternalFormat.R16ui,
            PixelInternalFormat.R32i,
            PixelInternalFormat.R32ui,
            PixelInternalFormat.Rg8i,
            PixelInternalFormat.Rg8ui,
            PixelInternalFormat.Rg16i,
            PixelInternalFormat.Rg16ui,
            PixelInternalFormat.Rg32i,
            PixelInternalFormat.Rg32ui,
            PixelInternalFormat.Rgba32f,
            PixelInternalFormat.Rgb32f,
            PixelInternalFormat.Rgba16f,
            PixelInternalFormat.Rgb16f,
            PixelInternalFormat.Depth24Stencil8,
            PixelInternalFormat.R11fG11fB10f,
            PixelInternalFormat.Rgb9E5,
            PixelInternalFormat.Srgb8,
            PixelInternalFormat.Srgb8Alpha8,
            PixelInternalFormat.DepthComponent32f,
            PixelInternalFormat.Depth32fStencil8,
            PixelInternalFormat.Rgba32ui,
            PixelInternalFormat.Rgb32ui,
            PixelInternalFormat.Rgba16ui,
            PixelInternalFormat.Rgb16ui,
            PixelInternalFormat.Rgba8ui,
            PixelInternalFormat.Rgb8ui,
            PixelInternalFormat.Rgba32i,
            PixelInternalFormat.Rgb32i,
            PixelInternalFormat.Rgba16i,
            PixelInternalFormat.Rgb16i,
            PixelInternalFormat.Rgba8i,
            PixelInternalFormat.Rgb8i
        };

        private static PixelFormat[] _pixelFormat = 
        {
            PixelFormat.StencilIndex,
            PixelFormat.DepthComponent,
            PixelFormat.Red,
            PixelFormat.Green,
            PixelFormat.Blue,
            PixelFormat.Rgb,
            PixelFormat.Rgba,
            PixelFormat.Bgr,
            PixelFormat.Bgra,
            PixelFormat.Rg,
            PixelFormat.RgInteger,
            PixelFormat.DepthStencil,
            PixelFormat.RedInteger,
            PixelFormat.GreenInteger,
            PixelFormat.BlueInteger,
            PixelFormat.RgbInteger,
            PixelFormat.RgbaInteger,
            PixelFormat.BgrInteger,
            PixelFormat.BgraInteger
        };

        private static PixelType[] _pixelType = 
        {
            PixelType.Byte,
            PixelType.UnsignedByte,
            PixelType.Short,
            PixelType.UnsignedShort,
            PixelType.Int,
            PixelType.UnsignedInt,
            PixelType.Float,
            PixelType.HalfFloat,
            PixelType.UnsignedByte332,
            PixelType.UnsignedShort4444,
            PixelType.UnsignedShort5551,
            PixelType.UnsignedInt8888,
            PixelType.UnsignedInt1010102,
            PixelType.UnsignedByte233Reversed,
            PixelType.UnsignedShort565,
            PixelType.UnsignedShort565Reversed,
            PixelType.UnsignedShort4444Reversed,
            PixelType.UnsignedShort1555Reversed,
            PixelType.UnsignedInt8888Reversed,
            PixelType.UnsignedInt2101010Reversed,
            PixelType.UnsignedInt248,
            PixelType.UnsignedInt10F11F11FRev,
            PixelType.UnsignedInt5999Rev,
            PixelType.Float32UnsignedInt248Rev
        };

        static readonly PixelFormat[] _textureToPixelFormats = new PixelFormat[]
        {
            PixelFormat.Rgb,            // TextureFormat.RedGreenBlue8
            PixelFormat.Rgb,            // TextureFormat.RedGreenBlue16
            PixelFormat.Rgba,           // TextureFormat.RedGreenBlueAlpha8
            PixelFormat.Rgba,           // TextureFormat.RedGreenBlue10A2
            PixelFormat.Rgba,           // TextureFormat.RedGreenBlueAlpha16
            PixelFormat.DepthComponent, // TextureFormat.Depth16
            PixelFormat.DepthComponent, // TextureFormat.Depth24
            PixelFormat.Red,            // TextureFormat.Red8
            PixelFormat.Red,            // TextureFormat.Red16
            PixelFormat.Rg,             // TextureFormat.RedGreen8
            PixelFormat.Rg,             // TextureFormat.RedGreen16
            PixelFormat.Red,            // TextureFormat.Red16f
            PixelFormat.Red,            // TextureFormat.Red32f
            PixelFormat.Rg,             // TextureFormat.RedGreen16f
            PixelFormat.Rg,             // TextureFormat.RedGreen32f
            PixelFormat.RedInteger,     // TextureFormat.Red8i
            PixelFormat.RedInteger,     // TextureFormat.Red8ui
            PixelFormat.RedInteger,     // TextureFormat.Red16i
            PixelFormat.RedInteger,     // TextureFormat.Red16ui
            PixelFormat.RedInteger,     // TextureFormat.Red32i
            PixelFormat.RedInteger,     // TextureFormat.Red32ui
            PixelFormat.RgInteger,      // TextureFormat.RedGreen8i
            PixelFormat.RgInteger,      // TextureFormat.RedGreen8ui
            PixelFormat.RgInteger,      // TextureFormat.RedGreen16i
            PixelFormat.RgInteger,      // TextureFormat.RedGreen16ui
            PixelFormat.RgInteger,      // TextureFormat.RedGreen32i
            PixelFormat.RgInteger,      // TextureFormat.RedGreen32ui
            PixelFormat.Rgba,           // TextureFormat.RedGreenBlueAlpha32f
            PixelFormat.Rgb,            // TextureFormat.RedGreenBlue32f
            PixelFormat.Rgba,           // TextureFormat.RedGreenBlueAlpha16f
            PixelFormat.Rgb,            // TextureFormat.RedGreenBlue16f
            PixelFormat.DepthStencil,   // TextureFormat.Depth24Stencil8
            PixelFormat.Rgb,            // TextureFormat.Red11fGreen11fBlue10f
            PixelFormat.Rgb,            // TextureFormat.RedGreenBlue9E5
            PixelFormat.RgbInteger,     // TextureFormat.SRedGreenBlue8
            PixelFormat.RgbaInteger,    // TextureFormat.SRedGreenBlue8Alpha8
            PixelFormat.DepthComponent, // TextureFormat.Depth32f
            PixelFormat.DepthStencil,   // TextureFormat.Depth32fStencil8
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha32ui
            PixelFormat.RgbInteger,     // TextureFormat.RedGreenBlue32ui
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha16ui
            PixelFormat.RgbInteger,     // TextureFormat.RedGreenBlue16ui
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha8ui
            PixelFormat.RgbInteger,     // TextureFormat.RedGreenBlue8ui
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha32i
            PixelFormat.RgbInteger,     // TextureFormat.RedGreenBlue32i
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha16i
            PixelFormat.RgbInteger,     // TextureFormat.RedGreenBlue16i
            PixelFormat.RgbaInteger,    // TextureFormat.RedGreenBlueAlpha8i
            PixelFormat.RgbInteger      // TextureFormat.RedGreenBlue8i
        };

        static readonly PixelType[] _textureToPixelTypes = new PixelType[]
        {
            PixelType.UnsignedByte,         // TextureFormat.RedGreenBlue8
            PixelType.UnsignedShort,        // TextureFormat.RedGreenBlue16
            PixelType.UnsignedByte,         // TextureFormat.RedGreenBlueAlpha8
            PixelType.UnsignedInt1010102,   // TextureFormat.RedGreenBlue10A2
            PixelType.UnsignedInt,          // TextureFormat.RedGreenBlueAlpha16
            PixelType.HalfFloat,            // TextureFormat.Depth16
            PixelType.Float,                // TextureFormat.Depth24
            PixelType.UnsignedByte,         // TextureFormat.Red8
            PixelType.UnsignedShort,        // TextureFormat.Red16
            PixelType.UnsignedByte,         // TextureFormat.RedGreen8
            PixelType.UnsignedShort,        // TextureFormat.RedGreen16
            PixelType.HalfFloat,            // TextureFormat.Red16f
            PixelType.Float,                // TextureFormat.Red32f
            PixelType.HalfFloat,            // TextureFormat.RedGreen16f
            PixelType.Float,                // TextureFormat.RedGreen32f
            PixelType.Byte,                 // TextureFormat.Red8i
            PixelType.UnsignedByte,         // TextureFormat.Red8ui
            PixelType.Short,                // TextureFormat.Red16i
            PixelType.UnsignedShort,        // TextureFormat.Red16ui
            PixelType.Int,                  // TextureFormat.Red32i
            PixelType.UnsignedInt,          // TextureFormat.Red32ui
            PixelType.Byte,                 // TextureFormat.RedGreen8i
            PixelType.UnsignedByte,         // TextureFormat.RedGreen8ui
            PixelType.Short,                // TextureFormat.RedGreen16i
            PixelType.UnsignedShort,        // TextureFormat.RedGreen16ui
            PixelType.Int,                  // TextureFormat.RedGreen32i
            PixelType.UnsignedInt,          // TextureFormat.RedGreen32ui
            PixelType.Float,                // TextureFormat.RedGreenBlueAlpha32f
            PixelType.Float,                // TextureFormat.RedGreenBlue32f
            PixelType.HalfFloat,            // TextureFormat.RedGreenBlueAlpha16f
            PixelType.HalfFloat,            // TextureFormat.RedGreenBlue16f
            PixelType.UnsignedInt248,       // TextureFormat.Depth24Stencil8
            PixelType.Float,                // TextureFormat.Red11fGreen11fBlue10f
            PixelType.Float,                // TextureFormat.RedGreenBlue9E5
            PixelType.Byte,                 // TextureFormat.SRedGreenBlue8
            PixelType.Byte,                 // TextureFormat.SRedGreenBlue8Alpha8
            PixelType.Float,                // TextureFormat.Depth32f
            PixelType.Float,                // TextureFormat.Depth32fStencil8
            PixelType.UnsignedInt,          // TextureFormat.RedGreenBlueAlpha32ui
            PixelType.UnsignedInt,          // TextureFormat.RedGreenBlue32ui
            PixelType.UnsignedShort,        // TextureFormat.RedGreenBlueAlpha16ui
            PixelType.UnsignedShort,        // TextureFormat.RedGreenBlue16ui
            PixelType.UnsignedByte,         // TextureFormat.RedGreenBlueAlpha8ui
            PixelType.UnsignedByte,         // TextureFormat.RedGreenBlue8ui
            PixelType.UnsignedInt,          // TextureFormat.RedGreenBlueAlpha32i
            PixelType.UnsignedInt,          // TextureFormat.RedGreenBlue32i
            PixelType.UnsignedShort,        // TextureFormat.RedGreenBlueAlpha16i
            PixelType.UnsignedShort,        // TextureFormat.RedGreenBlue16i
            PixelType.UnsignedByte,         // TextureFormat.RedGreenBlueAlpha8i
            PixelType.UnsignedByte          // TextureFormat.RedGreenBlue8i
        };

        private static TextureMinFilter[] _textureMinFilter = 
        {
            TextureMinFilter.Nearest,
            TextureMinFilter.Linear,
            TextureMinFilter.NearestMipmapNearest,
            TextureMinFilter.LinearMipmapNearest,
            TextureMinFilter.NearestMipmapLinear,
            TextureMinFilter.LinearMipmapLinear,
        };

        private static TextureMagFilter[] _textureMagFilter = 
        {
            TextureMagFilter.Nearest,
            TextureMagFilter.Linear
        };

        private static TextureWrapMode[] _textureWrapMode = 
        {
            TextureWrapMode.ClampToEdge,
            TextureWrapMode.Repeat,
            TextureWrapMode.MirroredRepeat
        };
    }
}
