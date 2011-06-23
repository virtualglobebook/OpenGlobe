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
using System.Globalization;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class DepthBufferPrecision : IDisposable
    {
        public DepthBufferPrecision()
        {
            _globeShape = Ellipsoid.Wgs84;
            _nearDistance = 1;
            _cubeRootFarDistance = 300;

            _window = Device.CreateWindow(800, 600, "Chapter 6:  Depth Buffer Precision");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyUp += OnKeyUp;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.45f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.5f;

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _globeShape);

            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);
            PersistentView.Execute(@"E:\Manuscript\DepthBufferPrecision\Figures\DepthBufferFormat.xml", _window, _sceneState.Camera);

            ///////////////////////////////////////////////////////////////////

            _globe = new TessellatedGlobe();
            _globe.Shape = _globeShape;
            _globe.NumberOfSlicePartitions = 64;
            _globe.NumberOfStackPartitions = 32;
            _globe.Texture = Device.CreateTexture2D(new Bitmap("world_topo_bathy_200411_3x5400x2700.jpg"), TextureFormat.RedGreenBlue8, false);
            _globe.Textured = true;

            _plane = new Plane(_window.Context);
            _plane.XAxis = 0.6 * _globeShape.MaximumRadius * Vector3D.UnitX;
            _plane.YAxis = 0.6 * _globeShape.MinimumRadius * Vector3D.UnitZ;
            _plane.OutlineWidth = 3;
            _cubeRootPlaneHeight = 100.0;
            UpdatePlaneOrigin();

            _viewportQuad = new ViewportQuad(_window.Context, null);

            _framebuffer = _window.Context.CreateFramebuffer();
            _depthFormatIndex = 1;
            _depthTestLess = true;
            _logarithmicDepthConstant = 1;
            UpdatePlanesAndDepthTests();

            _clearState = new ClearState();

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Blue;
            UpdateHUD();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;

            UpdateFramebufferAttachments();
        }

        private void OnKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.N)
            {
                _nKeyDown = false;
            }
            else if (e.Key == KeyboardKey.F)
            {
                _fKeyDown = false;
            }
            else if (e.Key == KeyboardKey.C)
            {
                _cKeyDown = false;
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.N)
            {
                _nKeyDown = true;
            }
            else if (e.Key == KeyboardKey.F)
            {
                _fKeyDown = true;
            }
            else if (e.Key == KeyboardKey.C)
            {
                _cKeyDown = true;
            }
            else if (_nKeyDown && ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down)))
            {
                _nearDistance += (e.Key == KeyboardKey.Up) ? 1 : -1;

                UpdatePlanesAndDepthTests();
            }
            else if (_fKeyDown && ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down)))
            {
                _cubeRootFarDistance += (e.Key == KeyboardKey.Up) ? 1 : -1;

                UpdatePlanesAndDepthTests();
            }
            else if ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus) ||
                (e.Key == KeyboardKey.Minus) || (e.Key == KeyboardKey.KeypadMinus))
            {
                _cubeRootPlaneHeight += ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus)) ? 1 : -1;

                UpdatePlaneOrigin();
            }
            else if ((e.Key == KeyboardKey.Left) || (e.Key == KeyboardKey.Right))
            {
                _depthFormatIndex += (e.Key == KeyboardKey.Right) ? 1 : -1;
                if (_depthFormatIndex < 0)
                {
                    _depthFormatIndex = 2;
                }
                else if (_depthFormatIndex > 2)
                {
                    _depthFormatIndex = 0;
                }

                UpdateFramebufferAttachments();
            }
            else if (e.Key == KeyboardKey.D)
            {
                _depthTestLess = !_depthTestLess;

                UpdatePlanesAndDepthTests();
            }
            else if (e.Key == KeyboardKey.L)
            {
                _logarithmicDepthBuffer = !_logarithmicDepthBuffer;

                UpdateLogarithmicDepthBuffer();
            }
            else if (_cKeyDown && ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down)))
            {
                _logarithmicDepthConstant += (e.Key == KeyboardKey.Up) ? 0.1 : -0.1;

                UpdateLogarithmicDepthConstant();
            }

            UpdateHUD();
        }

        private void UpdatePlaneOrigin()
        {
            _plane.Origin = -(_globeShape.MaximumRadius * Vector3D.UnitY +
                (_cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight * Vector3D.UnitY));
        }

        private void UpdateFramebufferAttachments()
        {
            DisposeFramebufferAttachments();
            _colorTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8, false));
            _depthTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, _depthFormats[_depthFormatIndex], false));
            _framebuffer.ColorAttachments[0] = _colorTexture;
            _framebuffer.DepthAttachment = _depthTexture;
            _viewportQuad.Texture = _colorTexture;
        }

        private void UpdatePlanesAndDepthTests()
        {
            double farDistance = _cubeRootFarDistance * _cubeRootFarDistance * _cubeRootFarDistance;

            _sceneState.Camera.PerspectiveNearPlaneDistance = _depthTestLess ? _nearDistance : farDistance;
            _sceneState.Camera.PerspectiveFarPlaneDistance = _depthTestLess ? farDistance : _nearDistance;

            _globe.DepthTestFunction = _depthTestLess ? DepthTestFunction.Less : DepthTestFunction.Greater;
            _plane.DepthTestFunction = _depthTestLess ? DepthTestFunction.Less : DepthTestFunction.Greater;
        }

        private void UpdateLogarithmicDepthBuffer()
        {
            _globe.LogarithmicDepth = _logarithmicDepthBuffer;
            _plane.LogarithmicDepth = _logarithmicDepthBuffer;
        }

        private void UpdateLogarithmicDepthConstant()
        {
            _globe.LogarithmicDepthConstant = (float)_logarithmicDepthConstant;
            _plane.LogarithmicDepthConstant = (float)_logarithmicDepthConstant;
        }

        private void UpdateViewerHeight()
        {
            double height = _sceneState.Camera.Height(_globeShape);
            if (_viewerHeight != height)
            {
                _viewerHeight = height;
                UpdateHUD();
            }
        }

        private void UpdateHUD()
        {
            string text;

            text = "Near Plane: " + string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('n' + up/down)", _nearDistance) + "\n";
            text += "Far Plane: " + string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('f' + up/down)", _cubeRootFarDistance * _cubeRootFarDistance * _cubeRootFarDistance) + "\n";
            text += "Viewer Height: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _viewerHeight) + "\n";
            text += "Plane Height: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight) + " ('-'/'+')\n";
            text += "Depth Test: " + (_depthTestLess ? "less" : "greater") + " ('d')\n";
            text += "Depth Format: " + _depthFormatsStrings[_depthFormatIndex] + " (left/right)\n";
            text += "Logarithmic Depth Buffer: " + (_logarithmicDepthBuffer ? "on" : "off") + " ('l')\n";
            text += "Logarithmic Depth Constant: " + _logarithmicDepthConstant + " ('c' + up/down)";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnRenderFrame()
        {
            UpdateViewerHeight();

            Context context = _window.Context;

            //
            // Render to frame buffer
            //
            context.Framebuffer = _framebuffer;

            _clearState.Depth = _depthTestLess ? 1 : 0;
            context.Clear(_clearState);

            _globe.Render(context, _sceneState);
            _plane.Render(context, _sceneState);

            //
            // Render viewport quad to show contents of frame buffer's color buffer
            //
            context.Framebuffer = null;
            _viewportQuad.Render(context, _sceneState);
            _hud.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _globe.Texture.Dispose();
            _globe.Dispose();
            _plane.Dispose();
            _viewportQuad.Dispose();

            DisposeFramebufferAttachments();
            _framebuffer.Dispose();

            _hudFont.Dispose();
            _hud.Texture.Dispose();
            _hud.Dispose();
            _window.Dispose();
        }

        #endregion

        private void DisposeFramebufferAttachments()
        {
            if (_colorTexture != null)
            {
                _colorTexture.Dispose();
                _colorTexture = null;
            }

            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (DepthBufferPrecision example = new DepthBufferPrecision())
            {
                example.Run(30.0);
            }
        }

        private readonly Ellipsoid _globeShape;
        private double _nearDistance;
        private double _cubeRootFarDistance;

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly TessellatedGlobe _globe;
        private readonly Plane _plane;
        private double _cubeRootPlaneHeight;
        private double _viewerHeight;
        private readonly ViewportQuad _viewportQuad;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private readonly Framebuffer _framebuffer;
        private int _depthFormatIndex;
        private bool _depthTestLess;
        private bool _logarithmicDepthBuffer;
        private double _logarithmicDepthConstant;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _hud;

        private bool _nKeyDown;
        private bool _fKeyDown;
        private bool _cKeyDown;

        private readonly TextureFormat[] _depthFormats = new TextureFormat[]
        {
            TextureFormat.Depth16,
            TextureFormat.Depth24,
            TextureFormat.Depth32f
        };
        private readonly string[] _depthFormatsStrings = new string[]
        {
            "16-bit fixed point",
            "24-bit fixed point",
            "32-bit floating point",
        };
    }
}