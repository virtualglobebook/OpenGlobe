#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 4) out;

uniform mat4 og_perspectiveProjectionMatrix;

void main()
{
    // normal
    vec3 v0 = gl_in[0].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 v1 = gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz;
    vec3 cr = cross(v1, v0);
    cr = normalize(cr);

    if (abs(cr.z) > 0.017452406437283512819418978516316)
    {
        gl_Position = og_perspectiveProjectionMatrix * gl_in[1].gl_Position;
        EmitVertex();
        gl_Position = og_perspectiveProjectionMatrix * gl_in[0].gl_Position;
        EmitVertex();
        gl_Position = og_perspectiveProjectionMatrix * gl_in[2].gl_Position;
        EmitVertex();
        gl_Position = og_perspectiveProjectionMatrix * gl_in[3].gl_Position;
        EmitVertex();
        EndPrimitive();
    }
}