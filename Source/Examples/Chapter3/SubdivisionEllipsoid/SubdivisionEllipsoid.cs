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

using OpenTK;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Core;

namespace MiniGlobe.Examples.Chapter3
{
    sealed class SubdivisionEllipsoid : IDisposable
    {
        public SubdivisionEllipsoid()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Subdivision Ellipsoid");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();

            string sphereVS =
                @"#version 150

                  in vec4 position;
                  in vec3 normal;
                  in vec2 textureCoordinate;

                  out vec3 positionToLight;
                  out vec3 positionToEye;
                  out vec3 surfaceNormal;
                  out vec2 surfaceTextureCoordinate;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 mg_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 

                        positionToLight = mg_cameraLightPosition - position.xyz;
                        positionToEye = mg_cameraEye - position.xyz;

                        surfaceNormal = normal;
                        surfaceTextureCoordinate = textureCoordinate;
                  }";

            string sphereFS =
                @"#version 150
                 
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  in vec3 surfaceNormal;
                  in vec2 surfaceTextureCoordinate;

                  out vec3 fragmentColor;

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

                  void main()
                  {
                      vec3 normal = normalize(surfaceNormal);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                      fragmentColor = intensity * texture(mg_texture0, surfaceTextureCoordinate).rgb;
                  }";
            _sphereShaderProgram = Device.CreateShaderProgram(sphereVS, sphereFS);

            string vs =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 
                  }";

            string fs =
                @"#version 150
                 
                  out vec3 fragmentColor;

                  void main()
                  {
                      fragmentColor = vec3(1.0, 0.0, 0.0);
                  }";
            _ellipsoidShaderProgram = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Mesh ellipsoidMesh = SubdivisionEllipsoidTessellator.Compute(Ellipsoid.Wgs84, 5, SubdivisionEllipsoidVertexAttributes.Position);
            _ellipsoidVertexArray = _window.Context.CreateVertexArray(ellipsoidMesh, _ellipsoidShaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh sphereMesh = SubdivisionEllipsoidTessellator.Compute(new Ellipsoid(Ellipsoid.Wgs84.Radii.Z, Ellipsoid.Wgs84.Radii.Z, Ellipsoid.Wgs84.Radii.Z), 5, SubdivisionEllipsoidVertexAttributes.All);
            _sphereVertexArray = _window.Context.CreateVertexArray(sphereMesh, _sphereShaderProgram.VertexAttributes, BufferHint.StaticDraw);

            ///////////////////////////////////////////////////////////////////

            _renderState = new RenderState();
            //_renderState.RasterizationMode = RasterizationMode.Line;
            _renderState.FacetCulling.FrontFaceWindingOrder = ellipsoidMesh.FrontFaceWindingOrder;
            
            ///////////////////////////////////////////////////////////////////

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            ///////////////////////////////////////////////////////////////////
            _sceneState = new SceneState();

            //Vector3d eye = new Vector3d(0, -(float)(Ellipsoid.Wgs84.Radii.Y * 3), 0);
            //Vector3d to = Vector3d.Zero;
            //Vector3d up = Vector3d.UnitZ;
            _sceneState.Camera.Eye = new Vector3D(-(float)(Ellipsoid.Wgs84.Radii.X * 0.5), -(float)(Ellipsoid.Wgs84.Radii.Y * 1.5), 0);
            _sceneState.Camera.Target = new Vector3D(0, -(float)Ellipsoid.Wgs84.Radii.Y, 0);
            _sceneState.Camera.Up = Vector3D.UnitZ;
            _sceneState.Camera.PerspectiveNearPlaneDistance = Ellipsoid.Wgs84.Radii.Y * 0.25;
            _sceneState.Camera.PerspectiveFarPlaneDistance = Ellipsoid.Wgs84.Radii.Y * 5;
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;

            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);

            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.Black, 1, 0);
            context.Bind(_ellipsoidShaderProgram);
            context.Bind(_ellipsoidVertexArray);
            context.Draw(PrimitiveType.Triangles, _sceneState);

            context.Clear(ClearBuffers.DepthBuffer, Color.Black, 1, 0);
            context.Bind(_sphereShaderProgram);
            context.Bind(_sphereVertexArray);
            context.Draw(PrimitiveType.Triangles, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _texture.Dispose();
            _ellipsoidShaderProgram.Dispose();
            _sphereShaderProgram.Dispose();
            _ellipsoidVertexArray.Dispose();
            _sphereVertexArray.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (SubdivisionEllipsoid example = new SubdivisionEllipsoid())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly RenderState _renderState;

        private readonly ShaderProgram _ellipsoidShaderProgram;
        private readonly ShaderProgram _sphereShaderProgram;
        
        private readonly VertexArray _ellipsoidVertexArray;
        private readonly VertexArray _sphereVertexArray;

        private readonly Texture2D _texture;
    }
}