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
    public class PlaneClipmapTerrainTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            GraphicsWindow window = Device.CreateWindow(800, 600, "Clipmap Terrain Test");

            WorldWindTerrainSource worldWind = new WorldWindTerrainSource();
            EsriRestImagery imagery = new EsriRestImagery();
            PlaneClipmapTerrain clipmap = new PlaneClipmapTerrain(window.Context, worldWind, 255, imagery);

            SceneState sceneState = new SceneState();
            sceneState.DiffuseIntensity = 0.90f;
            sceneState.SpecularIntensity = 0.05f;
            sceneState.AmbientIntensity = 0.05f;

            ClearState clearState = new ClearState();
            clearState.Color = Color.LightSkyBlue;

            Ellipsoid ellipsoid = Ellipsoid.Wgs84;
            sceneState.Camera.PerspectiveNearPlaneDistance = 0.001;
            sceneState.Camera.PerspectiveFarPlaneDistance = 200.0;
            sceneState.SunPosition = new Vector3D(200000, 300000, 200000);

            CameraLookAtPoint camera = new CameraLookAtPoint(sceneState.Camera, window, Ellipsoid.UnitSphere);
            camera.CenterPoint = new Vector3D(-119.43, 37.64, 0.00001 * 3000.0);
            camera.ZoomRateRangeAdjustment = 0.0;
            camera.Azimuth = 0.0;
            camera.Elevation = Trig.ToRadians(30.0);
            camera.Range = 0.1;

            RayCastedGlobe globe = new RayCastedGlobe(window.Context);
            globe.Shape = ellipsoid;
            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            globe.Texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            ClearState clearDepth = new ClearState();
            clearDepth.Buffers = ClearBuffers.DepthBuffer;

            //camera.Dispose();
            //CameraFly fly = new CameraFly(sceneState.Camera, window);
            //fly.UpdateParametersFromCamera();
            //fly.MovementRate = 2000.0;

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
                //globe.Render(context, sceneState);

                //context.Clear(clearDepth);

                clipmap.Render(context, sceneState);
            };
            window.PreRenderFrame += delegate()
            {
                Context context = window.Context;
                clipmap.PreRender(context, sceneState);
            };

            clipmap.PreRender(window.Context, sceneState);

            PersistentView.Execute(@"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\ClipmapWithCracks.xml", window, sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(window, sceneState);
            snap.ColorFilename = @"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\ClipmapWithCracks.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            window.Run(30.0);
        }
    }
}
