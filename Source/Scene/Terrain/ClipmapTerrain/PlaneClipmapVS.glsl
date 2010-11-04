#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;

out vec3 normalFS;
out vec3 positionToLightFS;
out vec3 positionToEyeFS;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_sunPosition;
uniform vec2 u_patchOriginInClippedLevel;
uniform vec2 u_levelScaleFactor;
uniform vec2 u_levelZeroWorldScaleFactor;
uniform vec2 u_viewerOffsetFromOrigin;
uniform vec2 u_fineLevelOriginInCoarse;
uniform vec2 u_viewPosInClippedLevel;
uniform vec2 u_unblendedRegionSize;
uniform vec2 u_oneOverBlendedRegionSize;
uniform float u_heightExaggeration;
uniform sampler2DRect og_texture0;    // finer height map
uniform sampler2DRect og_texture1;    // coarser height map

float SampleHeight(vec2 clippedLevelCurrent)
{
	vec2 uvFine = clippedLevelCurrent + vec2(0.5, 0.5);
	vec2 uvCoarse = clippedLevelCurrent * 0.5 + u_fineLevelOriginInCoarse + vec2(0.5, 0.5);

	vec2 alpha = clamp((abs(clippedLevelCurrent - u_viewPosInClippedLevel) - u_unblendedRegionSize) * u_oneOverBlendedRegionSize, 0, 1);
	float alphaScalar = max(alpha.x, alpha.y);

	float fineHeight = texture(og_texture0, uvFine).r;
	float coarseHeight = texture(og_texture1, uvCoarse).r;
	return mix(fineHeight, coarseHeight, alphaScalar);
}

vec3 ComputeNormal(vec2 levelPos, out float height)
{
	// Compute a normal by forward differencing.
	vec2 right = levelPos + vec2(1.0, 0.0);
	vec2 top = levelPos + vec2(0.0, 1.0);

	height = SampleHeight(levelPos) * u_heightExaggeration;
	float rightHeight = SampleHeight(right) * u_heightExaggeration;
	float topHeight = SampleHeight(top) * u_heightExaggeration;

	vec2 gridDeltaInWorld = u_levelScaleFactor * u_levelZeroWorldScaleFactor;
	return cross(vec3(gridDeltaInWorld.x, 0.0, rightHeight - height), vec3(0.0, gridDeltaInWorld.y, topHeight - height));
}

void main()
{
	vec2 levelPos = position + u_patchOriginInClippedLevel;

	float height;
	normalFS = ComputeNormal(levelPos, height);

	vec2 worldPos = levelPos * u_levelScaleFactor * u_levelZeroWorldScaleFactor - u_viewerOffsetFromOrigin;
	vec3 displacedPosition = vec3(worldPos, height);

    positionToLightFS = og_sunPosition - displacedPosition;

    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
}
