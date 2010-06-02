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

using MiniGlobe.Core.Geometry;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

using MiniGlobe.Core;
using MiniGlobe.Terrain;

namespace MiniGlobe.Examples.Chapter8
{
    sealed class LinesOnTerrain : IDisposable
    {
        public LinesOnTerrain()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 8:  Lines on Terrain");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.Camera.PerspectiveNearPlaneDistance = 10;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.jpg"));
            _tile = new TriangleMeshTerrainTile(_window.Context, terrainTile);
            const double heightExaggeration = 30.0;
            _tile.HeightExaggeration = (float)heightExaggeration;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Size.X, terrainTile.Size.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Size.X * 0.5, terrainTile.Size.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);
            
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\LinesOnTerrain.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;
            
            //
            // Depth
            //
            CreateDepth();

            //
            // Line on terrain
            //
            // Pass-thru shader
            //
            string vs;
            string fs;

            vs =
                @"#version 150

                in vec3 position;
                  
                uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                void main()
                {
                    gl_Position = mg_modelViewPerspectiveProjectionMatrix * vec4(position, 1.0);
                }";
            fs =
                @"#version 150
                 
                out vec3 fragmentColor;

                uniform sampler2D mg_texture0;
                uniform vec2 mg_inverseViewportDimensions;

                void main()
                {
                    vec2 of = mg_inverseViewportDimensions * gl_FragCoord.xy;
                    float center = texture(mg_texture0, of).r;
                    if (gl_FragCoord.z < center)
                    {
                        fragmentColor = vec3(1.0, 1.0, 0.0);
                    }
                    else
                    {
                        discard;
                    }
                }";
            _passThruSP = Device.CreateShaderProgram(vs, fs);

            vs =
                @"#version 150

                uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                in vec3 position;

                void main()
                {
                    gl_Position = mg_modelViewPerspectiveProjectionMatrix * vec4(position, 1.0);
                }";
#if false
            fs =
                @"#version 150
                
                uniform sampler2D mg_texture0;
                uniform vec2 mg_inverseViewportDimensions;
 
                out vec3 fragmentColor;

                void main()
                {
                    vec2 of = mg_inverseViewportDimensions * gl_FragCoord.xy;
                    float center = textureOffset(mg_texture0, of, ivec2(0, 0)).r;
                    if (gl_FragCoord.z < center)
                    {
                        //
                        // Fragment is above the terrain
                        //
                        float upperLeft = textureOffset(mg_texture0, of, ivec2(-1.0, 1.0)).r;
                        float upperCenter = textureOffset(mg_texture0, of, ivec2(0.0, 1.0)).r;
                        float upperRight = textureOffset(mg_texture0, of, ivec2(1.0, 1.0)).r;
                        float left = textureOffset(mg_texture0, of, ivec2(-1.0, 0.0)).r;
                        float right = textureOffset(mg_texture0, of, ivec2(1.0, 0.0)).r;
                        float lowerLeft = textureOffset(mg_texture0, of, ivec2(-1.0, -1.0)).r;
                        float lowerCenter = textureOffset(mg_texture0, of, ivec2(0.0, -1.0)).r;
                        float lowerRight = textureOffset(mg_texture0, of, ivec2(1.0, -1.0)).r;
                        if ((step(upperLeft, gl_FragCoord.z) > 0.0) ||
                            (step(upperCenter, gl_FragCoord.z) > 0.0) ||
                            (step(upperRight, gl_FragCoord.z) > 0.0) ||
                            (step(left, gl_FragCoord.z) > 0.0) ||
                            (step(right, gl_FragCoord.z) > 0.0) ||
                            (step(lowerLeft, gl_FragCoord.z) > 0.0) ||
                            (step(lowerCenter, gl_FragCoord.z) > 0.0) ||
                            (step(lowerRight, gl_FragCoord.z) > 0.0))
                        {
                            fragmentColor = vec3(1.0, 1.0, 0.0);
                        }
                        else
                        {
                            discard;
                        }
                    }
                    else
                    {
                        discard;
                    }
                }";
