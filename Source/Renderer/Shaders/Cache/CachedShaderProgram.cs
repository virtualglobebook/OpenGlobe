#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;

namespace OpenGlobe.Renderer
{
    internal sealed class CachedShaderProgram : IDisposable
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

        #region IDisposable Members

        public void Dispose()
        {
            _shaderProgram.Dispose();
            _shaderProgram = null;
            _referenceCount = 0;
        }

        #endregion

        ShaderProgram _shaderProgram;
        int _referenceCount;
    }
}
