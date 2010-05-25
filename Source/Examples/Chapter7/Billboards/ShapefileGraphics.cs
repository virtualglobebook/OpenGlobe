#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using Catfood.Shapefile;

namespace MiniGlobe.Examples.Chapter7
{
    public class ShapefileGraphics : IDisposable
    {
        public ShapefileGraphics(Context context, Ellipsoid globeShape, string filename)
        {
            _context = context;

            // TODO: capacities

            using (Shapefile shapefile = new Shapefile(filename))
            {
                foreach (Shape shape in shapefile)
                {
                    switch (shape.Type)
                    {
                        case ShapeType.PolyLine:

                            ShapePolyLine shapePolyline = shape as ShapePolyLine;

                            foreach (PointD[] part in shapePolyline.Parts)
                            {
                                foreach (PointD point in part)
                                {
                                    globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y)));
                                }
                                Console.WriteLine();
                            }
                            break;

                        default:
                            Debug.Fail("Not Implemented");
                    }
                }
            }
        }

        public void Render(SceneState sceneState)
        {
        }

        public Context Context
        {
            get { return _context; }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        private Context _context;
    }
}