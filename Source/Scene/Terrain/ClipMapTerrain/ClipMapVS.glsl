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

uniform mat4 og_modelViewPerspectiveMatrix;
uniform vec3 og_cameraEye;
uniform vec3 og_sunPosition;
uniform sampler2D og_texture0;    // Height map
uniform vec4 u_scaleFactor;
uniform vec4 u_fineBlockOrig;

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

vec3 ComputeNormalSobelFilter(
    vec3 displacedPosition, 
    sampler2D heightMap, 
	vec2 textureScale,
    float heightExaggeration)
{
	//
	// Original unoptimized verion
	//
	//vec2 position = displacedPosition.xy;
	//float upperLeft = texture(heightMap, position + vec2(-1.0, 1.0)).r * heightExaggeration;
	//float upperCenter = texture(heightMap, position + vec2(0.0, 1.0)).r * heightExaggeration;
	//float upperRight = texture(heightMap, position + vec2(1.0, 1.0)).r * heightExaggeration;
	//float left = texture(heightMap, position + vec2(-1.0, 0.0)).r * heightExaggeration;
	//float right = texture(heightMap, position + vec2(1.0, 0.0)).r * heightExaggeration;
	//float lowerLeft = texture(heightMap, position + vec2(-1.0, -1.0)).r * heightExaggeration;
	//float lowerCenter = texture(heightMap, position + vec2(0.0, -1.0)).r * heightExaggeration;
	//float lowerRight = texture(heightMap, position + vec2(1.0, -1.0)).r * heightExaggeration;
	//
	//mat3 positions = mat3(
	//    upperLeft, left, lowerLeft,
	//    upperCenter, 0.0, lowerCenter,
	//    upperRight, right, lowerRight);
	//mat3 sobelX = mat3(
	//    -1.0, -2.0, -1.0,
	//     0.0,  0.0,  0.0,
	//     1.0,  2.0,  1.0);
	//mat3 sobelY = mat3(
	//    -1.0, 0.0, 1.0,
	//    -2.0, 0.0, 2.0,
	//    -1.0, 0.0, 1.0);
	//
	//float x = SumElements(matrixCompMult(positions, sobelX));
	//float y = SumElements(matrixCompMult(positions, sobelY));
	//
	//return vec3(-x, y, 1.0);

    vec2 position = displacedPosition.xy;
    float upperLeft = texture(heightMap, position + vec2(-1.0, 1.0) * textureScale).r * heightExaggeration;
    float upperCenter = texture(heightMap, position + vec2(0.0, 1.0) * textureScale).r * heightExaggeration;
    float upperRight = texture(heightMap, position + vec2(1.0, 1.0) * textureScale).r * heightExaggeration;
    float left = texture(heightMap, position + vec2(-1.0, 0.0) * textureScale).r * heightExaggeration;
    float right = texture(heightMap, position + vec2(1.0, 0.0) * textureScale).r * heightExaggeration;
    float lowerLeft = texture(heightMap, position + vec2(-1.0, -1.0) * textureScale).r * heightExaggeration;
    float lowerCenter = texture(heightMap, position + vec2(0.0, -1.0) * textureScale).r * heightExaggeration;
    float lowerRight = texture(heightMap, position + vec2(1.0, -1.0) * textureScale).r * heightExaggeration;

    float x = upperRight + (2.0 * right) + lowerRight - upperLeft - (2.0 * left) - lowerLeft;
    float y = lowerLeft + (2.0 * lowerCenter) + lowerRight - upperLeft - (2.0 * upperCenter) - upperRight;

    return vec3(-x, y, 1.0);
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
	vec2 uv = position * u_fineBlockOrig.xy + u_fineBlockOrig.zw;

	// sample the vertex texture
	height = texture(og_texture0, uv).r;

	normalFS = ComputeNormalForwardDifference(vec3(uv, height * 0.00001), og_texture0, u_fineBlockOrig.xy, 0.00001);

	vec3 displacedPosition = vec3(worldPos.x, worldPos.y, height * 0.00001);
	gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
    positionToLightFS = og_sunPosition - displacedPosition;
    positionToEyeFS = og_cameraEye - displacedPosition;
}
