using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGlobe.Renderer;
using OpenGlobe.Core;

namespace OpenGlobe.Scene.Terrain
{
    public class ClipmapUpdater : IDisposable
    {
        public ClipmapUpdater(Context context, int maxUpdate)
        {
            _maxUpdate = maxUpdate;

            ShaderProgram shaderProgram = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.ClipmapTerrain.ClipmapUpdateFS.glsl"));
            _heightOutput = shaderProgram.FragmentOutputs["heightOutput"];
            _normalOutput = shaderProgram.FragmentOutputs["normalOutput"];

            _sourceDimensions = (Uniform<Vector2S>)shaderProgram.Uniforms["u_sourceDimensions"];
            _heightExaggeration = (Uniform<float>)shaderProgram.Uniforms["u_heightExaggeration"];
            _postDelta = (Uniform<float>)shaderProgram.Uniforms["u_postDelta"];

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

        public void Update(Context context, Texture2D heightMap, Texture2D normalMap, float postDelta, int x, int y, int width, int height, float[] posts)
        {
            // Copy the post data into the source texture.
            using (WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, posts.Length * sizeof(float)))
            {
                pixelBuffer.CopyFromSystemMemory(posts);
                _sourceTexture.CopyFromBuffer(pixelBuffer, 0, 0, width, height, ImageFormat.Red, ImageDatatype.Float, 4);
            }

            context.TextureUnits[0].Texture = _sourceTexture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
            _postDelta.Value = postDelta;

            // Set the target location
            _frameBuffer.ColorAttachments[_heightOutput] = heightMap;
            _frameBuffer.ColorAttachments[_normalOutput] = normalMap;
            _sceneState.Camera.OrthographicLeft = x;
            _sceneState.Camera.OrthographicTop = y;
            _sceneState.Camera.OrthographicRight = x + width;
            _sceneState.Camera.OrthographicBottom = y + height;
            _sourceDimensions.Value = new Vector2S(width, height);

            // Render to the frame buffer
            FrameBuffer oldFrameBuffer = context.FrameBuffer;
            context.FrameBuffer = _frameBuffer;

            context.Draw(_primitiveType, _drawState, _sceneState);

            context.FrameBuffer = oldFrameBuffer;
        }

        private int _maxUpdate;
        private Texture2D _sourceTexture;
        private SceneState _sceneState;
        private DrawState _drawState;
        private FrameBuffer _frameBuffer;
        private PrimitiveType _primitiveType;
        private Uniform<Vector2S> _sourceDimensions;
        private Uniform<float> _heightExaggeration;
        private Uniform<float> _postDelta;
        private int _heightOutput;
        private int _normalOutput;
    }
}
