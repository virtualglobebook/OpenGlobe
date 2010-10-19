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

            _scaleFactor = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_scaleFactor"];
            _fineBlockOrigin = (Uniform<Vector4S>)_shaderProgram.Uniforms["u_fineBlockOrig"];

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = fieldBlockMesh.FrontFaceWindingOrder;
            _primitiveType = fieldBlockMesh.PrimitiveType;
        }

        double centerLongitude = -119.5326056;
        double centerLatitude = 37.74451389;
        //double altitude = 3112.9224;

        //double centerLongitude = -115.21470277777777777777777777778;
        //double centerLatitude = 28.164322222222222222222222222222;

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

            //short[] blah = new short[2700 * 1350];

            //levelData.GetPosts(0, 0, 2699, 1349, blah, 0, 2700);
            //levelData.GetPosts(0, 0, 149, 159, blah, 0, 2700);
            //levelData.GetPosts(0, 1190, 149, 1349, blah, 1190*2700, 2700);
            //levelData.GetPosts(0, 0, 159, 149, blah, 0, 2700);

            /*int j;
            for (j = 0; (j + 254) < 1350; j += 255)
            {
                int i;
                for (i = 0; (i + 254) < 2700; i += 255)
                {
                    levelData.GetPosts(i, j, i + 254, j + 254, blah, (1094 - j) * 2700 + i, 2700);
                }
                //levelData.GetPosts(i, j, 2699, j + 254, blah, (1094 - j) * 2700 + i, 2700);
            }*/
            /*int k;
            for (k = 0; (k + 254) < 2700; k += 255)
            {
                levelData.GetPosts(k, j, k + 254, 1349, blah, k, 2700);
            }
            levelData.GetPosts(k, j, 2699, 1349, blah, k, 2700);*/


            //PostsToBitmap("InParts.png", blah, 2700, 1350);

            int longitudeIndex = levelData.LongitudeToIndex(centerLongitude);
            int west = longitudeIndex - _clipMapSize / 2;
            int east = west + _clipMapSize - 1;

            int latitudeIndex = levelData.LatitudeToIndex(centerLatitude);
            int south = latitudeIndex - _clipMapSize / 2;
            int north = south + _clipMapSize - 1;

            short[] posts = new short[_clipMapSize * _clipMapSize];
            levelData.GetPosts(west, south, east, north, posts, 0, _clipMapSize);

            float[] floatPosts = new float[posts.Length];
            for (int i = 0; i < floatPosts.Length; ++i)
            {
                floatPosts[i] = posts[i];
            }

            //PostsToBitmap(level.ToString() + ".png", floatPosts, _clipMapSize, _clipMapSize);

            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw, _clipMapSize * _clipMapSize * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(floatPosts);
                Texture2D terrainTexture = _clipMapLevels[level];
                terrainTexture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
                context.TextureUnits[0].Texture2D = terrainTexture;

                DrawFieldBlock(levelData, west, south, west, south, context, sceneState);
                DrawFieldBlock(levelData, west, south, west + _fieldBlockSize - 1, south, context, sceneState);
                DrawFieldBlock(levelData, west, south, east - 2 * (_fieldBlockSize - 1), south, context, sceneState);
                DrawFieldBlock(levelData, west, south, east - (_fieldBlockSize - 1), south, context, sceneState);

                DrawFieldBlock(levelData, west, south, west, south + _fieldBlockSize - 1, context, sceneState);
                DrawFieldBlock(levelData, west, south, east - (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);

                DrawFieldBlock(levelData, west, south, west, north - 2 * (_fieldBlockSize - 1), context, sceneState);
                DrawFieldBlock(levelData, west, south, east - (_fieldBlockSize - 1), north - 2 * (_fieldBlockSize - 1), context, sceneState);

                DrawFieldBlock(levelData, west, south, west, north - (_fieldBlockSize - 1), context, sceneState);
                DrawFieldBlock(levelData, west, south, west + _fieldBlockSize - 1, north - (_fieldBlockSize - 1), context, sceneState);
                DrawFieldBlock(levelData, west, south, east - 2 * (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);
                DrawFieldBlock(levelData, west, south, east - (_fieldBlockSize - 1), north - (_fieldBlockSize - 1), context, sceneState);

                if (level == _clipMapLevels.Length - 1)
                {
                    DrawFieldBlock(levelData, west, south, west + _fieldBlockSize - 1, south + _fieldBlockSize - 1, context, sceneState);
                    DrawFieldBlock(levelData, west, south, east - 2 * (_fieldBlockSize - 1), south + _fieldBlockSize - 1, context, sceneState);
                    DrawFieldBlock(levelData, west, south, west + _fieldBlockSize - 1, north - 2 * (_fieldBlockSize - 1), context, sceneState);
                    DrawFieldBlock(levelData, west, south, east - 2 * (_fieldBlockSize - 1), north - 2 * (_fieldBlockSize - 1), context, sceneState);
                }
            }
        }

        private void DrawFieldBlock(RasterTerrainLevel levelData, int overallWest, int overallSouth, int blockWest, int blockSouth, Context context, SceneState sceneState)
        {
            double originLongitude = levelData.IndexToLongitude(blockWest) - centerLongitude;
            double originLatitude = levelData.IndexToLatitude(blockSouth) - centerLatitude;

            int textureWest = blockWest - overallWest;
            int textureSouth = blockSouth - overallSouth;

            DrawState drawState = new DrawState(_renderState, _shaderProgram, _fieldBlock);
            _scaleFactor.Value = new Vector4S((float)levelData.PostDeltaLongitude, (float)levelData.PostDeltaLatitude, (float)originLongitude, (float)originLatitude);
            _fineBlockOrigin.Value = new Vector4S((float)(1.0 / _clipMapSize), (float)(1.0 / _clipMapSize), (float)textureWest / _clipMapSize, (float)textureSouth / _clipMapSize);
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
        //private VertexArray _vertexArray;
        //private Mesh _mesh;

        private int _fieldBlockSize;
        private VertexArray _fieldBlock;
        private VertexArray _ringFixupHorizontal;
        private VertexArray _ringFixupVertical;
        private VertexArray _interiorTrimTopLeft;
        private VertexArray _interiorTrimTopRight;
        private VertexArray _interiorTrimBottomLeft;
        private VertexArray _interiorTrimBottomRight;
        private VertexArray _degenerateRing;

        private Uniform<Vector4S> _scaleFactor;
        private Uniform<Vector4S> _fineBlockOrigin;
    }
}
