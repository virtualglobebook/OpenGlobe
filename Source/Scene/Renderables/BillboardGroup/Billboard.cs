#region License
//
// (C) Copyright 20010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;
using System.Drawing;

namespace MiniGlobe.Scene
{
    public class Billboard
    {
        public Billboard()
        {
            _textureCoordinates = new RectangleH(Vector2H.Zero, new Vector2H(1.0, 1.0));
        }

        public Vector3D Position 
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    MakeDirty();
                }
            }
        }

        public RectangleH TextureCoordinates
        {
            get { return _textureCoordinates; }
            set
            {
                if (_textureCoordinates != value)
                {
                    _textureCoordinates = value;
                    MakeDirty();
                }
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    MakeDirty();
                }
            }
        }

        private void MakeDirty()
        {
            if (!Dirty)
            {
                Dirty = true;

                if (Group != null)
                {
                    Group.NotifyDirty(this);
                }
            }
        }

        internal bool Dirty { get; set; }
        public BillboardGroup2 Group 
        { 
            get; 
            internal set; 
        }
        internal int VertexBufferOffset { get; set; }

        private Vector3D _position;
        private RectangleH _textureCoordinates;
        private Color _color;
    }
}
