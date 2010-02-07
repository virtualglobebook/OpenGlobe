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
    public sealed class RayCastedGlobe : IRenderable, IDisposable
    {
        public RayCastedGlobe(Context context, Ellipsoid globeShape)
        {
            _context = context;

            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 worldPosition;

                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 
                      worldPosition = position.xyz;
                  }";

            string fs =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec4 fragColor;

                  uniform mat4x2 mg_ModelZToClipCoordinates;
                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform sampler2D mg_Texture0;
                  uniform vec3 mg_CameraLightPosition;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 u_GlobeOneOverRadiiSquared;";
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
                      vec3 rayDirection = normalize(worldPosition - mg_CameraEye);
                      Intersection i = RayIntersectEllipsoid(mg_CameraEye, rayDirection, u_GlobeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          vec3 position = mg_CameraEye + (i.Time * rayDirection);
                          vec3 normal = ComputeDeticSurfaceNormal(position, u_GlobeOneOverRadiiSquared);

                          vec3 toLight = normalize(mg_CameraLightPosition - position);
                          vec3 toEye = normalize(mg_CameraEye - position);
                          float intensity = LightIntensity(normal, toLight, toEye, mg_DiffuseSpecularAmbientShininess);

                          fragColor = vec4(intensity * texture(mg_Texture0, ComputeTextureCoordinates(normal)).rgb, 1.0);
                          gl_FragDepth = ComputeWorldPositionDepth(position);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            (_sp.Uniforms["u_GlobeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(globeShape.OneOverRadiiSquared);

            Mesh mesh = BoxTessellator.Compute(2 * globeShape.Radii);
            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            ///////////////////////////////////////////////////////////////////

            string solidFS =
                @"#version 150
                 
                  in vec3 worldPosition;
                  out vec4 fragColor;

                  uniform mat4x2 mg_ModelZToClipCoordinates;
                  uniform vec4 mg_DiffuseSpecularAmbientShininess;
                  uniform vec3 mg_CameraEye;
                  uniform vec3 u_GlobeOneOverRadiiSquared;";
            solidFS += RayIntersectEllipsoidGLSL();
            solidFS += ComputeWorldPositionDepthGLSL();
            solidFS +=
                @"void main()
                  {
                      vec3 rayDirection = normalize(worldPosition - mg_CameraEye);
                      Intersection i = RayIntersectEllipsoid(mg_CameraEye, rayDirection, u_GlobeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          vec3 position = mg_CameraEye + (i.Time * rayDirection);

                          fragColor = vec4(0.0, 1.0, 1.0, 1.0);
                          gl_FragDepth = ComputeWorldPositionDepth(position);
                      }
                      else
                      {
                          fragColor = vec4(0.3, 0.3, 0.3, 1.0);
                      }
                  }";
            
            _solidSP = Device.CreateShaderProgram(vs, solidFS);
            (_solidSP.Uniforms["u_GlobeOneOverRadiiSquared"] as Uniform<Vector3>).Value = Conversion.ToVector3(globeShape.OneOverRadiiSquared);

            ///////////////////////////////////////////////////////////////////

            string fs2 =
                @"#version 150
                 
                  out vec4 fragColor;

                  void main()
                  {
                      fragColor = vec4(0.0, 0.0, 0.0, 1.0);
                  }";
            _boxSP = Device.CreateShaderProgram(PassThroughVS(), fs2);

            _boxRenderState = new RenderState();
            _boxRenderState.RasterizationMode = RasterizationMode.Line;
            _boxRenderState.FacetCulling.Face = CullFace.Front;
            _boxRenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
        }

        private static string PassThroughVS()
        {
            return
                @"#version 150

                  in vec4 position;
                  uniform mat4 mg_ModelViewPerspectiveProjectionMatrix;

                  void main()                     
                  {
                      gl_Position = mg_ModelViewPerspectiveProjectionMatrix * position; 
                  }";
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
              @"float ComputeWorldPositionDepth(vec3 position)
                { 
                    vec2 v = mg_ModelZToClipCoordinates * vec4(position, 1);   // clip coordinates
                    v.x /= v.y;                                                // normalized device coordinates
                    v.x = (v.x + 1.0) * 0.5;
                    return v.x;
                }";
        }

        #region IRenderable Members

        public void Render(SceneState sceneState)
        {
            if (Texture == null)
            {
                throw new InvalidOperationException("Texture");
            }

            if (ShowSolidBox)
            {
                _context.Bind(_solidSP);
            }
            else
            {
                _context.TextureUnits[0].Texture2D = Texture;
                _context.Bind(_sp);
            }
            _context.Bind(_renderState);
            _context.Bind(_va);
            _context.Draw(_primitiveType, sceneState);

            if (ShowWireframeBoundingBox)
            {
                _context.Bind(_boxRenderState);
                _context.Bind(_boxSP);
                _context.Draw(_primitiveType, sceneState);
            }
        }

        #endregion

        public Context Context
        {
            get { return _context; }
        }

        public bool ShowSolidBox { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _solidSP.Dispose();
            _va.Dispose();
            _boxSP.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly ShaderProgram _solidSP;
        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;

        private readonly RenderState _boxRenderState;
        private readonly ShaderProgram _boxSP;
    }
}