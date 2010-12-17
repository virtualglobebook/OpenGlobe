#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(triangles) in;
layout(triangle_strip, max_vertices = 8) out;

uniform mat4 og_viewportTransformationMatrix;
uniform mat4 og_viewportOrthographicMatrix;
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

    if (numPositions > 0)
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
		    if (numPositions == 3)
			{
				vec2 v01 = normalize(window[1].xy - window[0].xy);
				vec2 v12 = normalize(window[2].xy - window[1].xy);
				vec2 v20 = normalize(window[0].xy - window[2].xy);

				vec2 v0Expand = normalize(v20 - v01);
				vec2 v1Expand = normalize(v01 - v12);
				vec2 v2Expand = normalize(v12 - v20);

				vec2 v01Expand = vec2(-v01.y, v01.x);
				vec2 v12Expand = vec2(-v12.y, v12.x);
				vec2 v20Expand = vec2(-v20.y, v20.x);
				
				vec4 v[6];
				
				float c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v0Expand, v20Expand))));
				v[0] = vec4(window[0].xy + c * normalize(v0Expand + v20Expand), -window[0].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v0Expand, v01Expand))));
				v[1] = vec4(window[0].xy + c * normalize(v0Expand + v01Expand), -window[0].z, 1.0);
				
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v1Expand, v01Expand))));
				v[2] = vec4(window[1].xy + c * normalize(v1Expand + v01Expand), -window[1].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v1Expand, v12Expand))));
				v[3] = vec4(window[1].xy + c * normalize(v1Expand + v12Expand), -window[1].z, 1.0);
				
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v2Expand, v12Expand))));
				v[4] = vec4(window[2].xy + c * normalize(v2Expand + v12Expand), -window[2].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v2Expand, v20Expand))));
				v[5] = vec4(window[2].xy + c * normalize(v2Expand + v20Expand), -window[2].z, 1.0);
				
				gl_Position = og_viewportOrthographicMatrix * v[0];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[1];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[5];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[2];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[4];
				EmitVertex();		
				gl_Position = og_viewportOrthographicMatrix * v[3];
				EmitVertex();			
			}
			else // if (numPositions == 4)
			{
				vec2 v01 = normalize(window[1].xy - window[0].xy);
				vec2 v12 = normalize(window[2].xy - window[1].xy);
				vec2 v23 = normalize(window[3].xy - window[2].xy);
				vec2 v30 = normalize(window[0].xy - window[3].xy);

				vec2 v0Expand = normalize(v30 - v01);
				vec2 v1Expand = normalize(v01 - v12);
				vec2 v2Expand = normalize(v12 - v23);
				vec2 v3Expand = normalize(v23 - v30);
			
				vec2 v01Expand = vec2(-v01.y, v01.x);
				vec2 v12Expand = vec2(-v12.y, v12.x);
				vec2 v23Expand = vec2(-v23.y, v23.x);
				vec2 v30Expand = vec2(-v30.y, v30.x);

				vec4 v[8];
			
				float c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v0Expand, v30Expand))));
				v[0] = vec4(window[0].xy + c * normalize(v0Expand + v30Expand), -window[0].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v0Expand, v01Expand))));
				v[1] = vec4(window[0].xy + c * normalize(v0Expand + v01Expand), -window[0].z, 1.0);
				
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v1Expand, v01Expand))));
				v[2] = vec4(window[1].xy + c * normalize(v1Expand + v01Expand), -window[1].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v1Expand, v12Expand))));
				v[3] = vec4(window[1].xy + c * normalize(v1Expand + v12Expand), -window[1].z, 1.0);
				 
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v2Expand, v12Expand))));
				v[4] = vec4(window[2].xy + c * normalize(v2Expand + v12Expand), -window[2].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v2Expand, v23Expand))));
				v[5] = vec4(window[2].xy + c * normalize(v2Expand + v23Expand), -window[2].z, 1.0);
				
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v3Expand, v23Expand))));
				v[6] = vec4(window[3].xy + c * normalize(v3Expand + v23Expand), -window[3].z, 1.0);
				c = u_fillDistance / sqrt(0.5 * (1.0 + abs(dot(v3Expand, v30Expand))));
				v[7] = vec4(window[3].xy + c * normalize(v3Expand + v30Expand), -window[3].z, 1.0);
		
				gl_Position = og_viewportOrthographicMatrix * v[0];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[1];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[7];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[2];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[6];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[3];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[5];
				EmitVertex();			
				gl_Position = og_viewportOrthographicMatrix * v[4];
				EmitVertex();			
			}
        }
    }
}