#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

//
// Keyboards Controls
//  Left/Right - Switch between optimized ray casted, ray casted and tessellated globe
//  Up/Down 
//     - When optimized ray casted globe is shown:  increase/decrease bounding polygon points
//     - When tessellated globe is shown:  increase/decrease tessellation
//
//  1 - Show/hide wireframe
//  2 - Switch between solid and shaded ray casted globe (when ray casted globe is shown)
//  3 - Show/hide billboards
//
//  Space - Switch between whole earth and horizon view
//

using System;
using System.Drawing;
using System.Collections.Generic;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Research.RayCasting
{
    sealed class RayCasting : IDisposable
    {
        public RayCasting()
        {
            _globeShape = Ellipsoid.UnitSphere;
            //_globeShape = new Ellipsoid(2.0, 4.0, 6.0);

            _billboardPositions = AllCities(); 

            _window = Device.CreateWindow(800, 600, "Research:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _globeShape);

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);
            _billboards = new List<BillboardGroup>();

            _numberOfSlicePartitions = 32;
            _numberOfBoundingPolygonPoints = 3;
            _shade = true;

            LoadGlobe();

            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.png";
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

            if (_optimizedRayCastedGlobe != null)
            {
                _optimizedRayCastedGlobe.Render(_sceneState);
            }
            else if (_rayCastedGlobe != null)
            {
                _rayCastedGlobe.Render(_sceneState);
            }
            else if (_tessellatedGlobe != null)
            {
                _tessellatedGlobe.Render(_sceneState);
            }

            for (int i = 0; i < _billboards.Count; ++i)
            {
                _billboards[i].Render(_sceneState);
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
                //
                // Left/Right - Switch between optimized ray casted, ray casted and tessellated globe
                //
                _currentAlgorithm += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_currentAlgorithm < 0)
                {
                    _currentAlgorithm = 2;
                }
                else if (_currentAlgorithm > 2)
                {
                    _currentAlgorithm = 0;
                }

                LoadGlobe();
            }
            else if ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down))
            {
                //
                //  Up/Down 
                //     - When optimized ray casted globe is shown:  increase/decrease bounding polygon points
                //     - When tessellated globe is shown:  increase/decrease tessellation
                //
                if (_optimizedRayCastedGlobe != null)
                {
                    _numberOfBoundingPolygonPoints += (e.Key == KeyboardKey.Up) ? 1 : -1;
                    _numberOfBoundingPolygonPoints = Math.Max(_numberOfBoundingPolygonPoints, 3);
                    _optimizedRayCastedGlobe.NumberOfBoundingPolygonPoints = _numberOfBoundingPolygonPoints;
                }
                else if (_tessellatedGlobe != null)
                {
                    if (e.Key == KeyboardKey.Up)
                    {
                        _numberOfSlicePartitions *= 2;
                        _numberOfSlicePartitions = Math.Min(_numberOfSlicePartitions, 2048);
                    }
                    else if (e.Key == KeyboardKey.Down)
                    {
                        _numberOfSlicePartitions /= 2;
                        _numberOfSlicePartitions = Math.Max(_numberOfSlicePartitions, 4);
                    }

                    _tessellatedGlobe.NumberOfSlicePartitions = _numberOfSlicePartitions;
                    _tessellatedGlobe.NumberOfStackPartitions = _numberOfSlicePartitions / 2;
                }
            }
            else if (e.Key == KeyboardKey.Number1)
            {
                //
                // 1 - Show/hide wireframe
                //
                _wireframe = !_wireframe;

                if (_optimizedRayCastedGlobe != null)
                {
                    _optimizedRayCastedGlobe.ShowWireframeBoundingPolygon = _wireframe;
                }
                if (_rayCastedGlobe != null)
                {
                    _rayCastedGlobe.ShowWireframeBoundingBox = _wireframe;
                }
                else if (_tessellatedGlobe != null)
                {
                    _tessellatedGlobe.Wireframe = _wireframe;
                }
            }
            else if (e.Key == KeyboardKey.Number2)
            {
                //
                //  2 - Switch between solid and shaded ray casted globe (when ray casted globe is shown)
                //
                if (_optimizedRayCastedGlobe != null)
                {
                    _shade = !_shade;
                    _optimizedRayCastedGlobe.Shade = _shade;
                }
                else if (_rayCastedGlobe != null)
                {
                    _shade = !_shade;
                    _rayCastedGlobe.Shade = _shade;
                }
            }
            else if (e.Key == KeyboardKey.Number3)
            {
                //
                // 3 - Show/hide billboards
                //
                if (_billboards.Count != 0)
                {
                    for (int i = 0; i < _billboards.Count; ++i)
                    {
                        _billboards[i].Dispose();
                    }
                    _billboards.Clear();
                }
                else
                {
                    LoadBillboards();
                }
            }
            else if (e.Key == KeyboardKey.Space)
            {
                _viewingHorizon = !_viewingHorizon;
                if (_viewingHorizon)
                {
                    CenterCameraOnPoint();
                }
                else
                {
                    CenterCameraOnGlobeCenter();
                }
            }
        }

        private void CenterCameraOnPoint()
        {
            _camera.ViewPoint(Trig.ToRadians(-75.697), Trig.ToRadians(40.039), 0.0);
            _camera.Azimuth = 0.0;
            _camera.Elevation = Trig.ToRadians(10);
            _camera.Range = _globeShape.MaximumRadius * 0.05;
        }

        private void CenterCameraOnGlobeCenter()
        {
            _sceneState.Camera.Target = Vector3d.Zero;
            _sceneState.Camera.Up = new Vector3d(0, 0, 1);
            _sceneState.Camera.Eye = new Vector3d(0, -1, 0);
            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);
            _camera.UpdateParametersFromCamera();
        }

        private void LoadGlobe()
        {
            if (_optimizedRayCastedGlobe != null)
            {
                _optimizedRayCastedGlobe.Dispose();
                _optimizedRayCastedGlobe = null;
            }

            if (_rayCastedGlobe != null)
            {
                _rayCastedGlobe.Dispose();
                _rayCastedGlobe = null;
            }

            if (_tessellatedGlobe != null)
            {
                _tessellatedGlobe.Dispose();
                _tessellatedGlobe = null;
            }

            if (_currentAlgorithm == 0)
            {
                _optimizedRayCastedGlobe = new OptimizedRayCastedGlobe(_window.Context);
                _optimizedRayCastedGlobe.Shape = _globeShape;
                _optimizedRayCastedGlobe.NumberOfBoundingPolygonPoints = _numberOfBoundingPolygonPoints;
                _optimizedRayCastedGlobe.Shade = _shade;
                _optimizedRayCastedGlobe.ShowWireframeBoundingPolygon = _wireframe;
                _optimizedRayCastedGlobe.Texture = _texture;
            }
            else if (_currentAlgorithm == 1)
            {
                _rayCastedGlobe = new RayCastedGlobe(_window.Context);
                _rayCastedGlobe.Shape = _globeShape;
                _rayCastedGlobe.Shade = _shade;
                _rayCastedGlobe.ShowWireframeBoundingBox = _wireframe;
                _rayCastedGlobe.Texture = _texture;
            }
            else if (_currentAlgorithm == 2)
            {
                _tessellatedGlobe = new TessellatedGlobe(_window.Context);
                _tessellatedGlobe.Shape = _globeShape;
                _tessellatedGlobe.NumberOfSlicePartitions = _numberOfSlicePartitions;
                _tessellatedGlobe.NumberOfStackPartitions = _numberOfSlicePartitions / 2;
                _tessellatedGlobe.Wireframe = _wireframe;
                _tessellatedGlobe.Texture = _texture;
            }
        }

        private void LoadBillboards()
        {
            IList<Vector3> one = new List<Vector3>();
            IList<Vector3> two = new List<Vector3>();
            IList<Vector3> three = new List<Vector3>();
            IList<Vector3> four = new List<Vector3>();
            IList<Vector3> five = new List<Vector3>();

            for (int i = 0; i < _billboardPositions.Length; ++i)
            {
                if (i % 5 == 0)
                {
                    one.Add(_billboardPositions[i]);
                }
                else if (i % 5 == 1)
                {
                    two.Add(_billboardPositions[i]);
                }
                else if (i % 5 == 2)
                {
                    three.Add(_billboardPositions[i]);
                }                
                else if (i % 5 == 3)
                {
                    four.Add(_billboardPositions[i]);
                }                
                else if (i % 5 == 4)
                {
                    five.Add(_billboardPositions[i]);
                }
            }

            Vector3[] positionsOne = new Vector3[one.Count];
            Vector3[] positionsTwo = new Vector3[two.Count];
            Vector3[] positionsThree = new Vector3[three.Count];
            Vector3[] positionsFour = new Vector3[four.Count];
            Vector3[] positionsFive = new Vector3[five.Count];

            one.CopyTo(positionsOne, 0);
            two.CopyTo(positionsTwo, 0);
            three.CopyTo(positionsThree, 0);
            four.CopyTo(positionsFour, 0);
            five.CopyTo(positionsFive, 0);
            
            _billboards.Add(new BillboardGroup(_window.Context, positionsOne, new Bitmap(@"032.png")));
            _billboards.Add(new BillboardGroup(_window.Context, positionsTwo, new Bitmap(@"045.png")));
            _billboards.Add(new BillboardGroup(_window.Context, positionsThree, new Bitmap(@"asterisk.png")));
            _billboards.Add(new BillboardGroup(_window.Context, positionsFour, new Bitmap(@"building.png")));
            _billboards.Add(new BillboardGroup(_window.Context, positionsFive, new Bitmap(@"pin.png")));
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_optimizedRayCastedGlobe != null)
            {
                _optimizedRayCastedGlobe.Dispose();
            }
            else if (_rayCastedGlobe != null)
            {
                _rayCastedGlobe.Dispose();
            }
            else if (_tessellatedGlobe != null)
            {
                _tessellatedGlobe.Dispose();
            }

            for (int i = 0; i < _billboards.Count; ++i)
            {
                _billboards[i].Dispose();
            }

            _texture.Dispose();
            _camera.Dispose();
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

        private Vector3[] AllCities()
        {
            Vector3[] worldCities = WorldCities();
            Vector3[] northAmericanCities = NorthAmericanCities();
            Vector3[] cities = new Vector3[worldCities.Length + northAmericanCities.Length];
            worldCities.CopyTo(cities, 0);
            northAmericanCities.CopyTo(cities, worldCities.Length);
            return cities;
        }

        private Vector3[] WorldCities()
        {
            //
            // From http://www.infoplease.com/ipa/A0001796.html
            //
            return new Vector3[]
            {
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-73.45), Trig.ToRadians(42.40), 0)), // Albany, N.Y.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-106.39), Trig.ToRadians(35.05), 0)), // Albuquerque, N.M.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-101.50), Trig.ToRadians(35.11), 0)), // Amarillo, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-149.54), Trig.ToRadians(61.13), 0)), // Anchorage, Alaska
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-84.23), Trig.ToRadians(33.45), 0)), // Atlanta, Ga.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-97.44), Trig.ToRadians(30.16), 0)), // Austin, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-117.50), Trig.ToRadians(44.47), 0)), // Baker, Ore.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-76.38), Trig.ToRadians(39.18), 0)), // Baltimore, Md.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-68.47), Trig.ToRadians(44.48), 0)), // Bangor, Maine
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-86.50), Trig.ToRadians(33.30), 0)), // Birmingham, Ala.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-100.47), Trig.ToRadians(46.48), 0)), // Bismarck, N.D.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-116.13), Trig.ToRadians(43.36), 0)), // Boise, Idaho
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-71.5), Trig.ToRadians(42.21), 0)), // Boston, Mass.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-78.50), Trig.ToRadians(42.55), 0)), // Buffalo, N.Y.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-114.1), Trig.ToRadians(51.1), 0)), // Calgary, Alba., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-104.15), Trig.ToRadians(32.26), 0)), // Carlsbad, N.M.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.56), Trig.ToRadians(32.47), 0)), // Charleston, S.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.38), Trig.ToRadians(38.21), 0)), // Charleston, W. Va.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-80.50), Trig.ToRadians(35.14), 0)), // Charlotte, N.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-104.52), Trig.ToRadians(41.9), 0)), // Cheyenne, Wyo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-87.37), Trig.ToRadians(41.50), 0)), // Chicago, Ill.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-84.30), Trig.ToRadians(39.8), 0)), // Cincinnati, Ohio
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.37), Trig.ToRadians(41.28), 0)), // Cleveland, Ohio
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.2), Trig.ToRadians(34.0), 0)), // Columbia, S.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-83.1), Trig.ToRadians(40.0), 0)), // Columbus, Ohio
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-96.46), Trig.ToRadians(32.46), 0)), // Dallas, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-105.0), Trig.ToRadians(39.45), 0)), // Denver, Colo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-93.37), Trig.ToRadians(41.35), 0)), // Des Moines, Iowa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-83.3), Trig.ToRadians(42.20), 0)), // Detroit, Mich.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.40), Trig.ToRadians(42.31), 0)), // Dubuque, Iowa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-92.5), Trig.ToRadians(46.49), 0)), // Duluth, Minn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-67.0), Trig.ToRadians(44.54), 0)), // Eastport, Maine
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-113.28), Trig.ToRadians(53.34), 0)), // Edmonton, Alb., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-115.33), Trig.ToRadians(32.38), 0)), // El Centro, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-106.29), Trig.ToRadians(31.46), 0)), // El Paso, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-123.5), Trig.ToRadians(44.3), 0)), // Eugene, Ore.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-96.48), Trig.ToRadians(46.52), 0)), // Fargo, N.D.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-111.41), Trig.ToRadians(35.13), 0)), // Flagstaff, Ariz.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-97.19), Trig.ToRadians(32.43), 0)), // Fort Worth, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-119.48), Trig.ToRadians(36.44), 0)), // Fresno, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-108.33), Trig.ToRadians(39.5), 0)), // Grand Junction, Colo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-85.40), Trig.ToRadians(42.58), 0)), // Grand Rapids, Mich.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-109.43), Trig.ToRadians(48.33), 0)), // Havre, Mont.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-112.2), Trig.ToRadians(46.35), 0)), // Helena, Mont.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-157.50), Trig.ToRadians(21.18), 0)), // Honolulu, Hawaii
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-93.3), Trig.ToRadians(34.31), 0)), // Hot Springs, Ark.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-95.21), Trig.ToRadians(29.45), 0)), // Houston, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-112.1), Trig.ToRadians(43.30), 0)), // Idaho Falls, Idaho 
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-86.10), Trig.ToRadians(39.46), 0)), // Indianapolis, Ind.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.12), Trig.ToRadians(32.20), 0)), // Jackson, Miss.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.40), Trig.ToRadians(30.22), 0)), // Jacksonville, Fla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-134.24), Trig.ToRadians(58.18), 0)), // Juneau, Alaska
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-94.35), Trig.ToRadians(39.6), 0)), // Kansas City, Mo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.48), Trig.ToRadians(24.33), 0)), // Key West, Fla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-76.30), Trig.ToRadians(44.15), 0)), // Kingston, Ont., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-121.44), Trig.ToRadians(42.10), 0)), // Klamath Falls, Ore.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-83.56), Trig.ToRadians(35.57), 0)), // Knoxville, Tenn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-115.12), Trig.ToRadians(36.10), 0)), // Las Vegas, Nev.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-117.2), Trig.ToRadians(46.24), 0)), // Lewiston, Idaho
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-96.40), Trig.ToRadians(40.50), 0)), // Lincoln, Neb.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.34), Trig.ToRadians(43.2), 0)), // London, Ont., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-118.11), Trig.ToRadians(33.46), 0)), // Long Beach, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-118.15), Trig.ToRadians(34.3), 0)), // Los Angeles, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-85.46), Trig.ToRadians(38.15), 0)), // Louisville, Ky.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-71.30), Trig.ToRadians(43.0), 0)), // Manchester, N.H.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.3), Trig.ToRadians(35.9), 0)), // Memphis, Tenn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-80.12), Trig.ToRadians(25.46), 0)), // Miami, Fla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-87.55), Trig.ToRadians(43.2), 0)), // Milwaukee, Wis.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-93.14), Trig.ToRadians(44.59), 0)), // Minneapolis, Minn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-88.3), Trig.ToRadians(30.42), 0)), // Mobile, Ala.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-86.18), Trig.ToRadians(32.21), 0)), // Montgomery, Ala.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-72.32), Trig.ToRadians(44.15), 0)), // Montpelier, Vt.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-73.35), Trig.ToRadians(45.30), 0)), // Montreal, Que., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-105.31), Trig.ToRadians(50.37), 0)), // Moose Jaw, Sask., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-86.47), Trig.ToRadians(36.10), 0)), // Nashville, Tenn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-117.17), Trig.ToRadians(49.30), 0)), // Nelson, B.C., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-74.10), Trig.ToRadians(40.44), 0)), // Newark, N.J.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-72.55), Trig.ToRadians(41.19), 0)), // New Haven, Conn.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.4), Trig.ToRadians(29.57), 0)), // New Orleans, La.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-73.58), Trig.ToRadians(40.47), 0)), // New York, N.Y.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-165.30), Trig.ToRadians(64.25), 0)), // Nome, Alaska
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-122.16), Trig.ToRadians(37.48), 0)), // Oakland, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-97.28), Trig.ToRadians(35.26), 0)), // Oklahoma City, Okla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-95.56), Trig.ToRadians(41.15), 0)), // Omaha, Neb.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-75.43), Trig.ToRadians(45.24), 0)), // Ottawa, Ont., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-75.10), Trig.ToRadians(39.57), 0)), // Philadelphia, Pa.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-112.4), Trig.ToRadians(33.29), 0)), // Phoenix, Ariz.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-100.21), Trig.ToRadians(44.22), 0)), // Pierre, S.D.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.57), Trig.ToRadians(40.27), 0)), // Pittsburgh, Pa.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-70.15), Trig.ToRadians(43.40), 0)), // Portland, Maine
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-122.41), Trig.ToRadians(45.31), 0)), // Portland, Ore.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-71.24), Trig.ToRadians(41.50), 0)), // Providence, R.I.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-71.11), Trig.ToRadians(46.49), 0)), // Quebec, Que., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-78.39), Trig.ToRadians(35.46), 0)), // Raleigh, N.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-119.49), Trig.ToRadians(39.30), 0)), // Reno, Nev.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-112.5), Trig.ToRadians(38.46), 0)), // Richfield, Utah
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-77.29), Trig.ToRadians(37.33), 0)), // Richmond, Va.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.57), Trig.ToRadians(37.17), 0)), // Roanoke, Va.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-121.30), Trig.ToRadians(38.35), 0)), // Sacramento, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-66.10), Trig.ToRadians(45.18), 0)), // St. John, N.B., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.12), Trig.ToRadians(38.35), 0)), // St. Louis, Mo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-111.54), Trig.ToRadians(40.46), 0)), // Salt Lake City, Utah
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-98.33), Trig.ToRadians(29.23), 0)), // San Antonio, Tex.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-117.10), Trig.ToRadians(32.42), 0)), // San Diego, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-122.26), Trig.ToRadians(37.47), 0)), // San Francisco, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-121.53), Trig.ToRadians(37.20), 0)), // San Jose, Calif.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-66.10), Trig.ToRadians(18.30), 0)), // San Juan, P.R.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-105.57), Trig.ToRadians(35.41), 0)), // Santa Fe, N.M.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-81.5), Trig.ToRadians(32.5), 0)), // Savannah, Ga.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-122.20), Trig.ToRadians(47.37), 0)), // Seattle, Wash.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-93.42), Trig.ToRadians(32.28), 0)), // Shreveport, La.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-96.44), Trig.ToRadians(43.33), 0)), // Sioux Falls, S.D.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-135.15), Trig.ToRadians(57.10), 0)), // Sitka, Alaska
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-117.26), Trig.ToRadians(47.40), 0)), // Spokane, Wash.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-89.38), Trig.ToRadians(39.48), 0)), // Springfield, Ill.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-72.34), Trig.ToRadians(42.6), 0)), // Springfield, Mass.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-93.17), Trig.ToRadians(37.13), 0)), // Springfield, Mo.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-76.8), Trig.ToRadians(43.2), 0)), // Syracuse, N.Y.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-82.27), Trig.ToRadians(27.57), 0)), // Tampa, Fla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-83.33), Trig.ToRadians(41.39), 0)), // Toledo, Ohio
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.24), Trig.ToRadians(43.40), 0)), // Toronto, Ont., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-95.59), Trig.ToRadians(36.09), 0)), // Tulsa, Okla.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-123.06), Trig.ToRadians(49.13), 0)), // Vancouver, B.C., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-123.21), Trig.ToRadians(48.25), 0)), // Victoria, B.C., Can.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-75.58), Trig.ToRadians(36.51), 0)), // Virginia Beach, Va.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-77.02), Trig.ToRadians(38.53), 0)), // Washington, D.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-97.17), Trig.ToRadians(37.43), 0)), // Wichita, Kan.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-77.57), Trig.ToRadians(34.14), 0)), // Wilmington, N.C.
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-97.7), Trig.ToRadians(49.54), 0))  // Winnipeg, Man., Can.
            };
        }

        private Vector3[] NorthAmericanCities()
        {
            //
            // From http://www.infoplease.com/ipa/A0001769.html
            //
            return new Vector3[]
            { 
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-2.9), Trig.ToRadians(57.9), 0)), // Aberdeen, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(138.36), Trig.ToRadians(-34.55), 0)), // Adelaide, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(3.0), Trig.ToRadians(36.50), 0)), // Algiers, Algeria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(4.53), Trig.ToRadians(52.22), 0)), // Amsterdam, Netherlands
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(32.55), Trig.ToRadians(39.55), 0)), // Ankara, Turkey
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-57.40), Trig.ToRadians(-25.15), 0)), // Asunción, Paraguay
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(23.43), Trig.ToRadians(37.58), 0)), // Athens, Greece
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(174.45), Trig.ToRadians(-36.52), 0)), // Auckland, New Zealand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(100.30), Trig.ToRadians(13.45), 0)), // Bangkok, Thailand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(2.9), Trig.ToRadians(41.23), 0)), // Barcelona, Spain
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(116.25), Trig.ToRadians(9.55), 0)), // Beijing, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-48.29), Trig.ToRadians(-1.28), 0)), // Belém, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-5.56), Trig.ToRadians(54.37), 0)), // Belfast, Northern Ireland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(20.32), Trig.ToRadians(44.52), 0)), // Belgrade, Serbia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(13.25), Trig.ToRadians(52.30), 0)), // Berlin, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-1.55), Trig.ToRadians(52.25), 0)), // Birmingham, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-74.15), Trig.ToRadians(4.32), 0)), // Bogotá, Colombia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(72.48), Trig.ToRadians(19.0), 0)), // Bombay, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-0.31), Trig.ToRadians(44.50), 0)), // Bordeaux, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(8.49), Trig.ToRadians(53.5), 0)), // Bremen, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(153.8), Trig.ToRadians(-27.29), 0)), // Brisbane, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-2.35), Trig.ToRadians(51.28), 0)), // Bristol, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(4.22), Trig.ToRadians(50.52), 0)), // Brussels, Belgium
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(26.7), Trig.ToRadians(44.25), 0)), // Bucharest, Romania
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(19.5), Trig.ToRadians(47.30), 0)), // Budapest, Hungary
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-58.22), Trig.ToRadians(-34.35), 0)), // Buenos Aires, Argentina
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(31.21), Trig.ToRadians(30.2), 0)), // Cairo, Egypt
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(88.24), Trig.ToRadians(22.34), 0)), // Calcutta, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(113.15), Trig.ToRadians(23.7), 0)), // Canton, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(18.22), Trig.ToRadians(-33.55), 0)), // Cape Town, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-67.2), Trig.ToRadians(10.28), 0)), // Caracas, Venezuela
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-52.18), Trig.ToRadians(4.49), 0)), // Cayenne, French Guiana
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-106.5), Trig.ToRadians(28.37), 0)), // Chihuahua, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(106.34), Trig.ToRadians(29.46), 0)), // Chongqing, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(12.34), Trig.ToRadians(55.40), 0)), // Copenhagen, Denmark
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-64.10), Trig.ToRadians(-31.28), 0)), // Córdoba, Argentina
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-17.28), Trig.ToRadians(14.40), 0)), // Dakar, Senegal
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(130.51), Trig.ToRadians(-12.28), 0)), // Darwin, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(43.3), Trig.ToRadians(11.30), 0)), // Djibouti, Djibouti
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-6.15), Trig.ToRadians(53.20), 0)), // Dublin, Ireland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(30.53), Trig.ToRadians(-29.53), 0)), // Durban, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-3.10), Trig.ToRadians(55.55), 0)), // Edinburgh, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(8.41), Trig.ToRadians(50.7), 0)), // Frankfurt, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-58.15), Trig.ToRadians(6.45), 0)), // Georgetown, Guyana
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-4.15), Trig.ToRadians(55.50), 0)), // Glasgow, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-90.31), Trig.ToRadians(14.37), 0)), // Guatemala City, Guatemala
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.56), Trig.ToRadians(-2.10), 0)), // Guayaquil, Ecuador
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(10.2), Trig.ToRadians(53.33), 0)), // Hamburg, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(23.38), Trig.ToRadians(70.38), 0)), // Hammerfest, Norway
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-82.23), Trig.ToRadians(23.8), 0)), // Havana, Cuba
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(25.0), Trig.ToRadians(60.10), 0)), // Helsinki, Finland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(147.19), Trig.ToRadians(-42.52), 0)), // Hobart, Tasmania
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(114.11), Trig.ToRadians(22.20), 0)), // Hong Kong, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-70.7), Trig.ToRadians(-20.10), 0)), // Iquique, Chile
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(104.20), Trig.ToRadians(52.30), 0)), // Irkutsk, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(106.48), Trig.ToRadians(-6.16), 0)), // Jakarta, Indonesia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(28.4), Trig.ToRadians(-26.12), 0)), // Johannesburg, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-76.49), Trig.ToRadians(17.59), 0)), // Kingston, Jamaica
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(15.17), Trig.ToRadians(-4.18), 0)), // Kinshasa, Congo
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(101.42), Trig.ToRadians(3.8), 0)), // Kuala Lumpur, Malaysia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-68.22), Trig.ToRadians(-16.27), 0)), // La Paz, Bolivia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-1.30), Trig.ToRadians(53.45), 0)), // Leeds, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-77.2), Trig.ToRadians(-12.0), 0)), // Lima, Peru
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-9.9), Trig.ToRadians(38.44), 0)), // Lisbon, Portugal
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-3.0), Trig.ToRadians(53.25), 0)), // Liverpool, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-0.5), Trig.ToRadians(51.32), 0)), // London, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(4.50), Trig.ToRadians(45.45), 0)), // Lyons, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-3.42), Trig.ToRadians(40.26), 0)), // Madrid, Spain
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-2.15), Trig.ToRadians(53.30), 0)), // Manchester, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(120.57), Trig.ToRadians(4.35), 0)), // Manila, Philippines
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(5.20), Trig.ToRadians(43.20), 0)), // Marseilles, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-106.25), Trig.ToRadians(23.12), 0)), // Mazatlán, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(39.45), Trig.ToRadians(21.29), 0)), // Mecca, Saudi Arabia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(144.58), Trig.ToRadians(-37.47), 0)), // Melbourne, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-99.7), Trig.ToRadians(19.26), 0)), // Mexico City, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(9.10), Trig.ToRadians(45.27), 0)), // Milan, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-56.10), Trig.ToRadians(-34.53), 0)), // Montevideo, Uruguay
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(37.36), Trig.ToRadians(55.45), 0)), // Moscow, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(11.35), Trig.ToRadians(48.8), 0)), // Munich, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(129.57), Trig.ToRadians(32.48), 0)), // Nagasaki, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(136.56), Trig.ToRadians(35.7), 0)), // Nagoya, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(36.55), Trig.ToRadians(-1.25), 0)), // Nairobi, Kenya
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(118.53), Trig.ToRadians(32.3), 0)), // Nanjing (Nanking), China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(14.15), Trig.ToRadians(40.50), 0)), // Naples, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(77.12), Trig.ToRadians(28.35), 0)), // New Delhi, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-1.37), Trig.ToRadians(54.58), 0)), // Newcastle-on-Tyne, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(30.48), Trig.ToRadians(46.27), 0)), // Odessa, Ukraine
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(135.30), Trig.ToRadians(34.32), 0)), // Osaka, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(10.42), Trig.ToRadians(59.57), 0)), // Oslo, Norway
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-79.32), Trig.ToRadians(8.58), 0)), // Panama City, Panama
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-55.15), Trig.ToRadians(5.45), 0)), // Paramaribo, Suriname
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(2.20), Trig.ToRadians(48.48), 0)), // Paris, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(115.52), Trig.ToRadians(-31.57), 0)), // Perth, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-4.5), Trig.ToRadians(50.25), 0)), // Plymouth, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(147.8), Trig.ToRadians(-9.25), 0)), // Port Moresby, Papua New Guinea
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(14.26), Trig.ToRadians(50.5), 0)), // Prague, Czech Republic
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(96.0), Trig.ToRadians(16.50), 0)), // Rangoon, Myanmar
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-21.58), Trig.ToRadians(64.4), 0)), // Reykjavík, Iceland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-43.12), Trig.ToRadians(-22.57), 0)), // Rio de Janeiro, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(12.27), Trig.ToRadians(41.54), 0)), // Rome, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-38.27), Trig.ToRadians(-12.56), 0)), // Salvador, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-70.45), Trig.ToRadians(-33.28), 0)), // Santiago, Chile
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(30.18), Trig.ToRadians(59.56), 0)), // St. Petersburg, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-46.31), Trig.ToRadians(-23.31), 0)), // São Paulo, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(121.28), Trig.ToRadians(31.10), 0)), // Shanghai, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(103.55), Trig.ToRadians(1.14), 0)), // Singapore, Singapore
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(23.20), Trig.ToRadians(42.40), 0)), // Sofia, Bulgaria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(18.3), Trig.ToRadians(59.17), 0)), // Stockholm, Sweden
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(151.0), Trig.ToRadians(-34.0), 0)), // Sydney, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(47.33), Trig.ToRadians(-18.50), 0)), // Tananarive, Madagascar
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(51.45), Trig.ToRadians(35.45), 0)), // Teheran, Iran
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(139.45), Trig.ToRadians(35.40), 0)), // Tokyo, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(13.12), Trig.ToRadians(32.57), 0)), // Tripoli, Libya
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(12.20), Trig.ToRadians(45.26), 0)), // Venice, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-96.10), Trig.ToRadians(19.10), 0)), // Veracruz, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(16.20), Trig.ToRadians(48.14), 0)), // Vienna, Austria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(132.0), Trig.ToRadians(43.10), 0)), // Vladivostok, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(21.0), Trig.ToRadians(52.14), 0)), // Warsaw, Poland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(174.47), Trig.ToRadians(-41.17), 0)), // Wellington, New Zealand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(8.31), Trig.ToRadians(47.21), 0)), // Zürich, Switzerland
            };
        }

        private readonly Ellipsoid _globeShape;
        private readonly Vector3[] _billboardPositions;

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly Texture2D _texture;
        private OptimizedRayCastedGlobe _optimizedRayCastedGlobe;
        private RayCastedGlobe _rayCastedGlobe;
        private TessellatedGlobe _tessellatedGlobe;
        private IList<BillboardGroup> _billboards;

        int _currentAlgorithm;
        int _numberOfSlicePartitions;
        int _numberOfBoundingPolygonPoints;
        bool _wireframe;
        bool _shade;
        bool _viewingHorizon;
    }
}