#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
layout(location = og_positionHighVertexLocation) in vec3 positionHigh;
layout(location = og_positionLowVertexLocation) in vec3 positionLow;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform float og_perspectiveFarPlaneDistance;
uniform bool u_logarithmicDepth;
uniform float u_logarithmicDepthConstant;

uniform vec3 og_cameraEyeHigh;
uniform vec3 og_cameraEyeLow;
uniform mat4 og_modelViewPerspectiveMatrixRelativeToEye;

vec4 applyLogarithmicDepth(
    vec4 clipPosition,
    bool logarithmicDepth,
    float logarithmicDepthConstant,
    float perspectiveFarPlaneDistance)
{
    if (logarithmicDepth)
    {
        clipPosition.z = ((2.0 * log((logarithmicDepthConstant * clipPosition.z) + 1.0) / 
                   log((logarithmicDepthConstant * perspectiveFarPlaneDistance) + 1.0)) - 1.0) * clipPosition.w;
    }

    return clipPosition;
}

void main()                     
{
	vec4 clipPosition = ogTransformEmulatedDoublePosition(
		positionHigh, positionLow, og_cameraEyeHigh, og_cameraEyeLow,
		og_modelViewPerspectiveMatrixRelativeToEye);
	gl_Position = applyLogarithmicDepth(clipPosition, u_logarithmicDepth, u_logarithmicDepthConstant, og_perspectiveFarPlaneDistance);
}