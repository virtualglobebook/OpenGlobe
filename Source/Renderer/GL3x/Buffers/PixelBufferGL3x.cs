#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class PixelBufferGL3x : IDisposable
    {
        public PixelBufferGL3x(
            BufferTarget type,
            BufferHint usageHint,
            int sizeInBytes)
        {
            if (sizeInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException("sizeInBytes", "sizeInBytes must be greater than zero.");
            }

            _name = new BufferNameGL3x();

            _sizeInBytes = sizeInBytes;
            _type = type;
            _usageHint = TypeConverterGL3x.To(usageHint);

            //
            // Allocating here with GL.BufferData, then writing with GL.BufferSubData
            // in CopyFromSystemMemory() should not have any serious overhead:
            //
            //   http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=267373#Post267373
            //
            // Alternately, we can delay GL.BufferData until the first
            // CopyFromSystemMemory() call.
            //
            Bind();
            GL.BufferData(_type, new IntPtr(sizeInBytes), new IntPtr(), _usageHint);

            GC.AddMemoryPressure(sizeInBytes);
        }

        public void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes) where T : struct
        {
            if (destinationOffsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("destinationOffsetInBytes", 
                    "destinationOffsetInBytes must be greater than or equal to zero.");
            }

            if (destinationOffsetInBytes + lengthInBytes > _sizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "destinationOffsetInBytes + lengthInBytes must be less than or equal to SizeInBytes.");
            }

            if (lengthInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be greater than or equal to zero.");
            }

            if (lengthInBytes > ArraySizeInBytes.Size(bufferInSystemMemory))
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be less than or equal to the size of bufferInSystemMemory in bytes.");
            }

            Bind();
            GL.BufferSubData<T>(_type,
                new IntPtr(destinationOffsetInBytes),
                new IntPtr(lengthInBytes),
                bufferInSystemMemory);
        }
        
        public void CopyFromBitmap(Bitmap bitmap)
        {
            Bitmap lockedBitmap;

            if (BitmapAlgorithms.RowOrder(bitmap) == ImageRowOrder.TopToBottom)
            {
                //
                // OpenGL wants rows bottom to top.
                //
                Bitmap flippedBitmap = (Bitmap)bitmap.Clone();
                flippedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                lockedBitmap = flippedBitmap;
            }
            else
            {
                lockedBitmap = bitmap;
            }

            BitmapData lockedPixels = lockedBitmap.LockBits(new Rectangle(
                0, 0, lockedBitmap.Width, lockedBitmap.Height),
                ImageLockMode.ReadOnly, lockedBitmap.PixelFormat);

            int sizeInBytes = lockedPixels.Stride * lockedPixels.Height;

            if (sizeInBytes > _sizeInBytes)
            {
                throw new ArgumentException(
                    "bitmap's size in bytes must be less than or equal to SizeInBytes.", 
                    "bitmap");
            }

            Bind();
            GL.BufferSubData(_type,
                new IntPtr(),
                new IntPtr(sizeInBytes),
                lockedPixels.Scan0);

            lockedBitmap.UnlockBits(lockedPixels);
        }

        public T[] CopyToSystemMemory<T>(int offsetInBytes, int lengthInBytes) where T : struct
        {
            if (offsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("offsetInBytes",
                    "offsetInBytes must be greater than or equal to zero.");
            }

            if (lengthInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be greater than zero.");
            }

            if (offsetInBytes + lengthInBytes > _sizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "offsetInBytes + lengthInBytes must be less than or equal to SizeInBytes.");
            }

            T[] bufferInSystemMemory = new T[lengthInBytes / SizeInBytes<T>.Value];

            Bind();
            GL.GetBufferSubData(_type, new IntPtr(offsetInBytes), new IntPtr(lengthInBytes), bufferInSystemMemory);
            return bufferInSystemMemory;
        }

        public Bitmap CopyToBitmap(int width, int height, ImagingPixelFormat pixelFormat)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width",
                    "width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height",
                    "height must be greater than zero.");
            }

            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            BitmapData lockedPixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int sizeInBytes = lockedPixels.Stride * lockedPixels.Height;

            Bind();
            GL.GetBufferSubData(_type, new IntPtr(), new IntPtr(sizeInBytes), lockedPixels.Scan0);

            bitmap.UnlockBits(lockedPixels);

            //
            // OpenGL had rows bottom to top.  Bitmap wants them top to bottom.
            //
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bitmap;
        }

        public int SizeInBytes
        {
            get { return _sizeInBytes; }
        }

        public BufferHint UsageHint
        {
            get { return TypeConverterGL3x.To(_usageHint); }
        }

        public BufferNameGL3x Handle
        {
            get { return _name; }
        }

        public void Bind()
        {
            GL.BindBuffer(_type, _name.Value);
        }

        public void Dispose()
        {
            _name.Dispose();
            GC.RemoveMemoryPressure(_sizeInBytes);
        }

        private BufferNameGL3x _name;
        private readonly int _sizeInBytes;
        private readonly BufferTarget _type;
        private readonly BufferUsageHint _usageHint;
    }
}
