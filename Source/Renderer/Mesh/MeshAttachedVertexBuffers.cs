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
    internal class MeshAttachedVertexBuffers : AttachedVertexBuffers
    {
        public MeshAttachedVertexBuffers()
        {
            // TODO:  Don't call GL here
            float numberOfAttributes;
            GL.GetFloat(GetPName.MaxVertexAttribs, out numberOfAttributes);
            _attachedBuffers = new AttachedVertexBuffer[Convert.ToInt32(numberOfAttributes)];
        }

        #region AttachedVertexBuffers Members

        public override AttachedVertexBuffer this[int index]
        {
            get { return _attachedBuffers[index]; }

            set
            {
                if ((_attachedBuffers[index] != null) && (value == null))
                {
                    --_count;
                }
                else if ((_attachedBuffers[index] == null) && (value != null))
                {
                    ++_count;
                }

                _attachedBuffers[index] = value;
            }
        }

        public override int Count
        {
            get { return _count; }
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (AttachedVertexBuffer vb in _attachedBuffers)
            {
                if (_attachedBuffers != null)
                {
                    yield return _attachedBuffers;
                }
            }
        }

        #endregion

        private AttachedVertexBuffer[] _attachedBuffers;
        private int _count;
    }
}
