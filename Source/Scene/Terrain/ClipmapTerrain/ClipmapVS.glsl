#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
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

vec2 GridToWorld(vec2 gridPos)
{
	return gridPos * u_scaleFactor.xy + u_scaleFactor.zw;
}

float SampleHeight(vec2 gridPos, vec2 worldPos)
{
	// Compute coordinates for vertex texture
	//  u_fineBlockOrig.xy: 1/(w, h) of texture
	//  u_fineBlockOrig.zw: origin of block in texture
	vec2 uvFine = gridPos * u_fineBlockOrig.xy + u_fineBlockOrig.zw;
	vec2 uvCoarse = gridPos * u_coarseBlockOrig.xy + u_coarseBlockOrig.zw;

	// compute alpha (transition parameter) and blend elevation
	// u_viewerPos should eventually be simply og_cameraEye.xy.
	vec2 alpha = clamp((abs(worldPos - u_viewerPos) - u_alphaOffset) * u_oneOverTransitionWidth, 0, 1);
	alphaScalar = max(alpha.x, alpha.y);

	// sample the vertex texture
	float heightFine = texture(og_texture0, uvFine).r;
	float heightCoarse = texture(og_texture1, uvCoarse).r;
	return (1.0 - alphaScalar) * heightFine + alphaScalar * heightCoarse;
}

vec3 ComputeNormalForwardDifference(
    vec2 gridPos, 
	vec3 worldPos,
    float heightExaggeration)
{
	vec2 rightGrid = gridPos.xy + vec2(1.0, 0.0);
	vec2 rightWorld = GridToWorld(rightGrid);
    vec3 right = vec3(rightWorld, SampleHeight(rightGrid, rightWorld) * heightExaggeration);
	vec2 topGrid = gridPos.xy + vec2(0.0, 1.0);
	vec2 topWorld = GridToWorld(topGrid);
    vec3 top = vec3(topWorld, SampleHeight(topGrid, topWorld) * heightExaggeration);
    return cross(right - worldPos, top - worldPos);
}

void main()
{
	// Convert from grid xy to world xy coordinates
	//  u_scaleFactor.xy: grid spacing of current level
	//  u_scaleFactor.zw: origin of current block within world
	vec2 worldPos = GridToWorld(position);
	height = SampleHeight(position, worldPos);

	float heightExaggeration = 0.00001;
	vec3 displacedPosition = vec3(worldPos, height * heightExaggeration);
	normalFS = ComputeNormalForwardDifference(position, displacedPosition, heightExaggeration);

	gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
    positionToLightFS = og_sunPosition - displacedPosition;
    positionToEyeFS = og_cameraEye - displacedPosition;
}
