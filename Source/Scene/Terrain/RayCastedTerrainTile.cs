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
    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;
            
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.RayCastedTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("MiniGlobe.Scene.Terrain.RayCastedTerrainTile.TerrainFS.glsl"));

            _tileSize = tile.Size;
            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;
            _tileAABBLowerLeft = Vector3D.Zero;             // TEXEL_SPACE_TODO
            _tileAABBUpperRight = new Vector3D(tile.Size.X, tile.Size.Y,
                tile.MaximumHeight - tile.MinimumHeight);

            _heightExaggeration = _sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _minimumHeight = _sp.Uniforms["u_minimumHeight"] as Uniform<float>;
            _maximumHeight = _sp.Uniforms["u_maximumHeight"] as Uniform<float>;
            _aabbLowerLeft = _sp.Uniforms["u_aabbLowerLeft"] as Uniform<Vector3S>;
            _aabbUpperRight = _sp.Uniforms["u_aabbUpperRight"] as Uniform<Vector3S>;
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.X, tile.Size.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;
        }

        private void Update()
        {
            if (_dirtyVA)
            {
                Vector3D radii = new Vector3D(_tileSize.X, _tileSize.Y,
                    (_tileMaximumHeight - _tileMinimumHeight) * _heightExaggeration.Value);
                Vector3D halfRadii = 0.5 * radii;

                Mesh mesh = BoxTessellator.Compute(radii);

                //
                // TEXEL_SPACE_TODO:  Translate box so it is not centered at 
                // the origin - world space and texel space will match up.
                //
                IList<Vector3D> positions = (mesh.Attributes["position"] as VertexAttributeDoubleVector3).Values;
                for (int i = 0; i < positions.Count; ++i)
                {
                    positions[i] = positions[i] + halfRadii;
                }

                if (_va != null)
                {
                    _va.Dispose();
                }
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _dirtyVA = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Update();

            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
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
                    _aabbLowerLeft.Value = new Vector3S((float)_tileAABBLowerLeft.X, (float)_tileAABBLowerLeft.Y, (float)(_tileAABBLowerLeft.Z * value));
                    _aabbUpperRight.Value = new Vector3S((float)_tileAABBUpperRight.X, (float)_tileAABBUpperRight.Y, (float)(_tileAABBUpperRight.Z * value));

                    _dirtyVA = true;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;
        private readonly Uniform<Vector3S> _aabbLowerLeft;
        private readonly Uniform<Vector3S> _aabbUpperRight;

        private readonly Vector2I _tileSize;
        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;
        private readonly Vector3D _tileAABBLowerLeft;
        private readonly Vector3D _tileAABBUpperRight;

        private readonly Texture2D _texture;
        private readonly RenderState _renderState;

        private VertexArray _va;
        private PrimitiveType _primitiveType;
        private bool _dirtyVA;
    }
}