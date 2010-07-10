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

using OpenGlobe.Core.Geometry;
using OpenGlobe.Core.Tessellation;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples.Chapter3
{
    sealed class NightLights : IDisposable
    {
        public NightLights()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Night Lights");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.ScaledWgs84);
            _clearState = new ClearState();

            string vs =
                @"#version 330

                  in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 og_modelViewPerspectiveProjectionMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_sunPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveProjectionMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = og_sunPosition - worldPosition;
                        positionToEye = og_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec3 fragmentColor;

                  uniform vec4 og_diffuseSpecularAmbientShininess;
                  uniform sampler2D og_texture0;                    // Day
                  uniform sampler2D og_texture1;                    // Night

                  uniform float u_blendDuration;
                  uniform float u_blendDurationScale;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(diffuseDot, 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, diffuseSpecularAmbientShininess.w);

                      return (diffuseSpecularAmbientShininess.x * diffuse) +
                             (diffuseSpecularAmbientShininess.y * specular) +
                              diffuseSpecularAmbientShininess.z;
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan(normal.y, normal.x) * og_oneOverTwoPi + 0.5, asin(normal.z) * og_oneOverPi + 0.5);
                  }

                  vec3 NightColor(vec3 normal)
                  {
                      return texture(og_texture1, ComputeTextureCoordinates(normal)).rgb;
                  }

                  vec3 DayColor(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      float intensity = LightIntensity(normal, toLight, toEye, diffuseDot, diffuseSpecularAmbientShininess);
                      return intensity * texture(og_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      vec3 toLight = normalize(positionToLight);
                      float diffuse = dot(toLight, normal);

                      if (diffuse > u_blendDuration)
                      {
                          fragmentColor = DayColor(normal, toLight, normalize(positionToEye), diffuse, og_diffuseSpecularAmbientShininess);
                      }
                      else if (diffuse < -u_blendDuration)
                      {
                          fragmentColor = NightColor(normal);
                      }
                      else
                      {
                          vec3 night = NightColor(normal);
                          vec3 day = DayColor(normal, toLight, normalize(positionToEye), diffuse, og_diffuseSpecularAmbientShininess);
                          fragmentColor = mix(night, day, (diffuse + u_blendDuration) * u_blendDurationScale);
                      }
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);

            float blendDurationScale = 0.1f;
            (sp.Uniforms["u_blendDuration"] as Uniform<float>).Value = blendDurationScale;
            (sp.Uniforms["u_blendDurationScale"] as Uniform<float>).Value = 1 / (2 * blendDurationScale);

            Mesh mesh = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.ScaledWgs84, 5, SubdivisionEllipsoidVertexAttributes.Position);
            VertexArray va = _window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            RenderState renderState = new RenderState();
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

            Bitmap dayBitmap = new Bitmap("world.topo.200412.3x5400x2700.jpg");
            _dayTexture = Device.CreateTexture2D(dayBitmap, TextureFormat.RedGreenBlue8, false);
            Bitmap nightBitmap = new Bitmap("land_ocean_ice_lights_2048.jpg");
            _nightTexture = Device.CreateTexture2D(nightBitmap, TextureFormat.RedGreenBlue8, false);

            _sceneState.DiffuseIntensity = 0.5f;
            _sceneState.SpecularIntensity = 0.15f;
            _sceneState.AmbientIntensity = 0.35f;
            _sceneState.Camera.ZoomToTarget(1);
            PersistentView.Execute(@"E:\Manuscript\GlobeRendering\Figures\NightLights.xml", _window, _sceneState.Camera);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\GlobeRendering\Figures\NightLightsPointTwo.png";
            snap.WidthInInches = 2;
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
            context.TextureUnits[0].Texture2D = _dayTexture;
            context.TextureUnits[1].Texture2D = _nightTexture;
            context.Draw(_primitiveType, _drawState, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _nightTexture.Dispose();
            _dayTexture.Dispose();
            _drawState.VertexArray.Dispose();
            _drawState.ShaderProgram.Dispose();
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
            using (NightLights example = new NightLights())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly DrawState _drawState;
        private readonly Texture2D _dayTexture;
        private readonly Texture2D _nightTexture;
        private readonly PrimitiveType _primitiveType;
    }
}