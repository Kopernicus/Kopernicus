
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
*/

/**
* Real-time Realistic Ocean Lighting using Seamless Transitions from Geometry to BRDF
* Copyright (c) 2009 INRIA
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions
* are met:
* 1. Redistributions of source code must retain the above copyright
*    notice, this list of conditions and the following disclaimer.
* 2. Redistributions in binary form must reproduce the above copyright
*    notice, this list of conditions and the following disclaimer in the
*    documentation and/or other materials provided with the distribution.
* 3. Neither the name of the copyright holders nor the names of its
*    contributors may be used to endorse or promote products derived from
*    this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
* LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
* CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
	* SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
	* INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
* THE POSSIBILITY OF SUCH DAMAGE.
*/

/**
* Author: Eric Bruneton
* Modified and ported to Unity by Justin Hawkins 2014
* Modified and adapted for use with Kerbal Space Program by Ghassen Lahmar 2015-2020
	*/

	Shader "Scatterer/OceanWhiteCapsPixelLights" 
{
	SubShader 
	{
		Tags { "Queue" = "Geometry+100"
				"RenderType"="Transparent"
				"IgnoreProjector"="True"}

		Pass   
		{

			Tags { "LightMode" = "MainPass"
					"Queue" = "Geometry+100"
					"RenderType"="Transparent"
					"IgnoreProjector"="True"}

			Blend SrcAlpha OneMinusSrcAlpha
			Offset 0.0, -0.14

			Cull Back

			ZWrite [_ZwriteVariable]

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			//#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile SKY_REFLECTIONS_OFF SKY_REFLECTIONS_ON
			#pragma multi_compile UNDERWATER_OFF UNDERWATER_ON
			#pragma multi_compile OCEAN_SHADOWS_OFF OCEAN_SHADOWS_HARD OCEAN_SHADOWS_SOFT
			#pragma multi_compile REFRACTIONS_AND_TRANSPARENCY_OFF REFRACTIONS_AND_TRANSPARENCY_ON
			#pragma multi_compile SCATTERER_MERGED_DEPTH_ON SCATTERER_MERGED_DEPTH_OFF
			#pragma multi_compile DITHERING_OFF DITHERING_ON
			#pragma multi_compile GODRAYS_OFF GODRAYS_ON
			#pragma multi_compile DEPTH_BUFFER_MODE_OFF DEPTH_BUFFER_MODE_ON
			#pragma multi_compile FOAM_OFF FOAM_ON
			//#pragma multi_compile SCATTERING_ON SCATTERING_OFF

			#include "../CommonAtmosphere.cginc"
			#include "../DepthCommon.cginc"
			#if defined (OCEAN_SHADOWS_HARD) || defined (OCEAN_SHADOWS_SOFT)
			#include "OceanShadows.cginc"
			#endif			
			#include "OceanBRDF.cginc"
			#include "OceanDisplacement3.cginc"
			#include "OceanUtils.cginc"
			#include "../ClippingUtils.cginc"
			#include "../Atmo/Godrays/GodraysCommon.cginc"

			uniform float offScreenVertexStretch;

			uniform float4x4 _Globals_ScreenToCamera;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_WorldToScreen;
			uniform float4x4 _Globals_CameraToScreen;
			uniform float3 _Globals_WorldCameraPos;

			uniform float4x4 _Globals_WorldToOcean;
			uniform float4x4 _Globals_OceanToWorld;

			uniform float3 _Sun_WorldSunDir;

			uniform float2 _Ocean_MapSize;
			uniform float4 _Ocean_Choppyness;
			uniform float3 _Ocean_SunDir;
			uniform float4 _Ocean_GridSizes;
			uniform float2 _Ocean_ScreenGridSize;
			uniform float _Ocean_WhiteCapStr;
			uniform float farWhiteCapStr;
			uniform float shoreFoam;

			uniform sampler3D _Ocean_Variance;
			uniform sampler2D _Ocean_Map0;
			uniform sampler2D _Ocean_Map1;
			uniform sampler2D _Ocean_Map2;
			uniform sampler2D _Ocean_Map3;
			uniform sampler2D _Ocean_Map4;
			uniform sampler2D _Ocean_Foam0;
			uniform sampler2D _Ocean_Foam1;

			uniform float alphaRadius;
			uniform float _PlanetOpacity;  //to fade out the ocean when PQS is fading out
			uniform float _ScatteringExposure;

			uniform float _Alpha_Global;
			uniform float _SkyExposure;

			uniform float2 _VarianceMax;

			uniform float darknessDepth;

			uniform float3 _planetPos;
			uniform float _openglThreshold;
			uniform float _global_depth;
			uniform float _global_alpha;
			uniform float _Post_Extinction_Tint;
			uniform float extinctionThickness;

			#if defined (REFRACTIONS_AND_TRANSPARENCY_ON)
			uniform sampler2D ScattererScreenCopyBeforeOcean;   //background texture used for refraction
			#endif

			#if defined (GODRAYS_ON)
			uniform sampler2D _godrayDepthTexture;
			uniform float _godrayStrength;
			#endif

			#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
			#endif

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 oceanU	: TEXCOORD0;
				float3 oceanP	: TEXCOORD1;
				float4 screenPos: TEXCOORD2;
				float4 worldPos	: TEXCOORD3;
				float4 viewPos	: TEXCOORD4;
				float3 sunL	: TEXCOORD5;
				float3 skyE	: TEXCOORD6;
			};

			v2f vert(appdata_base v)
			{
				float t;
				float3 cameraDir, oceanDir;
				float4 vert = v.vertex;
				vert.xy *= offScreenVertexStretch;

				float2 u = OceanPos(vert, _Globals_ScreenToCamera, t, cameraDir, oceanDir);	//camera dir is viewing direction in camera space
				float2 dux = OceanPos(vert + float4(_Ocean_ScreenGridSize.x, 0.0, 0.0, 0.0), _Globals_ScreenToCamera) - u;
				float2 duy = OceanPos(vert + float4(0.0, _Ocean_ScreenGridSize.y, 0.0, 0.0), _Globals_ScreenToCamera) - u;

				float3 dP = float3(0, 0, _Ocean_HeightOffset);

				if(duy.x != 0.0 || duy.y != 0.0)
				{
					dP.z += Tex2DGrad(_Ocean_Map0, u / _Ocean_GridSizes.x, dux / _Ocean_GridSizes.x, duy / _Ocean_GridSizes.x, _Ocean_MapSize).x;
					dP.z += Tex2DGrad(_Ocean_Map0, u / _Ocean_GridSizes.y, dux / _Ocean_GridSizes.y, duy / _Ocean_GridSizes.y, _Ocean_MapSize).y;
					dP.z += Tex2DGrad(_Ocean_Map0, u / _Ocean_GridSizes.z, dux / _Ocean_GridSizes.z, duy / _Ocean_GridSizes.z, _Ocean_MapSize).z;
					dP.z += Tex2DGrad(_Ocean_Map0, u / _Ocean_GridSizes.w, dux / _Ocean_GridSizes.w, duy / _Ocean_GridSizes.w, _Ocean_MapSize).w;
					//
					dP.xy += _Ocean_Choppyness.x * Tex2DGrad(_Ocean_Map3, u / _Ocean_GridSizes.x, dux / _Ocean_GridSizes.x, duy / _Ocean_GridSizes.x, _Ocean_MapSize).xy;
					dP.xy += _Ocean_Choppyness.y * Tex2DGrad(_Ocean_Map3, u / _Ocean_GridSizes.y, dux / _Ocean_GridSizes.y, duy / _Ocean_GridSizes.y, _Ocean_MapSize).zw;
					dP.xy += _Ocean_Choppyness.z * Tex2DGrad(_Ocean_Map4, u / _Ocean_GridSizes.z, dux / _Ocean_GridSizes.z, duy / _Ocean_GridSizes.z, _Ocean_MapSize).xy;
					dP.xy += _Ocean_Choppyness.w * Tex2DGrad(_Ocean_Map4, u / _Ocean_GridSizes.w, dux / _Ocean_GridSizes.w, duy / _Ocean_GridSizes.w, _Ocean_MapSize).zw;
				}

				v2f OUT;

				float3x3 otoc = _Ocean_OceanToCamera;
				float tClamped = clamp(t*0.25, 0.0, 1.0);

				#if defined (UNDERWATER_ON)
				dP = lerp(float3(0.0,0.0,0.1),dP,tClamped);   //prevents projected grid intersecting near plane
				#else
				dP = lerp(float3(0.0,0.0,-0.1),dP,tClamped);  //prevents projected grid intersecting near plane
				#endif
				float4 screenP = float4(t * cameraDir + mul(otoc, dP), 1.0);   //position in camera space
				float3 oceanP = t * oceanDir + dP + float3(0.0, 0.0, _Ocean_CameraPos.z);

				OUT.pos = mul(UNITY_MATRIX_P, screenP);


				OUT.oceanU = u;
				OUT.oceanP = oceanP;

				OUT.screenPos = ComputeScreenPos(OUT.pos);
				OUT.worldPos=mul(_Globals_CameraToWorld , screenP);

				OUT.viewPos = screenP;
				#if SHADER_API_D3D11 || SHADER_API_D3D9 || SHADER_API_D3D || SHADER_API_D3D12
				OUT.pos	= (_PlanetOpacity == 0.0) ? float4(2.0, 2.0, 2.0, 1.0) : OUT.pos;		//cull when completely transparent
				#else
				OUT.pos = lerp(float4(2.0, 2.0, 2.0, 1.0), OUT.pos, step(0.001,_PlanetOpacity));	//stupid opengl
				#endif

				float3 earthP = normalize(oceanP + float3(0.0, 0.0, _Ocean_Radius)) * (_Ocean_Radius + 10.0);

				float3 sunL, skyE;
				SunRadianceAndSkyIrradiance(earthP, float3(0.0,0.0,1.0), _Ocean_SunDir, sunL, skyE);
				OUT.sunL = sunL;
				OUT.skyE = skyE;

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				float2 u = IN.oceanU;
				float3 oceanP = IN.oceanP;

				float3 oceanCamera = float3(0.0, 0.0, _Ocean_CameraPos.z);

				float3 V = normalize(oceanCamera - oceanP);

				float2 slopes = float2(0,0);
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.x).xy;
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.y).zw;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.z).xy;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.w).zw;

				slopes -= oceanP.xy / (_Ocean_Radius + oceanP.z);

				float3 N = normalize(float3(-slopes.x, -slopes.y, 1.0));

				float Jxx = ddx(u.x);
				float Jxy = ddy(u.x);
				float Jyx = ddx(u.y);
				float Jyy = ddy(u.y);
				float A = Jxx * Jxx + Jyx * Jyx;
				float B = Jxx * Jxy + Jyx * Jyy;
				float C = Jxy * Jxy + Jyy * Jyy;
				const float SCALE = 10.0;
				float ua = pow(A / SCALE, 0.25);
				float ub = 0.5 + 0.5 * B / sqrt(A * C);
				float uc = pow(C / SCALE, 0.25);
				float sigmaSq = tex3D(_Ocean_Variance, float3(ua, ub, uc)).x * _VarianceMax.x;

				sigmaSq = max(sigmaSq, 2e-5);

				#if defined (FOAM_ON)
				// extract mean and variance of the jacobian matrix determinant
				float2 jm1 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.x).xy;
				float2 jm2 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.y).zw;
				float2 jm3 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.z).xy;
				float2 jm4 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.w).zw;

				float2 jm  = jm1+jm2+jm3+jm4;
				float jSigma2 = max(jm.y - (jm1.x*jm1.x + jm2.x*jm2.x + jm3.x*jm3.x + jm4.x*jm4.x), 0.0);
				#endif

				float3 earthP = normalize(oceanP + float3(0.0, 0.0, _Ocean_Radius)) * (_Ocean_Radius + 10.0);

				float3 sunL = IN.sunL;
				float3 skyE = IN.skyE;

				half shadowTerm = 1.0;
				#if defined (OCEAN_SHADOWS_HARD) || defined (OCEAN_SHADOWS_SOFT)
				shadowTerm = getOceanShadow (IN.worldPos, -IN.viewPos.z);
				#endif

				float fresnel = getFresnel(V, N, sigmaSq);
				float3 Lsky = getSkyColor(fresnel, V, N, _Ocean_SunDir, earthP, skyE, shadowTerm, _Ocean_Radius);
				float3 Lsea = getOceanColor(fresnel, V, N, _Ocean_SunDir, earthP, skyE, shadowTerm);

				float oceanDistance = length(IN.viewPos);

				#if defined (REFRACTIONS_AND_TRANSPARENCY_ON)
				float fragDistance, depth;
				float2 uv = IN.screenPos.xy / IN.screenPos.w;

				#if SHADER_API_D3D11 || SHADER_API_D3D9 || SHADER_API_D3D || SHADER_API_D3D12
				uv.y = 1.0 - uv.y;
				#endif
				uv = getPerturbedUVsAndDepth(uv, N, oceanDistance, fragDistance, depth);

				_Ocean_WhiteCapStr = adjustWhiteCapStrengthWithDepth(_Ocean_WhiteCapStr, shoreFoam, depth);
				float transparencyAlpha = getTransparencyAlpha(depth);
				#else
				float transparencyAlpha=1.0;
				#endif

				float3 R_ftot = float3(0.0,0.0,0.0);
