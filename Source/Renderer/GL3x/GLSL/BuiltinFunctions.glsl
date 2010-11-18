//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

//
// Emulates double precision subtraction.
// Returns left - right.
//
vec3 ogEmulatedDoubleSubtract(
	vec3 leftHigh,  vec3 leftLow,
	vec3 rightHigh, vec3 rightLow)
{
    vec3 t1 = leftLow - rightLow;
    vec3 e = t1 - leftLow;
    vec3 t2 = ((-rightLow - e) + (leftLow - (t1 - e))) + leftHigh - rightHigh;
    vec3 highDifference = t1 + t2;
    vec3 lowDifference = t2 - (highDifference - t1);

    return highDifference + lowDifference;
}

//
// Typical inputs:
//
//   in vec3 positionHigh;
//   in vec3 positionLow;
//   uniform vec3 og_cameraEyeHigh;
//   uniform vec3 og_cameraEyeLow;
//   uniform mat4 og_modelViewPerspectiveMatrixRelativeToEye;
//
vec4 ogTransformEmulatedDoublePosition(
	vec3 positionHigh,  vec3 positionLow, 
	vec3 cameraEyeHigh, vec3 cameraEyeLow,
	mat4 modelViewPerspectiveMatrixRelativeToEye)
{
    vec3 positionEye = ogEmulatedDoubleSubtract(positionHigh, positionLow, cameraEyeHigh, cameraEyeLow);
    return modelViewPerspectiveMatrixRelativeToEye * vec4(positionEye, 1.0);
}

vec4 ogTransformEmulatedDoublePosition(
	vec3 positionHigh,  vec3 positionLow, 
	vec3 cameraEyeHigh, vec3 cameraEyeLow,
	mat4 modelViewPerspectiveMatrixRelativeToEye,
	out vec3 positionInModelCoordinates)
{
    vec3 positionEye = ogEmulatedDoubleSubtract(positionHigh, positionLow, cameraEyeHigh, cameraEyeLow);

    positionInModelCoordinates = cameraEyeHigh + positionEye;
    return modelViewPerspectiveMatrixRelativeToEye * vec4(positionEye, 1.0);
}