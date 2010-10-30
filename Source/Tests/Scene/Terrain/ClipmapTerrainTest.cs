#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using NUnit.Framework;
using OpenGlobe.Renderer;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Core;
using System.Drawing;

namespace OpenGlobe.Scene.Terrain
{
    [TestFixture]
    public class ClipmapTerrainTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            GraphicsWindow window = Device.CreateWindow(800, 600, "Clipmap Terrain Test");

            WorldWindTerrainSource worldWind = new WorldWindTerrainSource();
            EsriRestImagery imagery = new EsriRestImagery();
            ClipmapTerrain clipmap = new ClipmapTerrain(window.Context, worldWind, 255, imagery);

            SceneState sceneState = new SceneState();
            sceneState.DiffuseIntensity = 0.45f;
            sceneState.SpecularIntensity = 0.05f;
            sceneState.AmbientIntensity = 0.5f;

            ClearState clearState = new ClearState();

            Ellipsoid ellipsoid = Ellipsoid.Wgs84;
            sceneState.Camera.PerspectiveNearPlaneDistance = 0.0001 * ellipsoid.MaximumRadius;
            sceneState.Camera.PerspectiveFarPlaneDistance = 20.0 * ellipsoid.MaximumRadius;
            sceneState.SunPosition = new Vector3D(200000, 300000, 200000);

            CameraLookAtPoint camera = new CameraLookAtPoint(sceneState.Camera, window, ellipsoid);
            camera.ViewPoint(ellipsoid, new Geodetic3D(Trig.ToRadians(-119.5326056), Trig.ToRadians(37.74451389), 2700.0));
            camera.ZoomRateRangeAdjustment = 0.0;
            //camera.CenterPoint = ellipsoid.ToVector3D();
            camera.Azimuth = 0.0;
            camera.Elevation = Trig.ToRadians(30.0);
            camera.Range = 1000.0;

            RayCastedGlobe globe = new RayCastedGlobe(window.Context);
            globe.Shape = ellipsoid;
            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            globe.Texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            ClearState clearDepth = new ClearState();
            clearDepth.Buffers = ClearBuffers.DepthBuffer;

            //camera.Dispose();
            //CameraFly fly = new CameraFly(sceneState.Camera, window);
            //fly.UpdateParametersFromCamera();
            //fly.MovementRate = 0.01;

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
                globe.Render(context, sceneState);

                context.Clear(clearDepth);

                clipmap.Render(context, sceneState);
            };
            window.PreRenderFrame += delegate()
            {
                Context context = window.Context;
                //clipmap.PreRender(context, sceneState);
            };

            clipmap.PreRender(window.Context, sceneState);

            window.Run(30.0);
        }
    }
}
