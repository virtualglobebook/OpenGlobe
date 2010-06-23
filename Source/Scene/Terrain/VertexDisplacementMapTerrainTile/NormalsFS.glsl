#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
                 
in float distanceToEyeFS;
out vec4 fragmentColor;
uniform vec3 u_color;

void main()
{
    //
    // Apply linear attenuation to alpha
    //
    float a = min(1.0 / (0.015 * distanceToEyeFS), 1.0);
    if (a == 0.0)
    {
        discard;
    }

    fragmentColor = vec4(u_color, a);
}