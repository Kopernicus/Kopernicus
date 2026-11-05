
Shader "Scatterer/DepthToDistance" {
	SubShader {
		Tags {"IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass {
			Cull Off
			ZTest Off
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			uniform sampler2D _CameraDepthTexture;

			struct v2f
			{
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv  = ComputeNonStereoScreenPos(o.pos);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(i.uv,0,0));

				#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
				zdepth = 1 - zdepth;
				#endif

				float4 clipPos = float4(i.uv, zdepth, 1.0);
				clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
				float4 camPos = mul(unity_CameraInvProjection, clipPos);
				camPos.xyz /= camPos.w;
				camPos.z *= -1;

				float fragDistance = length(camPos.xyz) / 750000.0;

				return float4(fragDistance,fragDistance,fragDistance, (zdepth < 1.0) ? 1.0 : 0.0);  //discard if zdepth is 1.0, ie nothing written to depth texture
			}

			ENDCG
		}
	}
}