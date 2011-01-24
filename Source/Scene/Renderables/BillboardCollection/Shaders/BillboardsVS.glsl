#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;
layout(location = og_textureCoordinateVertexLocation) in vec4 textureCoordinates;
layout(location = og_colorVertexLocation) in vec4 color;
in float origin;                  // TODO:  Why does this not work when float is int?
in vec2 pixelOffset;

out vec4 gsTextureCoordinates;
out vec4 gsColor;
out float gsOrigin;
out vec2 gsPixelOffset;

vec4 ModelToWindowCoordinates(
    vec4 v, 
    mat4 modelViewPerspectiveMatrix, 
    mat4 viewportTransformationMatrix)
{
    v = modelViewPerspectiveMatrix * v;                  // clip coordinates
    v.xyz /= v.w;                                                  // normalized device coordinates
    v.xyz = (viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz; // window coordinates
    return v;
}

void main()                     
{
    gl_Position = ModelToWindowCoordinates(position, 
        og_modelViewPerspectiveMatrix, og_viewportTransformationMatrix);
    gsTextureCoordinates = textureCoordinates;
    gsColor = color;
    gsOrigin = origin;
    gsPixelOffset = pixelOffset;
}