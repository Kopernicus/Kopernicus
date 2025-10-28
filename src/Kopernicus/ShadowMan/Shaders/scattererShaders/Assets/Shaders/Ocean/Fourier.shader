// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/Fourier" 
{
	CGINCLUDE

	#include "UnityCG.cginc"

	uniform sampler2D _ReadBuffer0, _ReadBuffer1, _ReadBuffer2;
	uniform sampler2D _ButterFlyLookUp;
	uniform float _Size;

	struct v2f 
	{
		float4  pos : SV_POSITION;
		float2  uv : TEXCOORD0;
	};

	struct f2a_1
	{
		float4 col0 : COLOR0;
	};

	struct f2a_2
	{
		float4 col0 : COLOR0;
		float4 col1 : COLOR1;
	};

	struct f2a_3
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

	//Performs two FFTs on two complex numbers packed in a vector4
	float4 FFT(float2 w, float4 input1, float4 input2) 
	{
		float rx = w.x * input2.x - w.y * input2.y;
		float ry = w.y * input2.x + w.x * input2.y;
		float rz = w.x * input2.z - w.y * input2.w;
		float rw = w.y * input2.z + w.x * input2.w;

		return input1 + float4(rx,ry,rz,rw);
	}

	f2a_1 fragX_1(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.x, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_1 OUT;

		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));

		return OUT;
	}

	f2a_1 fragY_1(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.y, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_1 OUT;

		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));

		return OUT;
	}

	f2a_2 fragX_2(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.x, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_2 OUT;

		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));

		return OUT;
	}

	f2a_2 fragY_2(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.y, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_2 OUT;

		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));

		return OUT;
	}

	f2a_3 fragX_3(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.x, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_3 OUT;

		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));

		return OUT;
	}

	f2a_3 fragY_3(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.y, 0));

		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size-1.0;

		float PI = 3.1415926;
		float2 w = float2(cos(2.0*PI*lookUp.z/_Size), sin(2.0*PI*lookUp.z/_Size));

		if(lookUp.w > 0.5) w *= -1.0;

		f2a_3 OUT;

		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);

		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));

		return OUT;
	}

	ENDCG

	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_1
			ENDCG
		}

		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_1
			ENDCG
		}

		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_2
			ENDCG
		}

		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_2
			ENDCG
		}

		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_3
			ENDCG
		}

		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_3
			ENDCG
		}
	}

}