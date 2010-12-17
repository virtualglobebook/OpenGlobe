#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;
using System.Drawing.Imaging;

namespace OpenGlobe.Core
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