#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
            
in float height;

out vec3 fragmentColor;

uniform float u_minimumHeight;
uniform float u_maximumHeight;

void main()
{
    fragmentColor = vec3((height - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
}