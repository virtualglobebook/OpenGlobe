#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Threading;
using System.Drawing;
using NUnit.Framework;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Renderer
{
    [TestFixture]
    [Explicit]
    public class MultiThreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates two textures on two 
        /// different threads ran one after the other.  The textures are 
        /// used to render one point.
        /// </summary>
        [Test]
        [Explicit]
        public void CreateTexturesSequential()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
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

            TestUtility.ValidateColor(factory0.Texture, 255, 0, 0);
            TestUtility.ValidateColor(factory1.Texture, 0, 255, 0);

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);
            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader());
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            window.Context.TextureUnits[0].Texture2D = factory0.Texture;
            window.Context.TextureUnits[1].Texture2D = factory1.Texture;
            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
            factory1.Dispose();
            factory0.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates two textures on two 
        /// different threads ran one in parallel.  The textures are 
        /// used to render one point.
        /// </summary>
        [Test]
        public void CreateTexturesParallel()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            ///////////////////////////////////////////////////////////////////

            TextureFactory factory0 = new TextureFactory(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
            TextureFactory factory1 = new TextureFactory(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));

            Thread t0 = new Thread(factory0.Create);
            t0.Start();

            Thread t1 = new Thread(factory1.Create);
            t1.Start();

            //factory0.Create();
            //factory1.Create();
            t0.Join();
            t1.Join();

            ///////////////////////////////////////////////////////////////////

            TestUtility.ValidateColor(factory0.Texture, 255, 0, 0);
            TestUtility.ValidateColor(factory1.Texture, 0, 255, 0);

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);
            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader());
            VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location);

            window.Context.TextureUnits[0].Texture2D = factory0.Texture;
            window.Context.TextureUnits[1].Texture2D = factory1.Texture;
            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
            factory1.Dispose();
            factory0.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates a shader program on a
        /// different thread.  The shader program is used to render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgram()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            ///////////////////////////////////////////////////////////////////

            ShaderProgramFactory factory = new ShaderProgramFactory(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());

            Thread t1 = new Thread(factory.Create);
            t1.Start();
            t1.Join();
            //factory.Create();

            ///////////////////////////////////////////////////////////////////

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            VertexArray va = TestUtility.CreateVertexArray(window.Context, factory.ShaderProgram.VertexAttributes["position"].Location);

            window.Context.Bind(frameBuffer);
            window.Context.Bind(factory.ShaderProgram);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            factory.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }
    }
}
