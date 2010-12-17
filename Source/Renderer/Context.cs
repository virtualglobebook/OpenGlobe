#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [Flags]
    public enum ClearBuffers
    {
        ColorBuffer = 1,
        DepthBuffer = 2,
        StencilBuffer = 4,
        ColorAndDepthBuffer = ColorBuffer | DepthBuffer, 
        All = ColorBuffer | DepthBuffer | StencilBuffer
    }

    public abstract class Context
    {
        public abstract void MakeCurrent();

        public virtual VertexArray CreateVertexArray(Mesh mesh, ShaderVertexAttributeCollection shaderAttributes, BufferHint usageHint)
        {
            return CreateVertexArray(Device.CreateMeshBuffers(mesh, shaderAttributes, usageHint));
        }

        public virtual VertexArray CreateVertexArray(MeshBuffers meshBuffers)
        {
            VertexArray va = CreateVertexArray();

            va.DisposeBuffers = true;
            va.IndexBuffer = meshBuffers.IndexBuffer;
            for (int i = 0; i < meshBuffers.Attributes.MaximumCount; ++i)
            {
                va.Attributes[i] = meshBuffers.Attributes[i];
            }

            return va;
        }

        public abstract VertexArray CreateVertexArray();
        public abstract Framebuffer CreateFramebuffer();

        public abstract TextureUnits TextureUnits { get; }
        public abstract Rectangle Viewport { get; set; }
        public abstract Framebuffer Framebuffer { get; set; }

        public abstract void Clear(ClearState clearState);
        public abstract void Draw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState);
        public abstract void Draw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState);
    }
}
