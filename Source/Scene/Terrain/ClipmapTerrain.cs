#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using OpenGlobe.Renderer;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Core;
using OpenGlobe.Terrain;
using OpenGlobe.Core.Tessellation;
using System.Drawing;

namespace OpenGlobe.Scene.Terrain
{
    public class ClipmapTerrain : IRenderable, IDisposable
    {
        public ClipmapTerrain(Context context, RasterTerrainSource terrainSource, int clipmapSize)
        {
            _terrainSource = terrainSource;
            _clipmapSize = clipmapSize;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new Level[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                _clipmapLevels[i] = new Level();
                _clipmapLevels[i].Terrain = _terrainSource.Levels[i];
                _clipmapLevels[i].Texture = Device.CreateTexture2D(new Texture2DDescription(_clipmapSize, _clipmapSize, TextureFormat.Red32f));
                _clipmapLevels[i].Texture.Filter = Texture2DFilter.LinearClampToEdge;
            }

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapFS.glsl"));

            _fieldBlockSize = (clipmapSize + 1) / 4; // M

            // Create the MxM block used to fill the ring and the field.
            Mesh fieldBlockMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSize, _fieldBlockSize)),
                _fieldBlockSize - 1, _fieldBlockSize - 1);
            _fieldBlock = context.CreateVertexArray(fieldBlockMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the Mx3 block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSize, 3.0)),
                _fieldBlockSize - 1, 2);
            _ringFixupHorizontal = context.CreateVertexArray(ringFixupHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the 3xM block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(3.0, _fieldBlockSize)),
                2, _fieldBlockSize - 1);
            _ringFixupVertical = context.CreateVertexArray(ringFixupVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fieldBlockSize + 1, 1.0)),
                2 * _fieldBlockSize, 1);
            _offsetStripHorizontal = context.CreateVertexArray(offsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 2 * _fieldBlockSize)),
                1, 2 * _fieldBlockSize - 1);
            _offsetStripVertical = context.CreateVertexArray(offsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh degenerateTriangleMesh = CreateDegenerateTriangleMesh();
            _degenerateTriangles = context.CreateVertexArray(degenerateTriangleMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _scaleFactor = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_scaleFactor"];
            _fineBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_fineBlockOrig"];
            _coarseBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_coarseBlockOrig"];
            _color = (Uniform<Vector3S>)_shaderProgram.Uniforms["u_color"];
            _viewerPos = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_viewerPos"];
            _alphaOffset = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_alphaOffset"];
            _oneOverTransitionWidth = (Uniform<float>)_shaderProgram.Uniforms["u_oneOverTransitionWidth"];

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;
        }

        public void Render(Context context, SceneState sceneState)
        {
            double centerLongitude = sceneState.Camera.Target.X;
            double centerLatitude = sceneState.Camera.Target.Y;

            Level level = _clipmapLevels[_clipmapLevels.Length - 1];
            double longitudeIndex = level.Terrain.LongitudeToIndex(centerLongitude);
            double latitudeIndex = level.Terrain.LatitudeToIndex(centerLatitude);

            int west = (int)(longitudeIndex - _clipmapSize / 2);
            if ((west % 2) != 0)
            {
                ++west;
            }
            int south = (int)(latitudeIndex - _clipmapSize / 2);
            if ((south % 2) != 0)
            {
                ++south;
            }

            level.West = west;
            level.South = south;
            level.OffsetStripOnEast = true;
            level.OffsetStripOnNorth = true;

            for (int i = _clipmapLevels.Length - 2; i >= 0; --i)
            {
                level = _clipmapLevels[i];
                longitudeIndex = level.Terrain.LongitudeToIndex(centerLongitude);
                latitudeIndex = level.Terrain.LatitudeToIndex(centerLatitude);

                Level finerLevel = _clipmapLevels[i + 1];
                level.West = (int)Math.Round(longitudeIndex - _clipmapSize / 2);
                level.OffsetStripOnEast = west != finerLevel.West / 2 - _fieldBlockSize;
                level.South = (int)Math.Round(latitudeIndex - _clipmapSize / 2);
                level.OffsetStripOnNorth = south != finerLevel.South / 2 - _fieldBlockSize;
            }

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RenderLevel(i, context, sceneState);
            }
        }

        private void RenderLevel(int levelIndex, Context context, SceneState sceneState)
        {
            Level level = _clipmapLevels[levelIndex];
            Level coarserLevel = _clipmapLevels[levelIndex > 0 ? levelIndex - 1 : 0];

            int west = level.West;
            int south = level.South;
            int east = west + _clipmapSize - 1;
            int north = south + _clipmapSize - 1;

            short[] posts = new short[_clipmapSize * _clipmapSize];
            level.Terrain.GetPosts(west, south, east, north, posts, 0, _clipmapSize);

            // TODO: This is AWESOME!
            float[] floatPosts = new float[posts.Length];
            for (int i = 0; i < floatPosts.Length; ++i)
            {
                floatPosts[i] = posts[i];
            }

            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, _clipmapSize * _clipmapSize * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(floatPosts);
                level.Texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);
                context.TextureUnits[0].Texture2D = level.Texture;
                context.TextureUnits[1].Texture2D = coarserLevel.Texture;

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSize - 1, south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * (_fieldBlockSize - 1), south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - (_fieldBlockSize - 1), south, context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south + _fieldBlockSize - 1, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - 2 * (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - (_fieldBlockSize - 1), north - 2 * (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSize - 1, north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, west, south + 2 * (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, east - (_fieldBlockSize - 1), south + 2 * (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * (_fieldBlockSize - 1), south, context, sceneState);
                DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);

                int offset = level.OffsetStripOnNorth
                                ? north - _fieldBlockSize
                                : south + _fieldBlockSize - 1;
                DrawBlock(_offsetStripHorizontal, level, coarserLevel, west, south, west + _fieldBlockSize - 1, offset, context, sceneState);

                offset = level.OffsetStripOnEast
                                ? east - _fieldBlockSize
                                : west + _fieldBlockSize - 1;
                DrawBlock(_offsetStripVertical, level, coarserLevel, west, south, offset, south + _fieldBlockSize, context, sceneState);

                DrawBlock(_degenerateTriangles, level, coarserLevel, west, south, west, south, context, sceneState);

                // Fill the center of the highest-detail ring
                if (levelIndex == _clipmapLevels.Length - 1)
                {
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSize - 1, south + _fieldBlockSize - 1, context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSize - 1, south + 2 * (_fieldBlockSize - 1), context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * (_fieldBlockSize - 1), south + 2 * (_fieldBlockSize - 1), context, sceneState);
                }
            }
        }

        private void DrawBlock(VertexArray block, Level level, Level coarserLevel, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            double originLongitude = level.Terrain.IndexToLongitude(blockWest);
            double originLatitude = level.Terrain.IndexToLatitude(blockSouth);

            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            int parentOverallWest = coarserLevel.West * 2;
            int parentOverallSouth = coarserLevel.South * 2;
            double parentTextureWest = blockWest - parentOverallWest;
            double parentTextureSouth = blockSouth - parentOverallSouth;

            DrawState drawState = new DrawState(_renderState, _shaderProgram, block);
            _scaleFactor.Value = new Vector4S((float)level.Terrain.PostDeltaLongitude, (float)level.Terrain.PostDeltaLatitude, (float)originLongitude, (float)originLatitude);
            _fineBlockOrigin.Value = new Vector4S((float)(1.0 / _clipmapSize), (float)(1.0 / _clipmapSize), (float)textureWest / _clipmapSize, (float)textureSouth / _clipmapSize);
            _coarseBlockOrigin.Value = new Vector4S((float)(1.0 / (2 * _clipmapSize)), (float)(1.0 / (2 * _clipmapSize)), (float)(parentTextureWest / (2 * _clipmapSize)), (float)(parentTextureSouth / (2 * _clipmapSize)));
            _viewerPos.Value = sceneState.Camera.Target.XY.ToVector2S();
            double w = _clipmapSize / 10.0f;
            double alphaOffset = (_clipmapSize - 1) / 2.0f - w - 1.0f;
            _alphaOffset.Value = new Vector2S((float)(alphaOffset * level.Terrain.PostDeltaLongitude), (float)(alphaOffset * level.Terrain.PostDeltaLatitude));
            _oneOverTransitionWidth.Value = (float)(1.0 / (w * level.Terrain.PostDeltaLongitude));
            //if (block == _degenerateTriangles)
            //    _color.Value = new Vector3S(1.0f, 0.0f, 0.0f);
            //else
                _color.Value = new Vector3S(0.0f, 1.0f, 0.0f);
            context.Draw(_primitiveType, drawState, sceneState);
        }

        public void Dispose()
        {
        }

        private Mesh CreateDegenerateTriangleMesh()
        {
            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = (_clipmapSize - 2) * 4;
            VertexAttributeDoubleVector2 positionsAttribute = new VertexAttributeDoubleVector2("position", numberOfPositions);
            IList<Vector2D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipmapSize - 1) * 2 * 3;
            IndicesInt16 indices = new IndicesInt16(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipmapSize - 1; ++i)
            {
                positions.Add(new Vector2D(0.0, i));
            }

            for (int i = 0; i < _clipmapSize - 1; ++i)
            {
                positions.Add(new Vector2D(i, _clipmapSize - 1));
            }

            for (int i = _clipmapSize - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(_clipmapSize - 1, i));
            }

            for (int i = _clipmapSize - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(i, _clipmapSize - 1));
            }

            for (int i = 0; i < (short)numberOfIndices; i += 2)
            {
                indices.AddTriangle(new TriangleIndicesInt16((short)i, (short)(i + 1), (short)(i + 2)));
            }

            return mesh;
        }

        private void PostsToBitmap(string filename, float[] posts, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    float elev = posts[j * width + i];
                    if (elev == -9999 || elev == 0.0)
                        bmp.SetPixel(i, j, Color.Black);
                    else if (elev < 0.0)
                        bmp.SetPixel(i, j, Color.Blue);
                    else
                        bmp.SetPixel(i, j, Color.Green);
                }
            }

            bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void PostsToBitmap(string filename, short[] posts, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            for (int j = 0; j < height; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    short elev = posts[j * width + i];
                    if (elev == -9999 || elev == 0)
                        bmp.SetPixel(i, j, Color.Black);
                    else if (elev < 0)
                        bmp.SetPixel(i, j, Color.Blue);
                    else
                        bmp.SetPixel(i, j, Color.Green);
                }
            }

            bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        private class Level
        {
            public RasterTerrainLevel Terrain;
            public Texture2D Texture;
            public int West;
            public int South;
            public bool OffsetStripOnNorth;
            public bool OffsetStripOnEast;
        }

        private RasterTerrainSource _terrainSource;
        private int _clipmapSize;
        private Level[] _clipmapLevels;

        private ShaderProgram _shaderProgram;
        private RenderState _renderState;
        private PrimitiveType _primitiveType;

        private int _fieldBlockSize;
        private VertexArray _fieldBlock;
        private VertexArray _ringFixupHorizontal;
        private VertexArray _ringFixupVertical;
        private VertexArray _offsetStripHorizontal;
        private VertexArray _offsetStripVertical;
        private VertexArray _degenerateTriangles;

        private Uniform<Vector4S> _scaleFactor;
        private Uniform<Vector4S> _fineBlockOrigin;
        private Uniform<Vector4S> _coarseBlockOrigin;
        private Uniform<Vector3S> _color;
        private Uniform<Vector2S> _viewerPos;
        private Uniform<Vector2S> _alphaOffset;
        private Uniform<float> _oneOverTransitionWidth;

    }
}
