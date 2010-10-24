#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
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

namespace OpenGlobe.Renderer.GL3x
{
    internal struct VertexBufferAttributeGL3x
    {
        public VertexBufferAttribute VertexBufferAttribute { get; set; }
        public bool Dirty { get; set; }
    }

    internal class VertexBufferAttributesGL3x : VertexBufferAttributes
    {
        public VertexBufferAttributesGL3x()
	    {
            _attributes = new VertexBufferAttributeGL3x[Device.MaximumNumberOfVertexAttributes];
        }

        #region VertexBufferAttributes Members

        public override VertexBufferAttribute this[int index]
        {
            get { return _attributes[index].VertexBufferAttribute; }

            set
            {
                if ((_attributes[index].VertexBufferAttribute != null) && (value == null))
                {
                    --_count;
                }
                else if ((_attributes[index].VertexBufferAttribute == null) && (value != null))
                {
                    ++_count;
                }

                _attributes[index].VertexBufferAttribute = value;
                _attributes[index].Dirty = true;
                _dirty = true;
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
            foreach (VertexBufferAttributeGL3x vb in _attributes)
            {
                if (vb.VertexBufferAttribute != null)
                {
                    yield return vb.VertexBufferAttribute;
                }
            }
        }

        #endregion

        internal void Clean()
        {
            if (_dirty)
            {
                int maximumArrayIndex = 0;

                for (int i = 0; i < _attributes.Length; ++i)
                {
                    VertexBufferAttribute vb = _attributes[i].VertexBufferAttribute;

                    if (_attributes[i].Dirty)
                    {
                        if (vb != null)
                        {
                            Attach(i);
                        }
                        else
                        {
                            Detach(i);
                        }

                        _attributes[i].Dirty = false;
                    }

                    if (vb != null)
                    {
                        maximumArrayIndex = Math.Max(NumberOfVertices(vb) - 1, maximumArrayIndex);
                    }
                }

                _dirty = false;
                _maximumArrayIndex = maximumArrayIndex;
            }
        }

        private void Attach(int index)
        {
            VertexBufferAttribute vb = _attributes[index].VertexBufferAttribute;

            Debug.Assert(vb.NumberOfComponents >= 1);
            Debug.Assert(vb.NumberOfComponents <= 4);
#if DEBUG
            if (vb.Normalize)
            {
                Debug.Assert(
                    (vb.ComponentDatatype == ComponentDatatype.Byte) ||
                    (vb.ComponentDatatype == ComponentDatatype.UnsignedByte) ||
                    (vb.ComponentDatatype == ComponentDatatype.Short) ||
                    (vb.ComponentDatatype == ComponentDatatype.UnsignedShort) ||
                    (vb.ComponentDatatype == ComponentDatatype.Int) ||
                    (vb.ComponentDatatype == ComponentDatatype.UnsignedInt));
            }
#endif

            GL.EnableVertexAttribArray(index);

            VertexBufferGL3x bufferObjectGL = vb.VertexBuffer as VertexBufferGL3x;
            
            bufferObjectGL.Bind();
            GL.VertexAttribPointer(index,
                vb.NumberOfComponents,
                TypeConverterGL3x.To(vb.ComponentDatatype),
                vb.Normalize,
                vb.StrideInBytes,
                vb.OffsetInBytes);
        }

        private static void Detach(int index)
        {
            GL.DisableVertexAttribArray(index);
            VertexBufferGL3x.UnBind();
            GL.VertexAttribPointer(index, 0, VertexAttribPointerType.Float, false, 0, 0);
        }

        internal int MaximumArrayIndex
        {
            get 
            {
                Debug.Assert(!_dirty);
                return _maximumArrayIndex; 
            }
        }

        private static int NumberOfVertices(VertexBufferAttribute vb)
        {
            return vb.VertexBuffer.SizeInBytes / vb.StrideInBytes;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (VertexBufferAttributeGL3x attribute in _attributes)
                {
                    if (attribute.VertexBufferAttribute != null)
                    {
                        attribute.VertexBufferAttribute.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        private VertexBufferAttributeGL3x[] _attributes;
        private int _count;
        private int _maximumArrayIndex;
        private bool _dirty;
    }
}
