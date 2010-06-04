#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;

namespace MiniGlobe.Renderer
{
    public class ClearState
    {
        public ClearState()
        {
            RenderState = new RenderState();
            Buffers = ClearBuffers.All;
            Color = Color.White;
            Depth = 1;
            Stencil = 0;
        }

        public FrameBuffer FrameBuffer { get; set; }
        public RenderState RenderState { get; set; }
        public ClearBuffers Buffers { get; set; }
        public Color Color { get; set; }
        public float Depth { get; set; }
        public int Stencil { get; set; }
    }
}
