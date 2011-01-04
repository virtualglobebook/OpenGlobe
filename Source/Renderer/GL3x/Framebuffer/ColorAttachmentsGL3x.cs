#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections;
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
            _colorAttachments = new ColorAttachmentGL3x[Device.MaximumColorAttachments];
        }

        #region ColorAttachments Members

        public override Texture2D this[int index]
        {
            get { return _colorAttachments[index].Texture; }

            set
            {
                if ((value != null) && (!value.Description.ColorRenderable))
                {
                    throw new ArgumentException("Texture must be color renderable but the Description.ColorRenderable property is false.");
                }

                if (_colorAttachments[index].Texture != value)
                {
                    if ((_colorAttachments[index].Texture != null) && (value == null))
                    {
                        --_count;
                    }
                    else if ((_colorAttachments[index].Texture == null) && (value != null))
                    {
                        ++_count;
                    }

                    _colorAttachments[index].Texture = value;
                    _colorAttachments[index].Dirty = true;
                    Dirty = true;
                }
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