#if defined (FOAM_ON)
				float outWhiteCapStr=applyFarWhiteCapStrength(oceanDistance, alphaRadius, _Ocean_WhiteCapStr, farWhiteCapStr);
				R_ftot = getTotalWhiteCapRadiance(outWhiteCapStr, jm.x, jSigma2, sunL, N, _Ocean_SunDir, skyE, shadowTerm);
#endif

				float3 Lsun = ReflectedSunRadiance(_Ocean_SunDir, V, N, sigmaSq) * sunL * shadowTerm;

				#if defined (UNDERWATER_ON)
				float3 surfaceColor = hdr(abs(Lsea + R_ftot),_ScatteringExposure);
				#else
				float3 surfaceColor = hdr(abs(Lsun + Lsea + R_ftot),_ScatteringExposure);
				Lsky = _Alpha_Global*hdr(Lsky,_SkyExposure);
				surfaceColor = surfaceColor + (1.0-surfaceColor) * Lsky;
				#endif
				float3 LsunTotal = Lsun; float3 R_ftotTotal = R_ftot; float3 LseaTotal = Lsea; float3 LskyTotal = Lsky;
				#if defined (PLANETSHINE_ON)

				getPlanetShineContribution(LsunTotal, R_ftotTotal, LseaTotal, LskyTotal);
				#endif

				#if SHADER_API_D3D11
				bool insideClippingRange = true;
				#else
				bool insideClippingRange = oceanFragmentInsideOfClippingRange(-IN.viewPos.z/IN.viewPos.w);
				#endif

				#if defined (REFRACTIONS_AND_TRANSPARENCY_ON)
				transparencyAlpha = max(hdr(LsunTotal + R_ftotTotal,_ScatteringExposure), fresnel+transparencyAlpha) ; //seems about perfect
				transparencyAlpha = min(transparencyAlpha, 1.0);

				float3 backGrnd = 0.0;

				//	#if defined (DEPTH_BUFFER_MODE_ON)
				//				backGrnd = tex2D(ScattererScreenCopyBeforeOcean, uv );
				//	#else
				#if SHADER_API_D3D11 || SHADER_API_D3D9 || SHADER_API_D3D || SHADER_API_D3D12
				backGrnd = tex2D(ScattererScreenCopyBeforeOcean, (_ProjectionParams.x == 1.0) ? uv : float2(uv.x,1.0-uv.y) );
				#else
				backGrnd = tex2D(ScattererScreenCopyBeforeOcean, uv );
				#endif
				//	#endif

				#endif

				#if defined (UNDERWATER_ON)
				Lsky = _Alpha_Global*hdr(Lsky,_SkyExposure);
				float3 transmittance =  hdr(R_ftot, _ScatteringExposure);
				transmittance = clamp(transmittance, float3(0.0,0.0,0.0), float3(1.0,1.0,1.0));
				transmittance = transmittance + (1.0-transmittance) * Lsky;

				float3 finalColor = lerp(transmittance,Lsea, 1-fresnel);	//consider not using transmittance but instead background texture, change the refraction angle to have something matching what you would see from underwater


				#if defined (REFRACTIONS_AND_TRANSPARENCY_ON)
				backGrnd+=hdr(R_ftotTotal,_ScatteringExposure)*(1-backGrnd); //make foam visible from below as well
				finalColor = (fragDistance < 750000.0) ? backGrnd : finalColor;
				#endif

				float3 Vworld = mul ( _Globals_OceanToWorld, float4(V,0.0));
				float3 Lworld = mul ( _Globals_OceanToWorld, float4(_Ocean_SunDir,0.0));

				float3 earthCamPos = normalize(float3(_Ocean_CameraPos.xy,0.0) + float3(0.0, 0.0, _Ocean_Radius)) * (_Ocean_Radius + 10.0);

				float underwaterDepth = lerp(1.0,0.0,-_Ocean_CameraPos.z / darknessDepth);

				float waterLightExtinction = length(getSkyExtinction(earthCamPos, _Ocean_SunDir));
				float3 _camPos = _WorldSpaceCameraPos - _planetPos;

				float3 oceanCol = underwaterDepth * hdrNoExposure(waterLightExtinction * _sunColor * oceanColor(-Vworld,Lworld,normalize(_camPos))); //add planetshine loop here over Ls

				finalColor= clamp(finalColor, float3(0.0,0.0,0.0),float3(1.0,1.0,1.0));
				finalColor= lerp(finalColor, oceanCol, min(length(oceanCamera - oceanP)/transparencyDepth,1.0));

				return float4(dither(finalColor, IN.pos),insideClippingRange);
				#else
				#if defined (REFRACTIONS_AND_TRANSPARENCY_ON)
				float3 finalColor = lerp(backGrnd, surfaceColor, transparencyAlpha);	//refraction on and not underwater
				#else
				float3 finalColor = surfaceColor;  					//refraction off and not underwater
				#endif

				#if defined (DEPTH_BUFFER_MODE_OFF)
				if (_PlanetOpacity == 1.0)
				{
				float3 worldPos= IN.worldPos - _planetPos;
				worldPos = (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix
				float3 _camPos = _WorldSpaceCameraPos - _planetPos;

				float minDistance = length(worldPos-_camPos);

				#if defined (GODRAYS_ON)
				float2 godrayUV = IN.screenPos.xy / IN.screenPos.w;
				float godrayDepth = 0.0;
				godrayDepth = sampleGodrayDepth(_godrayDepthTexture, godrayUV, 1.0);

				//trying to find the optical depth from the terrain level
				float muTerrain = dot(normalize(worldPos), normalize(_camPos - worldPos));

				godrayDepth = _godrayStrength * DistanceFromOpticalDepth(_experimentalAtmoScale * (Rt-Rg) * 0.5, length(worldPos), muTerrain, godrayDepth, minDistance);

				worldPos = worldPos - godrayDepth * normalize(worldPos-_camPos);
				#endif

				float3 inscatter=0.0;float3 extinction=1.0;
				inscatter = InScattering2(_camPos, worldPos,SUN_DIR,extinction);

				inscatter*= (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ; //somehow the shader compiler for OpenGL behaves differently around braces            				
				inscatter = hdr(inscatter,_ScatteringExposure) *_global_alpha;

				float average=(extinction.r+extinction.g+extinction.b)/3;

				//lerped manually because of an issue with opengl or whatever
				extinction = _Post_Extinction_Tint * extinction + (1-_Post_Extinction_Tint) * float3(average,average,average);

				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

				finalColor*= extinction;
				finalColor = inscatter*(1-finalColor) + finalColor;
				}
				#endif

				insideClippingRange = (transparencyAlpha == 1.0) ? 1.0 : insideClippingRange;     //if no transparency -> render normally, if transparency play with the overlap to hide seams between cameras
				return float4(dither(finalColor,IN.pos), _PlanetOpacity*insideClippingRange);
				#endif
			}

			ENDCG
		}


		Pass   //forward Add
		{
			Tags { "LightMode" = "ForwardAdd" } 


			Blend One OneMinusSrcColor //"reverse" soft-additive
			Offset 0.0, -0.14 //give a superior depth offset to that of localScattering to prevent scattering that should be behind ocean from appearing in front

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile_fwdadd
//			#pragma multi_compile FOAM_OFF FOAM_ON

			#include "../Utility.cginc"
			//#include "AtmosphereNew.cginc"
			#include "OceanBRDF.cginc"
			#include "OceanDisplacement3.cginc"

			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "OceanLight.cginc"

			uniform float offScreenVertexStretch;

			uniform float4x4 _Globals_ScreenToCamera;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_WorldToScreen;
			uniform float4x4 _Globals_CameraToScreen;
			uniform float4x4 _Globals_WorldToOcean;
			uniform float4x4 _Globals_OceanToWorld;
			uniform float3 _Globals_WorldCameraPos;

			uniform float2 _Ocean_MapSize;
			uniform float4 _Ocean_Choppyness;
			uniform float3 _Ocean_SunDir;
			uniform float3 _Ocean_Color;
			uniform float4 _Ocean_GridSizes;
			uniform float2 _Ocean_ScreenGridSize;
			uniform float _Ocean_WhiteCapStr;
			uniform float farWhiteCapStr;

			uniform sampler3D _Ocean_Variance;
			uniform sampler2D _Ocean_Map0;
			uniform sampler2D _Ocean_Map1;
			uniform sampler2D _Ocean_Map2;
			uniform sampler2D _Ocean_Map3;
			uniform sampler2D _Ocean_Map4;
			uniform sampler2D _Ocean_Foam0;
			uniform sampler2D _Ocean_Foam1;

			uniform float alphaRadius;
			uniform float _ScatteringExposure;
			uniform float _PlanetOpacity;  //to fade out the ocean when PQS is fading out

			uniform float2 _VarianceMax;

			uniform sampler2D _Sky_Map;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  oceanU : TEXCOORD0;
				float3  oceanP : TEXCOORD1;
				LIGHTING_COORDS(2,3)
			};

			v2f vert(appdata_base v)
			{
				float t;
				float3 cameraDir, oceanDir;
				float4 vert = v.vertex;
				vert.xy *= offScreenVertexStretch;

				float2 u = OceanPos(vert, _Globals_ScreenToCamera, t, cameraDir, oceanDir);		//camera dir is viewing direction in camera space
				float2 dux = OceanPos(vert + float4(_Ocean_ScreenGridSize.x, 0.0, 0.0, 0.0), _Globals_ScreenToCamera) - u;
				float2 duy = OceanPos(vert + float4(0.0, _Ocean_ScreenGridSize.y, 0.0, 0.0), _Globals_ScreenToCamera) - u;

				float3 dP = float3(0, 0, _Ocean_HeightOffset);

				if(duy.x != 0.0 || duy.y != 0.0) 
				{
					float4 GRID_SIZES = _Ocean_GridSizes;
					float4 CHOPPYNESS = _Ocean_Choppyness;

					dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).x;
					dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).y;
					dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).z;
					dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).w;

					dP.xy += CHOPPYNESS.x * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).xy;
					dP.xy += CHOPPYNESS.y * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).zw;
					dP.xy += CHOPPYNESS.z * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).xy;
					dP.xy += CHOPPYNESS.w * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).zw;
				}

				v2f OUT;

				float3x3 otoc = _Ocean_OceanToCamera;
				float tClamped = clamp(t*0.25, 0.0, 1.0);

				dP = lerp(float3(0.0,0.0,-0.1),dP,tClamped);  //prevents projected grid intersecting near plane

				float4 screenP = float4(t * cameraDir + mul(otoc, dP), 1.0);   //position in camera space?
				float3 oceanP = t * oceanDir + dP + float3(0.0, 0.0, _Ocean_CameraPos.z);

				OUT.pos = mul(_Globals_CameraToScreen, screenP);
				OUT.pos.y = OUT.pos.y *_ProjectionParams.x;

				OUT.oceanU = u;
				OUT.oceanP = oceanP;

				float4 worldPos=mul(_Globals_CameraToWorld , screenP);
				OCEAN_TRANSFER_VERTEX_TO_FRAGMENT(OUT);

				OUT.pos	= (_PlanetOpacity == 1.0) ? OUT.pos : float4(2.0, 2.0, 2.0, 1.0) ; //cull when planet starts fading out
						
				return OUT;
			}


			float4 frag(v2f IN) : COLOR
			{
				float radius = _Ocean_Radius;
				float2 u = IN.oceanU;
				float3 oceanP = IN.oceanP;

				float3 earthCamera = float3(0.0, 0.0, _Ocean_CameraPos.z + radius); 

				float3 earthP = normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 10.0); 

				float dist=length(earthP-earthCamera);

				float clampFactor= clamp(dist/alphaRadius,0.0,1.0);			

				float outWhiteCapStr=lerp(_Ocean_WhiteCapStr,farWhiteCapStr,clampFactor);

				float3 oceanCamera = float3(0.0, 0.0, _Ocean_CameraPos.z);
				float3 V = normalize(oceanCamera - oceanP);

				float2 slopes = float2(0,0);
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.x).xy;
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.y).zw;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.z).xy;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.w).zw;

				slopes -= oceanP.xy / (radius + oceanP.z);

				float3 N = normalize(float3(-slopes.x, -slopes.y, 1.0));

				if (dot(V, N) < 0.0) {
					N = reflect(N, V); // reflects backfacing normals
				}

				float Jxx = ddx(u.x);
				float Jxy = ddy(u.x);
				float Jyx = ddx(u.y);
				float Jyy = ddy(u.y);
				float A = Jxx * Jxx + Jyx * Jyx;
				float B = Jxx * Jxy + Jyx * Jyy;
				float C = Jxy * Jxy + Jyy * Jyy;
				const float SCALE = 10.0;
				float ua = pow(A / SCALE, 0.25);
				float ub = 0.5 + 0.5 * B / sqrt(A * C);
				float uc = pow(C / SCALE, 0.25);
				//			    float sigmaSq = tex3D(_Ocean_Variance, float3(ua, ub, uc)).x;
				float2 sigmaSq = tex3D(_Ocean_Variance, float3(ua, ub, uc)).xy * _VarianceMax;

				sigmaSq = max(sigmaSq, 2e-5);

				float atten=LIGHT_ATTENUATION(IN)*15;

				float3 Lsky;
				Lsky = MeanFresnel(V, N, sigmaSq) * atten / M_PI;

				float3 oceanL= mul(_Globals_WorldToOcean, _WorldSpaceLightPos0);
				float3 L = normalize(oceanL - oceanP); //light dir in ocean space, find it
				float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * atten;

				float3 Lsea = RefractedSeaRadiance(V, N, sigmaSq) * _Ocean_Color * atten;

//#if defined (FOAM_ON)
//				// extract mean and variance of the jacobian matrix determinant
//				float2 jm1 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.x).xy;
//				float2 jm2 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.y).zw;
//				float2 jm3 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.z).xy;
//				float2 jm4 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.w).zw;
//				float2 jm  = jm1+jm2+jm3+jm4;
//				float jSigma2 = max(jm.y - (jm1.x*jm1.x + jm2.x*jm2.x + jm3.x*jm3.x + jm4.x*jm4.x), 0.0);
//
//				// get coverage
//				float W = WhitecapCoverage(outWhiteCapStr,jm.x,jSigma2);
//
//				// compute and add whitecap radiance
//				//				float3 l = (atten * (max(dot(N, L), 0.0))) / M_PI;
//				//				float3 R_ftot = float3(W * l * 0.4);
//#endif

				//float3 surfaceColor = (Lsun + Lsky + Lsea + R_ftot) * _LightColor0.rgb;
				float3 surfaceColor = (Lsun + Lsky + Lsea) * _LightColor0.rgb;

				float3 finalColor= surfaceColor;

				return float4(hdr(finalColor,_ScatteringExposure), 1.0);
			}

			ENDCG
		}
	}
}

