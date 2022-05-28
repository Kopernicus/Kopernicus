// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Proland/Ocean/InitDisplacement" 
{
	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D _Buffer1;
			uniform sampler2D _Buffer2;
			uniform float4 _InverseGridSizes;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			struct f2a
			{
				float4 col0 : COLOR0;
				float4 col1 : COLOR1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord;
				return OUT;
			}

			f2a frag(v2f IN)
			{ 
				float2 uv = IN.uv.xy;

				float2 st;
				st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
				st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

				float2 k1 = st * _InverseGridSizes.x;
				float2 k2 = st * _InverseGridSizes.y;
				float2 k3 = st * _InverseGridSizes.z;
				float2 k4 = st * _InverseGridSizes.w;

				float K1 = length(k1);
				float K2 = length(k2);
				float K3 = length(k3);
				float K4 = length(k4);

				float IK1 = K1 == 0.0 ? 0.0 : 1.0 / K1;
				float IK2 = K2 == 0.0 ? 0.0 : 1.0 / K2;
				float IK3 = K3 == 0.0 ? 0.0 : 1.0 / K3;
				float IK4 = K4 == 0.0 ? 0.0 : 1.0 / K4;

				f2a OUT;

				OUT.col0 = tex2D(_Buffer1, IN.uv) * float4(IK1, IK1, IK2, IK2);
				OUT.col1 = tex2D(_Buffer2, IN.uv) * float4(IK3, IK3, IK4, IK4);

				return OUT;
			}

			ENDCG

		}
	}
}