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
using MiniGlobe.Scene;
using Catfood.Shapefile;

namespace MiniGlobe.Examples.Chapter7
{
    public class ShapefileGraphics : IDisposable
    {
        public ShapefileGraphics(Context context, Ellipsoid globeShape, string filename)
            : this(context, globeShape, filename, Color.Yellow, Color.Black)
        {
        }

        public ShapefileGraphics(Context context, Ellipsoid globeShape, string filename, Color color, Color outlineColor)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (globeShape == null)
            {
                throw new ArgumentNullException("globeShape");
            }

            _context = context;

            using (Shapefile shapefile = new Shapefile(filename))
            {
                if (shapefile.Type == ShapeType.PolyLine)
                {
                    _polyline = new OutlinedPolylineTexture(context);
                    CreatePolylines(globeShape, shapefile, color, outlineColor);
                }
                else if (shapefile.Type == ShapeType.Polygon)
                {
                    _polyline = new OutlinedPolylineTexture(context);
                    CreatePolygons(globeShape, shapefile, color, outlineColor);
                }
                else
                {
                    throw new NotSupportedException("Shapefile type is not supported.");
                }
            }
        }

        private void CreatePolylines(Ellipsoid globeShape, Shapefile shapefile, Color color, Color outlineColor)
        {
            int positionsCount = 0;
            int indicesCount = 0;
            //PolylineCapacities(shapefile, out positionsCount, out indicesCount);

            VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position", positionsCount);
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", positionsCount);
            VertexAttributeRGBA outlineColorAttribute = new VertexAttributeRGBA("outlineColor", positionsCount);
            IndicesInt32 indices = new IndicesInt32(indicesCount);

            foreach (Shape shape in shapefile)
            {
                if (shape.Type == ShapeType.Null)
                {
                    continue;
                }

                if (shape.Type != ShapeType.PolyLine)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                IList<PointD[]> parts = (shape as ShapePolyLine).Parts;

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
            _polyline.Set(mesh);

            PolylineWidth = _polyline.Width;
            PolylineOutlineWidth = _polyline.OutlineWidth;
        }

        private static void PolylineCapacities(Shapefile shapefile, out int positionsCount, out int indicesCount)
        {
            int numberOfPositions = 0;
            int numberOfIndices = 0;

            foreach (Shape shape in shapefile)
            {
                if (shape.Type == ShapeType.Null)
                {
                    continue;
                }

                if (shape.Type != ShapeType.PolyLine)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                IList<PointD[]> parts = (shape as ShapePolyLine).Parts;

                for (int i = 0; i < parts.Count; ++i)
                {
                    numberOfPositions += parts[i].Length;
                    numberOfIndices += (parts[i].Length - 1) * 2;
                }
            }

            positionsCount = numberOfPositions;
            indicesCount = numberOfIndices;
        }

        private void CreatePolygons(Ellipsoid globeShape, Shapefile shapefile, Color color, Color outlineColor)
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
            _polyline.Set(mesh);

            PolylineWidth = _polyline.Width;
            PolylineOutlineWidth = _polyline.OutlineWidth;
        }

        public void Render(SceneState sceneState)
        {
            if (_polyline != null)
            {
                _polyline.Width = PolylineWidth;
                _polyline.OutlineWidth = PolylineOutlineWidth;
                _polyline.Render(sceneState);
            }
        }

        public double PolylineWidth { get; set; }
        public double PolylineOutlineWidth { get; set; }

        public Context Context
        {
            get { return _context; }
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

        private readonly Context _context;
        private readonly OutlinedPolylineTexture _polyline;
    }
}