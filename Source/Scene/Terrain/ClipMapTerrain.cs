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

            _scaleFactor = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_scaleFactor"];
            _fineBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_fineBlockOrig"];
            _color = (Uniform<Vector3S>)_shaderProgram.Uniforms["u_color"];

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;
        }

        public void Render(Context context, SceneState sceneState)
        {
            for (int i = 0; i < _clipMapLevels.Length; ++i)
            {
                RenderLevel(i, context, sceneState);
            }
        }

        private void RenderLevel(int level, Context context, SceneState sceneState)
        {
            RasterTerrainLevel levelData = _terrainSource.Levels[level];

            double centerLongitude = sceneState.Camera.Target.X;
            double centerLatitude = sceneState.Camera.Target.Y;

            // Compute the post indices of this clipmap level.  LongitudeToIndex and LatitudeToIndex
            // return the post at or immediately to the southwest of the specified coordinate.
            // Note that the indices must be even in order to align with the next-coarser level.
            // If they're odd, we'll bump to the northeast.

            int longitudeIndex = levelData.LongitudeToIndex(centerLongitude);
            int west = longitudeIndex - _clipMapSize / 2;
            bool bumpedLongitude = false;
            if ((west % 2) != 0)
            {
                ++west;
                bumpedLongitude = true;
            }
            int east = west + _clipMapSize - 1;

            int latitudeIndex = levelData.LatitudeToIndex(centerLatitude);
            int south = latitudeIndex - _clipMapSize / 2;
            bool bumpedLatitude = false;
            if ((south % 2) != 0)
            {
                ++south;
                bumpedLatitude = true;
            }
            int north = south + _clipMapSize - 1;

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

                if (!bumpedLatitude)
                {
                    //Console.WriteLine("top");
                    DrawBlock(_offsetStripHorizontal, levelData, west, south, west + _fieldBlockSize - 1, south + _fieldBlockSize - 1, context, sceneState);
                }
                else
                {
                    //Console.WriteLine("bottom");
                    DrawBlock(_offsetStripHorizontal, levelData, west, south, west + _fieldBlockSize - 1, north - _fieldBlockSize, context, sceneState);
                }

                if (!bumpedLongitude)
                {
                    //Console.WriteLine("left");
                    DrawBlock(_offsetStripVertical, levelData, west, south, west + _fieldBlockSize - 1, south + _fieldBlockSize, context, sceneState);
                }
                else
                {
                    //Console.WriteLine("right");
                    DrawBlock(_offsetStripVertical, levelData, west, south, east - _fieldBlockSize, south + _fieldBlockSize, context, sceneState);
                }

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

            //if (blockSouth == overallSouth && blockWest == overallWest)
            //{
            //    Console.WriteLine("({0}) {1}\t{2}", _terrainSource.Levels.IndexOf(levelData), overallWest, overallSouth);
            //}

            DrawState drawState = new DrawState(_renderState, _shaderProgram, block);
            _scaleFactor.Value = new Vector4S((float)levelData.PostDeltaLongitude, (float)levelData.PostDeltaLatitude, (float)originLongitude, (float)originLatitude);
            _fineBlockOrigin.Value = new Vector4S((float)(1.0 / _clipMapSize), (float)(1.0 / _clipMapSize), (float)textureWest / _clipMapSize, (float)textureSouth / _clipMapSize);
            if (block == _offsetStripHorizontal || block == _offsetStripVertical)
                _color.Value = new Vector3S(1.0f, 0.0f, 0.0f);
            else
                _color.Value = new Vector3S(0.0f, 1.0f, 0.0f);
            context.Draw(_primitiveType, drawState, sceneState);
        }

        public void Dispose()
        {
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

        private Uniform<Vector4S> _scaleFactor;
        private Uniform<Vector4S> _fineBlockOrigin;
        private Uniform<Vector3S> _color;
    }
}
