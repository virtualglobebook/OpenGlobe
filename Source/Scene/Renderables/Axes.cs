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
            _context = context;
            Length = 1;
            Width = 1;
        }

        private void Update()
        {
            if (_dirtyLength)
            {
                VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position", 4);
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(_length, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, _length, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, _length));

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

                if (_polyline != null)
                {
                    _polyline.Dispose();
                }
                _polyline = new Polyline(_context, mesh);

                _dirtyLength = false;
            }

            _polyline.Width = Width;
        }

        public void Render(SceneState sceneState)
        {
            Update();
            _polyline.Render(sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Length
        {
            get { return _length; }
            set 
            {
                if (_length != value)
                {
                    _length = value;
                    _dirtyLength = true;
                }
            }
        }
        
        public double Width { get; set; }

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
        private Polyline _polyline;
        private double _length;
        private bool _dirtyLength;
    }
}