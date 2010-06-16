#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal abstract class DrawAutomaticUniformFactory
    {
        public abstract string Name { get; }
        public abstract DrawAutomaticUniform Create(Uniform uniform);
    }
}
