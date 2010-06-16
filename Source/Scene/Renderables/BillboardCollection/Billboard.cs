#region License
//
// (C) Copyright 20010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;

namespace OpenGlobe.Scene
{
    public enum HorizontalOrigin
    {
        Center = 0,
        Left = 1,
        Right = 2
    }

    public enum VerticalOrigin
    {
        Center = 0,
        Bottom = 1,
        Top = 2,
    }

    public class Billboard
    {
        public Billboard()
        {
            _textureCoordinates = new RectangleH(Vector2H.Zero, new Vector2H(1.0, 1.0));
            _color = Color.White;
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

        public HorizontalOrigin HorizontalOrigin
        {
            get { return (HorizontalOrigin)((_origins & _horizontalOriginMask) >> _horizontalOriginShift); }
            set
            {
                if (HorizontalOrigin != value)
                {
                    _origins &= ~_horizontalOriginMask;
                    _origins |= (((uint)value << _horizontalOriginShift) & _horizontalOriginMask);
                    MakeDirty();
                }
            }
        }

        public VerticalOrigin VerticalOrigin
        {
            get { return (VerticalOrigin)((_origins & _verticalOriginMask) >> _verticalOriginShift); }
            set
            {
                if (VerticalOrigin != value)
                {
                    _origins &= ~_verticalOriginMask;
                    _origins |= (((uint)value << _verticalOriginShift) & _verticalOriginMask);
                    MakeDirty();
                }
            }
        }

        public Vector2H PixelOffset
        {
            get { return _pixelOffset; }
            set
            {
                if (_pixelOffset != value)
                {
                    _pixelOffset = value;
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
        public BillboardCollection Group 
        { 
            get; 
            internal set; 
        }
        internal int VertexBufferOffset { get; set; }

        private Vector3D _position;
        private RectangleH _textureCoordinates;
        private Color _color;
        private uint _origins;
        private Vector2H _pixelOffset;

        private const short _horizontalOriginShift = 0;
        private const short _verticalOriginShift = 2;
        private const uint _horizontalOriginMask = 0x03 << _horizontalOriginShift;
        private const uint _verticalOriginMask = 0x03 << _verticalOriginShift;
    }
}
