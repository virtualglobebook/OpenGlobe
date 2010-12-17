#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

out vec3 fragmentColor;
uniform vec3 u_color;

void main()
{
    fragmentColor = u_color;
}