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
uniform vec4 u_gridScaleFactor;
uniform vec4 u_worldScaleFactor;
uniform vec4 u_fineBlockOrig;
uniform vec4 u_coarseBlockOrig;
uniform vec2 u_viewerPos;
uniform vec2 u_alphaOffset;
uniform float u_oneOverTransitionWidth;
uniform vec4 u_textureOrigin;

vec2 GridToWorld(vec2 gridPos)
{
    return (gridPos * u_gridScaleFactor.xy + u_gridScaleFactor.zw) * u_worldScaleFactor.xy + u_worldScaleFactor.zw;
}

float SampleHeight(vec2 gridPos)
{
    // Compute coordinates for vertex texture
    //  u_fineBlockOrig.xy: 1/(w, h) of texture
    //  u_fineBlockOrig.zw: origin of block in texture
    vec2 uvFine = gridPos + u_fineBlockOrig.zw;
    vec2 uvCoarse = gridPos * 0.5 + u_coarseBlockOrig.zw;

    // compute alpha (transition parameter) and blend elevation
    vec2 alpha = clamp((abs(gridPos - u_viewerPos) - u_alphaOffset) * u_oneOverTransitionWidth, 0, 1);
    float alphaScalar = max(alpha.x, alpha.y);

    // sample the vertex texture
    float heightFine = texture(og_texture0, uvFine).r;
    float heightCoarse = texture(og_texture1, uvCoarse).r;
    return mix(heightFine, heightCoarse, alphaScalar);
}

vec3 ComputeNormalForwardDifference(
    vec2 gridPos, 
    vec3 worldPos,
    float heightExaggeration)
{
    vec2 rightGrid = gridPos + vec2(1.0, 0.0);
    vec3 right = vec3(rightGrid * u_gridScaleFactor.xy * u_worldScaleFactor.xy, SampleHeight(rightGrid) * heightExaggeration);
    vec2 topGrid = gridPos + vec2(0.0, 1.0);
    vec3 top = vec3(topGrid * u_gridScaleFactor.xy * u_worldScaleFactor.xy, SampleHeight(topGrid) * heightExaggeration);
    vec3 center = vec3(gridPos * u_gridScaleFactor.xy * u_worldScaleFactor.xy, worldPos.z);
    return cross(right - center, top - center);
}

vec3 GeodeticToCartesian(vec3 geodetic)
{
    vec2 geodeticRadians = geodetic.xy * og_radiansPerDegree;
    vec2 cosGeodetic = cos(geodeticRadians.xy);
    vec2 sinGeodetic = sin(geodeticRadians.xy);

    vec3 normal = vec3(cosGeodetic.y * cosGeodetic.x,
                       cosGeodetic.y * sinGeodetic.x,
                       sinGeodetic.y);
    const vec3 Radii = vec3(1.0, 1.0, 0.99664718933522437664791458697109);
    vec3 position = normalize(normal * Radii) * Radii;
    return normal * geodetic.z + position;
}

void main()
{
    vec2 worldPos = GridToWorld(position);
    float height = SampleHeight(position);

    textureCoordinateFS = (position + u_fineBlockOrig.zw) * u_textureOrigin.xy + u_textureOrigin.zw;

    const float heightExaggeration = 1.0 / 6378137.0;
    vec3 displacedPosition = vec3(worldPos, height * heightExaggeration);
    normalFS = ComputeNormalForwardDifference(position, displacedPosition, heightExaggeration);

    positionToLightFS = og_sunPosition - displacedPosition;
    positionToEyeFS = og_cameraEye - displacedPosition;

    displacedPosition = GeodeticToCartesian(displacedPosition);
    gl_Position = og_modelViewPerspectiveMatrix * vec4(displacedPosition, 1.0);
}
