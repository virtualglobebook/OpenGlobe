#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class Curves : IDisposable
    {
        public Curves()
        {
            _semiMinorAxis = Ellipsoid.ScaledWgs84.Radii.Z;
            SetShape();

            _window = Device.CreateWindow(800, 600, "Chapter 2:  Curves");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _globeShape);

            _clearState = new ClearState();

            _texture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8));
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, 3);
            pixelBuffer.CopyFromSystemMemory(new byte[] { 0, 255, 127 });
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1);

            _instructions = new HeadsUpDisplay();
            _instructions.Color = Color.Black;
            
            _sampledPoints = new BillboardCollection(_window.Context);
            _sampledPoints.Texture = Device.CreateTexture2D(Device.CreateBitmapFromPoint(8), TextureFormat.RedGreenBlueAlpha8, false);
            _sampledPoints.DepthTestEnabled = false;
            
            _ellipsoid = new RayCastedGlobe(_window.Context);
            _ellipsoid.Texture = _texture;

            _polyline = new Polyline();
            _polyline.Width = 3;
            _polyline.DepthTestEnabled = false;

            _plane = new Plane(_window.Context);
            _plane.Origin = Vector3D.Zero;
            _plane.OutlineWidth = 3;

            CreateScene();
            
            ///////////////////////////////////////////////////////////////////

            _sceneState.Camera.Eye = Vector3D.UnitY;
            _sceneState.Camera.ZoomToTarget(2 * _globeShape.MaximumRadius);
            PersistentView.Execute(@"E:\Manuscript\VirtualGlobeFoundations\Figures\Curves.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\VirtualGlobeFoundations\Figures\Curves.png";
            snap.WidthInInches = 1.5;
            snap.DotsPerInch = 1200;
        }

        private void CreateScene()
        {
            string text = "Granularity: " + _granularityInDegrees + " (left/right)\n";
            text += "Points: " + (_sampledPoints.Show ? "on" : "off") + " ('1')\n";
            text += "Polyline: " + (_polyline.Show ? "on" : "off") + " ('2')\n";
            text += "Plane: " + (_plane.Show ? "on" : "off") + " ('3')\n";
            text += "Semi-minor axis (up/down)\n";

            _instructions.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, new Font("Arial", 24)),
                TextureFormat.RedGreenBlueAlpha8, false);

            ///////////////////////////////////////////////////////////////////

            IList<Vector3D> positions = _globeShape.ComputeCurve(
                _p, _q, Trig.ToRadians(_granularityInDegrees));

            _sampledPoints.Clear();
            _sampledPoints.Add(new Billboard() { Position = positions[0], Color = Color.Orange });
            _sampledPoints.Add(new Billboard() { Position = positions[positions.Count - 1], Color = Color.Orange });

            for (int i = 1; i < positions.Count - 1; ++i)
            {
                _sampledPoints.Add(new Billboard() 
                { 
                    Position = positions[i], 
                    Color = Color.Yellow 
                });
            }

            ///////////////////////////////////////////////////////////////////

            _ellipsoid.Shape = _globeShape;

            ///////////////////////////////////////////////////////////////////
            
            VertexAttributeFloatVector3 positionAttribute = new VertexAttributeFloatVector3("position", positions.Count);
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", positions.Count);

            for (int i = 0; i < positions.Count; ++i)
            {
                positionAttribute.Values.Add(positions[i].ToVector3F());
                colorAttribute.AddColor(Color.Red);
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.LineStrip;
            mesh.Attributes.Add(positionAttribute);
            mesh.Attributes.Add(colorAttribute);

            _polyline.Set(_window.Context, mesh);

            ///////////////////////////////////////////////////////////////////

            double scale = 1.25 * _globeShape.Radii.MaximumComponent;
            _plane.XAxis = scale * _p.Normalize();
            _plane.YAxis = scale * _p.Cross(_q).Cross(_p).Normalize();
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

            _ellipsoid.Render(context, _sceneState);
            _polyline.Render(context, _sceneState);
            _sampledPoints.Render(context, _sceneState);
            _plane.Render(context, _sceneState);
            _instructions.Render(context, _sceneState);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right) ||
                (e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down))
            {
                if (e.Key == KeyboardKey.Left)
                {
                    _granularityInDegrees = Math.Max(_granularityInDegrees - 1.0, 1.0);
                }
                else if (e.Key == KeyboardKey.Right)
                {
                    _granularityInDegrees = Math.Min(_granularityInDegrees + 1.0, 30.0);
                }
                else if (e.Key == KeyboardKey.Up)
                {
                    _semiMinorAxis = Math.Min(_semiMinorAxis + _semiMinorAxisDelta, 2.0);
                }
                else if (e.Key == KeyboardKey.Down)
                {
                    _semiMinorAxis = Math.Max(_semiMinorAxis - _semiMinorAxisDelta, 0.1);
                }
                SetShape();
            }
            else if ((e.Key == KeyboardKey.Number1) ||
                     (e.Key == KeyboardKey.Keypad1))
            {
                _sampledPoints.Show = !_sampledPoints.Show;
            }
            else if ((e.Key == KeyboardKey.Number2) ||
                     (e.Key == KeyboardKey.Keypad2))
            {
                _polyline.Show = !_polyline.Show;
            }
            else if ((e.Key == KeyboardKey.Number3) || 
                     (e.Key == KeyboardKey.Keypad3))
            {
                _plane.Show = !_plane.Show;
            }

            CreateScene();
        }

        private void SetShape()
        {
            _globeShape = new Ellipsoid(
                Ellipsoid.ScaledWgs84.Radii.X,
                Ellipsoid.ScaledWgs84.Radii.Y,
                _semiMinorAxis);
            _p = _globeShape.ToVector3D(new Geodetic2D(Trig.ToRadians(40), Trig.ToRadians(40)));
            _q = _globeShape.ToVector3D(new Geodetic2D(Trig.ToRadians(120), Trig.ToRadians(-30)));
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _camera.Dispose();
            _instructions.Dispose();
            _ellipsoid.Dispose();
            _sampledPoints.Dispose();
            _sampledPoints.Texture.Dispose();
            _polyline.Dispose();
            _plane.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (Curves example = new Curves())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;

        private readonly Texture2D _texture;
        private readonly HeadsUpDisplay _instructions;
        private readonly RayCastedGlobe _ellipsoid;
        private readonly BillboardCollection _sampledPoints;
        private readonly Polyline _polyline;
        private readonly Plane _plane;

        private Ellipsoid _globeShape;
        private Vector3D _p;
        private Vector3D _q;

        private double _semiMinorAxis;
        private const double _semiMinorAxisDelta = 0.025;
        private double _granularityInDegrees = 5.0;
    }
}