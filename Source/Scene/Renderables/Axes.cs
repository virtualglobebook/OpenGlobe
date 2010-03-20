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
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Core;

namespace MiniGlobe.Scene
{
    public sealed class Axes : IDisposable
    {
        public Axes(Context context)
        {
            VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position", 4);
            positionAttribute.Values.Add(new Vector3D(0, 0, 0));
            positionAttribute.Values.Add(new Vector3D(1, 0, 0));
            positionAttribute.Values.Add(new Vector3D(0, 0, 0));
            positionAttribute.Values.Add(new Vector3D(0, 1, 0));
            positionAttribute.Values.Add(new Vector3D(0, 0, 0));
            positionAttribute.Values.Add(new Vector3D(0, 0, 1));

            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", 4);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Green);
            colorAttribute.AddColor(Color.Green);
            colorAttribute.AddColor(Color.Blue);
            colorAttribute.AddColor(Color.Blue);

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Lines;
            mesh.Attributes.Add(positionAttribute);
            mesh.Attributes.Add(colorAttribute);

            _polyline = new Polyline(context, mesh);
        }

        public void Render(SceneState sceneState)
        {
            _polyline.Render(sceneState);
        }

        public Context Context
        {
            get { return _polyline.Context; }
        }

        public double Width
        {
            get { return _polyline.Width; }
            set { _polyline.Width = value; }
        }
        
        public double Length
        {
            get { return _polyline.Length; }
            set { _polyline.Length = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _polyline.Dispose();
        }

        #endregion

        private readonly Polyline _polyline;
    }
}