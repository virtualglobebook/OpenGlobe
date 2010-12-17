#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using NUnit.Framework;
using OpenGlobe.Renderer;

namespace OpenGlobe.Core
{
    /// <summary>
    /// System tests for OpenGlobe.Core.  System tests are higher level 
    /// than unit tests; instead of validating a single class, system tests 
    /// use multiple classes to validate a more complicated task.
    /// </summary>
    [TestFixture]
    public class SystemTests
    {
        [Test]
        public void RenderSubdivisionSphere()
        {
            Render(SubdivisionSphereTessellatorSimple.Compute(1));
        }

        [Test]
        public void RenderCubeMapSphere()
        {
            Render(CubeMapEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 3, CubeMapEllipsoidVertexAttributes.Position));
        }

        [Test]
        public void RenderGeographicGridSphere()
        {
            Render(GeographicGridEllipsoidTessellator.Compute(Ellipsoid.UnitSphere, 9, 4, GeographicGridEllipsoidVertexAttributes.Position));
        }

        [Test]
        public void RenderBox()
        {
            Render(BoxTessellator.Compute(new Vector3D(1, 1, 1)));
        }

        private static void Render(Mesh mesh)
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (VertexArray va = window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw))
            {
                window.Context.Framebuffer = framebuffer;
                window.Context.Clear(new ClearState());
                window.Context.Draw(PrimitiveType.Triangles, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }
    }
}
