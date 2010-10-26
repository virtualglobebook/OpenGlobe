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
using OpenTK;

namespace OpenGlobe.Scene.Terrain
{
    public class ClipmapTerrain : IRenderable, IDisposable
    {
        public ClipmapTerrain(Context context, RasterTerrainSource terrainSource, int clipmapPosts, EsriRestImagery imagery)
        {
            _terrainSource = terrainSource;
            _clipmapPosts = clipmapPosts;
            _clipmapSegments = _clipmapPosts - 1;
            _imagery = imagery;

            int clipmapLevels = _terrainSource.Levels.Count;
            _clipmapLevels = new Level[clipmapLevels];

            for (int i = 0; i < _clipmapLevels.Length; ++i)
            {
                RasterTerrainLevel terrainLevel = _terrainSource.Levels[i];
                _clipmapLevels[i] = new Level();
                _clipmapLevels[i].Terrain = terrainLevel;
                _clipmapLevels[i].TerrainTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_clipmapPosts, _clipmapPosts, TextureFormat.Red32f));
                _clipmapLevels[i].TerrainTexture.Filter = Texture2DFilter.LinearClampToEdge;

                // Aim for roughly one imagery texel per geometry texel.
                // Find the first imagery level that meets our resolution needs.
                double longitudeResRequired = terrainLevel.PostDeltaLongitude;
                double latitudeResRequired = terrainLevel.PostDeltaLatitude;
                EsriRestImageryLevel imageryLevel = null;
                for (int j = 0; j < _imagery.Levels.Count; ++j)
                {
                    imageryLevel = _imagery.Levels[j];
                    if (imageryLevel.PostDeltaLongitude <= longitudeResRequired &&
                        imageryLevel.PostDeltaLatitude <= latitudeResRequired)
                    {
                        break;
                    }
                }

