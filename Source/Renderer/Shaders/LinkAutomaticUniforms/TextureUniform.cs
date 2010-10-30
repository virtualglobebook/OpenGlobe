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
    internal class TextureUniform : LinkAutomaticUniform
    {
        public TextureUniform(int textureUnit)
	    {
            _textureUnit = textureUnit;
        }

        #region LinkAutomaticUniform Members

        public override string Name 
        { 
            get { return "og_texture" + _textureUnit; }
        }

        public override void Set(Uniform uniform)
        {
 	        ((Uniform<int>)uniform).Value = _textureUnit;
        }

        #endregion

        int _textureUnit;
    }
}
