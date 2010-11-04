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
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class Axes : IDisposable
    {
        public Axes()
        {
            _polyline = new Polyline();
            Length = 1;
            Width = 1;
        }

        private void Update(Context context)
        {
            if (_dirtyLength)
            {
                VertexAttributeDoubleVector3 positionAttribute = new VertexAttributeDoubleVector3("position", 6);
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(_length, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, _length, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, 0));
                positionAttribute.Values.Add(new Vector3D(0, 0, _length));

                VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", 6);
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

                _polyline.Set(context, mesh);

                _dirtyLength = false;
            }

            _polyline.Width = Width;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Update(context);
            _polyline.Render(context, sceneState);
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
            _polyline.Dispose();
        }

        #endregion

        private Polyline _polyline;
        private double _length;
        private bool _dirtyLength;
    }
}