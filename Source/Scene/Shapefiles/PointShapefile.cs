#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;
using Catfood.Shapefile;

namespace OpenGlobe.Scene
{
    public class PointShapefile : IRenderable, IDisposable
    {
        public PointShapefile(string filename, string labelName, Context context, Ellipsoid globeShape, Bitmap icon)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);

            using (Shapefile shapefile = new Shapefile(filename))
            {
                if (shapefile.Type == ShapeType.Point)
                {
                    _billboards = new BillboardCollection(context);
                    CreateBillboards(labelName, globeShape, shapefile, icon);
                }
                else
                {
                    throw new NotSupportedException("Shapefile type \"" + shapefile.Type.ToString() + "\" is not a point shape file.");
                }
            }
        }

        private void CreateBillboards(string labelName, Ellipsoid globeShape, Shapefile shapefile, Bitmap iconBitmap)
        {
            Font font = new Font("Arial", 16);
            IList<Bitmap> bitmaps = new List<Bitmap>();
            bitmaps.Add(iconBitmap);
            int labelPixelOffset = iconBitmap.Width / 2;

            foreach (Shape shape in shapefile)
            {
                if (shape.Type == ShapeType.Null)
                {
                    continue;
                }

                if (shape.Type != ShapeType.Point)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                PointD point = (shape as ShapePoint).Point;
                Vector3D position = globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y))); ;

                Billboard icon = new Billboard();
                icon.Position = position;
                _billboards.Add(icon);

                if (labelName != null)
                {
                    string labelText = shape.GetMetadata(labelName);

                    bitmaps.Add(Device.CreateBitmapFromText(labelText, font));

                    Billboard label = new Billboard();
                    label.Position = position;
                    label.HorizontalOrigin = HorizontalOrigin.Left;
                    label.PixelOffset = new Vector2H(labelPixelOffset, 0);
                    _billboards.Add(label);
                }
            }

            if (labelName != null)
            {
                TextureAtlas labelAtlas = new TextureAtlas(bitmaps);
                int j = 1;
                for (int i = 0; i < _billboards.Count; i += 2)
                {
                    _billboards[i].TextureCoordinates = labelAtlas.TextureCoordinates[0];
                    _billboards[i + 1].TextureCoordinates = labelAtlas.TextureCoordinates[j];
                    ++j;
                }
                _billboards.Texture = Device.CreateTexture2D(labelAtlas.Bitmap, TextureFormat.RedGreenBlueAlpha8, false);
            }
            else
            {
                _billboards.Texture = Device.CreateTexture2D(iconBitmap, TextureFormat.RedGreenBlueAlpha8, false);
            }

            for (int i = 1; i < bitmaps.Count; ++i)
            {
                bitmaps[i].Dispose();
            }
            font.Dispose();
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            _billboards.Render(context, sceneState);
        }

        #endregion

        public bool DepthWrite 
        {
            get { return _billboards.DepthWrite; }
            set { _billboards.DepthWrite = value; }
        }

        public bool Wireframe
        {
            get { return _billboards.Wireframe; }
            set { _billboards.Wireframe = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_billboards != null)
            {
                _billboards.Dispose();
            }
        }

        #endregion

        private readonly BillboardCollection _billboards;
    }
}