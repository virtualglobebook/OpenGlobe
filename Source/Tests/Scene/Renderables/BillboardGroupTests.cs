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
using MiniGlobe.Core;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    [TestFixture]
    public class BillboardGroupTests
    {
        [Test]
        public void Billboard()
        {
            Billboard b = new Billboard();
            b.Position = new Vector3D(0, 1, 2);
            Assert.AreEqual(new Vector3D(0, 1, 2), b.Position);
            Assert.IsNull(b.Group);
        }

        [Test]
        public void Construct()
        {
            BillboardGroupTest billboardGroup = new BillboardGroupTest();
            BillboardGroup2 group = billboardGroup.Group;

            Assert.AreEqual(billboardGroup.Context, group.Context);
            Assert.IsTrue(group.DepthTestEnabled);
            Assert.IsFalse(group.Wireframe);

            billboardGroup.Dispose();
        }

        [Test]
        public void List()
        {
            BillboardGroupTest billboardGroup = new BillboardGroupTest();
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
            using (BillboardGroupTest billboardGroup = new BillboardGroupTest())
            {
                billboardGroup.Group.Add(null);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddDuplicate()
        {
            using (BillboardGroupTest billboardGroup = new BillboardGroupTest())
            {
                Billboard b = new Billboard() { Position = Vector3D.Zero };
                billboardGroup.Group.Add(b);
                billboardGroup.Group.Add(b);
            }
        }

        [Test]
        public void Remove()
        {
            using (BillboardGroupTest billboardGroup = new BillboardGroupTest())
            {
                BillboardGroup2 group = billboardGroup.Group;
                Billboard b = new Billboard() { Position = Vector3D.Zero };

                group.Add(b);
                Assert.AreEqual(group, b.Group);
                b.Position = Vector3D.UnitX;        // Make billboard dirty;
                Assert.IsTrue(group.Remove(b));
                Assert.IsNull(b.Group);
            }
        }

        class BillboardGroupTest : IDisposable
        {
            public BillboardGroupTest()
            {
                _window = Device.CreateWindow(1, 1);
                _bitmap = new Bitmap(1, 1);
                _group = new BillboardGroup2(_window.Context, _bitmap);
            }

            public Context Context
            {
                get { return _window.Context; }
            }

            public BillboardGroup2 Group
            {
                get { return _group; }
            }

            #region IDisposable Members

            public void Dispose()
            {
                _window.Dispose();
                _bitmap.Dispose();
                _group.Dispose();
            }

            #endregion

            private MiniGlobeWindow _window;
            private Bitmap _bitmap;
            private BillboardGroup2 _group;
        }
    }
}
