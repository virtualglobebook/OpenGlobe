#version 150 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

out vec2 fsTextureCoordinates;

uniform mat4 mg_viewportOrthographicProjectionMatrix;
uniform sampler2D mg_texture0;
uniform float mg_highResolutionSnapScale;
uniform vec2 u_originScale;

void main()
{
    vec2 halfSize = vec2(textureSize(mg_texture0, 0)) * 0.5 * mg_highResolutionSnapScale;
    vec4 center = gl_in[0].gl_Position;
    center.xy += (u_originScale * halfSize);

    vec4 v0 = vec4(center.xy - halfSize, center.z, 1.0);
    vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), center.z, 1.0);
    vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), center.z, 1.0);
    vec4 v3 = vec4(center.xy + halfSize, center.z, 1.0);

    gl_Position = mg_viewportOrthographicProjectionMatrix * v0;
    fsTextureCoordinates = vec2(0.0, 0.0);
    EmitVertex();

    gl_Position = mg_viewportOrthographicProjectionMatrix * v1;
    fsTextureCoordinates = vec2(1.0, 0.0);
    EmitVertex();

    gl_Position = mg_viewportOrthographicProjectionMatrix * v2;
    fsTextureCoordinates = vec2(0.0, 1.0);
    EmitVertex();

    gl_Position = mg_viewportOrthographicProjectionMatrix * v3;
    fsTextureCoordinates = vec2(1.0, 1.0);
    EmitVertex();
}