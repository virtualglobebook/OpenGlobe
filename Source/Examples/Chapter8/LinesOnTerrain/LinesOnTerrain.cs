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
            string vs =
                @"#version 150

                uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                in vec3 position;

                void main()
                {
                    gl_Position = mg_modelViewPerspectiveProjectionMatrix * vec4(position, 1.0);
                }";
            string fs =
                @"#version 150
                 
                uniform sampler2D mg_texture0;
                uniform vec2 mg_inverseViewportDimensions;
                out vec3 fragmentColor;

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

            //
            // Line on terrain wall
            //
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
            _lotWallSP = Device.CreateShaderProgram(vs, fs);

            //
            // Lines on terrain line
            //
            vs =
                @"#version 150
            
                in vec4 position;

                void main()                     
                {
	                gl_Position = position;
                }";
            string gs =
                @"#version 150 

                layout(lines_adjacency) in;
                layout(triangle_strip, max_vertices = 4) out;

                flat out vec4 fsColor;

                uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                uniform mat4 mg_viewportTransformationMatrix;
                uniform mat4 mg_viewportOrthographicProjectionMatrix;
                uniform float mg_perspectiveNearPlaneDistance;
                uniform float u_fillDistance;

                vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
                {
                    v.xyz /= v.w;                                                        // normalized device coordinates
                    v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                    return v;
                }

                void ClipLineSegmentToNearPlane(
                    float nearPlaneDistance, 
                    mat4 modelViewPerspectiveProjectionMatrix,
                    vec4 modelP0, 
                    vec4 modelP1, 
                    out vec4 clipP0, 
                    out vec4 clipP1)
                {
                    clipP0 = modelViewPerspectiveProjectionMatrix * modelP0;
                    clipP1 = modelViewPerspectiveProjectionMatrix * modelP1;

                    float distanceToP0 = clipP0.z - nearPlaneDistance;
                    float distanceToP1 = clipP1.z - nearPlaneDistance;

                    if ((distanceToP0 * distanceToP1) < 0.0)
                    {
                        float t = distanceToP0 / (distanceToP0 - distanceToP1);
                        vec3 modelV = vec3(modelP0) + t * (vec3(modelP1) - vec3(modelP0));
                        vec4 clipV = modelViewPerspectiveProjectionMatrix * vec4(modelV, 1);

                        if (distanceToP0 < 0.0)
                        {
                            clipP0 = clipV;
                        }
                        else
                        {
                            clipP1 = clipV;
                        }
                    }
                }

                void main()
                {
                    vec4 clipP0;
                    vec4 clipP1;
                    ClipLineSegmentToNearPlane(mg_perspectiveNearPlaneDistance, 
                        mg_modelViewPerspectiveProjectionMatrix,
                        gl_in[1].gl_Position, gl_in[2].gl_Position, clipP0, clipP1);

                    vec4 windowP0 = ClipToWindowCoordinates(clipP0, mg_viewportTransformationMatrix);
                    vec4 windowP1 = ClipToWindowCoordinates(clipP1, mg_viewportTransformationMatrix);

                    vec2 direction = windowP1.xy - windowP0.xy;
                    vec2 normal = normalize(vec2(direction.y, -direction.x));

                    vec4 v0 = vec4(windowP0.xy - (normal * u_fillDistance), windowP0.z, 1.0);
                    vec4 v1 = vec4(windowP1.xy - (normal * u_fillDistance), windowP1.z, 1.0);
                    vec4 v2 = vec4(windowP0.xy + (normal * u_fillDistance), windowP0.z, 1.0);
                    vec4 v3 = vec4(windowP1.xy + (normal * u_fillDistance), windowP1.z, 1.0);



                    vec4 clipS = mg_modelViewPerspectiveProjectionMatrix * gl_in[0].gl_Position;
                    vec4 clipE = mg_modelViewPerspectiveProjectionMatrix * gl_in[3].gl_Position;
                    vec4 windowPS = ClipToWindowCoordinates(clipS, mg_viewportTransformationMatrix);
                    vec4 windowPE = ClipToWindowCoordinates(clipE, mg_viewportTransformationMatrix);

           //         float area = (windowPS.x * windowP0.y - windowP0.x * windowPS.y) +
             //                    (windowP0.x * windowP1.y - windowP1.x * windowP0.y) +
               //                  (windowP1.x * windowPE.y - windowPE.x * windowP1.y) +
                 //                (windowPE.x * windowPS.y - windowPS.x * windowPE.y);
       //             area = abs(area);
// s- 1
// e - 2
                    vec2 s = vec2(windowPS.xy);
                    vec2 e = vec2(windowPE.xy);
                    vec2 p0 = vec2(windowP0.x, windowP0.y);
                    vec2 p1 = vec2(windowP1.x, windowP1.y);
                    float lenA =  length(s - p0);
                    float lenB =  length(e - p1);
                    
           //         if (area < (len * 2.0))
                    if ((lenA < 2.0) || (lenB < 2.0))
                    {


                    gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                    EmitVertex();

                    gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                    EmitVertex();

                    gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                    EmitVertex();

                    gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                    EmitVertex();
                    }
                }";
            fs =
                @"#version 150

                flat in vec4 fsColor;
                out vec4 fragmentColor;

                void main()
                {
                    fragmentColor = vec4(1.0, 1.0, 0.0, 1.0);
                }";
            _lotLineSP = Device.CreateShaderProgram(vs, gs, fs);
            _fillDistance = _lotLineSP.Uniforms["u_fillDistance"] as Uniform<float>;

            //
            // Positions
            //
            IList<Vector3D> positions = new List<Vector3D>();
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
            // Wall mesh
            //
            Mesh wallMesh = new Mesh();
            wallMesh.PrimitiveType = PrimitiveType.TriangleStrip;

            //
            // Positions
            //
            const int numberOfLineSegments = 4;
            const int numberOfPositions = 2 + numberOfLineSegments + numberOfLineSegments;
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfPositions);
            IList<Vector3D> tempPositions = positionsAttribute.Values;
            wallMesh.Attributes.Add(positionsAttribute);
            foreach (Vector3D v in positions)
            {
                tempPositions.Add(v);
            }

            //
            // Vertex array
            //
            _wallVA = _window.Context.CreateVertexArray(wallMesh, _lotWallSP.VertexAttributes, BufferHint.StaticDraw);

            //
            // Line mesh
            //
            Mesh lineMesh = new Mesh();
            lineMesh.PrimitiveType = PrimitiveType.LineStrip;

            //
            // Positions
            //
            positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfPositions);
            tempPositions = positionsAttribute.Values;
            lineMesh.Attributes.Add(positionsAttribute);

            // todo reorg to be more cache efficient
            foreach (Vector3D v in positions)
            {
                tempPositions.Add(v);
            }

            //
            // Indices
            //
            int numIndices = 4 * numberOfLineSegments;
            ushort[] indices = new ushort[numIndices];
            int baseIndex = 1;

            // todo reorg to be more cache efficient
            for (int i = 0; i < numIndices; i += 4, baseIndex += 2)
            {
                indices[i] = (ushort)baseIndex;
                indices[i + 1] = (ushort)(baseIndex - 1);
                indices[i + 2] = (ushort)(baseIndex + 1);
                indices[i + 3] = (ushort)(baseIndex + 2);
            }
            IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StaticDraw, numIndices * sizeof(ushort));
            indexBuffer.CopyFromSystemMemory(indices);

            //
            // Vertex array
            //
            _lineVA = _window.Context.CreateVertexArray(lineMesh, _lotWallSP.VertexAttributes, BufferHint.StaticDraw);
            _lineVA.IndexBuffer = indexBuffer;

            //
            // Render state
            //
            _lotRenderState = new RenderState();
            _lotRenderState.FacetCulling.Enabled = false;
            _lotRenderState.DepthTest.Enabled = false;
            _lotRenderState.DepthWrite = false;
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
            _window.Context.Bind(new RenderState());
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _tile.Render(_window.Context, _sceneState);
            _window.Context.Bind(null as FrameBuffer);

            //
            // Terrain
            //
            _window.Context.Clear(ClearBuffers.ColorAndDepthBuffer, Color.White, 1, 0);
            _tile.Render(_window.Context, _sceneState);

            //
            // Line on terrain wall
            //
            if (_passThru1)
            {
                _window.Context.Bind(_wallVA);
                _window.Context.Bind(_lotRenderState);
                _window.Context.TextureUnits[0].Texture2D = _depthTexture;
                _window.Context.Bind(_lotWallSP);
                _window.Context.Draw(PrimitiveType.TriangleStrip, _sceneState);
            }

            //
            // Line on terrain line
            //
            _window.Context.Bind(_lineVA);
            _window.Context.Bind(_lotRenderState);
            _window.Context.Bind(_lotLineSP);
            _fillDistance.Value = (float)(_width * 0.5);
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _sceneState);

            //
            // Pass-thru line on terrain
            //
            if (_passThru)
            {
                _window.Context.Bind(_wallVA);
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

            // TODO - add all the disposes
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
        private bool _passThru1 = true;
        Texture2D _depthTexture;
        Texture2D _colorTexture;
        FrameBuffer _fbo;
        VertexArray _wallVA;
        VertexArray _lineVA;
        RenderState _lotRenderState;
        private readonly ShaderProgram _passThruSP;
        private readonly ShaderProgram _lotWallSP;
        private readonly ShaderProgram _lotLineSP;

        // TODO - should you keep this here?
        private float _width = 1.0f;
        private Uniform<float> _fillDistance;
    }
}