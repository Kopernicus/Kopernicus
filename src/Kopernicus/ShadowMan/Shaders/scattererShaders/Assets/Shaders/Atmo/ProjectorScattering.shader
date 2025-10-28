Shader "Scatterer/AtmosphericLocalScatter" {
	SubShader {
		Tags {"Queue" = "Transparent-497" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		//extinction pass
		Pass {
			Tags {"Queue" = "Transparent-497" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			//Cull Front
			Cull Back
			ZTest LEqual
			ZWrite Off

			Blend DstColor Zero //multiplicative
			Offset 0.0, -0.07

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"

			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON

			uniform float _global_alpha;
			uniform float _global_depth;
			uniform float3 _planetPos; //planet origin, in world space
			uniform float3 _camForward; //camera's viewing direction, in world space

			uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

			uniform float _Post_Extinction_Tint;
			uniform float extinctionThickness;

			uniform float _openglThreshold;
			uniform float4x4 _Globals_CameraToWorld;

			//            //eclipse uniforms
			//#if defined (ECLIPSES_ON)			
			//			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			//			uniform float4x4 lightOccluders1; //array of light occluders
			//											 //for each float4 xyz pos w radius
			//			uniform float4x4 lightOccluders2;
			//#endif

			#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
			#endif

			struct v2f
			{
				float4 pos: SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float3 _camPos : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
				worldPos.xyz/=worldPos.w; //needed?

#if defined (CUSTOM_OCEAN_ON)
				worldPos.xyz = (_PlanetOpacity < 1.0) && (length(worldPos.xyz-_planetPos) < Rg) ? _planetPos+Rg* normalize(worldPos.xyz-_planetPos)  : worldPos.xyz;
#endif

				o.worldPos = float4(worldPos.xyz,1.0);
				o.pos = mul (UNITY_MATRIX_VP,o.worldPos);
				o._camPos = _WorldSpaceCameraPos - _planetPos;

				return o;
			}


			half4 frag(v2f i) : SV_Target
			{
				float3 worldPos = i.worldPos.xyz/i.worldPos.w - _planetPos; //worldPos relative to planet origin

				half returnPixel = ((  (length(i._camPos)-Rg) < 1000 )  && (length(worldPos) < (Rg-50))) ? 0.0: 1.0;  //enable in case of ocean and close enough to water surface, works well for kerbin


				worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

				float3 extinction = getExtinction(i._camPos, worldPos, 1.0, 1.0, 1.0);
				float average=(extinction.r+extinction.g+extinction.b)/3;

				//lerped manually because of an issue with opengl or whatever
				extinction = _Post_Extinction_Tint * extinction + (1-_Post_Extinction_Tint) * float3(average,average,average);

				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );
				extinction = (returnPixel == 1.0) ? extinction : float3(1.0,1.0,1.0);
				return float4(extinction,1.0);
			}
			ENDCG
		}


		//scattering pass
		Pass {
			Tags {"Queue" = "Transparent-498" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			//Cull Front
			Cull Back
			ZTest LEqual

			ZWrite [_ZwriteVariable]

			Blend OneMinusDstColor One //soft additive
			//Blend SrcAlpha OneMinusSrcAlpha //alpha
			Offset 0.0, -0.07

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"
			#include "Godrays/GodraysCommon.cginc"

			//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON
			#pragma multi_compile DITHERING_OFF DITHERING_ON
			#pragma multi_compile GODRAYS_OFF GODRAYS_ON

			uniform float _global_alpha;
			uniform float _global_depth;
			uniform float3 _planetPos; //planet origin, in world space
			uniform float3 _camForward; //camera's viewing direction, in world space
			uniform float _ScatteringExposure;

			uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

#if defined (GODRAYS_ON)
			uniform sampler2D _godrayDepthTexture;
			uniform float _godrayStrength;
#endif
			uniform float _openglThreshold;
			uniform float4x4 _Globals_CameraToWorld;

			//            //eclipse uniforms
			//#if defined (ECLIPSES_ON)			
			//			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			//			uniform float4x4 lightOccluders1; //array of light occluders
			//											 //for each float4 xyz pos w radius
			//			uniform float4x4 lightOccluders2;
			//#endif

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

			struct v2f
			{
				float4 worldPos : TEXCOORD0;
				float3 _camPos  : TEXCOORD1;
#if defined (GODRAYS_ON)
				float4 projPos  : TEXCOORD2;
#endif

			};

			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f o;

				float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
				worldPos.xyz/=worldPos.w; //needed?

				//display scattering at ocean level when we are fading out local shading
				//at the same time ocean stops rendering it's own scattering
#if defined (CUSTOM_OCEAN_ON)
				worldPos.xyz = (_PlanetOpacity < 1.0) && (length(worldPos.xyz-_planetPos) < Rg) ? _planetPos+Rg* normalize(worldPos.xyz-_planetPos)  : worldPos.xyz;
#endif

				o.worldPos = float4(worldPos.xyz,1.0);
				o.worldPos.xyz*=worldPos.w;
				outpos = mul (UNITY_MATRIX_VP,o.worldPos);

				o._camPos = _WorldSpaceCameraPos - _planetPos;

#if defined (GODRAYS_ON)
				o.projPos = ComputeScreenPos(outpos);
#endif

				return o;
			}

			half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float3 worldPos = i.worldPos.xyz/i.worldPos.w - _planetPos; //worldPos relative to planet origin

				half returnPixel = ((  (length(i._camPos)-Rg) < 1000 )  && (length(worldPos) < (Rg-50))) ? 0.0: 1.0;  //enable in case of ocean and close enough to water surface, works well for kerbin

				float3 groundPos = normalize (worldPos) * Rg*1.0008;
				float Rt2 = Rg + (Rt - Rg) * _experimentalAtmoScale;
				worldPos = (length(worldPos) < Rt2) ? lerp(groundPos,worldPos,_PlanetOpacity) : worldPos; //fades to flatScaledSpace planet shading to ease the transition to scaledSpace

				worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

				float minDistance = length(worldPos-i._camPos);
				float3 inscatter=0.0;
				float3 extinction=1.0;

				//TODO: put planetshine stuff in callable function
#if defined (PLANETSHINE_ON)
				for (int j=0; j<4;++j)
				{
					if (planetShineRGB[j].w == 0) break;

					float intensity=1;  
					if (planetShineSources[j].w != 1.0f)
					{
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[j].xyz - worldPos),SUN_DIR)),0); //if source is not a sun compute intensity of light from angle to light source
						//totally made up formula by eyeballing it
					}

					inscatter+=InScattering2(i._camPos, worldPos, normalize(planetShineSources[j].xyz),extinction)  //lot of potential extinction recomputations here
					*planetShineRGB[j].xyz*planetShineRGB[j].w*intensity;
				}
