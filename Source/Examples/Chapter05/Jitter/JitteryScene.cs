#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class JitteryScene : IDisposable, IRenderable
    {
        public JitteryScene(Context context, double xTranslation)
        {
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  layout(location = og_colorVertexLocation) in vec3 color;
                  out vec3 fsColor;
                  uniform mat4 og_modelViewPerspectiveMatrix;

                  void main()                     
                  {
                        gl_PointSize = 8.0;
                        gl_Position = og_modelViewPerspectiveMatrix * position; 
                        fsColor = color;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 fsColor;
                  out vec3 fragmentColor;

                  void main()
                  {
                      fragmentColor = fsColor;
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", 7);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", 7);
            mesh.Attributes.Add(positionsAttribute);
            mesh.Attributes.Add(colorAttribute);

            double delta = 1;

            IList<Vector3D> positions = positionsAttribute.Values;
            positions.Add(new Vector3D(xTranslation, delta + 0, 0));            // Red triangle
            positions.Add(new Vector3D(xTranslation, delta + 1000000, 0));
            positions.Add(new Vector3D(xTranslation, delta + 0, 1000000));
            positions.Add(new Vector3D(xTranslation, -delta - 0, 0));           // Green triangle
            positions.Add(new Vector3D(xTranslation, -delta - 0, 1000000));
            positions.Add(new Vector3D(xTranslation, -delta - 1000000, 0));
            positions.Add(new Vector3D(xTranslation, 0, 0));                    // Blue point

            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Red);
            colorAttribute.AddColor(Color.Green);
            colorAttribute.AddColor(Color.Green);
            colorAttribute.AddColor(Color.Green);
            colorAttribute.AddColor(Color.Blue);

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawState = new DrawState(renderState, _sp, _va);
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            context.Draw(PrimitiveType.Triangles, 0, 6, _drawState, sceneState);
            context.Draw(PrimitiveType.Points, 6, 1, _drawState, sceneState);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _va.Dispose();
            _sp.Dispose();
        }

        #endregion

        private readonly VertexArray _va;
        private readonly ShaderProgram _sp;
        private readonly DrawState _drawState;
    }
}