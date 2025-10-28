// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/sunFlareExtinction" 
{
	SubShader 
	{
		Pass //pass 0 - atmospheric extinction
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DISABLE_UNDERWATER_OFF DISABLE_UNDERWATER_ON

			uniform float Rg;
			uniform float Rt;
			uniform sampler2D _Sky_Transmittance;

			uniform float3 _Sun_WorldSunDir;
			uniform float3 _Globals_WorldCameraPos;
			uniform float _experimentalAtmoScale;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord;

				return OUT;
			}

			float2 GetTransmittanceUV(float r, float mu)
			{
				float uR, uMu;
				uR = sqrt((r - Rg) / (Rt - Rg));
				uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
				return float2(uMu, uR);
			}

			float3 Transmittance(float r, float mu) 
			{
				float2 uv = GetTransmittanceUV(r, mu);
				return tex2Dlod(_Sky_Transmittance, float4(uv,0,0)).rgb;
			}

			float SQRT(float f, float err)
			{
				return f >= 0.0 ? sqrt(f) : err;
			}

			float3 getExtinction(float3 camera, float3 viewdir)
			{
				float3 extinction = float3(1,1,1);

				Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;

				//float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
				float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,0.000001);


				float din = max(-rMu - deltaSq, 0.0);
				if (din > 0.0)
				{
					camera += din * viewdir;
					rMu += din;
					mu = rMu / Rt;
					r = Rt;
				}

				extinction = (r > Rt) ? float3(1,1,1) : Transmittance(r, mu);

				return extinction;
			}

			float4 frag(v2f IN): COLOR
			{
				float3 WSD = _Sun_WorldSunDir;
				float3 WCP = _Globals_WorldCameraPos;

				float3 extinction = getExtinction(WCP,WSD);

#if defined (DISABLE_UNDERWATER_ON) //disable when underwater
				extinction = (length(WCP) >= Rg ) ? extinction : float4(0.0,0.0,0.0,1.0);
#endif

				return float4(extinction,1.0);
			}

			ENDCG
		}


		Pass //pass 1 - ring extinction
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "../IntersectCommon.cginc"
			#include "../RingCommon.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float3 _Sun_WorldSunDir;
			uniform float3 _Globals_WorldCameraPos;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord;
				return OUT;
			}

			float4 frag(v2f IN): COLOR
			{
				float3 WCP = _Globals_WorldCameraPos;

				float4 ringColor = getLinearRingColor(WCP,_Sun_WorldSunDir,float3(0,0,0));

				return float4(ringColor.xyz,1.0);
			}

			ENDCG
		}
	}
}
