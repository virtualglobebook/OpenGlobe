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

namespace MiniGlobe.Scene
{
    public sealed class DayNightGlobe : IDisposable
    {
        public DayNightGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

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
                  out vec4 dayColor;
                  out vec4 nightColor;
                  out float blendAlpha;

                  uniform mat4x2 mg_modelZToClipCoordinates;
                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform sampler2D mg_texture0;                    // Day
                  uniform sampler2D mg_texture1;                    // Night
                  uniform vec3 mg_sunPosition;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 u_cameraEyeSquared;
                  uniform vec3 u_globeOneOverRadiiSquared;
                  uniform float u_blendDuration;
                  uniform float u_blendDurationScale;
                  uniform bool u_useAverageDepth;";
            fs += RayIntersectEllipsoidGLSL();
            fs += ComputeWorldPositionDepthGLSL();
            fs +=
                @"vec3 ComputeDeticSurfaceNormal(vec3 positionOnEllipsoid, vec3 oneOverEllipsoidRadiiSquared)
                  {
                      return normalize(positionOnEllipsoid * oneOverEllipsoidRadiiSquared);
                  }

                  float LightIntensity(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      vec3 toReflectedLight = reflect(-toLight, normal);

                      float diffuse = max(diffuseDot, 0.0);
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

                  vec3 NightColor(vec3 normal)
                  {
                      return texture(mg_texture1, ComputeTextureCoordinates(normal)).rgb;
                  }

                  vec3 DayColor(vec3 normal, vec3 toLight, vec3 toEye, float diffuseDot, vec4 diffuseSpecularAmbientShininess)
                  {
                      float intensity = LightIntensity(normal, toLight, toEye, diffuseDot, diffuseSpecularAmbientShininess);
                      return intensity * texture(mg_texture0, ComputeTextureCoordinates(normal)).rgb;
                  }

                  void main()
                  {
                      vec3 rayDirection = normalize(worldPosition - mg_cameraEye);
                      Intersection i = RayIntersectEllipsoid(mg_cameraEye, u_cameraEyeSquared, rayDirection, u_globeOneOverRadiiSquared);

                      if (i.Intersects)
                      {
                          vec3 position = mg_cameraEye + (i.NearTime * rayDirection);
                          vec3 normal = ComputeDeticSurfaceNormal(position, u_globeOneOverRadiiSquared);

                          vec3 toLight = normalize(mg_sunPosition - position);
                          vec3 toEye = normalize(mg_cameraEye - position);

                          float diffuse = dot(toLight, normal);

                          dayColor = vec4(DayColor(normal, toLight, normalize(toLight), diffuse, mg_diffuseSpecularAmbientShininess), 1.0);
                          nightColor = vec4(NightColor(normal), 1.0);
                          blendAlpha = clamp((diffuse + u_blendDuration) * u_blendDurationScale, 0.0, 1.0);

                          if (u_useAverageDepth)
                          {
                              position = mg_cameraEye + (mix(i.NearTime, i.FarTime, 0.5) * rayDirection);
                          }

                          gl_FragDepth = ComputeWorldPositionDepth(position, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);
            _cameraEyeSquaredSP = _sp.Uniforms["u_cameraEyeSquared"] as Uniform<Vector3S>;
            _useAverageDepth = _sp.Uniforms["u_useAverageDepth"] as Uniform<bool>;

            float blendDurationScale = 0.1f;
            (_sp.Uniforms["u_blendDuration"] as Uniform<float>).Value = blendDurationScale;
            (_sp.Uniforms["u_blendDurationScale"] as Uniform<float>).Value = 1 / (2 * blendDurationScale);

            _renderState = new RenderState();

            Shape = Ellipsoid.UnitSphere;
            ShowGlobe = true;
        }

        private static string RayIntersectEllipsoidGLSL()
        {
            return
                @"struct Intersection
                  {
                      bool  Intersects;
                      float NearTime;         // Along ray
                      float FarTime;          // Along ray
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
                          return Intersection(false, 0.0, 0.0);
                      }
                      else if (discriminant == 0.0)
                      {
                          float time = -0.5 * b / a;
                          return Intersection(true, time, time);
                      }

                      float t = -0.5 * (b + (b > 0.0 ? 1.0 : -1.0) * sqrt(discriminant));
                      float root1 = t / a;
                      float root2 = c / t;

                      return Intersection(true, min(root1, root2), max(root1, root2));
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

        private void Clean()
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                }

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.Face = CullFace.Front;
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                (_sp.Uniforms["u_globeOneOverRadiiSquared"] as Uniform<Vector3S>).Value = _shape.OneOverRadiiSquared.ToVector3S();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                }
                _wireframe = new Wireframe(_context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirty = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Verify.ThrowInvalidOperationIfNull(DayTexture, "DayTexture");
            Verify.ThrowInvalidOperationIfNull(NightTexture, "NightTexture");

            Clean();

            if (ShowGlobe || ShowWireframeBoundingBox)
            {
                _context.Bind(_va);
            }

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3S cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3S();
                _cameraEyeSquaredSP.Value = cameraEyeSquared;

                _context.TextureUnits[0].Texture2D = DayTexture;
                _context.TextureUnits[1].Texture2D = NightTexture;
                _context.Bind(_sp);
                _context.Bind(_renderState);
                _context.Draw(_primitiveType, sceneState);
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(sceneState);
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
                _dirty = true;
                _shape = value;
            }
        }

        public bool ShowGlobe { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D DayTexture { get; set; }
        public Texture2D NightTexture { get; set; }

        public bool UseAverageDepth
        {
            get { return _useAverageDepth.Value; }
            set { _useAverageDepth.Value = value; }
        }

        public int FragmentOutputs(string name)
        {
            return _sp.FragmentOutputs[name];
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }
        }

        #endregion

        private readonly Context _context;

        private readonly RenderState _renderState;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3S> _cameraEyeSquaredSP;
        private readonly Uniform<bool> _useAverageDepth;
        
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}