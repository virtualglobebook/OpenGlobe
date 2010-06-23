#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

in float distanceToEyeGS[];
out float distanceToEyeFS;

uniform mat4 og_modelViewPerspectiveProjectionMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform mat4 og_viewportOrthographicProjectionMatrix;
uniform float og_perspectiveNearPlaneDistance;
uniform sampler2DRect og_texture0;    // Height map
uniform float u_heightExaggeration;
uniform float u_fillDistance;
uniform int u_normalAlgorithm;

vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
{
    v.xyz /= v.w;                                                        // normalized device coordinates
    v.xyz = (viewportTransformationMatrix * vec4(v.xyz + 1.0, 1.0)).xyz; // windows coordinates
    return v;
}

void ClipLineSegmentToNearPlane(
    float nearPlaneDistance, 
    mat4 modelViewPerspectiveProjectionMatrix,
    vec4 modelP0, 
    vec4 modelP1, 
    out vec4 clipP0, 
    out vec4 clipP1)
{
    clipP0 = modelViewPerspectiveProjectionMatrix * modelP0;
    clipP1 = modelViewPerspectiveProjectionMatrix * modelP1;

    float distanceToP0 = clipP0.z - nearPlaneDistance;
    float distanceToP1 = clipP1.z - nearPlaneDistance;

    if ((distanceToP0 * distanceToP1) < 0.0)
    {
        float t = distanceToP0 / (distanceToP0 - distanceToP1);
        vec3 modelV = vec3(modelP0) + t * (vec3(modelP1) - vec3(modelP0));
        vec4 clipV = modelViewPerspectiveProjectionMatrix * vec4(modelV, 1);

        if (distanceToP0 < 0.0)
        {
            clipP0 = clipV;
        }
        else
        {
            clipP1 = clipV;
        }
    }
}

vec3 ComputeNormalForwardDifference(
    vec3 displacedPosition, 
    sampler2DRect heightMap, 
    float heightExaggeration)
{
    vec3 right = vec3(displacedPosition.xy + vec2(1.0, 0.0), texture(heightMap, displacedPosition.xy + vec2(1.0, 0.0)).r * heightExaggeration);
    vec3 top = vec3(displacedPosition.xy + vec2(0.0, 1.0), texture(heightMap, displacedPosition.xy + vec2(0.0, 1.0)).r * heightExaggeration);
    return cross(right - displacedPosition, top - displacedPosition);
}

vec3 ComputeNormalCentralDifference(
    vec3 displacedPosition, 
    sampler2DRect heightMap, 
    float heightExaggeration)
{
    vec2 position = displacedPosition.xy;
    float leftHeight = texture(heightMap, position - vec2(1.0, 0.0)).r * heightExaggeration;
    float rightHeight = texture(heightMap, position + vec2(1.0, 0.0)).r * heightExaggeration;
    float bottomHeight = texture(heightMap, position - vec2(0.0, 1.0)).r * heightExaggeration;
    float topHeight = texture(heightMap, position.xy + vec2(0.0, 1.0)).r * heightExaggeration;
    return vec3(leftHeight - rightHeight, bottomHeight - topHeight, 2.0);
}

vec3 ComputeNormalSobelFilter(
    vec3 displacedPosition, 
    sampler2DRect heightMap, 
    float heightExaggeration)
{
    vec2 position = displacedPosition.xy;
    float upperLeft = texture(heightMap, position + vec2(-1.0, 1.0)).r * heightExaggeration;
    float upperCenter = texture(heightMap, position + vec2(0.0, 1.0)).r * heightExaggeration;
    float upperRight = texture(heightMap, position + vec2(1.0, 1.0)).r * heightExaggeration;
    float left = texture(heightMap, position + vec2(-1.0, 0.0)).r * heightExaggeration;
    float right = texture(heightMap, position + vec2(1.0, 0.0)).r * heightExaggeration;
    float lowerLeft = texture(heightMap, position + vec2(-1.0, -1.0)).r * heightExaggeration;
    float lowerCenter = texture(heightMap, position + vec2(0.0, -1.0)).r * heightExaggeration;
    float lowerRight = texture(heightMap, position + vec2(1.0, -1.0)).r * heightExaggeration;

    float x = upperRight + (2.0 * right) + lowerRight - upperLeft - (2.0 * left) - lowerLeft;
    float y = lowerLeft + (2.0 * lowerCenter) + lowerRight - upperLeft - (2.0 * upperCenter) - upperRight;

    return vec3(-x, y, 1.0);
}

void main()
{
    vec3 terrainNormal = vec3(0.0);

    if (u_normalAlgorithm == 1)       // TerrainNormalsAlgorithm.ForwardDifference
    {
        terrainNormal = ComputeNormalForwardDifference(gl_in[0].gl_Position.xyz, og_texture0, u_heightExaggeration);
    }
    else if (u_normalAlgorithm == 2)  // TerrainNormalsAlgorithm.CentralDifference
    {
        terrainNormal = ComputeNormalCentralDifference(gl_in[0].gl_Position.xyz, og_texture0, u_heightExaggeration);
    }
    else if (u_normalAlgorithm == 3)  // TerrainNormalsAlgorithm.SobelFilter
    {
        terrainNormal = ComputeNormalSobelFilter(gl_in[0].gl_Position.xyz, og_texture0, u_heightExaggeration);
    }

    vec4 clipP0;
    vec4 clipP1;
    ClipLineSegmentToNearPlane(og_perspectiveNearPlaneDistance, 
        og_modelViewPerspectiveProjectionMatrix,
        gl_in[0].gl_Position, 
        gl_in[0].gl_Position + vec4(normalize(terrainNormal), 0.0),
        clipP0, clipP1);

    vec4 windowP0 = ClipToWindowCoordinates(clipP0, og_viewportTransformationMatrix);
    vec4 windowP1 = ClipToWindowCoordinates(clipP1, og_viewportTransformationMatrix);

    vec2 direction = windowP1.xy - windowP0.xy;
    vec2 normal = normalize(vec2(direction.y, -direction.x));

    vec4 v0 = vec4(windowP0.xy - (normal * u_fillDistance), windowP0.z, 1.0);
    vec4 v1 = vec4(windowP1.xy - (normal * u_fillDistance), windowP1.z, 1.0);
    vec4 v2 = vec4(windowP0.xy + (normal * u_fillDistance), windowP0.z, 1.0);
    vec4 v3 = vec4(windowP1.xy + (normal * u_fillDistance), windowP1.z, 1.0);

    gl_Position = og_viewportOrthographicProjectionMatrix * v0;
    distanceToEyeFS = distanceToEyeGS[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicProjectionMatrix * v1;
    distanceToEyeFS = distanceToEyeGS[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicProjectionMatrix * v2;
    distanceToEyeFS = distanceToEyeGS[0];
    EmitVertex();

    gl_Position = og_viewportOrthographicProjectionMatrix * v3;
    distanceToEyeFS = distanceToEyeGS[0];
    EmitVertex();
}
