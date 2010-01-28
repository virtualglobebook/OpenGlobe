#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Diagnostics;

using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MiniGlobe.Renderer
{
    internal static class TextureUtility
    {
        public static ImageDataType ImagingPixelFormatToDataType(ImagingPixelFormat pixelFormat)
        {
            // TODO:  Not tested exhaustively
            Debug.Assert(Supported(pixelFormat));

            if (pixelFormat == ImagingPixelFormat.Format16bppRgb555)
            {
                return ImageDataType.UnsignedShort5551;
            }
            else if (pixelFormat == ImagingPixelFormat.Format16bppRgb565)
            {
                return ImageDataType.UnsignedShort565;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format24bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppArgb))
            {
                return ImageDataType.UnsignedByte;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format48bppRgb) ||
                     (pixelFormat == ImagingPixelFormat.Format64bppArgb))
            {
                return ImageDataType.UnsignedShort;
            }
            else if (pixelFormat == ImagingPixelFormat.Format16bppArgb1555)
            {
                return ImageDataType.UnsignedShort1555Reversed;
            }

            Debug.Fail("pixelFormat");
            return ImageDataType.UnsignedByte;
        }

        public static ImageFormat ImagingPixelFormatToImageFormat(ImagingPixelFormat pixelFormat)
        {
            Debug.Assert(Supported(pixelFormat));

            if ((pixelFormat == ImagingPixelFormat.Format16bppRgb555) ||
                (pixelFormat == ImagingPixelFormat.Format16bppRgb565) ||
                (pixelFormat == ImagingPixelFormat.Format24bppRgb) ||
                (pixelFormat == ImagingPixelFormat.Format32bppRgb) ||
                (pixelFormat == ImagingPixelFormat.Format48bppRgb))
            {
                return ImageFormat.BlueGreenRed;
            }
            else if ((pixelFormat == ImagingPixelFormat.Format16bppArgb1555) ||
                     (pixelFormat == ImagingPixelFormat.Format32bppArgb) ||
                     (pixelFormat == ImagingPixelFormat.Format64bppArgb))
            {
                // TODO:  Test a Bitmap with alpha channel
                return ImageFormat.BlueGreenRedAlpha;
            }

            Debug.Fail("pixelFormat");
            return ImageFormat.BlueGreenRed;
        }

        private static bool Supported(ImagingPixelFormat pixelFormat)
        {
            return
                (pixelFormat != ImagingPixelFormat.DontCare) &&
                (pixelFormat != ImagingPixelFormat.Undefined) &&
                (pixelFormat != ImagingPixelFormat.Indexed) &&
                (pixelFormat != ImagingPixelFormat.Gdi) &&
                (pixelFormat != ImagingPixelFormat.Extended) &&
                (pixelFormat != ImagingPixelFormat.Format1bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Format4bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Format8bppIndexed) &&
                (pixelFormat != ImagingPixelFormat.Alpha) &&
                (pixelFormat != ImagingPixelFormat.PAlpha) &&
                (pixelFormat != ImagingPixelFormat.Format32bppPArgb) &&
                (pixelFormat != ImagingPixelFormat.Format16bppGrayScale) &&
                (pixelFormat != ImagingPixelFormat.Format64bppPArgb) &&
                (pixelFormat != ImagingPixelFormat.Canonical);
        }

        public static bool IsPowerOfTwo(uint i)
        {
            return (i != 0) && ((i & (i - 1)) == 0);
        }

        public static int RequiredSizeInBytes(int width, int height, ImageFormat format, ImageDataType dataType)
        {
            return width * height * NumberOfChannels(format) * SizeInBytes(dataType);
        }

        public static int NumberOfChannels(ImageFormat format)
        {
            Debug.Assert(
                (format == ImageFormat.StencilIndex) ||
                (format == ImageFormat.DepthComponent) ||
                (format == ImageFormat.Red) ||
                (format == ImageFormat.Green) ||
                (format == ImageFormat.Blue) ||
                (format == ImageFormat.RedGreenBlue) ||
                (format == ImageFormat.RedGreenBlueAlpha) ||
                (format == ImageFormat.BlueGreenRed) ||
                (format == ImageFormat.BlueGreenRedAlpha) ||
                (format == ImageFormat.RedGreen) ||
                (format == ImageFormat.RedGreenInteger) ||
                (format == ImageFormat.DepthStencil) ||
                (format == ImageFormat.RedInteger) ||
                (format == ImageFormat.GreenInteger) ||
                (format == ImageFormat.BlueInteger) ||
                (format == ImageFormat.RedGreenBlueInteger) ||
                (format == ImageFormat.RedGreenBlueAlphaInteger) ||
                (format == ImageFormat.BlueGreenRedInteger) ||
                (format == ImageFormat.BlueGreenRedAlphaInteger));

            return _numberOfChannels[(int)format];
        }

        public static int SizeInBytes(ImageDataType dataType)
        {
            Debug.Assert(
                (dataType == ImageDataType.Byte) ||
                (dataType == ImageDataType.UnsignedByte) ||
                (dataType == ImageDataType.Short) ||
                (dataType == ImageDataType.UnsignedShort) ||
                (dataType == ImageDataType.Int) ||
                (dataType == ImageDataType.UnsignedInt) ||
                (dataType == ImageDataType.Float) ||
                (dataType == ImageDataType.HalfFloat) ||
                (dataType == ImageDataType.UnsignedByte332) ||
                (dataType == ImageDataType.UnsignedShort4444) ||
                (dataType == ImageDataType.UnsignedShort5551) ||
                (dataType == ImageDataType.UnsignedInt8888) ||
                (dataType == ImageDataType.UnsignedInt1010102) ||
                (dataType == ImageDataType.UnsignedByte233Reversed) ||
                (dataType == ImageDataType.UnsignedShort565) ||
                (dataType == ImageDataType.UnsignedShort565Reversed) ||
                (dataType == ImageDataType.UnsignedShort4444Reversed) ||
                (dataType == ImageDataType.UnsignedShort1555Reversed) ||
                (dataType == ImageDataType.UnsignedInt8888Reversed) ||
                (dataType == ImageDataType.UnsignedInt2101010Reversed) ||
                (dataType == ImageDataType.UnsignedInt248) ||
                (dataType == ImageDataType.UnsignedInt10F11F11FReversed) ||
                (dataType == ImageDataType.UnsignedInt5999Reversed) ||
                (dataType == ImageDataType.Float32UnsignedInt248Reversed));

            return _sizeInBytes[(int)dataType];
        }

        private static readonly int[] _numberOfChannels = new int[]
        {
            1, // StencilIndex
            1, // DepthComponent
            1, // Red
            1, // Green
            1, // Blue
            3, // Rgb
            4, // Rgba
            3, // Bgr
            4, // Bgra
            2, // Rg
            2, // RgInteger
            2, // DepthStencil
            1, // RedInteger
            1, // GreenInteger
            1, // BlueInteger
            3, // RgbInteger
            4, // RgbaInteger
            3, // BgrInteger
            4, // BgraInteger
        };

        private static readonly int[] _sizeInBytes = new int[]
        {
            1, // Byte
            1, // UnsignedByte
            2, // Short
            2, // UnsignedShort
            4, // Int
            4, // UnsignedInt
            4, // Float
            2, // HalfFloat
            1, // UnsignedByte332
            2, // UnsignedShort4444
            2, // UnsignedShort5551
            4, // UnsignedInt8888
            4, // UnsignedInt1010102
            1, // UnsignedByte233Reversed
            2, // UnsignedShort565
            2, // UnsignedShort565Reversed
            2, // UnsignedShort4444Reversed
            2, // UnsignedShort1555Reversed
            4, // UnsignedInt8888Reversed
            4, // UnsignedInt2101010Reversed
            4, // UnsignedInt248
            4, // UnsignedInt10F11F11FReversed
            4, // UnsignedInt5999Reversed
            8  // Float32UnsignedInt248Reversed
        };
    }
}
