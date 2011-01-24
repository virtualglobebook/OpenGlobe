#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Examples
{
    sealed class Triangle : IDisposable
    {
        public Triangle()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Triangle");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _clearState = new ClearState();

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 
                  }";

            string fs =
                @"#version 330
                 
                  out vec3 fragmentColor;
                  uniform vec3 u_color;

                  void main()
                  {
                      fragmentColor = u_color;
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);
            ((Uniform<Vector3F>)sp.Uniforms["u_color"]).Value = new Vector3F(1, 0, 0);

            ///////////////////////////////////////////////////////////////////
            
            Mesh mesh = new Mesh();

            VertexAttributeFloatVector3 positionsAttribute = new VertexAttributeFloatVector3("position", 3);
            mesh.Attributes.Add(positionsAttribute);

            IndicesUnsignedShort indices = new IndicesUnsignedShort(3);
            mesh.Indices = indices;

            IList<Vector3F> positions = positionsAttribute.Values;
            positions.Add(new Vector3F(0, 0, 0));
            positions.Add(new Vector3F(1, 0, 0));
            positions.Add(new Vector3F(0, 0, 1));

            indices.AddTriangle(new TriangleIndicesUnsignedShort(0, 1, 2));

            VertexArray va = _window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;

            _drawState = new DrawState(renderState, sp, va);

            ///////////////////////////////////////////////////////////////////
            
            _sceneState.Camera.ZoomToTarget(1);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);

            context.Draw(PrimitiveType.Triangles, _drawState, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.VertexArray.Dispose();
            _drawState.ShaderProgram.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (Triangle example = new Triangle())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly ClearState _clearState;
        private readonly DrawState _drawState;
    }
}