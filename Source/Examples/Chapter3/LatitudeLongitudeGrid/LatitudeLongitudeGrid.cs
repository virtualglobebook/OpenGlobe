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
using System.Collections.Generic;

using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Chapter3.LatitudeLongitudeGrid
{
    ///////////////////////////////////////////////////////////////////////
    public enum IntervalEndPoint
    {
        Open,
        Closed
    }

    // TODO:  Move to core
    // TODO:  Add unit tests
    // TODO:  full struct implementation
    // TODO:  Add POI marker to this example
    // TODO:  Add equator, etc.
    public struct Interval
    {
        public Interval(double minimum, double maximum)
            : this(minimum, maximum, IntervalEndPoint.Closed, IntervalEndPoint.Closed)
        {
        }

        public Interval(double minimum, double maximum, IntervalEndPoint minimumEndPoint, IntervalEndPoint maximumEndPoint)
        {
            if (maximum < minimum)
            {
                throw new ArgumentException("maximum < minimum");
            }

            _minimum = minimum;
            _maximum = maximum;
            _minimumEndPoint = minimumEndPoint;
            _maximumEndPoint = maximumEndPoint;
        }

        public double Minimum { get { return _minimum; } }
        public double Maximum { get { return _maximum; } }
        public IntervalEndPoint MinimumEndPoint { get { return _minimumEndPoint; } }
        public IntervalEndPoint MaximumEndPoint { get { return _maximumEndPoint; } }

        public bool Contains(double value)
        {
            bool satisfiesMinimum = (_minimumEndPoint == IntervalEndPoint.Closed) ? (value >= _minimum) : (value > _minimum);
            bool satisfiesMaximum = (_maximumEndPoint == IntervalEndPoint.Closed) ? (value <= _maximum) : (value < _maximum);

            return satisfiesMinimum && satisfiesMaximum;
        }

        private readonly double _minimum;
        private readonly double _maximum;
        private readonly IntervalEndPoint _minimumEndPoint;
        private readonly IntervalEndPoint _maximumEndPoint;
    }
    ///////////////////////////////////////////////////////////////////////

    class GridResolution
    {
        public GridResolution(Interval interval, Vector2 resolution)
        {
            _interval = interval;
            _resolution = resolution;
        }

        public Interval Interval { get { return _interval;  } }
        public Vector2 Resolution { get { return _resolution;  } }

        private readonly Interval _interval;
        private readonly Vector2 _resolution;
    }
    ///////////////////////////////////////////////////////////////////////

    sealed class LatitudeLongitudeGrid : IDisposable
    {
        public LatitudeLongitudeGrid()
        {
            _globeShape = Ellipsoid.Wgs84;
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Latitude Longitude Grid");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraGlobeCentered(_sceneState.Camera, _window, Ellipsoid.UnitSphere);

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 mg_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = mg_cameraLightPosition - worldPosition;
                        positionToEye = mg_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec3 fragmentColor;

                  uniform vec2 u_gridLineWidth;
                  uniform vec2 u_gridResolution;
                  uniform vec3 u_globeOneOverRadiiSquared;

                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform sampler2D mg_texture0;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, diffuseSpecularAmbientShininess.w);

                      return (diffuseSpecularAmbientShininess.x * diffuse) +
                             (diffuseSpecularAmbientShininess.y * specular) +
                              diffuseSpecularAmbientShininess.z;
                  }

                  vec3 ComputeDeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
                  {
                      return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan2(normal.y, normal.x) / mg_twoPi + 0.5, asin(normal.z) / mg_pi + 0.5);
                  }

                  void main()
                  {
                      vec3 normal = ComputeDeticSurfaceNormal(worldPosition, u_globeOneOverRadiiSquared);
                      vec2 textureCoordinate = ComputeTextureCoordinates(normal);

                      vec2 distanceToLine = mod(textureCoordinate, u_gridResolution);
                      vec2 dx = abs(dFdx(textureCoordinate));
                      vec2 dy = abs(dFdy(textureCoordinate));
                      vec2 dF = vec2(max(dx.s, dy.s), max(dx.t, dy.t)) * u_gridLineWidth;

                      if (any(lessThan(distanceToLine, dF)))
                      {
                          fragmentColor = vec3(1.0, 0.0, 0.0);
                      }
                      else
                      {
                          float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                          fragmentColor = intensity * texture(mg_texture0, textureCoordinate).rgb;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(_globeShape.OneOverRadiiSquared);
            (_sp.Uniforms["u_gridLineWidth"] as Uniform<Vector2>).Value = new Vector2(1, 1);
            _gridResolution = _sp.Uniforms["u_gridResolution"] as Uniform<Vector2>;

            Mesh mesh = GeographicGridEllipsoidTessellator.Compute(_globeShape, 64, 32, GeographicGridEllipsoidVertexAttributes.Position);
            _va = _window.Context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _sceneState.Camera.PerspectiveNearPlaneDistance = 0.01 * _globeShape.MaximumRadius;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * _globeShape.MaximumRadius;
            _sceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);
            PersistentView.Execute(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\LatitudeLongitudeGrid.xml", _window, _sceneState.Camera);

            _gridResolutions = new List<GridResolution>();
            _gridResolutions.Add(new GridResolution(
                new Interval(0, 1000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2(0.005f, 0.005f)));
            _gridResolutions.Add(new GridResolution(
                new Interval(1000000, 2000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2(0.01f, 0.01f)));
            _gridResolutions.Add(new GridResolution(
                new Interval(2000000, 20000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2(0.05f, 0.05f)));
            _gridResolutions.Add(new GridResolution(
                new Interval(20000000, double.MaxValue, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2(0.1f, 0.1f)));
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            double altitude = _sceneState.Camera.Altitude(_globeShape);

            for (int i = 0; i < _gridResolutions.Count; ++i)
            {
                if (_gridResolutions[i].Interval.Contains(altitude))
                {
                    _gridResolution.Value = _gridResolutions[i].Resolution;
                    break;
                }
            }

            Context context = _window.Context;

#if FBO
            HighResolutionSnapFrameBuffer snapBuffer = new HighResolutionSnapFrameBuffer(context, 3, 600, _sceneState.Camera.AspectRatio);
            _window.Context.Viewport = new Rectangle(0, 0, snapBuffer.WidthInPixels, snapBuffer.HeightInPixels);
            context.Bind(snapBuffer.FrameBuffer);
#endif

            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);

#if FBO
            snapBuffer.SaveColorBuffer(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\LatitudeLongitudeGrid.png");
            //snapBuffer.SaveDepthBuffer(@"c:\depth.tif");
            Environment.Exit(0);
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _va.Dispose();
            _sp.Dispose();
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
            using (LatitudeLongitudeGrid example = new LatitudeLongitudeGrid())
            {
                example.Run(30.0);
            }
        }

        private readonly Ellipsoid _globeShape;
        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraGlobeCentered _camera;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
        private readonly Uniform<Vector2> _gridResolution;
        private readonly IList<GridResolution> _gridResolutions;
    }
}