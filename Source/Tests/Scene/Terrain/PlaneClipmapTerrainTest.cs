#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System.Drawing;
using NUnit.Framework;
using OpenGlobe.Renderer;
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    [TestFixture]
    public class PlaneClipmapTerrainTest
    {
        [Test]
        [Explicit]
        public void Test()
        {
            GraphicsWindow window = Device.CreateWindow(640, 480, "Clipmap Terrain Test");

            //WorldWindTerrainSource worldWind = new WorldWindTerrainSource();
            SimpleTerrainSource terrainSource = new SimpleTerrainSource(@"..\..\..\..\Data\Terrain\ps_height_16k");
            EsriRestImagery imagery = new EsriRestImagery();
            PlaneClipmapTerrain clipmap = new PlaneClipmapTerrain(window.Context, terrainSource, 255, imagery);
            clipmap.HeightExaggeration = 0.00001f;

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
            //camera.CenterPoint = new Vector3D(-119.533283, 37.74523, 0.00001 * 2700.0);
            camera.CenterPoint = new Vector3D(0.0, 0.0, 0.0000001 * 2700.0);
            //camera.CenterPoint = new Vector3D(-75.5967666, 40.0388333, 0.00001 * 100.0);
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
            //sceneState.Camera.Eye = new Vector3D(-119.533283, 37.74523, 0.00001 * 2700.0);
            //sceneState.Camera.Target = sceneState.Camera.Eye + Vector3D.UnitZ;
            //CameraFly fly = new CameraFly(sceneState.Camera, window);
            //fly.UpdateParametersFromCamera();
            //fly.MovementRate = 0.01;

            window.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                if (e.Key == KeyboardKey.S)
                {
                    sceneState.SunPosition = sceneState.Camera.Eye;
                }
                else if (e.Key == KeyboardKey.W)
                {
                    clipmap.Wireframe = !clipmap.Wireframe;
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

            PersistentView.Execute(@"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\HalfDome.xml", window, sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(window, sceneState);
            snap.ColorFilename = @"C:\Users\Kevin Ring\Documents\Book\svn\TerrainLevelOfDetail\Figures\HalfDome.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            window.Run(30.0);
        }
    }
}
