#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using OpenTK;
using OpenTK.Graphics;

namespace MiniGlobe.Renderer.GL3x
{
    internal static class FinalizerThreadContextGL3x
    {
        public static void MakeCurrent()
        {
            if (_window == null)
            {
                _window = new NativeWindow();
                _context = new GraphicsContext(new GraphicsMode(32, 24, 8), _window.WindowInfo, 3, 2, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug);
                _context.MakeCurrent(_window.WindowInfo);
            }
        }

        private static INativeWindow _window;
        private static IGraphicsContext _context;
    }
}
