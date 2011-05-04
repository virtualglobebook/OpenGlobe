#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;

out vec4 texelOutput;

uniform sampler2DRect og_texture0;

void main()
{
    texelOutput = texture(og_texture0, fsTextureCoordinates);
}