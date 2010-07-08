#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 18) out;

uniform mat4 og_perspectiveProjectionMatrix;
uniform float og_pixelSizePerDistance;

void main()
{
    // normal
    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 cr = cross(v1, v0);
    cr = normalize(cr);

    if (abs(cr.z) <= 0.017452406437283512819418978516316)
    {
        // vertex
        vec3 norm = cr * (0.5 * og_pixelSizePerDistance);
        vec4 vertices[8];

        vec3 n = norm * abs(gl_in[1].gl_Position.z);
        vertices[0] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz - n, 1.0);
        vertices[1] = og_perspectiveProjectionMatrix * vec4(gl_in[1].gl_Position.xyz + n, 1.0);
        n = norm * abs(gl_in[0].gl_Position.z);
        vertices[2] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz + n, 1.0);
        vertices[3] = og_perspectiveProjectionMatrix * vec4(gl_in[0].gl_Position.xyz - n, 1.0);
        n = norm * abs(gl_in[2].gl_Position.z);
        vertices[4] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz - n, 1.0);
        vertices[5] = og_perspectiveProjectionMatrix * vec4(gl_in[2].gl_Position.xyz + n, 1.0);
        n = norm * abs(gl_in[3].gl_Position.z);
        vertices[6] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz + n, 1.0);
        vertices[7] = og_perspectiveProjectionMatrix * vec4(gl_in[3].gl_Position.xyz - n, 1.0);
                    
        gl_Position = vertices[0];
        EmitVertex();
        gl_Position = vertices[1];
        EmitVertex();
        gl_Position = vertices[3];
        EmitVertex();
        gl_Position = vertices[2];
        EmitVertex();
        gl_Position = vertices[7];
        EmitVertex();
        gl_Position = vertices[6];
        EmitVertex();
        gl_Position = vertices[4];
        EmitVertex();
        gl_Position = vertices[5];
        EmitVertex();
        gl_Position = vertices[0];
        EmitVertex();
        gl_Position = vertices[1];
        EmitVertex();
        EndPrimitive();

        gl_Position = vertices[2];
        EmitVertex();
        gl_Position = vertices[1];
        EmitVertex();
        gl_Position = vertices[6];
        EmitVertex();
        gl_Position = vertices[5];
        EmitVertex();
        EndPrimitive();

        gl_Position = vertices[0];
        EmitVertex();
        gl_Position = vertices[3];
        EmitVertex();
        gl_Position = vertices[4];
        EmitVertex();
        gl_Position = vertices[7];
        EmitVertex();
        EndPrimitive();
    }
}