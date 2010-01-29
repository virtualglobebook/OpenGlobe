#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.IO;
using NUnit.Framework;
using OpenTK;

namespace MiniGlobe.Renderer
{
    [TestFixture]
    public class CameraTests
    {
        [Test]
        public void SaveLoadView()
        {
            string filename = "view.xml";

            Camera camera = new Camera();
            camera.Eye = Vector3d.Zero;
            camera.Target = -Vector3d.UnitZ;
            camera.Up = Vector3d.UnitY;

            try
            {
                camera.SaveView(filename);

                Camera camera2 = new Camera();
                camera2.LoadView(filename);

                Assert.IsTrue(camera.Eye.Equals(camera2.Eye));
                Assert.IsTrue(camera.Target.Equals(camera2.Target));
                Assert.IsTrue(camera.Up.Equals(camera2.Up));
            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}