                _clipmapLevels[i].Imagery = imageryLevel;
                _clipmapLevels[i].ImageryWidth = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLongitude / imageryLevel.PostDeltaLongitude);
                _clipmapLevels[i].ImageryHeight = (int)Math.Ceiling(_clipmapPosts * terrainLevel.PostDeltaLatitude / imageryLevel.PostDeltaLatitude);
                _clipmapLevels[i].ImageryTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_clipmapLevels[i].ImageryWidth, _clipmapLevels[i].ImageryHeight, TextureFormat.RedGreenBlue8, false));
                _clipmapLevels[i].ImageryTexture.Filter = Texture2DFilter.LinearClampToEdge;
            }

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapFS.glsl"));

            _fieldBlockPosts = (clipmapPosts + 1) / 4; // M
            _fieldBlockSegments = _fieldBlockPosts - 1;

            // Create the MxM block used to fill the ring and the field.
            Mesh fieldBlockMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSegments, _fieldBlockSegments)),
                _fieldBlockSegments, _fieldBlockSegments);
            _fieldBlock = context.CreateVertexArray(fieldBlockMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the Mx3 block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(_fieldBlockSegments, 2.0)),
                _fieldBlockSegments, 2);
            _ringFixupHorizontal = context.CreateVertexArray(ringFixupHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            // Create the 3xM block used to fill the space between the MxM blocks in the ring
            Mesh ringFixupVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, _fieldBlockSegments)),
                2, _fieldBlockSegments);
            _ringFixupVertical = context.CreateVertexArray(ringFixupVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fieldBlockPosts, 1.0)),
                2 * _fieldBlockPosts, 1);
            _offsetStripHorizontal = context.CreateVertexArray(offsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh offsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 2 * _fieldBlockPosts - 1)),
                1, 2 * _fieldBlockPosts - 1);
            _offsetStripVertical = context.CreateVertexArray(offsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh finestOffsetStripHorizontalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2 * _fieldBlockPosts, 2.0)),
                2 * _fieldBlockPosts, 2);
            _finestOffsetStripHorizontal = context.CreateVertexArray(finestOffsetStripHorizontalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh finestOffsetStripVerticalMesh = RectangleTessellator.Compute(
                new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(2.0, 2 * _fieldBlockPosts - 1)),
                2, 2 * _fieldBlockPosts - 1);
            _finestOffsetStripVertical = context.CreateVertexArray(finestOffsetStripVerticalMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            Mesh degenerateTriangleMesh = CreateDegenerateTriangleMesh();
            _degenerateTriangles = context.CreateVertexArray(degenerateTriangleMesh, _shaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _gridScaleFactor = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_gridScaleFactor"];
            _worldScaleFactor = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_worldScaleFactor"];
            _fineBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_fineBlockOrig"];
            _coarseBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_coarseBlockOrig"];
            _color = (Uniform<Vector3S>)_shaderProgram.Uniforms["u_color"];
            _viewerPos = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_viewerPos"];
            _alphaOffset = (Uniform<Vector2S>)_shaderProgram.Uniforms["u_alphaOffset"];
            _oneOverTransitionWidth = (Uniform<float>)_shaderProgram.Uniforms["u_oneOverTransitionWidth"];
            _textureOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_textureOrigin"];

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

            int west = (int)(longitudeIndex - _clipmapPosts / 2);
            if ((west % 2) != 0)
            {
                ++west;
            }
            int south = (int)(latitudeIndex - _clipmapPosts / 2);
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
                Level finerLevel = _clipmapLevels[i + 1];

                level.West = finerLevel.West / 2 - _fieldBlockSegments;
                level.OffsetStripOnEast = (level.West % 2) == 0;
                if (!level.OffsetStripOnEast)
                {
                    --level.West;
                }

                level.South = finerLevel.South / 2 - _fieldBlockSegments;
                level.OffsetStripOnNorth = (level.South % 2) == 0;
                if (!level.OffsetStripOnNorth)
                {
                    --level.South;
                }
            }

            for (int i = _clipmapLevels.Length - 1; i >= 0; --i)
            {
                //if (i > 1)
                //    continue;
                RenderLevel(i, context, sceneState);
            }
        }

        private void RenderLevel(int levelIndex, Context context, SceneState sceneState)
        {
            Level level = _clipmapLevels[levelIndex];
            Level coarserLevel = _clipmapLevels[levelIndex > 0 ? levelIndex - 1 : 0];

            int west = level.West;
            int south = level.South;
            int east = west + _clipmapPosts - 1;
            int north = south + _clipmapPosts - 1;

            short[] posts = new short[_clipmapPosts * _clipmapPosts];
            level.Terrain.GetPosts(west, south, east, north, posts, 0, _clipmapPosts);

            // TODO: This is AWESOME!
            float[] floatPosts = new float[posts.Length];
            for (int i = 0; i < floatPosts.Length; ++i)
            {
                floatPosts[i] = posts[i];
            }

            // Map the terrain posts indices to imagery post indices and offsets
            double imageryWestIndex = level.Imagery.LongitudeToIndex(level.Terrain.IndexToLongitude(level.West));
            double imagerySouthIndex = level.Imagery.LatitudeToIndex(level.Terrain.IndexToLatitude(level.South));
            int imageryEastIndex = (int)imageryWestIndex + level.ImageryWidth - 1;
            int imageryNorthIndex = (int)imagerySouthIndex + level.ImageryHeight - 1;

            double imageryWestTerrainPostOffset = imageryWestIndex - (int)imageryWestIndex;
            double imagerySouthTerrainPostOffset = imagerySouthIndex - (int)imagerySouthIndex;

            _textureOrigin.Value = new Vector4S((float)(level.Terrain.PostDeltaLongitude / level.Imagery.PostDeltaLongitude),
                                                (float)(level.Terrain.PostDeltaLatitude / level.Imagery.PostDeltaLatitude),
                                                (float)imageryWestTerrainPostOffset,
                                                (float)imagerySouthTerrainPostOffset);

            double longitudeOffset = sceneState.Camera.Target.X - level.Terrain.IndexToLongitude(level.West);
            double latitudeOffset = sceneState.Camera.Target.Y - level.Terrain.IndexToLatitude(level.South);

            byte[] imagery = level.Imagery.GetImage((int)imageryWestIndex, (int)imagerySouthIndex, imageryEastIndex, imageryNorthIndex);

            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, _clipmapPosts * _clipmapPosts * sizeof(float)))
            using (WritePixelBuffer imageryPixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, imagery.Length))
            {
                pixelBuffer.CopyFromSystemMemory(floatPosts);
                level.TerrainTexture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);
                context.TextureUnits[0].Texture2DRectangle = level.TerrainTexture;
                context.TextureUnits[1].Texture2DRectangle = coarserLevel.TerrainTexture;

                imageryPixelBuffer.CopyFromSystemMemory(imagery);
                level.ImageryTexture.CopyFromBuffer(imageryPixelBuffer, ImageFormat.BlueGreenRed, ImageDatatype.UnsignedByte);
                context.TextureUnits[2].Texture2DRectangle = level.ImageryTexture;

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments, south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * _fieldBlockSegments, south, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, south, context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, south + _fieldBlockSegments, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, south + _fieldBlockSegments, context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - 2 * _fieldBlockSegments, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, north - 2 * _fieldBlockSegments, context, sceneState);

                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west, north - _fieldBlockSegments, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - 2 * _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);
                DrawBlock(_fieldBlock, level, coarserLevel, west, south, east - _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);

                DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, west, south + 2 * _fieldBlockSegments, context, sceneState);
                DrawBlock(_ringFixupHorizontal, level, coarserLevel, west, south, east - _fieldBlockSegments, south + 2 * _fieldBlockSegments, context, sceneState);

                DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments, south, context, sceneState);
                DrawBlock(_ringFixupVertical, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments, north - _fieldBlockSegments, context, sceneState);

                DrawBlock(_degenerateTriangles, level, coarserLevel, west, south, west, south, context, sceneState);

                // Fill the center of the highest-detail ring
                if (levelIndex == _clipmapLevels.Length - 1)
                {
                    int westOffset = level.OffsetStripOnEast ? 0 : 2;
                    int southOffset = level.OffsetStripOnNorth ? 0 : 2;

                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments + westOffset, south + _fieldBlockSegments + southOffset, context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments + westOffset, south + _fieldBlockSegments + southOffset, context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + _fieldBlockSegments + westOffset, south + 2 * _fieldBlockSegments + southOffset, context, sceneState);
                    DrawBlock(_fieldBlock, level, coarserLevel, west, south, west + 2 * _fieldBlockSegments + westOffset, south + 2 * _fieldBlockSegments + southOffset, context, sceneState);

                    int offset = level.OffsetStripOnNorth
                                    ? north - _fieldBlockPosts - 1
                                    : south + _fieldBlockSegments;
                    DrawBlock(_finestOffsetStripHorizontal, level, coarserLevel, west, south, west + _fieldBlockSegments, offset, context, sceneState);

                    offset = level.OffsetStripOnEast
                                    ? east - _fieldBlockPosts - 1
                                    : west + _fieldBlockSegments;
                    DrawBlock(_finestOffsetStripVertical, level, coarserLevel, west, south, offset, south + _fieldBlockSegments + southOffset, context, sceneState);
                }
                else
                {
                    int offset = level.OffsetStripOnNorth
                                    ? north - _fieldBlockPosts
                                    : south + _fieldBlockSegments;
                    DrawBlock(_offsetStripHorizontal, level, coarserLevel, west, south, west + _fieldBlockSegments, offset, context, sceneState);

                    int southOffset = level.OffsetStripOnNorth ? 0 : 1;
                    offset = level.OffsetStripOnEast
                                    ? east - _fieldBlockPosts
                                    : west + _fieldBlockSegments;
                    DrawBlock(_offsetStripVertical, level, coarserLevel, west, south, offset, south + _fieldBlockSegments + southOffset, context, sceneState);
                }
            }
        }

        private void DrawBlock(VertexArray block, Level level, Level coarserLevel, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            double blockOriginLongitude = level.Terrain.IndexToLongitude(blockWest);
            double blockOriginLatitude = level.Terrain.IndexToLatitude(blockSouth);

            double overallOriginLongitude = level.Terrain.IndexToLongitude(overallWest);
            double overallOriginLatitude = level.Terrain.IndexToLatitude(overallSouth);

            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            int parentOverallWest = coarserLevel.West * 2;
            int parentOverallSouth = coarserLevel.South * 2;
            double parentTextureWest = blockWest - parentOverallWest;
            double parentTextureSouth = blockSouth - parentOverallSouth;

            DrawState drawState = new DrawState(_renderState, _shaderProgram, block);

            // TODO: Pass this in instead of computing it.
            int levelIndex = Array.IndexOf(_clipmapLevels, level);
            double resolution = Math.Pow(2.0, -levelIndex);

            // Scale our convenient power-of-two coordinates up to world coordinates.
            double xScale = _clipmapLevels[0].Terrain.PostDeltaLongitude;
            double yScale = _clipmapLevels[0].Terrain.PostDeltaLatitude;
            double xOffset = _terrainSource.Extent.West;
            double yOffset = _terrainSource.Extent.South;

            _gridScaleFactor.Value = new Vector4S((float)resolution, (float)resolution, (float)(blockWest * resolution), (float)(blockSouth * resolution));
            _worldScaleFactor.Value = new Vector4S((float)xScale, (float)yScale, (float)xOffset, (float)yOffset);
            _fineBlockOrigin.Value = new Vector4S((float)(1.0 / _clipmapPosts), (float)(1.0 / _clipmapPosts), (float)textureWest, (float)textureSouth);
            _coarseBlockOrigin.Value = new Vector4S((float)(1.0 / (2 * _clipmapPosts)), (float)(1.0 / (2 * _clipmapPosts)), (float)(parentTextureWest / 2), (float)(parentTextureSouth / 2));

            _viewerPos.Value = new Vector2S(_clipmapSegments / 2.0f - textureWest, _clipmapSegments / 2.0f - textureSouth);

            double w = _clipmapPosts / 10.0f;
            double alphaOffset = (_clipmapPosts - 1) / 2.0f - w - 1.0f;
            _alphaOffset.Value = new Vector2S((float)alphaOffset, (float)alphaOffset);
            if (level != coarserLevel)
                _oneOverTransitionWidth.Value = (float)(1.0 / w);
            else
                _oneOverTransitionWidth.Value = 0.0f;

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

            int numberOfPositions = _clipmapSegments * 4;
            VertexAttributeDoubleVector2 positionsAttribute = new VertexAttributeDoubleVector2("position", numberOfPositions);
            IList<Vector2D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipmapSegments / 2) * 3 * 4;
            IndicesUnsignedShort indices = new IndicesUnsignedShort(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2D(0.0, i));
            }

            for (int i = 1; i < _clipmapPosts; ++i)
            {
                positions.Add(new Vector2D(i, _clipmapSegments));
            }

            for (int i = _clipmapSegments - 1; i >= 0; --i)
            {
                positions.Add(new Vector2D(_clipmapSegments, i));
            }

            for (int i = _clipmapSegments - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(i, 0.0));
            }

            for (int i = 0; i < numberOfIndices; i += 2)
            {
                indices.AddTriangle(new TriangleIndicesUnsignedShort((ushort)i, (ushort)(i + 1), (ushort)(i + 2)));
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
            public Texture2D TerrainTexture;
            public EsriRestImageryLevel Imagery;
            public Texture2D ImageryTexture;
            public int ImageryWidth;
            public int ImageryHeight;
            public int West;
            public int South;
            public bool OffsetStripOnNorth;
            public bool OffsetStripOnEast;
        }

        private RasterTerrainSource _terrainSource;
        private int _clipmapPosts;
        private int _clipmapSegments;
        private Level[] _clipmapLevels;

        private ShaderProgram _shaderProgram;
        private RenderState _renderState;
        private PrimitiveType _primitiveType;

        private int _fieldBlockPosts;
        private int _fieldBlockSegments;

        private VertexArray _fieldBlock;
        private VertexArray _ringFixupHorizontal;
        private VertexArray _ringFixupVertical;
        private VertexArray _offsetStripHorizontal;
        private VertexArray _offsetStripVertical;
        private VertexArray _finestOffsetStripHorizontal;
        private VertexArray _finestOffsetStripVertical;
        private VertexArray _degenerateTriangles;

        private Uniform<Vector4S> _gridScaleFactor;
        private Uniform<Vector4S> _worldScaleFactor;
        private Uniform<Vector4S> _fineBlockOrigin;
        private Uniform<Vector4S> _coarseBlockOrigin;
        private Uniform<Vector3S> _color;
        private Uniform<Vector2S> _viewerPos;
        private Uniform<Vector2S> _alphaOffset;
        private Uniform<float> _oneOverTransitionWidth;
        private Uniform<Vector4S> _textureOrigin;

        private EsriRestImagery _imagery;
    }
}
