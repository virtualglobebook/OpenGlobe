#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal struct ColorAttachmentGL3x
    {
        public Texture2D Texture { get; set; }
        public bool Dirty { get; set; }
    }

    internal class ColorAttachmentsGL3x : ColorAttachments
    {
        public ColorAttachmentsGL3x()
        {
            int maximumColorAttachments;
            GL.GetInteger(GetPName.MaxColorAttachments, out maximumColorAttachments);
            _colorAttachments = new ColorAttachmentGL3x[maximumColorAttachments];
        }

        #region ColorAttachments Members

        public override Texture2D this[int index]
        {
            get { return _colorAttachments[index].Texture; }

            set
            {
                if ((_colorAttachments[index].Texture != null) && (value == null))
                {
                    --_count;
                }
                else if ((_colorAttachments[index].Texture == null) && (value != null))
                {
                    ++_count;
                }

                Debug.Assert(value == null || value.Description.ColorRenderable);

                _colorAttachments[index].Texture = value;
                _colorAttachments[index].Dirty = true;
                Dirty = true;
            }
        }

        public override int Count
        {
            get { return _count; }
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (ColorAttachmentGL3x attachment in _colorAttachments)
            {
                if (attachment.Texture != null)
                {
                    yield return attachment.Texture;
                }
            }
        }

        #endregion

        internal bool Dirty { get; set; }

        internal ColorAttachmentGL3x[] Attachments
        {
            get { return _colorAttachments; }
        }

        private ColorAttachmentGL3x[] _colorAttachments;
        private int _count;
    }
}
