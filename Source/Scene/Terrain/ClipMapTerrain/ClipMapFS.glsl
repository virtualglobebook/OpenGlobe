#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in float height;
                 
out vec3 fragmentColor;

void main()
{
    fragmentColor = vec3(0.0, clamp(height / 4000.0, 0.0, 1.0), clamp(-height / 5000, 0.0, 1.0));
}
