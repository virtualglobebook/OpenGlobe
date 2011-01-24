#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;
in vec2 fsPosition;

out vec3 normalOutput;

uniform float u_heightExaggeration;
uniform float u_postDelta;
uniform vec2 u_oneOverHeightMapSize;

void main()
{
    float top = texture(og_texture0, fsTextureCoordinates + vec2(0.0, u_oneOverHeightMapSize.y)).r * u_heightExaggeration;
    float bottom = texture(og_texture0, fsTextureCoordinates + vec2(0.0, -u_oneOverHeightMapSize.y)).r * u_heightExaggeration;
    float left = texture(og_texture0, fsTextureCoordinates + vec2(-u_oneOverHeightMapSize.x, 0.0)).r * u_heightExaggeration;
    float right = texture(og_texture0, fsTextureCoordinates + vec2(u_oneOverHeightMapSize.x, 0.0)).r * u_heightExaggeration;

    normalOutput = vec3(left - right, bottom - top, 2.0 * u_postDelta);
}