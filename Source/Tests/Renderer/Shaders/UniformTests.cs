#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
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
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
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

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
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
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
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

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix32()
        {
            string fs =
                @"#version 330

                  uniform mat3x2 exampleMat32;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat32[1].x, exampleMat32[2].x, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix32<float> m32 = new Matrix32<float>(
                        0.0f, 1.0f, 1.0f,
                        0.0f, 0.0f, 0.0f);
                Uniform<Matrix32<float>> exampleMat32 = (Uniform<Matrix32<float>>)sp.Uniforms["exampleMat32"];
                Assert.AreEqual("exampleMat32", exampleMat32.Name);
                Assert.AreEqual(UniformType.FloatMatrix32, exampleMat32.Datatype);
                Assert.AreEqual(new Matrix32<float>(), exampleMat32.Value);
                exampleMat32.Value = m32;
                Assert.AreEqual(m32, exampleMat32.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix23()
        {
            string fs =
                @"#version 330

                  uniform mat2x3 exampleMat23;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat23[0].z, exampleMat23[1].x, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix23<float> m23 = new Matrix23<float>(
                        0.0f, 1.0f,
                        0.0f, 0.0f,
                        1.0f, 0.0f);
                Uniform<Matrix23<float>> exampleMat23 = (Uniform<Matrix23<float>>)sp.Uniforms["exampleMat23"];
                Assert.AreEqual("exampleMat23", exampleMat23.Name);
                Assert.AreEqual(UniformType.FloatMatrix23, exampleMat23.Datatype);
                Assert.AreEqual(new Matrix23<float>(), exampleMat23.Value);
                exampleMat23.Value = m23;
                Assert.AreEqual(m23, exampleMat23.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix43()
        {
            string fs =
                @"#version 330

                  uniform mat4x3 exampleMat43;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat43[1].y, exampleMat43[3].x, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix43<float> m42 = new Matrix43<float>(
                        0.0f, 0.0f, 0.0f, 1.0f,
                        0.0f, 1.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 0.0f, 0.0f);
                Uniform<Matrix43<float>> exampleMat43 = (Uniform<Matrix43<float>>)sp.Uniforms["exampleMat43"];
                Assert.AreEqual("exampleMat43", exampleMat43.Name);
                Assert.AreEqual(UniformType.FloatMatrix43, exampleMat43.Datatype);
                Assert.AreEqual(new Matrix43<float>(), exampleMat43.Value);
                exampleMat43.Value = m42;
                Assert.AreEqual(m42, exampleMat43.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix34()
        {
            string fs =
                @"#version 330

                  uniform mat3x4 exampleMat34;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat34[1].x, exampleMat34[2].z, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix34<float> m34 = new Matrix34<float>(
                        0.0f, 1.0f, 0.0f,
                        0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 1.0f,
                        0.0f, 0.0f, 0.0f);
                Uniform<Matrix34<float>> exampleMat34 = (Uniform<Matrix34<float>>)sp.Uniforms["exampleMat34"];
                Assert.AreEqual("exampleMat34", exampleMat34.Name);
                Assert.AreEqual(UniformType.FloatMatrix34, exampleMat34.Datatype);
                Assert.AreEqual(new Matrix34<float>(), exampleMat34.Value);
                exampleMat34.Value = m34;
                Assert.AreEqual(m34, exampleMat34.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix22()
        {
            string fs =
                @"#version 330

                  uniform mat2 exampleMat2;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat2[1].x, exampleMat2[1].y, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix2<float> m2 = new Matrix2<float>(
                        0.0f, 1.0f,
                        0.0f, 1.0f);
                Uniform<Matrix2<float>> exampleMat2 = (Uniform<Matrix2<float>>)sp.Uniforms["exampleMat2"];
                Assert.AreEqual("exampleMat2", exampleMat2.Name);
                Assert.AreEqual(UniformType.FloatMatrix22, exampleMat2.Datatype);
                Assert.AreEqual(new Matrix2<float>(), exampleMat2.Value);
                exampleMat2.Value = m2;
                Assert.AreEqual(m2, exampleMat2.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix4()
        {
            string fs =
                @"#version 330

                  uniform mat4 exampleMat4;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat4[0].z, exampleMat4[3].y, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix4F m4 = new Matrix4F(
                        0.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 0.0f, 1.0f,
                        1.0f, 0.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 0.0f, 0.0f);
                Uniform<Matrix4F> exampleMat4 = (Uniform<Matrix4F>)sp.Uniforms["exampleMat4"];
                Assert.AreEqual("exampleMat4", exampleMat4.Name);
                Assert.AreEqual(UniformType.FloatMatrix44, exampleMat4.Datatype);
                Assert.AreEqual(new Matrix4F(), exampleMat4.Value);
                exampleMat4.Value = m4;
                Assert.AreEqual(m4, exampleMat4.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        [Test]
        public void Matrix3()
        {
            string fs =
                @"#version 330

                  uniform mat3 exampleMat3;
                  out vec3 FragColor;

                  void main()
                  {
                      FragColor = vec3(exampleMat3[0].y, exampleMat3[2].x, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Matrix3F m3 = new Matrix3F(
                        0.0f, 0.0f, 1.0f,
                        1.0f, 0.0f, 0.0f,
                        0.0f, 0.0f, 0.0f);
                Uniform<Matrix3F> exampleMat3 = (Uniform<Matrix3F>)sp.Uniforms["exampleMat3"];
                Assert.AreEqual("exampleMat3", exampleMat3.Name);
                Assert.AreEqual(UniformType.FloatMatrix33, exampleMat3.Datatype);
                Assert.AreEqual(new Matrix3F(), exampleMat3.Value);
                exampleMat3.Value = m3;
                Assert.AreEqual(m3, exampleMat3.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }
    }
}
