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
    internal class TextureLinkAutomaticUniform : LinkAutomaticUniform
    {
        public TextureLinkAutomaticUniform(int textureUnit)
	    {
            _textureUnit = textureUnit;
        }

        #region LinkAutomaticUniform Members

        public override string Name 
        { 
            get { return "mg_Texture" + _textureUnit; }
        }

        public override void Set(Uniform uniform)
        {
 	        (uniform as Uniform<int>).Value = _textureUnit;
        }

        #endregion

        int _textureUnit;
    }
}
