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
using System.Collections.Generic;

namespace MiniGlobe.Terrain
{
    public sealed class RayCastedTerrainTile : IDisposable
    {
        public RayCastedTerrainTile(Context context, TerrainTile tile)
        {
            _context = context;
            
            string vs =
                @"#version 150

                  in vec4 position;
                  out vec3 boxExit;
                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                      boxExit = position.xyz;
                  }";
            string fs =
                @"#version 150
                 
                  in vec3 boxExit;

                  out vec3 fragmentColor;

                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform vec3 mg_cameraEye;
                  uniform mat4x2 mg_modelZToClipCoordinates;

                  uniform vec3 u_aabbLowerLeft;
                  uniform vec3 u_aabbUpperRight;
                  uniform float u_minimumHeight;
                  uniform float u_maximumHeight;
                  uniform float u_heightExaggeration;

                  struct Intersection
                  {
                      bool Intersects;
                      vec3 IntersectionPoint;
                  };

                  bool PointInsideAxisAlignedBoundingBox(vec3 point, vec3 lowerLeft, vec3 upperRight)
                  {
                      return all(greaterThanEqual(point, lowerLeft)) && all(lessThanEqual(point, upperRight));
                  }

                  void Swap(inout float left, inout float right)
                  {
                      float temp = left;
                      left = right;
                      right = temp;
                  }

                  bool PlanePairTest(
                      float origin, 
                      float direction, 
                      float aabbLowerLeft, 
                      float aabbUpperRight,
                      inout float tNear,
                      inout float tFar)
                  {
                      if (direction == 0.0)
                      {
                          //
                          // Ray is parallel to planes
                          //
                          if (origin < aabbLowerLeft || origin > aabbUpperRight)
                          {
                              return false;
                          }
                      }
                      else
                      {
                          //
                          // Compute the intersection distances of the planes
                          //
                          float oneOverDirection = 1.0 / direction;
                          float t1 = (aabbLowerLeft - origin) * oneOverDirection;
                          float t2 = (aabbUpperRight - origin) * oneOverDirection;

                          //
                          // Make t1 intersection with nearest plane
                          //
                          if (t1 > t2)
                          {
                              Swap(t1, t2);
                          }

                          //
                          // Track largest tNear and smallest tFar
                          //
                          tNear = max(t1, tNear);
                          tFar = min(t2, tFar);

                          //
                          // Missed box
                          //
                          if (tNear > tFar)
                          {
                              return false;
                          }

                          //
                          // Box is behind ray
                          //
                          if (tFar < 0.0)
                          {
                              return false;
                          }
                      }

                      return true;
                  }

                  Intersection RayIntersectsAABB(vec3 origin, vec3 direction, vec3 aabbLowerLeft, vec3 aabbUpperRight)
                  {
                      //
                      // Implementation of http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
                      //

                      float tNear = -100000.0;    // TODO:  How to get float max?
                      float tFar = 100000.0;

                      if (PlanePairTest(origin.x, direction.x, aabbLowerLeft.x, aabbUpperRight.x, tNear, tFar) &&
                          PlanePairTest(origin.y, direction.y, aabbLowerLeft.y, aabbUpperRight.y, tNear, tFar) &&
                          PlanePairTest(origin.z, direction.z, aabbLowerLeft.z, aabbUpperRight.z, tNear, tFar))
                      {
                          return Intersection(true, origin + (tNear * direction));
                      }

                      return Intersection(false, vec3(0.0));
                  }

                  void Mirror(
                      bool mirror,
                      float heightMapSize,
                      inout float boxEntry,
                      inout float direction,
                      inout float mirrorTextureCoordinates)
                  {
                      if (mirror)
                      {
                          direction = -direction;
                          boxEntry = heightMapSize - boxEntry;
                          mirrorTextureCoordinates = heightMapSize - 1.0;
                      }
                      else
                      {
                          mirrorTextureCoordinates = 0.0;
                      }
                  }

