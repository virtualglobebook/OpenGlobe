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
using System.Globalization;

using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Examples.Chapter4
{
    sealed class DepthBufferPrecision : IDisposable
    {
        public DepthBufferPrecision()
        {
            _globeShape = Ellipsoid.Wgs84;
            _nearDistance = 1;
            _cubeRootFarDistance = 300;

            _window = Device.CreateWindow(800, 600, "Chapter 4:  Depth Buffer Precision");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyUp += OnKeyUp;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();

            ///////////////////////////////////////////////////////////////////

            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, _globeShape);
            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);

            PersistentView.Execute(@"E:\Manuscript\DepthBufferPrecision\Figures\DepthBufferPrecisionNear.xml", _window, _sceneState.Camera);

            ///////////////////////////////////////////////////////////////////

            Context context = _window.Context;

            _globe = new TessellatedGlobe(_window.Context);
            _globe.Shape = _globeShape;
            _globe.Texture = Device.CreateTexture2D(new Bitmap("MapperWDB.jpg"), TextureFormat.RedGreenBlue8, false);

            _plane = new Plane(_window.Context);
            _plane.XAxis = 0.6 * _globeShape.MaximumRadius * Vector3D.UnitX;
            _plane.YAxis = 0.6 * _globeShape.MinimumRadius * Vector3D.UnitZ;
            _plane.OutlineWidth = 3;
            _cubeRootPlaneHeight = 100.0;
            UpdatePlaneOrigin();

            _viewportQuad = new ViewportQuad(_window.Context);

            _frameBuffer = _window.Context.CreateFrameBuffer();
            _depthFormatIndex = 1;
            _depthTestLess = true;
            UpdatePlanesAndDepthTests();

            ///////////////////////////////////////////////////////////////////

            _hudFont = new Font("Arial", 16);

            _nearDistanceHUD = new HeadsUpDisplay(_window.Context);
            _nearDistanceHUD.Color = Color.Black;
            _nearDistanceHUD.Position = new Vector2D(0, 172);
            UpdateNearDistanceHUD();

            _farDistanceHUD = new HeadsUpDisplay(_window.Context);
            _farDistanceHUD.Color = Color.Black;
            _farDistanceHUD.Position = new Vector2D(0, 148);
            UpdateFarDistanceHUD();
            
            _viewerHeightHUD = new HeadsUpDisplay(_window.Context);
            _viewerHeightHUD.Color = Color.Black;
            _viewerHeightHUD.Position = new Vector2D(0, 120);
            UpdateViewerHeightHUD();

            _planeHeightHUD = new HeadsUpDisplay(_window.Context);
            _planeHeightHUD.Color = Color.Black;
            _planeHeightHUD.Position = new Vector2D(0, 96);
            UpdatePlaneHeightHUD();

            _depthFormatHUD = new HeadsUpDisplay(_window.Context);
            _depthFormatHUD.Color = Color.Black;
            _depthFormatHUD.Position = new Vector2D(0, 72);
            UpdateDepthFormatHUD();

            _depthTestHUD = new HeadsUpDisplay(_window.Context);
            _depthTestHUD.Color = Color.Black;
            _depthTestHUD.Position = new Vector2D(0, 48);
            UpdateDepthTestHUD();

            _logarithmicDepthBufferHUD = new HeadsUpDisplay(_window.Context);
            _logarithmicDepthBufferHUD.Color = Color.Black;
            _logarithmicDepthBufferHUD.Position = new Vector2D(0, 24);
            UpdateLogarithmicDepthBufferHUD();

            _logarithmicDepthConstantHUD = new HeadsUpDisplay(_window.Context);
            _logarithmicDepthConstantHUD.Color = Color.Black;
            _logarithmicDepthConstant = 1;
            UpdateLogarithmicDepthConstantHUD();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;

            UpdateFrameBufferAttachments();
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
                UpdateNearDistanceHUD();
            }
            else if (_fKeyDown && ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down)))
            {
                _cubeRootFarDistance += (e.Key == KeyboardKey.Up) ? 1 : -1;

                UpdatePlanesAndDepthTests();
                UpdateFarDistanceHUD();
            }
            else if ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus) ||
                (e.Key == KeyboardKey.Minus) || (e.Key == KeyboardKey.KeypadMinus))
            {
                _cubeRootPlaneHeight += ((e.Key == KeyboardKey.Plus) || (e.Key == KeyboardKey.KeypadPlus)) ? 1 : -1;

                UpdatePlaneOrigin();
                UpdatePlaneHeightHUD();
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

                UpdateFrameBufferAttachments();
                UpdateDepthFormatHUD();
            }
            else if (e.Key == KeyboardKey.D)
            {
                _depthTestLess = !_depthTestLess;

                UpdatePlanesAndDepthTests();
                UpdateDepthTestHUD();
            }
            else if (e.Key == KeyboardKey.L)
            {
                _logarithmicDepthBuffer = !_logarithmicDepthBuffer;

                UpdateLogarithmicDepthBuffer();
                UpdateLogarithmicDepthBufferHUD();
            }
            else if (_cKeyDown && ((e.Key == KeyboardKey.Up) || (e.Key == KeyboardKey.Down)))
            {
                _logarithmicDepthConstant += (e.Key == KeyboardKey.Up) ? 0.1 : -0.1;

                UpdateLogarithmicDepthConstant();
                UpdateLogarithmicDepthConstantHUD();
            }
        }

        private void UpdatePlaneOrigin()
        {
            _plane.Origin = -(_globeShape.MaximumRadius * Vector3D.UnitY +
                (_cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight * Vector3D.UnitY));
        }

        private void UpdateFrameBufferAttachments()
        {
            DisposeFrameBufferAttachments();
            _colorTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8, false));
            _depthTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, _depthFormats[_depthFormatIndex], false));
            _frameBuffer.ColorAttachments[0] = _colorTexture;
            _frameBuffer.DepthAttachment = _depthTexture;
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

        private void UpdateNearDistanceHUD()
        {
            UpdateHUDTexture(_nearDistanceHUD, "Near Plane: " + 
                string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('n' + up/down)", _nearDistance));
        }

        private void UpdateFarDistanceHUD()
        {
            UpdateHUDTexture(_farDistanceHUD, "Far Plane: " + string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('f' + up/down)",
                _cubeRootFarDistance * _cubeRootFarDistance * _cubeRootFarDistance));
        }

        private void UpdateViewerHeightHUD()
        {
            double height = _sceneState.Camera.Altitude(_globeShape);
            if (_viewerHeight != height)
            {
                UpdateHUDTexture(_viewerHeightHUD, "Viewer Height: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", height));
                _viewerHeight = height;
            }
        }

        private void UpdatePlaneHeightHUD()
        {
            UpdateHUDTexture(_planeHeightHUD, "Plane Height: " +
                string.Format(CultureInfo.CurrentCulture, "{0:N}", _cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight) + " ('-'/'+')");
        }

        private void UpdateDepthTestHUD()
        {
            UpdateHUDTexture(_depthTestHUD, "Depth Test: " + (_depthTestLess ? "less" : "greater") + " ('d')");
        }

        private void UpdateDepthFormatHUD()
        {
            UpdateHUDTexture(_depthFormatHUD, "Depth Format: " + _depthFormatsStrings[_depthFormatIndex] + " (left/right)");
        }

        private void UpdateLogarithmicDepthBufferHUD()
        {
            UpdateHUDTexture(_logarithmicDepthBufferHUD, "Logarithmic Depth Buffer: " + (_logarithmicDepthBuffer ? "on" : "off") + " ('l')");
        }

        private void UpdateLogarithmicDepthConstantHUD()
        {
            UpdateHUDTexture(_logarithmicDepthConstantHUD, "Logarithmic Depth Constant: " + _logarithmicDepthConstant + " ('c' + up/down)");
        }

        private void UpdateHUDTexture(HeadsUpDisplay hud, string text)
        {
            if (hud.Texture != null)
            {
                hud.Texture.Dispose();
                hud.Texture = null;
            }

            hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void OnRenderFrame()
        {
            UpdateViewerHeightHUD();

            Context context = _window.Context;

            //
            // Render to frame buffer
            //
            context.Bind(_frameBuffer);
            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, _depthTestLess ? 1 : 0, 0);
            _globe.Render(_sceneState);
            _plane.Render(_sceneState);

            _nearDistanceHUD.Render(_sceneState);
            _farDistanceHUD.Render(_sceneState);
            _viewerHeightHUD.Render(_sceneState);
            _planeHeightHUD.Render(_sceneState);
            _depthFormatHUD.Render(_sceneState);
            _depthTestHUD.Render(_sceneState);
            _logarithmicDepthBufferHUD.Render(_sceneState);
            _logarithmicDepthConstantHUD.Render(_sceneState);

            //
            // Render viewport quad to show contents of frame buffer's color buffer
            //
            context.Bind(null as FrameBuffer);
            _viewportQuad.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _window.Dispose();
            _camera.Dispose();
            _globe.Texture.Dispose();
            _globe.Dispose();
            _plane.Dispose();
            _viewportQuad.Dispose();

            DisposeFrameBufferAttachments();
            _frameBuffer.Dispose();

            _hudFont.Dispose();
            _nearDistanceHUD.Texture.Dispose();
            _nearDistanceHUD.Dispose();
            _farDistanceHUD.Texture.Dispose();
            _farDistanceHUD.Dispose();
            _viewerHeightHUD.Texture.Dispose();
            _viewerHeightHUD.Dispose();
            _planeHeightHUD.Texture.Dispose();
            _planeHeightHUD.Dispose();
            _depthFormatHUD.Texture.Dispose();
            _depthFormatHUD.Dispose();
            _depthTestHUD.Texture.Dispose();
            _depthTestHUD.Dispose();
            _logarithmicDepthBufferHUD.Texture.Dispose();
            _logarithmicDepthBufferHUD.Dispose();
            _logarithmicDepthConstantHUD.Texture.Dispose();
            _logarithmicDepthConstantHUD.Dispose();
        }

        #endregion

        private void DisposeFrameBufferAttachments()
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

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TessellatedGlobe _globe;
        private readonly Plane _plane;
        private double _cubeRootPlaneHeight;
        private double _viewerHeight;
        private readonly ViewportQuad _viewportQuad;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private readonly FrameBuffer _frameBuffer;
        private int _depthFormatIndex;
        private bool _depthTestLess;
        private bool _logarithmicDepthBuffer;
        private double _logarithmicDepthConstant;

        private readonly Font _hudFont;
        private readonly HeadsUpDisplay _nearDistanceHUD;
        private readonly HeadsUpDisplay _farDistanceHUD;
        private readonly HeadsUpDisplay _viewerHeightHUD;
        private readonly HeadsUpDisplay _planeHeightHUD;
        private readonly HeadsUpDisplay _depthFormatHUD;
        private readonly HeadsUpDisplay _depthTestHUD;
        private readonly HeadsUpDisplay _logarithmicDepthBufferHUD;
        private readonly HeadsUpDisplay _logarithmicDepthConstantHUD;

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