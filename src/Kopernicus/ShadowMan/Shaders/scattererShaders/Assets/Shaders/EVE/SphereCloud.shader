Shader "Scatterer-EVE/Cloud" {
	Properties{
		_Color("Color Tint", Color) = (1,1,1,1)
		_MainTex("Main (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_UVNoiseTex("UV Noise (RG)", 2D) = "black" {}
		_FalloffPow("Falloff Power", Range(0,3)) = 2
		_FalloffScale("Falloff Scale", Range(0,20)) = 3
		_DetailScale("Detail Scale", Range(0,100)) = 100
		_DetailDist("Detail Distance", Range(0,1)) = 0.00875
		_UVNoiseScale("UV Noise Scale", Range(0,0.1)) = 0.01
		_UVNoiseStrength("UV Noise Strength", Range(0,0.1)) = 0.002
		_UVNoiseAnimation("UV Noise Animation", Vector) = (0.002,0.001,0)
		_UniversalTime("Universal Time", Vector) = (0,0,0,0)
		_MinLight("Minimum Light", Range(0,1)) = 0
		_DistFade("Fade Distance", Range(0,100)) = 10
		_DistFadeVert("Fade Scale", Range(0,1)) = .002
		_RimDist("Rim Distance", Range(0,1)) = 1
		_RimDistSub("Rim Distance Sub", Range(0,2)) = 1.01
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = .01
		_OceanRadius("Ocean Radius", Float) = 63000
		_PlanetOrigin("Sphere Center", Vector) = (0,0,0,1)
		_DepthPull("Depth Augment", Float) = .99
		_SunPos("_SunPos", Vector) = (0,0,0)
		_SunRadius("_SunRadius", Float) = 1
	}

	Category{

		Tags { "Queue" = "Transparent-2" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Fog { Mode Global}
		//AlphaTest Greater 0
		//ColorMask RGB
		Cull Off
		ZWrite Off
		Lighting On

		SubShader {
			Pass {

				Lighting On
				Tags { "LightMode" = "ForwardBase"}

				CGPROGRAM


				#include "EVEUtils.cginc"
				#pragma target 3.0
				#pragma glsl
				#pragma vertex vert
				#pragma fragment frag
				#define MAG_ONE 1.4142135623730950488016887242097
				#pragma multi_compile SOFT_DEPTH_OFF SOFT_DEPTH_ON
				#pragma multi_compile WORLD_SPACE_OFF WORLD_SPACE_ON
				#pragma multi_compile MAP_TYPE_1 MAP_TYPE_CUBE_1 MAP_TYPE_CUBE2_1 MAP_TYPE_CUBE6_1

				#pragma multi_compile SCATTERER_OFF SCATTERER_ON
#ifdef SCATTERER_ON
				#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
				#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
				#pragma multi_compile PRESERVECLOUDCOLORS_OFF PRESERVECLOUDCOLORS_ON
				//#pragma multi_compile GODRAYS_OFF GODRAYS_ON
				#define GODRAYS_OFF
#endif
				
#ifndef MAP_TYPE_CUBE2_1
				#pragma multi_compile ALPHAMAP_N_1 ALPHAMAP_1
#endif
				#include "alphaMap.cginc"
				#include "cubeMap.cginc"
				
#ifdef SCATTERER_ON
				#include "../CommonAtmosphere.cginc"
				#include "../EclipseCommon.cginc"
				#include "../Atmo/Godrays/GodraysCommon.cginc"
				#include "../RingCommon.cginc"	
#endif
				
				CUBEMAP_DEF_1(_MainTex)

				sampler2D _DetailTex;
				sampler2D _UVNoiseTex;
				fixed4 _Color;
				float _FalloffPow;
				float _FalloffScale;
				float _DetailScale;
				float _DetailDist;

				float _UVNoiseScale;
				float _UVNoiseStrength;
				float2 _UVNoiseAnimation;

				float _MinLight;
				float _DistFade;
				float _DistFadeVert;
				float _RimDist;
				float _RimDistSub;
				float _OceanRadius;
				float _InvFade;
				float3 _PlanetOrigin;
				sampler2D _CameraDepthTexture;
				float _DepthPull;

#if defined (SCATTERER_ON)
				uniform float cloudColorMultiplier;
				uniform float cloudScatteringMultiplier;
				uniform float cloudSkyIrradianceMultiplier;
				uniform float3 _Sun_WorldSunDir;
				uniform float extinctionThickness;
				float _Radius;

	#if defined (GODRAYS_ON)
				uniform sampler2D _godrayDepthTexture;
				uniform float _godrayStrength;
	#endif
#endif

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldVert : TEXCOORD0;
					float3 L : TEXCOORD1;
					float4 objDetail : TEXCOORD2;
					float4 objMain : TEXCOORD3;
					float3 worldNormal : TEXCOORD4;
					float3 viewDir : TEXCOORD5;
					LIGHTING_COORDS(6,7)
					float4 projPos : TEXCOORD8;
#if defined (SCATTERER_ON)
					float3 worldOrigin: TEXCOORD9;
#endif					
				};


				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					o.pos = UnityObjectToClipPos(v.vertex);

					float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
					float3 origin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
					o.worldVert = vertexPos;
					o.worldNormal = normalize(vertexPos - origin);
					o.objMain = mul(_MainRotation, v.vertex);
					o.objDetail = mul(_DetailRotation, o.objMain);
					o.viewDir = normalize(WorldSpaceViewDir(v.vertex));

					o.projPos = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.projPos.z);
					TRANSFER_VERTEX_TO_FRAGMENT(o);

					o.L = _PlanetOrigin - _WorldSpaceCameraPos.xyz;
#if defined (SCATTERER_ON)
					o.worldOrigin = origin;
#endif
					return o;
				}

				struct fout
				{
					float4 color : COLOR;
#if !(SHADER_API_D3D11 && WORLD_SPACE_ON)
					float depth : DEPTH;
#endif
				};

				fout frag(v2f IN)

				{
					fout OUT;
					float4 color;
					float4 main;

					main = GET_CUBE_MAP_P(_MainTex, IN.objMain.xyz, _UVNoiseTex, _UVNoiseScale, _UVNoiseStrength, _UVNoiseAnimation);
					main = ALPHA_COLOR_1(main);

					float4 detail = GetCubeDetailMap(_DetailTex, IN.objDetail, _DetailScale);

					float viewDist = distance(IN.worldVert,_WorldSpaceCameraPos);
					half detailLevel = saturate(2 * _DetailDist*viewDist);
					color = _Color * main.rgba * lerp(detail.rgba, 1, detailLevel);

					float rim = saturate(abs(dot(IN.viewDir, IN.worldNormal)));
					rim = saturate(pow(_FalloffScale*rim,_FalloffPow));
					float dist = distance(IN.worldVert,_WorldSpaceCameraPos);
					float distLerp = saturate(_RimDist*(distance(_PlanetOrigin,_WorldSpaceCameraPos) - _RimDistSub*distance(IN.worldVert,_PlanetOrigin)));
					float distFade = 1 - GetDistanceFade(dist, _DistFade, _DistFadeVert);
					float distAlpha = lerp(distFade, rim, distLerp);

					color.a = lerp(0, color.a, distAlpha);

#ifdef WORLD_SPACE_ON
					float3 worldDir = normalize(IN.worldVert - _WorldSpaceCameraPos.xyz);
					float tc = dot(IN.L, worldDir);
					float d = sqrt(dot(IN.L,IN.L) - (tc*tc));
					float3 norm = normalize(-IN.L);
					float d2 = pow(d,2);
					float td = sqrt(dot(IN.L,IN.L) - d2);
					float tlc = sqrt((_OceanRadius*_OceanRadius) - d2);

					half sphereCheck = saturate(step(d, _OceanRadius)*step(0.0, tc) + step(length(IN.L), _OceanRadius));
					float sphereDist = lerp(tlc - td, tc - tlc, step(0.0, tc));
					sphereCheck *= step(sphereDist, dist);

					color.a *= 1 - sphereCheck;
#endif




////////////////////////////SCATTERER OFF
#if !defined (SCATTERER_ON)
					//lighting
					half transparency = color.a;
					float4 scolor = SpecularColorLight(_WorldSpaceLightPos0, IN.viewDir, IN.worldNormal, color, 0, 0, LIGHT_ATTENUATION(IN));
					scolor *= Terminator(normalize(_WorldSpaceLightPos0), IN.worldNormal);
					scolor.a = transparency;

	#ifdef SOFT_DEPTH_ON
					float depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
					depth = LinearEyeDepth(depth);
					float partZ = IN.projPos.z;
					float fade = saturate(_InvFade * (depth - partZ));
					scolor.a *= fade;
	#endif

					//scolor.rgb *= MultiBodyShadow(IN.worldVert, _SunRadius, _SunPos, _ShadowBodies); //causes artifacts idk why
					OUT.color = lerp(scolor, color, _MinLight);

////////////////////////////SCATTERER ON
#else
					float4 texColor = color;
					float3 extinction = float3(0, 0, 0);

	#ifdef WORLD_SPACE_ON
					float3 WCP = _WorldSpaceCameraPos; //unity supplied, in local Space
					float3 worldPos = IN.worldVert;
					float3 worldOrigin = IN.worldOrigin;
	#else
			    		float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords
					float3 worldPos = IN.worldVert * 6000;
					float3 worldOrigin = IN.worldOrigin * 6000;
	#endif

					float3 relWorldPos=worldPos-worldOrigin;
					relWorldPos = _Radius * normalize(relWorldPos);
					
					float alt = length(relWorldPos);
					float threshold = Rg * 1.00333333;

					relWorldPos = (alt < threshold) ? normalize(relWorldPos) * (threshold) : relWorldPos;   //artifacts fix (black scattering and overbright skyirradiance) when cloud altitude < Rg *( 1 + 2000/600000)

					float3 relCameraPos=WCP-worldOrigin;

					float3 scatteringPos = relWorldPos;

#if defined (GODRAYS_ON) && defined(WORLD_SPACE_ON)
					float2 depthUV = IN.projPos.xy/IN.projPos.w;
					float godrayDepth = sampleGodrayDepth(_godrayDepthTexture, depthUV, _godrayStrength);
					godrayDepth = min(godrayDepth, _godrayStrength * length(scatteringPos - relCameraPos));
					scatteringPos -= godrayDepth * IN.viewDir; //this works but it looks suuuper wrong when outside the cloud layer, to be revised I guess
#endif
					
					//inScattering from cloud to observer
					float3 inscatter = InScattering2(relCameraPos, scatteringPos, _Sun_WorldSunDir,extinction);
					
                			//extinction from cloud to observer
					extinction = getExtinction(relCameraPos, relWorldPos, 1.0, 1.0, 1.0);

					//extinction of light from sun to cloud
					extinction *= getSkyExtinction(relWorldPos,_Sun_WorldSunDir);

					extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

					extinction*= getEclipseShadow(relWorldPos, 20.0 * _Sun_WorldSunDir * Rg, 0.0, Rg, Rg); //just the terminator, extinction ignores ground intersection

					//skyLight
					float3 skyE = SimpleSkyirradiance(relWorldPos, IN.viewDir, _Sun_WorldSunDir);

	#if defined (PRESERVECLOUDCOLORS_OFF)
					color = float4(hdrNoExposure(color.rgb*cloudColorMultiplier*extinction+ inscatter*cloudScatteringMultiplier+skyE*cloudSkyIrradianceMultiplier), color.a); //not bad
	#else
					float3 cloudColor = color.rgb*cloudColorMultiplier*(extinction+hdrNoExposure(skyE * cloudSkyIrradianceMultiplier));
					inscatter = hdrNoExposure(inscatter * cloudScatteringMultiplier);
					color = float4(cloudColor + (float3(1.0,1.0,1.0)-cloudColor)*inscatter, color.a); //basically soft blend
	#endif					

	#if defined (ECLIPSES_ON)				
					color.rgb*=getEclipseShadows(worldPos);
	#endif

	#if defined (RINGSHADOW_ON)
					color.rgb *= getLinearRingColor(relWorldPos, _Sun_WorldSunDir, 0.0).a;
	#endif
					OUT.color = lerp(color, texColor, _MinLight);
#endif //endif SCATTERER_ON

					float depthWithOffset = IN.projPos.z;
#ifndef WORLD_SPACE_ON
					depthWithOffset *= _DepthPull;
					OUT.color.a *= step(0, dot(IN.viewDir, IN.worldNormal));
#endif

#if !(SHADER_API_D3D11 && WORLD_SPACE_ON) //fixes clouds fading into the planet when zooming out
					OUT.depth = (1.0 - depthWithOffset * _ZBufferParams.w) / (depthWithOffset * _ZBufferParams.z);
#endif

					return OUT;
				}
				ENDCG

			}

		}

	}
}