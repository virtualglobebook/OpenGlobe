#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class Framebuffer : Disposable
    {
        public abstract ColorAttachments ColorAttachments { get; }
        public abstract Texture2D DepthAttachment { get; set; }
        public abstract Texture2D DepthStencilAttachment { get; set; }
    }
}
