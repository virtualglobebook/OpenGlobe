#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class ShaderCacheTests
    {
        [Test]
        public void FindOrAdd()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                ShaderCache cache = new ShaderCache();

                ShaderProgram sp = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughFragmentShader());
                ShaderProgram sp2 = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughFragmentShader());

                Assert.AreEqual(sp, sp2);

                cache.Release("PassThrough");
                cache.Release("PassThrough");
            }
        }

        [Test]
        public void FindOrAdd2()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                ShaderCache cache = new ShaderCache();

                ShaderProgram sp = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughFragmentShader());
                ShaderProgram sp2 = cache.FindOrAdd("PassThrough2",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughFragmentShader());

                Assert.AreNotEqual(sp, sp2);

                cache.Release("PassThrough");
                cache.Release("PassThrough2");
            }
        }

        [Test]
        public void FindOrAddGS()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                ShaderCache cache = new ShaderCache();

                ShaderProgram sp = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughGeometryShader(),
                    ShaderSources.PassThroughFragmentShader());
                ShaderProgram sp2 = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughGeometryShader(),
                    ShaderSources.PassThroughFragmentShader());

                Assert.AreEqual(sp, sp2);

                cache.Release("PassThrough");
                cache.Release("PassThrough");
            }
        }

        [Test]
        public void Find()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                ShaderCache cache = new ShaderCache();

                ShaderProgram sp = cache.FindOrAdd("PassThrough",
                    ShaderSources.PassThroughVertexShader(),
                    ShaderSources.PassThroughFragmentShader());
                ShaderProgram sp2 = cache.Find("PassThrough");

                Assert.AreEqual(sp, sp2);

                cache.Release("PassThrough");
                cache.Release("PassThrough");
            }
        }

        [Test]
        public void FindNothing()
        {
            Assert.IsNull(new ShaderCache().Find("PassThrough"));
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ReleaseNothing()
        {
            new ShaderCache().Release("PassThrough");
        }
    }
}
