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
    public class ClipMapTerrain : IRenderable, IDisposable
    {
        public ClipMapTerrain(Context context, RasterTerrainSource terrainSource, int clipMapSize)
        {
            _terrainSource = terrainSource;
            _clipMapSize = clipMapSize;

            int clipMapLevels = _terrainSource.Levels.Count;
            _clipMapLevels = new Texture2D[clipMapLevels];

            for (int i = 0; i < _clipMapLevels.Length; ++i)
            {
                _clipMapLevels[i] = Device.CreateTexture2D(new Texture2DDescription(_clipMapSize, _clipMapSize, TextureFormat.Red32f));
                _clipMapLevels[i].Filter = Texture2DFilter.NearestClampToEdge;
            }

            _shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipMapTerrain.ClipMapVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipMapTerrain.ClipMapFS.glsl"));

            _fieldBlockSize = (clipMapSize + 1) / 4; // M

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
            int finerWest = -1;
            int finerSouth = -1;
            for (int i = _clipMapLevels.Length - 1; i >= 0; --i)
            {
                int currentWest, currentSouth;
                RenderLevel(i, context, sceneState, finerWest, finerSouth, out currentWest, out currentSouth);
                finerWest = currentWest;
                finerSouth = currentSouth;
            }
        }

        private void RenderLevel(int level, Context context, SceneState sceneState, int finerWest, int finerSouth, out int currentWest, out int currentSouth)
        {
            RasterTerrainLevel levelData = _terrainSource.Levels[level];

            double centerLongitude = sceneState.Camera.Target.X;
            double centerLatitude = sceneState.Camera.Target.Y;

            double longitudeIndex = levelData.LongitudeToIndex(centerLongitude);
            double latitudeIndex = levelData.LatitudeToIndex(centerLatitude);

            int west, south;
            bool offsetStripOnNorth;
            bool offsetStripOnEast;

            if (finerWest == -1)
            {
                // This is the finest level of the clipmap.
                west = (int)longitudeIndex - _clipMapSize / 2;
                south = (int)latitudeIndex - _clipMapSize / 2;

                if ((west % 2) != 0)
                {
                    ++west;
                }
                if ((south % 2) != 0)
                {
                    ++south;
                }

                // Arbitrarily place the offset strips on the north and east sides.
                offsetStripOnNorth = true;
                offsetStripOnEast = true;
            }
            else
            {
                // Place the offset strips appropriately based on the position of the finer level.
                finerWest /= 2;
                finerSouth /= 2;

                double westDesired = longitudeIndex - _clipMapSize / 2;
                int westOption1 = finerWest - (_fieldBlockSize - 1);
                int westOption2 = finerWest - _fieldBlockSize;

                if (Math.Abs(westDesired - westOption1) < Math.Abs(westDesired - westOption2))
                {
                    west = westOption1;
                    offsetStripOnEast = true;
                }
                else
                {
                    west = westOption2;
                    offsetStripOnEast = false;
                }

                double southDesired = latitudeIndex - _clipMapSize / 2;
                int southOption1 = finerSouth - (_fieldBlockSize - 1);
                int southOption2 = finerSouth - _fieldBlockSize;

                if (Math.Abs(southDesired - southOption1) < Math.Abs(southDesired - southOption2))
                {
                    south = southOption1;
                    offsetStripOnNorth = true;
                }
                else
                {
                    south = southOption2;
                    offsetStripOnNorth = false;
                }
            }

            int east = west + _clipMapSize - 1;
            int north = south + _clipMapSize - 1;

            currentWest = west;
            currentSouth = south;

            short[] posts = new short[_clipMapSize * _clipMapSize];
            levelData.GetPosts(west, south, east, north, posts, 0, _clipMapSize);

            float[] floatPosts = new float[posts.Length];
            for (int i = 0; i < floatPosts.Length; ++i)
            {
                floatPosts[i] = posts[i];
            }

            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, _clipMapSize * _clipMapSize * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(floatPosts);
                Texture2D terrainTexture = _clipMapLevels[level];
                terrainTexture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);
                context.TextureUnits[0].Texture2D = terrainTexture;
                Texture2D coarserTexture = level == 0 ? _clipMapLevels[0] : _clipMapLevels[level - 1];
                context.TextureUnits[1].Texture2D = coarserTexture;

                DrawBlock(_fieldBlock, levelData, west, south, west, south, context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, west + _fieldBlockSize - 1, south, context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - 2 * (_fieldBlockSize - 1), south, context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - (_fieldBlockSize - 1), south, context, sceneState);

                DrawBlock(_fieldBlock, levelData, west, south, west, south + _fieldBlockSize - 1, context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);

                DrawBlock(_fieldBlock, levelData, west, south, west, north - 2 * (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - (_fieldBlockSize - 1), north - 2 * (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_fieldBlock, levelData, west, south, west, north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, west + _fieldBlockSize - 1, north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - 2 * (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_fieldBlock, levelData, west, south, east - (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_ringFixupHorizontal, levelData, west, south, west, south + 2 * (_fieldBlockSize - 1), context, sceneState);
                DrawBlock(_ringFixupHorizontal, levelData, west, south, east - (_fieldBlockSize - 1), south + 2 * (_fieldBlockSize - 1), context, sceneState);

                DrawBlock(_ringFixupVertical, levelData, west, south, west + 2 * (_fieldBlockSize - 1), south, context, sceneState);
                DrawBlock(_ringFixupVertical, levelData, west, south, west + 2 * (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);

                int offset = offsetStripOnNorth
                                ? north - _fieldBlockSize
                                : south + _fieldBlockSize - 1;
                DrawBlock(_offsetStripHorizontal, levelData, west, south, west + _fieldBlockSize - 1, offset, context, sceneState);

                offset = offsetStripOnEast
                                ? east - _fieldBlockSize
                                : west + _fieldBlockSize - 1;
                DrawBlock(_offsetStripVertical, levelData, west, south, offset, south + _fieldBlockSize, context, sceneState);

                DrawBlock(_degenerateTriangles, levelData, west, south, west, south, context, sceneState);

                // Fill the center of the highest-detail ring
                if (level == _clipMapLevels.Length - 1)
                {
                    DrawBlock(_fieldBlock, levelData, west, south, west + _fieldBlockSize - 1, south + _fieldBlockSize - 1, context, sceneState);
                    DrawBlock(_fieldBlock, levelData, west, south, west + 2 * (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);
                    DrawBlock(_fieldBlock, levelData, west, south, west + _fieldBlockSize - 1, south + 2 * (_fieldBlockSize - 1), context, sceneState);
                    DrawBlock(_fieldBlock, levelData, west, south, west + 2 * (_fieldBlockSize - 1), south + 2 * (_fieldBlockSize - 1), context, sceneState);
                }
            }
        }

        private void DrawBlock(VertexArray block, RasterTerrainLevel levelData, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            double originLongitude = levelData.IndexToLongitude(blockWest);
            double originLatitude = levelData.IndexToLatitude(blockSouth);

            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            double parentOverallWest = overallWest / 2.0;
            double parentOverallSouth = overallSouth / 2.0;
            double parentBlockWest = blockWest / 2.0;
            double parentBlockSouth = blockSouth / 2.0;
            double parentTextureWest = parentBlockWest - parentOverallWest;
            double parentTextureSouth = parentBlockSouth - parentOverallSouth;

            DrawState drawState = new DrawState(_renderState, _shaderProgram, block);
            _scaleFactor.Value = new Vector4S((float)levelData.PostDeltaLongitude, (float)levelData.PostDeltaLatitude, (float)originLongitude, (float)originLatitude);
            _fineBlockOrigin.Value = new Vector4S((float)(1.0 / _clipMapSize), (float)(1.0 / _clipMapSize), (float)textureWest / _clipMapSize, (float)textureSouth / _clipMapSize);
            _coarseBlockOrigin.Value = new Vector4S((float)(1.0 / (2 * _clipMapSize)), (float)(1.0 / (2 * _clipMapSize)), (float)parentTextureWest / (_clipMapSize * 2), (float)parentTextureSouth / (_clipMapSize * 2));
            _viewerPos.Value = sceneState.Camera.Target.XY.ToVector2S();
            float w = _clipMapSize / 10.0f;
            float alphaOffset = (_clipMapSize - 1) / 2.0f - w - 1.0f;
            _alphaOffset.Value = new Vector2S(alphaOffset, alphaOffset);
            _oneOverTransitionWidth.Value = 1.0f / w;
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

            int numberOfPositions = (_clipMapSize - 2) * 4;
            VertexAttributeDoubleVector2 positionsAttribute = new VertexAttributeDoubleVector2("position", numberOfPositions);
            IList<Vector2D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (_clipMapSize - 1) * 2 * 3;
            IndicesInt16 indices = new IndicesInt16(numberOfIndices);
            mesh.Indices = indices;

            for (int i = 0; i < _clipMapSize - 1; ++i)
            {
                positions.Add(new Vector2D(0.0, i));
            }

            for (int i = 0; i < _clipMapSize - 1; ++i)
            {
                positions.Add(new Vector2D(i, _clipMapSize - 1));
            }

            for (int i = _clipMapSize - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(_clipMapSize - 1, i));
            }

            for (int i = _clipMapSize - 1; i > 0; --i)
            {
                positions.Add(new Vector2D(i, _clipMapSize - 1));
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

        private RasterTerrainSource _terrainSource;
        private int _clipMapSize;
        private Texture2D[] _clipMapLevels;
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
