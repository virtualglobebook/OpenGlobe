#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public static class ShaderSources
    {
        public static string PassThroughVertexShader()
        {
            return
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;               

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
                @"#version 330 

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
                @"#version 330
                 
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = vec4(1, 0, 0, 1);
                  }";
        }

        public static string RedUniformBlockFragmentShader()
        {
            return
                @"#version 330
                 
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
                @"#version 330
                 
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
