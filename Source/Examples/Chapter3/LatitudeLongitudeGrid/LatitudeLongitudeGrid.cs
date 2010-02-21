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
using System.Collections.Generic;

using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Chapter3
{
    sealed class LatitudeLongitudeGrid : IDisposable
    {
        public LatitudeLongitudeGrid()
        {
            _globeShape = Ellipsoid.Wgs84;
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Latitude Longitude Grid");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);

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

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState.Camera);
            snap.ColorFilename = @"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\LatitudeLongitudeGrid.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
            snap.ExitAfterSnap = true;
            snap.Enabled = false;

            _gridResolutions = new List<GridResolution>();
            _gridResolutions.Add(new GridResolution(
                new Interval(0, 1000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2d(0.005, 0.005)));
            _gridResolutions.Add(new GridResolution(
                new Interval(1000000, 2000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2d(0.01, 0.01)));
            _gridResolutions.Add(new GridResolution(
                new Interval(2000000, 20000000, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2d(0.05, 0.05)));
            _gridResolutions.Add(new GridResolution(
                new Interval(20000000, double.MaxValue, IntervalEndPoint.Closed, IntervalEndPoint.Open),
                new Vector2d(0.1, 0.1)));

            Vector3[] positions = new[]
            {
                Conversion.ToVector3(_globeShape.DeticToVector3d(Trig.ToRadians(-123.06), Trig.ToRadians(49.13), 0)), // Vancouver, B.C., Can.
            };
            _billboard = new BillboardGroup(_window.Context, positions, new Bitmap(@"building.png"));
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
                    _gridResolution.Value = Conversion.ToVector2(_gridResolutions[i].Resolution);
                    break;
                }
            }

            Context context = _window.Context;
            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);

            _billboard.Render(_sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _va.Dispose();
            _sp.Dispose();
            _camera.Dispose();
            _window.Dispose();
            _billboard.Dispose();
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
        private readonly CameraLookAtPoint _camera;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
        private readonly Uniform<Vector2> _gridResolution;
        private readonly IList<GridResolution> _gridResolutions;
        private readonly BillboardGroup _billboard;
    }
}