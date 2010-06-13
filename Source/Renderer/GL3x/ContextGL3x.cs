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
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;

namespace MiniGlobe.Renderer.GL3x
{
    internal class ContextGL3x : Context
    {
        public ContextGL3x()
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
            _textureUnits = new TextureUnitsGL3x();

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
            GL.CullFace(TypeConverterGL3x.To(renderState.FacetCulling.Face));
            GL.FrontFace(TypeConverterGL3x.To(renderState.FacetCulling.FrontFaceWindingOrder));

            Enable(EnableCap.ProgramPointSize, renderState.ProgramPointSize == ProgramPointSize.Enabled);
            GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL3x.To(renderState.RasterizationMode));

            Enable(EnableCap.ScissorTest, renderState.ScissorTest.Enabled);
            Rectangle rectangle = renderState.ScissorTest.Rectangle;
            GL.Scissor(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);

            Enable(EnableCap.StencilTest, renderState.StencilTest.Enabled);
            ForceApplyStencil(StencilFace.Front, renderState.StencilTest.FrontFace);
            ForceApplyStencil(StencilFace.Back, renderState.StencilTest.BackFace);

            Enable(EnableCap.DepthTest, renderState.DepthTest.Enabled);
            GL.DepthFunc(TypeConverterGL3x.To(renderState.DepthTest.Function));

            GL.DepthRange(renderState.DepthRange.Near, renderState.DepthRange.Far);

            Enable(EnableCap.Blend, renderState.Blending.Enabled);
            GL.BlendFuncSeparate(
                TypeConverterGL3x.To(renderState.Blending.SourceRGBFactor),
                TypeConverterGL3x.To(renderState.Blending.DestinationRGBFactor),
                TypeConverterGL3x.To(renderState.Blending.SourceAlphaFactor),
                TypeConverterGL3x.To(renderState.Blending.DestinationAlphaFactor));
            GL.BlendEquationSeparate(
                TypeConverterGL3x.To(renderState.Blending.RGBEquation),
                TypeConverterGL3x.To(renderState.Blending.AlphaEquation));
            GL.BlendColor(renderState.Blending.Color);

