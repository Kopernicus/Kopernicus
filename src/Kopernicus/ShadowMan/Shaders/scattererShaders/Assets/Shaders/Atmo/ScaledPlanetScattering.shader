Shader "Scatterer/ScaledPlanetScattering" {
	SubShader 
	{
		Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

		Pass {  //extinction pass

			ZWrite Off
			ZTest LEqual
			Cull Back

			Offset -0.05, -0.05

			Blend DstColor Zero //multiplicative

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"
			#include "../EclipseCommon.cginc"
			#include "../RingCommon.cginc"		

			#define useAnalyticSkyTransmittance

			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
			#pragma multi_compile LOCAL_MODE_OFF LOCAL_MODE_ON

			uniform float _Alpha_Global;
			uniform float extinctionTint;
			uniform float extinctionThickness;
			uniform float3 _Sun_WorldSunDir;

			uniform float flatScaledSpaceModel;

			struct v2f
			{
				float4 pos: SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);

				//this is a hack to make the scaledScattering "fill in" for the local scattering when it gets cut off bu the limited clip plane
				//essentially render both scaled scattering and sky at max depth and check the z-buffer, that way only render where localScattering hasn't rendered
#if defined(LOCAL_MODE_ON)
	#ifdef SHADER_API_D3D11
				OUT.pos.z = 0.00000000000001;
	#else
				OUT.pos.z = (1.0 - 0.00000000000001) * OUT.pos.w;
				OUT.pos = ( _ProjectionParams.y > 200.0 ) ? OUT.pos : float4(2.0,2.0,2.0,1.0); //cull on near camera on OpenGL
	#endif
#endif

				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;

#if defined(LOCAL_MODE_OFF)
				OUT.worldPos *= 6000.0;
				OUT.planetOrigin *= 6000.0;
#endif


				return OUT;
			}

			half4 frag(v2f IN): COLOR
			{
				float3 extinction=0.0;

				float3 WCP = _WorldSpaceCameraPos;

#if defined(LOCAL_MODE_OFF)
				WCP*=6000.0;
#endif

				float3 planetSurfacePosition = IN.worldPos-IN.planetOrigin;
				float3 planetSurfaceScatteringPosition = (flatScaledSpaceModel == 1.0) ? normalize(planetSurfacePosition) * Rg * 1.0001 : planetSurfacePosition * (6005.0/6000.0); //1.0001 Rg and 6005*6000 to avoid some precision artifacts
				extinction = getExtinction((WCP-IN.planetOrigin), planetSurfaceScatteringPosition, 1.0, 1.0, 1.0);

				#if defined (ECLIPSES_ON)	
				extinction*= getEclipseShadows(IN.worldPos);
				#endif

				#if defined (RINGSHADOW_ON)
				extinction *= getLinearRingColor(IN.worldPos, _Sun_WorldSunDir, IN.planetOrigin).a;
				#endif
				float average=(extinction.r+extinction.g+extinction.b)/3;
				extinction = extinctionTint * extinction + (1-extinctionTint) * float3(average,average,average);
				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

				return float4(extinction,1.0);
			}
			ENDCG
		}

		Pass {  //scattering pass

			ZWrite On
			ZTest LEqual
			Cull Back

			Offset -0.05, -0.05

			Blend OneMinusDstColor One //soft additive

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"
			#include "../EclipseCommon.cginc"
			#include "../RingCommon.cginc"	

			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
			#pragma multi_compile LOCAL_MODE_OFF LOCAL_MODE_ON

			//#pragma fragmentoption ARB_precision_hint_nicest

			uniform float _Alpha_Global;

			uniform float3 _Sun_WorldSunDir;
			uniform float _ScatteringExposure;

			#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
			#endif

			//sampler2D _MainTex;
			uniform float flatScaledSpaceModel;

			struct v2f
			{
				float4 pos: SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);

#if defined(LOCAL_MODE_ON)
	#ifdef SHADER_API_D3D11
				OUT.pos.z = 0.00000000000001;
	#else
				OUT.pos.z = (1.0 - 0.00000000000001) * OUT.pos.w;
				OUT.pos = ( _ProjectionParams.y > 200.0 ) ? OUT.pos : float4(2.0,2.0,2.0,1.0); //cull on near camera on OpenGL
	#endif
#endif

				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;

#if defined(LOCAL_MODE_OFF)
				OUT.worldPos *= 6000.0;
				OUT.planetOrigin *= 6000.0;
#endif
				return OUT;
			}

			half4 frag(v2f IN): COLOR
			{
				float3 inscatter=0.0;
				float3 extinction=0.0;

				float3 WCP = _WorldSpaceCameraPos;
#if defined(LOCAL_MODE_OFF)
				WCP*=6000.0;
#endif

				float3 planetSurfacePosition = IN.worldPos-IN.planetOrigin;
				float3 planetSurfaceScatteringPosition = (flatScaledSpaceModel == 1.0) ? normalize(planetSurfacePosition) * Rg * 1.0001 : planetSurfacePosition * (6005.0/6000.0); //1.0001 Rg and 6005*6000 to avoid some precision artifacts
				inscatter= InScattering2((WCP-IN.planetOrigin), planetSurfaceScatteringPosition, _Sun_WorldSunDir,extinction);

				#if defined (ECLIPSES_ON)
				inscatter *= getEclipseShadows(IN.worldPos);
				#endif

				#if defined (RINGSHADOW_ON)
				inscatter *= getLinearRingColor(IN.worldPos, _Sun_WorldSunDir, IN.planetOrigin).a;
				#endif

				///////////////////PLANETSHINE///////////////////////////////						    
				//#if defined (PLANETSHINE_ON)
				//			    float3 inscatter2=0;
				//			   	float intensity=1;
				//			    for (int i=0; i<4; ++i)
				//    			{
				//    				if (planetShineRGB[i].w == 0) break;
				//    					
				//    				//if source is not a sun compute intensity of light from angle to light source
				//			   		intensity=1;  
				//			   		if (planetShineSources[i].w != 1.0f)
				//					{
				////						intensity = 0.5f*(1-dot(normalize(planetShineSources[i].xyz - worldPos),WSD));
				//						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - planetSurfacePosition),WSD)),0);
				//					}
				//				    				
				//    				inscatter2+=SkyRadiance3(WCP - IN.planetOrigin, d, normalize(planetShineSources[i].xyz))
				//    							*planetShineRGB[i].xyz*planetShineRGB[i].w*intensity;
				//    			}
				//			    
				//				finalColor+=inscatter2;
				//#endif
				/////////////////////////////////////////////////////////////	

				return float4(hdr(inscatter,_ScatteringExposure),1.0);
			}
			ENDCG
		}

	}
}