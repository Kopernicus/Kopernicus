Shader "Scatterer/sunFlare" 
{
	CGINCLUDE

	#include "UnityCG.cginc"
	#include "../CommonAtmosphere.cginc"
	#include "../DepthCommon.cginc"

	uniform float sunGlareScale;
	uniform float sunGlareFade;

	uniform sampler2D sunSpikes;
	uniform sampler2D sunFlare;
	uniform sampler2D sunGhost1;
	uniform sampler2D sunGhost2;
	uniform sampler2D sunGhost3;

	uniform float3 flareColor;

	uniform sampler2D extinctionTexture;

	uniform float3 flareSettings;		//intensity, aspect ratio, scale
	uniform float3 spikesSettings;

	uniform float4x4 ghost1Settings1;	//each row is an instance of a ghost
	uniform float4x4 ghost1Settings2;	//for each row: intensity, aspect ratio, scale, position on sun-screenCenter line. Intensity of 0 means nothing defined

	uniform float4x4 ghost2Settings1;
	uniform float4x4 ghost2Settings2;

	uniform float4x4 ghost3Settings1;
	uniform float4x4 ghost3Settings2;

	uniform float3 sunViewPortPos;
	uniform float aspectRatio;

	uniform float renderSunFlare;
	uniform float renderOnCurrentCamera;
	uniform float useDbufferOnCamera;	

	struct v2f 
	{
		float4 pos: SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 color : TEXCOORD1;
	};

	v2f flareVertexShader(appdata_base v, float3 sunViewPortPos, float3 settings)
	{
		v2f OUT;

		float drawFlare = (useDbufferOnCamera < 1.0) ? 1.0 : checkDepthBufferEmpty(sunViewPortPos.xy);  //if there's something in the way don't render the flare

		sunViewPortPos.xy = 2.0 * sunViewPortPos.xy - float2(1.0,1.0);
		sunViewPortPos.y *= _ProjectionParams.x;

		v.vertex.xy = sunViewPortPos.xy + v.vertex.xy / (settings.z * sunGlareScale * float2(aspectRatio * settings.y, 1.0)); //change this so it no longer has to do a division

		OUT.pos = float4(v.vertex.xy, 1.0, 1.0);

		OUT.pos = (settings.x > 0.0) && (renderSunFlare == 1.0) && (_ProjectionParams.y < 200.0) && (renderOnCurrentCamera == 1.0) && (drawFlare ==1.0) ? OUT.pos : float4(2.0,2.0,2.0,1.0);	//if we don't need to render the sunflare, cull vertexes by placing them outside clip space
		//also use near plane to not render on far camera
		OUT.uv = v.texcoord.xy;

		OUT.color = settings.x * sunGlareFade * flareColor * tex2Dlod(extinctionTexture,float4(0.0,0.0,0.0,0.0)).rgb;

		return OUT;
	}

	struct v2g
	{
		float4 pos: SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 color : TEXCOORD1;
	};

	struct g2f
	{
		float4 pos: SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 color : TEXCOORD1;
	};

	v2g ghostVertexShader (appdata_base v, float3 sunViewPortPos, float ghostFade)
	{
		v2g OUT;

		OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
		OUT.uv = v.texcoord.xy;

		float2 toScreenCenter=sunViewPortPos.xy-0.5;
		OUT.color = ghostFade * smoothstep(0,1.0,1.0-length(toScreenCenter)) * sunGlareFade * flareColor * tex2Dlod(extinctionTexture,float4(0.0,0.0,0.0,0.0)).rgb;

		return OUT;
	}

	g2f buildGhostVertex(v2g input, float4 ghostSettings, float3 sunViewPortPos)
	{
		g2f tri;

		tri.pos = input.pos;
		tri.pos.xy = sunViewPortPos.xy - sunViewPortPos.xy * ghostSettings.w + tri.pos.xy / (ghostSettings.z * float2(aspectRatio * ghostSettings.y, 1.0)); //change this so it no longer has to do a division
		tri.uv = input.uv;
		tri.color = input.color * ghostSettings.x ;

		return tri;
	}

	//geometry shader
	//for every triangle of the input, create a max of 8 triangles, (max 8 possible instances of each ghost)
	inline void ghostGeometryShader(v2g input[3], inout TriangleStream<g2f> outStream, float4x4 settings1, float4x4 settings2, float3 sunViewPortPos)
	{
		float drawFlare = (useDbufferOnCamera < 1.0) ? 1.0 : checkDepthBufferEmpty(sunViewPortPos.xy);
		drawFlare = (renderSunFlare == 1.0) && (_ProjectionParams.y < 200.0) && (renderOnCurrentCamera == 1.0) && (drawFlare ==1.0) ? 1.0 : 0.0;

		sunViewPortPos.xy = 2.0 * sunViewPortPos.xy - float2(1.0,1.0);
		sunViewPortPos.y *= _ProjectionParams.x;

		for (int i=0; i<4; ++i)
		{
			if ((settings1[i].x == 0) || (drawFlare < 1.0))
				break;

			outStream.Append(buildGhostVertex(input[0], settings1[i], sunViewPortPos));
			outStream.Append(buildGhostVertex(input[1], settings1[i], sunViewPortPos));
			outStream.Append(buildGhostVertex(input[2], settings1[i], sunViewPortPos));

			outStream.RestartStrip();
		}

		for (i=0; i<4; ++i)
		{
			if ((settings2[i].x == 0) || (drawFlare < 1.0))
				break;

			outStream.Append(buildGhostVertex(input[0], settings2[i], sunViewPortPos));
			outStream.Append(buildGhostVertex(input[1], settings2[i], sunViewPortPos));
			outStream.Append(buildGhostVertex(input[2], settings2[i], sunViewPortPos));

			outStream.RestartStrip();
		}
	}

	ENDCG

	SubShader 
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		//Pass 0: flare 1
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor  //"reverse" soft-additive

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			v2f vert(appdata_base v)
			{
				return flareVertexShader(v, sunViewPortPos, flareSettings);
			}

			float4 frag(v2f IN) : SV_Target
			{
				float3 sunColor= IN.color * tex2Dlod (sunFlare,float4(IN.uv.xy,0,0)).rgb;
				return float4(sunColor,1.0);
			}
			ENDCG
		}

		//Pass 1: flare2
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			v2f vert(appdata_base v)
			{
				return flareVertexShader(v, sunViewPortPos, spikesSettings);
			}
				
			float4 frag(v2f IN) : SV_Target
			{
				float3 sunColor= IN.color *  tex2Dlod (sunSpikes,float4(IN.uv.xy,0,0)).rgb;
				return float4(sunColor,1.0);
			}
			ENDCG
		}

		//Pass 2: ghost 1
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma glsl

			uniform float ghost1Fade;

			v2g vert(appdata_base v)
			{
				return ghostVertexShader(v, sunViewPortPos, ghost1Fade);
			}

			[maxvertexcount(24)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
			{
				ghostGeometryShader(input, outStream, ghost1Settings1, ghost1Settings2, sunViewPortPos);
			}


			float4 frag(g2f IN) : SV_Target
			{
				float3 sunColor= IN.color * tex2Dlod (sunGhost1,float4(IN.uv.xy,0,0)).rgb;
				return float4(sunColor,1.0);
			}
			ENDCG
		}

		//Pass 3: ghost 2
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma glsl

			uniform float ghost2Fade;

			v2g vert(appdata_base v)
			{
				return ghostVertexShader(v, sunViewPortPos, ghost2Fade);
			}

			[maxvertexcount(24)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
			{
				ghostGeometryShader(input, outStream, ghost2Settings1, ghost2Settings2, sunViewPortPos);
			}


			float4 frag(g2f IN) : SV_Target
			{
				float3 sunColor= IN.color * tex2Dlod (sunGhost2,float4(IN.uv.xy,0,0)).rgb;
				return float4(sunColor,1.0);
			}
			ENDCG
		}

		//Pass 4: ghost 3
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma glsl

			uniform float ghost3Fade;

			v2g vert(appdata_base v)
			{
				return ghostVertexShader(v, sunViewPortPos, ghost3Fade);
			}
				
			[maxvertexcount(24)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
			{
				ghostGeometryShader(input, outStream, ghost3Settings1, ghost3Settings2, sunViewPortPos);
			}


			float4 frag(g2f IN) : SV_Target
			{
				float3 sunColor= IN.color * tex2Dlod (sunGhost3,float4(IN.uv.xy,0,0)).rgb;
				return float4(sunColor,1.0);
			}
			ENDCG
		}
	}
}