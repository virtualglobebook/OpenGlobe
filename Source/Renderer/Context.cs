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

        public void Clear(ClearState clearState)
        {
            if (PerformanceCountersEnabled)
            {
                ++_numberOfClearCalls;
            }

            DoClear(clearState);
        }

        public void Draw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState)
        {
            if (PerformanceCountersEnabled)
            {
                IncreasePerformanceCounters(primitiveType, count, drawState);
            }

            DoDraw(primitiveType, offset, count, drawState, sceneState);
        }

        public void Draw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState)
        {
            if (PerformanceCountersEnabled)
            {
                int count;
                IndexBuffer indexBuffer = drawState.VertexArray.IndexBuffer;

                if (indexBuffer != null)
                {
                    count = indexBuffer.Count;
                }
                else
                {
                    // TODO:  Can't rely on zero.
                    VertexBufferAttribute attribute = drawState.VertexArray.Attributes[0];
                    count = attribute.VertexBuffer.SizeInBytes / attribute.StrideInBytes;
                }

                IncreasePerformanceCounters(primitiveType, count, drawState);
            }

            DoDraw(primitiveType, drawState, sceneState);
        }

        protected abstract void DoClear(ClearState clearState);
        protected abstract void DoDraw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState);
        protected abstract void DoDraw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState);

        public bool PerformanceCountersEnabled { get; set; }
        public long NumberOfPointsRendered { get { return _numberOfPoints; } }
        public long NumberOfLinesRendered { get { return _numberOfLines; } }
        public long NumberOfTrianglesRendered { get { return _numberOfTriangles; } }
        public long NumberOfPrimitivesRendered { get { return _numberOfPrimitives; } }
        public long NumberOfDrawCalls { get { return _numberOfDrawCalls; } }
        public long NumberOfClearCalls { get { return _numberOfClearCalls; } }

        public void ResetPerformanceCounters()
        {
            _numberOfPoints = 0;
            _numberOfLines = 0;
            _numberOfTriangles = 0;
            _numberOfPrimitives = 0;
            _numberOfDrawCalls = 0;
            _numberOfClearCalls = 0;
        }

        private void IncreasePerformanceCounters(PrimitiveType primitiveType, int count, DrawState drawState)
        {
            if (drawState.RenderState.PrimitiveRestart.Enabled)
            {
                throw new NotImplementedException("Performance counters are not implemented for when PrimitiveRestart is disabled.  Comment out this exception, implement the counters, or set PerformanceCountersEnabled to false.");
            }

            switch (primitiveType)
            {
                case PrimitiveType.Points:
                    _numberOfPoints += count;
                    _numberOfPrimitives += count;
                    break;

                case PrimitiveType.Lines:
                    _numberOfLines += (count / 2);
                    _numberOfPrimitives += (count / 2);
                    break;
                case PrimitiveType.LinesAdjacency:
                    _numberOfLines += ((count * 3) / 4);
                    _numberOfPrimitives += ((count * 3) / 4);
                    break;
                case PrimitiveType.LineLoop:
                    _numberOfLines += count;
                    _numberOfPrimitives += count;
                    break;
                case PrimitiveType.LineStrip:
                case PrimitiveType.LineStripAdjacency:
                    _numberOfLines += (count - 1);
                    _numberOfPrimitives += (count - 1);
                    break;

                case PrimitiveType.Triangles:
                    _numberOfTriangles += (count / 3);
                    _numberOfPrimitives += (count / 3);
                    break;
                case PrimitiveType.TriangleStrip:
                case PrimitiveType.TriangleFan:
                    _numberOfTriangles += (count - 2);
                    _numberOfPrimitives += (count - 2);
                    break;
                case PrimitiveType.TrianglesAdjacency:
                case PrimitiveType.TriangleStripAdjacency:
                    throw new NotImplementedException("Performance counters for TrianglesAdjacency and TriangleStripAdjacency primitive types are not implemented.  Comment out this exception, implement them, or set PerformanceCountersEnabled to false.");
            }

            ++_numberOfDrawCalls;
        }

        private long _numberOfPoints;
        private long _numberOfLines;
        private long _numberOfTriangles;
        private long _numberOfPrimitives;
        private long _numberOfDrawCalls;
        private long _numberOfClearCalls;
    }
}
