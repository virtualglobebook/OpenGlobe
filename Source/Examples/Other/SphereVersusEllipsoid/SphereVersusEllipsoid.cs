using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGlobe.Scene;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using System.Drawing;

namespace OpenGlobe.Examples
{
    public class SphereVersusEllipsoid : IDisposable
    {
        public SphereVersusEllipsoid()
        {
            _clearState = new ClearState();
            _clearState.Color = Color.Blue;

            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.90f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _sceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            _window = Device.CreateWindow(800, 600, "Sphere versus Ellipsoid");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            Texture2D texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _ellipsoid = new RayCastedGlobe(_window.Context);
            _ellipsoid.Shape = Ellipsoid.Wgs84;
            _ellipsoid.Texture = texture;

            double averageRadius = (Ellipsoid.Wgs84.MaximumRadius + Ellipsoid.Wgs84.MinimumRadius) / 2.0;

            _sphere = new RayCastedGlobe(_window.Context);
            _sphere.Shape = new Ellipsoid(averageRadius, averageRadius, averageRadius);
            _sphere.Texture = texture;

            _sceneState.Camera.PerspectiveNearPlaneDistance =  1000;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 100000000;

            _sceneState.Camera.ZoomToTarget(_ellipsoid.Shape.MaximumRadius * 2.0);
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _ellipsoid.Shape);

            _billboards = new BillboardCollection(_window.Context);
            _billboards.Texture = Device.CreateTexture2D(new Bitmap("paper-plane--arrow.png"), TextureFormat.RedGreenBlue8, false);
            _billboards.Add(new Billboard()
            {
                Position = _ellipsoid.Shape.ToVector3D(new Geodetic3D(Trig.ToRadians(-55), Trig.ToRadians(60), 1000.0))
            });
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

            if (_renderEllipsoid)
                _ellipsoid.Render(context, _sceneState);
            else
                _sphere.Render(context, _sceneState);

            _billboards.Render(context, _sceneState);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.Space)
            {
                _renderEllipsoid = !_renderEllipsoid;
            }
            else if (e.Key == KeyboardKey.Z)
            {
                _camera.ViewPoint(_ellipsoid.Shape, new Geodetic3D(Trig.ToRadians(-55), Trig.ToRadians(60)));
                _camera.ZoomRateRangeAdjustment = 0.0;
                _camera.RotateRateRangeAdjustment = 0.0;
                _camera.RotateFactor = 0.0;
                _camera.MinimumRotateRate = 2.0;
            }
        }

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        public void Dispose()
        {
            _ellipsoid.Dispose();
            _window.Dispose();
        }

        static void Main()
        {
            using (SphereVersusEllipsoid example = new SphereVersusEllipsoid())
            {
                example.Run(30.0);
            }
        }

        private RayCastedGlobe _ellipsoid;
        private GraphicsWindow _window;
        private ClearState _clearState;
        private SceneState _sceneState;
        private CameraLookAtPoint _camera;
        private RayCastedGlobe _sphere;
        private bool _renderEllipsoid = true;
        private BillboardCollection _billboards;
    }
}
