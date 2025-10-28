Shader "Scatterer/SkySphere" 
{
	SubShader 
	{
		Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

		Pass   	//extinction pass, I should really just put the shared components in an include file to clean this up
		{		
			Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }    	 	 		

			ZWrite Off
			cull Front

			Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"

			#pragma multi_compile LOCAL_SKY_OFF LOCAL_SKY_ON

			uniform float _Alpha_Global;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;

			uniform float _experimentalExtinctionScale;
			uniform float3 _Sun_WorldSunDir;

			uniform float extinctionThickness;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				//v.vertex.xyz*= (_experimentalExtinctionScale * (Rt-Rg)+ Rg) / Rt;
				OUT.pos = UnityObjectToClipPos(v.vertex);

				//this is a hack to make the sky and scaledScattering "fill in" for the local scattering when it gets cut off bu the limited clip plane
				//essentially render both scaled scattering and sky at max depth and check the z-buffer, that way only render where localScattering hasn't rendered
#if defined (LOCAL_SKY_ON)
	#ifdef SHADER_API_D3D11
				OUT.pos.z = 0.0;
	#else
				OUT.pos.z = OUT.pos.w;
				OUT.pos = ( _ProjectionParams.y > 200.0 ) ? OUT.pos : float4(2.0,2.0,2.0,1.0); //cull on near camera on OpenGL
	#endif
#endif

				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;
#if defined (LOCAL_SKY_OFF)
				OUT.planetOrigin *= 6000;  //all calculations are done in localSpace
#endif
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{

				float3 extinction = float3(1,1,1);

				float3 WCP = _WorldSpaceCameraPos;

#if defined (LOCAL_SKY_OFF)
				WCP *= 6000;
#endif

				float3 viewDir = normalize(IN.worldPos-_WorldSpaceCameraPos);

				//Rt=Rg+(Rt-Rg)*_experimentalExtinctionScale;

				float3 viewdir=normalize(viewDir);
				float3 camera=WCP - IN.planetOrigin;

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;
				float r0 = r;
				float mu0 = mu;

				float dSq = rMu * rMu - r * r + Rt*Rt;
				float deltaSq = sqrt(dSq);

				float din = max(-rMu - deltaSq, 0.0);

				if (din > 0.0)
				{
					camera += din * viewdir;
					rMu += din;
					mu = rMu / Rt;
					r = Rt;
				}

				if (r > Rt || dSq < 0.0)
				{
					return float4(1.0,1.0,1.0,1.0);
				} 

				extinction = Transmittance(r, mu);			//this doesn't scale correctly as the view angle would have to be adjusted in a non-trivial way, and even then, intersections with the ground in the precomputed radius won't be adjusted for
															//Best to switch to analyticTransmittance, with scaling of HR and HM with experimentalAtmoScale, and rescaling HR and HM when applying config to the scaled Body		

				float average=(extinction.r+extinction.g+extinction.b)/3;
				extinction = _Extinction_Tint * extinction + (1-_Extinction_Tint) * float3(average,average,average);

				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

				return float4(extinction,1.0);
			}

			ENDCG

		}



		Pass 	//inscattering pass
		{

			Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

			ZWrite Off
			cull Front

			//Blend One One  //additive blending
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
			#include "Godrays/GodraysCommon.cginc"

			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
			#pragma multi_compile DITHERING_OFF DITHERING_ON
			#pragma multi_compile GODRAYS_OFF GODRAYS_ON
			#pragma multi_compile LOCAL_SKY_OFF LOCAL_SKY_ON

			uniform float _Alpha_Global;

			uniform float3 _Sun_WorldSunDir;
			uniform float _SkyExposure;

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

			uniform sampler2D _godrayDepthTexture;
			uniform float _godrayStrength;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
#if defined(GODRAYS_ON) && defined(LOCAL_SKY_ON)
				float4 projPos  : TEXCOORD2;
#endif
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				v.vertex.xyz*= (_experimentalAtmoScale * (Rt-Rg)+ Rg) / Rt;
				OUT.pos = UnityObjectToClipPos(v.vertex);

#if defined (LOCAL_SKY_ON)
	#ifdef SHADER_API_D3D11
				OUT.pos.z = 0.0;
	#else
				OUT.pos.z = OUT.pos.w;
				OUT.pos = ( _ProjectionParams.y > 200.0 ) ? OUT.pos : float4(2.0,2.0,2.0,1.0); //cull on near camera on OpenGL
	#endif
#endif

				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);


				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;
#if defined (LOCAL_SKY_OFF)
				OUT.planetOrigin *= 6000;  //all calculations are done in localSpace
#endif

#if defined(GODRAYS_ON) && defined(LOCAL_SKY_ON)
				OUT.projPos = ComputeScreenPos(OUT.pos);
#endif
				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				float3 WSD = _Sun_WorldSunDir;
				float3 WCP = _WorldSpaceCameraPos;

#if defined (LOCAL_SKY_OFF)
				WCP *= 6000;
#endif

				float3 viewDir = normalize(IN.worldPos-_WorldSpaceCameraPos);

				float3 scatteringCameraPos = WCP - IN.planetOrigin;
#if defined(GODRAYS_ON) && defined(LOCAL_SKY_ON)
				float2 depthUV = IN.projPos.xy/IN.projPos.w;
				float godrayDepth = 0.0;

				godrayDepth = sampleGodrayDepth(_godrayDepthTexture, depthUV, 1.0);

				//trying to find the optical depth from the camera level, should probably remove these from the boundary of the atmo and not the camera level?
				float muCamera = dot(normalize(_WorldSpaceCameraPos - IN.planetOrigin), viewDir);

				godrayDepth = DistanceFromOpticalDepth(_experimentalAtmoScale * (Rt-Rg) * 0.5, length(_WorldSpaceCameraPos - IN.planetOrigin), muCamera, godrayDepth, 500000.0);
				//cap by 0.3*distance to atmo boundary
				float skyIntersectDistance =  intersectSphereInside(_WorldSpaceCameraPos, viewDir, IN.planetOrigin, Rg + _experimentalAtmoScale * (Rt-Rg));
				godrayDepth = min(0.3 * skyIntersectDistance, godrayDepth);

				godrayDepth *= _godrayStrength;

				scatteringCameraPos = scatteringCameraPos + viewDir * godrayDepth ;
#endif
				float3 inscatter = SkyRadiance3(scatteringCameraPos, viewDir, WSD);

				float3 finalColor = inscatter;
				float eclipseShadow = 1;

				//find worldPos of the point in the atmo we're looking at directly
				//necessary for eclipses, ring shadows and planetshine
				float3 worldPos;
#if defined (PLANETSHINE_ON) || defined (ECLIPSES_ON) || defined (RINGSHADOW_ON)
				float interSectPt= intersectSphereInside(WCP,viewDir,IN.planetOrigin,Rt);//*_rimQuickFixMultiplier

				if (interSectPt != -1)
				{
				worldPos = WCP + viewDir * interSectPt;
				}
#endif

#if defined (ECLIPSES_ON)
				if (interSectPt != -1)
				{				
					finalColor*= getEclipseShadows(worldPos);
				}
#endif

#if defined (RINGSHADOW_ON)
				if (interSectPt != -1)
				{
					finalColor *= getLinearRingColor(worldPos, _Sun_WorldSunDir, IN.planetOrigin).a;
				}
#endif

				/////////////////PLANETSHINE///////////////////////////////						    
#if defined (PLANETSHINE_ON)
				float3 inscatter2=0;
				float intensity=1;
				for (int i=0; i<4; ++i)
				{
					if (planetShineRGB[i].w == 0) break;

					//if source is not a sun compute intensity of light from angle to light source
					intensity=1;  
					if (planetShineSources[i].w != 1.0f)
					{
//						intensity = 0.5f*(1-dot(normalize(planetShineSources[i].xyz - worldPos),WSD));
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - worldPos),WSD)),0);
					}

					inscatter2+=SkyRadiance3(WCP - IN.planetOrigin, viewDir, normalize(planetShineSources[i].xyz)) *planetShineRGB[i].xyz*planetShineRGB[i].w*intensity;
				}

				finalColor+=inscatter2;
				#endif
				///////////////////////////////////////////////////////////	

				return float4(_Alpha_Global*dither(hdr(finalColor,_SkyExposure), IN.pos),1.0);	
			}
			ENDCG
		}


	}
}