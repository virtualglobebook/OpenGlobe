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
            _wallDrawState.RenderState.DepthWrite = false;
            _wallDrawState.VertexArray = _lineVA;
            _wallDrawState.ShaderProgram = _wallSP;

            _shadowVolumeDrawState = new DrawState();
            _shadowVolumeDrawState.RenderState.FacetCulling.Enabled = true;
            _shadowVolumeDrawState.RenderState.DepthWrite = false;
            StencilTest stencilTest = new StencilTest();
            stencilTest.Enabled = true;
            _shadowVolumeDrawState.RenderState.StencilTest = stencilTest;
            _shadowVolumeDrawState.VertexArray = _lineVA;
            _shadowVolumeDrawState.ShaderProgram = _shadowVolumeSP;
        }

        public void Render(Context context, SceneState sceneState, Texture2D silhouetteTexture, Texture2D depthTexture)
        {
            //
            // Render the line on terrain using the wall method
            //
            context.TextureUnits[0].Texture2D = silhouetteTexture;
            context.TextureUnits[1].Texture2D = depthTexture;
            context.Draw(PrimitiveType.LinesAdjacency, _wallDrawState, sceneState);

            //
            // Render the line on terrain using the depth-fail shadow volume method
            //
            // Render the back faces
            //
            StencilTest stencilTest = _shadowVolumeDrawState.RenderState.StencilTest;
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Increment;
            stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.FrontFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.FrontFace.Function = StencilTestFunction.Always;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Increment;
            stencilTest.BackFace.DepthPassStencilPassOperation = StencilOperation.Keep;
            stencilTest.BackFace.StencilFailOperation = StencilOperation.Keep;
            stencilTest.BackFace.Function = StencilTestFunction.Always;
            _shadowVolumeDrawState.RenderState.ColorMask = new ColorMask(false, false, false, false);
            _shadowVolumeDrawState.RenderState.FacetCulling.Face = CullFace.Front;
            context.TextureUnits[0].Texture2D = silhouetteTexture;
            context.Draw(PrimitiveType.LinesAdjacency, _shadowVolumeDrawState, sceneState);

            //
            // Render the front faces
            //
            stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            stencilTest.BackFace.DepthFailStencilPassOperation = StencilOperation.Decrement;
            _shadowVolumeDrawState.RenderState.FacetCulling.Face = CullFace.Back;
            context.Draw(PrimitiveType.LinesAdjacency, _shadowVolumeDrawState, sceneState);

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
            _shadowVolumeDrawState.RenderState.ColorMask = new ColorMask(true, true, true, true);
            context.Draw(PrimitiveType.LinesAdjacency, _shadowVolumeDrawState, sceneState);
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
        private DrawState _shadowVolumeDrawState;
    }
}