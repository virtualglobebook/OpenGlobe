#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
                 
in float fsDistanceToEye;
out vec4 fragmentColor;
uniform vec3 u_color;

void main()
{
    //
    // Apply linear attenuation to alpha
    //
    float a = min(1.0 / (0.015 * fsDistanceToEye), 1.0);
    if (a == 0.0)
    {
        discard;
    }

    fragmentColor = vec4(u_color, a);
}