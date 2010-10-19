using System;
using System.Collections.Generic;
using System.Linq;
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
            ClearState clearState = new ClearState();

            sceneState.Camera.PerspectiveFarPlaneDistance = 40.0;
            sceneState.Camera.PerspectiveNearPlaneDistance = 0.001;

            CameraLookAtPoint camera = new CameraLookAtPoint(sceneState.Camera, window, Ellipsoid.UnitSphere);
            camera.ZoomRateRangeAdjustment = 0.0;
            camera.CenterPoint = new Vector3D(0.0, 0.0, 0.0);
            camera.Azimuth = 0.0;
            camera.Elevation = 0.0;
            camera.Range = 30.0;

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
