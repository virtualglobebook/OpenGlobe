#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;
using NUnit.Framework;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class TransformFeedbackTests
    {
        [Test]
        [Explicit("Don't know why transform feedback doesn't work.  Maybe our custom OpenTK.dll is the problem.")]
        public void Points()
        {
            string vs = ShaderSources.PassThroughVertexShader();
            string fs = ShaderSources.PassThroughFragmentShader();

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(vs, fs, 
                new string[] { "gl_Position" },
                TransformFeedbackAttributeLayout.Interleaved))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            using (VertexBuffer feedback = Device.CreateVertexBuffer(BufferHint.StaticDraw, SizeInBytes<Vector4F>.Value))
            {
                Context context = window.Context;

                context.BeginTransformFeedback(TransformFeedbackPrimitiveType.Points,
                    new Buffer[] { feedback });
                context.Framebuffer = framebuffer;
                context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                context.EndBeginTransform();

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);

                float[] position = feedback.CopyToSystemMemory<float>();
                Assert.AreEqual(0.0f, position[0]);
                Assert.AreEqual(0.0f, position[1]);
                Assert.AreEqual(0.0f, position[2]);
                Assert.AreEqual(1.0f, position[3]);
            }
        }
    }
}
