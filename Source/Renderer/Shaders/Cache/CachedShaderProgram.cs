#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class CachedShaderProgram
    {
        public CachedShaderProgram(ShaderProgram shaderProgram)
        {
            _shaderProgram = shaderProgram;
            _referenceCount = 1;
        }

        public ShaderProgram ShaderProgram
        {
            get { return _shaderProgram; }
        }

        public int ReferenceCount
        {
            get { return _referenceCount; }
            set { _referenceCount = value; }
        }

        ShaderProgram _shaderProgram;
        int _referenceCount;
    }
}
