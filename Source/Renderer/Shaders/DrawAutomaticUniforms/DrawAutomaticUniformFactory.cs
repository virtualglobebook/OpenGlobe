#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public abstract class DrawAutomaticUniformFactory
    {
        public abstract string Name { get; }
        public abstract UniformType Datatype { get; }
        public abstract DrawAutomaticUniform Create(Uniform uniform);
    }
}
