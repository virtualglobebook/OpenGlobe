#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

//#define FBO

using System;
using System.Drawing;

using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;
using MiniGlobe.Core;

namespace MiniGlobe.Examples.Research.RayCasting
{
    internal enum RenderingAlgorithm
    {
        RayCasting,
        Rasterization
    }

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
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-2.9), Trig.DegreesToRadians(57.9), 0)), // Aberdeen, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(138.36), Trig.DegreesToRadians(-34.55), 0)), // Adelaide, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(3.0), Trig.DegreesToRadians(36.50), 0)), // Algiers, Algeria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(4.53), Trig.DegreesToRadians(52.22), 0)), // Amsterdam, Netherlands
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(32.55), Trig.DegreesToRadians(39.55), 0)), // Ankara, Turkey
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-57.40), Trig.DegreesToRadians(-25.15), 0)), // Asunción, Paraguay
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(23.43), Trig.DegreesToRadians(37.58), 0)), // Athens, Greece
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(174.45), Trig.DegreesToRadians(-36.52), 0)), // Auckland, New Zealand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(100.30), Trig.DegreesToRadians(13.45), 0)), // Bangkok, Thailand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(2.9), Trig.DegreesToRadians(41.23), 0)), // Barcelona, Spain
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(116.25), Trig.DegreesToRadians(9.55), 0)), // Beijing, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-48.29), Trig.DegreesToRadians(-1.28), 0)), // Belém, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-5.56), Trig.DegreesToRadians(54.37), 0)), // Belfast, Northern Ireland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(20.32), Trig.DegreesToRadians(44.52), 0)), // Belgrade, Serbia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(13.25), Trig.DegreesToRadians(52.30), 0)), // Berlin, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-1.55), Trig.DegreesToRadians(52.25), 0)), // Birmingham, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-74.15), Trig.DegreesToRadians(4.32), 0)), // Bogotá, Colombia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(72.48), Trig.DegreesToRadians(19.0), 0)), // Bombay, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-0.31), Trig.DegreesToRadians(44.50), 0)), // Bordeaux, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(8.49), Trig.DegreesToRadians(53.5), 0)), // Bremen, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(153.8), Trig.DegreesToRadians(-27.29), 0)), // Brisbane, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-2.35), Trig.DegreesToRadians(51.28), 0)), // Bristol, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(4.22), Trig.DegreesToRadians(50.52), 0)), // Brussels, Belgium
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(26.7), Trig.DegreesToRadians(44.25), 0)), // Bucharest, Romania
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(19.5), Trig.DegreesToRadians(47.30), 0)), // Budapest, Hungary
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-58.22), Trig.DegreesToRadians(-34.35), 0)), // Buenos Aires, Argentina
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(31.21), Trig.DegreesToRadians(30.2), 0)), // Cairo, Egypt
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(88.24), Trig.DegreesToRadians(22.34), 0)), // Calcutta, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(113.15), Trig.DegreesToRadians(23.7), 0)), // Canton, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(18.22), Trig.DegreesToRadians(-33.55), 0)), // Cape Town, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-67.2), Trig.DegreesToRadians(10.28), 0)), // Caracas, Venezuela
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-52.18), Trig.DegreesToRadians(4.49), 0)), // Cayenne, French Guiana
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-106.5), Trig.DegreesToRadians(28.37), 0)), // Chihuahua, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(106.34), Trig.DegreesToRadians(29.46), 0)), // Chongqing, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(12.34), Trig.DegreesToRadians(55.40), 0)), // Copenhagen, Denmark
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-64.10), Trig.DegreesToRadians(-31.28), 0)), // Córdoba, Argentina
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-17.28), Trig.DegreesToRadians(14.40), 0)), // Dakar, Senegal
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(130.51), Trig.DegreesToRadians(-12.28), 0)), // Darwin, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(43.3), Trig.DegreesToRadians(11.30), 0)), // Djibouti, Djibouti
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-6.15), Trig.DegreesToRadians(53.20), 0)), // Dublin, Ireland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(30.53), Trig.DegreesToRadians(-29.53), 0)), // Durban, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-3.10), Trig.DegreesToRadians(55.55), 0)), // Edinburgh, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(8.41), Trig.DegreesToRadians(50.7), 0)), // Frankfurt, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-58.15), Trig.DegreesToRadians(6.45), 0)), // Georgetown, Guyana
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-4.15), Trig.DegreesToRadians(55.50), 0)), // Glasgow, Scotland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-90.31), Trig.DegreesToRadians(14.37), 0)), // Guatemala City, Guatemala
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-79.56), Trig.DegreesToRadians(-2.10), 0)), // Guayaquil, Ecuador
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(10.2), Trig.DegreesToRadians(53.33), 0)), // Hamburg, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(23.38), Trig.DegreesToRadians(70.38), 0)), // Hammerfest, Norway
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-82.23), Trig.DegreesToRadians(23.8), 0)), // Havana, Cuba
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(25.0), Trig.DegreesToRadians(60.10), 0)), // Helsinki, Finland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(147.19), Trig.DegreesToRadians(-42.52), 0)), // Hobart, Tasmania
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(114.11), Trig.DegreesToRadians(22.20), 0)), // Hong Kong, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-70.7), Trig.DegreesToRadians(-20.10), 0)), // Iquique, Chile
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(104.20), Trig.DegreesToRadians(52.30), 0)), // Irkutsk, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(106.48), Trig.DegreesToRadians(-6.16), 0)), // Jakarta, Indonesia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(28.4), Trig.DegreesToRadians(-26.12), 0)), // Johannesburg, South Africa
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-76.49), Trig.DegreesToRadians(17.59), 0)), // Kingston, Jamaica
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(15.17), Trig.DegreesToRadians(-4.18), 0)), // Kinshasa, Congo
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(101.42), Trig.DegreesToRadians(3.8), 0)), // Kuala Lumpur, Malaysia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-68.22), Trig.DegreesToRadians(-16.27), 0)), // La Paz, Bolivia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-1.30), Trig.DegreesToRadians(53.45), 0)), // Leeds, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-77.2), Trig.DegreesToRadians(-12.0), 0)), // Lima, Peru
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-9.9), Trig.DegreesToRadians(38.44), 0)), // Lisbon, Portugal
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-3.0), Trig.DegreesToRadians(53.25), 0)), // Liverpool, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-0.5), Trig.DegreesToRadians(51.32), 0)), // London, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(4.50), Trig.DegreesToRadians(45.45), 0)), // Lyons, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-3.42), Trig.DegreesToRadians(40.26), 0)), // Madrid, Spain
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-2.15), Trig.DegreesToRadians(53.30), 0)), // Manchester, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(120.57), Trig.DegreesToRadians(4.35), 0)), // Manila, Philippines
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(5.20), Trig.DegreesToRadians(43.20), 0)), // Marseilles, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-106.25), Trig.DegreesToRadians(23.12), 0)), // Mazatlán, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(39.45), Trig.DegreesToRadians(21.29), 0)), // Mecca, Saudi Arabia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(144.58), Trig.DegreesToRadians(-37.47), 0)), // Melbourne, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-99.7), Trig.DegreesToRadians(19.26), 0)), // Mexico City, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(9.10), Trig.DegreesToRadians(45.27), 0)), // Milan, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-56.10), Trig.DegreesToRadians(-34.53), 0)), // Montevideo, Uruguay
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(37.36), Trig.DegreesToRadians(55.45), 0)), // Moscow, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(11.35), Trig.DegreesToRadians(48.8), 0)), // Munich, Germany
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(129.57), Trig.DegreesToRadians(32.48), 0)), // Nagasaki, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(136.56), Trig.DegreesToRadians(35.7), 0)), // Nagoya, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(36.55), Trig.DegreesToRadians(-1.25), 0)), // Nairobi, Kenya
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(118.53), Trig.DegreesToRadians(32.3), 0)), // Nanjing (Nanking), China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(14.15), Trig.DegreesToRadians(40.50), 0)), // Naples, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(77.12), Trig.DegreesToRadians(28.35), 0)), // New Delhi, India
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-1.37), Trig.DegreesToRadians(54.58), 0)), // Newcastle-on-Tyne, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(30.48), Trig.DegreesToRadians(46.27), 0)), // Odessa, Ukraine
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(135.30), Trig.DegreesToRadians(34.32), 0)), // Osaka, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(10.42), Trig.DegreesToRadians(59.57), 0)), // Oslo, Norway
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-79.32), Trig.DegreesToRadians(8.58), 0)), // Panama City, Panama
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-55.15), Trig.DegreesToRadians(5.45), 0)), // Paramaribo, Suriname
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(2.20), Trig.DegreesToRadians(48.48), 0)), // Paris, France
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(115.52), Trig.DegreesToRadians(-31.57), 0)), // Perth, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-4.5), Trig.DegreesToRadians(50.25), 0)), // Plymouth, England
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(147.8), Trig.DegreesToRadians(-9.25), 0)), // Port Moresby, Papua New Guinea
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(14.26), Trig.DegreesToRadians(50.5), 0)), // Prague, Czech Republic
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(96.0), Trig.DegreesToRadians(16.50), 0)), // Rangoon, Myanmar
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-21.58), Trig.DegreesToRadians(64.4), 0)), // Reykjavík, Iceland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-43.12), Trig.DegreesToRadians(-22.57), 0)), // Rio de Janeiro, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(12.27), Trig.DegreesToRadians(41.54), 0)), // Rome, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-38.27), Trig.DegreesToRadians(-12.56), 0)), // Salvador, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-70.45), Trig.DegreesToRadians(-33.28), 0)), // Santiago, Chile
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(30.18), Trig.DegreesToRadians(59.56), 0)), // St. Petersburg, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-46.31), Trig.DegreesToRadians(-23.31), 0)), // São Paulo, Brazil
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(121.28), Trig.DegreesToRadians(31.10), 0)), // Shanghai, China
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(103.55), Trig.DegreesToRadians(1.14), 0)), // Singapore, Singapore
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(23.20), Trig.DegreesToRadians(42.40), 0)), // Sofia, Bulgaria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(18.3), Trig.DegreesToRadians(59.17), 0)), // Stockholm, Sweden
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(151.0), Trig.DegreesToRadians(-34.0), 0)), // Sydney, Australia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(47.33), Trig.DegreesToRadians(-18.50), 0)), // Tananarive, Madagascar
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(51.45), Trig.DegreesToRadians(35.45), 0)), // Teheran, Iran
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(139.45), Trig.DegreesToRadians(35.40), 0)), // Tokyo, Japan
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(13.12), Trig.DegreesToRadians(32.57), 0)), // Tripoli, Libya
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(12.20), Trig.DegreesToRadians(45.26), 0)), // Venice, Italy
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(-96.10), Trig.DegreesToRadians(19.10), 0)), // Veracruz, Mexico
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(16.20), Trig.DegreesToRadians(48.14), 0)), // Vienna, Austria
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(132.0), Trig.DegreesToRadians(43.10), 0)), // Vladivostok, Russia
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(21.0), Trig.DegreesToRadians(52.14), 0)), // Warsaw, Poland
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(174.47), Trig.DegreesToRadians(-41.17), 0)), // Wellington, New Zealand
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.DegreesToRadians(8.31), Trig.DegreesToRadians(47.21), 0)) // Zürich, Switzerland
            };

            _window = Device.CreateWindow(800, 600, "Research:  Ray Casting");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _camera = new CameraGlobeCentered(_sceneState.Camera, _window, _globeShape);

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);
            _globe = new RayCastedGlobe(_window.Context, _globeShape, _texture);
            _renderingAlgorithm = RenderingAlgorithm.RayCasting;

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
            _globe.Render(_sceneState);

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
                (_globe as IDisposable).Dispose();

                if (_renderingAlgorithm == RenderingAlgorithm.RayCasting)
                {
                    _globe = new TessellatedGlobe(_window.Context, _globeShape, _texture);
                    _renderingAlgorithm = RenderingAlgorithm.Rasterization;
                }
                else if (_renderingAlgorithm == RenderingAlgorithm.Rasterization)
                {
                    _globe = new RayCastedGlobe(_window.Context, _globeShape, _texture);
                    _renderingAlgorithm = RenderingAlgorithm.RayCasting;
                }
            }
            else if (e.Key == KeyboardKey.Number2)
            {
                if (_billboards != null)
                {
                    (_billboards as IDisposable).Dispose();
                    _billboards = null;
                }
                else
                {
                    // TODO:  Don't reload bitmap!
                    _billboards = new BillboardGroup(_window.Context, _billboardPositions, new Bitmap(@"032.png"));
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_billboards != null)
            {
                (_billboards as IDisposable).Dispose();
            }
            (_globe as IDisposable).Dispose();
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
        private IRenderable _globe;
        private IRenderable _billboards;
        private RenderingAlgorithm _renderingAlgorithm;
    }
}