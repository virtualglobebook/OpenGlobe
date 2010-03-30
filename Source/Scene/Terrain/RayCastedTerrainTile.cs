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

                          fragmentColor = vec3(intersectionPoint.z / 0.5, 0.0, 0.0);
                          gl_FragDepth = ComputeWorldPositionDepth(intersectionPoint, mg_modelZToClipCoordinates);
                      }
                      else
                      {
                          discard;
                      }
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            Vector3D radii = new Vector3D(
                tile.Extent.East - tile.Extent.West,
                tile.Extent.North - tile.Extent.South,
                tile.MaximumHeight - tile.MinimumHeight);
            Vector3D halfRadii = 0.5 * radii;

            (_sp.Uniforms["u_aabbLowerLeft"] as Uniform<Vector3S>).Value = Vector3S.Zero;
            (_sp.Uniforms["u_aabbUpperRight"] as Uniform<Vector3S>).Value = radii.ToVector3S();

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = BoxTessellator.Compute(radii);

            //
            // Translate box so it is not centered at the origin -
            // world space and texel space will match up.
            // TODO:  We don't always want this!
            //
            IList<Vector3D> positions = (mesh.Attributes["position"] as VertexAttributeDoubleVector3).Values;
            for (int i = 0; i < positions.Count; ++i)
            {
                positions[i] = positions[i] + halfRadii;
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

            //
            // Upload height map as a one channel floating point texture
            //
            WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                sizeof(float) * tile.Heights.Length);
            pixelBuffer.CopyFromSystemMemory(tile.Heights);

            _texture = Device.CreateTexture2DRectangle(new Texture2DDescription(
                tile.Size.Width, tile.Size.Height, TextureFormat.Red32f));
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.Red, ImageDataType.Float);
            _texture.Filter = Texture2DFilter.NearestClampToEdge;
        }

        public void Render(SceneState sceneState)
        {
            _context.TextureUnits[0].Texture2DRectangle = _texture;
            _context.Bind(_sp);
            _context.Bind(_va);
            _context.Bind(_renderState);
            _context.Draw(_primitiveType, sceneState);
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
        private readonly VertexArray _va;
        private readonly Texture2D _texture;
        private readonly PrimitiveType _primitiveType;
        private readonly RenderState _renderState;
    }
}