#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Drawing;

using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Examples.Chapter3.SubdivisionSphere2
{
    sealed class SubdivisionSphere2 : IDisposable
    {
        public SubdivisionSphere2()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Subdivision Sphere 2");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraGlobeCentered(_sceneState.Camera, _window, Ellipsoid.UnitSphere);

            string vs =
                @"#version 150

                  in vec4 position;
                  in vec3 normal;
                  in vec2 textureCoordinate;

                  out vec3 positionToLight;
                  out vec3 positionToEye;
                  out vec3 surfaceNormal;
                  out vec2 surfaceTextureCoordinate;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 mg_CameraLightPosition;

                  void main()                     
                  {
                        gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 

                        positionToLight = mg_CameraLightPosition - position.xyz;
                        positionToEye = mg_CameraEye - position.xyz;

                        surfaceNormal = normal;
                        surfaceTextureCoordinate = textureCoordinate;
                  }";
            string fs =
                @"#version 150
                 
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  in vec3 surfaceNormal;
                  in vec2 surfaceTextureCoordinate;

                  out vec4 fragColor;

                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform sampler2D mg_Texture0;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, mg_DiffuseSpecularAmbientShininess.w);

                      return (mg_DiffuseSpecularAmbientShininess.x * diffuse) +
                             (mg_DiffuseSpecularAmbientShininess.y * specular) +
                              mg_DiffuseSpecularAmbientShininess.z;
                  }

                  void main()
                  {
                      vec3 normal = normalize(surfaceNormal);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_DiffuseSpecularAmbientShininess);
                      fragColor = vec4(intensity * texture(mg_Texture0, surfaceTextureCoordinate).rgb, 1.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            Mesh mesh = SubdivisionSphereTessellator.Compute(5, SubdivisionSphereVertexAttributes.All);
            _va = _window.Context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            
            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);
        }

        public void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
            _sceneState.Camera.ZoomToTarget(1);
        }

        public void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.Black, 1, 0);
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);
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
            using (SubdivisionSphere2 example = new SubdivisionSphere2())
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
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
    }
}