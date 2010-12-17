#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Threading;
using System.Drawing;
using OpenGlobe.Core;
using NUnit.Framework;

namespace OpenGlobe.Renderer.Multithreading
{
    [TestFixture]
    public class VertexBufferMultithreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates a vertex buffer on a
        /// different thread.  The vertex buffers is then verified in the 
        /// rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffer()
        {
            Vector3F[] positions = new Vector3F[] { new Vector3F(1, 2, 3) };

            using (var threadWindow = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (VertexBufferFactory factory = new VertexBufferFactory(threadWindow.Context, positions))
            {
                Thread t = new Thread(factory.Create);
                t.Start();
                t.Join();

                Assert.AreEqual(positions[0], factory.VertexBuffer.CopyToSystemMemory<Vector3F>()[0]);
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two vertex buffers on two 
        /// different threads ran one after the other.  The vertex buffers is then 
        /// verified in the rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffersSequential()
        {
            Vector3F[] positions0 = new Vector3F[] { new Vector3F(1, 2, 3) };
            Vector3F[] positions1 = new Vector3F[] { new Vector3F(4, 5, 6) };

            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (VertexBufferFactory factory0 = new VertexBufferFactory(thread0Window.Context, positions0))
            using (VertexBufferFactory factory1 = new VertexBufferFactory(thread1Window.Context, positions1))
            {
                Thread t0 = new Thread(factory0.Create);
                t0.Start();
                t0.Join();

                Thread t1 = new Thread(factory1.Create);
                t1.Start();
                t1.Join();

                Assert.AreEqual(positions0[0], factory0.VertexBuffer.CopyToSystemMemory<Vector3F>()[0]);
                Assert.AreEqual(positions1[0], factory1.VertexBuffer.CopyToSystemMemory<Vector3F>()[0]);
            }
        }

        /// <summary>
        /// Creates the rendering context, then creates two vertex buffers on two 
        /// different threads ran one in parallel.  The vertex buffers is then 
        /// verified in the rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffersParallel()
        {
            Vector3F[] positions0 = new Vector3F[] { new Vector3F(1, 2, 3) };
            Vector3F[] positions1 = new Vector3F[] { new Vector3F(4, 5, 6) };

            using (var thread0Window = Device.CreateWindow(1, 1))
            using (var thread1Window = Device.CreateWindow(1, 1))
            using (var window = Device.CreateWindow(1, 1))
            using (VertexBufferFactory factory0 = new VertexBufferFactory(thread0Window.Context, positions0))
            using (VertexBufferFactory factory1 = new VertexBufferFactory(thread1Window.Context, positions1))
            {
                Thread t0 = new Thread(factory0.Create);
                Thread t1 = new Thread(factory1.Create);

                t0.Start();
                t1.Start();

                t0.Join();
                t1.Join();

                Assert.AreEqual(positions0[0], factory0.VertexBuffer.CopyToSystemMemory<Vector3F>()[0]);
                Assert.AreEqual(positions1[0], factory1.VertexBuffer.CopyToSystemMemory<Vector3F>()[0]);
            }
        }
    }
}
