#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class UniformTests
    {
        [Test]
        public void Matrix42()
        {
            string fs =
                @"#version 330

                  uniform mat4x2 exampleMat42;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat42[0].x, exampleMat42[2].y, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix42<float> m42 = new Matrix42<float>(
                        1.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 1.0f, 0.0f);
                Uniform<Matrix42<float>> exampleMat42 = (Uniform<Matrix42<float>>)sp.Uniforms["exampleMat42"];
                Assert.AreEqual("exampleMat42", exampleMat42.Name);
                Assert.AreEqual(UniformType.FloatMatrix42, exampleMat42.Datatype);
                Assert.AreEqual(new Matrix42<float>(), exampleMat42.Value);
                exampleMat42.Value = m42;
                Assert.AreEqual(m42, exampleMat42.Value);

                window.Context.FrameBuffer = frameBuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix24()
        {
            string fs =
                @"#version 330

                  uniform mat2x4 exampleMat24;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat24[0].y, exampleMat24[0].w, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix24<float> m24 = new Matrix24<float>(
                        0.0f, 0.0f, 
                        1.0f, 0.0f,
                        0.0f, 0.0f, 
                        1.0f, 0.0f);
                Uniform<Matrix24<float>> exampleMat24 = (Uniform<Matrix24<float>>)sp.Uniforms["exampleMat24"];
                Assert.AreEqual("exampleMat24", exampleMat24.Name);
                Assert.AreEqual(UniformType.FloatMatrix24, exampleMat24.Datatype);
                Assert.AreEqual(new Matrix24<float>(), exampleMat24.Value);
                exampleMat24.Value = m24;
                Assert.AreEqual(m24, exampleMat24.Value);

                window.Context.FrameBuffer = frameBuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

    }
}
