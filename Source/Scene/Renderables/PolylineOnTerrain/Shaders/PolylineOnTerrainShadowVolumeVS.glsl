#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
   
uniform mat4 og_modelViewMatrix;
in vec4 position;

void main()                     
{
	gl_Position = og_modelViewMatrix * position;
}