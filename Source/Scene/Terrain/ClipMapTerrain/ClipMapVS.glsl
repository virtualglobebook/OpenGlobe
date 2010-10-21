#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;

out float height;
out vec3 normalFS;
out vec3 positionToLightFS;
out vec3 positionToEyeFS;
out float alphaScalar;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_cameraEye;
uniform vec3 og_sunPosition;
uniform sampler2D og_texture0;    // Fine height map
uniform sampler2D og_texture1;    // Coarse height map
uniform vec4 u_scaleFactor;
uniform vec4 u_fineBlockOrig;
uniform vec4 u_coarseBlockOrig;
uniform vec2 u_viewerPos;
uniform vec2 u_alphaOffset;
uniform float u_oneOverTransitionWidth;

vec3 ComputeNormalForwardDifference(
    vec3 displacedPosition, 
    sampler2D heightMap, 
	vec2 textureScale, // 1/(w, h) of texture
    float heightExaggeration)
{
    vec3 right = vec3(displacedPosition.xy + vec2(1.0, 0.0) * textureScale, texture(heightMap, displacedPosition.xy + vec2(1.0, 0.0) * textureScale).r * heightExaggeration);
    vec3 top = vec3(displacedPosition.xy + vec2(0.0, 1.0) * textureScale, texture(heightMap, displacedPosition.xy + vec2(0.0, 1.0) * textureScale).r * heightExaggeration);
    return cross(right - displacedPosition, top - displacedPosition);
}

void main()
{
	// Convert from grid xy to world xy coordinates
	//  u_scaleFactor.xy: grid spacing of current level
	//  u_scaleFactor.zw: origin of current block within world
	vec2 worldPos = position * u_scaleFactor.xy + u_scaleFactor.zw;

	// Compute coordinates for vertex texture
	//  u_fineBlockOrig.xy: 1/(w, h) of texture
	//  u_fineBlockOrig.zw: origin of block in texture
	vec2 uvFine = position * u_fineBlockOrig.xy + u_fineBlockOrig.zw;
	vec2 uvCoarse = position * u_coarseBlockOrig.xy + u_coarseBlockOrig.zw;

	// compute alpha (transition parameter) and blend elevation
	// u_viewerPos should eventually be simply og_cameraEye.
	vec2 alpha = clamp((abs(worldPos - u_viewerPos) - u_alphaOffset) * u_oneOverTransitionWidth, 0, 1);
	alphaScalar = max(alpha.x, alpha.y);
	alphaScalar = 1.0;

	// sample the vertex texture
	float heightFine = texture(og_texture0, uvFine).r;
	float heightCoarse = texture(og_texture1, uvCoarse).r;
	height = (1.0 - alphaScalar) * heightFine + alphaScalar * heightCoarse;

	normalFS = ComputeNormalForwardDifference(vec3(uvFine, height * 0.00001), og_texture0, u_fineBlockOrig.xy, 0.00001);

	vec3 displacedPosition = vec3(worldPos.x, worldPos.y, height * 0.00001);
	gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
    positionToLightFS = og_sunPosition - displacedPosition;
    positionToEyeFS = og_cameraEye - displacedPosition;
}
