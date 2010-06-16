#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

namespace OpenGlobe.Renderer
{
    public static class ShaderSources
    {
        public static string PassThroughVertexShader()
        {
            return
                @"#version 150

                  in vec4 position;               

                  void main()                     
                  {
                      gl_Position = position; 
                  }";
        }

        public static string PassThroughGeometryShader()
        {
            //
            // The first assignment to gl_Position is necessary to eliminate warning C7050.
            //
            // See http://www.opengl.org/discussion_boards/ubbthreads.php?ubb=showflat&Number=267989#Post267989
            //
            return
                @"#version 150 

                  layout(triangles) in;
                  layout(triangle_strip, max_vertices = 3) out;

                  void main()
                  {
                      gl_Position = vec4(0);

                      for(int i = 0; i < gl_in.length(); ++i)
                      {
                          gl_Position = gl_in[i].gl_Position;
                          EmitVertex();
                      }
                      EndPrimitive();
                  }";
        }

        public static string PassThroughFragmentShader()
        {
            return
                @"#version 150
                 
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(1, 0, 0, 1);
                  }";
        }

        public static string RedUniformBlockFragmentShader()
        {
            return
                @"#version 150
                 
                  layout(std140) uniform RedBlock
                  {
                      uniform float red;
                  };

                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(red, 0, 0, 1);
                  }";
        }

        public static string MultitextureFragmentShader()
        {
            return
                @"#version 150
                 
                  uniform sampler2D og_texture0;
                  uniform sampler2D og_texture1;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(
                          texture(og_texture0, vec2(0, 0)).r,
                          texture(og_texture1, vec2(0, 0)).g, 0, 1);
                  }";
        }
    }
}
