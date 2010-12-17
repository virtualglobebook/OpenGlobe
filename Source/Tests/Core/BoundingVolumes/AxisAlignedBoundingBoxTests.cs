#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
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
    public class AxisAlignedBoundingBoxTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            new AxisAlignedBoundingBox(null);
        }

        [Test]
        public void Empty()
        {
            AxisAlignedBoundingBox box = new AxisAlignedBoundingBox(new List<Vector3D>());
            Assert.AreEqual(new Vector3D(double.MinValue, double.MinValue, double.MinValue), box.Minimum);
            Assert.AreEqual(new Vector3D(double.MaxValue, double.MaxValue, double.MaxValue), box.Maximum);
        }

        [Test]
        public void Simple()
        {
            Vector3D min = new Vector3D(-1, -1, -1);
            Vector3D max = new Vector3D(1, 1, 1);

            IList<Vector3D> positions = new List<Vector3D>();
            positions.Add(min);
            positions.Add(max);

            AxisAlignedBoundingBox box = new AxisAlignedBoundingBox(positions);
            Assert.AreEqual(min, box.Minimum);
            Assert.AreEqual(max, box.Maximum);
            Assert.AreEqual(Vector3D.Zero, box.Center);
        }
    }
}
