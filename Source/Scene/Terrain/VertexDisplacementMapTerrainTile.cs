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

            string vsTerrain =
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
                  uniform int u_normalAlgorithm;

                  vec3 ComputeNormalThreeSamples(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec3 right = vec3(displacedPosition.xy + vec2(1.0, 0.0), texture(heightField, displacedPosition.xy + vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 top = vec3(displacedPosition.xy + vec2(0.0, 1.0), texture(heightField, displacedPosition.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                      return cross(right - displacedPosition, top - displacedPosition);
                  }

                  vec3 ComputeNormalFourSamples(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec2 position = displacedPosition.xy;
                      vec3 left = vec3(position - vec2(1.0, 0.0), texture(heightField, position - vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 right = vec3(position + vec2(1.0, 0.0), texture(heightField, position + vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 bottom = vec3(position - vec2(0.0, 1.0), texture(heightField, position - vec2(0.0, 1.0)).r * heightExaggeration);
                      vec3 top = vec3(position + vec2(0.0, 1.0), texture(heightField, position.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                      return cross(right - left, top - bottom);
                  }

                  float SumElements(mat3 m)
                  {
                      return 
                          m[0].x + m[0].y + m[0].z +
                          m[1].x + m[1].y + m[1].z +
                          m[2].x + m[2].y + m[2].z;
                  }

                  vec3 ComputeNormalSobelFilter(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec2 position = displacedPosition.xy;
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

                      return normalize(vec3(x, y, 1.0 * 8.0));
                  }

                  void main()
                  {
                      vec3 displacedPosition = vec3(position.xy, texture(mg_texture0, position.xy).r * u_heightExaggeration);

                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * vec4(displacedPosition, 1.0);
                      height = displacedPosition.z;

                      if (u_normalAlgorithm != 0)
                      {
                          if (u_normalAlgorithm == 1)
                          {
                              normal = ComputeNormalThreeSamples(displacedPosition, mg_texture0, u_heightExaggeration);
                          }
                          else if (u_normalAlgorithm == 2)
                          {
                              normal = ComputeNormalFourSamples(displacedPosition, mg_texture0, u_heightExaggeration);
                          }
                          else if (u_normalAlgorithm == 3)
                          {
                              normal = ComputeNormalSobelFilter(displacedPosition, mg_texture0, u_heightExaggeration);
                          }

                          positionToLight = mg_cameraLightPosition - displacedPosition;
                          positionToEye = mg_cameraEye - displacedPosition;
                      }
                  }";

            string fsTerrain =
                @"#version 150
                 
                  in float height;
                  in vec3 normal;
                  in vec3 positionToLight;
                  in vec3 positionToEye;

                  out vec3 fragmentColor;

                  uniform vec4 mg_diffuseSpecularAmbientShininess;
                  uniform float u_minimumHeight;
                  uniform float u_maximumHeight;
                  uniform int u_normalAlgorithm;

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
                      if (u_normalAlgorithm == 0)
                      {
                          fragmentColor = vec3(0.0, 1.0, 0.0);
                      }
                      else
                      {
                          float intensity = LightIntensity(normalize(normal),  normalize(positionToLight), normalize(positionToEye), mg_diffuseSpecularAmbientShininess);
                          //fragmentColor = intensity * vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                          fragmentColor = vec3(0.0, intensity, 0.0);
                      }
                  }";
            _spTerrain = Device.CreateShaderProgram(vsTerrain, fsTerrain);
            _heightExaggerationUniform = _spTerrain.Uniforms["u_heightExaggeration"] as Uniform<float>;
            //_minimumHeight = _spTerrain.Uniforms["u_minimumHeight"] as Uniform<float>;
            //_maximumHeight = _spTerrain.Uniforms["u_maximumHeight"] as Uniform<float>;

            ///////////////////////////////////////////////////////////////////

            string vsNormals =
                @"#version 150

                  in vec2 position;
                  
                  out float distanceToEyeGS;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform vec3 mg_cameraEye;
                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform float u_heightExaggeration;

                  void main()
                  {
                      vec3 displacedPosition = vec3(position, texture(mg_texture0, position).r * u_heightExaggeration);

                      gl_Position = vec4(displacedPosition, 1.0);
                      distanceToEyeGS = distance(displacedPosition, mg_cameraEye);
                  }";
            string gsNormals =
                @"#version 150 

                  layout(points) in;
                  layout(triangle_strip, max_vertices = 4) out;

                  in float distanceToEyeGS[];
                  out float distanceToEyeFS;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                  uniform mat4 mg_viewportTransformationMatrix;
                  uniform mat4 mg_viewportOrthographicProjectionMatrix;
                  uniform float mg_perspectiveNearPlaneDistance;
                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform float u_heightExaggeration;
                  uniform float u_fillDistance;
                  uniform int u_normalAlgorithm;

                  vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
                  {
                      v.xyz /= v.w;                                                        // normalized device coordinates
                      v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
                      return v;
                  }

                  void ClipLineSegmentToNearPlane(
                      float nearPlaneDistance, 
                      mat4 modelViewPerspectiveProjectionMatrix,
                      vec4 modelP0, 
                      vec4 modelP1, 
                      out vec4 clipP0, 
                      out vec4 clipP1)
                  {
                      clipP0 = modelViewPerspectiveProjectionMatrix * modelP0;
                      clipP1 = modelViewPerspectiveProjectionMatrix * modelP1;

                      float distanceToP0 = clipP0.z - nearPlaneDistance;
                      float distanceToP1 = clipP1.z - nearPlaneDistance;

                      if ((distanceToP0 * distanceToP1) < 0.0)
                      {
                          float t = distanceToP0 / (distanceToP0 - distanceToP1);
                          vec3 modelV = vec3(modelP0) + t * (vec3(modelP1) - vec3(modelP0));
                          vec4 clipV = modelViewPerspectiveProjectionMatrix * vec4(modelV, 1);

                          if (distanceToP0 < 0.0)
                          {
                              clipP0 = clipV;
                          }
                          else
                          {
                              clipP1 = clipV;
                          }
                      }
                  }

                  vec3 ComputeNormalThreeSamples(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec3 right = vec3(displacedPosition.xy + vec2(1.0, 0.0), texture(heightField, displacedPosition.xy + vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 top = vec3(displacedPosition.xy + vec2(0.0, 1.0), texture(heightField, displacedPosition.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                      return cross(right - displacedPosition, top - displacedPosition);
                  }

                  vec3 ComputeNormalFourSamples(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec2 position = displacedPosition.xy;
                      vec3 left = vec3(position - vec2(1.0, 0.0), texture(heightField, position - vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 right = vec3(position + vec2(1.0, 0.0), texture(heightField, position + vec2(1.0, 0.0)).r * heightExaggeration);
                      vec3 bottom = vec3(position - vec2(0.0, 1.0), texture(heightField, position - vec2(0.0, 1.0)).r * heightExaggeration);
                      vec3 top = vec3(position + vec2(0.0, 1.0), texture(heightField, position.xy + vec2(0.0, 1.0)).r * heightExaggeration);
                      return cross(right - left, top - bottom);
                  }

                  float SumElements(mat3 m)
                  {
                      return 
                          m[0].x + m[0].y + m[0].z +
                          m[1].x + m[1].y + m[1].z +
                          m[2].x + m[2].y + m[2].z;
                  }

                  vec3 ComputeNormalSobelFilter(
                      vec3 displacedPosition, 
                      sampler2DRect heightField, 
                      float heightExaggeration)
                  {
                      vec2 position = displacedPosition.xy;
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

                      return normalize(vec3(x, y, 1.0 * 8.0));
                  }

                  void main()
                  {
                      vec3 terrainNormal = vec3(0.0);

                      if (u_normalAlgorithm == 1)
                      {
                          terrainNormal = ComputeNormalThreeSamples(gl_in[0].gl_Position.xyz, mg_texture0, u_heightExaggeration);
                      }
                      else if (u_normalAlgorithm == 2)
                      {
                          terrainNormal = ComputeNormalFourSamples(gl_in[0].gl_Position.xyz, mg_texture0, u_heightExaggeration);
                      }
                      else if (u_normalAlgorithm == 3)
                      {
                          terrainNormal = ComputeNormalSobelFilter(gl_in[0].gl_Position.xyz, mg_texture0, u_heightExaggeration);
                      }

                      vec4 clipP0;
                      vec4 clipP1;
                      ClipLineSegmentToNearPlane(mg_perspectiveNearPlaneDistance, 
                      mg_modelViewPerspectiveProjectionMatrix,
                      gl_in[0].gl_Position, 
                      gl_in[0].gl_Position + vec4(normalize(terrainNormal), 0.0),
                      clipP0, clipP1);

                      vec4 windowP0 = ClipToWindowCoordinates(clipP0, mg_viewportTransformationMatrix);
                      vec4 windowP1 = ClipToWindowCoordinates(clipP1, mg_viewportTransformationMatrix);

                      vec2 direction = windowP1.xy - windowP0.xy;
                      vec2 normal = normalize(vec2(direction.y, -direction.x));

                      vec4 v0 = vec4(windowP0.xy - (normal * u_fillDistance), windowP0.z, 1.0);
                      vec4 v1 = vec4(windowP1.xy - (normal * u_fillDistance), windowP1.z, 1.0);
                      vec4 v2 = vec4(windowP0.xy + (normal * u_fillDistance), windowP0.z, 1.0);
                      vec4 v3 = vec4(windowP1.xy + (normal * u_fillDistance), windowP1.z, 1.0);

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      distanceToEyeFS = distanceToEyeGS[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      distanceToEyeFS = distanceToEyeGS[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      distanceToEyeFS = distanceToEyeGS[0];
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      distanceToEyeFS = distanceToEyeGS[0];
                      EmitVertex();
                  }";
            string fsNormals =
                @"#version 150
                 
                  in float distanceToEyeFS;
                  out vec4 fragmentColor;
                  uniform vec3 u_color;

                  void main()
                  {
                      //
                      // Apply linear attenuation to alpha
                      //
                      float a = min(1.0 / (0.015 * distanceToEyeFS), 1.0);
                      if (a == 0.0)
                      {
                          discard;
                      }

                      fragmentColor = vec4(u_color, a);
                  }";
            _spNormals = Device.CreateShaderProgram(vsNormals, gsNormals, fsNormals);
            _heightExaggerationNormals = _spNormals.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _fillDistanceNormals = _spNormals.Uniforms["u_fillDistance"] as Uniform<float>;
            (_spNormals.Uniforms["u_color"] as Uniform<Vector3S>).Value = Vector3S.Zero;

            ///////////////////////////////////////////////////////////////////

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
                  uniform vec3 u_color;

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

                      fragmentColor = vec4(u_color, a);
                  }";
            _spWireframe = Device.CreateShaderProgram(vsWireframe, gsWireframe, fsWireframe);

            _lineWidthWireframe = _spWireframe.Uniforms["u_halfLineWidth"] as Uniform<float>;
            _heightExaggerationWireframe = _spWireframe.Uniforms["u_heightExaggeration"] as Uniform<float>;
            (_spWireframe.Uniforms["u_color"] as Uniform<Vector3S>).Value = Vector3S.Zero;
            
            ///////////////////////////////////////////////////////////////////

            Mesh mesh = RectangleTessellator.Compute(new RectangleD(new Vector2D(0.5, 0.5),
                new Vector2D((double)tile.Size.X - 0.5, (double)tile.Size.Y - 0.5)),
                tile.Size.X - 1, tile.Size.Y - 1);
            _va = _context.CreateVertexArray(mesh, _spWireframe.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            ///////////////////////////////////////////////////////////////////

            _rsTerrain = new RenderState();
            _rsTerrain.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            _rsWireframe = new RenderState();
            _rsWireframe.Blending.Enabled = true;
            _rsWireframe.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _rsWireframe.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsWireframe.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _rsWireframe.DepthTest.Function = DepthTestFunction.LessThanOrEqual;

            _rsNormals = new RenderState();
            _rsNormals.FacetCulling.Enabled = false;
            _rsNormals.Blending.Enabled = true;
            _rsNormals.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _rsNormals.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _rsNormals.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _rsNormals.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;

            ///////////////////////////////////////////////////////////////////

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = 1;
            _normals = TerrainNormals.ThreeSamples;
            ShowTerrain = true;

            _dirty = true;
        }

        private void Update(SceneState sceneState)
        {
            if (_dirty)
            {
                (_spTerrain.Uniforms["u_normalAlgorithm"] as Uniform<int>).Value = (int)_normals;
                (_spNormals.Uniforms["u_normalAlgorithm"] as Uniform<int>).Value = (int)_normals;

                _dirty = false;
            }

            _heightExaggerationUniform.Value = _heightExaggeration;
            //_minimumHeight.Value = _tileMinimumHeight * _heightExaggeration;
            //_maximumHeight.Value = _tileMaximumHeight * _heightExaggeration;

            _heightExaggerationWireframe.Value = _heightExaggeration;
            _lineWidthWireframe.Value = (float)(0.5 * 3.0 * sceneState.HighResolutionSnapScale);

            if (_normals != TerrainNormals.None)
            {
                _heightExaggerationNormals.Value = _heightExaggeration;
                _fillDistanceNormals.Value = (float)(0.5 * 3.0 * sceneState.HighResolutionSnapScale);
            }
        }

        public void Render(SceneState sceneState)
        {
            if (ShowTerrain || ShowWireframe || ShowNormals)
            {
                Update(sceneState);

                _context.TextureUnits[0].Texture2DRectangle = _texture;
                _context.Bind(_va);

                if (ShowTerrain)
                {
                    _context.Bind(_spTerrain);
                    _context.Bind(_rsTerrain);
                    _context.Draw(_primitiveType, sceneState);
                }

                if (ShowWireframe)
                {
                    _context.Bind(_spWireframe);
                    _context.Bind(_rsWireframe);
                    _context.Draw(_primitiveType, sceneState);
                }

                if (ShowNormals && (_normals != TerrainNormals.None))
                {
                    _context.Bind(_spNormals);
                    _context.Bind(_rsNormals);
                    _context.Draw(PrimitiveType.Points, sceneState);
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
                    _dirty = true;
                }
            }
        }

        public bool ShowTerrain { get; set; }
        public bool ShowWireframe { get; set; }
        public bool ShowNormals { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _spTerrain.Dispose();
            _spWireframe.Dispose();
            _spNormals.Dispose();

            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;

        private readonly RenderState _rsTerrain;
        private readonly ShaderProgram _spTerrain;
        private readonly Uniform<float> _heightExaggerationUniform;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;

        private readonly RenderState _rsWireframe;
        private readonly ShaderProgram _spWireframe;
        private readonly Uniform<float> _heightExaggerationWireframe;
        private readonly Uniform<float> _lineWidthWireframe;

        private readonly RenderState _rsNormals;
        private readonly ShaderProgram _spNormals;
        private readonly Uniform<float> _heightExaggerationNormals;
        private readonly Uniform<float> _fillDistanceNormals;

        private readonly Texture2D _texture;
        private readonly VertexArray _va;
        private readonly PrimitiveType _primitiveType;

        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private float _heightExaggeration;
        private TerrainNormals _normals;
        private bool _dirty;
    }
}