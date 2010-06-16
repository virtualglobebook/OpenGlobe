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

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class ContextTests
    {
        [Test]
        public void Clear()
        {
            GraphicsWindow window = Device.CreateWindow(1, 1);

            window.Context.Clear(new ClearState { Buffers = ClearBuffers.ColorBuffer });
            window.Context.Clear(new ClearState { Buffers = ClearBuffers.DepthBuffer });
            window.Context.Clear(new ClearState { Buffers = ClearBuffers.StencilBuffer });

            //
            // The following is much faster
            //
            window.Context.Clear(new ClearState { Buffers = ClearBuffers.All });

            window.Dispose();
        }
    }
}
