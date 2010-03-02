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
using System.Collections.Generic;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;
using OpenTK;

namespace MiniGlobe.Examples.Research.RayCasting
{
    public sealed class OptimizedRayCastedGlobe : IRenderable, IDisposable
    {
        public OptimizedRayCastedGlobe(Context context)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 worldPosition;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position; 
                      worldPosition = position.xyz;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec3 fragmentColor;

                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform sampler2D mg_texture0;
                  uniform vec3 mg_cameraLightPosition;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_cameraEyeSquared;
                  uniform vec3 u_globeOneOverRadiiSquared;";
            fs += RayIntersectEllipsoidGLSL();
            fs += ComputeWorldPositionDepthGLSL();
            fs +=
                @"vec3 ComputeDeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
                  {
                      return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
                  }

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(dot(toLight, normal), 0.0);
                      float specular = max(dot(toReflectedLight, toEye), 0.0);
                      specular = pow(specular, diffuseSpecularAmbientShininess.w);

                      return (diffuseSpecularAmbientShininess.x * diffuse) +
                             (diffuseSpecularAmbientShininess.y * specular) +
                              diffuseSpecularAmbientShininess.z;
                  }

                  vec2 ComputeTextureCoordinates(vec3 normal)
                  {
                      return vec2(atan2(normal.y, normal.x) / mg_twoPi + 0.5, asin(normal.z) / mg_pi + 0.5);
                  }

