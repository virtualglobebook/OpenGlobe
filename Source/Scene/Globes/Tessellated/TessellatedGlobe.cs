#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using System;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class TessellatedGlobe : IDisposable
    {
        public TessellatedGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.Tessellated.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.Tessellated.Shaders.GlobeFS.glsl"));
            _textured = (Uniform<bool>)sp.Uniforms["u_Textured"];
            _logarithmicDepth = (Uniform<bool>)sp.Uniforms["u_logarithmicDepth"];
            _logarithmicDepthConstant = (Uniform<float>)sp.Uniforms["u_logarithmicDepthConstant"];
            LogarithmicDepthConstant = 1;
            
            _drawState = new DrawState();
            _drawState.ShaderProgram = sp;

            Shape = Ellipsoid.ScaledWgs84;
            NumberOfSlicePartitions = 32;
            NumberOfStackPartitions = 16;
        }

        private void Clean(Context context)
        {
            if (_dirty)
            {
                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }

                Mesh mesh = GeographicGridEllipsoidTessellator.Compute(Shape,
                    _numberOfSlicePartitions, _numberOfStackPartitions, GeographicGridEllipsoidVertexAttributes.Position);
                _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;
                _numberOfTriangles = (((IndicesUnsignedInt)mesh.Indices).Values.Count / 3);

                _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);

            Clean(context);

            if (_textured.Value)
            {
                Verify.ThrowInvalidOperationIfNull(Texture, "Texture");
                context.TextureUnits[0].Texture = Texture;
                context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            }

            _drawState.RenderState.RasterizationMode = Wireframe ? RasterizationMode.Line : RasterizationMode.Fill;

            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public DepthTestFunction DepthTestFunction
        {
            get { return _drawState.RenderState.DepthTest.Function; }
            set { _drawState.RenderState.DepthTest.Function = value; }
        }

        public bool Wireframe { get; set; }
        public Texture2D Texture { get; set; }

        public bool Textured
        {
            get { return _textured.Value; }
            set { _textured.Value = value; }
        }

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
                // TODO: This is wrong unless Clean is called.
                //Clean();
                return _numberOfTriangles; 
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
            }
        }

        #endregion

        private readonly Uniform<bool> _textured;
        private readonly Uniform<bool> _logarithmicDepth;
        private readonly Uniform<float> _logarithmicDepthConstant;

        private readonly DrawState _drawState;
        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private int _numberOfSlicePartitions;
        private int _numberOfStackPartitions;
        private int _numberOfTriangles;
        private bool _dirty;
    }
}