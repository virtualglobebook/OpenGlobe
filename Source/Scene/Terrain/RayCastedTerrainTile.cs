#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;

namespace MiniGlobe.Terrain
{
    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                  }";
            string fs =
                @"#version 150
                 
                  out vec3 fragmentColor;

                  void main()
                  {
                      fragmentColor = vec3(0.0, 0.0, 0.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            ///////////////////////////////////////////////////////////////////

            Vector3D radii = new Vector3D(
                tile.Extent.East - tile.Extent.West,
                tile.Extent.North - tile.Extent.South,
                tile.MaximumHeight - tile.MinimumHeight);

            Mesh mesh = BoxTessellator.Compute(radii);
            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _renderState.RasterizationMode = RasterizationMode.Line;
        }

        public void Render(SceneState sceneState)
        {
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
        }

        #endregion


        private readonly Context _context;
        private readonly ShaderProgram _sp;
        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;
        private readonly RenderState _renderState;
    }
}