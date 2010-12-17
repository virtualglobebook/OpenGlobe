#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public class ShapefileRenderer : IRenderable, IDisposable
    {
        public ShapefileRenderer(
            string filename, 
            Context context, 
            Ellipsoid globeShape,
            ShapefileAppearance appearance)
        {
            Shapefile shapefile = new Shapefile(filename);

            switch (shapefile.ShapeType)
            {
                case ShapeType.Point:
                    PointShapefile pointShapefile = new PointShapefile(shapefile, context, globeShape, appearance);
                    pointShapefile.DepthWrite = false;
                    _shapefileGraphics = pointShapefile;
                    break;
                case ShapeType.Polyline:
                    PolylineShapefile polylineShapefile = new PolylineShapefile(shapefile, context, globeShape, appearance);
                    polylineShapefile.DepthWrite = false;
                    _shapefileGraphics = polylineShapefile;
                    break;
                case ShapeType.Polygon:
                    PolygonShapefile polygonShapefile = new PolygonShapefile(shapefile, context, globeShape, appearance);
                    polygonShapefile.DepthWrite = false;
                    _shapefileGraphics = polygonShapefile;
                    break;
                default:
                    throw new NotSupportedException("Rendering is not supported for " + shapefile.ShapeType.ToString() + " shapefiles.");
            }
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            _shapefileGraphics.Render(context, sceneState);
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (_shapefileGraphics != null)
            {
                _shapefileGraphics.Dispose();
            }
        }

        #endregion

        public bool Wireframe
        {
            get { return _shapefileGraphics.Wireframe; }
            set { _shapefileGraphics.Wireframe = value; }
        }

        private readonly ShapefileGraphics _shapefileGraphics;
    }
}