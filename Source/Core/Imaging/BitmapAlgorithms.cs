#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using System.Drawing.Imaging;

namespace MiniGlobe.Core
{
    public enum ImageRowOrder
    {
        BottomToTop,
        TopToBottom
    }
    
    public static class BitmapAlgorithms
    {
        public static ImageRowOrder RowOrder(Bitmap bitmap)
        {
            BitmapData lockedPixels = bitmap.LockBits(new Rectangle(
                0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            ImageRowOrder rowOrder;
            if (lockedPixels.Stride > 0)
            {
                rowOrder = ImageRowOrder.TopToBottom;
            }
            else
            {
                rowOrder = ImageRowOrder.BottomToTop;
            }

            bitmap.UnlockBits(lockedPixels);

            return rowOrder;
        }

        public static int SizeOfPixelsInBytes(Bitmap bitmap)
        {
            BitmapData lockedPixels = bitmap.LockBits(new Rectangle(
                0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);

            //
            // Includes row padding for Bitmap's 4 byte alignment.
            //
            int sizeInBytes = lockedPixels.Stride * bitmap.Height;
            bitmap.UnlockBits(lockedPixels);

            return sizeInBytes;
        }

    }
}