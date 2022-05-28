Shader "Scatterer-EVE/CloudShadowMap"
{
	Properties
	{
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
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ShadowCaster"}

			BlendOp Max
			Blend One One 
				
			CGPROGRAM
					
			#include "EVEUtils.cginc"
			#pragma target 3.0
			#pragma glsl
			#pragma vertex vert
			#pragma fragment frag
			#define MAG_ONE 1.4142135623730950488016887242097
			#pragma multi_compile MAP_TYPE_1 MAP_TYPE_CUBE_1 MAP_TYPE_CUBE2_1 MAP_TYPE_CUBE6_1

			#ifndef MAP_TYPE_CUBE2_1
			#pragma multi_compile ALPHAMAP_N_1 ALPHAMAP_1
			#endif
			#include "alphaMap.cginc"
			#include "cubeMap.cginc"
					
			CUBEMAP_DEF_1(_MainTex)

			fixed4 _Color;
			uniform sampler2D _DetailTex;
			uniform sampler2D _UVNoiseTex;
			fixed4 _DetailOffset;
			float _DetailScale;
			float _DetailDist;
			float _UVNoiseScale;
			float _UVNoiseStrength;
			float2 _UVNoiseAnimation;
			float4 _SunDir;
			float _Radius;
			float _PlanetRadius;

			float3 _PlanetOrigin;
			float _godrayCloudThreshold;
			
			struct appdata_t
			{
				float4 vertex : POSITION;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				//float4 objMain : TEXCOORD0;
				float4 worldPos : TEXCOORD0;
			};
			
			
			v2f vert(appdata_t v)
			{
				v2f o;

				o.worldPos = float4(_PlanetOrigin + _Radius * normalize(v.vertex.xyz),1.0);
				o.pos = UnityWorldToClipPos(o.worldPos);

//				o.worldPos = float4(_PlanetOrigin + v.vertex.xyz/v.vertex.w * _Radius, 1.0);
//				o.pos = UnityWorldToClipPos(o.worldPos);

//#if defined(UNITY_REVERSED_Z)
				float clamped = min(o.pos.z, o.pos.w*UNITY_NEAR_CLIP_VALUE);
//#else
//				float clamped = max(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
//#endif
				o.pos.z = lerp(o.pos.z, clamped, unity_LightShadowBias.y);


//				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
//				UnityApplyLinearShadowBias(o.pos);

				return o;
			}

			float4 frag(v2f IN) : SV_Target

			{
				float4 worldPos = float4(IN.worldPos.xyz/IN.worldPos.w,1.0);
				float4 mainPos = mul(_MainRotation, worldPos);

				float4 color;
				float4 main;
				
				main = GET_CUBE_MAP_P(_MainTex, mainPos.xyz/mainPos.w, _UVNoiseTex, _UVNoiseScale, _UVNoiseStrength, _UVNoiseAnimation);
				main = ALPHA_COLOR_1(main);
				
				color = _Color * main.rgba;

				if (color.a < _godrayCloudThreshold)
					discard;
				
				return float4(IN.pos.z/IN.pos.w,0.0,0.0,1.0);
			}
			ENDCG
		}
	}
}