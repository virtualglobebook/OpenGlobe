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
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Renderer.Multithreading
{
    [TestFixture]
    public class TextureMultithreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates a texture on a
        /// different thread.  The textures is then verified in the 
        /// rendering context.
        /// </summary>
        [Test]
        public void CreateTexture()
        {
            using (var threadWindow = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            {
                TextureFactory factory = new TextureFactory(threadWindow, new BlittableRGBA(Color.FromArgb(0, 1, 2, 3)));

                Thread t = new Thread(factory.Create);
                t.Start();
                t.Join();
                ///////////////////////////////////////////////////////////////////

                TestUtility.ValidateColor(factory.Texture, 1, 2, 3);
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two textures on two 
        /// different threads ran one after the other.  The textures are 
        /// used to render one point.
        /// </summary>
        [Test]
        public void CreateTexturesSequential()
        {
            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            {
                TextureFactory factory0 = new TextureFactory(thread0Window, new BlittableRGBA(Color.FromArgb(0, 127, 0, 0)));
                TextureFactory factory1 = new TextureFactory(thread1Window, new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));

                Thread t0 = new Thread(factory0.Create);
                t0.Start();
                t0.Join();

                Thread t1 = new Thread(factory1.Create);
                t1.Start();
                t1.Join();

                ///////////////////////////////////////////////////////////////////

                TestUtility.ValidateColor(factory0.Texture, 127, 0, 0);
                TestUtility.ValidateColor(factory1.Texture, 0, 255, 0);

                using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
                using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader()))
                using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
                {
                    window.Context.TextureUnits[0].Texture = factory0.Texture;
                    window.Context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
                    window.Context.TextureUnits[1].Texture = factory1.Texture;
                    window.Context.TextureUnits[1].TextureSampler = Device.TextureSamplers.NearestClamp;
                    window.Context.FrameBuffer = frameBuffer;
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 127, 255, 0);
                }
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two textures on two 
        /// different threads ran one in parallel.  The textures are 
        /// used to render one point.
        /// </summary>
        [Test]
        public void CreateTexturesParallel()
        {
            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            {
                TextureFactory factory0 = new TextureFactory(thread0Window, new BlittableRGBA(Color.FromArgb(0, 255, 0, 0)));
                TextureFactory factory1 = new TextureFactory(thread1Window, new BlittableRGBA(Color.FromArgb(0, 0, 255, 0)));

                Thread t0 = new Thread(factory0.Create);
                Thread t1 = new Thread(factory1.Create);

                t0.Start();
                t1.Start();

                t0.Join();
                t1.Join();

                ///////////////////////////////////////////////////////////////////

                TestUtility.ValidateColor(factory0.Texture, 255, 0, 0);
                TestUtility.ValidateColor(factory1.Texture, 0, 255, 0);

                using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
                using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader()))
                using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
                {
                    window.Context.TextureUnits[0].Texture = factory0.Texture;
                    window.Context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
                    window.Context.TextureUnits[1].Texture = factory1.Texture;
                    window.Context.TextureUnits[1].TextureSampler = Device.TextureSamplers.NearestClamp;
                    window.Context.FrameBuffer = frameBuffer;
                    window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                    TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);
                }
            }
        }
    }
}
