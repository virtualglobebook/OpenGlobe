#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class TerrainRayCasting : IDisposable
    {
        public TerrainRayCasting()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 11:  Terrain Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _clearState = new ClearState();

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.png"));
            _tile = new RayCastedTerrainTile(terrainTile);
            _tile.HeightExaggeration = 30;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            _camera.MinimumRotateRate = 1.0;
            _camera.MaximumRotateRate = 1.0;
            _camera.RotateRateRangeAdjustment = 0.0;
            _camera.RotateFactor = 0.0;
            _sceneState.Camera.ZoomToTarget(tileRadius);

            PersistentView.Execute(@"E:\Manuscript\TerrainBasics\Figures\GPURayCastingNearestNeighbor.xml", _window, _sceneState.Camera);
            
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainBasics\Figures\GPURayCastingNearestNeighbor.png";
            snap.WidthInInches = 4;
            snap.DotsPerInch = 600;

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Black;
            UpdateHUD();
        }

        private static string TerrainShadingAlgorithmToString(RayCastedTerrainShadingAlgorithm shading)
        {
            switch (shading)
            {
                case RayCastedTerrainShadingAlgorithm.ByHeight:
                    return "By Height";
                case RayCastedTerrainShadingAlgorithm.ByRaySteps:
                    return "Number of Ray Steps";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Shading Algorithm: " + TerrainShadingAlgorithmToString(_tile.ShadingAlgorithm) + " (left/right)\n";
            text += "Wireframe: " + (_tile.ShowWireframe ? "on" : "off") + " ('w')\n";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
                _tile.ShadingAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_tile.ShadingAlgorithm < RayCastedTerrainShadingAlgorithm.ByHeight)
                {
                    _tile.ShadingAlgorithm = RayCastedTerrainShadingAlgorithm.ByRaySteps;
                }
                else if (_tile.ShadingAlgorithm > RayCastedTerrainShadingAlgorithm.ByRaySteps)
                {
                    _tile.ShadingAlgorithm = RayCastedTerrainShadingAlgorithm.ByHeight;
                }
            }
            if (e.Key == KeyboardKey.W)
            {
                _tile.ShowWireframe = !_tile.ShowWireframe;
            }

            UpdateHUD();
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);

            _tile.Render(context, _sceneState);
            _hud.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _tile.Dispose();
            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (TerrainRayCasting example = new TerrainRayCasting())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly RayCastedTerrainTile _tile;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;
    }
}