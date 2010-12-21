#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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

        public virtual void CopyFromFramebuffer()
        {
            CopyFromFramebuffer(0, 0, 0, 0, Description.Width, Description.Height);
        }

        public abstract void CopyFromFramebuffer(
            int xOffset,
            int yOffset,
            int framebufferXOffset,
            int framebufferYOffset,
            int width,
            int height);

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
            if (Description.TextureFormat == TextureFormat.RedGreenBlue8)
            {
                SaveColor(filename);
            }
            else if ((Description.TextureFormat == TextureFormat.Depth16) ||
                     (Description.TextureFormat == TextureFormat.Depth24) ||
                     (Description.TextureFormat == TextureFormat.Depth32f))
            {
                SaveDepth(filename);
            }
            else if ((Description.TextureFormat == TextureFormat.Red32f))
            {
                SaveRed(filename);
            }
            else
            {
                Debug.Fail("Texture2D.Save() is not implement for this TextureFormat.");
            }
        }

        private void SaveColor(string filename)
        {
            //
            // The pixel buffer uses four byte row alignment because it matches
            // a bitmap's row alignment (BitmapData.Stride).
            //
            using (ReadPixelBuffer pixelBuffer = CopyToBuffer(ImageFormat.BlueGreenRed, ImageDatatype.UnsignedByte, 4))
            {
                Bitmap bitmap = pixelBuffer.CopyToBitmap(Description.Width, Description.Height, PixelFormat.Format24bppRgb);
                bitmap.Save(filename);
            }
        }

        private void SaveDepth(string filename)
        {
            SaveFloat(filename, ImageFormat.DepthComponent);
        }

        private void SaveRed(string filename)
        {
            SaveFloat(filename, ImageFormat.Red);
        }

        private void SaveFloat(string filename, ImageFormat imageFormat)
        {
            using (ReadPixelBuffer pixelBuffer = CopyToBuffer(imageFormat, ImageDatatype.Float, 1))
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
