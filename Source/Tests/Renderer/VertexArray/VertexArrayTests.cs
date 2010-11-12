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
            Vector3S[] positions = new Vector3S[] 
            { 
                new Vector3S(0, 0, 0),
                new Vector3S(1, 0, 0),
                new Vector3S(0, 1, 0)
            };
            int vbSizeInBytes = ArraySizeInBytes.Size(positions);

            uint[] indices = new uint[] { 0, 1, 2 };
            int ibSizeInBytes = indices.Length * sizeof(uint);

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, vbSizeInBytes))
            using (IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StreamDraw, ibSizeInBytes))
            using (VertexArray va = window.Context.CreateVertexArray())
            {
                vertexBuffer.CopyFromSystemMemory(positions);
                indexBuffer.CopyFromSystemMemory(indices);

                //
                // Create and verify vertex buffer attribute
                //
                VertexBufferAttribute vertexBufferAttribute = new VertexBufferAttribute(
                    vertexBuffer, ComponentDatatype.Float, 3, false, 0, 0);
                Assert.AreEqual(vertexBuffer, vertexBufferAttribute.VertexBuffer);
                Assert.AreEqual(ComponentDatatype.Float, vertexBufferAttribute.ComponentDatatype);
                Assert.AreEqual(3, vertexBufferAttribute.NumberOfComponents);
                Assert.IsFalse(vertexBufferAttribute.Normalize);
                Assert.AreEqual(0, vertexBufferAttribute.OffsetInBytes);
                Assert.AreEqual(SizeInBytes<Vector3S>.Value, vertexBufferAttribute.StrideInBytes);

                //
                // Verify vertex array
                //
                va.Attributes[0] = vertexBufferAttribute;
                va.IndexBuffer = indexBuffer;

                Assert.AreEqual(vertexBufferAttribute, va.Attributes[0]);
                Assert.AreEqual(indexBuffer, va.IndexBuffer);

                va.Attributes[0] = null;
                va.IndexBuffer = null;

                Assert.IsNull(va.Attributes[0]);
                Assert.IsNull(va.IndexBuffer);
            }
        }

        [Test]
        public void EnumeratevertexBufferAttributes()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (VertexArray va = window.Context.CreateVertexArray())
            using (VertexBuffer vb0 = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4))
            using (VertexBuffer vb1 = Device.CreateVertexBuffer(BufferHint.DynamicDraw, 4))
            using (VertexBuffer vb2 = Device.CreateVertexBuffer(BufferHint.StreamDraw, 4))
            {

                va.Attributes[0] = new VertexBufferAttribute(vb0, ComponentDatatype.Float, 1);
                va.Attributes[1] = new VertexBufferAttribute(vb1, ComponentDatatype.Float, 1);
                va.Attributes[2] = new VertexBufferAttribute(vb2, ComponentDatatype.Float, 1);
                Assert.AreEqual(3, va.Attributes.Count);

                va.Attributes[1] = null;
                Assert.AreEqual(2, va.Attributes.Count);

                va.Attributes[1] = new VertexBufferAttribute(vb1, ComponentDatatype.Float, 1);
                Assert.AreEqual(3, va.Attributes.Count);

                int count = 0;
                foreach (VertexBufferAttribute vb in va.Attributes)
                {
                    Assert.IsNotNull(vb.VertexBuffer);
                    ++count;
                }
                Assert.AreEqual(va.Attributes.Count, count);
            }
        }
    }
}
