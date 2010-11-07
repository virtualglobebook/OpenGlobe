#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
in vec3 fsColor;
out vec3 fragmentColor;

void main()
{
	fragmentColor = fsColor;
}