#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Renderer
{
    /// <summary>
    /// System tests for OpenGlobe.Renderer.  System tests are higher level 
    /// than unit tests; instead of validating a single class, system tests 
    /// use multiple classes to validate a more complicated task.
    /// </summary>
    [TestFixture]
    public class SystemTests
    {
        [Test]
        public void ClearColorDepth()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            Texture2DDescription depthDescription = new Texture2DDescription(1, 1, TextureFormat.Depth32f, false);
            Texture2D depthTexture = Device.CreateTexture2D(depthDescription);
            frameBuffer.DepthAttachment = depthTexture;

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Clear(new ClearState() { Buffers = ClearBuffers.All, Color = Color.Red, Depth = 0.5f });
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            ValidateDepth(frameBuffer.DepthAttachment, 0.5f);

            //
            // Scissor out window and verify clear doesn't modify contents
            //
            ScissorTest scissorTest = new ScissorTest();
            scissorTest.Enabled = true;
            scissorTest.Rectangle = new Rectangle(0, 0, 0, 0);

            window.Context.Clear(new ClearState() { ScissorTest = scissorTest, Buffers = ClearBuffers.All, Color = Color.Blue, Depth = 1 });
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            ValidateDepth(frameBuffer.DepthAttachment, 0.5f);

            depthTexture.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderPoint()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderTriangle()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());

            Vector4S[] positions = new[] 
            { 
                new Vector4S(-0.5f, -0.5f, 0, 1),
                new Vector4S(0.5f, -0.5f, 0, 1),
                new Vector4S(0.5f, 0.5f, 0, 1) 
            };
            VertexBuffer positionsBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * SizeInBytes<Vector4S>.Value);
            positionsBuffer.CopyFromSystemMemory(positions);

            ushort[] indices = new ushort[] 
            { 
                0, 1, 2
            };
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StaticDraw, indices.Length * sizeof(ushort));
            indexBuffer.CopyFromSystemMemory(indices);

            VertexArray va = window.Context.CreateVertexArray();
            va.VertexBuffers[sp.VertexAttributes["position"].Location] =
                new AttachedVertexBuffer(positionsBuffer, VertexAttributeComponentType.Float, 4);
            va.IndexBuffer = indexBuffer;

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Triangles, 0, 3, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            //
            // Verify detach
            //
            window.Context.Clear(new ClearState() { Buffers = ClearBuffers.ColorBuffer, Color = Color.FromArgb(0, 255, 0) });
            va.VertexBuffers[sp.VertexAttributes["position"].Location] = null;
            va.IndexBuffer = null;
            window.Context.Draw(PrimitiveType.Triangles, 0, 0, new DrawState(new RenderState(), sp, va), new SceneState());
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 0, 255, 0);

            //
            // Verify rendering without indices
            //
            va.VertexBuffers[sp.VertexAttributes["position"].Location] =
                new AttachedVertexBuffer(positionsBuffer, VertexAttributeComponentType.Float, 4);
            window.Context.Draw(PrimitiveType.Triangles, 0, 3, new DrawState(new RenderState(), sp, va), new SceneState());
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            positionsBuffer.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Renders one point with a 1x1 texture.
        /// </summary>
        [Test]
        public void RenderTexturedPoint()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            string fs =
                @"#version 330
                 
                  uniform sampler2D textureUnit;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = texture(textureUnit, vec2(0, 0));
                  }";

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);

            Uniform<int> textureUniform = sp.Uniforms["textureUnit"] as Uniform<int>;
            textureUniform.Value = 0;

            ///////////////////////////////////////////////////////////////////

            Texture2D texture = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            window.Context.TextureUnits[0].Texture2D = texture;
            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            texture.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderPointMultipleColorAttachments()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = window.Context.CreateFrameBuffer();

            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false);
            Texture2D redTexture = Device.CreateTexture2D(description);
            Texture2D greenTexture = Device.CreateTexture2D(description);

            window.Context.Viewport = new Rectangle(0, 0, 1, 1);

            string fs =
                @"#version 330
                 
                  out vec3 RedColor;
                  out vec3 GreenColor;

                  void main()
                  {
                      RedColor = vec3(1.0, 0.0, 0.0);
                      GreenColor = vec3(0.0, 1.0, 0.0);
                  }";

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            Assert.AreNotEqual(sp.FragmentOutputs["RedColor"], sp.FragmentOutputs["GreenColor"]);
            frameBuffer.ColorAttachments[sp.FragmentOutputs["RedColor"]] = redTexture;
            frameBuffer.ColorAttachments[sp.FragmentOutputs["GreenColor"]] = greenTexture;

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[1], 0, 255, 0);

            va.Dispose();
            sp.Dispose();
            greenTexture.Dispose();
            redTexture.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Renders one point with two 1x1 textures.
        /// </summary>
        [Test]
        public void RenderMultitexturedPoint()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader());

            ///////////////////////////////////////////////////////////////////

            Texture2D texture0 = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            Texture2D texture1 = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            window.Context.TextureUnits[0].Texture2D = texture0;
            window.Context.TextureUnits[1].Texture2D = texture1;
            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);

            va.Dispose();
            texture1.Dispose();
            texture0.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Renders a point to color and stencil buffers
        /// </summary>
        [Test]
        public void RenderPointWithStencil()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            Texture2DDescription depthStencilDescription = new Texture2DDescription(1, 1, TextureFormat.Depth24Stencil8, false);
            Texture2D depthStencilTexture = Device.CreateTexture2D(depthStencilDescription);
            frameBuffer.DepthStencilAttachment = depthStencilTexture;

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            StencilTest stencilTest = new StencilTest();
            stencilTest.Enabled = true;
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Replace;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Replace;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Replace;
            stencilTest.FrontFace.ReferenceValue = 1;

            RenderState renderState = new RenderState();
            renderState.StencilTest = stencilTest;

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Clear(new ClearState());
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(renderState, sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            ValidateStencil(frameBuffer.DepthStencilAttachment, stencilTest.FrontFace.ReferenceValue);

            va.Dispose();
            sp.Dispose();
            depthStencilTexture.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderPointWithDrawAutomaticUniforms()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_modelViewMatrix;
                  uniform mat4 og_perspectiveMatrix;
                  uniform mat4 og_modelViewPerspectiveMatrix;

                  void main()
                  {
                        if (position.x > 0)
                        {
                            mat4 modelViewMatrix = og_perspectiveMatrix * og_modelViewMatrix;
                            gl_Position = modelViewMatrix * position; 
                        }
                        else
                        {
                            gl_Position = og_modelViewPerspectiveMatrix * position; 
                        }
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, ShaderSources.PassThroughFragmentShader());
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            SceneState sceneState = new SceneState();
            sceneState.Camera.Eye = 2 * Vector3D.UnitX;
            sceneState.Camera.Target = Vector3D.Zero;
            sceneState.Camera.Up = Vector3D.UnitZ;

            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, new DrawState(new RenderState(), sp, va), sceneState);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderNonInterleavedVertexBuffer()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;               
                  layout(location = og_colorVertexLocation) in vec4 color;

                  out vec4 fsColor;

                  void main()                     
                  {
                      gl_Position = position; 
                      fsColor = color;
                  }",
                @"#version 330
                 
                  in vec4 fsColor;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = fsColor;
                  }");

            Vector4S[] positions = new[] { new Vector4S(0, 0, 0, 1) };
            BlittableRGBA[] colors = new[] { new BlittableRGBA(Color.Red) };
            int colorsOffset = positions.Length * SizeInBytes<Vector4S>.Value;

            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, 
                (positions.Length * SizeInBytes<Vector4S>.Value) + 
                (colors.Length * SizeInBytes<BlittableRGBA>.Value));
            vertexBuffer.CopyFromSystemMemory(positions);
            vertexBuffer.CopyFromSystemMemory(colors, colorsOffset);

            VertexArray va = window.Context.CreateVertexArray();
            va.VertexBuffers[sp.VertexAttributes["position"].Location] =
                new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 4);
            va.VertexBuffers[sp.VertexAttributes["color"].Location] =
                new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.UnsignedByte, 4, true, colorsOffset, 0);
            
            window.Context.FrameBuffer = frameBuffer;
            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct InterleavedVertex
        {
            public Vector4S Position { get; set; }
            public BlittableRGBA Color { get; set; }
        }

        [Test]
        public void RenderInterleavedVertexBuffer()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;               
                  layout(location = og_colorVertexLocation) in vec4 color;

                  out vec4 fsColor;

                  void main()                     
                  {
                      gl_Position = position; 
                      fsColor = color;
                  }",
                @"#version 330
                 
                  in vec4 fsColor;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = fsColor;
                  }");

            InterleavedVertex[] vertices = new InterleavedVertex[]
            {
                new InterleavedVertex()
                {
                    Position = new Vector4S(0, 0, 0, 1),
                    Color = new BlittableRGBA(Color.Red)
                },
                new InterleavedVertex()
                {
                    Position = new Vector4S(0, 0, 0, 1),
                    Color = new BlittableRGBA(Color.FromArgb(255, 0, 255, 0))
                }
            };
            int colorOffset = SizeInBytes<Vector4S>.Value;

            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, vertices.Length * SizeInBytes<InterleavedVertex>.Value);
            vertexBuffer.CopyFromSystemMemory(vertices);

            VertexArray va = window.Context.CreateVertexArray();
            va.VertexBuffers[sp.VertexAttributes["position"].Location] =
                new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 4, false, 0, SizeInBytes<InterleavedVertex>.Value);
            va.VertexBuffers[sp.VertexAttributes["color"].Location] =
                new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.UnsignedByte, 4, true, colorOffset, SizeInBytes<InterleavedVertex>.Value);

            window.Context.FrameBuffer = frameBuffer;

            window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(new RenderState(), sp, va), new SceneState());
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            RenderState rs = new RenderState();
            rs.DepthTest.Enabled = false;
            window.Context.Draw(PrimitiveType.Points, 1, 1, new DrawState(rs, sp, va), new SceneState());
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 0, 255, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        ///////////////////////////////////////////////////////////////////////

        private static void ValidateDepth(Texture2D depthTexture, float depth)
        {
            using (ReadPixelBuffer readPixelBuffer = depthTexture.CopyToBuffer(ImageFormat.DepthComponent, ImageDatatype.Float, 1))
            {
                float[] readDepth = readPixelBuffer.CopyToSystemMemory<float>();
                Assert.AreEqual (depth, readDepth[0]);
            }
        }

        private static void ValidateStencil(Texture2D depthStencilTexture, int stencil)
        {
            // TODO:  Don't call ReadPixels directly.  Reading back the FBO's depth/stencil is
            // not supported according to the spec.
            byte readStencil = 0;
            OpenTK.Graphics.OpenGL.GL.ReadPixels(0, 0, 1, 1, 
                OpenTK.Graphics.OpenGL.PixelFormat.StencilIndex,
                OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, ref readStencil);
            Assert.AreEqual(stencil, readStencil);
        }
    }
}
