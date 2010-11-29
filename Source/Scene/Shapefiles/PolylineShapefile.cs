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
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public class PolylineShapefile : IRenderable, IDisposable
    {
        public PolylineShapefile(string filename, Context context, Ellipsoid globeShape)
            : this(filename, context, globeShape, Color.Yellow, Color.Black)
        {
        }

        public PolylineShapefile(string filename, Context context, Ellipsoid globeShape, Color color, Color outlineColor)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);

            Shapefile shapefile = new Shapefile(filename);

            if (shapefile.ShapeType == ShapeType.Polyline)
            {
                _polyline = new OutlinedPolylineTexture();
                CreatePolylines(context, globeShape, shapefile, color, outlineColor);
            }
            else
            {
                throw new NotSupportedException("Shapefile type \"" + shapefile.ShapeType.ToString() + "\" is not a polyline shape file.");
            }
        }

        private void CreatePolylines(Context context, Ellipsoid globeShape, Shapefile shapefile, Color color, Color outlineColor)
        {
            int positionsCount = 0;
            int indicesCount = 0;
            PolylineCapacities(shapefile, out positionsCount, out indicesCount);

            VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position", positionsCount);
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", positionsCount);
            VertexAttributeRGBA outlineColorAttribute = new VertexAttributeRGBA("outlineColor", positionsCount);
            IndicesUnsignedInt indices = new IndicesUnsignedInt(indicesCount);

            foreach (Shape shape in shapefile)
            {
                if (shape.ShapeType != ShapeType.Polyline)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                PolylineShape polylineShape = (PolylineShape)shape;

                for (int j = 0; j < polylineShape.Count; ++j)
                {
                    ShapePart part = polylineShape[j];

                    for (int i = 0; i < part.Count; ++i)
                    {
                        Vector2D point = part[i];

                        positionAttribute.Values.Add(globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y))));
                        colorAttribute.AddColor(color);
                        outlineColorAttribute.AddColor(outlineColor);

                        if (i != 0)
                        {
                            indices.Values.Add((uint)positionAttribute.Values.Count - 2);
                            indices.Values.Add((uint)positionAttribute.Values.Count - 1);
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

        private static void PolylineCapacities(Shapefile shapefile, out int positionsCount, out int indicesCount)
        {
            int numberOfPositions = 0;
            int numberOfIndices = 0;

            foreach (Shape shape in shapefile)
            {
                if (shape.ShapeType != ShapeType.Polyline)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                PolylineShape polylineShape = (PolylineShape)shape;

                for (int j = 0; j < polylineShape.Count; ++j)
                {
                    ShapePart part = polylineShape[j];

                    numberOfPositions += part.Count;
                    numberOfIndices += (part.Count - 1) * 2;
                }
            }

            positionsCount = numberOfPositions;
            indicesCount = numberOfIndices;
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            _polyline.Render(context, sceneState);
        }

        #endregion

        public double Width 
        {
            get { return _polyline.Width;  }
            set { _polyline.Width = value;  }
        }
        
        public double OutlineWidth 
        {
            get { return _polyline.OutlineWidth; }
            set { _polyline.OutlineWidth = value; }
        }
        
        public bool DepthWrite 
        {
            get { return _polyline.DepthWrite;  }
            set { _polyline.DepthWrite = value;  }
        }

        public bool Wireframe
        {
            get { return _polyline.Wireframe; }
            set { _polyline.Wireframe = value; }
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