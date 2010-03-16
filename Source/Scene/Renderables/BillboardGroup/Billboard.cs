#region License
//
// (C) Copyright 20010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;

namespace MiniGlobe.Scene
{
    public class Billboard
    {
        public Billboard()
        {
            _lowerLeftTextureCoordinate = Vector2H.Zero;
            _upperRightTextureCoordinate = new Vector2H(1.0, 1.0);
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

        public Vector2H LowerLeftTextureCoordinate
        {
            get { return _lowerLeftTextureCoordinate; }
            set
            {
                if (_lowerLeftTextureCoordinate != value)
                {
                    _lowerLeftTextureCoordinate = value;
                    MakeDirty();
                }
            }
        }

        public Vector2H UpperRightTextureCoordinate
        {
            get { return _upperRightTextureCoordinate; }
            set
            {
                if (_upperRightTextureCoordinate != value)
                {
                    _upperRightTextureCoordinate = value;
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
        private Vector2H _lowerLeftTextureCoordinate;       // TODO:  Introduce texture rectangle?  Or use Vector4H?
        private Vector2H _upperRightTextureCoordinate;
    }
}
