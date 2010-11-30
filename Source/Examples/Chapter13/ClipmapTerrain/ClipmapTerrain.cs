#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

using OpenGlobe.Renderer;
using OpenGlobe.Scene;

using OpenGlobe.Core;
using OpenGlobe.Terrain;
using OpenGlobe.Scene.Terrain;

namespace OpenGlobe.Examples
{
    sealed class ClipmapTerrain : IDisposable
    {
        public ClipmapTerrain()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 11:  Clipmap Terrain");

            SimpleTerrainSource terrainSource = new SimpleTerrainSource(@"..\..\..\..\..\..\Data\Terrain\ps_height_16k");
            //WorldWindTerrainSource terrainSource = new WorldWindTerrainSource();
            _clipmap = new PlaneClipmapTerrain(_window, _window.Context, terrainSource, 511);
            _clipmap.HeightExaggeration = 0.01f;

            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.90f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _sceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            _clearState = new ClearState();
            _clearState.Color = Color.LightSkyBlue;

            _sceneState.Camera.PerspectiveNearPlaneDistance = _clipmap.HeightExaggeration * 10.0;
            _sceneState.Camera.PerspectiveFarPlaneDistance = _clipmap.HeightExaggeration * 8000000.0;
            _sceneState.SunPosition = new Vector3D(200000, 300000, 200000);

            //double longitude = -119.5326056;
            //double latitude = 37.74451389;
            double longitude = 0.0;
            double latitude = 0.0;

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(longitude, latitude, _clipmap.HeightExaggeration * 2700.0);
            _camera.ZoomRateRangeAdjustment = 0.0;
            _camera.Azimuth = 0.0;
            _camera.Elevation = Trig.ToRadians(30.0);
            _camera.Range = 0.05;
            _camera.Dispose();
            _sceneState.Camera.Eye = new Vector3D(-119.5326056, 37.74451389, _clipmap.HeightExaggeration * 2700.0);
            _sceneState.Camera.Target = _sceneState.Camera.Eye + Vector3D.UnitZ;
            _cameraFly = new CameraFly(_sceneState.Camera, _window);
            _cameraFly.UpdateParametersFromCamera();
            _cameraFly.MovementRate = _clipmap.HeightExaggeration * 100000.0;

            _window.Keyboard.KeyDown += OnKeyDown;

            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.PreRenderFrame += OnPreRenderFrame;

            PersistentView.Execute(@"C:\Users\Kevin Ring\Documents\Book\svn\GeometryClipmapping\Figures\ClipmapNestedLevels.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"C:\Users\Kevin Ring\Documents\Book\svn\GeometryClipmapping\Figures\ClipmapNestedLevels.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Blue;
            UpdateHUD();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.S)
            {
                _sceneState.SunPosition = _sceneState.Camera.Eye;
            }
            else if (e.Key == KeyboardKey.W)
            {
                _clipmap.Wireframe = !_clipmap.Wireframe;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.B)
            {
                if (!_clipmap.BlendRegionsEnabled)
                {
                    _clipmap.BlendRegionsEnabled = true;
                    _clipmap.ShowBlendRegions = false;
                }
                else if (_clipmap.ShowBlendRegions)
                {
                    _clipmap.BlendRegionsEnabled = false;
                }
                else
                {
                    _clipmap.ShowBlendRegions = true;
                }
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.L)
            {
                _clipmap.LodUpdateEnabled = !_clipmap.LodUpdateEnabled;
                UpdateHUD();
            }
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);
            _clipmap.Render(context, _sceneState);

            if (_hud != null)
            {
                _hud.Render(context, _sceneState);
            }
        }

        private void OnPreRenderFrame()
        {
            Context context = _window.Context;
            _clipmap.PreRender(context, _sceneState);
        }

        private void UpdateHUD()
        {
            if (_hud == null)
                return;

            string text;

            text = "Blending: " + GetBlendingString() + " (B)\n";
            text += "Wireframe: " + (_clipmap.Wireframe ? "Enabled" : "Disabled") + " (W)\n";
            text += "LOD Update: " + (_clipmap.LodUpdateEnabled ? "Enabled" : "Disabled") + " (L)\n";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private string GetBlendingString()
        {
            if (!_clipmap.BlendRegionsEnabled)
                return "Disabled";
            else if (_clipmap.ShowBlendRegions)
                return "Enabled and Shown";
            else
                return "Enabled";
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_camera != null)
                _camera.Dispose();
            if (_cameraFly != null)
                _cameraFly.Dispose();
            _clipmap.Dispose();
            if (_hudFont != null)
                _hudFont.Dispose();
            if (_hud != null)
            {
                _hud.Texture.Dispose();
                _hud.Dispose();
            }
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (ClipmapTerrain example = new ClipmapTerrain())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private CameraFly _cameraFly;
        private readonly ClearState _clearState;
        private readonly PlaneClipmapTerrain _clipmap;
        private HeadsUpDisplay _hud;
        private Font _hudFont;
    }
}