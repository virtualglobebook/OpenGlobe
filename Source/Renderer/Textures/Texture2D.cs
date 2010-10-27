#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class Texture2D : Disposable
    {
        public virtual void CopyFromBuffer(
            WritePixelBuffer pixelBuffer,
            ImageFormat format,
            ImageDatatype dataType)
        {
            CopyFromBuffer(pixelBuffer, 0, 0, Description.Width, Description.Height, format, dataType, 4);
        }

        public virtual void CopyFromBuffer(
            WritePixelBuffer pixelBuffer,
            ImageFormat format,
            ImageDatatype dataType,
            int rowAlignment)
        {
            CopyFromBuffer(pixelBuffer, 0, 0, Description.Width, Description.Height, format, dataType, rowAlignment);
        }

        public abstract void CopyFromBuffer(
            WritePixelBuffer pixelBuffer,
            int xOffset,
            int yOffset,
            int width,
            int height,
            ImageFormat format,
            ImageDatatype dataType,
            int rowAlignment);

        public virtual ReadPixelBuffer CopyToBuffer(ImageFormat format, ImageDatatype dataType)
        {
            return CopyToBuffer(format, dataType, 4);
        }
        
        public abstract ReadPixelBuffer CopyToBuffer(
            ImageFormat format, 
            ImageDatatype dataType,
            int rowAlignment);

        public abstract Texture2DDescription Description { get; }

        public virtual void Save(string filename)
        {
            if (Description.Format == TextureFormat.RedGreenBlue8)
            {
                SaveColor(filename);
            }
            else if ((Description.Format == TextureFormat.Depth16) ||
                     (Description.Format == TextureFormat.Depth24) ||
                     (Description.Format == TextureFormat.Depth32f))
            {
                SaveDepth(filename);
            }
            else
            {
                Debug.Fail("Texture2D.Save() is not implement for this TextureFormat.");
            }
        }

        private void SaveColor(string filename)
        {
            using (ReadPixelBuffer pixelBuffer = CopyToBuffer(ImageFormat.BlueGreenRed, ImageDatatype.UnsignedByte, 1))
            {
                Bitmap bitmap = pixelBuffer.CopyToBitmap(Description.Width, Description.Height, PixelFormat.Format24bppRgb);
                bitmap.Save(filename);
            }
        }

        private void SaveDepth(string filename)
        {
            using (ReadPixelBuffer pixelBuffer = CopyToBuffer(ImageFormat.DepthComponent, ImageDatatype.Float, 1))
            {
                float[] depths = pixelBuffer.CopyToSystemMemory<float>();

                float minValue = depths[0];
                float maxValue = depths[0];
                for (int i = 0; i < depths.Length; ++i)
                {
                    minValue = Math.Min(depths[i], minValue);
                    maxValue = Math.Max(depths[i], maxValue);
                }
                float deltaValue = maxValue - minValue;

                //
                // Avoid divide by zero if all depths are the same.
                //
                float oneOverDelta = (deltaValue > 0) ? (1 / deltaValue) : 1;

                Bitmap bitmap = new Bitmap(Description.Width, Description.Height, PixelFormat.Format24bppRgb);
                int j = 0;
                for (int y = Description.Height - 1; y >= 0; --y)
                {
                    for (int x = 0; x < Description.Width; ++x)
                    {
                        float linearDepth = (depths[j++] - minValue) * oneOverDelta;
                        int intensity = (int)(linearDepth * 255.0f);
                        bitmap.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                    }
                }
                bitmap.Save(filename);
            }
        }
    }
}
