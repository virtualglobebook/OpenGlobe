#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;

namespace MiniGlobe.Renderer
{
    public enum VertexAttributeComponentType
    {
        Byte,
        UnsignedByte,
        Short,
        UnsignedShort,
        Int,
        UnsignedInt,
        Float,
        Double,
        HalfFloat,
    }

    public abstract class VertexArray : Disposable
    {
        public virtual AttachedVertexBuffers VertexBuffers
        {
            get { return null; }
        }

        public virtual IndexBuffer IndexBuffer
        {
            get { return null; }
            set { }
        }
    }
}
