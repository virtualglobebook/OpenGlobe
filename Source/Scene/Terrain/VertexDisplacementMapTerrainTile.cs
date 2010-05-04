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
using System.Drawing;
using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;
using MiniGlobe.Core.Tessellation;
using MiniGlobe.Renderer;
using MiniGlobe.Scene;

namespace MiniGlobe.Terrain
{
    public enum TerrainNormals
    {
        None,
        ThreeSamples,
        FourSamples,
        SobelFilter
    }

    public sealed class VertexDisplacementMapTerrainTile : IDisposable
    {
        public VertexDisplacementMapTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;


            string vs =
                @"#version 150

                  in vec2 position;
                  
                  out vec3 normal;
                  out vec3 positionToLight;
                  out vec3 positionToEye;
                  out float height;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_cameraEye;
                  uniform vec3 mg_cameraLightPosition;
                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform float u_heightExaggeration;

                  void main()
                  {
                      vec4 displacedPosition = vec4(position.xy, 
                          texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                      height = displacedPosition.z;

//                      vec3 left = vec3(position.xy - vec2(1.0, 0.0), texture(mg_texture0, position.xy - vec2(1.0, 0.0)).r * u_heightExaggeration);
//                      vec3 right = vec3(position.xy + vec2(1.0, 0.0), texture(mg_texture0, position.xy + vec2(1.0, 0.0)).r * u_heightExaggeration);
//                      vec3 bottom = vec3(position.xy - vec2(0.0, 1.0), texture(mg_texture0, position.xy - vec2(0.0, 1.0)).r * u_heightExaggeration);
//                      vec3 top = vec3(position.xy + vec2(0.0, 1.0), texture(mg_texture0, position.xy + vec2(0.0, 1.0)).r * u_heightExaggeration);
//                      normal = cross(right - left, top - bottom);

                      vec3 right = vec3(position.xy + vec2(1.0, 0.0), texture(mg_texture0, position.xy + vec2(1.0, 0.0)).r * u_heightExaggeration);
                      vec3 top = vec3(position.xy + vec2(0.0, 1.0), texture(mg_texture0, position.xy + vec2(0.0, 1.0)).r * u_heightExaggeration);
                      normal = cross(right - displacedPosition.xyz, top - displacedPosition.xyz);

                      positionToLight = mg_cameraLightPosition - displacedPosition.xyz;
                      positionToEye = mg_cameraEye - displacedPosition.xyz;
                  }";
            string fs =
                @"#version 150
                 
                  in float height;
                  in vec3 normal;
                  in vec3 positionToLight;
                  in vec3 positionToEye;

                  out vec3 fragmentColor;

                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform float u_minimumHeight;
                  uniform float u_maximumHeight;

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

                  void main()
                  {
                      float intensity = LightIntensity(normalize(normal),  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                      //fragmentColor = intensity * vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                      fragmentColor = vec3(intensity, intensity, intensity);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggerationUniform = _sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            //_minimumHeight = _sp.Uniforms["u_minimumHeight"] as Uniform<float>;
            //_maximumHeight = _sp.Uniforms["u_maximumHeight"] as Uniform<float>;
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.X, tile.Size.Y, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.5, 0.5), 
                new Vector2D((double)tile.Size.X - 0.5, (double)tile.Size.Y - 0.5)), 
                tile.Size.X - 1, tile.Size.Y - 1);
            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            ///////////////////////////////////////////////////////////////////

            _rsWireframe = new RenderState();
            _rsWireframe.Blending.Enabled = true;
            _rsWireframe.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _rsWireframe.DepthTest.Function = DepthTestFunction.LessThanOrEqual;

            string vsWireframe =
                @"#version 150

                  in vec4 position;
                  out vec2 windowPosition;
                  out float distanceToEyeGS;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;
                  uniform vec3 mg_cameraEye;
                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform float u_heightExaggeration;

