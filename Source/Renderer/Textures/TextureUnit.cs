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
    public abstract class TextureUnit
    {
        public abstract Texture2D Texture { get; set; }
        public abstract TextureSampler TextureSampler { get; set; }
    }
}
