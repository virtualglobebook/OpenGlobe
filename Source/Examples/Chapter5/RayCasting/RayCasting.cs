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
    sealed class RayCasting : IDisposable
    {
        public RayCasting()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 5:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _clearState = new ClearState();

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.jpg"));
            _tile = new RayCastedTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30;

            ///////////////////////////////////////////////////////////////////

            _axes = new Axes();
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

            double tileRadius = Math.Max(terrainTile.Size.X, terrainTile.Size.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Size.X * 0.5, terrainTile.Size.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\GPURayCasting.png";
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
            Context context = _window.Context;
            context.Clear(_clearState);

            _tile.Render(context, _sceneState);
            _axes.Render(context, _sceneState);
            _labels.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _tile.Dispose();
            _axes.Dispose();
            _labels.Texture.Dispose();
            _labels.Dispose();
            _window.Dispose();
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
        private readonly ClearState _clearState;
        private readonly RayCastedTerrainTile _tile;
        private readonly Axes _axes;
        private readonly BillboardCollection _labels;
    }
}