#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;

namespace OpenGlobe.Renderer
{
    public sealed class ShaderCache : IDisposable
    {
        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            ShaderProgram shaderProgram;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
                else
                {
                    shaderProgram = Device.CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
                    _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));
                }
            }

            return shaderProgram;
        }

        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            ShaderProgram shaderProgram;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
                else
                {
                    shaderProgram = Device.CreateShaderProgram(vertexShaderSource,
                        geometryShaderSource, fragmentShaderSource);
                    _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));
                }
            }

            return shaderProgram;
        }

        public ShaderProgram Find(string key)
        {
            ShaderProgram shaderProgram = null;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
            }

            return shaderProgram;
        }

        public void Release(string key)
        {
            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram = _shaderPrograms[key];
                if (--cachedShaderProgram.ReferenceCount == 0)
                {
                    _shaderPrograms.Remove(key);
                    cachedShaderProgram.Dispose();
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_shaderPrograms)
            {
                foreach (KeyValuePair<string, CachedShaderProgram> pair in _shaderPrograms)
                {
                    pair.Value.Dispose();
                }
                _shaderPrograms.Clear();
            }
        }

        #endregion

        private Dictionary<string, CachedShaderProgram> _shaderPrograms = new Dictionary<string, CachedShaderProgram>();
    }
}
