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
    public enum DepthTestFunction
    {
        Never,
        Less,
        Equal,
        LessThanOrEqual,
        Greater,
        NotEqual,
        GreaterThanOrEqual,
        Always
    }

    public class DepthTest
    {
        public DepthTest()
        {
            Enabled = true;
            Function = DepthTestFunction.Less;
        }

        public bool Enabled { get; set; }
        public DepthTestFunction Function { get; set; }
    }
}
