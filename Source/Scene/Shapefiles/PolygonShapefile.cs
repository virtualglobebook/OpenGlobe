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
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using Catfood.Shapefile;

namespace MiniGlobe.Scene
{
    public class PolygonShapefile : IDisposable
    {
        public PolygonShapefile(string filename, Context context, Ellipsoid globeShape)
            : this(filename, context, globeShape, Color.Yellow, Color.Black)
        {
        }

        public PolygonShapefile(string filename, Context context, Ellipsoid globeShape, Color color, Color outlineColor)
        {
            Verify.ThrowIfNull(context);

            if (globeShape == null)
            {
                throw new ArgumentNullException("globeShape");
            }

            using (Shapefile shapefile = new Shapefile(filename))
            {
                if (shapefile.Type == ShapeType.Polygon)
                {
                    _polyline = new OutlinedPolylineTexture();
                    CreatePolygons(context, globeShape, shapefile, color, outlineColor);
                }
                else
                {
                    throw new NotSupportedException("Shapefile type \"" + shapefile.Type.ToString() + "\" is not a polygon shape file.");
                }
            }
        }

        private void CreatePolygons(Context context, Ellipsoid globeShape, Shapefile shapefile, Color color, Color outlineColor)
        {
            //
            // TODO:  This is temporary.  Since polygon tessellation is not supported yet,
            // polylines are created instead of polygons.
            //
            VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position");
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color");
            VertexAttributeRGBA outlineColorAttribute = new VertexAttributeRGBA("outlineColor");
            IndicesInt32 indices = new IndicesInt32();

            foreach (Shape shape in shapefile)
            {
                if (shape.Type == ShapeType.Null)
                {
                    continue;
                }

                if (shape.Type != ShapeType.Polygon)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                IList<PointD[]> parts = (shape as ShapePolygon).Parts;

                for (int j = 0; j < parts.Count; ++j)
                {
                    PointD[] part = parts[j];

                    for (int i = 0; i < part.Length; ++i)
                    {
                        PointD point = part[i];

                        positionAttribute.Values.Add(globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y))));
                        colorAttribute.AddColor(color);
                        outlineColorAttribute.AddColor(outlineColor);

                        if (i != 0)
                        {
                            indices.Values.Add(positionAttribute.Values.Count - 2);
                            indices.Values.Add(positionAttribute.Values.Count - 1);
                        }
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Lines;
            mesh.Attributes.Add(positionAttribute);
            mesh.Attributes.Add(colorAttribute);
            mesh.Attributes.Add(outlineColorAttribute);
            mesh.Indices = indices;
            _polyline.Set(context, mesh);

            Width = _polyline.Width;
            OutlineWidth = _polyline.OutlineWidth;
        }

        public void Render(Context context, SceneState sceneState)
        {
            _polyline.Render(context, sceneState);
        }

        public double Width 
        {
            get { return _polyline.OutlineWidth;  }
            set { _polyline.OutlineWidth = value;  }
        }
        
        public double OutlineWidth 
        {
            get { return _polyline.Width;  }
            set { _polyline.Width = value;  }
        }
        
        public bool DepthWrite 
        {
            get { return _polyline.DepthWrite;  }
            set { _polyline.DepthWrite = value;  }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_polyline != null)
            {
                _polyline.Dispose();
            }
        }

        #endregion

        private readonly OutlinedPolylineTexture _polyline;
    }
}