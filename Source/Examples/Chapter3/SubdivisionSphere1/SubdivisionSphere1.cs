#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
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

namespace MiniGlobe.Examples.Chapter3.SubdivisionSphere1
{
    sealed class SubdivisionSphere1 : IDisposable
    {
        public SubdivisionSphere1()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 3:  Subdivision Sphere 1");
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
                  out vec4 fragmentColor;

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

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan2(normal.y, normal.x) / mg_twoPi + 0.5, asin(normal.z) / mg_pi + 0.5);
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                      fragmentColor = vec4(intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb, 1.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = SubdivisionSphereTessellatorSimple.Compute(5);
            _va = _window.Context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            _renderState = new RenderState();
            //_renderState.RasterizationMode = RasterizationMode.Line;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            ///////////////////////////////////////////////////////////////////

            Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
            //Bitmap bitmap = new Bitmap("world_topo_bathy_200411_3x5400x2700.jpg");
            //Bitmap bitmap = new Bitmap("world.topo.200412.3x5400x2700.jpg");
            _texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

            _sceneState.Camera.ZoomToTarget(1);
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
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);

#if FBO
            snapBuffer.SaveColorBuffer(@"E:\Dropbox\My Dropbox\Book\Manuscript\GlobeRendering\Figures\GeographicGridEllipsoidTessellationPol.png");
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
            using (SubdivisionSphere1 example = new SubdivisionSphere1())
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