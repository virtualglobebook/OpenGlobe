#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
             
out vec4 fragmentColor;
uniform vec3 u_color;
uniform float u_alpha;

void main()
{
    fragmentColor = vec4(u_color, u_alpha);
}