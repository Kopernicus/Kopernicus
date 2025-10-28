// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Proland/Ocean/InitSpectrum" 
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

			uniform sampler2D _Spectrum01;
			uniform sampler2D _Spectrum23;
			uniform sampler2D _WTable;
			uniform float4 _Offset;
			uniform float4 _InverseGridSizes;
			uniform float _T;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			struct f2a
			{
				float4 col0 : COLOR0;
				float4 col1 : COLOR1;
				float4 col2 : COLOR2;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord;
				return OUT;
			}

			float2 GetSpectrum(float w, float2 s0, float2 s0c) 
			{
				float c = cos(w*_T);
				float s = sin(w*_T);
				return float2((s0.x + s0c.x) * c - (s0.y + s0c.y) * s, (s0.x - s0c.x) * s + (s0.y - s0c.y) * c);
			}

			float2 COMPLEX(float2 z) 
			{
				return float2(-z.y, z.x); // returns i times z (complex number)
			}

			f2a frag(v2f IN)
			{ 
				float2 uv = IN.uv.xy;

				float2 st;
				st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
				st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

				float4 s12 = tex2D(_Spectrum01, uv);
				float4 s12c = tex2D(_Spectrum01, _Offset.xy-uv);
				float4 s34 = tex2D(_Spectrum23, uv);
				float4 s34c = tex2D(_Spectrum23, _Offset.xy-uv);

				float2 k1 = st * _InverseGridSizes.x;
				float2 k2 = st * _InverseGridSizes.y;
				float2 k3 = st * _InverseGridSizes.z;
				float2 k4 = st * _InverseGridSizes.w;

				float4 w = tex2D(_WTable, uv);

				float2 h1 = GetSpectrum(w.x, s12.xy, s12c.xy);
				float2 h2 = GetSpectrum(w.y, s12.zw, s12c.zw);
				float2 h3 = GetSpectrum(w.z, s34.xy, s34c.xy);
				float2 h4 = GetSpectrum(w.w, s34.zw, s34c.zw);

				float2 h12 = h1 + COMPLEX(h2);
				float2 h34 = h3 + COMPLEX(h4);

				float2 n1 = COMPLEX(k1.x * h1) - k1.y * h1;
				float2 n2 = COMPLEX(k2.x * h2) - k2.y * h2;
				float2 n3 = COMPLEX(k3.x * h3) - k3.y * h3;
				float2 n4 = COMPLEX(k4.x * h4) - k4.y * h4;

				f2a OUT;

				OUT.col0 = float4(h12, h34);
				OUT.col1 = float4(n1, n2);
				OUT.col2 = float4(n3, n4);

				return OUT;
			}

			ENDCG

		}
	}
}