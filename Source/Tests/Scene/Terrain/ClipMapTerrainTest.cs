using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenGlobe.Scene.Terrain;
using OpenGlobe.Renderer;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Scene;
using OpenGlobe.Core;
using System.Drawing;

namespace OpenGlobe.Tests.Scene.Terrain
{
    [TestFixture]
    public class ClipMapTerrainTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            GraphicsWindow window = Device.CreateWindow(800, 600, "ClipMap Terrain Test");

            WorldWindTerrainSource worldWind = new WorldWindTerrainSource();
            ClipMapTerrain clipMap = new ClipMapTerrain(window.Context, worldWind, 255);

            SceneState sceneState = new SceneState();
            sceneState.DiffuseIntensity = 0.9f;
            sceneState.SpecularIntensity = 0.05f;
            sceneState.AmbientIntensity = 0.05f;

            ClearState clearState = new ClearState();

            sceneState.Camera.PerspectiveFarPlaneDistance = 100.0;
            sceneState.Camera.PerspectiveNearPlaneDistance = 0.001;
            sceneState.SunPosition = new Vector3D(200000, 300000, 200000);

            CameraLookAtPoint camera = new CameraLookAtPoint(sceneState.Camera, window, Ellipsoid.UnitSphere);
            camera.ZoomRateRangeAdjustment = 0.0;
            camera.CenterPoint = new Vector3D(-119.5326056, 37.74451389, 0.00001 * 2700.0);
            camera.Azimuth = 0.0;
            camera.Elevation = Trig.ToRadians(30.0);
            camera.Range = 10.0;

            window.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == KeyboardKey.S)
                {
                    sceneState.SunPosition = sceneState.Camera.Eye;
                }
            };

            window.Resize += delegate()
            {
                window.Context.Viewport = new Rectangle(0, 0, window.Width, window.Height);
                sceneState.Camera.AspectRatio = window.Width / (double)window.Height;
            };
            window.RenderFrame += delegate()
            {
                Context context = window.Context;
                context.Clear(clearState);
                clipMap.Render(context, sceneState);
            };

            window.Run(30.0);
        }
    }
}
