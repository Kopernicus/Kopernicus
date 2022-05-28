Shader "Scatterer/SubpixelMorphologicalAntialiasing"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always


		// 0 - Edge detection (Low)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertEdge
			#pragma fragment FragEdge
			#define SMAA_PRESET_LOW
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 1 - Edge detection (Medium)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertEdge
			#pragma fragment FragEdge
			#define SMAA_PRESET_MEDIUM
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 2 - Edge detection (High)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertEdge
			#pragma fragment FragEdge
			#define SMAA_PRESET_HIGH
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 3 - Blend Weights Calculation (Low)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertBlend
			#pragma fragment FragBlend
			#define SMAA_PRESET_LOW
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 4 - Blend Weights Calculation (Medium)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertBlend
			#pragma fragment FragBlend
			#define SMAA_PRESET_MEDIUM
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 5 - Blend Weights Calculation (High)
		Pass
		{
			CGPROGRAM

			#pragma vertex VertBlend
			#pragma fragment FragBlend
			#define SMAA_PRESET_HIGH
			#include "SMAABridge.cginc"

			ENDCG
		}

		// 6 - Neighborhood Blending
		Pass
		{
			CGPROGRAM

			#pragma vertex VertNeighbor
			#pragma fragment FragNeighbor
			#include "SMAABridge.cginc"

			ENDCG
		}
	}
}
