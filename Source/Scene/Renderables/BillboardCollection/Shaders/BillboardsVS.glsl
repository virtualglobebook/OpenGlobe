#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec4 position;
in vec4 textureCoordinates;
in vec4 color;
in float origin;                  // TODO:  Why does this not work when float is int?
in vec2 pixelOffset;

out vec4 gsTextureCoordinates;
out vec4 gsColor;
out float gsOrigin;
out vec2 gsPixelOffset;

uniform mat4 mg_modelViewPerspectiveProjectionMatrix;
uniform mat4 mg_viewportTransformationMatrix;

vec4 WorldToWindowCoordinates(
    vec4 v, 
    mat4 modelViewPerspectiveProjectionMatrix, 
    mat4 viewportTransformationMatrix)
{
    v = modelViewPerspectiveProjectionMatrix * v;                        // clip coordinates
    v.xyz /= v.w;                                                           // normalized device coordinates
    v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
    return v;
}

void main()                     
{
    gl_Position = WorldToWindowCoordinates(position, 
        mg_modelViewPerspectiveProjectionMatrix, mg_viewportTransformationMatrix);
    gsTextureCoordinates = textureCoordinates;
    gsColor = color;
    gsOrigin = origin;
    gsPixelOffset = pixelOffset;
}