#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

flat in vec4 fsColor;
out vec4 fragmentColor;

void main()
{
    fragmentColor = fsColor;
}