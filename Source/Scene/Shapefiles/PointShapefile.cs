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
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    internal class PointShapefile : ShapefileGraphics
    {
        public PointShapefile(
            Shapefile shapefile, 
            Context context, 
            Ellipsoid globeShape,
            ShapefileAppearance appearance)
        {
            Verify.ThrowIfNull(shapefile);
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);
            Verify.ThrowIfNull(appearance);

            _billboards = new BillboardCollection(context);
            _billboards.Texture = Device.CreateTexture2D(appearance.Bitmap, TextureFormat.RedGreenBlueAlpha8, false);
            
            foreach (Shape shape in shapefile)
            {
                if (shape.ShapeType != ShapeType.Point)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                Vector2D point = ((PointShape)shape).Position;
                Vector3D position = globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y)));

                Billboard billboard = new Billboard();
                billboard.Position = position;
                _billboards.Add(billboard);
            }
        }

        #region ShapefileGraphics Members

        public override void Render(Context context, SceneState sceneState)
        {
            _billboards.Render(context, sceneState);
        }

        public override void Dispose()
        {
            if (_billboards != null)
            {
                _billboards.Dispose();
            }
        }

        public override bool Wireframe
        {
            get { return _billboards.Wireframe; }
            set { _billboards.Wireframe = value; }
        }

        #endregion

        public bool DepthWrite 
        {
            get { return _billboards.DepthWrite; }
            set { _billboards.DepthWrite = value; }
        }

        private readonly BillboardCollection _billboards;
    }
}