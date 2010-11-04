#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class TessellationTests
    {
        [Test]
        public void SubdivisionSphereTessellatorTest()
        {
            Mesh simpleSphere = SubdivisionSphereTessellatorSimple.Compute(1);
            Assert.AreEqual(IndicesType.UnsignedInt, simpleSphere.Indices.Datatype);
            Assert.AreEqual(1, simpleSphere.Attributes.Count);
            Assert.IsNotNull(simpleSphere.Attributes["position"] as VertexAttributeDoubleVector3);

            Mesh sphere = SubdivisionSphereTessellator.Compute(1, SubdivisionSphereVertexAttributes.All);
            Assert.AreEqual(IndicesType.UnsignedInt, sphere.Indices.Datatype);
            Assert.AreEqual(3, sphere.Attributes.Count);
            Assert.IsNotNull(sphere.Attributes["position"] as VertexAttributeDoubleVector3);
            Assert.IsNotNull(sphere.Attributes["normal"] as VertexAttributeHalfFloatVector3);
            Assert.IsNotNull(sphere.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2);

            GraphicsAssert.ListsAreEqual(
                ((IndicesUnsignedInt)simpleSphere.Indices).Values, 
                ((IndicesUnsignedInt)sphere.Indices).Values);
            GraphicsAssert.ListsAreEqual(
                ((VertexAttributeDoubleVector3)simpleSphere.Attributes["position"]).Values,
                ((VertexAttributeDoubleVector3)sphere.Attributes["position"]).Values);
        }

        [Test]
        public void SubdivisionEllipsoidTessellatorTest()
        {
            Mesh sphere = SubdivisionSphereTessellator.Compute(1, SubdivisionSphereVertexAttributes.All);
            Mesh ellipsoid = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 1, SubdivisionEllipsoidVertexAttributes.All);

            GraphicsAssert.ListsAreEqual(
                ((IndicesUnsignedInt)sphere.Indices).Values,
                ((IndicesUnsignedInt)ellipsoid.Indices).Values);
            GraphicsAssert.ListsAreEqual(
                ((VertexAttributeDoubleVector3)sphere.Attributes["position"]).Values,
                ((VertexAttributeDoubleVector3)ellipsoid.Attributes["position"]).Values);
            GraphicsAssert.ListsAreEqual(
                ((VertexAttributeHalfFloatVector3)sphere.Attributes["normal"]).Values,
                ((VertexAttributeHalfFloatVector3)ellipsoid.Attributes["normal"]).Values);
            GraphicsAssert.ListsAreEqual(
                ((VertexAttributeHalfFloatVector2)sphere.Attributes["textureCoordinate"]).Values,
                ((VertexAttributeHalfFloatVector2)ellipsoid.Attributes["textureCoordinate"]).Values);
        }

        [Test]
        public void CubeMapEllipsoidTessellatorTest()
        {
            Mesh mesh = CubeMapEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 3, CubeMapEllipsoidVertexAttributes.All);

            VertexAttributeDoubleVector3 positions = (VertexAttributeDoubleVector3)mesh.Attributes["position"];
            VertexAttributeHalfFloatVector3 normals = (VertexAttributeHalfFloatVector3)mesh.Attributes["normal"];
            VertexAttributeHalfFloatVector2 textureCoordinates = (VertexAttributeHalfFloatVector2)mesh.Attributes["textureCoordinate"];

            Assert.AreEqual(positions.Values.Count, normals.Values.Count);
            Assert.AreEqual(positions.Values.Count, textureCoordinates.Values.Count);
        }

        [Test]
        public void GeographicGridEllipsoidTessellatorTest()
        {
            Mesh mesh = GeographicGridEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 8, 4, GeographicGridEllipsoidVertexAttributes.All);

            VertexAttributeDoubleVector3 positions = (VertexAttributeDoubleVector3)mesh.Attributes["position"];
            VertexAttributeHalfFloatVector3 normals = (VertexAttributeHalfFloatVector3)mesh.Attributes["normal"];
            VertexAttributeHalfFloatVector2 textureCoordinates = (VertexAttributeHalfFloatVector2)mesh.Attributes["textureCoordinate"];

            Assert.AreEqual(positions.Values.Count, normals.Values.Count);
            Assert.AreEqual(positions.Values.Count, textureCoordinates.Values.Count);
        }

        [Test]
        public void BoxTessellatorTest()
        {
            Mesh mesh = BoxTessellator.Compute(new Vector3D(1, 1, 1));

            Assert.IsNotNull(mesh.Attributes["position"] as VertexAttributeDoubleVector3);
            Assert.IsNotNull(mesh.Indices as IndicesUnsignedShort);
        }

        [Test]
        public void RectangleTessellatorTest()
        {
            Mesh mesh = RectangleTessellator.Compute(new RectangleD(Vector2D.Zero, new Vector2D(1, 1)), 1, 1);

            Assert.AreEqual(PrimitiveType.Triangles, mesh.PrimitiveType);
            Assert.AreEqual(WindingOrder.Counterclockwise, mesh.FrontFaceWindingOrder);

            IList<Vector2D> positions = ((VertexAttributeDoubleVector2)mesh.Attributes["position"]).Values;
            Assert.AreEqual(4, positions.Count);
            Assert.AreEqual(Vector2D.Zero, positions[0]);
            Assert.AreEqual(Vector2D.UnitX, positions[1]);
            Assert.AreEqual(Vector2D.UnitY, positions[2]);
            Assert.AreEqual(new Vector2D(1, 1), positions[3]);

            IList<uint> indices = ((IndicesUnsignedInt)mesh.Indices).Values;
            Assert.AreEqual(6, indices.Count);
            Assert.AreEqual(0, indices[0]);
            Assert.AreEqual(1, indices[1]);
            Assert.AreEqual(3, indices[2]);
            Assert.AreEqual(0, indices[3]);
            Assert.AreEqual(3, indices[4]);
            Assert.AreEqual(2, indices[5]);
        }
    }
}
