#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class DrawState
    {
        public DrawState()
        {
            RenderState = new RenderState();
        }

        public DrawState(RenderState renderState, ShaderProgram shaderProgram, VertexArray vertexArray)
        {
            RenderState = renderState;
            ShaderProgram = shaderProgram;
            VertexArray = vertexArray;
        }

        public RenderState RenderState { get; set; }
        public ShaderProgram ShaderProgram { get; set; }
        public VertexArray VertexArray { get; set; }
    }
}
