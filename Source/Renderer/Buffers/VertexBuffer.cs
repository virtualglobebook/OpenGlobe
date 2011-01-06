#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Runtime.InteropServices;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    public abstract class VertexBuffer : Buffer
    {
        public virtual void CopyFromSystemMemory<T>(T[] bufferInSystemMemory) where T : struct
        {
            CopyFromSystemMemory<T>(bufferInSystemMemory, 0);
        }

        public virtual void CopyFromSystemMemory<T>(T[] bufferInSystemMemory, int destinationOffsetInBytes) where T : struct
        {
            CopyFromSystemMemory<T>(bufferInSystemMemory, destinationOffsetInBytes, ArraySizeInBytes.Size(bufferInSystemMemory));
        }

        public abstract void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory, 
            int destinationOffsetInBytes,
            int lengthInBytes) where T : struct;

        public virtual T[] CopyToSystemMemory<T>() where T : struct
        {
            return CopyToSystemMemory<T>(0, SizeInBytes);
        }

        public abstract T[] CopyToSystemMemory<T>(int offsetInBytes, int sizeInBytes) where T : struct;

        public abstract int SizeInBytes { get; }
        public abstract BufferHint UsageHint { get; }
    }
}
