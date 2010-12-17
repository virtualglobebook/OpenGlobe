#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.IO;
using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    [TestFixture]
    public class CameraTests
    {
        [Test]
        public void SaveLoadView()
        {
            string filename = "view.xml";

            Camera camera = new Camera();
            camera.Eye = Vector3D.Zero;
            camera.Target = -Vector3D.UnitZ;
            camera.Up = Vector3D.UnitY;

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
