#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec2 windowPosition[];
in float gsDistanceToEye[];

noperspective out vec3 distanceToEdges;
out float fsDistanceToEye;

void main()
{
    vec2 p0 = windowPosition[0];
    vec2 p1 = windowPosition[1];
    vec2 p2 = windowPosition[2];

    gl_Position = gl_in[0].gl_Position;
    distanceToEdges = vec3(og_distanceToLine(p0, p1, p2), 0.0, 0.0);
    fsDistanceToEye = gsDistanceToEye[0];
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    distanceToEdges = vec3(0.0, og_distanceToLine(p1, p2, p0), 0.0);
    fsDistanceToEye = gsDistanceToEye[1];
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    distanceToEdges = vec3(0.0, 0.0, og_distanceToLine(p2, p0, p1));
    fsDistanceToEye = gsDistanceToEye[2];
    EmitVertex();
}