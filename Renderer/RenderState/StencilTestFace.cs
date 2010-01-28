#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace MiniGlobe.Renderer
{
    public enum StencilOperation
    {
        Zero,
        Invert,
        Keep,
        Replace,
        Increment,
        Decrement,
        IncrementWrap,
        DecrementWrap
    }

    public enum StencilTestFunction
    {
        Never,
        Less,
        Equal,
        LessThanOrEqual,
        Greater,
        NotEqual,
        GreaterThanOrEqual,
        Always,
    }

    public class StencilTestFace
    {
        public StencilTestFace()
        {
            StencilFailOperation = StencilOperation.Keep;
            DepthPassStencilFailOperation = StencilOperation.Keep;
            DepthPassStencilPassOperation = StencilOperation.Keep;
            Function = StencilTestFunction.Always;
            ReferenceValue = 0;
            Mask = 0xFF;            // TODO: Assumes 8-bit stencil
        }

        public StencilOperation StencilFailOperation { get; set; }
        public StencilOperation DepthPassStencilFailOperation { get; set; }
        public StencilOperation DepthPassStencilPassOperation { get; set; }
        
        public StencilTestFunction Function { get; set; }
        public int ReferenceValue { get; set; }
        public int Mask { get; set; }
    }
}
