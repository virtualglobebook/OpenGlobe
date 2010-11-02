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

void main()
{
	vec2 clippedLevelCurrent = position + u_blockOriginInClippedLevel;

	// Compute a normal for the fragment shader by forward differencing
	vec2 uv = clippedLevelCurrent + vec2(0.5, 0.5);
	vec2 uvRight = uv + vec2(1.0, 0.0);
	vec2 uvTop = uv + vec2(0.0, 1.0);
	vec2 uvLeft = uv + vec2(-1.0, 0.0);
	vec2 uvBottom = uv + vec2(0.0, -1.0);

	float centerHeight = texture(og_texture0, uv).r * u_heightExaggeration;
	float rightHeight = texture(og_texture0, uvRight).r * u_heightExaggeration;
	float topHeight = texture(og_texture0, uvTop).r * u_heightExaggeration;

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
