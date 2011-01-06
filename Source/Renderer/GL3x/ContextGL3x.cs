#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ContextGL3x : Context
    {
        public ContextGL3x(GameWindow gameWindow, int width, int height)
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
            ForceApplyRenderState(_renderState);

            Viewport = new Rectangle(0, 0, width, height);
            _gameWindow = gameWindow;
        }

        #region ForceApplyRenderState

        private static void ForceApplyRenderState(RenderState renderState)
        {
            Enable(EnableCap.PrimitiveRestart, renderState.PrimitiveRestart.Enabled);
            GL.PrimitiveRestartIndex(renderState.PrimitiveRestart.Index);

            Enable(EnableCap.CullFace, renderState.FacetCulling.Enabled);
            GL.CullFace(TypeConverterGL3x.To(renderState.FacetCulling.Face));
            GL.FrontFace(TypeConverterGL3x.To(renderState.FacetCulling.FrontFaceWindingOrder));

            Enable(EnableCap.ProgramPointSize, renderState.ProgramPointSize == ProgramPointSize.Enabled);
            GL.PolygonMode(MaterialFace.FrontAndBack, TypeConverterGL3x.To(renderState.RasterizationMode));

            Enable(EnableCap.RasterizerDiscard, !renderState.Rasterizer);

            Enable(EnableCap.ScissorTest, renderState.ScissorTest.Enabled);
            Rectangle rectangle = renderState.ScissorTest.Rectangle;
            GL.Scissor(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);

            Enable(EnableCap.StencilTest, renderState.StencilTest.Enabled);
            ForceApplyRenderStateStencil(StencilFace.Front, renderState.StencilTest.FrontFace);
            ForceApplyRenderStateStencil(StencilFace.Back, renderState.StencilTest.BackFace);

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

            GL.DepthMask(renderState.DepthMask);
            GL.ColorMask(renderState.ColorMask.Red, renderState.ColorMask.Green, 
                renderState.ColorMask.Blue, renderState.ColorMask.Alpha);
        }

        private static void ForceApplyRenderStateStencil(StencilFace face, StencilTestFace test)
        {
            GL.StencilOpSeparate(face,
                TypeConverterGL3x.To(test.StencilFailOperation),
                TypeConverterGL3x.To(test.DepthFailStencilPassOperation),
                TypeConverterGL3x.To(test.DepthPassStencilPassOperation));

            GL.StencilFuncSeparate(face,
                TypeConverterGL3x.To(test.Function),
                test.ReferenceValue,
                test.Mask);
        }

        #endregion

        #region Context Members

        public override void MakeCurrent()
        {
            _gameWindow.MakeCurrent();
        }

        public override VertexArray CreateVertexArray()
        {
            return new VertexArrayGL3x();
        }

        public override Framebuffer CreateFramebuffer()
        {
            return new FramebufferGL3x();
        }

        public override TextureUnits TextureUnits
        {
            get { return _textureUnits; }
        }

        public override Rectangle Viewport
        {
            get
            {
                return _viewport;
            }

            set
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException("Viewport", "The viewport width and height must be greater than or equal to zero.");
                }

                if (_viewport != value)
                {
                    _viewport = value;
                    GL.Viewport(value);
                }
            }
        }

        public override Framebuffer Framebuffer
        {
            get { return _setFramebuffer; }
            set { _setFramebuffer = (FramebufferGL3x)value; }
        }

        protected override void DoClear(ClearState clearState)
        {
            ApplyFramebuffer();

            ApplyScissorTest(clearState.ScissorTest);
            ApplyColorMask(clearState.ColorMask);
            ApplyDepthMask(clearState.DepthMask);
            // TODO: StencilMaskSeparate

            if (_clearColor != clearState.Color)
            {
                GL.ClearColor(clearState.Color);
                _clearColor = clearState.Color;
            }

            if (_clearDepth != clearState.Depth)
            {
                GL.ClearDepth((double)clearState.Depth);
                _clearDepth = clearState.Depth;
            }

            if (_clearStencil != clearState.Stencil)
            {
                GL.ClearStencil(clearState.Stencil);
                _clearStencil = clearState.Stencil;
            }

            GL.Clear(TypeConverterGL3x.To(clearState.Buffers));
        }

        public override void BeginTransformFeedback(
            TransformFeedbackPrimitiveType primitiveType,
            IEnumerable<Buffer> buffers)
        {
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }

            int i = 0;
            foreach (Buffer buffer in buffers)
            {
                int name = ((IBufferName)buffer).Name;
                GL.BindBufferBase(BufferTarget.TransformFeedbackBuffer, i++, name);

                // TF_TODO:  Support optional index and offset with each buffer, e.g.:
                //
                //   GL.BindBufferRange(BufferTarget.TransformFeedbackBuffer, i++, name, 
                //       new IntPtr(0), new IntPtr(16));
            }

            GL.BeginTransformFeedback(TypeConverterGL3x.To(primitiveType));
        }

        public override void EndBeginTransform()
        {
            GL.EndTransformFeedback();

            for (int i = 0; i < Device.MaximumTransformFeedbackSeparateAttributes; ++i)
            {
                GL.BindBufferBase(BufferTarget.TransformFeedbackBuffer, i, 0);
            }
        }

        protected override void DoDraw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState)
        {
            VerifyDraw(drawState, sceneState);
            ApplyBeforeDraw(drawState, sceneState);

            VertexArrayGL3x vertexArray = (VertexArrayGL3x)drawState.VertexArray;
            IndexBufferGL3x indexBuffer = vertexArray.IndexBuffer as IndexBufferGL3x;

            // TF_TODO:
            //int result;
            //int name;
            //GL.GenQueries(1, out name);
            //GL.BeginQuery(QueryTarget.TransformFeedbackPrimitivesWritten, name);

            if (indexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL3x.To(primitiveType),
                    0, vertexArray.MaximumArrayIndex(), count,
                    TypeConverterGL3x.To(indexBuffer.Datatype), new
                    IntPtr(offset * VertexArraySizes.SizeOf(indexBuffer.Datatype)));
            }
            else
            {
                GL.DrawArrays(TypeConverterGL3x.To(primitiveType), offset, count);
            }

            // TF_TODO:
            //GL.EndQuery(QueryTarget.PrimitivesGenerated);
            //GL.EndQuery(QueryTarget.TransformFeedbackPrimitivesWritten);
            //GL.GetQueryObject(name, GetQueryObjectParam.QueryResult, out result);
            //GL.DeleteQueries(1, ref name);
        }

        protected override void DoDraw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState)
        {
            VerifyDraw(drawState, sceneState);
            ApplyBeforeDraw(drawState, sceneState);

            VertexArrayGL3x vertexArray = (VertexArrayGL3x)drawState.VertexArray;
            IndexBufferGL3x indexBuffer = vertexArray.IndexBuffer as IndexBufferGL3x;

            if (indexBuffer != null)
            {
                GL.DrawRangeElements(TypeConverterGL3x.To(primitiveType),
                    0, vertexArray.MaximumArrayIndex(), indexBuffer.Count,
                    TypeConverterGL3x.To(indexBuffer.Datatype), new IntPtr());
            }
            else
            {
                GL.DrawArrays(TypeConverterGL3x.To(primitiveType), 0,
                    vertexArray.MaximumArrayIndex() + 1);
            }
        }

        #endregion

        private void ApplyPrimitiveRestart(PrimitiveRestart primitiveRestart)
        {
            if (_renderState.PrimitiveRestart.Enabled != primitiveRestart.Enabled)
            {
                Enable(EnableCap.PrimitiveRestart, primitiveRestart.Enabled);
                _renderState.PrimitiveRestart.Enabled = primitiveRestart.Enabled;
            }

            if (primitiveRestart.Enabled)
            {
                if (_renderState.PrimitiveRestart.Index != primitiveRestart.Index)
                {
                    GL.PrimitiveRestartIndex(primitiveRestart.Index);
                    _renderState.PrimitiveRestart.Index = primitiveRestart.Index;
                }
            }
        }

        private void ApplyFacetCulling(FacetCulling facetCulling)
        {
            if (_renderState.FacetCulling.Enabled != facetCulling.Enabled)
            {
                Enable(EnableCap.CullFace, facetCulling.Enabled);
                _renderState.FacetCulling.Enabled = facetCulling.Enabled;
            }

            if (facetCulling.Enabled)
            {
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

        private void ApplyRasterizer(bool rasterizer)
        {
            if (_renderState.Rasterizer != rasterizer)
            {
                Enable(EnableCap.RasterizerDiscard, !rasterizer);
                _renderState.Rasterizer = rasterizer;
            }
        }

        private void ApplyScissorTest(ScissorTest scissorTest)
        {
            Rectangle rectangle = scissorTest.Rectangle;

            if (rectangle.Width < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "renderState.ScissorTest.Rectangle.Width must be greater than or equal to zero.", 
                    "renderState");
            }

            if (rectangle.Height < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "renderState.ScissorTest.Rectangle.Height must be greater than or equal to zero.",
                    "renderState");
            }

            if (_renderState.ScissorTest.Enabled != scissorTest.Enabled)
            {
                Enable(EnableCap.ScissorTest, scissorTest.Enabled);
                _renderState.ScissorTest.Enabled = scissorTest.Enabled;
            }

            if (scissorTest.Enabled)
            {
                if (_renderState.ScissorTest.Rectangle != scissorTest.Rectangle)
                {
                    GL.Scissor(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                    _renderState.ScissorTest.Rectangle = scissorTest.Rectangle;
                }
            }
        }

        private void ApplyStencilTest(StencilTest stencilTest)
        {
            if (_renderState.StencilTest.Enabled != stencilTest.Enabled)
            {
                Enable(EnableCap.StencilTest, stencilTest.Enabled);
                _renderState.StencilTest.Enabled = stencilTest.Enabled;
            }

            if (stencilTest.Enabled)
            {
                ApplyStencil(StencilFace.Front, _renderState.StencilTest.FrontFace, stencilTest.FrontFace);
                ApplyStencil(StencilFace.Back, _renderState.StencilTest.BackFace, stencilTest.BackFace);
            }
        }

        private static void ApplyStencil(StencilFace face, StencilTestFace currentTest, StencilTestFace test)
        {
            if ((currentTest.StencilFailOperation != test.StencilFailOperation) ||
                (currentTest.DepthFailStencilPassOperation != test.DepthFailStencilPassOperation) ||
                (currentTest.DepthPassStencilPassOperation != test.DepthPassStencilPassOperation))
            {
                GL.StencilOpSeparate(face,
                    TypeConverterGL3x.To(test.StencilFailOperation),
                    TypeConverterGL3x.To(test.DepthFailStencilPassOperation),
                    TypeConverterGL3x.To(test.DepthPassStencilPassOperation));

                currentTest.StencilFailOperation = test.StencilFailOperation;
                currentTest.DepthFailStencilPassOperation = test.DepthFailStencilPassOperation;
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

            if (depthTest.Enabled)
            {
                if (_renderState.DepthTest.Function != depthTest.Function)
                {
                    GL.DepthFunc(TypeConverterGL3x.To(depthTest.Function));
                    _renderState.DepthTest.Function = depthTest.Function;
                }
            }
        }

        private void ApplyDepthRange(DepthRange depthRange)
        {
            if (depthRange.Near < 0.0 || depthRange.Near > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    "renderState.DepthRange.Near must be between zero and one.",
                    "depthRange");
            }

            if (depthRange.Far < 0.0 || depthRange.Far > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    "renderState.DepthRange.Far must be between zero and one.",
                    "depthRange");
            }

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

            if (blending.Enabled)
            {
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
        }

        private void ApplyColorMask(ColorMask colorMask)
        {
            if (_renderState.ColorMask != colorMask)
            {
                GL.ColorMask(colorMask.Red, colorMask.Green, colorMask.Blue, colorMask.Alpha);
                _renderState.ColorMask = colorMask;
            }
        }

        private void ApplyDepthMask(bool depthMask)
        {
            if (_renderState.DepthMask != depthMask)
            {
                GL.DepthMask(depthMask);
                _renderState.DepthMask = depthMask;
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

            if (_setFramebuffer != null)
            {
                if (drawState.RenderState.DepthTest.Enabled &&
                    !((_setFramebuffer.DepthAttachment != null) || 
                      (_setFramebuffer.DepthStencilAttachment != null)))
                {
                    throw new ArgumentException("The depth test is enabled (drawState.RenderState.DepthTest.Enabled) but the context's Framebuffer property doesn't have a depth or depth/stencil attachment (DepthAttachment or DepthStencilAttachment).", "drawState");
                }
            }
        }

        private void ApplyBeforeDraw(DrawState drawState, SceneState sceneState)
        {
            ApplyRenderState(drawState.RenderState);
            ApplyVertexArray(drawState.VertexArray);
            ApplyShaderProgram(drawState, sceneState);

            _textureUnits.Clean();
            ApplyFramebuffer();
        }

        private void ApplyRenderState(RenderState renderState)
        {
            ApplyPrimitiveRestart(renderState.PrimitiveRestart);
            ApplyFacetCulling(renderState.FacetCulling);
            ApplyProgramPointSize(renderState.ProgramPointSize);
            ApplyRasterizer(renderState.Rasterizer);
            ApplyRasterizationMode(renderState.RasterizationMode);
            ApplyScissorTest(renderState.ScissorTest);
            ApplyStencilTest(renderState.StencilTest);
            ApplyDepthTest(renderState.DepthTest);
            ApplyDepthRange(renderState.DepthRange);
            ApplyBlending(renderState.Blending);
            ApplyColorMask(renderState.ColorMask);
            ApplyDepthMask(renderState.DepthMask);
        }

        private void ApplyVertexArray(VertexArray vertexArray)
        {
            VertexArrayGL3x vertexArrayGL3x = (VertexArrayGL3x)vertexArray;
            vertexArrayGL3x.Bind();
            vertexArrayGL3x.Clean();
        }

        private void ApplyShaderProgram(DrawState drawState, SceneState sceneState)
        {
            ShaderProgramGL3x shaderProgramGL3x = (ShaderProgramGL3x)drawState.ShaderProgram;

            if (_boundShaderProgram != shaderProgramGL3x)
            {
                shaderProgramGL3x.Bind();
                _boundShaderProgram = shaderProgramGL3x;
            }
            _boundShaderProgram.Clean(this, drawState, sceneState);

#if DEBUG
            GL.ValidateProgram(_boundShaderProgram.Handle.Value);

            int validateStatus;
            GL.GetProgram(_boundShaderProgram.Handle.Value, ProgramParameter.ValidateStatus, out validateStatus);
            if (validateStatus == 0)
            {
                throw new ArgumentException(
                    "Shader program validation failed: " + _boundShaderProgram.Log, 
                    "drawState.ShaderProgram");
            }
#endif
        }

        private void ApplyFramebuffer()
        {
            if (_boundFramebuffer != _setFramebuffer)
            {
                if (_setFramebuffer != null)
                {
                    _setFramebuffer.Bind();
                }
                else
                {
                    FramebufferGL3x.UnBind();
                }

                _boundFramebuffer = _setFramebuffer;
            }

            if (_setFramebuffer != null)
            {
                _setFramebuffer.Clean();
#if DEBUG
                FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (errorCode != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new InvalidOperationException("Frame buffer is incomplete.  Error code: " +
                        errorCode.ToString());
                }
#endif
            }
        }

        private Color _clearColor;
        private float _clearDepth;
        private int _clearStencil;
        private Rectangle _viewport;

        private RenderState _renderState;
        private ShaderProgramGL3x _boundShaderProgram;
        private FramebufferGL3x _boundFramebuffer;
        private FramebufferGL3x _setFramebuffer;

        private TextureUnitsGL3x _textureUnits;

        private GameWindow _gameWindow;
    }
}
