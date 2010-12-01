#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
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
            switch (type)
            {
                case ActiveAttribType.Float:
                    return ShaderVertexAttributeType.Float;
                case ActiveAttribType.FloatVec2:
                    return ShaderVertexAttributeType.FloatVector2;
                case ActiveAttribType.FloatVec3:
                    return ShaderVertexAttributeType.FloatVector3;
                case ActiveAttribType.FloatVec4:
                    return ShaderVertexAttributeType.FloatVector4;
                case ActiveAttribType.FloatMat2:
                    return ShaderVertexAttributeType.FloatMatrix22;
                case ActiveAttribType.FloatMat3:
                    return ShaderVertexAttributeType.FloatMatrix33;
                case ActiveAttribType.FloatMat4:
                    return ShaderVertexAttributeType.FloatMatrix44;
                case (ActiveAttribType)All.Int:
                    return ShaderVertexAttributeType.Int;
                case (ActiveAttribType)All.IntVec2:
                    return ShaderVertexAttributeType.IntVector2;
                case (ActiveAttribType)All.IntVec3:
                    return ShaderVertexAttributeType.IntVector3;
                case (ActiveAttribType)All.IntVec4:
                    return ShaderVertexAttributeType.IntVector4;
            }

            throw new ArgumentException("type");
        }

        public static UniformType To(ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.Int:
                    return UniformType.Int;
                case ActiveUniformType.Float:
                    return UniformType.Float;
                case ActiveUniformType.FloatVec2:
                    return UniformType.FloatVector2;
                case ActiveUniformType.FloatVec3:
                    return UniformType.FloatVector3;
                case ActiveUniformType.FloatVec4:
                    return UniformType.FloatVector4;
                case ActiveUniformType.IntVec2:
                    return UniformType.IntVector2;
                case ActiveUniformType.IntVec3:
                    return UniformType.IntVector3;
                case ActiveUniformType.IntVec4:
                    return UniformType.IntVector4;
                case ActiveUniformType.Bool:
                    return UniformType.Bool;
                case ActiveUniformType.BoolVec2:
                    return UniformType.BoolVector2;
                case ActiveUniformType.BoolVec3:
                    return UniformType.BoolVector3;
                case ActiveUniformType.BoolVec4:
                    return UniformType.BoolVector4;
                case ActiveUniformType.FloatMat2:
                    return UniformType.FloatMatrix22;
                case ActiveUniformType.FloatMat3:
                    return UniformType.FloatMatrix33;
                case ActiveUniformType.FloatMat4:
                    return UniformType.FloatMatrix44;
                case ActiveUniformType.Sampler1D:
                    return UniformType.Sampler1D;
                case ActiveUniformType.Sampler2D:
                    return UniformType.Sampler2D;
                case ActiveUniformType.Sampler2DRect:
                    return UniformType.Sampler2DRectangle;
                case ActiveUniformType.Sampler2DRectShadow:
                    return UniformType.Sampler2DRectangleShadow;
                case ActiveUniformType.Sampler3D:
                    return UniformType.Sampler3D;
                case ActiveUniformType.SamplerCube:
                    return UniformType.SamplerCube;
                case ActiveUniformType.Sampler1DShadow:
                    return UniformType.Sampler1DShadow;
                case ActiveUniformType.Sampler2DShadow:
                    return UniformType.Sampler2DShadow;
                case ActiveUniformType.FloatMat2x3:
                    return UniformType.FloatMatrix23;
                case ActiveUniformType.FloatMat2x4:
                    return UniformType.FloatMatrix24;
                case ActiveUniformType.FloatMat3x2:
                    return UniformType.FloatMatrix32;
                case ActiveUniformType.FloatMat3x4:
                    return UniformType.FloatMatrix34;
                case ActiveUniformType.FloatMat4x2:
                    return UniformType.FloatMatrix42;
                case ActiveUniformType.FloatMat4x3:
                    return UniformType.FloatMatrix43;
                case ActiveUniformType.Sampler1DArray:
                    return UniformType.Sampler1DArray;
                case ActiveUniformType.Sampler2DArray:
                    return UniformType.Sampler2DArray;
                case ActiveUniformType.Sampler1DArrayShadow:
                    return UniformType.Sampler1DArrayShadow;
                case ActiveUniformType.Sampler2DArrayShadow:
                    return UniformType.Sampler2DArrayShadow;
                case ActiveUniformType.SamplerCubeShadow:
                    return UniformType.SamplerCubeShadow;
                case ActiveUniformType.IntSampler1D:
                    return UniformType.IntSampler1D;
                case ActiveUniformType.IntSampler2D:
                    return UniformType.IntSampler2D;
                case ActiveUniformType.IntSampler2DRect:
                    return UniformType.IntSampler2DRectangle;
                case ActiveUniformType.IntSampler3D:
                    return UniformType.IntSampler3D;
                case ActiveUniformType.IntSamplerCube:
                    return UniformType.IntSamplerCube;
                case ActiveUniformType.IntSampler1DArray:
                    return UniformType.IntSampler1DArray;
                case ActiveUniformType.IntSampler2DArray:
                    return UniformType.IntSampler2DArray;
                case ActiveUniformType.UnsignedIntSampler1D:
                    return UniformType.UnsignedIntSampler1D;
                case ActiveUniformType.UnsignedIntSampler2D:
                    return UniformType.UnsignedIntSampler2D;
                case ActiveUniformType.UnsignedIntSampler2DRect:
                    return UniformType.UnsignedIntSampler2DRectangle;
                case ActiveUniformType.UnsignedIntSampler3D:
                    return UniformType.UnsignedIntSampler3D;
                case ActiveUniformType.UnsignedIntSamplerCube:
                    return UniformType.UnsignedIntSamplerCube;
                case ActiveUniformType.UnsignedIntSampler1DArray:
                    return UniformType.UnsignedIntSampler1DArray;
            }

            throw new ArgumentException("type");
        }

        public static BufferHint To(BufferUsageHint hint)
        {
            switch (hint)
            {
                case BufferUsageHint.StreamDraw:
                    return BufferHint.StreamDraw;
                case BufferUsageHint.StreamRead:
                    return BufferHint.StreamRead;
                case BufferUsageHint.StreamCopy:
                    return BufferHint.StreamCopy;
                case BufferUsageHint.StaticDraw:
                    return BufferHint.StaticDraw;
                case BufferUsageHint.StaticRead:
                    return BufferHint.StaticRead;
                case BufferUsageHint.StaticCopy:
                    return BufferHint.StaticCopy;
                case BufferUsageHint.DynamicDraw:
                    return BufferHint.DynamicDraw;
                case BufferUsageHint.DynamicRead:
                    return BufferHint.DynamicRead;
                case BufferUsageHint.DynamicCopy:
                    return BufferHint.DynamicCopy;
            }

            throw new ArgumentException("type");
        }

        public static BufferUsageHint To(BufferHint hint)
        {
            switch (hint)
            {
                case BufferHint.StreamDraw:
                    return BufferUsageHint.StreamDraw;
                case BufferHint.StreamRead:
                    return BufferUsageHint.StreamRead;
                case BufferHint.StreamCopy:
                    return BufferUsageHint.StreamCopy;
                case BufferHint.StaticDraw:
                    return BufferUsageHint.StaticDraw;
                case BufferHint.StaticRead:
                    return BufferUsageHint.StaticRead;
                case BufferHint.StaticCopy:
                    return BufferUsageHint.StaticCopy;
                case BufferHint.DynamicDraw:
                    return BufferUsageHint.DynamicDraw;
                case BufferHint.DynamicRead:
                    return BufferUsageHint.DynamicRead;
                case BufferHint.DynamicCopy:
                    return BufferUsageHint.DynamicCopy;
            }

            throw new ArgumentException("type");
        }

        public static VertexAttribPointerType To(ComponentDatatype type)
        {
            switch (type)
            {
                case ComponentDatatype.Byte:
                    return VertexAttribPointerType.Byte;
                case ComponentDatatype.UnsignedByte:
                    return VertexAttribPointerType.UnsignedByte;
                case ComponentDatatype.Short:
                    return VertexAttribPointerType.Short;
                case ComponentDatatype.UnsignedShort:
                    return VertexAttribPointerType.UnsignedShort;
                case ComponentDatatype.Int:
                    return VertexAttribPointerType.Int;
                case ComponentDatatype.UnsignedInt:
                    return VertexAttribPointerType.UnsignedInt;
                case ComponentDatatype.Float:
                    return VertexAttribPointerType.Float;
                case ComponentDatatype.HalfFloat:
                    return VertexAttribPointerType.HalfFloat;
            }

            throw new ArgumentException("type");
        }

        public static BeginMode To(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Points:
                    return BeginMode.Points;
                case PrimitiveType.Lines:
                    return BeginMode.Lines;
                case PrimitiveType.LineLoop:
                    return BeginMode.LineLoop;
                case PrimitiveType.LineStrip:
                    return BeginMode.LineStrip;
                case PrimitiveType.Triangles:
                    return BeginMode.Triangles;
                case PrimitiveType.TriangleStrip:
                    return BeginMode.TriangleStrip;
                case PrimitiveType.LinesAdjacency:
                    return BeginMode.LinesAdjacency;;
                case PrimitiveType.LineStripAdjacency:
                    return BeginMode.LineStripAdjacency;
                case PrimitiveType.TrianglesAdjacency:
                    return BeginMode.TrianglesAdjacency;
                case PrimitiveType.TriangleStripAdjacency:
                    return BeginMode.TriangleStripAdjacency;
                case PrimitiveType.TriangleFan:
                    return BeginMode.TriangleFan;
            }

            throw new ArgumentException("type");
        }

        public static DrawElementsType To(IndexBufferDatatype type)
        {
            switch (type)
            {
                case IndexBufferDatatype.UnsignedShort:
                    return DrawElementsType.UnsignedShort;
                case IndexBufferDatatype.UnsignedInt:
                    return DrawElementsType.UnsignedInt;
            }

            throw new ArgumentException("type");
        }

        public static DepthFunction To(DepthTestFunction function)
        {
            switch (function)
            {
                case DepthTestFunction.Never:
                    return DepthFunction.Never;
                case DepthTestFunction.Less:
                    return DepthFunction.Less;
                case DepthTestFunction.Equal:
                    return DepthFunction.Equal;
                case DepthTestFunction.LessThanOrEqual:
                    return DepthFunction.Lequal;
                case DepthTestFunction.Greater:
                    return DepthFunction.Greater;
                case DepthTestFunction.NotEqual:
                    return DepthFunction.Notequal;
                case DepthTestFunction.GreaterThanOrEqual:
                    return DepthFunction.Gequal;
                case DepthTestFunction.Always:
                    return DepthFunction.Always;
            }

            throw new ArgumentException("function");
        }

        public static CullFaceMode To(CullFace face)
        {
            switch (face)
            {
                case CullFace.Front:
                    return CullFaceMode.Front;
                case CullFace.Back:
                    return CullFaceMode.Back;
                case CullFace.FrontAndBack:
                    return CullFaceMode.FrontAndBack;
            }

            throw new ArgumentException("face");
        }

        public static FrontFaceDirection To(WindingOrder windingOrder)
        {
            switch (windingOrder)
            {
                case WindingOrder.Clockwise:
                    return FrontFaceDirection.Cw;
                case WindingOrder.Counterclockwise:
                    return FrontFaceDirection.Ccw;
            }

            throw new ArgumentException("windingOrder");
        }

        public static PolygonMode To(RasterizationMode mode)
        {
            switch (mode)
            {
                case RasterizationMode.Point:
                    return PolygonMode.Point;
                case RasterizationMode.Line:
                    return PolygonMode.Line;
                case RasterizationMode.Fill:
                    return PolygonMode.Fill;
            }

            throw new ArgumentException("mode");
        }

        public static StencilOp To(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Zero:
                    return StencilOp.Zero;
                case StencilOperation.Invert:
                    return StencilOp.Invert;
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Replace:
                    return StencilOp.Replace;
                case StencilOperation.Increment:
                    return StencilOp.Incr;
                case StencilOperation.Decrement:
                    return StencilOp.Decr;
                case StencilOperation.IncrementWrap:
                    return StencilOp.IncrWrap;
                case StencilOperation.DecrementWrap:
                    return StencilOp.DecrWrap;
            }

            throw new ArgumentException("operation");
        }

        public static StencilFunction To(StencilTestFunction function)
        {
            switch (function)
            {
                case StencilTestFunction.Never:
                    return StencilFunction.Never;
                case StencilTestFunction.Less:
                    return StencilFunction.Less;
                case StencilTestFunction.Equal:
                    return StencilFunction.Equal;
                case StencilTestFunction.LessThanOrEqual:
                    return StencilFunction.Lequal;
                case StencilTestFunction.Greater:
                    return StencilFunction.Greater;
                case StencilTestFunction.NotEqual:
                    return StencilFunction.Notequal;
                case StencilTestFunction.GreaterThanOrEqual:
                    return StencilFunction.Gequal;
                case StencilTestFunction.Always:
                    return StencilFunction.Always;
            }

            throw new ArgumentException("function");
        }

        public static BlendEquationMode To(BlendEquation equation)
        {
            switch (equation)
            {
                case BlendEquation.Add:
                    return BlendEquationMode.FuncAdd;
                case BlendEquation.Minimum:
                    return BlendEquationMode.Min;
                case BlendEquation.Maximum:
                    return BlendEquationMode.Max;
                case BlendEquation.Subtract:
                    return BlendEquationMode.FuncSubtract;
                case BlendEquation.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
            }

            throw new ArgumentException("equation");
        }

        public static BlendingFactorSrc To(SourceBlendingFactor factor)
        {
            switch (factor)
            {
                case SourceBlendingFactor.Zero:
                    return BlendingFactorSrc.Zero;
                case SourceBlendingFactor.One:
                    return BlendingFactorSrc.One;
                case SourceBlendingFactor.SourceAlpha:
                    return BlendingFactorSrc.SrcAlpha;
                case SourceBlendingFactor.OneMinusSourceAlpha:
                    return BlendingFactorSrc.OneMinusSrcAlpha;
                case SourceBlendingFactor.DestinationAlpha:
                    return BlendingFactorSrc.DstAlpha;
                case SourceBlendingFactor.OneMinusDestinationAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;
                case SourceBlendingFactor.DestinationColor:
                    return BlendingFactorSrc.DstColor;
                case SourceBlendingFactor.OneMinusDestinationColor:
                    return BlendingFactorSrc.OneMinusDstColor;
                case SourceBlendingFactor.SourceAlphaSaturate:
                    return BlendingFactorSrc.SrcAlphaSaturate;
                case SourceBlendingFactor.ConstantColor:
                    return BlendingFactorSrc.ConstantColor;
                case SourceBlendingFactor.OneMinusConstantColor:
                    return BlendingFactorSrc.OneMinusConstantColor;
                case SourceBlendingFactor.ConstantAlpha:
                    return BlendingFactorSrc.ConstantAlpha;
                case SourceBlendingFactor.OneMinusConstantAlpha:
                    return BlendingFactorSrc.OneMinusConstantAlpha;
            }

            throw new ArgumentException("factor");
        }

        public static BlendingFactorDest To(DestinationBlendingFactor factor)
        {
            switch (factor)
            {
                case DestinationBlendingFactor.Zero:
                    return BlendingFactorDest.Zero;
                case DestinationBlendingFactor.One:
                    return BlendingFactorDest.One;
                case DestinationBlendingFactor.SourceColor:
                    return BlendingFactorDest.SrcColor;
                case DestinationBlendingFactor.OneMinusSourceColor:
                    return BlendingFactorDest.OneMinusSrcColor;
                case DestinationBlendingFactor.SourceAlpha:
                    return BlendingFactorDest.SrcAlpha;
                case DestinationBlendingFactor.OneMinusSourceAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;
                case DestinationBlendingFactor.DestinationAlpha:
                    return BlendingFactorDest.DstAlpha;
                case DestinationBlendingFactor.OneMinusDestinationAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;
                case DestinationBlendingFactor.DestinationColor:
                    return BlendingFactorDest.DstColor;
                case DestinationBlendingFactor.OneMinusDestinationColor:
                    return BlendingFactorDest.OneMinusDstColor;
                case DestinationBlendingFactor.ConstantColor:
                    return BlendingFactorDest.ConstantColor;
                case DestinationBlendingFactor.OneMinusConstantColor:
                    return BlendingFactorDest.OneMinusConstantColor;
                case DestinationBlendingFactor.ConstantAlpha:
                    return BlendingFactorDest.ConstantAlpha;
                case DestinationBlendingFactor.OneMinusConstantAlpha:
                    return BlendingFactorDest.OneMinusConstantAlpha;
            }

            throw new ArgumentException("factor");
        }

        public static PixelInternalFormat To(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RedGreenBlue8:
                    return PixelInternalFormat.Rgb8;
                case TextureFormat.RedGreenBlue16:
                    return PixelInternalFormat.Rgb16;
                case TextureFormat.RedGreenBlueAlpha8:
                    return PixelInternalFormat.Rgba8;
                case TextureFormat.RedGreenBlue10A2:
                    return PixelInternalFormat.Rgb10A2;
                case TextureFormat.RedGreenBlueAlpha16:
                    return PixelInternalFormat.Rgba16;
                case TextureFormat.Depth16:
                    return PixelInternalFormat.DepthComponent16;
                case TextureFormat.Depth24:
                    return PixelInternalFormat.DepthComponent24;
                case TextureFormat.Red8:
                    return PixelInternalFormat.R8;
                case TextureFormat.Red16:
                    return PixelInternalFormat.R16;
                case TextureFormat.RedGreen8:
                    return PixelInternalFormat.Rg8;
                case TextureFormat.RedGreen16:
                    return PixelInternalFormat.Rg16;
                case TextureFormat.Red16f:
                    return PixelInternalFormat.R16f;
                case TextureFormat.Red32f:
                    return PixelInternalFormat.R32f;
                case TextureFormat.RedGreen16f:
                    return PixelInternalFormat.Rg16f;
                case TextureFormat.RedGreen32f:
                    return PixelInternalFormat.Rg32f;
                case TextureFormat.Red8i:
                    return PixelInternalFormat.R8i;
                case TextureFormat.Red8ui:
                    return PixelInternalFormat.R8ui;
                case TextureFormat.Red16i:
                    return PixelInternalFormat.R16i;
                case TextureFormat.Red16ui:
                    return PixelInternalFormat.R16ui;
                case TextureFormat.Red32i:
                    return PixelInternalFormat.R32i;
                case TextureFormat.Red32ui:
                    return PixelInternalFormat.R32ui;
                case TextureFormat.RedGreen8i:
                    return PixelInternalFormat.Rg8i;
                case TextureFormat.RedGreen8ui:
                    return PixelInternalFormat.Rg8ui;
                case TextureFormat.RedGreen16i:
                    return PixelInternalFormat.Rg16i;
                case TextureFormat.RedGreen16ui:
                    return PixelInternalFormat.Rg16ui;
                case TextureFormat.RedGreen32i:
                    return PixelInternalFormat.Rg32i;
                case TextureFormat.RedGreen32ui:
                    return PixelInternalFormat.Rg32ui;
                case TextureFormat.RedGreenBlueAlpha32f:
                    return PixelInternalFormat.Rgba32f;
                case TextureFormat.RedGreenBlue32f:
                    return PixelInternalFormat.Rgb32f;
                case TextureFormat.RedGreenBlueAlpha16f:
                    return PixelInternalFormat.Rgba16f;
                case TextureFormat.RedGreenBlue16f:
                    return PixelInternalFormat.Rgb16f;
                case TextureFormat.Depth24Stencil8:
                    return PixelInternalFormat.Depth24Stencil8;
                case TextureFormat.Red11fGreen11fBlue10f:
                    return PixelInternalFormat.R11fG11fB10f;
                case TextureFormat.RedGreenBlue9E5:
                    return PixelInternalFormat.Rgb9E5;
                case TextureFormat.SRedGreenBlue8:
                    return PixelInternalFormat.Srgb8;
                case TextureFormat.SRedGreenBlue8Alpha8:
                    return PixelInternalFormat.Srgb8Alpha8;
                case TextureFormat.Depth32f:
                    return PixelInternalFormat.DepthComponent32f;
                case TextureFormat.Depth32fStencil8:
                    return PixelInternalFormat.Depth32fStencil8;
                case TextureFormat.RedGreenBlueAlpha32ui:
                    return PixelInternalFormat.Rgba32ui;
                case TextureFormat.RedGreenBlue32ui:
                    return PixelInternalFormat.Rgb32ui;
                case TextureFormat.RedGreenBlueAlpha16ui:
                    return PixelInternalFormat.Rgba16ui;
                case TextureFormat.RedGreenBlue16ui:
                    return PixelInternalFormat.Rgb16ui;
                case TextureFormat.RedGreenBlueAlpha8ui:
                    return PixelInternalFormat.Rgba8ui;
                case TextureFormat.RedGreenBlue8ui:
                    return PixelInternalFormat.Rgb8ui;
                case TextureFormat.RedGreenBlueAlpha32i:
                    return PixelInternalFormat.Rgba32i;
                case TextureFormat.RedGreenBlue32i:
                    return PixelInternalFormat.Rgb32i;
                case TextureFormat.RedGreenBlueAlpha16i:
                    return PixelInternalFormat.Rgba16i;
                case TextureFormat.RedGreenBlue16i:
                    return PixelInternalFormat.Rgb16i;
                case TextureFormat.RedGreenBlueAlpha8i:
                    return PixelInternalFormat.Rgba8i;
                case TextureFormat.RedGreenBlue8i:
                    return PixelInternalFormat.Rgb8i;
            }

            throw new ArgumentException("format");
        }

        public static PixelFormat To(ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.StencilIndex:
                    return PixelFormat.StencilIndex;
                case ImageFormat.DepthComponent:
                    return PixelFormat.DepthComponent;
                case ImageFormat.Red:
                    return PixelFormat.Red;
                case ImageFormat.Green:
                    return PixelFormat.Green;
                case ImageFormat.Blue:
                    return PixelFormat.Blue;
                case ImageFormat.RedGreenBlue:
                    return PixelFormat.Rgb;
                case ImageFormat.RedGreenBlueAlpha:
                    return PixelFormat.Rgba;
                case ImageFormat.BlueGreenRed:
                    return PixelFormat.Bgr;
                case ImageFormat.BlueGreenRedAlpha:
                    return PixelFormat.Bgra;
                case ImageFormat.RedGreen:
                    return PixelFormat.Rg;
                case ImageFormat.RedGreenInteger:
                    return PixelFormat.RgInteger;
                case ImageFormat.DepthStencil:
                    return PixelFormat.DepthStencil;
                case ImageFormat.RedInteger:
                    return PixelFormat.RedInteger;
                case ImageFormat.GreenInteger:
                    return PixelFormat.GreenInteger;
                case ImageFormat.BlueInteger:
                    return PixelFormat.BlueInteger;
                case ImageFormat.RedGreenBlueInteger:
                    return PixelFormat.RgbInteger;
                case ImageFormat.RedGreenBlueAlphaInteger:
                    return PixelFormat.RgbaInteger;
                case ImageFormat.BlueGreenRedInteger:
                    return PixelFormat.BgrInteger;
                case ImageFormat.BlueGreenRedAlphaInteger:
                    return PixelFormat.BgraInteger;
            }

            throw new ArgumentException("format");
        }

        public static PixelType To(ImageDatatype type)
        {
            switch (type)
            {
                case ImageDatatype.Byte:
                    return PixelType.Byte;
                case ImageDatatype.UnsignedByte:
                    return PixelType.UnsignedByte;
                case ImageDatatype.Short:
                    return PixelType.Short;
                case ImageDatatype.UnsignedShort:
                    return PixelType.UnsignedShort;
                case ImageDatatype.Int:
                    return PixelType.Int;
                case ImageDatatype.UnsignedInt:
                    return PixelType.UnsignedInt;
                case ImageDatatype.Float:
                    return PixelType.Float;
                case ImageDatatype.HalfFloat:
                    return PixelType.HalfFloat;
                case ImageDatatype.UnsignedByte332:
                    return PixelType.UnsignedByte332;
                case ImageDatatype.UnsignedShort4444:
                    return PixelType.UnsignedShort4444;
                case ImageDatatype.UnsignedShort5551:
                    return PixelType.UnsignedShort5551;
                case ImageDatatype.UnsignedInt8888:
                    return PixelType.UnsignedInt8888;
                case ImageDatatype.UnsignedInt1010102:
                    return PixelType.UnsignedInt1010102;
                case ImageDatatype.UnsignedByte233Reversed:
                    return PixelType.UnsignedByte233Reversed;
                case ImageDatatype.UnsignedShort565:
                    return PixelType.UnsignedShort565;
                case ImageDatatype.UnsignedShort565Reversed:
                    return PixelType.UnsignedShort565Reversed;
                case ImageDatatype.UnsignedShort4444Reversed:
                    return PixelType.UnsignedShort4444Reversed;
                case ImageDatatype.UnsignedShort1555Reversed:
                    return PixelType.UnsignedShort1555Reversed;
                case ImageDatatype.UnsignedInt8888Reversed:
                    return PixelType.UnsignedInt8888Reversed;
                case ImageDatatype.UnsignedInt2101010Reversed:
                    return PixelType.UnsignedInt2101010Reversed;
                case ImageDatatype.UnsignedInt248:
                    return PixelType.UnsignedInt248;
                case ImageDatatype.UnsignedInt10F11F11FReversed:
                    return PixelType.UnsignedInt10F11F11FRev;
                case ImageDatatype.UnsignedInt5999Reversed:
                    return PixelType.UnsignedInt5999Rev;
                case ImageDatatype.Float32UnsignedInt248Reversed:
                    return PixelType.Float32UnsignedInt248Rev;
            }

            throw new ArgumentException("type");
        }

        public static PixelFormat TextureToPixelFormat(TextureFormat textureFormat)
        {
            if (!IsTextureFormatValid(textureFormat))
            {
                throw new ArgumentException("Invalid texture format.", "textureFormat");
            }

            // TODO:  Not tested exhaustively
            switch (textureFormat)
            {
                case TextureFormat.RedGreenBlue8:
                case TextureFormat.RedGreenBlue16:
                    return PixelFormat.Rgb;
                case TextureFormat.RedGreenBlueAlpha8:
                case TextureFormat.RedGreenBlue10A2:
                case TextureFormat.RedGreenBlueAlpha16:
                    return PixelFormat.Rgba;
                case TextureFormat.Depth16:
                case TextureFormat.Depth24:
                    return PixelFormat.DepthComponent;
                case TextureFormat.Red8:
                case TextureFormat.Red16:
                    return PixelFormat.Red;
                case TextureFormat.RedGreen8:
                case TextureFormat.RedGreen16:
                    return PixelFormat.Rg;
                case TextureFormat.Red16f:
                case TextureFormat.Red32f:
                    return PixelFormat.Red;
                case TextureFormat.RedGreen16f:
                case TextureFormat.RedGreen32f:
                    return PixelFormat.Rg;
                case TextureFormat.Red8i:
                case TextureFormat.Red8ui:
                case TextureFormat.Red16i:
                case TextureFormat.Red16ui:
                case TextureFormat.Red32i:
                case TextureFormat.Red32ui:
                    return PixelFormat.RedInteger;
                case TextureFormat.RedGreen8i:
                case TextureFormat.RedGreen8ui:
                case TextureFormat.RedGreen16i:
                case TextureFormat.RedGreen16ui:
                case TextureFormat.RedGreen32i:
                case TextureFormat.RedGreen32ui:
                    return PixelFormat.RgInteger;
                case TextureFormat.RedGreenBlueAlpha32f:
                    return PixelFormat.Rgba;
                case TextureFormat.RedGreenBlue32f:
                    return PixelFormat.Rgb;
                case TextureFormat.RedGreenBlueAlpha16f:
                    return PixelFormat.Rgba;
                case TextureFormat.RedGreenBlue16f:
                    return PixelFormat.Rgb;
                case TextureFormat.Depth24Stencil8:
                    return PixelFormat.DepthStencil;
                case TextureFormat.Red11fGreen11fBlue10f:
                case TextureFormat.RedGreenBlue9E5:
                    return PixelFormat.Rgb;
                case TextureFormat.SRedGreenBlue8:
                    return PixelFormat.RgbInteger;
                case TextureFormat.SRedGreenBlue8Alpha8:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.Depth32f:
                    return PixelFormat.DepthComponent;
                case TextureFormat.Depth32fStencil8:
                    return PixelFormat.DepthStencil;
                case TextureFormat.RedGreenBlueAlpha32ui:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue32ui:
                    return PixelFormat.RgbInteger;
                case TextureFormat.RedGreenBlueAlpha16ui:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue16ui:
                    return PixelFormat.RgbInteger;
                case TextureFormat.RedGreenBlueAlpha8ui:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue8ui:
                    return PixelFormat.RgbInteger;
                case TextureFormat.RedGreenBlueAlpha32i:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue32i:
                    return PixelFormat.RgbInteger;
                case TextureFormat.RedGreenBlueAlpha16i:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue16i:
                    return PixelFormat.RgbInteger;
                case TextureFormat.RedGreenBlueAlpha8i:
                    return PixelFormat.RgbaInteger;
                case TextureFormat.RedGreenBlue8i:
                    return PixelFormat.RgbInteger;
            }

            throw new ArgumentException("textureFormat");
        }

        public static PixelType TextureToPixelType(TextureFormat textureFormat)
        {
            if (!IsTextureFormatValid(textureFormat))
            {
                throw new ArgumentException("Invalid texture format.", "textureFormat");
            }

            // TODO:  Not tested exhaustively
            switch (textureFormat)
            {
                case TextureFormat.RedGreenBlue8:
                    return PixelType.UnsignedByte;
                case TextureFormat.RedGreenBlue16:
                    return PixelType.UnsignedShort;
                case TextureFormat.RedGreenBlueAlpha8:
                    return PixelType.UnsignedByte;
                case TextureFormat.RedGreenBlue10A2:
                    return PixelType.UnsignedInt1010102;
                case TextureFormat.RedGreenBlueAlpha16:
                    return PixelType.UnsignedShort;
                case TextureFormat.Depth16:
                    return PixelType.HalfFloat;
                case TextureFormat.Depth24:
                    return PixelType.Float;
                case TextureFormat.Red8:
                    return PixelType.UnsignedByte;
                case TextureFormat.Red16:
                    return PixelType.UnsignedShort;
                case TextureFormat.RedGreen8:
                    return PixelType.UnsignedByte;
                case TextureFormat.RedGreen16:
                    return PixelType.UnsignedShort;
                case TextureFormat.Red16f:
                    return PixelType.HalfFloat;
                case TextureFormat.Red32f:
                    return PixelType.Float;
                case TextureFormat.RedGreen16f:
                    return PixelType.HalfFloat;
                case TextureFormat.RedGreen32f:
                    return PixelType.Float;
                case TextureFormat.Red8i:
                    return PixelType.Byte;
                case TextureFormat.Red8ui:
                    return PixelType.UnsignedByte;
                case TextureFormat.Red16i:
                    return PixelType.Short;
                case TextureFormat.Red16ui:
                    return PixelType.UnsignedShort;
                case TextureFormat.Red32i:
                    return PixelType.Int;
                case TextureFormat.Red32ui:
                    return PixelType.UnsignedInt;
                case TextureFormat.RedGreen8i:
                    return PixelType.Byte;
                case TextureFormat.RedGreen8ui:
                    return PixelType.UnsignedByte;
                case TextureFormat.RedGreen16i:
                    return PixelType.Short;
                case TextureFormat.RedGreen16ui:
                    return PixelType.UnsignedShort;
                case TextureFormat.RedGreen32i:
                    return PixelType.Int;
                case TextureFormat.RedGreen32ui:
                    return PixelType.UnsignedInt;
                case TextureFormat.RedGreenBlueAlpha32f:
                    return PixelType.Float;
                case TextureFormat.RedGreenBlue32f:
                    return PixelType.Float;
                case TextureFormat.RedGreenBlueAlpha16f:
                    return PixelType.HalfFloat;
                case TextureFormat.RedGreenBlue16f:
                    return PixelType.HalfFloat;
                case TextureFormat.Depth24Stencil8:
                    return PixelType.UnsignedInt248;
                case TextureFormat.Red11fGreen11fBlue10f:
                    return PixelType.Float;
                case TextureFormat.RedGreenBlue9E5:
                    return PixelType.Float;
                case TextureFormat.SRedGreenBlue8:
                case TextureFormat.SRedGreenBlue8Alpha8:
                    return PixelType.Byte;
                case TextureFormat.Depth32f:
                case TextureFormat.Depth32fStencil8:
                    return PixelType.Float;
                case TextureFormat.RedGreenBlueAlpha32ui:
                case TextureFormat.RedGreenBlue32ui:
                    return PixelType.UnsignedInt;
                case TextureFormat.RedGreenBlueAlpha16ui:
                case TextureFormat.RedGreenBlue16ui:
                    return PixelType.UnsignedShort;
                case TextureFormat.RedGreenBlueAlpha8ui:
                case TextureFormat.RedGreenBlue8ui:
                    return PixelType.UnsignedByte;
                case TextureFormat.RedGreenBlueAlpha32i:
                case TextureFormat.RedGreenBlue32i:
                    return PixelType.UnsignedInt;
                case TextureFormat.RedGreenBlueAlpha16i:
                case TextureFormat.RedGreenBlue16i:
                    return PixelType.UnsignedShort;
                case TextureFormat.RedGreenBlueAlpha8i:
                case TextureFormat.RedGreenBlue8i:
                    return PixelType.UnsignedByte;
            }

            throw new ArgumentException("textureFormat");
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
            switch (filter)
            {
                case TextureMinificationFilter.Nearest:
                    return TextureMinFilter.Nearest;
                case TextureMinificationFilter.Linear:
                    return TextureMinFilter.Linear;
                case TextureMinificationFilter.NearestMipmapNearest:
                    return TextureMinFilter.NearestMipmapNearest;
                case TextureMinificationFilter.LinearMipmapNearest:
                    return TextureMinFilter.LinearMipmapNearest;
                case TextureMinificationFilter.NearestMipmapLinear:
                    return TextureMinFilter.NearestMipmapLinear;
                case TextureMinificationFilter.LinearMipmapLinear:
                    return TextureMinFilter.LinearMipmapLinear;
            }

            throw new ArgumentException("filter");
        }

        public static TextureMagFilter To(TextureMagnificationFilter filter)
        {
            switch (filter)
            {
                case TextureMagnificationFilter.Nearest:
                    return TextureMagFilter.Nearest;
                case TextureMagnificationFilter.Linear:
                    return TextureMagFilter.Linear;
            }

            throw new ArgumentException("filter");
        }

        public static TextureWrapMode To(TextureWrap wrap)
        {
            switch (wrap)
            {
                case TextureWrap.Clamp:
                    return TextureWrapMode.ClampToEdge;
                case TextureWrap.Repeat:
                    return TextureWrapMode.Repeat;
                case TextureWrap.MirroredRepeat:
                    return TextureWrapMode.MirroredRepeat;
            }

            throw new ArgumentException("wrap");
        }
    }
}
