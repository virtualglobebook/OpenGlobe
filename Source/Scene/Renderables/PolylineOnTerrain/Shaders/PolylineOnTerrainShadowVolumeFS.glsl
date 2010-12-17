#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

uniform sampler2D og_texture0;
uniform vec2 og_inverseViewportDimensions;
out vec4 fragmentColor;

void main()
{
    vec2 of = og_inverseViewportDimensions * gl_FragCoord.xy;
    if (texture(og_texture0, of).r != 0.0)
    {
        fragmentColor = vec4(1.0, 1.0, 0.0, 1.0);
    }
}