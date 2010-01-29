#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using MiniGlobe.Renderer;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;

namespace MiniGlobe.Core
{
    /// <summary>
    /// System tests for MiniGlobe.Core.  System tests are higher level 
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

        private static void Render(Mesh mesh)
        {
            MiniGlobeWindow window = Device.CreateWindow(1, 1);
            FrameBuffer frameBuffer = TestUtility.CreateFrameBuffer(window.Context);

            ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader());
            VertexArray va = window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);

            window.Context.Bind(frameBuffer);
            window.Context.Bind(sp);
            window.Context.Bind(va);
            window.Context.Draw(PrimitiveType.Triangles);

            TestUtility.ValidateColor(frameBuffer.ColorAttachments[0], 255, 0, 0);

            va.Dispose();
            sp.Dispose();
            frameBuffer.Dispose();
            window.Dispose();
        }
    }
}
