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
    vec4 modelP0, 
    vec4 modelP1, 
    out vec4 window0, 
    out vec4 window1,
    out bool lineSegmentAtLeastPartlyInFrontOfNearPlane)
{
    float distanceToP0 = modelP0.z + og_perspectiveNearPlaneDistance;
    float distanceToP1 = modelP1.z + og_perspectiveNearPlaneDistance;
    if ((distanceToP0 * distanceToP1) < 0.0)
    {
        float t = distanceToP0 / (distanceToP0 - distanceToP1);
        vec4 clipV = modelP0 + (t * (modelP1 - modelP0));
        if (distanceToP0 < 0.0)
        {
            window0 = ClipToWindowCoordinates(clipV);
            window1 = ClipToWindowCoordinates(modelP1);
        }
        else
        {
            window0 = ClipToWindowCoordinates(modelP0);
            window1 = ClipToWindowCoordinates(clipV);
        }
    }
    else
    {
        window0 = ClipToWindowCoordinates(modelP0);
        window1 = ClipToWindowCoordinates(modelP1);
    }
    lineSegmentAtLeastPartlyInFrontOfNearPlane = (distanceToP0 >= 0.0) || (distanceToP1 >= 0.0);
}

void main()
{
    vec4 window[6];
    int index = 0;
    bool lineSegmentAtLeastPartlyInFrontOfNearPlane;

    ClipLineSegmentToNearPlane(gl_in[0].gl_Position, gl_in[1].gl_Position, window[index], window[index + 1],
        lineSegmentAtLeastPartlyInFrontOfNearPlane);
    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
    {
        index += 2;
    }

    ClipLineSegmentToNearPlane(gl_in[1].gl_Position, gl_in[2].gl_Position, window[index], window[index + 1],
        lineSegmentAtLeastPartlyInFrontOfNearPlane);
    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
    {
        index += 2;
    }

    ClipLineSegmentToNearPlane(gl_in[2].gl_Position, gl_in[0].gl_Position, window[index], window[index + 1],
        lineSegmentAtLeastPartlyInFrontOfNearPlane);
    if (lineSegmentAtLeastPartlyInFrontOfNearPlane)
    {
        index += 2;
    }

    if (index > 0)
    {
        float area = 0.0;
        int limit = index - 1;
        for (int i = 0; i < limit; ++i)
        {
            area += (window[i].x * window[i + 1].y) - (window[i + 1].x * window[i].y);
        }
        area += (window[limit].x * window[0].y) - (window[0].x * window[limit].y);

        if (area < 0.0)
        {
            for (int i = 0; i < index; i += 2)
            {
                vec4 window0 = window[i];
                vec4 window1 = window[i + 1];

                vec2 direction = window1.xy - window0.xy;
                    
                if (dot(direction, direction) > 0.0)
                {
                    direction = normalize(direction) * u_fillDistance;
                    vec2 cross = vec2(direction.y, -direction.x);

                    vec4 v0 = vec4(window0.xy - cross, -window0.z, 1.0);
                    vec4 v1 = vec4(window0.xy + cross, -window0.z, 1.0);
                    vec4 v2 = vec4(window1.xy - cross, -window1.z, 1.0);
                    vec4 v3 = vec4(window1.xy + cross, -window1.z, 1.0);
                    vec4 v4 = vec4(window1.xy + direction, -window1.z, 1.0);
 
                    gl_Position = og_viewportOrthographicProjectionMatrix * v0;
                    EmitVertex();

                    gl_Position = og_viewportOrthographicProjectionMatrix * v1;
                    EmitVertex();

                    gl_Position = og_viewportOrthographicProjectionMatrix * v2;
                    EmitVertex();

                    gl_Position = og_viewportOrthographicProjectionMatrix * v3;
                    EmitVertex();

                    gl_Position = og_viewportOrthographicProjectionMatrix * v4;
                    EmitVertex();

                    EndPrimitive();
                }
            }
        }
    }
}