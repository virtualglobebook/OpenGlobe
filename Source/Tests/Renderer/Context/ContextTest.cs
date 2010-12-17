#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                window.Context.Clear(new ClearState { Buffers = ClearBuffers.ColorBuffer });
                window.Context.Clear(new ClearState { Buffers = ClearBuffers.DepthBuffer });
                window.Context.Clear(new ClearState { Buffers = ClearBuffers.StencilBuffer });
                window.Context.Clear(new ClearState { Buffers = ClearBuffers.All });
            }
        }
    }
}
