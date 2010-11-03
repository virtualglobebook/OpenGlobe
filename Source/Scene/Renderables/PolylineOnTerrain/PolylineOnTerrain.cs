#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;

using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;

using OpenGlobe.Core;

//
// deron junk todo
//
// clipping to wall shader
// wall normal angle is not a good way to determine which shader to use
//

namespace OpenGlobe.Scene
{
    public sealed class PolylineOnTerrain : IDisposable
    {
        public PolylineOnTerrain()
        {
            _wallSP = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainWallVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainWallGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainWallFS.glsl"));

            _shadowVolumeSP = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainShadowVolumeVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainShadowVolumeGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.PolylineOnTerrain.Shaders.PolylineOnTerrainShadowVolumeFS.glsl"));
        }

        public void Set(Context context, IList<Vector3D> positions)
        {
            //
            // This method expects that the positions are ordered in a repeated pattern of 
            // below terrain, above terrain pairs.
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
            _wallVA = context.CreateVertexArray(wallMesh, _wallSP.VertexAttributes, BufferHint.StaticDraw);

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
            _lineVA = context.CreateVertexArray(lineMesh, _wallSP.VertexAttributes, BufferHint.StaticDraw);
            _lineVA.IndexBuffer = indexBuffer;

            //
            // State
            //
            _wallDrawState = new DrawState();
            _wallDrawState.RenderState.FacetCulling.Enabled = false;
            _wallDrawState.RenderState.DepthTest.Enabled = false;
            _wallDrawState.RenderState.DepthMask = false;
            _wallDrawState.VertexArray = _lineVA;
            _wallDrawState.ShaderProgram = _wallSP;

            _shadowVolumePassOne = new DrawState();
            _shadowVolumePassOne.VertexArray = _lineVA;
            _shadowVolumePassOne.ShaderProgram = _shadowVolumeSP;
            _shadowVolumePassOne.RenderState.FacetCulling.Enabled = false;
            _shadowVolumePassOne.RenderState.DepthMask = false;
            _shadowVolumePassOne.RenderState.ColorMask = new ColorMask(false, false, false, false);
            StencilTest stOne = _shadowVolumePassOne.RenderState.StencilTest;
            stOne.Enabled = true;
            stOne.FrontFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            stOne.BackFace.DepthFailStencilPassOperation = StencilOperation.Increment;

            _shadowVolumePassTwo = new DrawState();
            _shadowVolumePassTwo.VertexArray = _lineVA;
            _shadowVolumePassTwo.ShaderProgram = _shadowVolumeSP;
            _shadowVolumePassTwo.RenderState.DepthMask = false;
            StencilTest stTwo = _shadowVolumePassTwo.RenderState.StencilTest;
            stTwo.Enabled = true;
            stTwo.FrontFace.DepthFailStencilPassOperation = StencilOperation.Zero;
            stTwo.FrontFace.DepthPassStencilPassOperation = StencilOperation.Zero;
            stTwo.FrontFace.Function = StencilTestFunction.NotEqual;
            stTwo.BackFace.DepthFailStencilPassOperation = StencilOperation.Zero;
            stTwo.BackFace.DepthPassStencilPassOperation = StencilOperation.Zero;
            stTwo.BackFace.Function = StencilTestFunction.NotEqual;
        }

        public void Render(Context context, SceneState sceneState, Texture2D silhouetteTexture, Texture2D depthTexture)
        {
            //
            // Render the line on terrain using the wall method
            //
            context.TextureUnits[0].Texture = silhouetteTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            context.TextureUnits[1].Texture = depthTexture;
            context.TextureUnits[1].TextureSampler = Device.TextureSamplers.LinearClamp;
            context.Draw(PrimitiveType.LinesAdjacency, _wallDrawState, sceneState);

            //
            // Render the line on terrain using the depth-fail shadow volume method
            //
            context.TextureUnits[0].Texture = silhouetteTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            context.Draw(PrimitiveType.LinesAdjacency, _shadowVolumePassOne, sceneState);
            
            //
            // Render where the stencil is set; note that the stencil is also cleared 
            // where it is set.
            //
            context.Draw(PrimitiveType.LinesAdjacency, _shadowVolumePassTwo, sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_wallVA != null)
            {
                _wallVA.Dispose();
            }
            if (_lineVA != null)
            {
                _lineVA.Dispose();
            }
            if (_wallSP != null)
            {
                _wallSP.Dispose();
            }
            if (_shadowVolumeSP != null)
            {
                _shadowVolumeSP.Dispose();
            }
        }

        #endregion

        private VertexArray _wallVA;
        private VertexArray _lineVA;
        private readonly ShaderProgram _wallSP;
        private readonly ShaderProgram _shadowVolumeSP;
        private DrawState _wallDrawState;
        private DrawState _shadowVolumePassOne;
        private DrawState _shadowVolumePassTwo;
    }
}