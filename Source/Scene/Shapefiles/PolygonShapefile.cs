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
    internal class PolygonShapefile : ShapefileGraphics
    {
        public PolygonShapefile(
            Shapefile shapefile, 
            Context context, 
            Ellipsoid globeShape,
            ShapefileAppearance appearance)
        {
            Verify.ThrowIfNull(shapefile);
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);

            _polyline = new OutlinedPolylineTexture();
            _polygons = new List<Polygon>();

            VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position");
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color");
            VertexAttributeRGBA outlineColorAttribute = new VertexAttributeRGBA("outlineColor");
            IndicesUnsignedInt indices = new IndicesUnsignedInt();

            Random r = new Random(3);
            IList<Vector3D> positions = new List<Vector3D>();

            foreach (Shape shape in shapefile)
            {
                if (shape.ShapeType != ShapeType.Polygon)
                {
                    throw new NotSupportedException("The type of an individual shape does not match the Shapefile type.");
                }

                PolygonShape polygonShape = (PolygonShape)shape;

                for (int j = 0; j < polygonShape.Count; ++j)
                {
                    Color color = Color.FromArgb(127, r.Next(256), r.Next(256), r.Next(256));

                    positions.Clear();

                    ShapePart part = polygonShape[j];

                    for (int i = 0; i < part.Count; ++i)
                    {
                        Vector2D point = part[i];

                        positions.Add(globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y))));

                        //
                        // For polyline
                        //
                        positionAttribute.Values.Add(globeShape.ToVector3D(Trig.ToRadians(new Geodetic3D(point.X, point.Y))));
                        colorAttribute.AddColor(color);
                        outlineColorAttribute.AddColor(Color.Black);

                        if (i != 0)
                        {
                            indices.Values.Add((uint)positionAttribute.Values.Count - 2);
                            indices.Values.Add((uint)positionAttribute.Values.Count - 1);
                        }
                    }

                    try
                    {
                        Polygon p = new Polygon(context, globeShape, positions);
                        p.Color = color;
                        _polygons.Add(p);
                    }
                    catch (ArgumentOutOfRangeException) // Not enough positions after cleaning
                    {
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
        }

        #region ShapefileGraphics Members

        public override void Render(Context context, SceneState sceneState)
        {
            _polyline.Render(context, sceneState);

            foreach (Polygon polygon in _polygons)
            {
                polygon.Render(context, sceneState);
            }
        }

        public override void Dispose()
        {
            if (_polyline != null)
            {
                _polyline.Dispose();
            }

            foreach (Polygon polygon in _polygons)
            {
                polygon.Dispose();
            }
        }

        public override bool Wireframe
        {
            get { return _polyline.Wireframe; }
            set
            {
                _polyline.Wireframe = value;

                foreach (Polygon polygon in _polygons)
                {
                    polygon.Wireframe = value;
                }
            }
        }

        #endregion

        /*
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
        */

        public bool DepthWrite 
        {
            get { return _polyline.DepthWrite;  }
            set 
            { 
                _polyline.DepthWrite = value;  

                foreach (Polygon polygon in _polygons)
                {
                    polygon.DepthWrite = value;
                }
            }
        }

        private readonly OutlinedPolylineTexture _polyline;
        private readonly IList<Polygon> _polygons;
    }
}