//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in vec2 gsFineUv[];
in vec2 gsCoarseUv[];
in vec3 gsPositionToLight[];
in float gsAlpha[];

in vec2 gsWindowPosition[];
in float gsDistanceToEye[];

out vec2 fsFineUv;
out vec2 fsCoarseUv;
out vec3 fsPositionToLight;
out float fsAlpha;

noperspective out vec3 fsDistanceToEdges;
out float fsDistanceToEye;

void main()
{
    vec2 p0 = gsWindowPosition[0];
    vec2 p1 = gsWindowPosition[1];
    vec2 p2 = gsWindowPosition[2];

    gl_Position = gl_in[0].gl_Position;
	fsFineUv = gsFineUv[0];
	fsCoarseUv = gsCoarseUv[0];
	fsPositionToLight = gsPositionToLight[0];
	fsAlpha = gsAlpha[0];
    fsDistanceToEdges = vec3(og_distanceToLine(p0, p1, p2), 0.0, 0.0);
    fsDistanceToEye = gsDistanceToEye[0];
    EmitVertex();

    gl_Position = gl_in[1].gl_Position;
	fsFineUv = gsFineUv[1];
	fsCoarseUv = gsCoarseUv[1];
	fsPositionToLight = gsPositionToLight[1];
	fsAlpha = gsAlpha[1];
    fsDistanceToEdges = vec3(0.0, og_distanceToLine(p1, p2, p0), 0.0);
    fsDistanceToEye = gsDistanceToEye[1];
	EmitVertex();

    gl_Position = gl_in[2].gl_Position;
	fsFineUv = gsFineUv[2];
	fsCoarseUv = gsCoarseUv[2];
	fsPositionToLight = gsPositionToLight[2];
	fsAlpha = gsAlpha[2];
    fsDistanceToEdges = vec3(0.0, 0.0, og_distanceToLine(p2, p0, p1));
    fsDistanceToEye = gsDistanceToEye[2];
    EmitVertex();
}



