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
                    CreatePolylines(globeShape, shapefile);
                }
                else
                {
                    throw new NotSupportedException("Shapefile type is not supported.");
                }
            }
        }

        private void CreatePolylines(Ellipsoid globeShape, Shapefile shapefile)
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
                        colorAttribute.AddColor(Color.Yellow);
                        outlineColorAttribute.AddColor(Color.Black);

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

        public void Render(SceneState sceneState)
        {
            if (_polyline != null)
            {
                _polyline.Render(sceneState);
            }
        }

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