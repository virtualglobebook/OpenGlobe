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

using OpenGlobe.Core.Geometry;
using OpenGlobe.Core.Tessellation;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples.Chapter3
{
    sealed class SubdivisionSphere2 : IDisposable
    {
        public SubdivisionSphere2()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Subdivision Sphere 2");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _clearState = new ClearState();

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  layout(location = og_normalVertexLocation) in vec3 normal;
                  layout(location = og_textureCoordinateVertexLocation) in vec2 textureCoordinate;

                  out vec3 positionToLight;
                  out vec3 positionToEye;
                  out vec3 surfaceNormal;
                  out vec2 surfaceTextureCoordinate;

                  uniform mat4 og_modelViewPerspectiveMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 

                        positionToLight = og_cameraLightPosition - position.xyz;
                        positionToEye = og_cameraEye - position.xyz;

                        surfaceNormal = normal;
                        surfaceTextureCoordinate = textureCoordinate;
                  }";
            string fs =
                @"#version 330
                 
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  in vec3 surfaceNormal;
                  in vec2 surfaceTextureCoordinate;

                  out vec3 fragmentColor;

                  uniform vec4 og_diffuseSpecularAmbientShininess;
                  uniform sampler2D og_texture0;

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

                  void main()
                  {
                      vec3 normal = normalize(surfaceNormal);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), og_diffuseSpecularAmbientShininess);
                      fragmentColor = intensity * texture(og_texture0, surfaceTextureCoordinate).rgb;
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);

            Mesh mesh = SubdivisionSphereTessellator.Compute(5, SubdivisionSphereVertexAttributes.All);
            VertexArray va = _window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            RenderState renderState = new RenderState();
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
            _sceneState.Camera.ZoomToTarget(1);
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);
            context.TextureUnits[0].Texture2D = _texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClampToEdge;
            context.Draw(_primitiveType, _drawState, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
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
            using (SubdivisionSphere2 example = new SubdivisionSphere2())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;
        private readonly DrawState _drawState;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
    }
}