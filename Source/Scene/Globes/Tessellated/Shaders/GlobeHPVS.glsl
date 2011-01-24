#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
in vec3 positionHigh;
in vec3 positionLow;
out vec3 worldPosition;
out vec3 positionToLight;
out vec3 positionToEye;

uniform bool u_logarithmicDepth;
uniform float u_logarithmicDepthConstant;

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
	vec3 positionModel;
	vec4 clipPosition = ogTransformEmulatedDoublePosition(
		positionHigh, positionLow, og_cameraEyeHigh, og_cameraEyeLow,
		og_modelViewPerspectiveMatrixRelativeToEye, positionModel);
	gl_Position = applyLogarithmicDepth(clipPosition, u_logarithmicDepth, u_logarithmicDepthConstant, og_perspectiveFarPlaneDistance);

    worldPosition = positionModel.xyz;
    positionToLight = og_cameraLightPosition - positionModel;
    positionToEye = og_cameraEye - positionModel + og_cameraEyeHigh.z;
}