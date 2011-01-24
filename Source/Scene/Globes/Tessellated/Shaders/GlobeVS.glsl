#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(location = og_positionVertexLocation) in vec4 position;
out vec3 worldPosition;
out vec3 positionToLight;
out vec3 positionToEye;

uniform bool u_logarithmicDepth;
uniform float u_logarithmicDepthConstant;

vec4 ModelToClipCoordinates(
    vec4 position,
    mat4 modelViewPerspectiveMatrix,
    bool logarithmicDepth,
    float logarithmicDepthConstant,
    float perspectiveFarPlaneDistance)
{
    vec4 clip = modelViewPerspectiveMatrix * position; 

    if (logarithmicDepth)
    {
        clip.z = ((2.0 * log((logarithmicDepthConstant * clip.z) + 1.0) / 
                   log((logarithmicDepthConstant * perspectiveFarPlaneDistance) + 1.0)) - 1.0) * clip.w;
    }

    return clip;
}

void main()                     
{
    gl_Position = ModelToClipCoordinates(position, og_modelViewPerspectiveMatrix,
        u_logarithmicDepth, u_logarithmicDepthConstant, og_perspectiveFarPlaneDistance);

    worldPosition = position.xyz;
    positionToLight = og_cameraLightPosition - worldPosition;
    positionToEye = og_cameraEye - worldPosition;
}