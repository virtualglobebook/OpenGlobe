#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;

namespace MiniGlobe.Renderer
{
    public enum ProgramPointSize
    {
        Enabled,
        Disabled
    }

    public enum RasterizationMode
    {
        Point,
        Line,
        Fill
    }

    public class RenderState
    {
        public RenderState()
        {
            PrimitiveRestart = new PrimitiveRestart();
            FacetCulling = new FacetCulling();
            ProgramPointSize = ProgramPointSize.Disabled;
            RasterizationMode = RasterizationMode.Fill;
            ScissorTest = new ScissorTest();
            StencilTest = new StencilTest();
            DepthTest = new DepthTest();
            Blending = new Blending();
        }

        public PrimitiveRestart PrimitiveRestart { get; set; }
        public FacetCulling FacetCulling { get; set; }
        public ProgramPointSize ProgramPointSize { get; set; }
        public RasterizationMode RasterizationMode { get; set; }
        public ScissorTest ScissorTest { get; set; }
        public StencilTest StencilTest { get; set; }
        public DepthTest DepthTest { get; set; }
        public Blending Blending { get; set; }
    }
}
