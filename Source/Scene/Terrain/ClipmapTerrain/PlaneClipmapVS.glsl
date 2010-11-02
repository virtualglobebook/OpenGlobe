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
out vec2 textureCoordinateFS;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_cameraEye;
uniform vec3 og_sunPosition;
uniform sampler2DRect og_texture0;    // Fine height map
uniform sampler2DRect og_texture1;    // Coarse height map
uniform vec2 u_blockOriginInClippedLevel;
uniform vec2 u_clippedLevelOrigin;
uniform vec2 u_levelScaleFactor;
uniform vec2 u_levelZeroWorldScaleFactor;
uniform vec2 u_levelZeroWorldOrigin;
uniform float u_heightExaggeration;
uniform vec2 u_fineLevelOriginInCoarse;
uniform vec2 u_viewPosInClippedLevel;
uniform vec2 u_unblendedRegionSize;
uniform vec2 u_oneOverBlendedRegionSize;

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

void main()
{
	vec2 clippedLevelCurrent = position + u_blockOriginInClippedLevel;

	// Compute a normal for the fragment shader by forward differencing
	vec2 right = clippedLevelCurrent + vec2(1.0, 0.0);
	vec2 top = clippedLevelCurrent + vec2(0.0, 1.0);

	float centerHeight = SampleHeight(clippedLevelCurrent) * u_heightExaggeration;
	float rightHeight = SampleHeight(right) * u_heightExaggeration;
	float topHeight = SampleHeight(top) * u_heightExaggeration;

	vec2 levelGridDeltaInWorld = u_levelScaleFactor * u_levelZeroWorldScaleFactor;
	normalFS = cross(vec3(levelGridDeltaInWorld.x, 0.0, rightHeight - centerHeight), vec3(0.0, levelGridDeltaInWorld.y, topHeight - centerHeight));

	// Compute the displaced position of this vertex in the world.
	vec2 levelPos = clippedLevelCurrent + u_clippedLevelOrigin;
	vec2 worldPos = levelPos * u_levelScaleFactor * u_levelZeroWorldScaleFactor + u_levelZeroWorldOrigin;

	vec3 displacedPosition = vec3(worldPos, centerHeight);

	// Find the sun and eye direction vectors.
    positionToLightFS = og_sunPosition - displacedPosition;
    positionToEyeFS = og_cameraEye - displacedPosition;

    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
}
