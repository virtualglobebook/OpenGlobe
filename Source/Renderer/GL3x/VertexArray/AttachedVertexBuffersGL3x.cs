#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
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
    internal struct AttachedVertexBufferGL3x
    {
        public AttachedVertexBuffer AttachedVertexBuffer { get; set; }
        public bool Dirty { get; set; }
    }

    internal class AttachedVertexBuffersGL3x : AttachedVertexBuffers
    {
        public AttachedVertexBuffersGL3x()
	    {
            float numberOfAttributes;
            GL.GetFloat(GetPName.MaxVertexAttribs, out numberOfAttributes);
            _attachedBuffers = new AttachedVertexBufferGL3x[Convert.ToInt32(numberOfAttributes)];
        }

        #region AttachedVertexBuffers Members

        public override AttachedVertexBuffer this[int index]
        {
            get { return _attachedBuffers[index].AttachedVertexBuffer; }

            set
            {
                if ((_attachedBuffers[index].AttachedVertexBuffer != null) && (value == null))
                {
                    --_count;
                }
                else if ((_attachedBuffers[index].AttachedVertexBuffer == null) && (value != null))
                {
                    ++_count;
                }

                _attachedBuffers[index].AttachedVertexBuffer = value;
                _attachedBuffers[index].Dirty = true;
                _dirty = true;
            }
        }

        public override int Count
        {
            get { return _count; }
        }

        public override int MaximumCount 
        {
            get { return _attachedBuffers.Length; }
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (AttachedVertexBufferGL3x vb in _attachedBuffers)
            {
                if (vb.AttachedVertexBuffer != null)
                {
                    yield return vb.AttachedVertexBuffer;
                }
            }
        }

        #endregion

        internal void Clean()
        {
            if (_dirty)
            {
                int maximumArrayIndex = 0;

                for (int i = 0; i < _attachedBuffers.Length; ++i)
                {
                    AttachedVertexBuffer vb = _attachedBuffers[i].AttachedVertexBuffer;

                    if (_attachedBuffers[i].Dirty)
                    {
                        if (vb != null)
                        {
                            Attach(i);
                        }
                        else
                        {
                            Detach(i);
                        }

                        _attachedBuffers[i].Dirty = false;
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
            AttachedVertexBuffer vb = _attachedBuffers[index].AttachedVertexBuffer;

            Debug.Assert(vb.NumberOfComponents >= 1);
            Debug.Assert(vb.NumberOfComponents <= 4);
#if DEBUG
            if (vb.Normalize)
            {
                Debug.Assert(
                    (vb.ComponentType == VertexAttributeComponentType.Byte) ||
                    (vb.ComponentType == VertexAttributeComponentType.UnsignedByte) ||
                    (vb.ComponentType == VertexAttributeComponentType.Short) ||
                    (vb.ComponentType == VertexAttributeComponentType.UnsignedShort) ||
                    (vb.ComponentType == VertexAttributeComponentType.Int) ||
                    (vb.ComponentType == VertexAttributeComponentType.UnsignedInt));
            }
#endif

            GL.EnableVertexAttribArray(index);

            VertexBufferGL3x bufferObjectGL = vb.VertexBuffer as VertexBufferGL3x;
            VertexAttribPointerType vertexDataType = TypeConverterGL3x.To(vb.ComponentType);
            int stride = vb.NumberOfComponents * SizesGL3x.SizeOf(vb.ComponentType);

            bufferObjectGL.Bind();
            GL.VertexAttribPointer(index,
                vb.NumberOfComponents,
                vertexDataType,
                vb.Normalize,
                stride, 
                0);
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

        private static int NumberOfVertices(AttachedVertexBuffer vb)
        {
            return vb.VertexBuffer.SizeInBytes / (vb.NumberOfComponents * SizesGL3x.SizeOf(vb.ComponentType));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (AttachedVertexBufferGL3x attachedBuffer in _attachedBuffers)
                {
                    if (attachedBuffer.AttachedVertexBuffer != null)
                    {
                        attachedBuffer.AttachedVertexBuffer.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }

        private AttachedVertexBufferGL3x[] _attachedBuffers;
        private int _count;
        private int _maximumArrayIndex;
        private bool _dirty;
    }
}
