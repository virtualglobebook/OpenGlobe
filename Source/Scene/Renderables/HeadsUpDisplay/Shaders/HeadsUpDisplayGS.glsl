#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

out vec2 fsTextureCoordinates;

uniform vec2 u_originScale;

void main()
{
    vec2 halfSize = vec2(textureSize(og_texture0, 0)) * 0.5 * og_highResolutionSnapScale;
    vec4 center = gl_in[0].gl_Position;
    center.xy += (u_originScale * halfSize);

    vec4 v0 = vec4(center.xy - halfSize, center.z, 1.0);
    vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), center.z, 1.0);
    vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), center.z, 1.0);
    vec4 v3 = vec4(center.xy + halfSize, center.z, 1.0);

    gl_Position = og_viewportOrthographicMatrix * v0;
    fsTextureCoordinates = vec2(0.0, 0.0);
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v1;
    fsTextureCoordinates = vec2(1.0, 0.0);
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v2;
    fsTextureCoordinates = vec2(0.0, 1.0);
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v3;
    fsTextureCoordinates = vec2(1.0, 1.0);
    EmitVertex();
}