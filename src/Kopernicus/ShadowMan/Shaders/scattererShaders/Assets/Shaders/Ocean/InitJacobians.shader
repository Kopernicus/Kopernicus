// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Proland/Ocean/InitJacobians" 
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

				float K1 = length(k1);
				float K2 = length(k2);
				float K3 = length(k3);
				float K4 = length(k4);

				float IK1 = K1 == 0.0 ? 0.0 : 1.0 / K1;
				float IK2 = K2 == 0.0 ? 0.0 : 1.0 / K2;
				float IK3 = K3 == 0.0 ? 0.0 : 1.0 / K3;
				float IK4 = K4 == 0.0 ? 0.0 : 1.0 / K4;

				float4 w = tex2D(_WTable, uv);

				float2 h1 = GetSpectrum(w.x, s12.xy, s12c.xy);
				float2 h2 = GetSpectrum(w.y, s12.zw, s12c.zw);
				float2 h3 = GetSpectrum(w.z, s34.xy, s34c.xy);
				float2 h4 = GetSpectrum(w.w, s34.zw, s34c.zw);

				/// Jacobians
				float4 IK = float4(IK1,IK2,IK3,IK4);
				float2 k1Squared = k1*k1;
				float2 k2Squared = k2*k2;
				float2 k3Squared = k3*k3;
				float2 k4Squared = k4*k4;

				// 5: d(Dx(X,t))/dx 	Tes01 eq30
				// 6: d(Dy(X,t))/dy 	Tes01 eq30
				// 7: d(Dx(X,t))/dy 	Tes01 eq30
				float4 tmp = float4(h1.x, h2.x, h3.x, h4.x);

				f2a OUT;

				OUT.col0 = -tmp * (float4(k1Squared.x, k2Squared.x, k3Squared.x, k4Squared.x) * IK);
				OUT.col1 = -tmp * (float4(k1Squared.y, k2Squared.y, k3Squared.y, k4Squared.y) * IK);
				OUT.col2 = -tmp * (float4(k1.x*k1.y, k2.x*k2.y, k3.x*k3.y, k4.x*k4.y) * IK);

				return OUT;
			}

			ENDCG

		}
	}
}