            GL.DepthMask(renderState.DepthWrite);
            GL.ColorMask(renderState.ColorMask.Red, renderState.ColorMask.Green, 
                renderState.ColorMask.Blue, renderState.ColorMask.Alpha);
        }

        private static void ForceApplyStencil(StencilFace face, StencilTestFace test)
        {
            GL.StencilOpSeparate(face,
                TypeConverterGL3x.To(test.StencilFailOperation),
                TypeConverterGL3x.To(test.DepthPassStencilFailOperation),
                TypeConverterGL3x.To(test.DepthPassStencilPassOperation));

            GL.StencilFuncSeparate(face,
                TypeConverterGL3x.To(test.Function),
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
            return new VertexArrayGL3x();
        }

        public override FrameBuffer CreateFrameBuffer()
        {
            return new FrameBufferGL3x();
        }

        public override void Clear(ClearState clearState)
        {
            CleanFrameBuffer();

            ApplyScissorTest(clearState.ScissorTest);
            ApplyColorMask(clearState.ColorMask);
            ApplyDepthWrite(clearState.DepthWrite);
            // TODO: StencilMaskSeparate

            if (_clearColor != clearState.Color)
            {
                GL.ClearColor(clearState.Color);
                _clearColor = clearState.Color;
            }

            if (_clearDepth != clearState.Depth)
            {
                GL.ClearDepth(clearState.Depth);
                _clearDepth = clearState.Depth;
            }

            if (_clearStencil != clearState.Stencil)
            {
                GL.ClearStencil(clearState.Stencil);
                _clearStencil = clearState.Stencil;
            }

            GL.Clear(TypeConverterGL3x.To(clearState.Buffers));
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

        public override void Bind(FrameBuffer frameBuffer)
        {
            FrameBufferGL3x frameBufferGL3x = frameBuffer as FrameBufferGL3x;

            if (_boundFrameBuffer != frameBufferGL3x)
            {
                if (frameBufferGL3x != null)
                {
                    frameBufferGL3x.Bind();
                }
                else
                {
                    FrameBufferGL3x.UnBind();
                }

                _boundFrameBuffer = frameBufferGL3x;
            }
        }

        public override void Draw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState)
        {
            VerifyDraw(drawState, sceneState);
            ApplyBeforeDraw(drawState, sceneState);

            if (_boundIndexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL3x.To(primitiveType),
                    0, _boundVertexArray.MaximumArrayIndex(), count,
                    TypeConverterGL3x.To(_boundIndexBuffer.DataType), new
                    IntPtr(offset * SizesGL3x.SizeOf(_boundIndexBuffer.DataType)));
            }
            else
            {
                GL.DrawArrays(TypeConverterGL3x.To(primitiveType), offset, count);
            }
        }

        public override void Draw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState)
        {
            VerifyDraw(drawState, sceneState);
            ApplyBeforeDraw(drawState, sceneState);

            if (_boundIndexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL3x.To(primitiveType),
                    0, _boundVertexArray.MaximumArrayIndex(), _boundIndexBuffer.Count,
                    TypeConverterGL3x.To(_boundIndexBuffer.DataType), new IntPtr());
            }
            else
            {
                GL.DrawArrays(TypeConverterGL3x.To(primitiveType), 0,
                    _boundVertexArray.MaximumArrayIndex() + 1);
            }
        }

        public override void Finish()
        {
            GL.Finish();
        }

        #endregion

        private void ApplyPrimitiveRestart(PrimitiveRestart primitiveRestart)
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

        private void ApplyFacetCulling(FacetCulling facetCulling)
        {
            if (_renderState.FacetCulling.Enabled != facetCulling.Enabled)
            {
                Enable(EnableCap.CullFace, facetCulling.Enabled);
                _renderState.FacetCulling.Enabled = facetCulling.Enabled;
            }

            if (_renderState.FacetCulling.Face != facetCulling.Face)
            {
                GL.CullFace(TypeConverterGL3x.To(facetCulling.Face));
                _renderState.FacetCulling.Face = facetCulling.Face;
            }

            if (_renderState.FacetCulling.FrontFaceWindingOrder != facetCulling.FrontFaceWindingOrder)
            {
                GL.FrontFace(TypeConverterGL3x.To(facetCulling.FrontFaceWindingOrder));
                _renderState.FacetCulling.FrontFaceWindingOrder = facetCulling.FrontFaceWindingOrder;
            }
        }

        private void ApplyProgramPointSize(ProgramPointSize programPointSize)
        {
            if (_renderState.ProgramPointSize != programPointSize)
            {
                Enable(EnableCap.ProgramPointSize, programPointSize == ProgramPointSize.Enabled);
                _renderState.ProgramPointSize = programPointSize;
            }
        }

        private void ApplyRasterizationMode(RasterizationMode rasterizationMode)
        {
            if (_renderState.RasterizationMode != rasterizationMode)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL3x.To(rasterizationMode));
                _renderState.RasterizationMode = rasterizationMode;
            }
        }

        private void ApplyScissorTest(ScissorTest scissorTest)
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

        private void ApplyStencilTest(StencilTest stencilTest)
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
                    TypeConverterGL3x.To(test.StencilFailOperation),
                    TypeConverterGL3x.To(test.DepthPassStencilFailOperation),
                    TypeConverterGL3x.To(test.DepthPassStencilPassOperation));

                currentTest.StencilFailOperation = test.StencilFailOperation;
                currentTest.DepthPassStencilFailOperation = test.DepthPassStencilFailOperation;
                currentTest.DepthPassStencilPassOperation = test.DepthPassStencilPassOperation;
            }

            if ((currentTest.Function != test.Function) ||
                (currentTest.ReferenceValue != test.ReferenceValue) ||
                (currentTest.Mask != test.Mask))
            {
                GL.StencilFuncSeparate(face,
                    TypeConverterGL3x.To(test.Function),
                    test.ReferenceValue,
                    test.Mask);

                currentTest.Function = test.Function;
                currentTest.ReferenceValue = test.ReferenceValue;
                currentTest.Mask = test.Mask;
            }
        }

        private void ApplyDepthTest(DepthTest depthTest)
        {
            if (_renderState.DepthTest.Enabled != depthTest.Enabled)
            {
                Enable(EnableCap.DepthTest, depthTest.Enabled);
                _renderState.DepthTest.Enabled = depthTest.Enabled;
            }

            if (_renderState.DepthTest.Function != depthTest.Function)
            {
                GL.DepthFunc(TypeConverterGL3x.To(depthTest.Function));
                _renderState.DepthTest.Function = depthTest.Function;
            }
        }

        private void ApplyDepthRange(DepthRange depthRange)
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

        private void ApplyBlending(Blending blending)
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
                    TypeConverterGL3x.To(blending.SourceRGBFactor),
                    TypeConverterGL3x.To(blending.DestinationRGBFactor),
                    TypeConverterGL3x.To(blending.SourceAlphaFactor),
                    TypeConverterGL3x.To(blending.DestinationAlphaFactor));

                _renderState.Blending.SourceRGBFactor = blending.SourceRGBFactor;
                _renderState.Blending.DestinationRGBFactor = blending.DestinationRGBFactor;
                _renderState.Blending.SourceAlphaFactor = blending.SourceAlphaFactor;
                _renderState.Blending.DestinationAlphaFactor = blending.DestinationAlphaFactor;
            }

            if ((_renderState.Blending.RGBEquation != blending.RGBEquation) ||
                (_renderState.Blending.AlphaEquation != blending.AlphaEquation))
            {
                GL.BlendEquationSeparate(
                    TypeConverterGL3x.To(blending.RGBEquation),
                    TypeConverterGL3x.To(blending.AlphaEquation));

                _renderState.Blending.RGBEquation = blending.RGBEquation;
                _renderState.Blending.AlphaEquation = blending.AlphaEquation;

            }

            if (_renderState.Blending.Color != blending.Color)
            {
                GL.BlendColor(blending.Color);
                _renderState.Blending.Color = blending.Color;
            }
        }

        private void ApplyColorMask(ColorMask colorMask)
        {
            if (_renderState.ColorMask != colorMask)
            {
                GL.ColorMask(colorMask.Red, colorMask.Green, colorMask.Blue, colorMask.Alpha);
                _renderState.ColorMask = colorMask;
            }
        }

        private void ApplyDepthWrite(bool depthWrite)
        {
            if (_renderState.DepthWrite != depthWrite)
            {
                GL.DepthMask(depthWrite);
                _renderState.DepthWrite = depthWrite;
            }
        }

        protected static void Enable(EnableCap enableCap, bool enable)
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

        private void VerifyDraw(DrawState drawState, SceneState sceneState)
        {
            if (drawState == null)
            {
                throw new ArgumentNullException("drawState");
            }

            if (drawState.RenderState == null)
            {
                throw new ArgumentNullException("drawState.RenderState");
            }

            if (drawState.ShaderProgram == null)
            {
                throw new ArgumentNullException("drawState.ShaderProgram");
            }

            if (drawState.VertexArray == null)
            {
                throw new ArgumentNullException("drawState.VertexArray");
            }

            if (sceneState == null)
            {
                throw new ArgumentNullException("sceneState");
            }
        }

        private void ApplyBeforeDraw(DrawState drawState, SceneState sceneState)
        {
            ApplyRenderState(drawState.RenderState);
            ApplyVertexArray(drawState.VertexArray);
            ApplyShaderProgram(drawState.ShaderProgram, sceneState);

            CleanTextureUnits();
            CleanFrameBuffer();
        }

        private void ApplyRenderState(RenderState renderState)
        {
            ApplyPrimitiveRestart(renderState.PrimitiveRestart);
            ApplyFacetCulling(renderState.FacetCulling);
            ApplyProgramPointSize(renderState.ProgramPointSize);
            ApplyRasterizationMode(renderState.RasterizationMode);
            ApplyScissorTest(renderState.ScissorTest);
            ApplyStencilTest(renderState.StencilTest);
            ApplyDepthTest(renderState.DepthTest);
            ApplyDepthRange(renderState.DepthRange);
            ApplyBlending(renderState.Blending);
            ApplyColorMask(renderState.ColorMask);
            ApplyDepthWrite(renderState.DepthWrite);
        }

        private void ApplyVertexArray(VertexArray vertexArray)
        {
            VertexArrayGL3x vertexArrayGL3x = vertexArray as VertexArrayGL3x;

            if (_boundVertexArray != vertexArrayGL3x)
            {
                vertexArrayGL3x.Bind();
                _boundVertexArray = vertexArrayGL3x;
            }

            _boundVertexArray.Clean();
            _boundIndexBuffer = _boundVertexArray.IndexBuffer as IndexBufferGL3x;
        }

        private void ApplyShaderProgram(ShaderProgram shaderProgram, SceneState sceneState)
        {
            ShaderProgramGL3x shaderProgramGL3x = shaderProgram as ShaderProgramGL3x;

            if (_boundShaderProgram != shaderProgramGL3x)
            {
                shaderProgramGL3x.Bind();
                _boundShaderProgram = shaderProgramGL3x;
            }
            _boundShaderProgram.Clean(this, sceneState);

#if DEBUG
            GL.ValidateProgram(_boundShaderProgram.Handle.Value);

            int validateStatus;
            GL.GetProgram(_boundShaderProgram.Handle.Value, ProgramParameter.ValidateStatus, out validateStatus);
            if (validateStatus == 0)
            {
                Debug.Fail("Shader program validation failed: " + _boundShaderProgram.Log);
            }
#endif
        }

        private void CleanTextureUnits()
        {
            _textureUnits.Clean();
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
        private VertexArrayGL3x _boundVertexArray;
        private IndexBufferGL3x _boundIndexBuffer;
        private ShaderProgramGL3x _boundShaderProgram;
        private FrameBufferGL3x _boundFrameBuffer;

        private TextureUnitsGL3x _textureUnits;
    }
}
