#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class TessellatedGlobe : IDisposable
    {
        public TessellatedGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _context = context;

            string vs = EmbeddedResources.GetText("MiniGlobe.Scene.Globes.Tessellated.Shaders.GlobeVS.glsl");
            string fs = EmbeddedResources.GetText("MiniGlobe.Scene.Globes.Tessellated.Shaders.GlobeFS.glsl");
            _sp = Device.CreateShaderProgram(vs, fs);
            _logarithmicDepth = _sp.Uniforms["u_logarithmicDepth"] as Uniform<bool>;
            _logarithmicDepthConstant = _sp.Uniforms["u_logarithmicDepthConstant"] as Uniform<float>;
            LogarithmicDepthConstant = 1;
            
            _renderState = new RenderState();

            Shape = Ellipsoid.UnitSphere;
            NumberOfSlicePartitions = 32;
            NumberOfStackPartitions = 16;
        }

        private void Clean()
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                }

                Mesh mesh = GeographicGridEllipsoidTessellator.Compute(Shape,
                    _numberOfSlicePartitions, _numberOfStackPartitions, GeographicGridEllipsoidVertexAttributes.Position);
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;
                _numberOfTriangles = ((mesh.Indices as IndicesInt32).Values.Count / 3);

                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _dirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Clean();

            _renderState.RasterizationMode = Wireframe ? RasterizationMode.Line : RasterizationMode.Fill;

            _context.TextureUnits[0].Texture2D = Texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(_primitiveType, sceneState);
        }

        public Context Context
        {
            get { return _context; }
        }

        public DepthTestFunction DepthTestFunction
        {
            get { return _renderState.DepthTest.Function; }
            set { _renderState.DepthTest.Function = value; }
        }

        public bool Wireframe { get; set; }
        public Texture2D Texture { get; set; }

        public bool LogarithmicDepth
        {
            get { return _logarithmicDepth.Value; }
            set { _logarithmicDepth.Value = value; }
        }

        public float LogarithmicDepthConstant
        {
            get { return _logarithmicDepthConstant.Value; }
            set { _logarithmicDepthConstant.Value = value; }
        }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        public int NumberOfSlicePartitions
        {
            get { return _numberOfSlicePartitions; }
            set
            {
                _dirty = true;
                _numberOfSlicePartitions = value;
            }
        }

        public int NumberOfStackPartitions
        {
            get { return _numberOfStackPartitions; }
            set
            {
                _dirty = true;
                _numberOfStackPartitions = value;
            }
        }

        public int NumberOfTriangles
        {
            get 
            {
                Clean();
                return _numberOfTriangles; 
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            if (_va != null)
            {
                _va.Dispose();
            }
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;
        private readonly Uniform<bool> _logarithmicDepth;
        private readonly Uniform<float> _logarithmicDepthConstant;

        private readonly RenderState _renderState;
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private int _numberOfSlicePartitions;
        private int _numberOfStackPartitions;
        private int _numberOfTriangles;
        private bool _dirty;
    }
}