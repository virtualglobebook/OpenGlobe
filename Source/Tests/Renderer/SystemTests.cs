#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Threading;
using System.Drawing;
using NUnit.Framework;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Renderer
{
    /// <summary>
    /// System tests for MiniGlobe.Renderer.  System tests are higher level 
    /// than unit tests; instead of validating a single class, system tests 
    /// use multiple classes to validate a more complicated task.
    /// </summary>
    [TestFixture]
    public class SystemTests
    {
        [Test]
        public void ClearColorDepth()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            Texture2DDescription depthDescription = new Texture2DDescription(1, 1, TextureFormat.Depth32f, false);
            Texture2D depthTexture = Device.CreateTexture2D(depthDescription);
            frameBuffer.DepthAttachment = depthTexture;
            window.Context.Bind(frameBuffer);

            window.Context.Clear(ClearBuffers.All, Color.Red, 0.5f, 0);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            ValidateDepth(frameBuffer.DepthAttachment, 0.5f);

            //
            // Scissor out window and verify clear doesn't modify contents
            //
            RenderState renderState = new RenderState();
            renderState.ScissorTest.Enabled = true;
            renderState.ScissorTest.Rectangle = new Rectangle(0, 0, 0, 0);
            window.Context.Bind(renderState);

            window.Context.Clear(ClearBuffers.All, Color.Blue, 1, 0);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);
            ValidateDepth(frameBuffer.DepthAttachment, 0.5f);

            depthTexture.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderPoint()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void RenderTriangle()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
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

            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Triangles, 0, 3);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            //
            // Verify detach
            //
            window.Context.Clear(ClearBuffers.ColorBuffer, Color.FromArgb(0, 255, 0), 1, 0);
            va.VertexBuffers[sp.VertexAttributes["position"].Location] = null;
            va.IndexBuffer = null;
            window.Context.Draw(PrimitiveType.Triangles, 0, 0);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 0, 255, 0);

            //
            // Verify rendering without indices
            //
            va.VertexBuffers[sp.VertexAttributes["position"].Location] =
                new AttachedVertexBuffer(positionsBuffer, VertexAttributeComponentType.Float, 4);
            window.Context.Draw(PrimitiveType.Triangles, 0, 3);
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
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            string fs =
                @"#version 150
                 
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

            Texture2D texture = CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            window.Context.TextureUnits[0].Texture2D = texture;
            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

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
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = window.Context.CreateFrameBuffer();

            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false);
            Texture2D redTexture = Device.CreateTexture2D(description);
            Texture2D greenTexture = Device.CreateTexture2D(description);

            window.Context.Viewport = new Rectangle(0, 0, 1, 1);

            string fs =
                @"#version 150
                 
                  out vec3 RedColor;
                  out vec3 GreenColor;

                  void main()
                  {
                      RedColor = vec3(1.0, 0.0, 0.0);
                      GreenColor = vec3(0.0, 1.0, 0.0);
                  }";

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs);
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            Assert.AreNotEqual(sp.FragmentOutputs["RedColor"], sp.FragmentOutputs["GreenColor"]);
            frameBuffer.ColorAttachments[sp.FragmentOutputs["RedColor"]] = redTexture;
            frameBuffer.ColorAttachments[sp.FragmentOutputs["GreenColor"]] = greenTexture;

            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

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
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), MultitextureFragmentShader());

            ///////////////////////////////////////////////////////////////////

            Texture2D texture0 = CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            Texture2D texture1 = CreateTexture(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            window.Context.TextureUnits[0].Texture2D = texture0;
            window.Context.TextureUnits[1].Texture2D = texture1;
            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);

            va.Dispose();
            texture1.Dispose();
            texture0.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Creates two 1x1 textures on two different threads, then 
        /// uses the textures to render one point.
        /// </summary>
        [Test]
        [Ignore]
        public void RenderMultitexturedPointThreads()
        {
            // TODO:  If the main thread context is created first, this test does not pass.
            ///////////////////////////////////////////////////////////////////

            TextureFactory factory0 = new TextureFactory(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            TextureFactory factory1 = new TextureFactory(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));

            Thread t0 = new Thread(factory0.Create);
            t0.Start();
            t0.Join();
            //factory0.Create();

            Thread t1 = new Thread(factory1.Create);
            t1.Start();
            t1.Join();
            //factory1.Create();

            ///////////////////////////////////////////////////////////////////

            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), MultitextureFragmentShader());

            ///////////////////////////////////////////////////////////////////

            Texture2D texture0 = CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            Texture2D texture1 = CreateTexture(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            window.Context.TextureUnits[0].Texture2D = factory0.Texture;
            window.Context.TextureUnits[1].Texture2D = factory1.Texture;
            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);

            va.Dispose();
            texture1.Dispose();
            texture0.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
            factory1.Dispose();
            factory0.Dispose();
        }

        /// <summary>
        /// Renders a point to color and stencil buffers
        /// </summary>
        [Test]
        public void RenderPointWithStencil()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            Texture2DDescription depthStencilDescription = new Texture2DDescription(1, 1, TextureFormat.Depth24Stencil8, false);
            Texture2D depthStencilTexture = Device.CreateTexture2D(depthStencilDescription);
            frameBuffer.DepthStencilAttachment = depthStencilTexture;

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            ///////////////////////////////////////////////////////////////////

            StencilTest stencilTest = new StencilTest();
            stencilTest.Enabled = true;
            stencilTest.FrontFace.DepthPassStencilFailOperation = StencilOperation.Replace;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Replace;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Replace;
            stencilTest.FrontFace.ReferenceValue = 1;

            RenderState renderState = new RenderState();
            renderState.StencilTest = stencilTest;

            window.Context.Bind(frameBuffer);
            window.Context.Clear(ClearBuffers.All, Color.Black, 1, 0);
            window.Context.Bind(renderState);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

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
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            string vs =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_modelViewMatrix;
                  uniform mat4 mg_perspectiveProjectionMatrix;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                        if (position.x > 0)
                        {
                            mat4 modelViewProjectionMatrix = mg_perspectiveProjectionMatrix * mg_modelViewMatrix;
                            gl_Position = modelViewProjectionMatrix * position; 
                        }
                        else
                        {
                            gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 
                        }
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, ShaderSources.PassThroughFragmentShader());
            VertexArray va = CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            SceneState sceneState = new SceneState();
            sceneState.Camera.Eye = 2 * Vector3D.UnitX;
            sceneState.Camera.Target = Vector3D.Zero;
            sceneState.Camera.Up = Vector3D.UnitZ;

            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, sceneState);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }

        ///////////////////////////////////////////////////////////////////////

        public static string MultitextureFragmentShader()
        {
            return
                @"#version 150
                 
                  uniform sampler2D mg_texture0;
                  uniform sampler2D mg_texture1;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(
                          texture(mg_texture0, vec2(0, 0)).r,
                          texture(mg_texture1, vec2(0, 0)).g, 0, 1);
                  }";
        }

        public static Texture2D CreateTexture(BlittableRGBA rgba)
        {
            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlueAlpha8, false);
            Texture2D texture = Device.CreateTexture2D(description);

            BlittableRGBA[] pixels = new BlittableRGBA[] { rgba };

            int sizeInBytes = pixels.Length * SizeInBytes<BlittableRGBA>.Value;
            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, sizeInBytes))
            {
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.RedGreenBlueAlpha, ImageDataType.UnsignedByte);
            }
            texture.Filter = Texture2DFilter.NearestClampToEdge;

            return texture;
        }

        private static VertexArray CreateVertexArray(Context context, int positionLocation)
        {
            Vector4S[] positions = new[] { new Vector4S(0, 0, 0, 1) };
            VertexBuffer positionsBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, positions.Length * SizeInBytes<Vector4S>.Value);
            positionsBuffer.CopyFromSystemMemory(positions);

            VertexArray va = context.CreateVertexArray();
            va.VertexBuffers[positionLocation] =
                new AttachedVertexBuffer(positionsBuffer, VertexAttributeComponentType.Float, 4);

            return va;
        }

        private static void ValidateDepth(Texture2D depthTexture, float depth)
        {
            using (ReadPixelBuffer readPixelBuffer = depthTexture.CopyToBuffer(ImageFormat.DepthComponent, ImageDataType.Float, 1))
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

    internal class TextureFactory : Disposable
    {
        public TextureFactory(BlittableRGBA rgba)
	    {
            _rgba = rgba;
    	}

        public void Create()
        {
            //_window = new NativeWindow();
            //_context = new GraphicsContext(new GraphicsMode(32, 24, 8), _window.WindowInfo, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);

            _window = Device.CreateWindow(1, 1);
            _texture = SystemTests.CreateTexture(_rgba);

            // TODO:  Don't call Flush directly.
            OpenTK.Graphics.OpenGL.GL.Flush();
        }

        public Texture2D Texture 
        {
            get { return _texture; }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _texture.Dispose();
                _window.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        //private NativeWindow _window;
        //private GraphicsContext _context;

        private MiniGlobeWindow _window;
        private Texture2D _texture;
        private BlittableRGBA _rgba;
    }
}
