#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec4 position;
in vec4 color;
in vec4 outlineColor;

out vec4 gsColor;
out vec4 gsOutlineColor;

void main()                     
{
	gl_Position = position;
	gsColor = color;
	gsOutlineColor = outlineColor;
}