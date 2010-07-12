#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 4) out;

uniform mat4 og_perspectiveProjectionMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform float og_pixelSizePerDistance;
uniform float og_perspectiveNearPlaneDistance;

vec4 ClipToWindowCoordinates(vec4 v)
{
    v.xyz /= v.w;
    return vec4((og_viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz, 1.0);
}

void ClipLineSegmentToNearPlane(
    vec4 model0, 
    vec4 model1, 
    out vec4 clippedModel0, 
    out vec4 clippedModel1,
    inout int numPositions)
{
    //
    // I am adding 1.0 to the near plane to push it out slightly to hopefully
    // correct precision errors when tryint to clip exactly at the near plane.
    // I know this isn't the best way. 
    //
    float distanceTo0 = model0.z + og_perspectiveNearPlaneDistance + 1.0;
    float distanceTo1 = model1.z + og_perspectiveNearPlaneDistance + 1.0;
    if ((distanceTo0 * distanceTo1) < 0.0)
    {
        //
        // One is front of and one is behind the near plane
        //
        float t = distanceTo0 / (distanceTo0 - distanceTo1);
        vec4 clipV = model0 + (t * (model1 - model0));
        if (distanceTo0 < 0.0)
        {
            clippedModel0 = model0;
            clippedModel1 = clipV;
            numPositions += 2;
        }
        else
        {
            clippedModel0 = clipV;
            ++numPositions;
        }
    }
    else if (distanceTo0 < 0.0)
    {
        //
        // Both are in front of the near plane
        //
        clippedModel0 = model0;
        ++numPositions;
    }
}

void main()
{   
    int numPositions = 0;
    vec4 clippedModel[5];
    ClipLineSegmentToNearPlane(gl_in[0].gl_Position, gl_in[1].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
    ClipLineSegmentToNearPlane(gl_in[1].gl_Position, gl_in[2].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
    ClipLineSegmentToNearPlane(gl_in[2].gl_Position, gl_in[3].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);
    ClipLineSegmentToNearPlane(gl_in[3].gl_Position, gl_in[0].gl_Position, clippedModel[numPositions], clippedModel[numPositions + 1], numPositions);

	if (numPositions > 0)
    {
        //
        // Get longest length
        //
        vec4 window[5];
		vec4 tempVec4;
        for (int i = 0; i < numPositions; ++i)
        {
		    tempVec4 = og_perspectiveProjectionMatrix * vec4(clippedModel[i].xyz, 1.0);
            window[i] = ClipToWindowCoordinates(tempVec4);
        }
		float maxLength = 0.0;
		float temp;
		for (int i = 0; i < numPositions - 1; ++i)
		{
		    for (int j = i + 1; j < numPositions; ++j)
			{
			    //vec2 diff = window[i].xy - window[j].xy;
			    //temp = dot(diff, diff);
				temp = distance(window[i].xy, window[j].xy);
				if (temp > maxLength)
				{
				    maxLength = temp;
				}
			}
		}

        //
        // Compute area
        //
        float area = 0.0;
        int limit = numPositions - 1;
        for (int i = 0; i < limit; ++i)
        {
            area += (window[i].x * window[i + 1].y) - (window[i + 1].x * window[i].y);
        }
        area += (window[limit].x * window[0].y) - (window[0].x * window[limit].y);

		//
		// Area check
		//
        if ((20.0 * maxLength) <= abs(area))
        {
			gl_Position = og_perspectiveProjectionMatrix * gl_in[1].gl_Position;
			EmitVertex();
			gl_Position = og_perspectiveProjectionMatrix * gl_in[0].gl_Position;
			EmitVertex();
			gl_Position = og_perspectiveProjectionMatrix * gl_in[2].gl_Position;
			EmitVertex();
			gl_Position = og_perspectiveProjectionMatrix * gl_in[3].gl_Position;
			EmitVertex();
			EndPrimitive();
		}
	}
}