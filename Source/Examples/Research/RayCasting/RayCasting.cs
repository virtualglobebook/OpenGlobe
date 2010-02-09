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
//  1 - Switch between optimized ray casted, ray casted and tessellated globe
//  2 - Show/hide wireframe
//  3 - Show/hide billboards
//
//  Up/Down - Increase/decrease tessellation (when tessellated globe is shown)
//  Right - Switch between solid and shaded ray casted globe (when ray casted globe is shown)
//

//#define FBO

using System;
using System.Drawing;
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

            //
            // From http://www.infoplease.com/ipa/A0001769.html
            //
            _billboardPositions = new Vector3[] 
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
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(8.31), Trig.ToRadians(47.21), 0)) // Zürich, Switzerland
            };

            _window = Device.CreateWindow(800, 600, "Research:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _camera = new CameraGlobeCentered(_sceneState.Camera, _window, _globeShape);

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _optimizedRayCastedGlobe = new OptimizedRayCastedGlobe(_window.Context, _globeShape);
            _optimizedRayCastedGlobe.Texture = _texture;

            _numberOfSlicePartitions = 32;
            _shade = true;

            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
#if FBO
            HighResolutionSnapFrameBuffer snapBuffer = new HighResolutionSnapFrameBuffer(context, 3, 600, _sceneState.Camera.AspectRatio);
            _window.Context.Viewport = new Rectangle(0, 0, snapBuffer.WidthInPixels, snapBuffer.HeightInPixels);
            context.Bind(snapBuffer.FrameBuffer);
#endif

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

            if (_billboards != null)
            {
                _billboards.Render(_sceneState);
            }

#if FBO
            snapBuffer.SaveColorBuffer(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\RayCasting.png");
            //snapBuffer.SaveDepthBuffer(@"c:\depth.tif");
            Environment.Exit(0);
#endif
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.Number1)
            {
                //
                // 1 - Switch between optimized ray casted, ray casted and tessellated globe
                //
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

                if (++_currentAlgorithm == 3)
                {
                    _currentAlgorithm = 0;
                }

                if (_currentAlgorithm == 0)
                {
                    _optimizedRayCastedGlobe = new OptimizedRayCastedGlobe(_window.Context, _globeShape);
                    // TODO:  Shape property
                    _optimizedRayCastedGlobe.Shade = _shade;
                    _optimizedRayCastedGlobe.ShowWireframeBoundingVolume = _wireframe;
                    _optimizedRayCastedGlobe.Texture = _texture;
                }
                else if (_currentAlgorithm == 1)
                {
                    _rayCastedGlobe = new RayCastedGlobe(_window.Context, _globeShape);
                    // TODO:  Shape property
                    _rayCastedGlobe.Shade = _shade;
                    _rayCastedGlobe.ShowWireframeBoundingVolume = _wireframe;
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
            else if (e.Key == KeyboardKey.Number2)
            {
                //
                // 2 - Show/hide wireframe
                //
                _wireframe = !_wireframe;

                if (_optimizedRayCastedGlobe != null)
                {
                    _optimizedRayCastedGlobe.ShowWireframeBoundingVolume = _wireframe;
                }
                if (_rayCastedGlobe != null)
                {
                    _rayCastedGlobe.ShowWireframeBoundingVolume = _wireframe;
                }
                else if (_tessellatedGlobe != null)
                {
                    _tessellatedGlobe.Wireframe = _wireframe;
                }
            }
            else if (e.Key == KeyboardKey.Number3)
            {
                //
                // 3 - Show/hide billboards
                //
                if (_billboards != null)
                {
                    _billboards.Dispose();
                    _billboards = null;
                }
                else
                {
                    // TODO:  Don't reload bitmap!
                    _billboards = new BillboardGroup(_window.Context, _billboardPositions, new Bitmap(@"032.png"));
                }
            }
            else if (_tessellatedGlobe != null)
            {
                //
                // Up/Down - Increase/decrease tessellation (when tessellated globe is shown)
                //
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
            else if (e.Key == KeyboardKey.Right)
            {
                //
                //  Right - Switch between solid and shaded ray casted globe (when ray casted globe is shown)
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

        private readonly Ellipsoid _globeShape;
        private readonly Vector3[] _billboardPositions;

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraGlobeCentered _camera;
        private readonly Texture2D _texture;
        private OptimizedRayCastedGlobe _optimizedRayCastedGlobe;
        private RayCastedGlobe _rayCastedGlobe;
        private TessellatedGlobe _tessellatedGlobe;
        private BillboardGroup _billboards;

        int _currentAlgorithm;
        int _numberOfSlicePartitions;
        bool _wireframe;
        bool _shade;
    }
}