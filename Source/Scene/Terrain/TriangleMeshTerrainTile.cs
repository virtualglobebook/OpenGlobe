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
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Scene;
using MiniGlobe.Renderer;
using System.Collections.Generic;

namespace MiniGlobe.Terrain
{
    public sealed class TriangleMeshTerrainTile : IDisposable
    {
        public TriangleMeshTerrainTile(Context context, TerrainTile tile)
        {
            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.TriangleMeshTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.TriangleMeshTerrainTile.TerrainFS.glsl"));

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _minimumHeight = sp.Uniforms["u_minimumHeight"] as Uniform<float>;
            _maximumHeight = sp.Uniforms["u_maximumHeight"] as Uniform<float>;
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = tile.Size.X * tile.Size.Y;
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfPositions);
            IList<Vector3D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfPartitionsX = tile.Size.X - 1;
            int numberOfPartitionsY = tile.Size.Y - 1;
            int numberOfIndices = (numberOfPartitionsX * numberOfPartitionsY) * 6;
            IndicesInt32 indices = new IndicesInt32(numberOfIndices);
            mesh.Indices = indices;

            //
            // Positions
            //
            Vector2D lowerLeft = tile.Extent.LowerLeft;
            Vector2D toUpperRight = tile.Extent.UpperRight - lowerLeft;

            int heightIndex = 0;
            for (int y = 0; y <= numberOfPartitionsY; ++y)
            {
                double deltaY = y / (double)numberOfPartitionsY;
                double currentY = lowerLeft.Y + (deltaY * toUpperRight.Y);

                for (int x = 0; x <= numberOfPartitionsX; ++x)
                {
                    double deltaX = x / (double)numberOfPartitionsX;
                    double currentX = lowerLeft.X + (deltaX * toUpperRight.X);
                    positions.Add(new Vector3D(currentX, currentY, tile.Heights[heightIndex++]));
                }
            }

            //
            // Indices
            //
            int rowDelta = numberOfPartitionsX + 1;
            int i = 0;
            for (int y = 0; y < numberOfPartitionsY; ++y)
            {
                for (int x = 0; x < numberOfPartitionsX; ++x)
                {
                    indices.AddTriangle(new TriangleIndicesInt32(i, i + 1, rowDelta + (i + 1)));
                    indices.AddTriangle(new TriangleIndicesInt32(i, rowDelta + (i + 1), rowDelta + i));
                    i += 1;
                }
                i += 1;
            }

            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _drawState.ShaderProgram = sp;
            _drawState.VertexArray = context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);

            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration", "HeightExaggeration must be greater than zero.");
                }

                if (_heightExaggeration.Value != value)
                {
                    //
                    // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                    // scale will incorrectly move the entire tile.
                    //
                    _heightExaggeration.Value = value;
                    _minimumHeight.Value = _tileMinimumHeight * value;
                    _maximumHeight.Value = _tileMaximumHeight * value;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;

        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private readonly PrimitiveType _primitiveType;
    }
}