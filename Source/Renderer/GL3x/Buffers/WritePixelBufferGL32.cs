#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.Diagnostics;
using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MiniGlobe.Renderer.GL3x
{
    internal class WritePixelBufferGL3x : WritePixelBuffer
    {
        public WritePixelBufferGL3x(WritePixelBufferHint usageHint, int sizeInBytes)
        {
            _bufferObject = new BufferGL3x(BufferTarget.PixelUnpackBuffer, ToBufferHint(usageHint), sizeInBytes);
            _usageHint = usageHint;
        }

        internal void Bind()
        {
            _bufferObject.Bind();
        }

        internal static void UnBind()
        {
            // TODO: avoid duplicate binds
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        }

        #region WritePixelBuffer Members

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

        public override WritePixelBufferHint UsageHint
        {
            get { return _usageHint; }
        }

        #endregion

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            _bufferObject.Dispose(disposing);
            base.Dispose(disposing);
        }

        #endregion

        static BufferHint ToBufferHint(WritePixelBufferHint usageHint)
        {
            Debug.Assert(
                (usageHint == WritePixelBufferHint.StreamDraw) ||
                (usageHint == WritePixelBufferHint.StaticDraw) ||
                (usageHint == WritePixelBufferHint.DynamicDraw));

            return _bufferHints[(int)usageHint];
        }

        private BufferGL3x _bufferObject;
        private WritePixelBufferHint _usageHint;

        private static readonly BufferHint[] _bufferHints = new[]
        {
            BufferHint.StreamDraw,
            BufferHint.StaticDraw,
            BufferHint.DynamicDraw
        };
    }
}
