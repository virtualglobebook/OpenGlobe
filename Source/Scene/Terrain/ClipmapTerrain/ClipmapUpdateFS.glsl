#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;

out float heightOutput;

uniform sampler2DRect u_texture;

void main()
{
    heightOutput = texture(u_texture, fsTextureCoordinates).r;
}