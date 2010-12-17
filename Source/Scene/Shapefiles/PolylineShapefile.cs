#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
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
    internal class PolylineShapefile : ShapefileGraphics
    {
        public PolylineShapefile(
            Shapefile shapefile, 
            Context context, 
            Ellipsoid globeShape, 
            ShapefileAppearance appearance)
        {
            Verify.ThrowIfNull(shapefile);
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);
            Verify.ThrowIfNull(appearance);
            
            _polyline = new OutlinedPolylineTexture();

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
                        colorAttribute.AddColor(appearance.PolylineColor);
                        outlineColorAttribute.AddColor(appearance.PolylineOutlineColor);

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
            _polyline.Width = appearance.PolylineWidth;
            _polyline.OutlineWidth = appearance.PolylineOutlineWidth;
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

        #region ShapefileGraphics Members

        public override void Render(Context context, SceneState sceneState)
        {
            _polyline.Render(context, sceneState);
        }

        public override void Dispose()
        {
            if (_polyline != null)
            {
                _polyline.Dispose();
            }
        }

        public override bool Wireframe
        {
            get { return _polyline.Wireframe; }
            set { _polyline.Wireframe = value; }
        }

        #endregion

        public bool DepthWrite 
        {
            get { return _polyline.DepthWrite;  }
            set { _polyline.DepthWrite = value;  }
        }

        private readonly OutlinedPolylineTexture _polyline;
    }
}