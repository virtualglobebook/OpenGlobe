#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
              
in vec2 fsTextureCoordinates;

out vec4 fragmentColor;

uniform sampler2D og_texture0;

void main()
{
    fragmentColor = texture(og_texture0, fsTextureCoordinates);
}