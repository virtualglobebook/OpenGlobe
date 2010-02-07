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
using MiniGlobe.Renderer;
using OpenTK;

namespace MiniGlobe.Scene
{
    public sealed class TessellatedGlobe : IRenderable, IDisposable
    {
        public TessellatedGlobe(Context context)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 worldPosition;
                  out vec3 positionToLight;
                  out vec3 positionToEye;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 mg_CameraLightPosition;

                  void main()                     
                  {
                        gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 

                        worldPosition = position.xyz;
                        positionToLight = mg_CameraLightPosition - worldPosition;
                        positionToEye = mg_CameraEye - worldPosition;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  in vec3 positionToLight;
                  in vec3 positionToEye;
                  out vec4 fragColor;

                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform sampler2D mg_Texture0;

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, mg_DiffuseSpecularAmbientShininess.w);

                      return (mg_DiffuseSpecularAmbientShininess.x * diffuse) +
                             (mg_DiffuseSpecularAmbientShininess.y * specular) +
                              mg_DiffuseSpecularAmbientShininess.z;
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan2(normal.y, normal.x) / mg_TwoPi + 0.5, asin(normal.z) / mg_Pi + 0.5);
                  }

                  void main()
                  {
                      vec3 normal = normalize(worldPosition);
                      float intensity = LightIntensity(normal,  normalize(positionToLight), normalize(positionToEye), mg_DiffuseSpecularAmbientShininess);
                      fragColor = vec4(intensity * texture(mg_Texture0, ComputeTextureCoordinates(normal)).rgb, 1.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

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
                _numberOfTriangles = ((mesh.Indices as Indices<int>).Values.Count / 3);

                if (_renderState == null)
                {
                    _renderState = new RenderState();
                }
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _dirty = false;
            }
        }

        #region IRenderable Members

        public void Render(SceneState sceneState)
        {
            Clean();

            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            _renderState.RasterizationMode = Wireframe ? RasterizationMode.Line : RasterizationMode.Fill;

            _context.TextureUnits[0].Texture2D = Texture;
            _context.Bind(_renderState);
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Draw(_primitiveType, sceneState);
        }

        #endregion

        public Context Context
        {
            get { return _context; }
        }

        public bool Wireframe { get; set; }
        public Texture2D Texture { get; set; }

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
            _va.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;

        private RenderState _renderState;
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private int _numberOfSlicePartitions;
        private int _numberOfStackPartitions;
        private int _numberOfTriangles;
        private bool _dirty;
    }
}