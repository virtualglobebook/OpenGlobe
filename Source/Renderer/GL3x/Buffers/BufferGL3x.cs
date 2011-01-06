#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
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
            if (sizeInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException("sizeInBytes", "sizeInBytes must be greater than zero.");
            }

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

            //
            // We always unbind a buffer object after using it, so we don't 
            // leave it bound to a target, which would cause undefined 
            // behavior with transform feedback.
            //
            GL.BindVertexArray(0);
            Bind();
            GL.BufferData(_type, new IntPtr(sizeInBytes), new IntPtr(), _usageHint);
            UnBind();

            GC.AddMemoryPressure(sizeInBytes);
        }

        public void CopyFromSystemMemory<T>(
            T[] bufferInSystemMemory,
            int destinationOffsetInBytes,
            int lengthInBytes) where T : struct
        {
            if (destinationOffsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("destinationOffsetInBytes",
                    "destinationOffsetInBytes must be greater than or equal to zero.");
            }

            if (destinationOffsetInBytes + lengthInBytes > _sizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "destinationOffsetInBytes + lengthInBytes must be less than or equal to SizeInBytes.");
            }

            if (lengthInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be greater than or equal to zero.");
            }

            if (lengthInBytes > ArraySizeInBytes.Size(bufferInSystemMemory))
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be less than or equal to the size of bufferInSystemMemory in bytes.");
            }

            GL.BindVertexArray(0);
            Bind();
            GL.BufferSubData<T>(_type,
                new IntPtr(destinationOffsetInBytes),
                new IntPtr(lengthInBytes),
                bufferInSystemMemory);
            UnBind();
        }

        public T[] CopyToSystemMemory<T>(int offsetInBytes, int lengthInBytes) where T : struct
        {
            if (offsetInBytes < 0)
            {
                throw new ArgumentOutOfRangeException("offsetInBytes",
                    "offsetInBytes must be greater than or equal to zero.");
            }

            if (lengthInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException("lengthInBytes",
                    "lengthInBytes must be greater than zero.");
            }

            if (offsetInBytes + lengthInBytes > _sizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    "offsetInBytes + lengthInBytes must be less than or equal to SizeInBytes.");
            }

            T[] bufferInSystemMemory = new T[lengthInBytes / SizeInBytes<T>.Value];

            GL.BindVertexArray(0);
            Bind();
            GL.GetBufferSubData(_type, new IntPtr(offsetInBytes), new IntPtr(lengthInBytes), bufferInSystemMemory);
            UnBind();

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

        public int Name
        {
            get { return _name.Value; }
        }

        public void Bind()
        {
            GL.BindBuffer(_type, _name.Value);
        }

        public void UnBind()
        {
            GL.BindBuffer(_type, 0);
        }

        public void Dispose()
        {
            _name.Dispose();
            GC.RemoveMemoryPressure(_sizeInBytes);
        }

        private BufferNameGL3x _name;
        private readonly int _sizeInBytes;
        private readonly BufferTarget _type;
        private readonly BufferUsageHint _usageHint;
    }
}
