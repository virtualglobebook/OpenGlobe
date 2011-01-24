#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
        
in vec3 boxExit;

out vec3 fragmentColor;

uniform sampler2DRect u_heightMap;
uniform vec3 u_aabbLowerLeft;
uniform vec3 u_aabbUpperRight;
uniform float u_minimumHeight;
uniform float u_maximumHeight;
uniform float u_heightExaggeration;
uniform int u_shadingAlgorithm;

struct Intersection
{
    bool Intersects;
    vec3 IntersectionPoint;
};

bool PointInsideAxisAlignedBoundingBox(vec3 point, vec3 lowerLeft, vec3 upperRight)
{
    return all(greaterThanEqual(point, lowerLeft)) && all(lessThanEqual(point, upperRight));
}

void Swap(inout float left, inout float right)
{
    float temp = left;
    left = right;
    right = temp;
}

bool PlanePairTest(
    float origin, 
    float direction, 
    float aabbLowerLeft, 
    float aabbUpperRight,
    inout float tNear,
    inout float tFar)
{
    if (direction == 0.0)
    {
        //
        // Ray is parallel to planes
        //
        if (origin < aabbLowerLeft || origin > aabbUpperRight)
        {
            return false;
        }
    }
    else
    {
        //
        // Compute the intersection distances of the planes
        //
        float oneOverDirection = 1.0 / direction;
        float t1 = (aabbLowerLeft - origin) * oneOverDirection;
        float t2 = (aabbUpperRight - origin) * oneOverDirection;

        //
        // Make t1 intersection with nearest plane
        //
        if (t1 > t2)
        {
            Swap(t1, t2);
        }

        //
        // Track largest tNear and smallest tFar
        //
        tNear = max(t1, tNear);
        tFar = min(t2, tFar);

        //
        // Missed box
        //
        if (tNear > tFar)
        {
            return false;
        }

        //
        // Box is behind ray
        //
        if (tFar < 0.0)
        {
            return false;
        }
    }

    return true;
}

Intersection RayIntersectsAABB(vec3 origin, vec3 direction, vec3 aabbLowerLeft, vec3 aabbUpperRight)
{
    //
    // Implementation of http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
    //

    float tNear = og_minimumFloat;
    float tFar = og_maximumFloat;

    if (PlanePairTest(origin.x, direction.x, aabbLowerLeft.x, aabbUpperRight.x, tNear, tFar) &&
        PlanePairTest(origin.y, direction.y, aabbLowerLeft.y, aabbUpperRight.y, tNear, tFar) &&
        PlanePairTest(origin.z, direction.z, aabbLowerLeft.z, aabbUpperRight.z, tNear, tFar))
    {
        return Intersection(true, origin + (tNear * direction));
    }

    return Intersection(false, vec3(0.0));
}

vec2 MirrorRepeat(vec2 textureCoordinate, vec2 mirrorTextureCoordinates)
{
    return vec2(
        mirrorTextureCoordinates.x == 0.0 ? textureCoordinate.x : mirrorTextureCoordinates.x - textureCoordinate.x, 
        mirrorTextureCoordinates.y == 0.0 ? textureCoordinate.y : mirrorTextureCoordinates.y - textureCoordinate.y);
}

bool StepRay(
    vec3 direction, 
    vec2 oneOverDirectionXY,
    vec2 mirrorTextureCoordinates,
    inout vec3 texEntry,
    out vec3 intersectionPoint)
{
    vec2 floorTexEntry = floor(texEntry.xy);
    float height = texture(u_heightMap, MirrorRepeat(floorTexEntry, mirrorTextureCoordinates)).r;
    height *= u_heightExaggeration;

    vec2 delta = ((floorTexEntry + vec2(1.0)) - texEntry.xy) * oneOverDirectionXY;
    vec3 texExit = texEntry + (min(delta.x, delta.y) * direction);

    //
    // Explicitly set to avoid roundoff error
    //
    if (delta.x < delta.y)
    {
        texExit.x = floorTexEntry.x + 1.0;
    }
    else
    {
        texExit.y = floorTexEntry.y + 1.0;
    }

    //
    // Check for intersection
    //
    bool foundIntersection = false;

    if (direction.z >= 0.0)
    {
        if (texEntry.z <= height)
        {
            foundIntersection = true;
            intersectionPoint = texEntry;
        }
    }
    else
    {
        if (texExit.z <= height)
        {
            foundIntersection = true;
            intersectionPoint = texEntry + (max((height - texEntry.z) / direction.z, 0.0) * direction);
        }
    }

    texEntry = texExit;
    return foundIntersection;
}

float ComputeWorldPositionDepth(vec3 position, mat4x2 modelZToClipCoordinates)
{ 
    vec2 v = modelZToClipCoordinates * vec4(position, 1);   // clip coordinates
    v.x /= v.y;                                             // normalized device coordinates
    v.x = (v.x + 1.0) * 0.5;
    return v.x;
}

void main()
{
    vec3 direction = boxExit - og_cameraEye;

    vec3 boxEntry;
    if (PointInsideAxisAlignedBoundingBox(og_cameraEye, u_aabbLowerLeft, u_aabbUpperRight))
    {
        boxEntry = og_cameraEye;
    }
    else
    {
        Intersection i = RayIntersectsAABB(og_cameraEye, direction, u_aabbLowerLeft, u_aabbUpperRight);
        boxEntry = i.IntersectionPoint;
    }

	//
	// Mirror such that ray always steps in positive x and y direction
	//
    vec2 heightMapSize = vec2(textureSize(u_heightMap));
    bvec2 mirror = lessThan(direction.xy, vec2(0.0));
    vec2 mirrorTextureCoordinates = vec2(0.0);

	if (mirror.x)
	{
		direction.x = -direction.x;
		boxEntry.x = heightMapSize.x - boxEntry.x;
		mirrorTextureCoordinates.x = heightMapSize.x - 1.0;
    }

	if (mirror.y)
	{
		direction.y = -direction.y;
		boxEntry.y = heightMapSize.y - boxEntry.y;
		mirrorTextureCoordinates.y = heightMapSize.y - 1.0;
    }

    vec2 oneOverDirectionXY = vec2(1.0) / direction.xy;
    vec3 texEntry = boxEntry;
    vec3 intersectionPoint;
    bool foundIntersection = false;
	int numberOfSteps = 0;

    while (!foundIntersection && all(lessThan(texEntry.xy, heightMapSize)))
    {
        foundIntersection = StepRay(direction, oneOverDirectionXY, 
            mirrorTextureCoordinates, texEntry, intersectionPoint);
		++numberOfSteps;
    }

    if (foundIntersection)
    {
		if (mirror.x)
		{
			intersectionPoint.x = heightMapSize.x - intersectionPoint.x;
		}

		if (mirror.y)
		{
			intersectionPoint.y = heightMapSize.y - intersectionPoint.y;
		}

        gl_FragDepth = ComputeWorldPositionDepth(intersectionPoint, og_modelZToClipCoordinates);

		if (u_shadingAlgorithm == 0)      // RayCastedTerrainShadingAlgorithm.ByHeight
		{
	        fragmentColor = vec3((intersectionPoint.z - u_minimumHeight) / (u_maximumHeight - u_minimumHeight), 0.0, 0.0);
		}
		else if (u_shadingAlgorithm == 1) // RayCastedTerrainShadingAlgorithm.ByRaySteps
		{
			fragmentColor = mix(vec3(1.0, 0.5, 0.0), vec3(0.0, 0.0, 1.0), float(numberOfSteps) / (heightMapSize.x + heightMapSize.y));
		}
    }
    else
    {
        discard;
    }
}
