#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
              
in vec2 fsTextureCoordinates;

out float heightOutput;

uniform sampler2D og_texture0; // coarse height map

void main()
{
    heightOutput = texture(og_texture0, fsTextureCoordinates).r;
}