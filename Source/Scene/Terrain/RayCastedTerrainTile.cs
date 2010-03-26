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
                  in vec2 textureCoordinate;

                  out vec3 boxExit;
                  out vec2 fsTextureCoordinate;

                  uniform mat4 mg_modelViewPerspectiveProjectionMatrix;

                  void main()
                  {
                      gl_Position = mg_modelViewPerspectiveProjectionMatrix * position;
                      boxExit = position.xyz;
                      fsTextureCoordinate = textureCoordinate;
                  }";
            string fs =
                @"#version 150
                 
                  in vec3 boxExit;
                  in vec2 fsTextureCoordinate;

                  out vec3 fragmentColor;

                  uniform sampler2DRect mg_texture0;    // Height field
                  uniform vec3 mg_cameraEye;

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

                  void swap(inout float left, inout float right)
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
                      if (direction == 0)
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
                              swap(t1, t2);
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
                          if (tFar < 0)
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

                  void main()
                  {
                      vec3 rayDirection = boxExit - mg_cameraEye;

                      vec3 boxEntry;
                      if (PointInsideAxisAlignedBoundingBox(mg_cameraEye, u_aabbLowerLeft, u_aabbUpperRight))
                      {
                          boxEntry = mg_cameraEye;
                      }
                      else
                      {
                          Intersection i = RayIntersectsAABB(mg_cameraEye, rayDirection, u_aabbLowerLeft, u_aabbUpperRight);
                          boxEntry = i.IntersectionPoint;
                      }

                      fragmentColor = boxEntry + vec3(0.5);
                      //fragmentColor = vec3(texture(mg_texture0, fsTextureCoordinate).r / 15.0, 0.0, 0.0);
                  }";
            _sp = Device.CreateShaderProgram(vs, fs);

            Vector3D radii = new Vector3D(
                tile.Extent.East - tile.Extent.West,
                tile.Extent.North - tile.Extent.South,
                tile.MaximumHeight - tile.MinimumHeight);
            Vector3D halfRadii = 0.5 * radii;

            (_sp.Uniforms["u_aabbLowerLeft"] as Uniform<Vector3S>).Value = (-halfRadii).ToVector3S();
            (_sp.Uniforms["u_aabbUpperRight"] as Uniform<Vector3S>).Value = halfRadii.ToVector3S();

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = BoxTessellator.Compute(radii);

            VertexAttributeFloatVector2 textureCoordinatesAttribute = new VertexAttributeFloatVector2("textureCoordinate", 8);
            mesh.Attributes.Add(textureCoordinatesAttribute);
            IList<Vector2S> textureCoordinates = textureCoordinatesAttribute.Values;
            for (int i = 0; i < 2; ++i)
            {
                textureCoordinates.Add(new Vector2S(0, 0));
                textureCoordinates.Add(new Vector2S(tile.Size.Width, 0));
                textureCoordinates.Add(new Vector2S(tile.Size.Width, tile.Size.Height));
                textureCoordinates.Add(new Vector2S(0, tile.Size.Height));
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            //_renderState.RasterizationMode = RasterizationMode.Line;

            ///////////////////////////////////////////////////////////////////

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