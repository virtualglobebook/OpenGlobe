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
    sealed class TerrainShading : IDisposable
    {
        public TerrainShading()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 11:  Terrain Shading");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _window.Keyboard.KeyUp += OnKeyUp;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.DiffuseIntensity = 0.9f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _clearState = new ClearState();

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap("ps-e.lg.png"));
            _tile = new VertexDisplacementMapTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30;
            _tile.ColorMapTexture = Device.CreateTexture2D(new Bitmap("ps_texture_1k.png"), TextureFormat.RedGreenBlue8, false);
            _tile.ColorRampHeightTexture = Device.CreateTexture2D(new Bitmap("ColorRamp.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.ColorRampSlopeTexture = Device.CreateTexture2D(new Bitmap("ColorRampSlope.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.BlendRampTexture = Device.CreateTexture2D(new Bitmap("BlendRamp.jpg"), TextureFormat.Red8, false);
            _tile.GrassTexture = Device.CreateTexture2D(new Bitmap("Grass.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.StoneTexture = Device.CreateTexture2D(new Bitmap("Stone.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.BlendMaskTexture = Device.CreateTexture2D(new Bitmap("BlendMask.jpg"), TextureFormat.Red8, false);

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            _camera.MinimumRotateRate = 1.0;
            _camera.MaximumRotateRate = 1.0;
            _camera.RotateRateRangeAdjustment = 0.0;
            _camera.RotateFactor = 0.0;
            _sceneState.Camera.ZoomToTarget(tileRadius);

            PersistentView.Execute(@"E:\Manuscript\TerrainRendering\Figures\BlendMask.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\BlendMaskTerrain.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Black;
            UpdateHUD();
        }

        private static string TerrainNormalsAlgorithmToString(TerrainNormalsAlgorithm normals)
        {
            switch(normals)
            {
                case TerrainNormalsAlgorithm.None:
                    return "n/a";
                case TerrainNormalsAlgorithm.ForwardDifference:
                    return "Forward Samples";
                case TerrainNormalsAlgorithm.CentralDifference:
                    return "Central Samples";
                case TerrainNormalsAlgorithm.SobelFilter:
                    return "Sobel Filter";
            }

            return string.Empty;
        }

        private static string TerrainShadingAlgorithmToString(TerrainShadingAlgorithm shading)
        {
            switch (shading)
            {
                case TerrainShadingAlgorithm.ColorMap:
                    return "Color Map";
                case TerrainShadingAlgorithm.Solid:
                    return "Solid";
                case TerrainShadingAlgorithm.ByHeight:
                    return "By Height";
                case TerrainShadingAlgorithm.HeightContour:
                    return "Height Contour";
                case TerrainShadingAlgorithm.ColorRampByHeight:
                    return "Color Ramp By Height";
                case TerrainShadingAlgorithm.BlendRampByHeight:
                    return "Blend Ramp By Height";
                case TerrainShadingAlgorithm.BySlope:
                    return "By Slope";
                case TerrainShadingAlgorithm.SlopeContour:
                    return "Slope Contour";
                case TerrainShadingAlgorithm.ColorRampBySlope:
                    return "Color Ramp By Slope";
                case TerrainShadingAlgorithm.BlendRampBySlope:
                    return "Blend Ramp By Slope";
                case TerrainShadingAlgorithm.BlendMask:
                    return "Blend Mask";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Height Exaggeration: " + _tile.HeightExaggeration + " (up/down)\n";
            text += "Shading Algorithm: " + TerrainShadingAlgorithmToString(_tile.ShadingAlgorithm) + " ('s' + left/right)\n";
            text += "Normals Algorithm: " + TerrainNormalsAlgorithmToString(_tile.NormalsAlgorithm) + " ('a' + left/right)\n";
            text += "Terrain: " + (_tile.ShowTerrain ? "on" : "off") + " ('t')\n";
            text += "Silhouette: " + (_tile.ShowSilhouette ? "on" : "off") + " ('l')\n";
            text += "Wireframe: " + (_tile.ShowWireframe ? "on" : "off") + " ('w')\n";
            text += "Normals: " + (_tile.ShowNormals ? "on" : "off") + " ('n')\n";

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
            if (e.Key == KeyboardKey.S)
            {
                _sKeyDown = true;
            } 
            else if (e.Key == KeyboardKey.A)
            {
                _aKeyDown = true;
            }
            else if ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down))
            {
                _tile.HeightExaggeration = Math.Max(1, _tile.HeightExaggeration + ((e.Key == KeyboardKey.Up) ? 1 : -1));
            }
            else if (_sKeyDown && ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right)))
            {
                _tile.ShadingAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_tile.ShadingAlgorithm < TerrainShadingAlgorithm.ColorMap)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.BlendMask;
                }
                else if (_tile.ShadingAlgorithm > TerrainShadingAlgorithm.BlendMask)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.ColorMap;
                }
            }
            else if (_aKeyDown && ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right)))
            {
                _tile.NormalsAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_tile.NormalsAlgorithm < TerrainNormalsAlgorithm.None)
                {
                    _tile.NormalsAlgorithm = TerrainNormalsAlgorithm.SobelFilter;
                }
                else if (_tile.NormalsAlgorithm > TerrainNormalsAlgorithm.SobelFilter)
                {
                    _tile.NormalsAlgorithm = TerrainNormalsAlgorithm.None;
                }
            }
            else if (e.Key == KeyboardKey.T)
            {
                _tile.ShowTerrain = !_tile.ShowTerrain;
            }
            else if (e.Key == KeyboardKey.L)
            {
                _tile.ShowSilhouette = !_tile.ShowSilhouette;
            }
            else if (e.Key == KeyboardKey.W)
            {
                _tile.ShowWireframe = !_tile.ShowWireframe;
            }
            else if (e.Key == KeyboardKey.N)
            {
                _tile.ShowNormals = !_tile.ShowNormals;
            }

            UpdateHUD();
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.S)
            {
                _sKeyDown = false;
            }
            else if (e.Key == KeyboardKey.A)
            {
                _aKeyDown = false;
            }
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
            _tile.ColorMapTexture.Dispose();
            _tile.ColorRampHeightTexture.Dispose();
            _tile.ColorRampSlopeTexture.Dispose();
            _tile.BlendRampTexture.Dispose();
            _tile.GrassTexture.Dispose();
            _tile.StoneTexture.Dispose();
            _tile.BlendMaskTexture.Dispose();
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
            using (TerrainShading example = new TerrainShading())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly VertexDisplacementMapTerrainTile _tile;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private bool _sKeyDown;
        private bool _aKeyDown;
    }
}