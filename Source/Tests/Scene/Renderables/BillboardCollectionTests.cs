#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    [TestFixture]
    public class BillboardCollectionTests
    {
        [Test]
        public void Construct()
        {
            BillboardCollectionTest billboardGroup = new BillboardCollectionTest();
            BillboardCollection group = billboardGroup.Group;

            Assert.IsTrue(group.DepthTestEnabled);
            Assert.IsFalse(group.Wireframe);

            billboardGroup.Dispose();
        }

        [Test]
        public void List()
        {
            BillboardCollectionTest billboardGroup = new BillboardCollectionTest();
            IList<Billboard> group = billboardGroup.Group;

            Assert.AreEqual(0, group.Count);
            Assert.IsFalse(group.IsReadOnly);

            Billboard b0 = new Billboard() { Position = new Vector3D(0, 1, 2) };
            Billboard b1 = new Billboard() { Position = new Vector3D(3, 4, 5) };
            Billboard[] billboards = new Billboard[] { b0, b1 };
            group.Add(b0);
            group.Add(b1);

            Assert.AreEqual(billboards.Length, group.Count);
            Assert.AreEqual(b0, group[0]);
            Assert.AreEqual(0, group.IndexOf(b0));
            Assert.AreEqual(b1, group[1]);
            Assert.AreEqual(1, group.IndexOf(b1));

            int i = 0;
            foreach (Billboard b in group)
            {
                Assert.AreEqual(billboards[i++], b);
                Assert.AreEqual(group, b.Group);
            }

            Assert.IsTrue(group.Contains(b0));
            group.Clear();
            Assert.AreEqual(0, group.Count);
            Assert.IsFalse(group.Contains(b0));

            billboardGroup.Dispose();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNull()
        {
            using (BillboardCollectionTest billboardGroup = new BillboardCollectionTest())
            {
                billboardGroup.Group.Add(null);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddDuplicate()
        {
            using (BillboardCollectionTest billboardGroup = new BillboardCollectionTest())
            {
                Billboard b = new Billboard() { Position = Vector3D.Zero };
                billboardGroup.Group.Add(b);
                billboardGroup.Group.Add(b);
            }
        }

        [Test]
        public void Remove()
        {
            using (BillboardCollectionTest billboardGroup = new BillboardCollectionTest())
            {
                BillboardCollection group = billboardGroup.Group;
                Billboard b = new Billboard() { Position = Vector3D.Zero };

                group.Add(b);
                Assert.AreEqual(group, b.Group);
                b.Position = Vector3D.UnitX;        // Make billboard dirty;
                Assert.IsTrue(group.Remove(b));
                Assert.IsNull(b.Group);
            }
        }

        private sealed class BillboardCollectionTest : IDisposable
        {
            public BillboardCollectionTest()
            {
                _window = Device.CreateWindow(1, 1);
                _group = new BillboardCollection(_window.Context);
            }

            public Context Context
            {
                get { return _window.Context; }
            }

            public BillboardCollection Group
            {
                get { return _group; }
            }

            #region IDisposable Members

            public void Dispose()
            {
                _group.Dispose();
                _window.Dispose();
            }

            #endregion

            private MiniGlobeWindow _window;
            private BillboardCollection _group;
        }
    }
}
