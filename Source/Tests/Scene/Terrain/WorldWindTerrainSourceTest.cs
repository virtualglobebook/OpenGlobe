#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenGlobe.Scene.Terrain;
using System.Net;
using System.Diagnostics;

namespace OpenGlobe.Tests.Scene.Terrain
{
    [TestFixture]
    public class WorldWindTerrainSourceTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            WorldWindTerrainSource source = new WorldWindTerrainSource(new Uri("http://www.nasa.network.com/elev?service=WMS&request=GetMap&version=1.3&srs=EPSG:4326&layers=mergedElevations&styles=&format=application/bil16&bgColor=-9999.0&width=150&height=150"));
            Assert.IsNotNull(source);
        }
    }
}
