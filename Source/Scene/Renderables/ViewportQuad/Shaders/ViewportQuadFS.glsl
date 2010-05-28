#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
              
in vec2 fsTextureCoordinates;

out vec4 fragmentColor;

uniform sampler2D mg_texture0;

void main()
{
    fragmentColor = texture(mg_texture0, fsTextureCoordinates);
}