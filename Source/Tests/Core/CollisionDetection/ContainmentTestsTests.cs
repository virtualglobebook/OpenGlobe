#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class ContainmentTestsTests
    {
        [Test]
        public void Inside()
        {
            Assert.IsTrue(ContainmentTests.PointInsideTriangle(
                new Vector2D(0.25, 0.25),
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(0, 1)));
        }
    }
}
