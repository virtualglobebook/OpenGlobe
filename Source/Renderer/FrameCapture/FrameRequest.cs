#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;

namespace OpenGlobe.Renderer
{
    internal class FrameRequest : IDisposable
    {
        public FrameRequest(string filename, Bitmap bitmap)
        {
            _filename = filename;
            _bitmap = bitmap;
        }

        public string Filename { get { return _filename; } }
        public Bitmap Bitmap { get { return _bitmap; } }

        #region IDisposable Members

        public void Dispose()
        {
            _bitmap.Dispose();
        }

        #endregion

        private readonly string _filename;
        private readonly Bitmap _bitmap;
    }
}
