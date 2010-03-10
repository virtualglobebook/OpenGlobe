#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
//
// Portions Copyright (c) 2010, Analytical Graphics, Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright
//      notice, this list of conditions and the following disclaimer in the
//      documentation and/or other materials provided with the distribution.
//    * Neither the name of Analytical Graphics, Inc. nor the
//      names of its contributors may be used to endorse or promote products
//      derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Analytical Graphics, Inc. ``AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Analytical Graphics, Inc. BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
    public sealed class OptimizedRayCastedGlobe : IDisposable
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
                  uniform vec3 u_scaledCameraEye;
                  uniform float u_wMagnitudeSquared;
                  uniform vec3 u_globeRadii;
                  uniform vec3 u_globeOneOverRadii;";
            fs += RayIntersectEllipsoidGLSL();
            fs += ComputeWorldPositionDepthGLSL();
            fs +=
                @"float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, vec4 diffuseSpecularAmbientShininess)
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
                      return vec2(atan(normal.y, normal.x) * mg_oneOverTwoPi + 0.5, asin(normal.z) * mg_oneOverPi + 0.5);
                  }

                  void main()
                  {
                      vec3 rayDirection = worldPosition - mg_cameraEye;
                      Intersection i = RayIntersectEllipsoid(u_scaledCameraEye, u_wMagnitudeSquared, rayDirection, u_globeRadii, u_globeOneOverRadii);

                      if (i.Intersects)
                      {
                          vec3 normal = normalize(i.SurfaceNormal);
                          vec3 toLight = normalize(mg_cameraLightPosition - i.SurfacePosition);
                          vec3 toEye = normalize(mg_cameraEye - i.SurfacePosition);
                          float intensity = LightIntensity(normal, toLight, toEye, mg_diffuseSpecularAmbientShininess);

                          fragmentColor = intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb;
                          gl_FragDepth = ComputeWorldPositionDepth(i.SurfacePosition, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            _scaledCameraEyeSP = _sp.Uniforms["u_scaledCameraEye"] as Uniform<Vector3>;
            _wMagnitudeSquaredSP = _sp.Uniforms["u_wMagnitudeSquared"] as Uniform<float>;

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            ///////////////////////////////////////////////////////////////////

            string solidFS =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec3 fragmentColor;

                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_scaledCameraEye;
                  uniform float u_wMagnitudeSquared;
                  uniform vec3 u_globeRadii;
                  uniform vec3 u_globeOneOverRadii;";
            solidFS += RayIntersectEllipsoidGLSL();
            solidFS += ComputeWorldPositionDepthGLSL();
            solidFS +=
                @"void main()
                  {
                      vec3 rayDirection = worldPosition - mg_cameraEye;
                      Intersection i = RayIntersectEllipsoid(u_scaledCameraEye, u_wMagnitudeSquared, rayDirection, u_globeRadii, u_globeOneOverRadii);

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
            _scaledCameraEyeSolidSP = _solidSP.Uniforms["u_scaledCameraEye"] as Uniform<Vector3>;
            _wMagnitudeSquaredSolidSP = _solidSP.Uniforms["u_wMagnitudeSquared"] as Uniform<float>;

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
                      bool Intersects;
                      vec3 SurfacePosition;
                      vec3 SurfaceNormal;
                  };

                  //
                  // Assumes ellipsoid is at (0, 0, 0)
                  //
                  Intersection RayIntersectEllipsoid(
                      vec3  q,
                      float wMagnitudeSquared,
                      vec3  rayDirection, 
                      vec3  ellipsoidRadii,
                      vec3  oneOverEllipsoidRadii)
                  {
                      vec3 bUnit = normalize(rayDirection * oneOverEllipsoidRadii);

                      float t = -dot(bUnit, q);
                      float tSquared = t * t;

                      if ((t >= 0.0) && (tSquared >= wMagnitudeSquared))
                      {
                          float temp = t - sqrt(tSquared - wMagnitudeSquared);
                          vec3 r = (q + temp * bUnit);

                          vec3 s = r * ellipsoidRadii;
                          vec3 n = r * oneOverEllipsoidRadii;

                          return Intersection(true, s, n);
                      }
                      else
                      {
                          return Intersection(false, vec3(0.0), vec3(0.0));
                      }
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

        /// <summary>
        /// Computes the vertices of the bounding polygon in the reference frame of the ellipsoid.
        /// </summary>
        /// <param name="q">Scaled camera position. (Precomputed as q = D * p.)</param>
        /// <param name="DInverse">Diagonal matrix with semiaxis lengths as the diagonal elements (used for simple descaling).</param>
        /// <param name="n">The number of sides of the polygon.</param>
        /// <returns>The vertices of the polygon.</returns>
        private static Vector3D[] BoundingPolygon(Vector3D q, Vector3D DInverse, int n)
        {
            if (n < 3)
            {
                throw new ArgumentOutOfRangeException("n");
            }

            double qMagnitudeSquared = q.MagnitudeSquared;
            double qMagnitude = Math.Sqrt(qMagnitudeSquared);

            //
            // Compute the orthonormal basis defined by q and its most orthogonal axis.
            //
            Vector3D axis1 = q / qMagnitude;
            Vector3D reference = axis1.MostOrthogonalAxis;
            Vector3D axis2 = (reference.Cross(axis1)).Normalize();
            Vector3D axis3 = axis1.Cross(axis2);  // This should be a unit vector and may not need to be normalized.

            //
            // Compute the scaling and translation in the orthonormal basis.
            //
            double scaling = Math.Sqrt(1.0 - 1.0 / qMagnitudeSquared);
            double translation = 1.0 / qMagnitude;

            //
            // Compute the parameters of the bounding regular convex polygon.
            //
            double piOverN = Math.PI / n;
            double angle = 2.0 * piOverN;
            double r = scaling / Math.Cos(piOverN);

            //
            // Compute the reference vectors used to generate the vertices in the reference frame of the ellipsoid.
            //
            Vector3D xTemp = translation * DInverse.MultiplyComponents(axis1);
            Vector3D yTemp = r * DInverse.MultiplyComponents(axis2);
            Vector3D zTemp = r * DInverse.MultiplyComponents(axis3);

            //
            // Generate the vertices.
            //
            Vector3D[] result = new Vector3D[n];
            for (int i = 0; i < n; ++i)
            {
                double temp = i * angle;
                result[i] = xTemp + Math.Cos(temp) * yTemp + Math.Sin(temp) * zTemp;
            }

            return result;
        }

        public void Render(SceneState sceneState)
        {
            int sizeInBytes = NumberOfBoundingPolygonPoints * Vector3D.SizeInBytes;
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

            Vector3D q = _d.MultiplyComponents(sceneState.Camera.Eye);
            Vector3D[] polygon = BoundingPolygon(q, _dInverse, NumberOfBoundingPolygonPoints);

            _projectionPositions.CopyFromSystemMemory(polygon);

            ///////////////////////////////////////////////////////////////////

            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            Vector3D eye = sceneState.Camera.Eye;
            Vector3 cameraEyeSquared = Conversion.ToVector3(eye.MultiplyComponents(eye));
            double qMagnitudeSquared = q.MagnitudeSquared;
            double wMagnitudeSquared = qMagnitudeSquared - 1;

            if (Shade)
            {
                _context.TextureUnits[0].Texture2D = Texture;
                _context.Bind(_sp);
                _scaledCameraEyeSP.Value = Conversion.ToVector3(q);
                _wMagnitudeSquaredSP.Value = (float)wMagnitudeSquared;
            }
            else
            {
                _context.Bind(_solidSP);
                _scaledCameraEyeSolidSP.Value = Conversion.ToVector3(q);
                _wMagnitudeSquaredSolidSP.Value = (float)wMagnitudeSquared;
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

                (_sp.Uniforms["u_globeOneOverRadii"] as Uniform<Vector3>).Value = Conversion.ToVector3(_shape.OneOverRadii);
                (_sp.Uniforms["u_globeRadii"] as Uniform<Vector3>).Value = Conversion.ToVector3(_shape.Radii);
                (_solidSP.Uniforms["u_globeOneOverRadii"] as Uniform<Vector3>).Value = Conversion.ToVector3(_shape.OneOverRadii);
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
        private readonly Uniform<Vector3> _scaledCameraEyeSP;
        private readonly Uniform<float> _wMagnitudeSquaredSP;
        private readonly ShaderProgram _solidSP;
        private readonly Uniform<Vector3> _scaledCameraEyeSolidSP;
        private readonly Uniform<float> _wMagnitudeSquaredSolidSP;

        private readonly RenderState _wireframeRenderState;
        private readonly ShaderProgram _wireFrameSP;

        private readonly VertexArray _va;
        private VertexBuffer _projectionPositions;

        private Ellipsoid _shape;
        private Vector3D _d;
        private Vector3D _dInverse;
    }
}