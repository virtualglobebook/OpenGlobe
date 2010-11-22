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
                if (_attributes[index].VertexBufferAttribute != value)
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
            foreach (VertexBufferAttributeGL3x attribute in _attributes)
            {
                if (attribute.VertexBufferAttribute != null)
                {
                    yield return attribute.VertexBufferAttribute;
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
                    VertexBufferAttribute attribute = _attributes[i].VertexBufferAttribute;

                    if (_attributes[i].Dirty)
                    {
                        if (attribute != null)
                        {
                            Attach(i);
                        }
                        else
                        {
                            Detach(i);
                        }

                        _attributes[i].Dirty = false;
                    }

                    if (attribute != null)
                    {
                        maximumArrayIndex = Math.Max(NumberOfVertices(attribute) - 1, maximumArrayIndex);
                    }
                }

                _dirty = false;
                _maximumArrayIndex = maximumArrayIndex;
            }
        }

        private void Attach(int index)
        {
            VertexBufferAttribute attribute = _attributes[index].VertexBufferAttribute;

            Debug.Assert(attribute.NumberOfComponents >= 1);
            Debug.Assert(attribute.NumberOfComponents <= 4);
#if DEBUG
            if (attribute.Normalize)
            {
                Debug.Assert(
                    (attribute.ComponentDatatype == ComponentDatatype.Byte) ||
                    (attribute.ComponentDatatype == ComponentDatatype.UnsignedByte) ||
                    (attribute.ComponentDatatype == ComponentDatatype.Short) ||
                    (attribute.ComponentDatatype == ComponentDatatype.UnsignedShort) ||
                    (attribute.ComponentDatatype == ComponentDatatype.Int) ||
                    (attribute.ComponentDatatype == ComponentDatatype.UnsignedInt));
            }
#endif

            GL.EnableVertexAttribArray(index);

            VertexBufferGL3x bufferObjectGL = (VertexBufferGL3x)attribute.VertexBuffer;
            
            bufferObjectGL.Bind();
            GL.VertexAttribPointer(index,
                attribute.NumberOfComponents,
                TypeConverterGL3x.To(attribute.ComponentDatatype),
                attribute.Normalize,
                attribute.StrideInBytes,
                attribute.OffsetInBytes);
        }

        private static void Detach(int index)
        {
            GL.DisableVertexAttribArray(index);
        }

        internal int MaximumArrayIndex
        {
            get 
            {
                Debug.Assert(!_dirty);
                return _maximumArrayIndex; 
            }
        }

        private static int NumberOfVertices(VertexBufferAttribute attribute)
        {
            return attribute.VertexBuffer.SizeInBytes / attribute.StrideInBytes;
        }

        private VertexBufferAttributeGL3x[] _attributes;
        private int _count;
        private int _maximumArrayIndex;
        private bool _dirty;
    }
}
