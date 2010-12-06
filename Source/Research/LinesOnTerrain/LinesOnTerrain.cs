#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using OpenGlobe.Renderer;
using OpenGlobe.Scene;

using OpenGlobe.Core;
using OpenGlobe.Terrain;

// deron junk todo
//
// clipping to wall shader
// wall normal angle is not a good way to determine which shader to use
//

namespace OpenGlobe.Research
{
    sealed class LinesOnTerrain : IDisposable
    {
        public LinesOnTerrain()
        {
            _window = Device.CreateWindow(800, 600, "Research:  Lines on Terrain");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.Camera.PerspectiveNearPlaneDistance = 10;
            
            _instructions = new HeadsUpDisplay();
            _instructions.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(
                    "u - Use silhouette\ns - Show silhouette\n",
                    new Font("Arial", 24)),
                TextureFormat.RedGreenBlueAlpha8, false);
            _instructions.Color = Color.LightBlue;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.png"));
            _tile = new TriangleMeshTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30.0f;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);
            _sceneState.Camera.Eye = new Vector3D(_xPos, 256, 0);
            
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\LinesOnTerrain.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            //
            // Positions
            //
            IList<Vector3D> positions = new List<Vector3D>();
            double temp = 1.2 * _tile.HeightExaggeration;
            positions.Add(new Vector3D(0.0, 0.0, -temp));
            positions.Add(new Vector3D(0.0, 0.0, temp));
            positions.Add(new Vector3D(100.0, 100.0, -temp));
            positions.Add(new Vector3D(100.0, 100.0, temp));
            positions.Add(new Vector3D(200.0, 100.0, -temp));
            positions.Add(new Vector3D(200.0, 100.0, temp));
            positions.Add(new Vector3D(256.0, 256.0, -temp));
            positions.Add(new Vector3D(256.0, 256.0, temp));
            positions.Add(new Vector3D(512.0, 512.0, -temp));
            positions.Add(new Vector3D(512.0, 512.0, temp));

            //
            // junk 
            _polylineOnTerrain = new PolylineOnTerrain();
            _polylineOnTerrain.Set(_window.Context, positions);


            _clearState = new ClearState();

            // junk
            string fs =
                @"#version 330

                uniform sampler2D og_texture0;
                in vec2 fsTextureCoordinates;
                out vec4 fragmentColor;

                void main()
                {
                    if (texture(og_texture0, fsTextureCoordinates).r == 0.0)
                    {
                        fragmentColor = vec4(0.0, 0.0, 0.0, 1.0);
                    }
                    else
                    {
                        discard;
                    }
                }";

            _viewportQuad = new ViewportQuad(_window.Context, fs);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            //
            // Terrain and silhouette textures
            //
            _tile.RenderDepthAndSilhouetteTextures(_window.Context, _sceneState, _silhouette);

            //
            // Terrain to framebuffer
            //
            _window.Context.Framebuffer = null;
             _window.Context.Clear(_clearState);
            _tile.Render(_window.Context, _sceneState);

            //
            // Overlay the silhouette texture over the framebuffer
            //
            if (_showSilhouette)
            {
                _viewportQuad.Texture = _tile.SilhouetteTexture;
                _viewportQuad.Render(_window.Context, _sceneState);
            }

            //
            // Render the line on terrain
            //
            _polylineOnTerrain.Render(_window.Context, _sceneState, _tile.SilhouetteTexture, _tile.DepthTexture);

            //
            // Render the instructions
            //
            _instructions.Render(_window.Context, _sceneState);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.U)
            {
                _silhouette = !_silhouette;
            }
            else if (e.Key == KeyboardKey.S)
            {
                _showSilhouette = !_showSilhouette;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _instructions.Dispose();
            _tile.Dispose();
            _polylineOnTerrain.Dispose();
            _viewportQuad.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (LinesOnTerrain example = new LinesOnTerrain())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private  SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly HeadsUpDisplay _instructions;
        private readonly TriangleMeshTerrainTile _tile;
        private readonly PolylineOnTerrain _polylineOnTerrain;
        private readonly ViewportQuad _viewportQuad;
        private readonly ClearState _clearState;
        private bool _silhouette = true;
        private bool _showSilhouette = false;

        private double _xPos = 448; // junk deron todo use this still?
    }
}