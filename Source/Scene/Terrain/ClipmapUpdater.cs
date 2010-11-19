using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene.Terrain
{
    internal class ClipmapUpdater : IDisposable
    {
        public ClipmapUpdater(Context context, int maxUpdate)
        {
            _maxUpdate = maxUpdate;

            ShaderProgram shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateFS.glsl"));
            _heightOutput = shaderProgram.FragmentOutputs["heightOutput"];
            _normalOutput = shaderProgram.FragmentOutputs["normalOutput"];

            _sourceDimensions = (Uniform<Vector2F>)shaderProgram.Uniforms["u_sourceDimensions"];
            _heightExaggeration = (Uniform<float>)shaderProgram.Uniforms["u_heightExaggeration"];
            _postDelta = (Uniform<float>)shaderProgram.Uniforms["u_postDelta"];
            _updateOrigin = (Uniform<Vector2F>)shaderProgram.Uniforms["u_updateOrigin"];

            Mesh quad = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.0, 0.0), new Vector2D(1.0, 1.0)), 1, 1);
            VertexArray quadVertexArray = context.CreateVertexArray(quad, shaderProgram.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = quad.PrimitiveType;

            _sourceTexture = Device.CreateTexture2DRectangle(new Texture2DDescription(_maxUpdate, _maxUpdate, TextureFormat.Red32f));

            _drawState = new DrawState(new RenderState(), shaderProgram, quadVertexArray);
            _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = quad.FrontFaceWindingOrder;
            _drawState.RenderState.DepthTest.Enabled = false;
            _sceneState = new SceneState();

            _frameBuffer = context.CreateFrameBuffer();

            HeightExaggeration = 1.0f;
        }

        public void Dispose()
        {
            _frameBuffer.Dispose();
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
            _sourceTexture.Dispose();
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set { _heightExaggeration.Value = value; }
        }

        public void Update(Context context, ClipmapLevel level, ClipmapUpdate update)
        {
            int clipmapSize = level.NextExtent.East - level.NextExtent.West + 1;

            int west = (level.OriginInTextures.X + (update.West - level.NextExtent.West)) % clipmapSize;
            int east = (level.OriginInTextures.X + (update.East - level.NextExtent.West)) % clipmapSize;
            int south = (level.OriginInTextures.Y + (update.South - level.NextExtent.South)) % clipmapSize;
            int north = (level.OriginInTextures.Y + (update.North - level.NextExtent.South)) % clipmapSize;

            // We can't cross the texture boundary with one write, so split this
            // into two updates if necessary.
            if (east < west)
            {
                ClipmapUpdate leftUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    level.NextExtent.West + (clipmapSize - level.OriginInTextures.X - 1),
                    update.North);
                Update(context, level, leftUpdate);

                ClipmapUpdate rightUpdate = new ClipmapUpdate(
                    level,
                    level.NextExtent.West + clipmapSize - level.OriginInTextures.X,
                    update.South,
                    update.East,
                    update.North);
                Update(context, level, rightUpdate);
                return;
            }
            else if (north < south)
            {
                ClipmapUpdate bottomUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    update.South,
                    update.East,
                    level.NextExtent.South + (clipmapSize - level.OriginInTextures.Y - 1));
                Update(context, level, bottomUpdate);

                ClipmapUpdate topUpdate = new ClipmapUpdate(
                    level,
                    update.West,
                    level.NextExtent.South + clipmapSize - level.OriginInTextures.Y,
                    update.East,
                    update.North);
                Update(context, level, topUpdate);
                return;
            }

            float[] posts = new float[(update.Width + 2) * (update.Height + 2)];

            level.Terrain.GetPosts(update.West - 1, update.South - 1, update.East + 1, update.North + 1, posts, 0, update.Width + 2);
            Update(context, level.HeightTexture, level.NormalTexture, (float)level.Terrain.PostDeltaLongitude, west, south, update.Width, update.Height, posts);
        }

        public void Update(Context context, Texture2D heightMap, Texture2D normalMap, float postDelta, int x, int y, int width, int height, float[] posts)
        {
            // Copy the post data into the source texture.
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, posts.Length * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(posts);
                _sourceTexture.CopyFromBuffer(pixelBuffer, 0, 0, width + 2, height + 2, ImageFormat.Red, ImageDatatype.Float, 4);
            }

            context.TextureUnits[0].Texture = _sourceTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
            _postDelta.Value = postDelta;

            // Set the target location
            _frameBuffer.ColorAttachments[_heightOutput] = heightMap;
            _frameBuffer.ColorAttachments[_normalOutput] = normalMap;
            _updateOrigin.Value = new Vector2F(x, y);
            _sourceDimensions.Value = new Vector2F(width, height);

            Rectangle oldViewport = context.Viewport;
            context.Viewport = new Rectangle(0, 0, heightMap.Description.Width, heightMap.Description.Height);

            // Render to the frame buffer
            FrameBuffer oldFrameBuffer = context.FrameBuffer;
            context.FrameBuffer = _frameBuffer;

            context.Draw(_primitiveType, _drawState, _sceneState);

            // Restore the context to its original state
            context.FrameBuffer = oldFrameBuffer;
            context.Viewport = oldViewport;
        }

        private int _maxUpdate;
        private Texture2D _sourceTexture;
        private SceneState _sceneState;
        private DrawState _drawState;
        private FrameBuffer _frameBuffer;
        private PrimitiveType _primitiveType;
        private Uniform<Vector2F> _sourceDimensions;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;
        private Uniform<Vector2F> _updateOrigin;
        private int _heightOutput;
        private int _normalOutput;
    }
}
