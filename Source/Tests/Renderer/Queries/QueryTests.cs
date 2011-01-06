#region License
//
// (C) Copyright 2011 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Diagnostics;
using OpenGlobe.Core;
using NUnit.Framework;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void QueryTypeTest()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                Assert.AreEqual(QueryType.NumberOfPrimitivesGenerated, query.QueryType);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeingQuery()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                window.Context.BeginQuery(null);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BeingQuery2()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            using (Query query2 = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                window.Context.BeginQuery(query);
                try
                {
                    window.Context.BeginQuery(query2);
                }
                finally
                {
                    window.Context.EndQuery(query);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EndQuery()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            {
                window.Context.EndQuery(null);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EndQuery2()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                window.Context.BeginQuery(query);
                window.Context.EndQuery(query);
                window.Context.EndQuery(query);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateQuery()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                window.Context.BeginQuery(query);
                try
                {
                    Query query2 = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated);
                }
                finally
                {
                    window.Context.EndQuery(query);
                }
            }
        }

        [Test]
        public void IsResultAvailable()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                Assert.IsFalse(query.IsResultAvailable());
            }
        }

        [Test]
        public void IsResultAvailable2()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfPrimitivesGenerated))
            {
                window.Context.BeginQuery(query);
                window.Context.EndQuery(query);

                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (!query.IsResultAvailable())
                {
                    if (watch.ElapsedMilliseconds > 10000)
                    {
                        Debug.Fail("Query result is not available in a reasonable amount of time.");
                    }
                }
            }
        }

        [Test]
        public void Result()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfSamplesPassed))
            {
                window.Context.BeginQuery(query);
                window.Context.EndQuery(query);
                Assert.AreEqual(0, query.Result());
            }
        }

        [Test]
        public void Result2()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            using (Query query = window.Context.CreateQuery(QueryType.NumberOfSamplesPassed))
            {
                Context context = window.Context;
                context.Framebuffer = framebuffer;
                context.BeginQuery(query);
                context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                context.EndQuery(query);

                Assert.AreEqual(1, query.Result());
            }
        }
    }
}
