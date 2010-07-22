#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Core.Geometry;
using OpenGlobe.Renderer;
using System.Drawing;

namespace OpenGlobe.Scene
{
    public sealed class Polygon : IDisposable
    {
        public Polygon(Context context, Ellipsoid globeShape, IEnumerable<Vector3D> positions)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);

            List<Vector3D> cleanPositions = SimplePolygonAlgorithms.Cleanup(positions) as List<Vector3D>;
            cleanPositions.Reverse();

            IndicesInt32 indices = EarClippingOnEllipsoid.Triangulate(cleanPositions);

            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3(
                "position", (indices.Values.Count / 3) + 2);
            foreach (Vector3D position in cleanPositions)
            {
                positionsAttribute.Values.Add(position);
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;
            mesh.Attributes.Add(positionsAttribute);
            mesh.Indices = indices;

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polygon.Shaders.PolygonVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polygon.Shaders.PolygonFS.glsl"));
            (sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = globeShape.OneOverRadiiSquared.ToVector3S();
            _colorUniform = sp.Uniforms["u_color"] as Uniform<Vector3S>;

            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.Enabled = false;
            _drawState.ShaderProgram = sp;
            _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _primitiveType = mesh.PrimitiveType;

            Color = Color.White;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public Color Color
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector3S(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f);
            }
        }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthWrite
        {
            get { return _drawState.RenderState.DepthWrite; }
            set { _drawState.RenderState.DepthWrite = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
        }

        #endregion

        private Color _color;
        private readonly Uniform<Vector3S> _colorUniform;
        private readonly DrawState _drawState;
        private readonly PrimitiveType _primitiveType;
    }
}