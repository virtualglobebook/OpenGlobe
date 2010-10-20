#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    internal class MeshVertexBufferAttributes : VertexBufferAttributes
    {
        public MeshVertexBufferAttributes()
        {
            _attributes = new VertexBufferAttribute[Device.MaximumNumberOfVertexAttributes];
        }

        #region vertexBufferAttributes Members

        public override VertexBufferAttribute this[int index]
        {
            get { return _attributes[index]; }

            set
            {
                if ((_attributes[index] != null) && (value == null))
                {
                    --_count;
                }
                else if ((_attributes[index] == null) && (value != null))
                {
                    ++_count;
                }

                _attributes[index] = value;
            }
        }

        public override int Count
        {
            get { return _count; }
        }

        public override int MaximumCount
        {
            get { return _attributes.Length; }
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (VertexBufferAttribute vb in _attributes)
            {
                if (_attributes != null)
                {
                    yield return _attributes;
                }
            }
        }

        #endregion

        private VertexBufferAttribute[] _attributes;
        private int _count;
    }
}
