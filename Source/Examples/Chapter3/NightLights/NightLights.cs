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
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Examples.Chapter3.NightLights
{
    sealed class NightLights : IDisposable
    {
        public NightLights()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Night Lights");
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
                  uniform vec3 mg_sunPosition;

                  void main()                     
                  {
                        gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = mg_sunPosition - worldPosition;
                        positionToEye = mg_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec3 fragmentColor;

                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform sampler2D mg_texture0;                    // Day
                  uniform sampler2D mg_texture1;                    // Night

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
                      return vec2(atan2(normal.y, normal.x) / mg_twoPi + 0.5, asin(normal.z) / mg_pi + 0.5);
                  }

                  vec3 NightColor(vec3 normal)
                  {
                      return texture(mg_texture1, ComputeTextureCoordinates(normal)).rgb;
                  }

                  vec3 DayColor(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      float intensity = LightIntensity(normal, toLight, toEye, diffuseDot, diffuseSpecularAmbientShininess);
                      return intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      vec3 toLight = normalize(positionToLight);
                      float diffuse = dot(toLight, normal);

                      if (diffuse > u_blendDuration)
                      {
                          fragmentColor = DayColor(normal, toLight, normalize(positionToEye), diffuse, mg_diffuseSpecularAmbientShininess);
                      }
                      else if (diffuse >= -u_blendDuration)
                      {
                          vec3 night = NightColor(normal);
                          vec3 day = DayColor(normal, toLight, normalize(positionToEye), diffuse, mg_diffuseSpecularAmbientShininess);
                          fragmentColor = mix(night, day, (diffuse + u_blendDuration) * u_blendDurationScale);
                      }
                      else
                      {
                          fragmentColor = NightColor(normal);
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            float blendDurationScale = 0.1f;
            (_sp.Uniforms["u_blendDuration"] as Uniform<float>).Value = 0.1f;
            (_sp.Uniforms["u_blendDurationScale"] as Uniform<float>).Value = 1 / (2 * blendDurationScale);

            Mesh mesh = SubdivisionSphereTessellatorSimple.Compute(5);
            _va = _window.Context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            Bitmap dayBitmap = new Bitmap("world.topo.200412.3x5400x2700.jpg");
            _dayTexture = Device.CreateTexture2D(dayBitmap, TextureFormat.RedGreenBlue8, false);
            Bitmap nightBitmap = new Bitmap("land_ocean_ice_lights_2048.jpg");
            _nightTexture = Device.CreateTexture2D(nightBitmap, TextureFormat.RedGreenBlue8, false);

            _sceneState.DiffuseIntensity = 0.5f;
            _sceneState.SpecularIntensity = 0.15f;
            _sceneState.AmbientIntensity = 0.35f;
            _sceneState.Camera.ZoomToTarget(1);
            PersistentView.Execute(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\NightLights.xml", _window, _sceneState.Camera);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;

#if FBO
            HighResolutionSnapFrameBuffer snapBuffer = new HighResolutionSnapFrameBuffer(context, 3, 600, _sceneState.Camera.AspectRatio);
            _window.Context.Viewport = new Rectangle(0, 0, snapBuffer.WidthInPixels, snapBuffer.HeightInPixels);
            context.Bind(snapBuffer.FrameBuffer);
#endif

            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            context.TextureUnits[0].Texture2D = _dayTexture;
            context.TextureUnits[1].Texture2D = _nightTexture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);

#if FBO
            snapBuffer.SaveColorBuffer(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\NightLights.png");
            //snapBuffer.SaveDepthBuffer(@"c:\depth.tif");
            Environment.Exit(0);
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            _nightTexture.Dispose();
            _dayTexture.Dispose();
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
            using (NightLights example = new NightLights())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraGlobeCentered _camera;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly Texture2D _dayTexture;
        private readonly Texture2D _nightTexture;
        private readonly PrimitiveType _primitiveType;
    }
}