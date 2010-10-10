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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenGlobe.Core.Geometry;
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
        public virtual VertexArray CreateVertexArray(Mesh mesh, ShaderVertexAttributeCollection shaderAttributes, BufferHint usageHint)
        {
            return CreateVertexArray(Device.CreateMeshBuffers(mesh, shaderAttributes, usageHint));
        }

        public virtual VertexArray CreateVertexArray(MeshBuffers meshBuffers)
        {
            VertexArray va = CreateVertexArray();
            
            va.IndexBuffer = meshBuffers.IndexBuffer;
            for (int i = 0; i < meshBuffers.VertexBuffers.MaximumCount; ++i)
            {
                va.VertexBuffers[i] = meshBuffers.VertexBuffers[i];
            }

            return va;
        }

        public abstract VertexArray CreateVertexArray();
        public abstract FrameBuffer CreateFrameBuffer();

        public abstract TextureUnits TextureUnits { get; }
        public abstract Rectangle Viewport { get; set; }
        public abstract FrameBuffer FrameBuffer { get; set; }

        public abstract void Clear(ClearState clearState);
        public abstract void Draw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState);
        public abstract void Draw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState);
    }
}
