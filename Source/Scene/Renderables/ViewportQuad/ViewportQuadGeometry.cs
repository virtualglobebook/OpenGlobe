#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    internal sealed class ViewportQuadGeometry : IDisposable
    {
        public ViewportQuadGeometry()
        {
            _positionBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4 * SizeInBytes<Vector2S>.Value);
            _textureCoordinatesBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, 4 * SizeInBytes<Vector2H>.Value);
        }

        internal void Update(Context context, ShaderProgram sp)
        {
            if (_va == null)
            {
                VertexBufferAttribute positionAttribute = new VertexBufferAttribute(
                    _positionBuffer, VertexAttributeComponentType.Float, 2);
                VertexBufferAttribute textureCoordinatesAttribute = new VertexBufferAttribute(
                    _textureCoordinatesBuffer, VertexAttributeComponentType.HalfFloat, 2);

                _va = context.CreateVertexArray();
                _va.VertexBuffers[sp.VertexAttributes["position"].Location] = positionAttribute;
                _va.VertexBuffers[sp.VertexAttributes["textureCoordinates"].Location] = textureCoordinatesAttribute;
            }

            if (_viewport != context.Viewport)
            {
                //
                // Bottom and top swapped:  MS -> OpenGL
                //
                float left = context.Viewport.Left;
                float bottom = context.Viewport.Top;
                float right = context.Viewport.Right;
                float top = context.Viewport.Bottom;

                Vector2S[] positions = new Vector2S[] 
                { 
                    new Vector2S(left, bottom), 
                    new Vector2S(right, bottom), 
                    new Vector2S(left, top), 
                    new Vector2S(right, top)
                };
                _positionBuffer.CopyFromSystemMemory(positions);

                Vector2H[] textureCoordinates = new Vector2H[] 
                { 
                    new Vector2H(0, 0), 
                    new Vector2H(1, 0), 
                    new Vector2H(0, 1), 
                    new Vector2H(1, 1)
                };
                _textureCoordinatesBuffer.CopyFromSystemMemory(textureCoordinates);

                _viewport = context.Viewport;
            }
        }

        internal VertexArray VertexArray
        {
            get { return _va; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _positionBuffer.Dispose();
            _textureCoordinatesBuffer.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }
        }

        #endregion

        private Rectangle _viewport;
        private readonly VertexBuffer _positionBuffer;
        private readonly VertexBuffer _textureCoordinatesBuffer;
        private VertexArray _va;
    }
}