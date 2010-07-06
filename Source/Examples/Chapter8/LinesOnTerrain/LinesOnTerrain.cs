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

namespace OpenGlobe.Examples.Chapter8
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
            // Line on terrain
            //

            //
            // Silhouette
            //
            string vs =
                @"#version 330
            
                uniform mat4 og_modelViewPerspectiveProjectionMatrix;
                uniform float u_heightExaggeration;
                in vec4 position;

                void main()                     
                {
                    vec4 exaggeratedPosition = vec4(position.xy, position.z * u_heightExaggeration, 1.0);
	                gl_Position = og_modelViewPerspectiveProjectionMatrix * exaggeratedPosition;
                }";

            string gs =
                @"#version 330 

                layout(triangles) in;
                layout(triangle_strip, max_vertices = 15) out;

                uniform mat4 og_viewportTransformationMatrix;
                uniform mat4 og_viewportOrthographicProjectionMatrix;
                uniform float og_perspectiveNearPlaneDistance;
                uniform float u_fillDistance;

                vec4 ClipToWindowCoordinates(vec4 v)
                {
                    v.xyz /= v.w;
                    return vec4((og_viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz, 1.0);
                }

                void ClipLineSegmentToNearPlane(
                    vec4 modelP0, 
                    vec4 modelP1, 
                    out vec4 window0, 
                    out vec4 window1,
                    out bool lineSegmentAtLeastPartlyInFrontOfNearPlane)
                {
                    float distanceToP0 = modelP0.z + og_perspectiveNearPlaneDistance;
                    float distanceToP1 = modelP1.z + og_perspectiveNearPlaneDistance;
                    if ((distanceToP0 * distanceToP1) < 0.0)
                    {
                        float t = distanceToP0 / (distanceToP0 - distanceToP1);
                        vec4 clipV = modelP0 + (t * (modelP1 - modelP0));
                        if (distanceToP0 < 0.0)
                        {
                            window0 = ClipToWindowCoordinates(clipV);
                            window1 = ClipToWindowCoordinates(modelP1);
                        }
                        else
                        {
                            window0 = ClipToWindowCoordinates(modelP0);
                            window1 = ClipToWindowCoordinates(clipV);
                        }
                    }
                    else
                    {
                        window0 = ClipToWindowCoordinates(modelP0);
                        window1 = ClipToWindowCoordinates(modelP1);
                    }
                    lineSegmentAtLeastPartlyInFrontOfNearPlane = (distanceToP0 >= 0.0) || (distanceToP1 >= 0.0);
                }

                void main()
                {
                    vec4 window[6];
                    int index = 0;
                    bool lineSegmentAtLeastPartlyInFrontOfNearPlane;

                    ClipLineSegmentToNearPlane(gl_in[0].gl_Position, gl_in[1].gl_Position, window[index], window[index + 1],
                        lineSegmentAtLeastPartlyInFrontOfNearPlane);
                    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
                    {
                        index += 2;
                    }

                    ClipLineSegmentToNearPlane(gl_in[1].gl_Position, gl_in[2].gl_Position, window[index], window[index + 1],
                        lineSegmentAtLeastPartlyInFrontOfNearPlane);
                    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
                    {
                        index += 2;
                    }

                    ClipLineSegmentToNearPlane(gl_in[2].gl_Position, gl_in[0].gl_Position, window[index], window[index + 1],
                        lineSegmentAtLeastPartlyInFrontOfNearPlane);
                    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
                    {
                        index += 2;
                    }

                    if (index > 0)
                    {
                        float area = 0.0;
                        int limit = index - 1;
                        for (int i = 0; i < limit; ++i)
                        {
                            area += (window[i].x * window[i + 1].y) - (window[i + 1].x * window[i].y);
                        }
                        area += (window[limit].x * window[0].y) - (window[0].x * window[limit].y);

                        if (area < 0.0)
                        {
                            for (int i = 0; i < index; i += 2)
                            {
                                vec4 window0 = window[i];
                                vec4 window1 = window[i + 1];

                                vec2 direction = window1.xy - window0.xy;
                    
                                if (dot(direction, direction) > 0.0)
                                {
                                    direction = normalize(direction) * u_fillDistance;
                                    vec2 cross = vec2(direction.y, -direction.x);

                                    vec4 v0 = vec4(window0.xy - cross, -window0.z, 1.0);
                                    vec4 v1 = vec4(window0.xy + cross, -window0.z, 1.0);
                                    vec4 v2 = vec4(window1.xy - cross, -window1.z, 1.0);
                                    vec4 v3 = vec4(window1.xy + cross, -window1.z, 1.0);
                                    vec4 v4 = vec4(window1.xy + direction, -window1.z, 1.0);
 
                                    gl_Position = og_viewportOrthographicProjectionMatrix * v0;
                                    EmitVertex();

                                    gl_Position = og_viewportOrthographicProjectionMatrix * v1;
                                    EmitVertex();

                                    gl_Position = og_viewportOrthographicProjectionMatrix * v2;
                                    EmitVertex();

                                    gl_Position = og_viewportOrthographicProjectionMatrix * v3;
                                    EmitVertex();

                                    gl_Position = og_viewportOrthographicProjectionMatrix * v4;
                                    EmitVertex();

                                    EndPrimitive();
                                }
                            }
                        }
                    }
                }";
            string fs =
                @"#version 330

                out vec4 fsColor;

                void main()
                {
                    fsColor = vec4(0.0, 0.0, 0.0, 1.0);
                }";
            _silhouetteSP = Device.CreateShaderProgram(vs, gs, fs);
            Uniform<float> fillDistance = _silhouetteSP.Uniforms["u_fillDistance"] as Uniform<float>;
            fillDistance.Value = 2.0f;
            Uniform<float> heightExaggeration = _silhouetteSP.Uniforms["u_heightExaggeration"] as Uniform<float>;
            heightExaggeration.Value = _tile.HeightExaggeration;

            //
            // Line on terrain wall
            //



            //
            // Line on terrain wall alternative
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

            fs =
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
            _lotWallAlternativeSP = Device.CreateShaderProgram(vs, gs, fs);

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
                uniform float u_pixelSizePerDistance;

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
                        vec3 norm = cr * (u_pixelSizePerDistance * 1.0); // junk
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
                        fragmentColor = vec4(1.0, 1.0, 0.0, 1.0);
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
            _wallVA = _window.Context.CreateVertexArray(wallMesh, _lotWallAlternativeSP.VertexAttributes, BufferHint.StaticDraw);

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
            _lineVA = _window.Context.CreateVertexArray(lineMesh, _lotWallAlternativeSP.VertexAttributes, BufferHint.StaticDraw);
            _lineVA.IndexBuffer = indexBuffer;

            //
            // State
            //
            
            _silhouetteDrawState = new DrawState();
            _silhouetteDrawState.RenderState.FacetCulling.Enabled = false;
            _silhouetteDrawState.RenderState.DepthWrite = false;
            _silhouetteDrawState.ShaderProgram = _silhouetteSP;

            _lineOnTerrainWallDrawState = new DrawState();
            _lineOnTerrainWallDrawState.RenderState.FacetCulling.Enabled = false;
            _lineOnTerrainWallDrawState.RenderState.DepthTest.Enabled = false;
            _lineOnTerrainWallDrawState.RenderState.DepthWrite = false;
            _lineOnTerrainWallDrawState.VertexArray = _lineVA;
            _lineOnTerrainWallDrawState.ShaderProgram = _lotWallAlternativeSP;

            _lotLineBoxDrawState = new DrawState();
            _lotLineBoxDrawState.RenderState.FacetCulling.Enabled = true;
            _lotLineBoxDrawState.RenderState.DepthWrite = false;
            _lotLineBoxDrawState.VertexArray = _lineVA;
            _lotLineBoxDrawState.ShaderProgram = _lotLineBoxSP;

            _clearState = new ClearState();
            _clearState.Color = Color.White;
            _clearState.Stencil = 0;
            _clearState.Depth = 1;

            //
            // Depth
            //
            CreateDepth();
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
            // TODO: change this to render the terrain and then copy over the framebuffer into
            //       a depth texture.
            //
            // Terrain depth to texture
            //
            _window.Context.FrameBuffer = _terrainFrameBuffer;
            _clearState.Buffers = ClearBuffers.DepthBuffer;
            _window.Context.Clear(_clearState);
            _tile.Render(_window.Context, _sceneState);

            //
            // Silhouette to texture
            //
            _window.Context.FrameBuffer = _silhouetteFrameBuffer;
            _clearState.Buffers = ClearBuffers.ColorBuffer;
            _window.Context.Clear(_clearState);
            _tile.RenderCustom(_window.Context, _sceneState, _silhouetteDrawState);

            //
            // Terrain to framebuffer
            //
            _window.Context.FrameBuffer = null;
            _clearState.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.ColorBuffer | ClearBuffers.StencilBuffer;
             _window.Context.Clear(_clearState);
            _tile.Render(_window.Context, _sceneState);

            //
            // Silhouette to framebuffer
            //
            _silhouetteDrawState.RenderState.FacetCulling.Enabled = true;
            _silhouetteDrawState.RenderState.FacetCulling.Face = CullFace.Front;
            _silhouetteDrawState.RenderState.FacetCulling.FrontFaceWindingOrder = WindingOrder.Clockwise;
            _tile.RenderCustom(_window.Context, _sceneState, _silhouetteDrawState);
            _silhouetteDrawState.RenderState.FacetCulling.Enabled = false;

            //
            // Line on terrain wall
            //
            _window.Context.TextureUnits[0].Texture2D = _silhouetteTexture;
            _window.Context.TextureUnits[1].Texture2D = _depthTexture;
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lineOnTerrainWallDrawState, _sceneState);

            //
            // Line on terrain line
            //


            //
            // Using the stencil for line on terrain BOX
            //

            // pixel size per distance - pretend square pixels
            float psd = (float)(Math.Tan(0.5 * _sceneState.Camera.FieldOfViewY) * 2.0 / _window.Context.Viewport.Height);
            Uniform<float> upsd = _lotLineBoxSP.Uniforms["u_pixelSizePerDistance"] as Uniform<float>;
            upsd.Value = 0.5f * psd;

            //
            //  front face cull
            //
            StencilTest stencilTest = new StencilTest();
            _lotLineBoxDrawState.RenderState.StencilTest = stencilTest;

            stencilTest.Enabled = true;

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

            _window.Context.TextureUnits[0].Texture2D = _silhouetteTexture;
            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            //
            //  back face cull
            //
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Decrement;

            _lotLineBoxDrawState.RenderState.FacetCulling.Face = CullFace.Back;

            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            //
            // draw where stencil is set
            //

            // junk deron todo - clear the stencil at the same time

            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Keep;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.FrontFace.Function = StencilTestFunction.NotEqual;
            stencilTest.FrontFace.ReferenceValue = 0;

            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Keep;
            stencilTest.BackFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.BackFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.BackFace.Function = StencilTestFunction.NotEqual;
            stencilTest.BackFace.ReferenceValue = 0;

            _lotLineBoxDrawState.RenderState.ColorMask = new ColorMask(true, true, true, true);

            _window.Context.Draw(PrimitiveType.LinesAdjacency, _lotLineBoxDrawState, _sceneState);

            _lotLineBoxDrawState.RenderState.StencilTest = null;


            _lotLineBoxDrawState.RenderState.DepthTest.Function = DepthTestFunction.Less;
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
            //
            // Textures
            //
            _depthTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.Depth24));

            // TODO change to Red8
           _silhouetteTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.Red8));
 //           _silhouetteTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8));
            _colorTexture = Device.CreateTexture2D(new Texture2DDescription(_window.Width, _window.Height, TextureFormat.RedGreenBlue8));
            
            //
            // Terrain FBO
            //
            if (_terrainFrameBuffer == null)
            {
                _terrainFrameBuffer = _window.Context.CreateFrameBuffer();
            }
            _terrainFrameBuffer.DepthAttachment = _depthTexture;
            _terrainFrameBuffer.ColorAttachments[0] = _colorTexture;

            //
            // Silhouette FBO
            //
            if (_silhouetteFrameBuffer == null)
            {
                _silhouetteFrameBuffer = _window.Context.CreateFrameBuffer();
            }
            _silhouetteFrameBuffer.DepthAttachment = _depthTexture;
            _silhouetteFrameBuffer.ColorAttachments[0] = _silhouetteTexture;

            //
            // Line on terrain FBO
            //
            if (_lineOnTerrainFramebuffer == null)
            {
                _lineOnTerrainFramebuffer = _window.Context.CreateFrameBuffer();
            }
            _lineOnTerrainFramebuffer.ColorAttachments[0] = _colorTexture;
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

        private readonly GraphicsWindow _window;
        private  SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly TriangleMeshTerrainTile _tile;
        private Texture2D _depthTexture;
        private Texture2D _silhouetteTexture;
        private Texture2D _colorTexture;
        private FrameBuffer _terrainFrameBuffer;
        private FrameBuffer _silhouetteFrameBuffer;
        private FrameBuffer _lineOnTerrainFramebuffer;
        private readonly VertexArray _wallVA;
        private readonly VertexArray _lineVA;
        private readonly DrawState _silhouetteDrawState;
        private readonly DrawState _lineOnTerrainWallDrawState;
        private readonly DrawState _lotLineBoxDrawState;
        private readonly ShaderProgram _silhouetteSP;
        private readonly ShaderProgram _lotWallAlternativeSP;
        private readonly ShaderProgram _lotLineBoxSP;
        private readonly ClearState _clearState;

        // TODO - should you keep this here?
        private float _width = 1.0f;

        private double _xPos = 448;
    }
}