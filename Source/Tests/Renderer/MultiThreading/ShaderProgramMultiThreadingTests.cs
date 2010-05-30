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

namespace MiniGlobe.Renderer.MultiThreading
{
    [Explicit]
    [TestFixture]
    public class ShaderProgramMultiThreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates a shader program on a
        /// different thread.  The shader program is used to render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgram()
        {
            var threadWindow = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);
            ///////////////////////////////////////////////////////////////////

            ShaderProgramFactory factory = new ShaderProgramFactory(threadWindow, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());

            Thread t = new Thread(factory.Create);
            t.Start();
            t.Join();

            ///////////////////////////////////////////////////////////////////

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            VertexArray va = TestUtility.CreateVertexArray(window.Context, factory.ShaderProgram.VertexAttributes["position"].Location);

            window.Context.Bind(frameBuffer);
            window.Context.Bind(factory.ShaderProgram);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Points, 0, 1);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            frameBuffer.Dispose();
            factory.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates two shader programs on two 
        /// different threads ran one after the other.  The shader programs are 
        /// used to render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgramSequential()
        {
            var thread0Window = Device.CreateWindow(1, 1);
            var thread1Window = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);
            ///////////////////////////////////////////////////////////////////

            ShaderProgramFactory factory0 = new ShaderProgramFactory(thread0Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            Thread t0 = new Thread(factory0.Create);
            t0.Start();
            t0.Join();

            ShaderProgramFactory factory1 = new ShaderProgramFactory(thread1Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            Thread t1 = new Thread(factory1.Create);
            t1.Start();
            t1.Join();

            ///////////////////////////////////////////////////////////////////

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);
            VertexArray va = TestUtility.CreateVertexArray(window.Context, factory0.ShaderProgram.VertexAttributes["position"].Location);
            window.Context.Bind(frameBuffer);
            window.Context.Bind(va);

            window.Context.Bind(factory0.ShaderProgram);
            window.Context.Draw(PrimitiveType.Points, 0, 1);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);

            window.Context.Bind(factory1.ShaderProgram);
            window.Context.Draw(PrimitiveType.Points, 0, 1);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            frameBuffer.Dispose();
            factory1.Dispose();
            factory0.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates two shader programs on two 
        /// different threads ran in parallel.  The shader programs are used to
        /// render one point.
        /// </summary>
        [Test]
        public void CreateShaderProgramParallel()
        {
            var thread0Window = Device.CreateWindow(1, 1);
            var thread1Window = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);
            ///////////////////////////////////////////////////////////////////

            ShaderProgramFactory factory0 = new ShaderProgramFactory(thread0Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            Thread t0 = new Thread(factory0.Create);
            t0.Start();

            ShaderProgramFactory factory1 = new ShaderProgramFactory(thread1Window, ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            Thread t1 = new Thread(factory1.Create);
            t1.Start();

            t0.Join();
            t1.Join();
            ///////////////////////////////////////////////////////////////////

            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);
            VertexArray va = TestUtility.CreateVertexArray(window.Context, factory0.ShaderProgram.VertexAttributes["position"].Location);
            window.Context.Bind(frameBuffer);
            window.Context.Bind(va);

            window.Context.Bind(factory0.ShaderProgram);
            window.Context.Draw(PrimitiveType.Points, 0, 1);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);

            window.Context.Bind(factory1.ShaderProgram);
            window.Context.Draw(PrimitiveType.Points, 0, 1);
            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            frameBuffer.Dispose();
            factory1.Dispose();
            factory0.Dispose();
            window.Dispose();
        }
    }
}
