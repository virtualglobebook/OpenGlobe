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

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 mg_LightPosition;

                  void main()                     
                  {
                        gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = mg_LightPosition - worldPosition;
                        positionToEye = mg_CameraEye - worldPosition;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec4 fragColor;

                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform sampler2D mg_Texture0;

                  void main()
                  {
                      vec3 toLight = normalize(positionToLight);
                      vec3 toEye = normalize(positionToEye);

                      vec3 normal = normalize(worldPosition);
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, mg_DiffuseSpecularAmbientShininess.w);

                      float intensity = 
                         (mg_DiffuseSpecularAmbientShininess.x * diffuse) +
                         (mg_DiffuseSpecularAmbientShininess.y * specular) +
                         mg_DiffuseSpecularAmbientShininess.z;

                      vec2 textureCoordinate = vec2(atan2(normal.y, normal.x) / mg_TwoPi + 0.5, asin(normal.z) / mg_Pi + 0.5);
                      fragColor = vec4(intensity * texture2D(mg_Texture0, textureCoordinate).rgb, 1.0);
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

#if FBO
            HighResolutionSnapFrameBuffer snapBuffer = new HighResolutionSnapFrameBuffer(context, 6, 600, _sceneState.Camera.AspectRatio);
            _window.Context.Viewport = new Rectangle(0, 0, snapBuffer.WidthInPixels, snapBuffer.HeightInPixels);
            context.Bind(snapBuffer.FrameBuffer);
#endif

            context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.Black, 1, 0);
            context.TextureUnits[0].Texture2D = _texture;
            context.Bind(_renderState);
            context.Bind(_sp);
            context.Bind(_va);
            context.Draw(_primitiveType, _sceneState);

#if FBO
            snapBuffer.SaveColorBuffer(@"c:\color.tif");
            snapBuffer.SaveDepthBuffer(@"c:\depth.tif");
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