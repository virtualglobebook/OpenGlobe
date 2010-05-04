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

using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class VertexDisplacementMap : IDisposable
    {
        public VertexDisplacementMap()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Vertex Shader Displacement Mapping");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _window.Keyboard.KeyUp += OnKeyUp;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.DiffuseIntensity = 0.9f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap("ps-e.lg.jpg"));
            _tile = new VertexDisplacementMapTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30;
            _tile.ColorRampTexture = Device.CreateTexture2D(new Bitmap("ColorRamp.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.BlendRampTexture = Device.CreateTexture2D(new Bitmap("BlendRamp.jpg"), TextureFormat.Red8, false);
            _tile.GrassTexture = Device.CreateTexture2D(new Bitmap("Grass.jpg"), TextureFormat.RedGreenBlue8, false);
            _tile.StoneTexture = Device.CreateTexture2D(new Bitmap("Stone.jpg"), TextureFormat.RedGreenBlue8, false);
            
            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Size.X, terrainTile.Size.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Size.X * 0.5, terrainTile.Size.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);

            PersistentView.Execute(@"E:\Manuscript\TerrainRendering\Figures\VertexDisplacementMap.xml", _window, _sceneState.Camera);

            //HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            //snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\VertexDisplacementMap.png";
            //snap.WidthInInches = 3;
            //snap.DotsPerInch = 600;

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay(_window.Context);
            _hud.Color = Color.Black;
            UpdateHUD();
        }

        private static string TerrainNormalsAlgorithmToString(TerrainNormalsAlgorithm normals)
        {
            switch(normals)
            {
                case TerrainNormalsAlgorithm.None:
                    return "n/a";
                case TerrainNormalsAlgorithm.ThreeSamples:
                    return "Three Samples";
                case TerrainNormalsAlgorithm.FourSamples:
                    return "Four Samples";
                case TerrainNormalsAlgorithm.SobelFilter:
                    return "Sobel Filter";
            }

            return string.Empty;
        }

        private static string TerrainShadingAlgorithmToString(TerrainShadingAlgorithm shading)
        {
            switch (shading)
            {
                case TerrainShadingAlgorithm.Solid:
                    return "Solid";
                case TerrainShadingAlgorithm.ByHeight:
                    return "By Height";
                case TerrainShadingAlgorithm.HeightContour:
                    return "Height Contour";
                case TerrainShadingAlgorithm.ColorRamp:
                    return "Color Ramp";
                case TerrainShadingAlgorithm.BlendRamp:
                    return "Blend Ramp";
                case TerrainShadingAlgorithm.DetailTexture:
                    return "Detail Texture";
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
                if (_tile.ShadingAlgorithm < TerrainShadingAlgorithm.Solid)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.DetailTexture;
                }
                else if (_tile.ShadingAlgorithm > TerrainShadingAlgorithm.DetailTexture)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.Solid;
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
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);

            _tile.Render(_sceneState);
            _hud.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _tile.Dispose();
            _tile.ColorRampTexture.Dispose();
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
            using (VertexDisplacementMap example = new VertexDisplacementMap())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly VertexDisplacementMapTerrainTile _tile;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private bool _sKeyDown;
        private bool _aKeyDown;
    }
}