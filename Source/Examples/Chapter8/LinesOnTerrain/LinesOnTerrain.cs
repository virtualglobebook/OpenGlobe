#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

using OpenGlobe.Core;
using OpenGlobe.Terrain;

// deron junk todo
//
// clipping to wall shader
// wall normal angle is not a good way to determine which shader to use
//

namespace OpenGlobe.Examples.Chapter8
{
    sealed class LinesOnTerrain : IDisposable
    {
        public LinesOnTerrain()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 8:  Lines on Terrain");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.Keyboard.KeyDown += OnKeyDown;
            _sceneState = new SceneState();
            _sceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            _sceneState.Camera.PerspectiveNearPlaneDistance = 10;
            
            _instructions = new HeadsUpDisplay(_window.Context);
            _instructions.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText("s - Show silhouette",
                    new Font("Arial", 24)),
                TextureFormat.RedGreenBlueAlpha8, false);
            _instructions.Color = Color.LightBlue;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(new Bitmap(@"ps-e.lg.png"));
            _tile = new TriangleMeshTerrainTile(_window.Context, terrainTile);
            _tile.HeightExaggeration = 30.0f;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, Ellipsoid.UnitSphere);
            _camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            _sceneState.Camera.ZoomToTarget(tileRadius);
            _sceneState.Camera.Eye = new Vector3D(_xPos, 256, 0);
            
            HighResolutionSnap snap = new HighResolutionSnap(_window, _sceneState);
            snap.ColorFilename = @"E:\Manuscript\TerrainRendering\Figures\LinesOnTerrain.png";
            snap.WidthInInches = 3;
            snap.DotsPerInch = 600;

            //
            // Line on terrain wall
            //
            string vs =
                @"#version 330
            
                uniform mat4 og_modelViewMatrix;
                in vec4 position;

                void main()                     
                {
	                gl_Position = og_modelViewMatrix * position;
                }";
            string gs =
                @"#version 330 

                layout(lines_adjacency) in;
                layout(triangle_strip, max_vertices = 4) out;

                uniform mat4 og_perspectiveProjectionMatrix;

                void main()
                {
                    // normal
                    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
                    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
                    vec3 cr = cross(v1, v0);
                    cr = normalize(cr);

                    if (abs(cr.z) > 0.017452406437283512819418978516316)
                    {
                        gl_Position = og_perspectiveProjectionMatrix * gl_in[1].gl_Position;
                        EmitVertex();
                        gl_Position = og_perspectiveProjectionMatrix * gl_in[0].gl_Position;
                        EmitVertex();
                        gl_Position = og_perspectiveProjectionMatrix * gl_in[2].gl_Position;
                        EmitVertex();
                        gl_Position = og_perspectiveProjectionMatrix * gl_in[3].gl_Position;
                        EmitVertex();
                        EndPrimitive();
                    }

                }";
            string fs =
                 @"#version 330
                
                uniform sampler2D og_texture0;
                uniform sampler2D og_texture1;
                uniform vec2 og_inverseViewportDimensions;
                noperspective in vec2 vTexture0;
                out vec3 fragmentColor;

                void main()
                {
// deron junk todo - probably ok to put this in the if statement

                    vec2 dZ = vec2(dFdx(gl_FragCoord.z), dFdy(gl_FragCoord.z));

                    float z = gl_FragCoord.z;
                    vec2 of = og_inverseViewportDimensions * gl_FragCoord.xy;
                    float center = texture(og_texture1, of).r;
                    float silhouetteCenter = texture(og_texture0, of).r;
                    if ((silhouetteCenter != 0.0) && (gl_FragCoord.z < center))
                    {
                        //
                        // Fragment is above the terrain
                        //
                        float upperLeft = textureOffset(og_texture1, of, ivec2(-1.0, 1.0)).r;
                        float upperCenter = textureOffset(og_texture1, of, ivec2(0.0, 1.0)).r;
                        float upperRight = textureOffset(og_texture1, of, ivec2(1.0, 1.0)).r;
                        float left = textureOffset(og_texture1, of, ivec2(-1.0, 0.0)).r;
                        float right = textureOffset(og_texture1, of, ivec2(1.0, 0.0)).r;
                        float lowerLeft = textureOffset(og_texture1, of, ivec2(-1.0, -1.0)).r;
                        float lowerCenter = textureOffset(og_texture1, of, ivec2(0.0, -1.0)).r;
                        float lowerRight = textureOffset(og_texture1, of, ivec2(1.0, -1.0)).r;

                        float upperLeftM =  z - dZ.x + dZ.y;
                        float upperCenterM = z + dZ.y;
                        float upperRightM = z + dZ.x + dZ.y;
                        float leftM = z - dZ.x;
                        float rightM = z + dZ.x;
                        float lowerLeftM = z - dZ.x - dZ.y;
                        float lowerCenterM = z - dZ.y;
                        float lowerRightM = z + dZ.x - dZ.y;

                        if ((upperLeft < upperLeftM) ||
                            (upperCenter < upperCenterM) ||
                            (upperRight < upperRightM) ||
                            (left < leftM) ||
                            (right < rightM) ||
                            (lowerLeft < lowerLeftM) ||
                            (lowerCenter < lowerCenterM) ||
                            (lowerRight < lowerRightM))
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
            _lotWallSP = Device.CreateShaderProgram(vs, gs, fs);

            //
            // Lines on terrain box
            //
            vs =
                @"#version 330
            
                uniform mat4 og_modelViewMatrix;
                in vec4 position;

                void main()                     
                {
	                gl_Position = og_modelViewMatrix * position;
                }";
            gs =
                @"#version 330 

                layout(lines_adjacency) in;
                layout(triangle_strip, max_vertices = 18) out;

                uniform mat4 og_perspectiveProjectionMatrix;
                uniform float u_halfPixelSizePerDistance;

                void main()
                {
                    // normal
                    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
                    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
                    vec3 cr = cross(v1, v0);
                    cr = normalize(cr);

                    if (abs(cr.z) <= 0.017452406437283512819418978516316)
                    {
                        // vertex
                        vec3 norm = cr * u_halfPixelSizePerDistance;
                        vec4 vertices[8];

                        vec3 n = norm * abs(gl_in[1].gl_Position.z);
                        vertices[0] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz - n, 1.0);
                        vertices[1] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz + n, 1.0);
                        n = norm * abs(gl_in[0].gl_Position.z);
                        vertices[2] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz + n, 1.0);
                        vertices[3] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz - n, 1.0);
                        n = norm * abs(gl_in[2].gl_Position.z);
                        vertices[4] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz - n, 1.0);
                        vertices[5] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz + n, 1.0);
                        n = norm * abs(gl_in[3].gl_Position.z);
                        vertices[6] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz + n, 1.0);
                        vertices[7] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz - n, 1.0);
                    
                        gl_Position = vertices[0];
                        EmitVertex();
                        gl_Position = vertices[1];
                        EmitVertex();
                        gl_Position = vertices[3];
                        EmitVertex();
                        gl_Position = vertices[2];
                        EmitVertex();
                        gl_Position = vertices[7];
                        EmitVertex();
                        gl_Position = vertices[6];
                        EmitVertex();
                        gl_Position = vertices[4];
                        EmitVertex();
                        gl_Position = vertices[5];
                        EmitVertex();
                        gl_Position = vertices[0];
                        EmitVertex();
                        gl_Position = vertices[1];
                        EmitVertex();
                        EndPrimitive();

                        gl_Position = vertices[2];
                        EmitVertex();
                        gl_Position = vertices[1];
                        EmitVertex();
                        gl_Position = vertices[6];
                        EmitVertex();
                        gl_Position = vertices[5];
                        EmitVertex();
                        EndPrimitive();

                        gl_Position = vertices[0];
                        EmitVertex();
                        gl_Position = vertices[3];
                        EmitVertex();
                        gl_Position = vertices[4];
                        EmitVertex();
                        gl_Position = vertices[7];
                        EmitVertex();
                        EndPrimitive();
                    }
                }";
            fs =
                @"#version 330

                uniform sampler2D og_texture0;
                uniform vec2 og_inverseViewportDimensions;
                out vec4 fragmentColor;

                void main()
                {
                    vec2 of = og_inverseViewportDimensions * gl_FragCoord.xy;
                    if (texture(og_texture0, of).r != 0.0)
                    {
                        fragmentColor = vec4(0.0, 0.0, 1.0, 1.0);
                    }
                }";
            _lotLineBoxSP = Device.CreateShaderProgram(vs, gs, fs);

            //
            // Positions
            //
            IList<Vector3D> positions = new List<Vector3D>();
            double temp = 1.2 * _tile.HeightExaggeration;
            //positions.Add(new Vector3D(0.0, 0.0, -temp));
            //positions.Add(new Vector3D(0.0, 0.0, temp));
            //positions.Add(new Vector3D(100.0, 100.0, -temp));
            //positions.Add(new Vector3D(100.0, 100.0, temp));
            positions.Add(new Vector3D(200.0, 100.0, -temp));
            positions.Add(new Vector3D(200.0, 100.0, temp));
            positions.Add(new Vector3D(256.0, 256.0, -temp));
            positions.Add(new Vector3D(256.0, 256.0, temp));
            //positions.Add(new Vector3D(512.0, 512.0, -temp));
            //positions.Add(new Vector3D(512.0, 512.0, temp));

            //
            // Wall mesh
            //
            Mesh wallMesh = new Mesh();
            wallMesh.PrimitiveType = PrimitiveType.TriangleStrip;

            //
            // Positions
            //
            int numberOfLineSegments = (positions.Count / 2) - 1;
            int numberOfVertices = 2 + numberOfLineSegments + numberOfLineSegments;
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfVertices);
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
            positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfVertices);
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
            // State
            //
            _lotWallDrawState = new DrawState();
            _lotWallDrawState.RenderState.FacetCulling.Enabled = false;
            _lotWallDrawState.RenderState.DepthTest.Enabled = false;
            _lotWallDrawState.RenderState.DepthWrite = false;
            _lotWallDrawState.VertexArray = _lineVA;
            _lotWallDrawState.ShaderProgram = _lotWallSP;

            _lotLineBoxDrawState = new DrawState();
            _lotLineBoxDrawState.RenderState.FacetCulling.Enabled = true;
            _lotLineBoxDrawState.RenderState.DepthWrite = false;
            StencilTest stencilTest = new StencilTest();
            stencilTest.Enabled = true;
            _lotLineBoxDrawState.RenderState.StencilTest = stencilTest;
            _lotLineBoxDrawState.VertexArray = _lineVA;
            _lotLineBoxDrawState.ShaderProgram = _lotLineBoxSP;

            _clearState = new ClearState();
            _clearState.Color = Color.White;
            _clearState.Stencil = 0;
            _clearState.Depth = 1;

            // junk
            fs =
                @"#version 330

                uniform sampler2D og_texture0;
                in vec2 fsTextureCoordinates;
                out vec4 fragmentColor;

                void main()
                {
                    if (texture(og_texture0, fsTextureCoordinates).r == 0.0)
                    {
                        fragmentColor = vec4(0.0, 0.0, 0.0, 1.0);
                    }
                    else
                    {
                        discard;
                    }
                }";

            _viewportQuad = new ViewportQuad(_window.Context, fs);
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            //
            // Terrain and silhouette textures
            //
            _tile.RenderDepthAndSilhouetteTextures(_window.Context, _sceneState);

            //
            // Terrain to framebuffer
            //
            _window.Context.FrameBuffer = null;
            _clearState.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.ColorBuffer | ClearBuffers.StencilBuffer; // junk can't you get rid of the stencil clear?
             _window.Context.Clear(_clearState);
            _tile.Render(_window.Context, _sceneState);

            //
            // Overlay the silhouette texture over the framebuffer
            //
            if (_showSilhouette)
            {
                _viewportQuad.Texture = _tile.SilhouetteTexture;
                _viewportQuad.Render(_window.Context, _sceneState);
            }

            //
            // Render the line on terrain using the wall method
            //
            _window.Context.TextureUnits[0].Texture2D = _tile.SilhouetteTexture;
            _window.Context.TextureUnits[1].Texture2D = _tile.DepthTexture;
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotWallDrawState, _sceneState);

            //
            // Render the line on terrain using the depth-fail shadow volume method
            //
            // Compute the size of a half a pixel at a distance
            //
            float halfPixelSizePerDistance = (float)(0.5 * Math.Tan(0.5 * _sceneState.Camera.FieldOfViewY) * 2.0 / _window.Context.Viewport.Height);
            Uniform<float> uHalfPixelSizePerDistance = _lotLineBoxSP.Uniforms["u_halfPixelSizePerDistance"] as Uniform<float>;
            uHalfPixelSizePerDistance.Value = halfPixelSizePerDistance;

            //
            //  Render the back faces
            //
            StencilTest stencilTest = _lotLineBoxDrawState.RenderState.StencilTest;
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Increment;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.FrontFace.Function = StencilTestFunction.Always;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Increment;
            stencilTest.BackFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.BackFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.BackFace.Function = StencilTestFunction.Always;
            _lotLineBoxDrawState.RenderState.ColorMask = new ColorMask(false, false, false, false);
            _lotLineBoxDrawState.RenderState.FacetCulling.Face = CullFace.Front;
            _window.Context.TextureUnits[0].Texture2D = _tile.SilhouetteTexture;
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            //
            // Render the front faces
            //
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            _lotLineBoxDrawState.RenderState.FacetCulling.Face = CullFace.Back;
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            //
            // Render where the stencil is set; note that the stencil is also cleared where it is
            // set.
            //
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Zero;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Zero;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.FrontFace.Function = StencilTestFunction.NotEqual;
            stencilTest.FrontFace.ReferenceValue = 0;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Zero;
            stencilTest.BackFace.DepthPassStencilPassOperation = StencilOperation.Zero;
            stencilTest.BackFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.BackFace.Function = StencilTestFunction.NotEqual;
            stencilTest.BackFace.ReferenceValue = 0;
            _lotLineBoxDrawState.RenderState.ColorMask = new ColorMask(true, true, true, true);
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            //
            // Render the instructions
            //
            _instructions.Render(_window.Context, _sceneState);
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.S)
            {
                _showSilhouette = !_showSilhouette;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _instructions.Dispose();
            _tile.Dispose();
            _window.Dispose();

            // TODO - add all the disposes
        }

        #endregion

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

        private readonly GraphicsWindow _window;
        private  SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly HeadsUpDisplay _instructions;
        private readonly TriangleMeshTerrainTile _tile;
        private readonly VertexArray _wallVA;
        private readonly VertexArray _lineVA;
        private readonly ShaderProgram _lotWallSP;
        private readonly ShaderProgram _lotLineBoxSP;
        private readonly DrawState _lotWallDrawState;
        private readonly DrawState _lotLineBoxDrawState;
        private readonly ClearState _clearState;
        private readonly ViewportQuad _viewportQuad;
        private bool _showSilhouette = false;

        private double _xPos = 448; // junk deron todo use this still?
    }
}