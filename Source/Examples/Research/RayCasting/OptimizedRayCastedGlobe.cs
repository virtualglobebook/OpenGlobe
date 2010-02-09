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
        public OptimizedRayCastedGlobe(Context context, Ellipsoid globeShape)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_orthographicProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_orthographicProjectionMatrix * position; 
                  }";

            string fs =
                @"#version 150
                 
                  out vec4 fragmentColor;

                  uniform vec4 mg_viewport;
                  uniform mat4 mg_windowToWorldNearPlane;
                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform sampler2D mg_texture0;
                  uniform vec3 mg_cameraLightPosition;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_globeOneOverRadiiSquared;";
            fs += WindowToNormalizedDeviceCoordinates();
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
                      vec2 w = WindowToNormalizedDeviceCoordinates(gl_FragCoord.xy, mg_viewport);
                      vec3 wOnNearPlane = (mg_windowToWorldNearPlane * vec4(w.xy, 0, 1)).xyz;

                      vec3 rayDirection = normalize(wOnNearPlane - mg_cameraEye);
                      /////////////////////////////////////////////////////////

                      Intersection i = RayIntersectEllipsoid(mg_cameraEye, rayDirection, u_globeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          vec3 position = mg_cameraEye + (i.Time * rayDirection);
                          vec3 normal = ComputeDeticSurfaceNormal(position, u_globeOneOverRadiiSquared);

                          vec3 toLight = normalize(mg_cameraLightPosition - position);
                          vec3 toEye = normalize(mg_cameraEye - position);
                          float intensity = LightIntensity(normal, toLight, toEye, mg_diffuseSpecularAmbientShininess);

                          fragmentColor = vec4(intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb, 1.0);
                          gl_FragDepth = ComputeWorldPositionDepth(position, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(globeShape.OneOverRadiiSquared);

            Mesh mesh = BoxTessellator.Compute(2 * globeShape.Radii);
            _volumePositions = (mesh.Attributes["position"] as VertexAttribute<Vector3d>).Values;

            _projectionPositions = Device.CreateVertexBuffer(BufferHint.StreamDraw, 4 * Vector2.SizeInBytes);
            AttachedVertexBuffer attachedPositionBuffer = new AttachedVertexBuffer(
                _projectionPositions, VertexAttributeComponentType.Float, 2);
            _va = _context.CreateVertexArray();
            _va.VertexBuffers[_sp.VertexAttributes["position"].Location] = attachedPositionBuffer;

            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;

            ///////////////////////////////////////////////////////////////////

            string solidFS =
                @"#version 150
                 
                  out vec4 fragmentColor;

                  uniform vec4 mg_viewport;
                  uniform mat4 mg_windowToWorldNearPlane;
                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_globeOneOverRadiiSquared;";
            solidFS += WindowToNormalizedDeviceCoordinates();
            solidFS += RayIntersectEllipsoidGLSL();
            solidFS += ComputeWorldPositionDepthGLSL();
            solidFS +=
                @"void main()
                  {
                      vec2 w = WindowToNormalizedDeviceCoordinates(gl_FragCoord.xy, mg_viewport);
                      vec3 wOnNearPlane = (mg_windowToWorldNearPlane * vec4(w.xy, 0, 1)).xyz;

                      vec3 rayDirection = normalize(wOnNearPlane - mg_cameraEye);
                      /////////////////////////////////////////////////////////

                      Intersection i = RayIntersectEllipsoid(mg_cameraEye, rayDirection, u_globeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          fragmentColor = vec4(0.0, 1.0, 1.0, 1.0);
                      }
                      else
                      {
                          fragmentColor = vec4(0.2, 0.2, 0.2, 1.0);
                      }
                  }";
            _solidSP = Device.CreateShaderProgram(vs, solidFS);
            (_solidSP.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(globeShape.OneOverRadiiSquared);

            ///////////////////////////////////////////////////////////////////

            string wireFrameFS =
                @"#version 150
                 
                  out vec4 fragmentColor;

                  void main()
                  {
                      fragmentColor = vec4(0.0, 0.0, 0.0, 1.0);
                  }";
            _wireFrameSP = Device.CreateShaderProgram(vs, wireFrameFS);

            _wireframeRenderState = new RenderState();
            _wireframeRenderState.RasterizationMode = RasterizationMode.Line;
            _wireframeRenderState.FacetCulling.Enabled = false;
            
            Shade = true;
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
                  Intersection RayIntersectEllipsoid(vec3 rayOrigin, vec3 rayDirection, vec3 oneOverEllipsoidRadiiSquared)
                  {
                      float a = dot(rayDirection * rayDirection, oneOverEllipsoidRadiiSquared);
                      float b = 2.0 * dot(rayOrigin * rayDirection, oneOverEllipsoidRadiiSquared);
                      float c = dot(rayOrigin * rayOrigin, oneOverEllipsoidRadiiSquared) - 1.0;
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

        private static string WindowToNormalizedDeviceCoordinates()
        {
            return
              @"vec2 WindowToNormalizedDeviceCoordinates(vec2 fragmentCoordinate, vec4 viewport)
                {
                    vec2 v = vec2(fragmentCoordinate.xy / viewport.zw);
                    v = (2.0 * v) - 1.0;                                    // Scale [0, 1] to [-1, 1]
                    return v;
                }";
        }

        #region IRenderable Members

        public void Render(SceneState sceneState)
        {
            Vector2d minWindow = new Vector2d(double.MaxValue, double.MaxValue);
            Vector2d maxWindow = new Vector2d(double.MinValue, double.MinValue);

            Matrix4d mvp = sceneState.ModelViewPerspectiveProjectionMatrix;
            Matrix4d viewportTransform = sceneState.ComputeViewportTransformationMatrix(_context.Viewport);

            for (int i = 0; i < _volumePositions.Count; ++i)
            {
                Vector4d model = new Vector4d(_volumePositions[i].X, _volumePositions[i].Y, _volumePositions[i].Z, 1);
                Vector4d clip = Vector4d.Transform(model, mvp);
                Vector4d ndc = new Vector4d(clip.X / clip.W, clip.Y / clip.W, clip.Z / clip.W, clip.W);
                Vector4d window = Vector4d.Transform(new Vector4d(ndc.X + 1, ndc.Y + 1, ndc.Z, 1), viewportTransform);

                minWindow = new Vector2d(Math.Min(minWindow.X, window.X), Math.Min(minWindow.Y, window.Y));
                maxWindow = new Vector2d(Math.Max(maxWindow.X, window.X), Math.Max(maxWindow.Y, window.Y));
            }

            Vector2[] positions = new Vector2[] 
            { 
                Conversion.ToVector2(minWindow),
                Conversion.ToVector2(new Vector2d(maxWindow.X, minWindow.Y)),
                Conversion.ToVector2(maxWindow),
                Conversion.ToVector2(new Vector2d(minWindow.X, maxWindow.Y))
            };
            _projectionPositions.CopyFromSystemMemory(positions);

            ///////////////////////////////////////////////////////////////////

            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            if (Shade)
            {
                _context.TextureUnits[0].Texture2D = Texture;
                _context.Bind(_sp);
            }
            else
            {
                _context.Bind(_solidSP);
            }
            _context.Bind(_renderState);
            _context.Bind(_va);
            _context.Draw(PrimitiveType.TriangleFan, sceneState);

            if (ShowWireframeBoundingVolume)
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

        public bool Shade { get; set; }
        public bool ShowWireframeBoundingVolume { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _projectionPositions.Dispose();
            _solidSP.Dispose();
            _wireFrameSP.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly IList<Vector3d> _volumePositions;

        private readonly VertexArray _va;
        private readonly VertexBuffer _projectionPositions;

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly ShaderProgram _solidSP;

        private readonly RenderState _wireframeRenderState;
        private readonly ShaderProgram _wireFrameSP;
    }
}