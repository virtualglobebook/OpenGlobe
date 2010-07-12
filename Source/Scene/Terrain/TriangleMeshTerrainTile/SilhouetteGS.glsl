#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(triangles) in;
layout(triangle_strip, max_vertices = 15) out;

uniform mat4 og_viewportTransformationMatrix;
uniform mat4 og_viewportOrthographicProjectionMatrix;
uniform float og_perspectiveNearPlaneDistance;
uniform float u_fillDistance;

vec4 ClipToWindowCoordinates(vec4 v)
{
    v.xyz /= v.w;
    return vec4((og_viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz, 1.0);
}

void ClipLineSegmentToNearPlane(
    vec4 clip0, 
    vec4 clip1, 
    out vec4 window0, 
    out vec4 window1,
    inout int numPositions)
{
    float distanceTo0 = clip0.z + og_perspectiveNearPlaneDistance;
    float distanceTo1 = clip1.z + og_perspectiveNearPlaneDistance;
    if ((distanceTo0 * distanceTo1) < 0.0)
    {
        float t = distanceTo0 / (distanceTo0 - distanceTo1);
        vec4 clipV = clip0 + (t * (clip1 - clip0));
        if (distanceTo0 < 0.0)
        {
            window0 = ClipToWindowCoordinates(clipV);
            ++numPositions;
        }
        else
        {
            window0 = ClipToWindowCoordinates(clip0);
            window1 = ClipToWindowCoordinates(clipV);
            numPositions += 2;
        }
    }
    else if (distanceTo0 >= 0.0)
    {
        window0 = ClipToWindowCoordinates(clip0);
        ++numPositions;
    }
}

void main()
{
    vec4 window[4];
    int numPositions = 0;
    bool lineSegmentAtLeastPartlyInFrontOfNearPlane;

    ClipLineSegmentToNearPlane(gl_in[0].gl_Position, gl_in[1].gl_Position, window[numPositions], window[numPositions + 1],
        numPositions);
    ClipLineSegmentToNearPlane(gl_in[1].gl_Position, gl_in[2].gl_Position, window[numPositions], window[numPositions + 1],
        numPositions);
    ClipLineSegmentToNearPlane(gl_in[2].gl_Position, gl_in[0].gl_Position, window[numPositions], window[numPositions + 1],
        numPositions);

		// junk deron todo
//    if (numPositions > 0)
    if (numPositions == 3)
    {
        float area = 0.0;
        int limit = numPositions - 1;
        for (int i = 0; i < limit; ++i)
        {
            area += (window[i].x * window[i + 1].y) - (window[i + 1].x * window[i].y);
        }
        area += (window[limit].x * window[0].y) - (window[0].x * window[limit].y);

        if (area < 0.0)
        {
		    vec2 v01 = window[1].xy - window[0].xy;
		    vec2 v12 = window[2].xy - window[1].xy;
		    vec2 v20 = window[0].xy - window[2].xy;
			v01 = normalize(v01);
			v12 = normalize(v12);
			v20 = normalize(v20);
			vec2 v0Expand = v20 - v01;
			vec2 v1Expand = v01 - v12;
			vec2 v2Expand = v12 - v20;
			v0Expand = u_fillDistance * normalize(v0Expand); 
			v1Expand = u_fillDistance * normalize(v1Expand); 
			v2Expand = u_fillDistance * normalize(v2Expand); 

			

			vec4 v0 = vec4(window[0].xy + v0Expand, -window[0].z, 1.0);
			vec4 v1 = vec4(window[1].xy + v1Expand, -window[1].z, 1.0);
			vec4 v2 = vec4(window[2].xy + v2Expand, -window[2].z, 1.0);
			if (v0.x == 0.12323)
			{
			    v0.x += u_fillDistance;
			}

            gl_Position = og_viewportOrthographicProjectionMatrix * v0;
            EmitVertex();
            gl_Position = og_viewportOrthographicProjectionMatrix * v1;
            EmitVertex();
            gl_Position = og_viewportOrthographicProjectionMatrix * v2;
            EmitVertex();	
	        EndPrimitive();

        }
    }
}