                  void main()
                  {
                      vec3 rayDirection = normalize(worldPosition - mg_cameraEye);
                      Intersection i = RayIntersectEllipsoid(mg_cameraEye, u_cameraEyeSquared, rayDirection, u_globeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          vec3 position = mg_cameraEye + (i.Time * rayDirection);
                          vec3 normal = ComputeDeticSurfaceNormal(position, u_globeOneOverRadiiSquared);

                          vec3 toLight = normalize(mg_cameraLightPosition - position);
                          vec3 toEye = normalize(mg_cameraEye - position);
                          float intensity = LightIntensity(normal, toLight, toEye, mg_diffuseSpecularAmbientShininess);

                          fragmentColor = intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb;
                          gl_FragDepth = ComputeWorldPositionDepth(position, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            _cameraEyeSquaredSP = _sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3>;

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            ///////////////////////////////////////////////////////////////////

            string solidFS =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec3 fragmentColor;

                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_cameraEyeSquared;
                  uniform vec3 u_globeOneOverRadiiSquared;";
            solidFS += RayIntersectEllipsoidGLSL();
            solidFS += ComputeWorldPositionDepthGLSL();
            solidFS +=
                @"void main()
                  {
                      vec3 rayDirection = normalize(worldPosition - mg_cameraEye);
                      Intersection i = RayIntersectEllipsoid(mg_cameraEye, u_cameraEyeSquared, rayDirection, u_globeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          fragmentColor = vec3(0.0, 1.0, 1.0);
                      }
                      else
                      {
                          fragmentColor = vec3(0.2, 0.2, 0.2);
                      }
                  }";
            _solidSP = Device.CreateShaderProgram(vs, solidFS);
            _cameraEyeSquaredSolidSP = _solidSP.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3>;

            ///////////////////////////////////////////////////////////////////

            string wireFrameFS =
                @"#version 150
                 
                  out vec3 fragmentColor;

                  void main()
                  {
                      fragmentColor = vec3(0.0, 0.0, 0.0);
                  }";
            _wireFrameSP = Device.CreateShaderProgram(vs, wireFrameFS);

            _wireframeRenderState = new RenderState();
            _wireframeRenderState.RasterizationMode = RasterizationMode.Line;
            _wireframeRenderState.FacetCulling.Enabled = false;
            _wireframeRenderState.DepthTest.Enabled = false;

            ///////////////////////////////////////////////////////////////////

            _va = _context.CreateVertexArray();

            ///////////////////////////////////////////////////////////////////

            Shape = Ellipsoid.UnitSphere;
            Shade = true;
            NumberOfBoundingPolygonPoints = 3;
        }

        private static string RayIntersectEllipsoidGLSL()
        {
            return
                @"struct Intersection
                  {
                      bool  Intersects;
                      float Time;         // Along ray
                  };

                  //
                  // Assumes ellipsoid is at (0, 0, 0)
                  //
                  Intersection RayIntersectEllipsoid(vec3 rayOrigin, vec3 rayOriginSquared, vec3 rayDirection, vec3 oneOverEllipsoidRadiiSquared)
                  {
                      float a = dot(rayDirection * rayDirection, oneOverEllipsoidRadiiSquared);
                      float b = 2.0 * dot(rayOrigin * rayDirection, oneOverEllipsoidRadiiSquared);
                      float c = dot(rayOriginSquared, oneOverEllipsoidRadiiSquared) - 1.0;
                      float discriminant = b * b - 4.0 * a * c;

                      if (discriminant < 0.0)
                      {
                          return Intersection(false, 0);
                      }
                      else if (discriminant == 0.0)
                      {
                          return Intersection(true, -0.5 * b / a);
                      }

                      float t = -0.5 * (b + (b > 0 ? 1.0 : -1.0) * sqrt(discriminant));
                      float root1 = t / a;
                      float root2 = c / t;

                      return Intersection(true, min(root1, root2));
                  }";
        }

        private static string ComputeWorldPositionDepthGLSL()
        {
            return
              @"float ComputeWorldPositionDepth(vec3 position, mat4x2 modelZToClipCoordinates)
                { 
                    vec2 v = modelZToClipCoordinates * vec4(position, 1);   // clip coordinates
                    v.x /= v.y;                                             // normalized device coordinates
                    v.x = (v.x + 1.0) * 0.5;
                    return v.x;
                }";
        }

        // TODO:  Duplicate with RayCastedGlobe.MultiplyVectorComponents
        private static Vector3D MultiplyVectorComponents(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        private static Vector3D MostOrthogonalAxis(Vector3D v)
        {
            double x = Math.Abs(v.X);
            double y = Math.Abs(v.Y);
            double z = Math.Abs(v.Z);

            if ((x < y) && (x < z))
            {
                return Vector3D.UnitX;
            }
            else if ((y < x) && (y < z))
            {
                return Vector3D.UnitY;
            }
            else
            {
                return Vector3D.UnitZ;
            }
        }

        private static Vector3D[] BoundingPolygon(Vector3D q, Vector3D DInverse, int n)
        {
            if (n < 3)
            {
                throw new ArgumentOutOfRangeException("n");
            }
            
            // TODO:  Place holder
            return new Vector3D[] { Vector3D.UnitX, Vector3D.UnitY, Vector3D.UnitZ };
        }

        #region IRenderable Members

        public void Render(SceneState sceneState)
        {
            int sizeInBytes = NumberOfBoundingPolygonPoints * Vector3d.SizeInBytes;
            if ((_projectionPositions == null) || (_projectionPositions.SizeInBytes != sizeInBytes))
            {
                _projectionPositions = Device.CreateVertexBuffer(BufferHint.StreamDraw, sizeInBytes);
                AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                    _projectionPositions, VertexAttributeComponentType.Double, 3);

                int location = _sp.VertexAttributes["position"].Location;
                if (_va.VertexBuffers[location] != null)
                {
                    _va.VertexBuffers[location].VertexBuffer.Dispose();
                }
                _va.VertexBuffers[location] = attachedPositionBuffer;
            }
            
            Vector3D q = MultiplyVectorComponents(_d, sceneState.Camera.Eye);
            Vector3D[] polygon = BoundingPolygon(q, _dInverse, NumberOfBoundingPolygonPoints);

            _projectionPositions.CopyFromSystemMemory(polygon);

            ///////////////////////////////////////////////////////////////////

            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            Vector3D eye = sceneState.Camera.Eye;
            Vector3 cameraEyeSquared = Conversion.ToVector3(MultiplyVectorComponents(eye, eye));

            if (Shade)
            {
                _context.TextureUnits[0].Texture2D = Texture;
                _context.Bind(_sp);
                _cameraEyeSquaredSP.Value = cameraEyeSquared;
            }
            else
            {
                _context.Bind(_solidSP);
                _cameraEyeSquaredSolidSP.Value = cameraEyeSquared;
            }
            _context.Bind(_renderState);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.TriangleFan, sceneState);

            if (ShowWireframeBoundingPolygon)
            {
                _context.Bind(_wireframeRenderState);
                _context.Bind(_wireFrameSP);
                _context.Draw(PrimitiveType.TriangleFan, sceneState);
            }
        }

        #endregion

        public Context Context
        {
            get { return _context; }
        }

        public Ellipsoid Shape 
        {
            get { return _shape; }
            set
            {
                _shape = value;
                _d = new Vector3D(1 / _shape.Radii.X, 1 / _shape.Radii.Y, 1 / _shape.Radii.Z);
                _dInverse = new Vector3D(_shape.Radii.X, _shape.Radii.Y, _shape.Radii.Z);

                (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(_shape.OneOverRadiiSquared);
                (_solidSP.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(_shape.OneOverRadiiSquared);
            }
        }

        public bool Shade { get; set; }
        public bool ShowWireframeBoundingPolygon { get; set; }
        public int NumberOfBoundingPolygonPoints { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _solidSP.Dispose();
            _wireFrameSP.Dispose();
            _va.Dispose();
            if (_projectionPositions != null)
            {
                _projectionPositions.Dispose();
            }
        }

        #endregion

        private readonly Context _context;

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3> _cameraEyeSquaredSP;
        private readonly ShaderProgram _solidSP;
        private readonly Uniform<Vector3> _cameraEyeSquaredSolidSP;

        private readonly RenderState _wireframeRenderState;
        private readonly ShaderProgram _wireFrameSP;

        private readonly VertexArray _va;
        private VertexBuffer _projectionPositions;

        private Ellipsoid _shape;
        private Vector3D _d;
        private Vector3D _dInverse;
    }
}