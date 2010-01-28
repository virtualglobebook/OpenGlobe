#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using System.Drawing;

namespace MiniGlobe.Renderer
{
    [TestFixture]
    public class ContextTests
    {
        [Test]
        public void Clear()
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);

            window.Context.Clear(ClearBuffers.ColorBuffer, Color.Red, 1, 0);
            window.Context.Clear(ClearBuffers.DepthBuffer, Color.Red, 1, 0);
            window.Context.Clear(ClearBuffers.StencilBuffer, Color.Red, 1, 0);

            //
            // The following is much faster
            //
            window.Context.Clear(ClearBuffers.All, Color.Red, 1, 0);

            window.Dispose();
        }
    }
}
