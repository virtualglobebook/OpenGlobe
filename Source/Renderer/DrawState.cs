#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
