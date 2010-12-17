#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class EllipsoidTangentPlaneTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEllipsoid()
        {
            new EllipsoidTangentPlane(null, new List<Vector3D>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPositions()
        {
            new EllipsoidTangentPlane(Ellipsoid.UnitSphere, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EmptyPositions()
        {
            new EllipsoidTangentPlane(Ellipsoid.UnitSphere, new List<Vector3D>());
        }

        [Test]
        public void TangentPlane()
        {
            new EllipsoidTangentPlane(
                Ellipsoid.UnitSphere, new Vector3D[] { new Vector3D(1, 0, 0) });
        }

        [Test]
        public void TangentPlane2()
        {
            new EllipsoidTangentPlane(
                Ellipsoid.UnitSphere, new Vector3D[] { new Vector3D(0, 0, 1) });
        }
    }
}
