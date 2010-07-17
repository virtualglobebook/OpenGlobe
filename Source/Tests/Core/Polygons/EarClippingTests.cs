#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class EarClippingTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            EarClipping.Triangulate(null);
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Empty()
        {
            EarClipping.Triangulate(new List<Vector2D>());
        }
    }
}
