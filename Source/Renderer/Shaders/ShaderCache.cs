#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;

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

    public class ShaderCache
    {
        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            CachedShaderProgram cachedShaderProgram;
            if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
            {
                ++cachedShaderProgram.ReferenceCount;
                return cachedShaderProgram.ShaderProgram;
            }

            ShaderProgram shaderProgram = Device.CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
            _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));

            return shaderProgram;
        }

        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            CachedShaderProgram cachedShaderProgram;
            if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
            {
                ++cachedShaderProgram.ReferenceCount;
                return cachedShaderProgram.ShaderProgram;
            }

            ShaderProgram shaderProgram = Device.CreateShaderProgram(vertexShaderSource,
                geometryShaderSource, fragmentShaderSource);
            _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));

            return shaderProgram;
        }

        public void Release(string key)
        {
            if (--_shaderPrograms[key].ReferenceCount == 0)
            {
                _shaderPrograms.Remove(key);
            }
        }

        // TODO:  This class needs a Contains method so the client doesn't need to build the shader source string
        private Dictionary<string, CachedShaderProgram> _shaderPrograms = new Dictionary<string, CachedShaderProgram>();
    }
}
