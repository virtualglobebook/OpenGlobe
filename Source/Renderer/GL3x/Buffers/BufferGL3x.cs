#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class BufferGL3x : IDisposable
    {
        public BufferGL3x(
            BufferTarget type, 
            BufferHint usageHint, 
            int sizeInBytes)
        {
            Debug.Assert(sizeInBytes > 0);

            _name = new BufferNameGL3x();

            _sizeInBytes = sizeInBytes;
            _type = type;
            _usageHint = TypeConverterGL3x.To(usageHint);

            //
            // Allocating here with GL.BufferData, then writing with GL.BufferSubData
            // in CopyFromSystemMemory() should not have any serious overhead:
            //
            //   http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=267373#Post267373
            //
            // Alternately, we can delay GL.BufferData until the first
            // CopyFromSystemMemory() call.
            //
            GL.BindVertexArray(0);
            Bind();
            GL.BufferData(_type, new IntPtr(sizeInBytes), new IntPtr(), _usageHint);
        }

        public void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes) where T : struct
        {
            Debug.Assert(destinationOffsetInBytes >= 0);
            Debug.Assert(destinationOffsetInBytes + lengthInBytes <= _sizeInBytes);

            Debug.Assert(lengthInBytes >= 0);
            Debug.Assert(lengthInBytes <= bufferInSystemMemory.Length * SizeInBytes<T>.Value);

            GL.BindVertexArray(0);
            Bind();
            GL.BufferSubData<T>(_type,
                new IntPtr(destinationOffsetInBytes),
                new IntPtr(lengthInBytes),
                bufferInSystemMemory);
        }

        public T[] CopyToSystemMemory<T>(int offsetInBytes, int lengthInBytes) where T : struct
        {
            Debug.Assert(offsetInBytes >= 0);
            Debug.Assert(lengthInBytes > 0);
            Debug.Assert(offsetInBytes + lengthInBytes <= _sizeInBytes);

            T[] bufferInSystemMemory = new T[lengthInBytes / SizeInBytes<T>.Value];

            GL.BindVertexArray(0);
            Bind();
            GL.GetBufferSubData(_type, new IntPtr(offsetInBytes), new IntPtr(lengthInBytes), bufferInSystemMemory);
            return bufferInSystemMemory;
        }

        public int SizeInBytes
        {
            get { return _sizeInBytes; }
        }

        public BufferHint UsageHint
        {
            get { return TypeConverterGL3x.To(_usageHint); }
        }

        public BufferNameGL3x Handle
        {
            get { return _name; }
        }

        public void Bind()
        {
            GL.BindBuffer(_type, _name.Value);
        }

        public void Dispose()
        {
            _name.Dispose();
        }

        private BufferNameGL3x _name;
        private readonly int _sizeInBytes;
        private readonly BufferTarget _type;
        private readonly BufferUsageHint _usageHint;
    }
}
