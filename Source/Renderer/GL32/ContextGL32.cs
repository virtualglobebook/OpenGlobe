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

            GL.Clear(TypeConverterGL3x.To(buffers));
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
                GL.CullFace(TypeConverterGL3x.To(facetCulling.Face));
                _renderState.FacetCulling.Face = facetCulling.Face;
            }

            if (_renderState.FacetCulling.FrontFaceWindingOrder != facetCulling.FrontFaceWindingOrder)
            {
                GL.FrontFace(TypeConverterGL3x.To(facetCulling.FrontFaceWindingOrder));
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
                GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL3x.To(rasterizationMode));
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

        public override void Bind(DepthTest depthTest)
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
            VertexArrayGL3x vertexArrayGL3x = vertexArray as VertexArrayGL3x;

            if (_boundVertexArray != vertexArrayGL3x)
            {
                vertexArrayGL3x.Bind();
                _boundVertexArray = vertexArrayGL3x;
            }
        }

        public override void Bind(ShaderProgram shaderProgram)
        {
            ShaderProgramGL3x shaderProgramGL3x = shaderProgram as ShaderProgramGL3x;

            if (_boundShaderProgram != shaderProgramGL3x)
            {
                shaderProgramGL3x.Bind();
                _boundShaderProgram = shaderProgramGL3x;
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

        public override void Draw(PrimitiveType primitiveType, int offset, int count, SceneState sceneState)
        {
            CleanBeforeDraw(sceneState);

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

        public override void Draw(PrimitiveType primitiveType, SceneState sceneState)
        {
            CleanBeforeDraw(sceneState);

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
            _boundIndexBuffer = _boundVertexArray.IndexBuffer as IndexBufferGL3x;
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
        private VertexArrayGL3x _boundVertexArray;
        private IndexBufferGL3x _boundIndexBuffer;
        private ShaderProgramGL3x _boundShaderProgram;
        private FrameBufferGL3x _boundFrameBuffer;

        private TextureUnitsGL3x _textureUnits;
    }
}
