#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
              
in vec2 fsTextureCoordinates;

out float heightOutput;
out vec3 normalOutput;

uniform float u_heightExaggeration;
uniform float u_postDelta;
uniform sampler2DRect og_texture0;

void main()
{
    heightOutput = texture(og_texture0, fsTextureCoordinates).r;

    float top = texture(og_texture0, fsTextureCoordinates + vec2(0.0, 1.0)).r * u_heightExaggeration;
    float bottom = texture(og_texture0, fsTextureCoordinates + vec2(0.0, -1.0)).r * u_heightExaggeration;
    float left = texture(og_texture0, fsTextureCoordinates + vec2(-1.0, 0.0)).r * u_heightExaggeration;
    float right = texture(og_texture0, fsTextureCoordinates + vec2(1.0, 0.0)).r * u_heightExaggeration;

    normalOutput = vec3(left - right, bottom - top, 2.0 * u_postDelta);
}