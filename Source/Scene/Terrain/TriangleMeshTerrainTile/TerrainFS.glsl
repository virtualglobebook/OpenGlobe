#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
            
in float height;

out vec3 fragmentColor;

uniform float u_minimumHeight;
uniform float u_maximumHeight;

void main()
{
    fragmentColor = vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
}