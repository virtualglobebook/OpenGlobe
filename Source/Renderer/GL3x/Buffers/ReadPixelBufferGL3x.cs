#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;
using OpenTK.Graphics.OpenGL;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenGlobe.Renderer.GL3x
{
    internal class ReadPixelBufferGL3x : ReadPixelBuffer, IBufferName
    {
        public ReadPixelBufferGL3x(PixelBufferHint usageHint, int sizeInBytes)
        {
            _bufferObject = new PixelBufferGL3x(BufferTarget.PixelPackBuffer, ToBufferHint(usageHint), sizeInBytes);
            _usageHint = usageHint;
        }

        internal void Bind()
        {
            _bufferObject.Bind();
        }

        internal void UnBind()
        {
            _bufferObject.UnBind();
        }

        #region ReadPixelBuffer Members

        public override void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes)
        {
            _bufferObject.CopyFromSystemMemory(bufferInSystemMemory, destinationOffsetInBytes, lengthInBytes);
        }

        public override void CopyFromBitmap(Bitmap bitmap)
        {
            _bufferObject.CopyFromBitmap(bitmap);
        }

        public override T[] CopyToSystemMemory<T>(int offsetInBytes, int sizeInBytes)
        {
            return _bufferObject.CopyToSystemMemory<T>(offsetInBytes, sizeInBytes);
        }

        public override Bitmap CopyToBitmap(int width, int height, ImagingPixelFormat pixelFormat)
        {
            return _bufferObject.CopyToBitmap(width, height, pixelFormat);
        }

        public override int SizeInBytes
        {
            get { return _bufferObject.SizeInBytes; }
        }

        public override PixelBufferHint UsageHint
        {
            get { return _usageHint; }
        }

        #endregion

        #region IBufferName Members

        public int Name
        {
            get { return _bufferObject.Name; }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bufferObject.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        static BufferHint ToBufferHint(PixelBufferHint usageHint)
        {
            return _bufferHints[(int)usageHint];
        }

        private PixelBufferGL3x _bufferObject;
        private PixelBufferHint _usageHint;

        private static readonly BufferHint[] _bufferHints = new[]
        {
            BufferHint.StreamRead,
            BufferHint.StaticRead,
            BufferHint.DynamicRead
        };
    }
}