                  vec2 MirrorRepeat(vec2 textureCoordinate, vec2 mirrorTextureCoordinates)
                  {
                      return vec2(
                          mirrorTextureCoordinates.x == 0.0 ? textureCoordinate.x : mirrorTextureCoordinates.x - textureCoordinate.x, 
                          mirrorTextureCoordinates.y == 0.0 ? textureCoordinate.y : mirrorTextureCoordinates.y - textureCoordinate.y);
                  }

                  bool StepRay(
                      vec3 direction, 
                      vec2 oneOverDirectionXY,
                      vec2 mirrorTextureCoordinates,
                      inout vec3 texEntry,
                      out vec3 intersectionPoint)
                  {
                      vec2 floorTexEntry = floor(texEntry.xy);
                      float height = texture(mg_texture0, MirrorRepeat(floorTexEntry, mirrorTextureCoordinates)).r;
                      height *= u_heightExaggeration;

                      vec2 delta = ((floorTexEntry + vec2(1.0)) - texEntry.xy) * oneOverDirectionXY;
                      vec3 texExit = texEntry + (min(delta.x, delta.y) * direction);

                      //
                      // Explicitly set to avoid roundoff error
                      //
                      if (delta.x < delta.y)
                      {
                          texExit.x = floorTexEntry.x + 1.0;
                      }
                      else
                      {
                          texExit.y = floorTexEntry.y + 1.0;
                      }

                      //
                      // Check for intersection
                      //
                      bool foundIntersection = false;

                      if (direction.z >= 0.0)
                      {
                          if (texEntry.z <= height)
                          {
                              foundIntersection = true;
                              intersectionPoint = texEntry;
                          }
                      }
                      else
                      {
                          if (texExit.z <= height)
                          {
                              foundIntersection = true;
                              intersectionPoint = texEntry + (max((height - texEntry.z) / direction.z, 0.0) * direction);
                          }
                      }

                      texEntry = texExit;
                      return foundIntersection;
                  }

                  void UnMirrorIntersectionPoint(bvec2 mirror, vec2 heightMapSize, inout vec3 intersectionPoint)
                  {
                      if (mirror.x)
                      {
                          intersectionPoint.x = heightMapSize.x - intersectionPoint.x;
                      }

                      if (mirror.y)
                      {
                          intersectionPoint.y = heightMapSize.y - intersectionPoint.y;
                      }
                  }

                  float ComputeWorldPositionDepth(vec3 position, mat4x2 modelZToClipCoordinates)
                  { 
                      vec2 v = modelZToClipCoordinates * vec4(position, 1);   // clip coordinates
                      v.x /= v.y;                                             // normalized device coordinates
                      v.x = (v.x + 1.0) * 0.5;
                      return v.x;
                  }

