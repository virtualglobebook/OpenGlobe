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
using OpenGlobe.Core.Geometry;

namespace OpenGlobe.Core.Tessellation
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
                (simpleSphere.Indices as IndicesUnsignedInt).Values, 
                (sphere.Indices as IndicesUnsignedInt).Values);
            GraphicsAssert.ListsAreEqual(
                (simpleSphere.Attributes["position"] as VertexAttributeDoubleVector3).Values,
                (sphere.Attributes["position"] as VertexAttributeDoubleVector3).Values);
        }

        [Test]
        public void SubdivisionEllipsoidTessellatorTest()
        {
            Mesh sphere = SubdivisionSphereTessellator.Compute(1, SubdivisionSphereVertexAttributes.All);
            Mesh ellipsoid = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 1, SubdivisionEllipsoidVertexAttributes.All);

            GraphicsAssert.ListsAreEqual(
                (sphere.Indices as IndicesUnsignedInt).Values,
                (ellipsoid.Indices as IndicesUnsignedInt).Values);
            GraphicsAssert.ListsAreEqual(
                (sphere.Attributes["position"] as VertexAttributeDoubleVector3).Values,
                (ellipsoid.Attributes["position"] as VertexAttributeDoubleVector3).Values);
            GraphicsAssert.ListsAreEqual(
                (sphere.Attributes["normal"] as VertexAttributeHalfFloatVector3).Values,
                (ellipsoid.Attributes["normal"] as VertexAttributeHalfFloatVector3).Values);
            GraphicsAssert.ListsAreEqual(
                (sphere.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2).Values,
                (ellipsoid.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2).Values);
        }

        [Test]
        public void CubeMapEllipsoidTessellatorTest()
        {
            Mesh mesh = CubeMapEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 3, CubeMapEllipsoidVertexAttributes.All);

            VertexAttributeDoubleVector3 positions = mesh.Attributes["position"] as VertexAttributeDoubleVector3;
            VertexAttributeHalfFloatVector3 normals = mesh.Attributes["normal"] as VertexAttributeHalfFloatVector3;
            VertexAttributeHalfFloatVector2 textureCoordinates = mesh.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2;

            Assert.AreEqual(positions.Values.Count, normals.Values.Count);
            Assert.AreEqual(positions.Values.Count, textureCoordinates.Values.Count);
        }

        [Test]
        public void GeographicGridEllipsoidTessellatorTest()
        {
            Mesh mesh = GeographicGridEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 8, 4, GeographicGridEllipsoidVertexAttributes.All);

            VertexAttributeDoubleVector3 positions = mesh.Attributes["position"] as VertexAttributeDoubleVector3;
            VertexAttributeHalfFloatVector3 normals = mesh.Attributes["normal"] as VertexAttributeHalfFloatVector3;
            VertexAttributeHalfFloatVector2 textureCoordinates = mesh.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2;

            Assert.AreEqual(positions.Values.Count, normals.Values.Count);
            Assert.AreEqual(positions.Values.Count, textureCoordinates.Values.Count);
        }

        [Test]
        public void BoxTessellatorTest()
        {
            Mesh mesh = BoxTessellator.Compute(new Vector3D(1, 1, 1));

            Assert.IsNotNull(mesh.Attributes["position"] as VertexAttributeDoubleVector3);
            Assert.IsNotNull(mesh.Indices as IndicesByte);
        }

        [Test]
        public void RectangleTessellatorTest()
        {
            Mesh mesh = RectangleTessellator.Compute(new RectangleD(Vector2D.Zero, new Vector2D(1, 1)), 1, 1);

            Assert.AreEqual(PrimitiveType.Triangles, mesh.PrimitiveType);
            Assert.AreEqual(WindingOrder.Counterclockwise, mesh.FrontFaceWindingOrder);

            IList<Vector2D> positions = (mesh.Attributes["position"] as VertexAttributeDoubleVector2).Values;
            Assert.AreEqual(4, positions.Count);
            Assert.AreEqual(Vector2D.Zero, positions[0]);
            Assert.AreEqual(Vector2D.UnitX, positions[1]);
            Assert.AreEqual(Vector2D.UnitY, positions[2]);
            Assert.AreEqual(new Vector2D(1, 1), positions[3]);

            IList<uint> indices = (mesh.Indices as IndicesUnsignedInt).Values;
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
