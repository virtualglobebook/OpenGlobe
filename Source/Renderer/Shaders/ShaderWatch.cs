#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.IO;

namespace OpenGlobe.Renderer
{
    internal sealed class ShaderWatch : IDisposable
    {
        public delegate void ShaderChangedEventHandler();

        public ShaderWatch(
            string vertexShaderFilePath,
            string geometryShaderFilePath,
            string fragmentShaderFilePath,
            ShaderChangedEventHandler eventHandler)
        {
            _vsWatch = CreateWatch(vertexShaderFilePath);
            _gsWatch = CreateWatch(geometryShaderFilePath);
            _fsWatch = CreateWatch(fragmentShaderFilePath);

            _vsFilePath = vertexShaderFilePath;
            _gsFilePath = geometryShaderFilePath;
            _fsFilePath = fragmentShaderFilePath;
            _eventHandler = eventHandler;
        }

        private FileSystemWatcher CreateWatch(string filePath)
        {
            if (filePath != string.Empty)
            {
                FileSystemWatcher watch = new FileSystemWatcher();
                watch.Path = Path.GetDirectoryName(filePath);
                watch.Filter = Path.GetFileName(filePath);
                watch.NotifyFilter = NotifyFilters.LastWrite;
                watch.Changed += new FileSystemEventHandler(ShaderChanged);
                watch.EnableRaisingEvents = true;
                return watch;
            }

            return null;
        }

        private void ShaderChanged(object sender, FileSystemEventArgs e)
        {
            _eventHandler();
        }

        public string VertexShaderFilePath
        {
            get { return _vsFilePath; }
        }

        public string GeometryShaderFilePath
        {
            get { return _gsFilePath; }
        }

        public string FragmentShaderFilePath
        {
            get { return _fsFilePath; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_vsWatch != null)
            {
                _vsWatch.Dispose();
            }

            if (_gsWatch != null)
            {
                _gsWatch.Dispose();
            }

            if (_fsWatch != null)
            {
                _fsWatch.Dispose();
            }
        }

        #endregion

        private readonly FileSystemWatcher _vsWatch;
        private readonly FileSystemWatcher _gsWatch;
        private readonly FileSystemWatcher _fsWatch;
        private readonly string _vsFilePath;
        private readonly string _gsFilePath;
        private readonly string _fsFilePath;
        private readonly ShaderChangedEventHandler _eventHandler;
    }
}
