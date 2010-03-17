#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using MiniGlobe.Core;

namespace MiniGlobe.Scene
{
    [TestFixture]
    public class BillboardTests
    {
        [Test]
        public void Position()
        {
            Billboard b = new Billboard();
            b.Position = new Vector3D(0, 1, 2);
            Assert.AreEqual(new Vector3D(0, 1, 2), b.Position);
            Assert.IsNull(b.Group);
        }

        [Test]
        public void TextureCoordinates()
        {
            Billboard b = new Billboard();
            Assert.AreEqual(new RectangleH(Vector2H.Zero, new Vector2H(1.0, 1.0)), b.TextureCoordinates);

            b.TextureCoordinates = new RectangleH(new Vector2H(2.0, 3.0), new Vector2H(4.0, 5.0));
            Assert.AreEqual(new RectangleH(new Vector2H(2.0, 3.0), new Vector2H(4.0, 5.0)), b.TextureCoordinates);
        }

        [Test]
        public void Color()
        {
            Billboard b = new Billboard();
            Assert.AreEqual(System.Drawing.Color.White, b.Color);

            b.Color = System.Drawing.Color.Green;
            Assert.AreEqual(System.Drawing.Color.Green, b.Color);
        }

        [Test]
        public void Origin()
        {
            Billboard b = new Billboard();
            Assert.AreEqual(HorizontalOrigin.Center, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Center, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Left;
            Assert.AreEqual(HorizontalOrigin.Left, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Center, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Center;
            Assert.AreEqual(HorizontalOrigin.Center, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Center, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Right;
            Assert.AreEqual(HorizontalOrigin.Right, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Center, b.VerticalOrigin);

            b.VerticalOrigin = VerticalOrigin.Bottom;

            b.HorizontalOrigin = HorizontalOrigin.Left;
            Assert.AreEqual(HorizontalOrigin.Left, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Bottom, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Center;
            Assert.AreEqual(HorizontalOrigin.Center, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Bottom, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Right;
            Assert.AreEqual(HorizontalOrigin.Right, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Bottom, b.VerticalOrigin);

            b.VerticalOrigin = VerticalOrigin.Top;

            b.HorizontalOrigin = HorizontalOrigin.Left;
            Assert.AreEqual(HorizontalOrigin.Left, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Top, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Center;
            Assert.AreEqual(HorizontalOrigin.Center, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Top, b.VerticalOrigin);

            b.HorizontalOrigin = HorizontalOrigin.Right;
            Assert.AreEqual(HorizontalOrigin.Right, b.HorizontalOrigin);
            Assert.AreEqual(VerticalOrigin.Top, b.VerticalOrigin);
        }
    }
}
