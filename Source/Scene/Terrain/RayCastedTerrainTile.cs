#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Scene;
using OpenGlobe.Renderer;
using System.Collections.Generic;

namespace OpenGlobe.Scene
{
    public enum RayCastedTerrainShadingAlgorithm
    {
        ByHeight,
        ByRaySteps
    }

    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(TerrainTile tile)
        {
            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.RayCastedTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.RayCastedTerrainTile.TerrainFS.glsl"));

            _tileResolution = tile.Resolution;
            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;
            _tileAABBLowerLeft = Vector3D.Zero;             // TEXEL_SPACE_TODO
            _tileAABBUpperRight = new Vector3D(tile.Resolution.X, tile.Resolution.Y,
                tile.MaximumHeight - tile.MinimumHeight);

            ((Uniform<int>)sp.Uniforms["u_heightMap"]).Value = 0;
            _heightExaggeration = (Uniform<float>)sp.Uniforms["u_heightExaggeration"];
            _minimumHeight = (Uniform<float>)sp.Uniforms["u_minimumHeight"];
            _maximumHeight = (Uniform<float>)sp.Uniforms["u_maximumHeight"];
            _aabbLowerLeft = (Uniform<Vector3F>)sp.Uniforms["u_aabbLowerLeft"];
            _aabbUpperRight = (Uniform<Vector3F>)sp.Uniforms["u_aabbUpperRight"];
            _shadingAlgorithm = (Uniform<int>)sp.Uniforms["u_shadingAlgorithm"];

            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.Face = CullFace.Front;
            _drawState.ShaderProgram = sp;

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Resolution.X, tile.Resolution.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDatatype.Float);

            ShowTerrain = true;
        }

        private void Update(Context context)
        {
            if (_dirtyVA)
            {
                Vector3D radii = new Vector3D(_tileResolution.X, _tileResolution.Y,
                    (_tileMaximumHeight - _tileMinimumHeight) * _heightExaggeration.Value);
                Vector3D halfRadii = 0.5 * radii;

                Mesh mesh = BoxTessellator.Compute(radii);

                //
                // TEXEL_SPACE_TODO:  Translate box so it is not centered at 
                // the origin - world space and texel space will match up.
                //
                IList<Vector3D> positions = ((VertexAttributeDoubleVector3)mesh.Attributes["position"]).Values;
                for (int i = 0; i < positions.Count; ++i)
                {
                    positions[i] = positions[i] + halfRadii;
                }

                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }
                _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;
                _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                }
                _wireframe = new Wireframe(context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirtyVA = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);

            Update(context);

            if (ShowTerrain)
            {
                context.TextureUnits[0].Texture = _texture;
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
                context.Draw(_primitiveType, _drawState, sceneState);
            }

            if (ShowWireframe)
            {
                _wireframe.Render(context, sceneState);
            }
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
                    _aabbLowerLeft.Value = new Vector3F((float)_tileAABBLowerLeft.X, (float)_tileAABBLowerLeft.Y, (float)(_tileAABBLowerLeft.Z * value));
                    _aabbUpperRight.Value = new Vector3F((float)_tileAABBUpperRight.X, (float)_tileAABBUpperRight.Y, (float)(_tileAABBUpperRight.Z * value));

                    _dirtyVA = true;
                }
            }
        }

        public bool ShowTerrain { get; set; }
        public bool ShowWireframe { get; set; }
        public RayCastedTerrainShadingAlgorithm ShadingAlgorithm 
        {
            get { return (RayCastedTerrainShadingAlgorithm)_shadingAlgorithm.Value; }
            set { _shadingAlgorithm.Value = (int)value;  }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
            _texture.Dispose();
            _wireframe.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;
        private readonly Uniform<Vector3F> _aabbLowerLeft;
        private readonly Uniform<Vector3F> _aabbUpperRight;
        private readonly Uniform<int> _shadingAlgorithm;

        private readonly Vector2I _tileResolution;
        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;
        private readonly Vector3D _tileAABBLowerLeft;
        private readonly Vector3D _tileAABBUpperRight;

        private readonly Texture2D _texture;

        private Wireframe _wireframe;

        private PrimitiveType _primitiveType;
        private bool _dirtyVA;
    }
}