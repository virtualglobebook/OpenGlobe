#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class FrameBufferGL3x : FrameBuffer
    {
        public FrameBufferGL3x()
        {
            _handle = new FrameBufferHandleGL3x();
            _colorAttachments = new ColorAttachmentsGL3x();
        }

        internal void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle.Value);
        }

        internal static void UnBind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        internal void Clean()
        {
            if (_colorAttachments.Dirty)
            {
                ColorAttachmentGL3x[] colorAttachments = _colorAttachments.Attachments;
                
                DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[colorAttachments.Length];

                for (int i = 0; i < colorAttachments.Length; ++i)
                {
                    if (colorAttachments[i].Dirty)
                    {
                        Attach(FramebufferAttachment.ColorAttachment0 + i, colorAttachments[i].Texture);
                        colorAttachments[i].Dirty = false;
                    }

                    if (colorAttachments[i].Texture != null)
                    {
                        drawBuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
                    }
                }
                GL.DrawBuffers(drawBuffers.Length, drawBuffers);

                _colorAttachments.Dirty = false;
            }

            if (_dirtyFlags != DirtyFlags.None)
            {
                if ((_dirtyFlags & DirtyFlags.DepthAttachment) == DirtyFlags.DepthAttachment)
                {
                    Attach(FramebufferAttachment.DepthAttachment, _depthAttachment);
                }

                if ((_dirtyFlags & DirtyFlags.DepthStencilAttachment) == DirtyFlags.DepthStencilAttachment)
                {
                    Attach(FramebufferAttachment.DepthStencilAttachment, _depthStencilAttachment);
                }

                _dirtyFlags = DirtyFlags.None;
            }
        }

        #region FrameBuffer Members

        public override ColorAttachments ColorAttachments
        {
            get { return _colorAttachments; }
        }

        public override Texture2D DepthAttachment
        {
            get { return _depthAttachment; }

            set
            {
                if (_depthAttachment != value)
                {
                    if ((value != null) && (!value.Description.DepthRenderable))
                    {
                        throw new ArgumentException("Texture must be depth renderable but the Description.DepthRenderable property is false.");
                    }

                    _depthAttachment = value;
                    _dirtyFlags |= DirtyFlags.DepthAttachment;
                }
            }
        }

        public override Texture2D DepthStencilAttachment
        {
            get { return _depthStencilAttachment; }

            set
            {
                if (_depthStencilAttachment != value)
                {
                    if ((value != null) && (!value.Description.DepthStencilRenderable))
                    {
                        throw new ArgumentException("Texture must be depth/stencil renderable but the Description.DepthStencilRenderable property is false.");
                    }

                    _depthStencilAttachment = value;
                    _dirtyFlags |= DirtyFlags.DepthStencilAttachment;
                }
            }
        }

        #endregion

        internal static void Attach(FramebufferAttachment attachPoint, Texture2D texture)
        {
            if (texture != null)
            {
                // TODO:  Mipmap level
                Texture2DGL3x textureGL = texture as Texture2DGL3x;
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, textureGL.Handle.Value, 0);
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, 0, 0);
            }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handle.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            DepthAttachment = 1,
            DepthStencilAttachment = 2
        }

        private FrameBufferHandleGL3x _handle;
        private ColorAttachmentsGL3x _colorAttachments;
        private Texture2D _depthAttachment;
        private Texture2D _depthStencilAttachment;
        private DirtyFlags _dirtyFlags;
    }
}
