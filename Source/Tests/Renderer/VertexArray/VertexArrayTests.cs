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
            GraphicsWindow window = Device.CreateWindow(1, 1);

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
            uint[] indices = new uint[] { 0, 1, 2 };
            int ibSizeInBytes = indices.Length * sizeof(uint);
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StreamDraw, ibSizeInBytes);
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
            // Create and verify vertex array
            //
            VertexArray va = window.Context.CreateVertexArray();
            va.Attributes[0] = vertexBufferAttribute;
            va.IndexBuffer = indexBuffer;

            Assert.AreEqual(vertexBufferAttribute, va.Attributes[0]);
            Assert.AreEqual(indexBuffer, va.IndexBuffer);

            va.Attributes[0] = null;
            va.IndexBuffer = null;

            Assert.IsNull(va.Attributes[0]);
            Assert.IsNull(va.IndexBuffer);

            va.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            window.Dispose();
        }

        [Test]
        public void EnumeratevertexBufferAttributes()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);

            VertexArray va = window.Context.CreateVertexArray();
            VertexBuffer vb0 = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4);
            VertexBuffer vb1 = Device.CreateVertexBuffer(BufferHint.DynamicDraw, 4);
            VertexBuffer vb2 = Device.CreateVertexBuffer(BufferHint.StreamDraw, 4);

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

            vb2.Dispose();
            vb1.Dispose();
            vb0.Dispose();
            va.Dispose();
            window.Dispose();
        }
    }
}
