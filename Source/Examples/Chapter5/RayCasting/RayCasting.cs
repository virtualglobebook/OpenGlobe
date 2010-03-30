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
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter5
{
    sealed class RayCasting : IDisposable
    {
        public RayCasting()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;

            ///////////////////////////////////////////////////////////////////
            Size size = new Size(6, 6);
            float[] heights = new float[]
            {
                0f,    0.25f, 0.35f, 0.4f,  0.2f,  0.1f,    // Bottom row
                0.25f, 0.35f, 0.35f, 0.45f, 0.35f, 0.2f,
                0.35f, 0.35f, 0.35f, 0.45f, 0.5f,  0.4f,
                0.45f, 0.5f,  0.5f,  0.5f,  0.5f,  0.4f,
                0.25f, 0.4f,  0.3f,  0.5f,  0.4f,  0.2f,
                0.15f, 0.3f,  0.2f,  0.1f,  0f,    0f       // Top row
            };
            TerrainTile terrainTile = new TerrainTile(new Size(6, 6), heights, 0, 0.5f);

/*
            Bitmap bitmap = new Bitmap(@"c:\aaa.jpg");

            float[] heights = new float[bitmap.Width * bitmap.Height];
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            int k = 0;
            for (int j = bitmap.Height - 1; j >= 0; --j)
            {
                for (int i = 0; i < bitmap.Width; ++i)
                {
                    float height = (float)(bitmap.GetPixel(i, j).R / 255.0);
                    heights[k++] = height;
                    minHeight = Math.Min(height, minHeight);
                    maxHeight = Math.Max(height, maxHeight);
                }
            }

            TerrainTile terrainTile = new TerrainTile(
                new Size(bitmap.Width, bitmap.Height), 
                heights, minHeight, maxHeight);
*/
            _tile = new RayCastedTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30;

            ///////////////////////////////////////////////////////////////////

            _axes = new Axes(_window.Context);
            _axes.Length = 25;
            _axes.Width = 3;

            _labels = new BillboardCollection(_window.Context, 1);
            _labels.Texture = Device.CreateTexture2D(Device.CreateBitmapFromText("Label", new Font("Arial", 24)), TextureFormat.RedGreenBlueAlpha8, false);
            _labels.Add(new Billboard()
            {
                Position = new Vector3D(3.0, 3.0, 0.5),
                Color = Color.Cyan,
                HorizontalOrigin = HorizontalOrigin.Left,
                VerticalOrigin = VerticalOrigin.Bottom
            });

            double tileRadius = Math.Max(terrainTile.Size.Width, terrainTile.Size.Height) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Size.Width * 0.5, terrainTile.Size.Height * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\TerrainRendering\Figures\GPURayCasting.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);

            _tile.Render(_sceneState);
            _axes.Render(_sceneState);
            _labels.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _tile.Dispose();
            _axes.Dispose();
            _labels.Texture.Dispose();
            _labels.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (RayCasting example = new RayCasting())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly RayCastedTerrainTile _tile;
        private readonly Axes _axes;
        private readonly BillboardCollection _labels;
    }
}