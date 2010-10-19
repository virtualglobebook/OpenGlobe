#version 330
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//

layout(location = og_positionVertexLocation) in vec2 position;

out float height;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform sampler2D og_texture0;    // Height map
uniform vec4 u_scaleFactor;
uniform vec4 u_fineBlockOrig;

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

	gl_Position = og_modelViewPerspectiveMatrix * vec4(worldPos.x, worldPos.y, height * 0.00001, 1.0);
}
