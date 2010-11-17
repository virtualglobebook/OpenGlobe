#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class GeometryTests
    {
        [Test]
        public void TriangleIndicesTest()
        {
            TriangleIndicesUnsignedInt triangle = new TriangleIndicesUnsignedInt(0, 1, 2);
            Assert.AreEqual(0, triangle.UI0);
            Assert.AreEqual(1, triangle.UI1);
            Assert.AreEqual(2, triangle.UI2);

            TriangleIndicesUnsignedInt triangle2 = triangle;
            Assert.AreEqual(triangle, triangle2);

            TriangleIndicesUnsignedInt triangle3 = new TriangleIndicesUnsignedInt(3, 4, 5);
            Assert.AreNotEqual(triangle, triangle3);
        }

        [Test]
        public void MeshVertexAttributes()
        {
            Mesh mesh = new Mesh();

            VertexAttributeHalfFloat halfFloatAttribute = new VertexAttributeHalfFloat("halfFloatAttribute");
            mesh.Attributes.Add(halfFloatAttribute);
            Assert.AreEqual("halfFloatAttribute", mesh.Attributes["halfFloatAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloat, mesh.Attributes["halfFloatAttribute"].Datatype);

            VertexAttributeHalfFloatVector2 halfFloatAttribute2 = new VertexAttributeHalfFloatVector2("halfFloatAttribute2");
            mesh.Attributes.Add(halfFloatAttribute2);
            Assert.AreEqual("halfFloatAttribute2", mesh.Attributes["halfFloatAttribute2"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector2, mesh.Attributes["halfFloatAttribute2"].Datatype);

            VertexAttributeHalfFloatVector3 halfFloatAttribute3 = new VertexAttributeHalfFloatVector3("halfFloatAttribute3");
            mesh.Attributes.Add(halfFloatAttribute3);
            Assert.AreEqual("halfFloatAttribute3", mesh.Attributes["halfFloatAttribute3"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector3, mesh.Attributes["halfFloatAttribute3"].Datatype);

            VertexAttributeHalfFloatVector4 halfFloatAttribute4 = new VertexAttributeHalfFloatVector4("halfFloatAttribute4");
            mesh.Attributes.Add(halfFloatAttribute4);
            Assert.AreEqual("halfFloatAttribute4", mesh.Attributes["halfFloatAttribute4"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector4, mesh.Attributes["halfFloatAttribute4"].Datatype);

            ///////////////////////////////////////////////////////////////////

            VertexAttributeFloat floatAttribute = new VertexAttributeFloat("floatAttribute");
            mesh.Attributes.Add(floatAttribute);
            Assert.AreEqual("floatAttribute", mesh.Attributes["floatAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.Float, mesh.Attributes["floatAttribute"].Datatype);

            VertexAttributeFloatVector2 floatAttribute2 = new VertexAttributeFloatVector2("floatAttribute2");
            mesh.Attributes.Add(floatAttribute2);
            Assert.AreEqual("floatAttribute2", mesh.Attributes["floatAttribute2"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector2, mesh.Attributes["floatAttribute2"].Datatype);

            VertexAttributeFloatVector3 floatAttribute3 = new VertexAttributeFloatVector3("floatAttribute3");
            mesh.Attributes.Add(floatAttribute3);
            Assert.AreEqual("floatAttribute3", mesh.Attributes["floatAttribute3"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector3, mesh.Attributes["floatAttribute3"].Datatype);

            VertexAttributeFloatVector4 floatAttribute4 = new VertexAttributeFloatVector4("floatAttribute4");
            mesh.Attributes.Add(floatAttribute4);
            Assert.AreEqual("floatAttribute4", mesh.Attributes["floatAttribute4"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector4, mesh.Attributes["floatAttribute4"].Datatype);

            ///////////////////////////////////////////////////////////////////

            VertexAttributeByte byteAttribute = new VertexAttributeByte("byteAttribute");
            mesh.Attributes.Add(byteAttribute);
            Assert.AreEqual("byteAttribute", mesh.Attributes["byteAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.UnsignedByte, mesh.Attributes["byteAttribute"].Datatype);

            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("colorAttribute");
            mesh.Attributes.Add(colorAttribute);
            Assert.AreEqual("colorAttribute", mesh.Attributes["colorAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.UnsignedByte, mesh.Attributes["colorAttribute"].Datatype);

            colorAttribute.AddColor(Color.FromArgb(3, 0, 1, 2));
            Assert.AreEqual(0, colorAttribute.Values[0]);
            Assert.AreEqual(1, colorAttribute.Values[1]);
            Assert.AreEqual(2, colorAttribute.Values[2]);
            Assert.AreEqual(3, colorAttribute.Values[3]);
        }

        [Test]
        public void VertexAttributeEmulatedDoubleVector3()
        {
            Mesh mesh = new Mesh();

            VertexAttributeDoubleVector3 attribute = new VertexAttributeDoubleVector3("position");
            mesh.Attributes.Add(attribute);
            Assert.AreEqual("position", mesh.Attributes["position"].Name);
            Assert.AreEqual(VertexAttributeType.EmulatedDoubleVector3, mesh.Attributes["position"].Datatype);
        }

        [Test]
        public void MeshIndices()
        {
            Mesh mesh = new Mesh();

            IndicesUnsignedShort indicesShort = new IndicesUnsignedShort();
            mesh.Indices = indicesShort;
            Assert.AreEqual(IndicesType.UnsignedShort, mesh.Indices.Datatype);

            IndicesUnsignedInt indicesInt = new IndicesUnsignedInt();
            mesh.Indices = indicesInt;
            Assert.AreEqual(IndicesType.UnsignedInt, mesh.Indices.Datatype);

            indicesInt.AddTriangle(new TriangleIndicesUnsignedInt(0, 1, 2));
            Assert.AreEqual(0, indicesInt.Values[0]);
            Assert.AreEqual(1, indicesInt.Values[1]);
            Assert.AreEqual(2, indicesInt.Values[2]);
        }
    }
}
