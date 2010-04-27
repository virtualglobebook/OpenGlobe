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
using MiniGlobe.Renderer;
using OpenTK.Graphics.OpenGL;

namespace MiniGlobe.Renderer.GL3x
{
    internal class FrameBufferGL3x : FrameBuffer
    {
        public FrameBufferGL3x()
        {
            GL.GenFramebuffers(1, out _handle);
            _colorAttachments = new ColorAttachmentsGL3x();
        }

        ~FrameBufferGL3x()
        {
            FinalizerThreadContextGL3x.MakeCurrent();
            Dispose(false);
        }

        internal void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
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
                int drawBuffersIndex = 0;

                for (int i = 0; i < colorAttachments.Length; ++i)
                {
                    if (colorAttachments[i].Dirty)
                    {
                        Attach(FramebufferAttachment.ColorAttachment0 + i, colorAttachments[i].Texture);
                        colorAttachments[i].Dirty = false;
                    }

                    if (colorAttachments[i].Texture != null)
                    {
                        drawBuffers[drawBuffersIndex++] = DrawBuffersEnum.ColorAttachment0 + i;
                    }
                }
                GL.DrawBuffers(drawBuffersIndex, drawBuffers);

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
                Debug.Assert(value == null || value.Description.DepthRenderable);

                _depthAttachment = value;
                _dirtyFlags |= DirtyFlags.DepthAttachment;
            }
        }

        public override Texture2D DepthStencilAttachment
        {
            get { return _depthStencilAttachment; }

            set
            {
                Debug.Assert(value == null || value.Description.DepthStencilRenderable);

                _depthStencilAttachment = value;
                _dirtyFlags |= DirtyFlags.DepthStencilAttachment;
            }
        }

        #endregion

        internal static void Attach(FramebufferAttachment attachPoint, Texture2D texture)
        {
            if (texture != null)
            {
                // TODO:  Mipmap level
                Texture2DGL3x textureGL = texture as Texture2DGL3x;
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, textureGL.Handle, 0);
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachPoint, 0, 0);
            }
        }

        #region Disposable Members

        protected override void Dispose(bool disposing)
        {
            // Always delete the frame buffer, even in the finalizer.
            GL.DeleteFramebuffers(1, ref _handle);
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

        private int _handle;
        private ColorAttachmentsGL3x _colorAttachments;
        private Texture2D _depthAttachment;
        private Texture2D _depthStencilAttachment;
        private DirtyFlags _dirtyFlags;
    }
}
