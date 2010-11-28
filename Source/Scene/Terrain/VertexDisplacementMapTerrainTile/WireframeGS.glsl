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
in float distanceToEyeGS[];

noperspective out vec3 distanceToEdges;
out float fsDistanceToEye;

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
    fsDistanceToEye = distanceToEyeGS[0];
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
    distanceToEdges = vec3(0.0, distanceToLine(p1, p2, p0), 0.0);
    fsDistanceToEye = distanceToEyeGS[1];
    EmitVertex();

    gl_Position = gl_in[2].gl_Position;
    distanceToEdges = vec3(0.0, 0.0, distanceToLine(p2, p0, p1));
    fsDistanceToEye = distanceToEyeGS[2];
    EmitVertex();
}