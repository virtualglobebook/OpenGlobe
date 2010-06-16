#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class WritePixelBuffer : Disposable
    {
        public virtual void CopyFromSystemMemory<T>(T[] bufferInSystemMemory) where T : struct
        {
            CopyFromSystemMemory<T>(bufferInSystemMemory, 0);
        }

        public virtual void CopyFromSystemMemory<T>(T[] bufferInSystemMemory, int destinationOffsetInBytes) where T : struct
        {
            int lengthInBytes = bufferInSystemMemory.Length * SizeInBytes<T>.Value;

            CopyFromSystemMemory<T>(bufferInSystemMemory, destinationOffsetInBytes, lengthInBytes);
        }

        public abstract void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes) where T : struct;

        public abstract void CopyFromBitmap(Bitmap bitmap);

        public virtual T[] CopyToSystemMemory<T>() where T : struct
        {
            return CopyToSystemMemory<T>(0, SizeInBytes);
        }

        public abstract T[] CopyToSystemMemory<T>(int offsetInBytes, int sizeInBytes) where T : struct;
        public abstract Bitmap CopyToBitmap(int width, int height, PixelFormat pixelFormat);

        public abstract int SizeInBytes { get; }
        public abstract WritePixelBufferHint UsageHint { get; }
    }
}
