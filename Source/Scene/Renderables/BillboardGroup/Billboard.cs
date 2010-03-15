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
        public Vector3D Position 
        {
            get { return _position; }
            set
            {
                _position = value;
                MakeDirty();
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
    }
}
