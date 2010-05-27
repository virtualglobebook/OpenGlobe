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
using MiniGlobe.Renderer;

namespace MiniGlobe.Scene
{
    public sealed class OutlinedPolylineTexture : IDisposable
    {
        public OutlinedPolylineTexture(Context context)
        {
            _context = context;
            _renderState = new RenderState();
            _renderState.FacetCulling.Enabled = false;
            _renderState.Blending.Enabled = true;
            _renderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _renderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _renderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            
            Width = 3;
            OutlineWidth = 2;
        }

        public void Set(Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (mesh.PrimitiveType != PrimitiveType.Lines &&
                mesh.PrimitiveType != PrimitiveType.LineLoop &&
                mesh.PrimitiveType != PrimitiveType.LineStrip)
            {
                throw new ArgumentException("mesh.PrimitiveType must be Lines, LineLoop, or LineStrip.", "mesh");
            }

            if (!mesh.Attributes.Contains("position") &&
                !mesh.Attributes.Contains("color") &&
                !mesh.Attributes.Contains("outlineColor"))
            {
                throw new ArgumentException("mesh.Attributes should contain attributes named \"position\", \"color\", and \"outlineColor\".", "mesh");
            }

            if (_sp == null)
            {
                string vs =
                    @"#version 150

                      in vec4 position;
                      in vec4 color;
                      in vec4 outlineColor;

                      out vec4 gsColor;
                      out vec4 gsOutlineColor;

                      void main()                     
                      {
                        gl_Position = position;
                        gsColor = color;
                        gsOutlineColor = outlineColor;
                      }";
                string gs =
                    @"#version 150 

                    layout(lines) in;
                    layout(triangle_strip, max_vertices = 4) out;

                    in vec4 gsColor[];
                    in vec4 gsOutlineColor[];

                    flat out vec4 fsColor;
                    flat out vec4 fsOutlineColor;
                    out vec2 fsTextureCoordinate;

                    uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
                    uniform mat4 mg_viewportTransformationMatrix;
                    uniform mat4 mg_viewportOrthographicProjectionMatrix;
                    uniform float mg_perspectiveNearPlaneDistance;
                    uniform float u_distance;

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

                    void main()
                    {
                      vec4 clipP0;
                      vec4 clipP1;
                      ClipLineSegmentToNearPlane(mg_perspectiveNearPlaneDistance, 
                        mg_modelViewPerspectiveProjectionMatrix,
                        gl_in[0].gl_Position, gl_in[1].gl_Position, clipP0, clipP1);

                      vec4 windowP0 = ClipToWindowCoordinates(clipP0, mg_viewportTransformationMatrix);
                      vec4 windowP1 = ClipToWindowCoordinates(clipP1, mg_viewportTransformationMatrix);

                      vec2 direction = windowP1.xy - windowP0.xy;
                      vec2 normal = normalize(vec2(direction.y, -direction.x));

                      vec4 v0 = vec4(windowP0.xy - (normal * u_distance), windowP0.z, 1.0);
                      vec4 v1 = vec4(windowP1.xy - (normal * u_distance), windowP1.z, 1.0);
                      vec4 v2 = vec4(windowP0.xy + (normal * u_distance), windowP0.z, 1.0);
                      vec4 v3 = vec4(windowP1.xy + (normal * u_distance), windowP1.z, 1.0);

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
                      fsColor = gsColor[0];
                      fsOutlineColor = gsOutlineColor[0];
                      fsTextureCoordinate = vec2(0.0, 0.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
                      fsColor = gsColor[0];
                      fsOutlineColor = gsOutlineColor[0];
                      fsTextureCoordinate = vec2(0.0, 1.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
                      fsColor = gsColor[0];
                      fsOutlineColor = gsOutlineColor[0];
                      fsTextureCoordinate = vec2(1.0, 0.0);
                      EmitVertex();

                      gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
                      fsColor = gsColor[0];
                      fsOutlineColor = gsOutlineColor[0];
                      fsTextureCoordinate = vec2(1.0, 1.0);
                      EmitVertex();
                    }";
                string fs =
                    @"#version 150

                    flat in vec4 fsColor;
                    flat in vec4 fsOutlineColor;
                    in vec2 fsTextureCoordinate;

                    out vec4 fragmentColor;

                    uniform sampler2D mg_texture0;

                    void main()
                    {
                      vec2 texel = texture(mg_texture0, fsTextureCoordinate).rg;
                      float fill = texel.r;
                      float alpha = texel.g;

                      vec4 color = mix(fsOutlineColor, fsColor, fill);
                      fragmentColor = vec4(color.rgb, color.a * alpha);
                    }";
                _sp = Device.CreateShaderProgram(vs, gs, fs);
                _distance = _sp.Uniforms["u_distance"] as Uniform<float>;
            }

            ///////////////////////////////////////////////////////////////////
            _va = _context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        private void Update(SceneState sceneState)
        {
            int width = (int)Math.Ceiling(Width * sceneState.HighResolutionSnapScale);
            int outlineWidth = (int)Math.Ceiling(OutlineWidth * sceneState.HighResolutionSnapScale);

            int textureWidth = width + outlineWidth + outlineWidth + 2;

            if ((_texture == null) || (_texture.Description.Width != textureWidth))
            {
                int textureSize = textureWidth * 2;

                float[] texels = new float[textureSize];

                int k = 3;
                for (int i = 1; i < textureWidth - 1; ++i)
                {
                    texels[k] = 1;                  // Alpha (stored in Green channel)
                    k += 2;
                }

                int j = (outlineWidth + 1) * 2;
                for (int i = 0; i < width; ++i)
                {
                    texels[j] = 1;                  // Fill/Outline (stored in Red channel)
                    j += 2;
                }

                WritePixelBuffer pixelBuffer = Device.CreateWritePixelBuffer(WritePixelBufferHint.StreamDraw,
                    sizeof(float) * textureSize);
                pixelBuffer.CopyFromSystemMemory(texels);

                if (_texture != null)
                {
                    _texture.Dispose();
                }
                _texture = Device.CreateTexture2D(new Texture2DDescription(textureWidth, 1, TextureFormat.RedGreen8));
                _texture.CopyFromBuffer(pixelBuffer, ImageFormat.RedGreen, ImageDataType.Float);
                _texture.Filter = Texture2DFilter.LinearClampToEdge;
            }
        }

        public void Render(SceneState sceneState)
        {
            if (_sp != null)
            {
                Update(sceneState);

                _distance.Value = (float)(((Width * 0.5) + OutlineWidth + 1) * sceneState.HighResolutionSnapScale);

                _context.TextureUnits[0].Texture2D = _texture;
                _context.Bind(_renderState);
                _context.Bind(_sp);
                _context.Bind(_va);
                _context.Draw(_primitiveType, sceneState);
            }
        }

        public Context Context
        {
            get { return _context; }
        }

        public double Width { get; set; }

        public double OutlineWidth { get; set; }

        public bool Wireframe
        {
            get { return _renderState.RasterizationMode == RasterizationMode.Line; }
            set { _renderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool DepthWrite
        {
            get { return _renderState.DepthWrite; }
            set { _renderState.DepthWrite = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_sp != null)
            {
                _sp.Dispose();
            }

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_texture != null)
            {
                _texture.Dispose();
            }
        }

        #endregion

        private readonly Context _context;
        private readonly RenderState _renderState;
        private ShaderProgram _sp;
        private Uniform<float> _distance;
        private VertexArray _va;
        private PrimitiveType _primitiveType;
        private Texture2D _texture;
    }
}