#else
            fs =
                @"#version 150
                
                uniform sampler2D mg_texture0;
                uniform vec2 mg_inverseViewportDimensions;
 
                out vec3 fragmentColor;

                void main()
                {
                    float invDepth = 1.0 / gl_FragCoord.z;
                    vec2 dInvDepth = vec2(dFdx(invDepth), dFdy(invDepth));

                    vec2 of = mg_inverseViewportDimensions * gl_FragCoord.xy;
                    float center = texture(mg_texture0, of).r;
                    if (gl_FragCoord.z < center)
                    {
                        //
                        // Fragment is above the terrain
                        //
                        float upperLeft = textureOffset(mg_texture0, of, ivec2(-1.0, 1.0)).r;
                        float upperCenter = textureOffset(mg_texture0, of, ivec2(0.0, 1.0)).r;
                        float upperRight = textureOffset(mg_texture0, of, ivec2(1.0, 1.0)).r;
                        float left = textureOffset(mg_texture0, of, ivec2(-1.0, 0.0)).r;
                        float right = textureOffset(mg_texture0, of, ivec2(1.0, 0.0)).r;
                        float lowerLeft = textureOffset(mg_texture0, of, ivec2(-1.0, -1.0)).r;
                        float lowerCenter = textureOffset(mg_texture0, of, ivec2(0.0, -1.0)).r;
                        float lowerRight = textureOffset(mg_texture0, of, ivec2(1.0, -1.0)).r;

                        float upperLeftM = 1.0 / (invDepth - dInvDepth.x + dInvDepth.y);
                        float upperCenterM = 1.0 / (invDepth + dInvDepth.y);
                        float upperRightM = 1.0 / (invDepth + dInvDepth.x + dInvDepth.y);
                        float leftM = 1.0 / (invDepth - dInvDepth.x);
                        float rightM = 1.0 / (invDepth + dInvDepth.x);
                        float lowerLeftM = 1.0 / (invDepth - dInvDepth.x - dInvDepth.y);
                        float lowerCenterM = 1.0 / (invDepth - dInvDepth.y);
                        float lowerRightM = 1.0 / (invDepth + dInvDepth.x - dInvDepth.y);

                        if ((step(upperLeft, upperLeftM) > 0.0) ||
                            (step(upperCenter, upperCenterM) > 0.0) ||
                            (step(upperRight, upperRightM) > 0.0) ||
                            (step(left, leftM) > 0.0) ||
                            (step(right, rightM) > 0.0) ||
                            (step(lowerLeft, lowerLeftM) > 0.0) ||
                            (step(lowerCenter, lowerCenterM) > 0.0) ||
                            (step(lowerRight, lowerRightM) > 0.0))
                        {
                            fragmentColor = vec3(1.0, 1.0, 0.0);
                        }
                        else
                        {
                            discard;
                        }
                    }
                    else
                    {
                        discard;
                    }
                }";
#endif
            _lotSP = Device.CreateShaderProgram(vs, fs);

            //
            // Mesh
            //
            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.TriangleStrip;

            //
            // Positions
            //
            const int numberOfLineSegments = 4;
            const int numberOfPositions = 2 + numberOfLineSegments + numberOfLineSegments;
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfPositions);
            IList<Vector3D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);
            const double temp = 1.2;
            positions.Add(new Vector3D(0.0, 0.0, heightExaggeration * -temp));
            positions.Add(new Vector3D(0.0, 0.0, heightExaggeration * temp));
            positions.Add(new Vector3D(100.0, 100.0, heightExaggeration * -temp));
            positions.Add(new Vector3D(100.0, 100.0, heightExaggeration * temp));
            positions.Add(new Vector3D(200.0, 100.0, heightExaggeration * -temp));
            positions.Add(new Vector3D(200.0, 100.0, heightExaggeration * temp));
            positions.Add(new Vector3D(256.0, 256.0, heightExaggeration * -temp));
            positions.Add(new Vector3D(256.0, 256.0, heightExaggeration * temp));
            positions.Add(new Vector3D(512.0, 512.0, heightExaggeration * -temp));
            positions.Add(new Vector3D(512.0, 512.0, heightExaggeration * temp));

            //
            // Vertex array
            //
            _lineVA = _window.Context.CreateVertexArray(mesh, _lotSP.VertexAttributes, BufferHint.StaticDraw);
            _lineRenderState = new RenderState();
            _lineRenderState.FacetCulling.Enabled = false;
            _lineRenderState.DepthTest.Enabled = false;
            _lineRenderState.DepthWrite = false;
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
            CreateDepth();
        }

        private void OnRenderFrame()
        {
            //
            // Depth 
            //
            _window.Context.Bind(_fbo);
            _tile.Render(_window.Context, _sceneState);
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _tile.Render(_window.Context, _sceneState);
            _window.Context.Bind(null as FrameBuffer);

            //
            // Terrain
            //
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _tile.Render(_window.Context, _sceneState);

            //
            // Line on terrain
            //
            _window.Context.Bind(_lineVA);
            _window.Context.Bind(_lineRenderState);
            _window.Context.TextureUnits[0].Texture2D = _depthTexture;
            _window.Context.Bind(_lotSP);
            _window.Context.Draw(PrimitiveType.TriangleStrip, _sceneState);

            //
            // Pass-thru line on terrain
            //
            if (_passThru)
            {
                _window.Context.Bind(_passThruSP);
                _window.Context.Draw(PrimitiveType.TriangleStrip, _sceneState);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _tile.Dispose();
            _window.Dispose();
        }

        #endregion

        private void CreateDepth()
        {
            Texture2DDescription t2dd = new Texture2DDescription(_window.Width, _window.Height, TextureFormat.Depth24);
            _depthTexture = Device.CreateTexture2D(t2dd);
            t2dd = new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8);
            _colorTexture = Device.CreateTexture2D(t2dd);
            if (_fbo == null)
            {
                _fbo = _window.Context.CreateFrameBuffer();
            }
            _fbo.DepthAttachment = _depthTexture;
            _fbo.ColorAttachments[0] = _colorTexture;
        }

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (LinesOnTerrain example = new LinesOnTerrain())
            {
                example.Run(30.0);
            }
        }

        private readonly MiniGlobeWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TriangleMeshTerrainTile _tile;

        private bool _passThru = false;
        Texture2D _depthTexture;
        Texture2D _colorTexture;
        FrameBuffer _fbo;
        VertexArray _lineVA;
        RenderState _lineRenderState;
        private readonly ShaderProgram _lotSP;
        private readonly ShaderProgram _passThruSP;
    }
}