/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
* Proland is distributed under a dual-license scheme.
* You can obtain a specific license from Inria: proland-licensing@inria.fr.
*/

/*
* Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
* Modified and ported to Unity by Justin Hawkins 2014
*/

uniform float _Ocean_Radius;
//uniform float3 _Ocean_Horizon1;
//uniform float3 _Ocean_Horizon2;
uniform float _Ocean_HeightOffset;
uniform float3 _Ocean_CameraPos;
uniform float4x4 _Ocean_OceanToCamera;
uniform float4x4 _Ocean_CameraToOcean;

uniform float3 sphereDir;
uniform float cosTheta;
uniform float sinTheta;


// A glorified sphere intersect, with some code to snap above the horizon back to the horizon
// Returns Position in ocean space
float2 OceanPos(float4 vert, float4x4 stoc, out float t, out float3 cameraDir, out float3 oceanDir) 
{
	float h = _Ocean_CameraPos.z;
	float4 v = float4(vert.x, vert.y, 0.0, 1.0);
	cameraDir = normalize(mul(stoc, v).xyz); 			//Dir in camera space

	float3 n1= cross (sphereDir, cameraDir);			//Normal to plane containing dir to planet and vertex viewdir
	float3 n2= normalize(cross (n1, sphereDir)); 			//upwards vector in plane space, plane containing CamO and cameraDir

	float3 hor=cosTheta*sphereDir+sinTheta*n2;

	cameraDir= ( (dot(n1,cross(hor,cameraDir)) >=0) && (h>=0)) ? hor : cameraDir ; //checking if viewdir is above horizon
	//This could probably be optimized

	oceanDir = mul(_Ocean_CameraToOcean, float4(cameraDir, 0.0)).xyz;    

	float cz = _Ocean_CameraPos.z;
	float dz = oceanDir.z;
	float radius = _Ocean_Radius;

	float b = dz * (cz + radius);
	float c = cz * (cz + 2.0 * radius);

#if !defined (UNDERWATER_ON)
	float tSphere = - b - sqrt(max(b * b - c, 0.02));
#else
	float tSphere = - b + sqrt(max(b * b - c, 0.02));
#endif
	float tApprox = - cz / dz * (1.0 + cz / (2.0 * radius) * (1.0 - dz * dz));
	t = abs((tApprox - tSphere) * dz) < 1.0 ? tApprox : tSphere;


	return _Ocean_CameraPos.xy + t * oceanDir.xy;
}


float2 OceanPos(float4 vert, float4x4 stoc) 
{
	float t;
	float3 cameraDir;
	float3 oceanDir;
	return OceanPos(vert, stoc, t, cameraDir, oceanDir);
}

float4 Tex2DGrad(sampler2D tex, float2 uv, float2 dx, float2 dy, float2 texSize)
{
	//Sampling a texture by derivatives in unsupported in vert shaders in Unity but if you
	//can manually calculate the derivates you can reproduce its effect using tex2Dlod 
	float2 px = texSize.x * dx;
	float2 py = texSize.y * dy;
	float lod = 0.5 * log2(max(dot(px, px), dot(py, py)));
	return tex2Dlod(tex, float4(uv, 0, lod));
}