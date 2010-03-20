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

namespace MiniGlobe.Core.Geometry
{
    [TestFixture]
    public class GeometryTests
    {
        [Test]
        public void TriangleIndicesTest()
        {
            TriangleIndices<int> triangle = new TriangleIndices<int>(0, 1, 2);
            Assert.AreEqual(0, triangle.I0);
            Assert.AreEqual(1, triangle.I1);
            Assert.AreEqual(2, triangle.I2);

            TriangleIndices<int> triangle2 = triangle;
            Assert.AreEqual(triangle, triangle2);

            TriangleIndices<int> triangle3 = new TriangleIndices<int>(3, 4, 5);
            Assert.AreNotEqual(triangle, triangle3);
        }

        [Test]
        public void MeshVertexAttributes()
        {
            Mesh mesh = new Mesh();

            VertexAttributeHalfFloat halfFloatAttribute = new VertexAttributeHalfFloat("halfFloatAttribute");
            mesh.Attributes.Add(halfFloatAttribute);
            Assert.AreEqual("halfFloatAttribute", mesh.Attributes["halfFloatAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloat, mesh.Attributes["halfFloatAttribute"].DataType);

            VertexAttributeHalfFloatVector2 halfFloatAttribute2 = new VertexAttributeHalfFloatVector2("halfFloatAttribute2");
            mesh.Attributes.Add(halfFloatAttribute2);
            Assert.AreEqual("halfFloatAttribute2", mesh.Attributes["halfFloatAttribute2"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector2, mesh.Attributes["halfFloatAttribute2"].DataType);

            VertexAttributeHalfFloatVector3 halfFloatAttribute3 = new VertexAttributeHalfFloatVector3("halfFloatAttribute3");
            mesh.Attributes.Add(halfFloatAttribute3);
            Assert.AreEqual("halfFloatAttribute3", mesh.Attributes["halfFloatAttribute3"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector3, mesh.Attributes["halfFloatAttribute3"].DataType);

            VertexAttributeHalfFloatVector4 halfFloatAttribute4 = new VertexAttributeHalfFloatVector4("halfFloatAttribute4");
            mesh.Attributes.Add(halfFloatAttribute4);
            Assert.AreEqual("halfFloatAttribute4", mesh.Attributes["halfFloatAttribute4"].Name);
            Assert.AreEqual(VertexAttributeType.HalfFloatVector4, mesh.Attributes["halfFloatAttribute4"].DataType);

            ///////////////////////////////////////////////////////////////////

            VertexAttributeFloat floatAttribute = new VertexAttributeFloat("floatAttribute");
            mesh.Attributes.Add(floatAttribute);
            Assert.AreEqual("floatAttribute", mesh.Attributes["floatAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.Float, mesh.Attributes["floatAttribute"].DataType);

            VertexAttributeFloatVector2 floatAttribute2 = new VertexAttributeFloatVector2("floatAttribute2");
            mesh.Attributes.Add(floatAttribute2);
            Assert.AreEqual("floatAttribute2", mesh.Attributes["floatAttribute2"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector2, mesh.Attributes["floatAttribute2"].DataType);

            VertexAttributeFloatVector3 floatAttribute3 = new VertexAttributeFloatVector3("floatAttribute3");
            mesh.Attributes.Add(floatAttribute3);
            Assert.AreEqual("floatAttribute3", mesh.Attributes["floatAttribute3"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector3, mesh.Attributes["floatAttribute3"].DataType);

            VertexAttributeFloatVector4 floatAttribute4 = new VertexAttributeFloatVector4("floatAttribute4");
            mesh.Attributes.Add(floatAttribute4);
            Assert.AreEqual("floatAttribute4", mesh.Attributes["floatAttribute4"].Name);
            Assert.AreEqual(VertexAttributeType.FloatVector4, mesh.Attributes["floatAttribute4"].DataType);

            ///////////////////////////////////////////////////////////////////

            VertexAttributeDouble doubleAttribute = new VertexAttributeDouble("doubleAttribute");
            mesh.Attributes.Add(doubleAttribute);
            Assert.AreEqual("doubleAttribute", mesh.Attributes["doubleAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.Double, mesh.Attributes["doubleAttribute"].DataType);

            VertexAttributeDoubleVector2 doubleAttribute2 = new VertexAttributeDoubleVector2("doubleAttribute2");
            mesh.Attributes.Add(doubleAttribute2);
            Assert.AreEqual("doubleAttribute2", mesh.Attributes["doubleAttribute2"].Name);
            Assert.AreEqual(VertexAttributeType.DoubleVector2, mesh.Attributes["doubleAttribute2"].DataType);

            VertexAttributeDoubleVector3 doubleAttribute3 = new VertexAttributeDoubleVector3("doubleAttribute3");
            mesh.Attributes.Add(doubleAttribute3);
            Assert.AreEqual("doubleAttribute3", mesh.Attributes["doubleAttribute3"].Name);
            Assert.AreEqual(VertexAttributeType.DoubleVector3, mesh.Attributes["doubleAttribute3"].DataType);

            VertexAttributeDoubleVector4 doubleAttribute4 = new VertexAttributeDoubleVector4("doubleAttribute4");
            mesh.Attributes.Add(doubleAttribute4);
            Assert.AreEqual("doubleAttribute4", mesh.Attributes["doubleAttribute4"].Name);
            Assert.AreEqual(VertexAttributeType.DoubleVector4, mesh.Attributes["doubleAttribute4"].DataType);

            ///////////////////////////////////////////////////////////////////

            VertexAttributeByte byteAttribute = new VertexAttributeByte("byteAttribute");
            mesh.Attributes.Add(byteAttribute);
            Assert.AreEqual("byteAttribute", mesh.Attributes["byteAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.UnsignedByte, mesh.Attributes["byteAttribute"].DataType);

            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("colorAttribute");
            mesh.Attributes.Add(colorAttribute);
            Assert.AreEqual("colorAttribute", mesh.Attributes["colorAttribute"].Name);
            Assert.AreEqual(VertexAttributeType.UnsignedByte, mesh.Attributes["colorAttribute"].DataType);

            colorAttribute.AddColor(Color.FromArgb(3, 0, 1, 2));
            Assert.AreEqual(0, colorAttribute.Values[0]);
            Assert.AreEqual(1, colorAttribute.Values[1]);
            Assert.AreEqual(2, colorAttribute.Values[2]);
            Assert.AreEqual(3, colorAttribute.Values[3]);
        }

        [Test]
        public void MeshIndices()
        {
            Mesh mesh = new Mesh();

            IndicesByte indicesByte = new IndicesByte();
            mesh.Indices = indicesByte;
            Assert.AreEqual(IndicesType.Byte, mesh.Indices.DataType);

            IndicesShort indicesShort = new IndicesShort();
            mesh.Indices = indicesShort;
            Assert.AreEqual(IndicesType.Short, mesh.Indices.DataType);

            IndicesInt indicesInt = new IndicesInt();
            mesh.Indices = indicesInt;
            Assert.AreEqual(IndicesType.Int, mesh.Indices.DataType);

            indicesInt.AddTriangle(new TriangleIndices<int>(0, 1, 2));
            Assert.AreEqual(0, indicesInt.Values[0]);
            Assert.AreEqual(1, indicesInt.Values[1]);
            Assert.AreEqual(2, indicesInt.Values[2]);
        }
    }
}
