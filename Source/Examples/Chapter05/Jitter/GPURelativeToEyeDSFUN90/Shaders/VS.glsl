#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

in vec3 positionHigh;
in vec3 positionLow;
in vec3 color;

out vec3 fsColor;

//uniform vec3 u_cameraEyeHigh;
//uniform vec3 u_cameraEyeLow;
//uniform mat4 u_modelViewPerspectiveMatrixRelativeToEye;
//uniform mat4 og_modelViewPerspectiveMatrixRelativeToEye;
uniform vec3 og_cameraEyeHigh;
uniform vec3 og_cameraEyeLow;
uniform mat4 og_modelViewMatrixRelativeToEye;
uniform mat4 og_perspectiveMatrix;

uniform float u_pointSize;

void main()                     
{
    //
    // Emulated double precision subtraction ported from dssub() in DSFUN90.
    // http://crd.lbl.gov/~dhbailey/mpdist/
    //
	//vec3 t1 = positionLow - u_cameraEyeLow;
	//vec3 e = t1 - positionLow;
	//vec3 t2 = ((-u_cameraEyeLow - e) + (positionLow - (t1 - e))) + positionHigh - u_cameraEyeHigh;
	//vec3 highDifference = t1 + t2;
	//vec3 lowDifference = t2 - (highDifference - t1);

	vec3 t1 = positionLow - og_cameraEyeLow;
	vec3 e = t1 - positionLow;
	vec3 t2 = ((-og_cameraEyeLow - e) + (positionLow - (t1 - e))) + positionHigh - og_cameraEyeHigh;
	vec3 highDifference = t1 + t2;
	vec3 lowDifference = t2 - (highDifference - t1);

    //gl_Position = u_modelViewPerspectiveMatrixRelativeToEye * vec4(highDifference + lowDifference, 1.0);
    //gl_Position = og_modelViewPerspectiveMatrixRelativeToEye * vec4(highDifference + lowDifference, 1.0);
    gl_Position = og_perspectiveMatrix * og_modelViewMatrixRelativeToEye * vec4(highDifference + lowDifference, 1.0);
    gl_PointSize = u_pointSize;
    fsColor = color;
}