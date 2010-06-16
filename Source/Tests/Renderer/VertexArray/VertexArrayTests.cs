#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class VertexArrayTests
    {
        [Test]
        public void VertexArray()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            //
            // Create vertex buffer
            //
            Vector3S[] positions = new Vector3S[] 
            { 
                new Vector3S(0, 0, 0),
                new Vector3S(1, 0, 0),
                new Vector3S(0, 1, 0)
            };
            int vbSizeInBytes = positions.Length * SizeInBytes<Vector3S>.Value;
            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, vbSizeInBytes);
            vertexBuffer.CopyFromSystemMemory(positions);

            //
            // Create index buffer
            //
            uint[] indicies = new uint[] { 0, 1, 2 };
            int ibSizeInBytes = indicies.Length * sizeof(uint);
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StreamDraw, ibSizeInBytes);
            indexBuffer.CopyFromSystemMemory(indicies);

            //
            // Create and verify attached vertex buffer
            //
            AttachedVertexBuffer attachedVertexBuffer = new AttachedVertexBuffer(
                vertexBuffer, VertexAttributeComponentType.Float, 3, false);
            Assert.AreEqual(vertexBuffer, attachedVertexBuffer.VertexBuffer);
            Assert.AreEqual(VertexAttributeComponentType.Float, attachedVertexBuffer.ComponentType);
            Assert.AreEqual(3, attachedVertexBuffer.NumberOfComponents);
            Assert.IsFalse(attachedVertexBuffer.Normalize);

            //
            // Create and verify vertex array
            //
            VertexArray va = window.Context.CreateVertexArray();
            va.VertexBuffers[0] = attachedVertexBuffer;
            va.IndexBuffer = indexBuffer;

            Assert.AreEqual(attachedVertexBuffer, va.VertexBuffers[0]);
            Assert.AreEqual(indexBuffer, va.IndexBuffer);

            va.VertexBuffers[0] = null;
            va.IndexBuffer = null;

            Assert.IsNull(va.VertexBuffers[0]);
            Assert.IsNull(va.IndexBuffer);

            va.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void EnumerateAttachedVertexBuffers()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            VertexArray va = window.Context.CreateVertexArray();
            VertexBuffer vb0 = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4);
            VertexBuffer vb1 = Device.CreateVertexBuffer(BufferHint.DynamicDraw, 4);
            VertexBuffer vb2 = Device.CreateVertexBuffer(BufferHint.StreamDraw, 4);

            va.VertexBuffers[0] = new AttachedVertexBuffer(vb0, VertexAttributeComponentType.Float, 1);
            va.VertexBuffers[1] = new AttachedVertexBuffer(vb1, VertexAttributeComponentType.Float, 1);
            va.VertexBuffers[2] = new AttachedVertexBuffer(vb2, VertexAttributeComponentType.Float, 1);
            Assert.AreEqual(3, va.VertexBuffers.Count);

            va.VertexBuffers[1] = null;
            Assert.AreEqual(2, va.VertexBuffers.Count);

            va.VertexBuffers[1] = new AttachedVertexBuffer(vb1, VertexAttributeComponentType.Float, 1);
            Assert.AreEqual(3, va.VertexBuffers.Count);

            int count = 0;
            foreach (AttachedVertexBuffer vb in va.VertexBuffers)
            {
                Assert.IsNotNull(vb.VertexBuffer);
                ++count;
            }
            Assert.AreEqual(va.VertexBuffers.Count, count);

            vb2.Dispose();
            vb1.Dispose();
            vb0.Dispose();
            va.Dispose();
            window.Dispose();
        }
    }
}
