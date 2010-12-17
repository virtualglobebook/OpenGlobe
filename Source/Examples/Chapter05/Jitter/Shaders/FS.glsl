#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
in vec3 fsColor;
out vec3 fragmentColor;

void main()
{
	fragmentColor = fsColor;
}