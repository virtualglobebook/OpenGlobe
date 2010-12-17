#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

in vec4 gsTextureCoordinates[];
in vec4 gsColor[];
in float gsOrigin[];
in vec2 gsPixelOffset[];

out vec2 fsTextureCoordinates;
out vec4 fsColor;

uniform mat4 og_viewportOrthographicMatrix;
uniform sampler2D og_texture0;
uniform float og_highResolutionSnapScale;
uniform vec4 og_viewport;

void main()
{
    float originScales[3] = float[](0.0, 1.0, -1.0);

    vec4 textureCoordinate = gsTextureCoordinates[0];
    vec2 atlasSize = vec2(textureSize(og_texture0, 0));
    vec2 subRectangleSize = vec2(
        atlasSize.x * (textureCoordinate.p - textureCoordinate.s), 
        atlasSize.y * (textureCoordinate.q - textureCoordinate.t));
    vec2 halfSize = subRectangleSize * 0.5 * og_highResolutionSnapScale;

    vec4 center = gl_in[0].gl_Position;
    int horizontalOrigin = int(gsOrigin[0]) & 3;         // bits 0-1
    int verticalOrigin = (int(gsOrigin[0]) & 12) >> 2;   // bits 2-3
    center.xy += (vec2(originScales[horizontalOrigin], originScales[verticalOrigin]) * halfSize);

    center.xy += (gsPixelOffset[0] * og_highResolutionSnapScale);

    vec4 v0 = vec4(center.xy - halfSize, -center.z, 1.0);
    vec4 v1 = vec4(center.xy + vec2(halfSize.x, -halfSize.y), -center.z, 1.0);
    vec4 v2 = vec4(center.xy + vec2(-halfSize.x, halfSize.y), -center.z, 1.0);
    vec4 v3 = vec4(center.xy + halfSize, -center.z, 1.0);

    //
    // Cull - could also cull in z.
    //
    if ((v3.x < og_viewport.x) || (v3.y < og_viewport.y) ||
        (v0.x > og_viewport.z) || (v0.y > og_viewport.w))
    {
        return;
    }

    gl_Position = og_viewportOrthographicMatrix * v0;
    fsTextureCoordinates = textureCoordinate.st;
    fsColor = gsColor[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v1;
    fsTextureCoordinates = textureCoordinate.pt;
    fsColor = gsColor[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v2;
    fsTextureCoordinates = textureCoordinate.sq;
    fsColor = gsColor[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicMatrix * v3;
    fsTextureCoordinates = textureCoordinate.pq;
    fsColor = gsColor[0];
    EmitVertex();
}