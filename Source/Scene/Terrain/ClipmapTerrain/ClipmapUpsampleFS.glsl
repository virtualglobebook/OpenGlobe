#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;

out vec3 heightOutput;

uniform sampler2D og_texture0; // coarse height map

void main()
{
    heightOutput = texture(og_texture0, fsTextureCoordinates).rgb;
}