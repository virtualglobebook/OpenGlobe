#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using NUnit.Framework;

namespace OpenGlobe.Scene
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