                  void main()
                  {
                      vec3 direction = boxExit - mg_cameraEye;

                      vec3 boxEntry;
                      if (PointInsideAxisAlignedBoundingBox(mg_cameraEye, u_aabbLowerLeft, u_aabbUpperRight))
                      {
                          boxEntry = mg_cameraEye;
                      }
                      else
                      {
                          Intersection i = RayIntersectsAABB(mg_cameraEye, direction, u_aabbLowerLeft, u_aabbUpperRight);
                          boxEntry = i.IntersectionPoint;
                      }

                      vec2 heightMapSize = vec2(textureSize(mg_texture0, 0));

                      bvec2 mirror = lessThan(direction.xy, vec2(0.0));
                      vec2 mirrorTextureCoordinates;
                      Mirror(mirror.x, heightMapSize.x, boxEntry.x, direction.x, mirrorTextureCoordinates.x);
                      Mirror(mirror.y, heightMapSize.y, boxEntry.y, direction.y, mirrorTextureCoordinates.y);

                      vec2 oneOverDirectionXY = vec2(1.0) / direction.xy;
                      vec3 texEntry = boxEntry;
                      vec3 intersectionPoint;
                      bool foundIntersection = false;

                      while (!foundIntersection && all(lessThan(texEntry.xy, heightMapSize)))
                      {
                          foundIntersection = StepRay(direction, oneOverDirectionXY, 
                              mirrorTextureCoordinates, texEntry, intersectionPoint);
                      }

                      if (foundIntersection)
                      {
                          UnMirrorIntersectionPoint(mirror, heightMapSize, intersectionPoint);

                          fragmentColor = vec3((intersectionPoint.z - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
                          gl_FragDepth = ComputeWorldPositionDepth(intersectionPoint, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            _tileSize = tile.Size;
            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;
            _tileAABBLowerLeft = Vector3D.Zero;             // TEXEL_SPACE_TODO
            _tileAABBUpperRight = new Vector3D(tile.Size.X, tile.Size.Y,
                tile.MaximumHeight - tile.MinimumHeight);

            _heightExaggeration = _sp.Uniforms["u_heightExaggeration"] as Uniform<float>;
            _minimumHeight = _sp.Uniforms["u_minimumHeight"] as Uniform<float>;
            _maximumHeight = _sp.Uniforms["u_maximumHeight"] as Uniform<float>;
            _aabbLowerLeft = _sp.Uniforms["u_aabbLowerLeft"] as Uniform<Vector3S>;
            _aabbUpperRight = _sp.Uniforms["u_aabbUpperRight"] as Uniform<Vector3S>;
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;

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
        }

        private void Update()
        {
            if (_dirtyVA)
            {
                Vector3D radii = new Vector3D(_tileSize.X, _tileSize.Y,
                    (_tileMaximumHeight - _tileMinimumHeight) * _heightExaggeration.Value);
                Vector3D halfRadii = 0.5 * radii;

                Mesh mesh = BoxTessellator.Compute(radii);

                //
                // TEXEL_SPACE_TODO:  Translate box so it is not centered at 
                // the origin - world space and texel space will match up.
                //
                IList<Vector3D> positions = (mesh.Attributes["position"] as VertexAttributeDoubleVector3).Values;
                for (int i = 0; i < positions.Count; ++i)
                {
                    positions[i] = positions[i] + halfRadii;
                }

                if (_va != null)
                {
                    _va.Dispose();
                }
                _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;
                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _dirtyVA = false;
            }
        }

        public void Render(SceneState sceneState)
        {
            Update();

            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set 
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration", "HeightExaggeration must be greater than zero.");
                }

                if (_heightExaggeration.Value != value)
                {
                    //
                    // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                    // scale will incorrectly move the entire tile.
                    //
                    _heightExaggeration.Value = value;
                    _minimumHeight.Value = _tileMinimumHeight * value;
                    _maximumHeight.Value = _tileMaximumHeight * value;
                    _aabbLowerLeft.Value = new Vector3S((float)_tileAABBLowerLeft.X, (float)_tileAABBLowerLeft.Y, (float)(_tileAABBLowerLeft.Z * value));
                    _aabbUpperRight.Value = new Vector3S((float)_tileAABBUpperRight.X, (float)_tileAABBUpperRight.Y, (float)(_tileAABBUpperRight.Z * value));

                    _dirtyVA = true;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _sp.Dispose();
            _va.Dispose();
            _texture.Dispose();
        }

        #endregion

        private readonly Context _context;
        private readonly ShaderProgram _sp;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;
        private readonly Uniform<Vector3S> _aabbLowerLeft;
        private readonly Uniform<Vector3S> _aabbUpperRight;

        private readonly Vector2I _tileSize;
        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;
        private readonly Vector3D _tileAABBLowerLeft;
        private readonly Vector3D _tileAABBUpperRight;

        private readonly Texture2D _texture;
        private readonly RenderState _renderState;

        private VertexArray _va;
        private PrimitiveType _primitiveType;
        private bool _dirtyVA;
    }
}