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
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core;

namespace MiniGlobe.Renderer
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
            VertexArray va = CreateVertexArray();

            if (mesh.Indices != null)
            {
                if (mesh.Indices.DataType == IndicesType.Byte)
                {
                    IList<byte> meshIndices = (mesh.Indices as IndicesByte).Values;

                    byte[] indices = new byte[meshIndices.Count];
                    meshIndices.CopyTo(indices, 0);

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(byte));
                    indexBuffer.CopyFromSystemMemory(indices);
                    va.IndexBuffer = indexBuffer;
                }
                else if (mesh.Indices.DataType == IndicesType.Int16)
                {
                    IList<short> meshIndices = (mesh.Indices as IndicesInt16).Values;

                    ushort[] indices = new ushort[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = (ushort)meshIndices[j];
                    }

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(ushort));
                    indexBuffer.CopyFromSystemMemory(indices);
                    va.IndexBuffer = indexBuffer;
                }
                else
                {
                    Debug.Assert(mesh.Indices.DataType == IndicesType.Int32);

                    IList<int> meshIndices = (mesh.Indices as IndicesInt32).Values;

                    uint[] indices = new uint[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = (uint)meshIndices[j];
                    }

                    IndexBuffer indexBuffer = Device.CreateIndexBuffer(usageHint, indices.Length * sizeof(uint));
                    indexBuffer.CopyFromSystemMemory(indices);
                    va.IndexBuffer = indexBuffer;
                }
            }

            // TODO:  Not tested exhaustively
            foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
            {
                if (!mesh.Attributes.Contains(shaderAttribute.Name))
                {
                    throw new ArgumentException("Shader requires vertex attribute \"" + shaderAttribute.Name + "\", which is not present in mesh.");
                }

                VertexAttribute attribute = mesh.Attributes[shaderAttribute.Name];
                
                if (attribute.DataType == VertexAttributeType.Double)
                {
                    IList<double> values = (attribute as VertexAttribute<double>).Values;

                    float[] valuesArray = new float[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = (float)values[i];
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * sizeof(float));
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 1);
                }
                else if (attribute.DataType == VertexAttributeType.DoubleVector2)
                {
                    IList<Vector2D> values = (attribute as VertexAttribute<Vector2D>).Values;

                    Vector2S[] valuesArray = new Vector2S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector2S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector2S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 2);
                }
                else if (attribute.DataType == VertexAttributeType.DoubleVector3)
                {
                    IList<Vector3D> values = (attribute as VertexAttribute<Vector3D>).Values;

                    Vector3S[] valuesArray = new Vector3S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector3S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector3S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 3);
                }
                else if (attribute.DataType == VertexAttributeType.DoubleVector4)
                {
                    IList<Vector4D> values = (attribute as VertexAttribute<Vector4D>).Values;

                    Vector4S[] valuesArray = new Vector4S[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector4S();
                    }

                    VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeInBytes<Vector4S>.Value);
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 4);
                }
                else if (attribute.DataType == VertexAttributeType.HalfFloat)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Half>).Values, SizeInBytes<Half>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.HalfFloat, 1);
                }
                else if (attribute.DataType == VertexAttributeType.HalfFloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector2H>).Values, SizeInBytes<Vector2H>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.HalfFloat, 2);
                }
                else if (attribute.DataType == VertexAttributeType.HalfFloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector3H>).Values, SizeInBytes<Vector3H>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.HalfFloat, 3);
                }
                else if (attribute.DataType == VertexAttributeType.HalfFloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector4H>).Values, SizeInBytes<Vector4H>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.HalfFloat, 4);
                }
                else if (attribute.DataType == VertexAttributeType.Float)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<float>).Values, sizeof(float), usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 1);
                }
                else if (attribute.DataType == VertexAttributeType.FloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector2S>).Values, SizeInBytes<Vector2S>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 2);
                }
                else if (attribute.DataType == VertexAttributeType.FloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector3S>).Values, SizeInBytes<Vector3S>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 3);
                }
                else if (attribute.DataType == VertexAttributeType.FloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<Vector4S>).Values, SizeInBytes<Vector4S>.Value, usageHint);

                    va.VertexBuffers[shaderAttribute.Location] =
                        new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.Float, 4);
                }
                else if (attribute.DataType == VertexAttributeType.UnsignedByte)
                {
                    if (attribute is VertexAttributeRGBA)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<byte>).Values, sizeof(byte), usageHint);

                        va.VertexBuffers[shaderAttribute.Location] =
                            new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.UnsignedByte, 4, true);
                    }
                    else
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer((attribute as VertexAttribute<byte>).Values, sizeof(byte), usageHint);

                        va.VertexBuffers[shaderAttribute.Location] =
                            new AttachedVertexBuffer(vertexBuffer, VertexAttributeComponentType.UnsignedByte, 1);
                    }
                }
                else
                {
                    Debug.Fail("attribute.DataType");
                }
            }

            return va;
        }

        private static VertexBuffer CreateVertexBuffer<T>(IList<T> values, int SizeOfT, BufferHint usageHint) where T : struct
        {
            T[] valuesArray = new T[values.Count];
            values.CopyTo(valuesArray, 0);

            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(usageHint, valuesArray.Length * SizeOfT);
            vertexBuffer.CopyFromSystemMemory(valuesArray);
            return vertexBuffer;
        }

        public abstract VertexArray CreateVertexArray();
        public abstract FrameBuffer CreateFrameBuffer();
        public abstract TextureUnits TextureUnits { get; }

        public abstract void Clear(ClearState clearState);
        public abstract Rectangle Viewport { get; set; }

        public abstract void Bind(FrameBuffer frameBuffer);

        public abstract void Draw(PrimitiveType primitiveType, int offset, int count, DrawState drawState, SceneState sceneState);
        public abstract void Draw(PrimitiveType primitiveType, DrawState drawState, SceneState sceneState);

        public abstract void Finish();
    }
}
