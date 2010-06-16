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
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.MultiThreading
{
    [TestFixture]
    public class VertexBufferMultiThreadingTests
    {
        /// <summary>
        /// Creates the rendering context, then creates a vertex buffer on a
        /// different thread.  The vertex buffers is then verified in the 
        /// rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffer()
        {
            var threadWindow = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);

            Vector3S[] positions = new Vector3S[] { new Vector3S(1, 2, 3) };
            ///////////////////////////////////////////////////////////////////

            VertexBufferFactory factory = new VertexBufferFactory(threadWindow, positions);

            Thread t = new Thread(factory.Create);
            t.Start();
            t.Join();
            ///////////////////////////////////////////////////////////////////

            Assert.AreEqual(positions[0], factory.VertexBuffer.CopyToSystemMemory<Vector3S>()[0]);

            factory.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates two vertex buffers on two 
        /// different threads ran one after the other.  The vertex buffers is then 
        /// verified in the rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffersSequential()
        {
            var thread0Window = Device.CreateWindow(1, 1);
            var thread1Window = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);

            Vector3S[] positions0 = new Vector3S[] { new Vector3S(1, 2, 3) };
            Vector3S[] positions1 = new Vector3S[] { new Vector3S(4, 5, 6) };
            ///////////////////////////////////////////////////////////////////

            VertexBufferFactory factory0 = new VertexBufferFactory(thread0Window, positions0);
            VertexBufferFactory factory1 = new VertexBufferFactory(thread1Window, positions1);

            Thread t0 = new Thread(factory0.Create);
            t0.Start();
            t0.Join();

            Thread t1 = new Thread(factory1.Create);
            t1.Start();
            t1.Join();

            ///////////////////////////////////////////////////////////////////

            Assert.AreEqual(positions0[0], factory0.VertexBuffer.CopyToSystemMemory<Vector3S>()[0]);
            Assert.AreEqual(positions1[0], factory1.VertexBuffer.CopyToSystemMemory<Vector3S>()[0]);

            factory1.Dispose();
            factory0.Dispose();
            window.Dispose();
        }

        /// <summary>
        /// Creates the rendering context, then creates two vertex buffers on two 
        /// different threads ran one in parallel.  The vertex buffers is then 
        /// verified in the rendering context.
        /// </summary>
        [Test]
        public void CreateVertexBuffersParallel()
        {
            var thread0Window = Device.CreateWindow(1, 1);
            var thread1Window = Device.CreateWindow(1, 1);
            var window = Device.CreateWindow(1, 1);

            Vector3S[] positions0 = new Vector3S[] { new Vector3S(1, 2, 3) };
            Vector3S[] positions1 = new Vector3S[] { new Vector3S(4, 5, 6) };
            ///////////////////////////////////////////////////////////////////

            VertexBufferFactory factory0 = new VertexBufferFactory(thread0Window, positions0);
            VertexBufferFactory factory1 = new VertexBufferFactory(thread1Window, positions1);

            Thread t0 = new Thread(factory0.Create);
            Thread t1 = new Thread(factory1.Create);

            t0.Start();
            t1.Start();

            t0.Join();
            t1.Join();

            ///////////////////////////////////////////////////////////////////

            Assert.AreEqual(positions0[0], factory0.VertexBuffer.CopyToSystemMemory<Vector3S>()[0]);
            Assert.AreEqual(positions1[0], factory1.VertexBuffer.CopyToSystemMemory<Vector3S>()[0]);

            factory1.Dispose();
            factory0.Dispose();
            window.Dispose();
        }
    }
}