                  vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
                  {
                      v.xyz /= v.w;                                                        // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void main()                     
                  {
                      vec4 displacedPosition = vec4(position.xy, 
                          texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                      windowPosition = ClipToWindowCoordinates(gl_Position, mg_viewportTransformationMatrix).xy;
                      distanceToEyeGS = distance(displacedPosition.xyz, mg_cameraEye);
                  }";
            string gsWireframe =
                @"#version 150 

                  layout(triangles) in;
                  layout(triangle_strip, max_vertices = 3) out;

                  in vec2 windowPosition[];
                  in float distanceToEyeGS[];

                  noperspective out vec3 distanceToEdges;
                  out float distanceToEyeFS;

                  float distanceToLine(vec2 f, vec2 p0, vec2 p1)
                  {
                      vec2 l = f - p0;
                      vec2 d = p1 - p0;

                      //
                      // Closed point on line to f
                      //
                      vec2 p = p0 + (d * (dot(l, d) / dot(d, d)));
                      return distance(f, p);
                  }

                  void main()
                  {
                      vec2 p0 = windowPosition[0];
                      vec2 p1 = windowPosition[1];
                      vec2 p2 = windowPosition[2];

                      gl_Position = gl_in[0].gl_Position;
                      distanceToEdges = vec3(distanceToLine(p0, p1, p2), 0.0, 0.0);
                      distanceToEyeFS = distanceToEyeGS[0];
                      EmitVertex();

                      gl_Position = gl_in[1].gl_Position;
                      distanceToEdges = vec3(0.0, distanceToLine(p1, p2, p0), 0.0);
                      distanceToEyeFS = distanceToEyeGS[1];
                      EmitVertex();

                      gl_Position = gl_in[2].gl_Position;
                      distanceToEdges = vec3(0.0, 0.0, distanceToLine(p2, p0, p1));
                      distanceToEyeFS = distanceToEyeGS[2];
                      EmitVertex();
                  }";
            string fsWireframe =
                @"#version 150
                 
                  uniform float u_halfLineWidth;
                  uniform vec3 u_colorUniform;

                  noperspective in vec3 distanceToEdges;
                  in float distanceToEyeFS;

                  out vec4 fragmentColor;

                  void main()
                  {
                      float d = min(distanceToEdges.x, min(distanceToEdges.y, distanceToEdges.z));

                      if (d > u_halfLineWidth + 1.0)
                      {
                          discard;
                      }

                      d = clamp(d - (u_halfLineWidth - 1.0), 0.0, 2.0);
                      float a = exp2(-2.0 * d * d);

                      //
                      // Apply linear attenuation to alpha
                      //
                      a *= min(1.0 / (0.015 * distanceToEyeFS), 1.0);
                      if (a == 0.0)
                      {
                          discard;
                      }

                      fragmentColor = vec4(u_colorUniform, a);
                  }";
            _spWireframe = Device.CreateShaderProgram(vsWireframe, gsWireframe, fsWireframe);

            _lineWidthWireframe = _spWireframe.Uniforms["u_halfLineWidth"] as Uniform<float>;
            _heightExaggerationWireframe = _spWireframe.Uniforms["u_heightExaggeration"] as Uniform<float>;
            (_spWireframe.Uniforms["u_colorUniform"] as Uniform<Vector3S>).Value = Vector3S.Zero;
            
            ///////////////////////////////////////////////////////////////////

            ShowTerrain = true;
        }

        private void Update(SceneState sceneState)
        {
            if (_spDirty)
            {
                _spDirty = false;

                _sp.Dispose();
                _sp = null;

                string vs = string.Empty;
                string fs = string.Empty;

                if (_normals == TerrainNormals.None)
                {
                    vs =
                        @"#version 150

                          in vec2 position;
                  
                          out float height;

                          uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                          uniform sampler2DRect mg_texture0;    // Height field
                          uniform float u_heightExaggeration;

                          void main()
                          {
                              vec4 displacedPosition = vec4(position.xy, 
                                  texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                              gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                              height = displacedPosition.z;
                          }";
                }
                if (_normals == TerrainNormals.ThreeSamples)
                {
                    vs =
                        @"#version 150

                          in vec2 position;
                  
                          out vec3 normal;
                          out vec3 positionToLight;
                          out vec3 positionToEye;
                          out float height;

                          uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                          uniform vec3 mg_cameraEye;
                          uniform vec3 mg_cameraLightPosition;
                          uniform sampler2DRect mg_texture0;    // Height field
                          uniform float u_heightExaggeration;

                          vec3 ComputeNormalThreeSamples(
                              vec3 displacedPosition, 
                              sampler2DRect heightField, 
                              float heightExaggeration)
                          {
                              vec3 right = vec3(displacedPosition.xy + vec2(1.0, 0.0), texture(heightField, displacedPosition.xy + vec2(1.0, 0.0)).r * heightExaggeration);
                              vec3 top = vec3(displacedPosition.xy + vec2(0.0, 1.0), texture(heightField, displacedPosition.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                              return cross(right - displacedPosition, top - displacedPosition);
                          }

                          void main()
                          {
                              vec4 displacedPosition = vec4(position.xy, 
                                  texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                              gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                              height = displacedPosition.z;
                              normal = ComputeNormalThreeSamples(displacedPosition.xyz, mg_texture0, u_heightExaggeration);
                              positionToLight = mg_cameraLightPosition - displacedPosition.xyz;
                              positionToEye = mg_cameraEye - displacedPosition.xyz;
                          }";
                }
                else if (_normals == TerrainNormals.FourSamples)
                {
                    vs =
                        @"#version 150

                          in vec2 position;
                  
                          out vec3 normal;
                          out vec3 positionToLight;
                          out vec3 positionToEye;
                          out float height;

                          uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                          uniform vec3 mg_cameraEye;
                          uniform vec3 mg_cameraLightPosition;
                          uniform sampler2DRect mg_texture0;    // Height field
                          uniform float u_heightExaggeration;

                          vec3 ComputeNormalFourSamples(
                              vec2 position, 
                              sampler2DRect heightField, 
                              float heightExaggeration)
                          {
                              vec3 left = vec3(position - vec2(1.0, 0.0), texture(heightField, position - vec2(1.0, 0.0)).r * heightExaggeration);
                              vec3 right = vec3(position + vec2(1.0, 0.0), texture(heightField, position + vec2(1.0, 0.0)).r * heightExaggeration);
                              vec3 bottom = vec3(position - vec2(0.0, 1.0), texture(heightField, position - vec2(0.0, 1.0)).r * heightExaggeration);
                              vec3 top = vec3(position + vec2(0.0, 1.0), texture(heightField, position.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                              return cross(right - left, top - bottom);
                          }

                          void main()
                          {
                              vec4 displacedPosition = vec4(position.xy, 
                                  texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                              gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                              height = displacedPosition.z;
                              normal = ComputeNormalFourSamples(position, mg_texture0, u_heightExaggeration);
                              positionToLight = mg_cameraLightPosition - displacedPosition.xyz;
                              positionToEye = mg_cameraEye - displacedPosition.xyz;
                          }";
                }
                else if (_normals == TerrainNormals.SobelFilter)
                {
                    vs =
                        @"#version 150

                          in vec2 position;
                  
                          out vec3 normal;
                          out vec3 positionToLight;
                          out vec3 positionToEye;
                          out float height;

                          uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                          uniform vec3 mg_cameraEye;
                          uniform vec3 mg_cameraLightPosition;
                          uniform sampler2DRect mg_texture0;    // Height field
                          uniform float u_heightExaggeration;

                          float SumElements(mat3 m)
                          {
                              return 
                                  m[0].x + m[0].y + m[0].z +
                                  m[1].x + m[1].y + m[1].z +
                                  m[2].x + m[2].y + m[2].z;
                          }

                          vec3 ComputeNormalSobelFilter(
                              vec2 position, 
                              sampler2DRect heightField, 
                              float heightExaggeration)
                          {
                              float upperLeft = texture(heightField, position + vec2(-1.0, 1.0)).r * heightExaggeration;
                              float upperCenter = texture(heightField, position + vec2(0.0, 1.0)).r * heightExaggeration;
                              float upperRight = texture(heightField, position + vec2(1.0, 1.0)).r * heightExaggeration;
                              float left = texture(heightField, position + vec2(-1.0, 0.0)).r * heightExaggeration;
                              float right = texture(heightField, position + vec2(1.0, 0.0)).r * heightExaggeration;
                              float lowerLeft = texture(heightField, position + vec2(-1.0, -1.0)).r * heightExaggeration;
                              float lowerCenter = texture(heightField, position + vec2(0.0, -1.0)).r * heightExaggeration;
                              float lowerRight = texture(heightField, position + vec2(1.0, -1.0)).r * heightExaggeration;

                              mat3 positions = mat3(
                                  upperLeft, left, lowerLeft,
                                  upperCenter, 0.0, lowerCenter,
                                  upperRight, right, lowerRight);

                              mat3 sobelX = mat3(
                                  -1.0, -2.0, -1.0,
                                   0.0,  0.0,  0.0,
                                   1.0,  2.0,  1.0);
                              mat3 sobelY = mat3(
                                  -1.0, 0.0, 1.0,
                                  -2.0, 0.0, 2.0,
                                  -1.0, 0.0, 1.0);

                              float x = SumElements(matrixCompMult(positions, sobelX));
                              float y = SumElements(matrixCompMult(positions, sobelY));

                              return normalize(vec3(x, y, 1.0 / 2.0));
                          }

                          void main()
                          {
                              vec4 displacedPosition = vec4(position.xy, 
                                  texture(mg_texture0, position.xy).r * u_heightExaggeration, 1.0);

                              gl_Position = mg_modelViewPerspectiveProjectionMatrix * displacedPosition;
                              height = displacedPosition.z;
                              normal = ComputeNormalSobelFilter(position, mg_texture0, u_heightExaggeration);
                              positionToLight = mg_cameraLightPosition - displacedPosition.xyz;
                              positionToEye = mg_cameraEye - displacedPosition.xyz;
                          }";
                }

                if (_normals == TerrainNormals.None)
                {
                    fs =
                        @"#version 150
                 
                          in float height;

                          out vec3 fragmentColor;

                          uniform float u_minimumHeight;
                          uniform float u_maximumHeight;

                          void main()
                          {
                              //fragmentColor = intensity * vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                              fragmentColor = vec3(1.0, 0.0, 0.0);
                          }";
                }
                else
                {
                    fs =
                        @"#version 150
                 
                          in float height;
                          in vec3 normal;
                          in vec3 positionToLight;
                          in vec3 positionToEye;

                          out vec3 fragmentColor;

                          uniform vec4 mg_diffuseSpecularAmbientShininess;
                          uniform float u_minimumHeight;
                          uniform float u_maximumHeight;

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

                          void main()
                          {
                              float intensity = LightIntensity(normalize(normal),  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                              //fragmentColor = intensity * vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                              fragmentColor = vec3(intensity, 0.0, 0.0);
                          }";
                }

                _sp = Device.CreateShaderProgram(vs, fs);
                _heightExaggerationUniform = _sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            }

            _heightExaggerationUniform.Value = _heightExaggeration;
            _heightExaggerationWireframe.Value = _heightExaggeration;
            _lineWidthWireframe.Value = (float)(0.5 * 3.0 * sceneState.HighResolutionSnapScale);
        }

        public void Render(SceneState sceneState)
        {
            if (ShowTerrain || ShowWireframe)
            {
                Update(sceneState);

                _context.TextureUnits[0].Texture2DRectangle = _texture;
                _context.Bind(_va);

                if (ShowTerrain)
                {
                    _context.Bind(_sp);
                    _context.Bind(_renderState);
                    _context.Draw(_primitiveType, sceneState);
                }

                if (ShowWireframe)
                {
                    _context.Bind(_spWireframe);
                    _context.Bind(_rsWireframe);
                    _context.Draw(_primitiveType, sceneState);
                }
            }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration", "HeightExaggeration must be greater than zero.");
                }

                //
                // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                // scale will incorrectly move the entire tile.
                //
                _heightExaggeration = value;
                //_minimumHeight.Value = _tileMinimumHeight * value;
                //_maximumHeight.Value = _tileMaximumHeight * value;
            }
        }

        public TerrainNormals Normals 
        {
            get { return _normals; }
            set
            {
                if (_normals != value)
                {
                    _normals = value;
                    _spDirty = true;
                }
            }
        }

        public bool ShowTerrain { get; set; }
        public bool ShowWireframe { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _texture.Dispose();
            _spWireframe.Dispose();
        }

        #endregion

        private readonly Context _context;
        private ShaderProgram _sp;

        private Uniform<float> _heightExaggerationUniform;
        private Uniform<float> _minimumHeight;
        private Uniform<float> _maximumHeight;

        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private readonly Texture2D _texture;
        private readonly RenderState _renderState;

        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;

        private readonly RenderState _rsWireframe;
        private readonly ShaderProgram _spWireframe;
        private readonly Uniform<float> _heightExaggerationWireframe;
        private readonly Uniform<float> _lineWidthWireframe;

        private float _heightExaggeration;
        private TerrainNormals _normals;
        private bool _spDirty;
    }
}