#endif

#if defined (GODRAYS_ON)
				float2 depthUV = i.projPos.xy/i.projPos.w;
				float godrayDepth = 0.0;

				godrayDepth = sampleGodrayDepth(_godrayDepthTexture, depthUV, 1.0);

				//trying to find the optical depth from the terrain level
				float muTerrain = dot(normalize(i.worldPos.xyz/i.worldPos.w - _planetPos), normalize(_WorldSpaceCameraPos - i.worldPos.xyz/i.worldPos.w));

				godrayDepth = _godrayStrength * DistanceFromOpticalDepth(_experimentalAtmoScale * (Rt-Rg) * 0.5, length(i.worldPos.xyz/i.worldPos.w - _planetPos), muTerrain, godrayDepth, minDistance);

				worldPos -= godrayDepth * normalize(worldPos-i._camPos);
#endif

				inscatter+= InScattering2(i._camPos, worldPos,SUN_DIR,extinction);
				inscatter*= (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ; //somehow the shader compiler for OpenGL behaves differently around braces

				//#if defined (ECLIPSES_ON)				
				// 				float eclipseShadow = 1;
				// 							
				//            	for (int i=0; i<4; ++i)
				//    			{
				//        			if (lightOccluders1[i].w <= 0)	break;
				//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
				//								   lightOccluders1[i].w, sunPosAndRadius.w)	;
				//				}
				//						
				//				for (int j=0; j<4; ++j)
				//    			{
				//        			if (lightOccluders2[j].w <= 0)	break;
				//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
				//								   lightOccluders2[j].w, sunPosAndRadius.w)	;
				//				}
				//
				//				inscatter*=eclipseShadow;
				//#endif
				inscatter = hdr(inscatter,_ScatteringExposure) *_global_alpha;

				return float4(dither(inscatter,screenPos)*returnPixel,1.0);
			}
			ENDCG
		}
	}

}