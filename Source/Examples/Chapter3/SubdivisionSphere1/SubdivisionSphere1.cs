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

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class SubdivisionSphere1 : IDisposable
    {
        public SubdivisionSphere1()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Subdivision Sphere 1");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _clearState = new ClearState();

            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 og_modelViewPerspectiveMatrix;
                  uniform vec3 og_cameraEye;
                  uniform vec3 og_cameraLightPosition;

                  void main()                     
                  {
                        gl_Position = og_modelViewPerspectiveMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = og_cameraLightPosition - worldPosition;
                        positionToEye = og_cameraEye - worldPosition;
                  }";

            string fs =
                @"#version 330
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
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

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan(normal.y, normal.x) * og_oneOverTwoPi + 0.5, asin(normal.z) * og_oneOverPi + 0.5);
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), og_diffuseSpecularAmbientShininess);
                      fragmentColor = intensity * texture(og_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }";
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = SubdivisionSphereTessellatorSimple.Compute(5);
            VertexArray va = _window.Context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            RenderState renderState = new RenderState();
            //_renderState.RasterizationMode = RasterizationMode.Line;
            renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _drawState = new DrawState(renderState, sp, va);

            ///////////////////////////////////////////////////////////////////

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            //Bitmap bitmap = new Bitmap("world_topo_bathy_200411_3x5400x2700.jpg");
            //Bitmap bitmap = new Bitmap("world.topo.200412.3x5400x2700.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _sceneState.Camera.ZoomToTarget(1);

            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\GlobeRendering\Figures\GeographicGridEllipsoidTessellationPol.png";
            snap.WidthInInches = 3;
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
            context.TextureUnits[0].Texture = _texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
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
            using (SubdivisionSphere1 example = new SubdivisionSphere1())
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