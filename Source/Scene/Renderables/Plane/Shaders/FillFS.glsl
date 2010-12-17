#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
             
out vec4 fragmentColor;
uniform vec3 u_color;
uniform float u_alpha;

void main()
{
    fragmentColor = vec4(u_color, u_alpha);
}