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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL32
{
    internal class ContextGL32 : Context
    {
        public ContextGL32()
        {
            Vector4 clearColor = new Vector4();
            GL.GetFloat(GetPName.DepthClearValue, out _clearDepth);
            GL.GetInteger(GetPName.StencilClearValue, out _clearStencil);
            GL.GetFloat(GetPName.ColorClearValue, out clearColor);
            _clearColor = Color.FromArgb(
                Convert.ToInt32(clearColor.W * 255.0), 
                Convert.ToInt32(clearColor.X * 255.0), 
                Convert.ToInt32(clearColor.Y * 255.0),
                Convert.ToInt32(clearColor.Z * 255.0));

            _renderState = new RenderState();
            _textureUnits = new TextureUnitsGL32();

            //
            // Sync GL state with default render state.
            //
            ForceApply(_renderState);
        }

        #region ForceApply

        private static void ForceApply(RenderState renderState)
        {
            Enable(EnableCap.PrimitiveRestart, renderState.PrimitiveRestart.Enabled);
            GL.PrimitiveRestartIndex(renderState.PrimitiveRestart.Index);

            Enable(EnableCap.CullFace, renderState.FacetCulling.Enabled);
            GL.CullFace(TypeConverterGL32.To(renderState.FacetCulling.Face));
            GL.FrontFace(TypeConverterGL32.To(renderState.FacetCulling.FrontFaceWindingOrder));

            Enable(EnableCap.ProgramPointSize, renderState.ProgramPointSize == ProgramPointSize.Enabled);
            GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL32.To(renderState.RasterizationMode));

            Enable(EnableCap.ScissorTest, renderState.ScissorTest.Enabled);
            Rectangle rectangle = renderState.ScissorTest.Rectangle;
            GL.Scissor(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);

            Enable(EnableCap.StencilTest, renderState.StencilTest.Enabled);
            ForceApplyStencil(StencilFace.Front, renderState.StencilTest.FrontFace);
            ForceApplyStencil(StencilFace.Back, renderState.StencilTest.BackFace);

            Enable(EnableCap.DepthTest, renderState.DepthTest.Enabled);
            GL.DepthFunc(TypeConverterGL32.To(renderState.DepthTest.Function));

            GL.DepthRange(renderState.DepthRange.Near, renderState.DepthRange.Far);

            Enable(EnableCap.Blend, renderState.Blending.Enabled);
            GL.BlendFuncSeparate(
                TypeConverterGL32.To(renderState.Blending.SourceRGBFactor),
                TypeConverterGL32.To(renderState.Blending.DestinationRGBFactor),
                TypeConverterGL32.To(renderState.Blending.SourceAlphaFactor),
                TypeConverterGL32.To(renderState.Blending.DestinationAlphaFactor));
            GL.BlendEquationSeparate(
                TypeConverterGL32.To(renderState.Blending.RGBEquation),
                TypeConverterGL32.To(renderState.Blending.AlphaEquation));
            GL.BlendColor(renderState.Blending.Color);
        }

        private static void ForceApplyStencil(StencilFace face, StencilTestFace test)
        {
            GL.StencilOpSeparate(face,
                TypeConverterGL32.To(test.StencilFailOperation),
                TypeConverterGL32.To(test.DepthPassStencilFailOperation),
                TypeConverterGL32.To(test.DepthPassStencilPassOperation));

            GL.StencilFuncSeparate(face,
                TypeConverterGL32.To(test.Function),
                test.ReferenceValue,
                test.Mask);
        }

        #endregion

        #region Context Members

        public override TextureUnits TextureUnits
        {
            get { return _textureUnits; }
        }

        public override VertexArray CreateVertexArray()
        {
            return new VertexArrayGL32();
        }

        public override FrameBuffer CreateFrameBuffer()
        {
            return new FrameBufferGL32();
        }

        public override void Clear(ClearBuffers buffers, Color color, float depth, int stencil)
        {
            CleanFrameBuffer();

            if (_clearColor != color)
            {
                GL.ClearColor(color);
                _clearColor = color;
            }

            if (_clearDepth != depth)
            {
                GL.ClearDepth(depth);
                _clearDepth = depth;
            }

            if (_clearStencil != stencil)
            {
                GL.ClearStencil(stencil);
                _clearStencil = stencil;
            }

            GL.Clear(TypeConverterGL32.To(buffers));
        }

        public override Rectangle Viewport
        {
            get
            {
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);

                return new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);
            }

            set
            {
                Debug.Assert(value.Width >= 0);
                Debug.Assert(value.Height >= 0);

                GL.Viewport(value);
            }
        }

        public override void Bind(PrimitiveRestart primitiveRestart)
        {
            if (_renderState.PrimitiveRestart.Enabled != primitiveRestart.Enabled)
            {
                Enable(EnableCap.PrimitiveRestart, primitiveRestart.Enabled);
                _renderState.PrimitiveRestart.Enabled = primitiveRestart.Enabled;
            }

            if (_renderState.PrimitiveRestart.Index != primitiveRestart.Index)
            {
                GL.PrimitiveRestartIndex(primitiveRestart.Index);
                _renderState.PrimitiveRestart.Index = primitiveRestart.Index;
            }
        }
        
        public override void Bind(FacetCulling facetCulling)
        {
            if (_renderState.FacetCulling.Enabled != facetCulling.Enabled)
            {
                Enable(EnableCap.CullFace, facetCulling.Enabled);
                _renderState.FacetCulling.Enabled = facetCulling.Enabled;
            }

            if (_renderState.FacetCulling.Face != facetCulling.Face)
            {
                GL.CullFace(TypeConverterGL32.To(facetCulling.Face));
                _renderState.FacetCulling.Face = facetCulling.Face;
            }

            if (_renderState.FacetCulling.FrontFaceWindingOrder != facetCulling.FrontFaceWindingOrder)
            {
                GL.FrontFace(TypeConverterGL32.To(facetCulling.FrontFaceWindingOrder));
                _renderState.FacetCulling.FrontFaceWindingOrder = facetCulling.FrontFaceWindingOrder;
            }
        }

        public override void Bind(ProgramPointSize programPointSize)
        {
            if (_renderState.ProgramPointSize != programPointSize)
            {
                Enable(EnableCap.ProgramPointSize, programPointSize == ProgramPointSize.Enabled);
                _renderState.ProgramPointSize = programPointSize;
            }
        }

        public override void Bind(RasterizationMode rasterizationMode)
        {
            if (_renderState.RasterizationMode != rasterizationMode)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL32.To(rasterizationMode));
                _renderState.RasterizationMode = rasterizationMode;
            }
        }

        public override void Bind(ScissorTest scissorTest)
        {
            Rectangle rectangle = scissorTest.Rectangle;
            Debug.Assert(rectangle.Width >= 0);
            Debug.Assert(rectangle.Height >= 0);

            if (_renderState.ScissorTest.Enabled != scissorTest.Enabled)
            {
                Enable(EnableCap.ScissorTest, scissorTest.Enabled);
                _renderState.ScissorTest.Enabled = scissorTest.Enabled;
            }

            if (_renderState.ScissorTest.Rectangle != scissorTest.Rectangle)
            {
                GL.Scissor(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                _renderState.ScissorTest.Rectangle = scissorTest.Rectangle;
            }
        }

        public override void Bind(StencilTest stencilTest)
        {
            if (_renderState.StencilTest.Enabled != stencilTest.Enabled)
            {
                Enable(EnableCap.StencilTest, stencilTest.Enabled);
                _renderState.StencilTest.Enabled = stencilTest.Enabled;
            }

            ApplyStencil(StencilFace.Front, _renderState.StencilTest.FrontFace, stencilTest.FrontFace);
            ApplyStencil(StencilFace.Back, _renderState.StencilTest.BackFace, stencilTest.BackFace);
        }

        private static void ApplyStencil(StencilFace face, StencilTestFace currentTest, StencilTestFace test)
        {
            if ((currentTest.StencilFailOperation != test.StencilFailOperation) ||
                (currentTest.DepthPassStencilFailOperation != test.DepthPassStencilFailOperation) ||
                (currentTest.DepthPassStencilPassOperation != test.DepthPassStencilPassOperation))
            {
                GL.StencilOpSeparate(face,
                    TypeConverterGL32.To(test.StencilFailOperation),
                    TypeConverterGL32.To(test.DepthPassStencilFailOperation),
                    TypeConverterGL32.To(test.DepthPassStencilPassOperation));

                currentTest.StencilFailOperation = test.StencilFailOperation;
                currentTest.DepthPassStencilFailOperation = test.DepthPassStencilFailOperation;
                currentTest.DepthPassStencilPassOperation = test.DepthPassStencilPassOperation;
            }

            if ((currentTest.Function != test.Function) ||
                (currentTest.ReferenceValue != test.ReferenceValue) ||
                (currentTest.Mask != test.Mask))
            {
                GL.StencilFuncSeparate(face,
                    TypeConverterGL32.To(test.Function),
                    test.ReferenceValue,
                    test.Mask);

                currentTest.Function = test.Function;
                currentTest.ReferenceValue = test.ReferenceValue;
                currentTest.Mask = test.Mask;
            }
        }

        public override void Bind(DepthTest depthTest)
        {
            if (_renderState.DepthTest.Enabled != depthTest.Enabled)
            {
                Enable(EnableCap.DepthTest, depthTest.Enabled);
                _renderState.DepthTest.Enabled = depthTest.Enabled;
            }

            if (_renderState.DepthTest.Function != depthTest.Function)
            {
                GL.DepthFunc(TypeConverterGL32.To(depthTest.Function));
                _renderState.DepthTest.Function = depthTest.Function;
            }
        }

        public override void Bind(DepthRange depthRange)
        {
            Debug.Assert(depthRange.Near >= 0.0 && depthRange.Near <= 1.0);
            Debug.Assert(depthRange.Far >= 0.0 && depthRange.Far <= 1.0);

            if ((_renderState.DepthRange.Near != depthRange.Near) ||
                (_renderState.DepthRange.Far != depthRange.Far))
            {
                GL.DepthRange(depthRange.Near, depthRange.Far);

                _renderState.DepthRange.Near = depthRange.Near;
                _renderState.DepthRange.Far = depthRange.Far;
            }
        }

        public override void Bind(Blending blending)
        {
            if (_renderState.Blending.Enabled != blending.Enabled)
            {
                Enable(EnableCap.Blend, blending.Enabled);
                _renderState.Blending.Enabled = blending.Enabled;
            }

            if ((_renderState.Blending.SourceRGBFactor != blending.SourceRGBFactor) ||
                (_renderState.Blending.DestinationRGBFactor != blending.DestinationRGBFactor) ||
                (_renderState.Blending.SourceAlphaFactor != blending.SourceAlphaFactor) ||
                (_renderState.Blending.DestinationAlphaFactor != blending.DestinationAlphaFactor))
            {
                GL.BlendFuncSeparate(
                    TypeConverterGL32.To(blending.SourceRGBFactor),
                    TypeConverterGL32.To(blending.DestinationRGBFactor),
                    TypeConverterGL32.To(blending.SourceAlphaFactor),
                    TypeConverterGL32.To(blending.DestinationAlphaFactor));

                _renderState.Blending.SourceRGBFactor = blending.SourceRGBFactor;
                _renderState.Blending.DestinationRGBFactor = blending.DestinationRGBFactor;
                _renderState.Blending.SourceAlphaFactor = blending.SourceAlphaFactor;
                _renderState.Blending.DestinationAlphaFactor = blending.DestinationAlphaFactor;
            }

            if ((_renderState.Blending.RGBEquation != blending.RGBEquation) ||
                (_renderState.Blending.AlphaEquation != blending.AlphaEquation))
            {
                GL.BlendEquationSeparate(
                    TypeConverterGL32.To(blending.RGBEquation),
                    TypeConverterGL32.To(blending.AlphaEquation));

                _renderState.Blending.RGBEquation = blending.RGBEquation;
                _renderState.Blending.AlphaEquation = blending.AlphaEquation;

            }

            if (_renderState.Blending.Color != blending.Color)
            {
                GL.BlendColor(blending.Color);
                _renderState.Blending.Color = blending.Color;
            }
        }

        private static void Enable(EnableCap enableCap, bool enable)
        {
            if (enable)
            {
                GL.Enable(enableCap);
            }
            else
            {
                GL.Disable(enableCap);
            }
        }

        public override void Bind(VertexArray vertexArray)
        {
            VertexArrayGL32 vertexArrayGL32 = vertexArray as VertexArrayGL32;

            if (_boundVertexArray != vertexArrayGL32)
            {
                vertexArrayGL32.Bind();
                _boundVertexArray = vertexArrayGL32;
            }
        }

        public override void Bind(ShaderProgram shaderProgram)
        {
            ShaderProgramGL32 shaderProgramGL32 = shaderProgram as ShaderProgramGL32;

            if (_boundShaderProgram != shaderProgramGL32)
            {
                shaderProgramGL32.Bind();
                _boundShaderProgram = shaderProgramGL32;
            }
        }

        public override void Bind(FrameBuffer frameBuffer)
        {
            FrameBufferGL32 frameBufferGL32 = frameBuffer as FrameBufferGL32;

            if (_boundFrameBuffer != frameBufferGL32)
            {
                if (frameBufferGL32 != null)
                {
                    frameBufferGL32.Bind();
                }
                else
                {
                    FrameBufferGL32.UnBind();
                }

                _boundFrameBuffer = frameBufferGL32;
            }
        }

        public override void Draw(PrimitiveType primitiveType, int offset, int count, SceneState sceneState)
        {
            CleanBeforeDraw(sceneState);

            if (_boundIndexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL32.To(primitiveType),
                    0, _boundVertexArray.MaximumArrayIndex(), count,
                    TypeConverterGL32.To(_boundIndexBuffer.DataType), new
                    IntPtr(offset * SizesGL32.SizeOf(_boundIndexBuffer.DataType)));
            }
            else
            {
                GL.DrawArrays(TypeConverterGL32.To(primitiveType), offset, count);
            }
        }

        public override void Draw(PrimitiveType primitiveType, SceneState sceneState)
        {
            CleanBeforeDraw(sceneState);

            if (_boundIndexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL32.To(primitiveType),
                    0, _boundVertexArray.MaximumArrayIndex(), _boundIndexBuffer.Count,
                    TypeConverterGL32.To(_boundIndexBuffer.DataType), new IntPtr());
            }
            else
            {
                GL.DrawArrays(TypeConverterGL32.To(primitiveType), 0,
                    _boundVertexArray.MaximumArrayIndex() + 1);
            }
        }

        #endregion

        private void CleanBeforeDraw(SceneState sceneState)
        {
            CleanTextureUnits();
            CleanVertexArray();
            CleanShaderProgram(sceneState);
            CleanFrameBuffer();
        }

        private void CleanTextureUnits()
        {
            _textureUnits.Clean();
        }

        private void CleanVertexArray()
        {
            Debug.Assert(_boundVertexArray != null);
            _boundVertexArray.Clean();
            _boundIndexBuffer = _boundVertexArray.IndexBuffer as IndexBufferGL32;
        }

        private void CleanShaderProgram(SceneState sceneState)
        {
            Debug.Assert(_boundShaderProgram != null);
            _boundShaderProgram.Clean(this, sceneState);

#if DEBUG
            GL.ValidateProgram(_boundShaderProgram.Handle);

            int validateStatus;
            GL.GetProgram(_boundShaderProgram.Handle, ProgramParameter.ValidateStatus, out validateStatus);
            Debug.Assert(validateStatus != 0);
#endif
        }

        private void CleanFrameBuffer()
        {
            if (_boundFrameBuffer != null)
            {
                _boundFrameBuffer.Clean();

#if DEBUG
                FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                Debug.Assert(errorCode == FramebufferErrorCode.FramebufferComplete);
#endif
            }
        }

        private Color _clearColor;
        private float _clearDepth;
        private int _clearStencil;

        private RenderState _renderState;
        private VertexArrayGL32 _boundVertexArray;
        private IndexBufferGL32 _boundIndexBuffer;
        private ShaderProgramGL32 _boundShaderProgram;
        private FrameBufferGL32 _boundFrameBuffer;

        private TextureUnitsGL32 _textureUnits;
    }
}
