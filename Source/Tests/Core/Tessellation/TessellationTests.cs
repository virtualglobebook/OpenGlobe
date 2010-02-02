#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Core.Tessellation
{
    [TestFixture]
    public class TessellationTests
    {
        [Test]
        public void SubdivisionSphereTessellatorTest()
        {
            Mesh simpleSphere = SubdivisionSphereTessellatorSimple.Compute(1);
            Assert.AreEqual(IndicesType.Int, simpleSphere.Indices.DataType);
            Assert.AreEqual(1, simpleSphere.Attributes.Count);
            Assert.IsNotNull(simpleSphere.Attributes["position"] as VertexAttributeDoubleVector3);

            Mesh sphere = SubdivisionSphereTessellator.Compute(1, SubdivisionSphereVertexAttributes.All);
            Assert.AreEqual(IndicesType.Int, sphere.Indices.DataType);
            Assert.AreEqual(3, sphere.Attributes.Count);
            Assert.IsNotNull(sphere.Attributes["position"] as VertexAttributeDoubleVector3);
            Assert.IsNotNull(sphere.Attributes["normal"] as VertexAttributeHalfFloatVector3);
            Assert.IsNotNull(sphere.Attributes["textureCoordinate"] as VertexAttributeHalfFloatVector2);

            MiniGlobeAssert.ListsAreEqual(
                (simpleSphere.Indices as IndicesInt).Values, 
                (sphere.Indices as IndicesInt).Values);
            MiniGlobeAssert.ListsAreEqual(
                (simpleSphere.Attributes["position"] as VertexAttributeDoubleVector3).Values,
                (sphere.Attributes["position"] as VertexAttributeDoubleVector3).Values);
        }

        [Test]
        public void SubdivisionEllipsoidTessellatorTest()
        {
            Mesh sphere = SubdivisionSphereTessellator.Compute(1, SubdivisionSphereVertexAttributes.All);
            Mesh ellipsoid = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 1, SubdivisionEllipsoidVertexAttributes.All);

            MiniGlobeAssert.ListsAreEqual(
                (sphere.Indices as IndicesInt).Values,
                (ellipsoid.Indices as IndicesInt).Values);
            MiniGlobeAssert.ListsAreEqual(
                (sphere.Attributes["position"] as VertexAttributeDoubleVector3).Values,
                (ellipsoid.Attributes["position"] as VertexAttributeDoubleVector3).Values);
            MiniGlobeAssert.ListsAreEqual(
                (sphere.Attributes["normal"] as VertexAttributeHalfFloatVector3).Values,
                (ellipsoid.Attributes["normal"] as VertexAttributeHalfFloatVector3).Values);
            MiniGlobeAssert.ListsAreEqual(
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
    }
}
