#version 150
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec4 position;
uniform mat4 og_modelViewPerspectiveProjectionMatrix;
uniform float og_perspectiveFarPlaneDistance;
uniform bool u_logarithmicDepth;
uniform float u_logarithmicDepthConstant;

vec4 ModelToClipCoordinates(
    vec4 position,
    mat4 modelViewPerspectiveProjectionMatrix,
    bool logarithmicDepth,
    float logarithmicDepthConstant,
    float perspectiveFarPlaneDistance)
{
    vec4 clip = modelViewPerspectiveProjectionMatrix * position; 

    if (logarithmicDepth)
    {
        clip.z = (log((logarithmicDepthConstant * clip.z) + 1.0) / 
                log((logarithmicDepthConstant * perspectiveFarPlaneDistance) + 1.0)) * clip.w;
    }

    return clip;
}

void main()                     
{
    gl_Position = ModelToClipCoordinates(position, og_modelViewPerspectiveProjectionMatrix,
        u_logarithmicDepth, u_logarithmicDepthConstant, og_perspectiveFarPlaneDistance);
}