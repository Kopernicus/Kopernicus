// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Ring shader for Kopernicus
// by Ghassen Lahmar (blackrack)

Shader "Kopernicus/Rings"
{
  SubShader
  {
    Tags
    {
      "Queue"           = "Transparent"
      "IgnoreProjector" = "True"
      "RenderType"      = "Transparent"
      "LightMode"       = "ForwardBase"
    }

    Pass
    {
      ZWrite On
      Cull Back
      // Alpha blend
      Blend SrcAlpha OneMinusSrcAlpha

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma glsl
      #pragma target 3.0

      #include "UnityCG.cginc"

      // These properties are the global inputs shared by all pixels

      uniform sampler2D _MainTex;

      uniform float innerRadius;
      uniform float outerRadius;

      uniform float planetRadius;
      uniform float sunRadius;

      uniform float3 sunPosRelativeToPlanet;

      uniform float penumbraMultiplier;

      //Added values
      uniform sampler2D _BacklitTexture;

      uniform float anisotropy;
      uniform float albedoStrength;
      uniform float scatteringStrength;

      // Unity will set this to the material color automatically
      uniform float4 _Color;

      // Properties to simulate a shade moving past the inner surface of the ring
      uniform sampler2D _InnerShadeTexture;
      uniform int       innerShadeTiles;
      uniform float     innerShadeOffset;

      // For fading out the ring on close proximity.
      uniform float fadeoutStartDistance;
      uniform float fadeoutStopDistance;
      uniform float fadeoutMinAlpha;

      // For adding extra detail to rings.
      uniform float4 detailRegionsMask;
      uniform sampler2D _DetailRegionsTex;
      // Coarse detail, for larger scale noise.
      uniform sampler2D _CoarseDetailNoiseTex;
      uniform float4 coarseDetailAlphaMin;
      uniform float4 coarseDetailAlphaMax;
      uniform float coarseDetailStrength;
      uniform float4 coarseDetailMask;
      // Fine detail, for smaller scale noise.
      uniform sampler2D _FineDetailNoiseTex;
      uniform float4 fineDetailAlphaMin;
      uniform float4 fineDetailAlphaMax;
      uniform float fineDetailStrength;
      uniform float4 fineDetailMask;
      uniform float4 detailTiling;
      // These are presented differently on the CPU side.
      // The reason is to do SIMD smoothstep in the shader.
      uniform float4 detailFade0; // distances at which detail strength is 0
      uniform float4 detailFade1; // distances at which detail strength is 1
      // fade0, xy: fade-in start for coarse, fine
      // fade0, zw: fade-out stop for coarse, fine
      // fade1, xy: fade-in stop for coarse, fine
      // fade1, zw: fade-out start for coarse, fine

      #define M_PI 3.1415926535897932384626

      // This structure defines the inputs for each pixel
      struct v2f
      {
        float4 pos:          SV_POSITION;
        float4 worldPos:     TEXCOORD0;
        // Moved from fragment shader
        float4 planetOrigin: TEXCOORD1;
        float3 texCoord:     TEXCOORD2;
      };

      // Set up the inputs for the fragment shader
      v2f vert(appdata_base v)
      {
        v2f o;
        o.pos          = UnityObjectToClipPos(v.vertex);
        o.worldPos     = mul(unity_ObjectToWorld, v.vertex);
        o.planetOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
        o.texCoord.xy  = v.texcoord.xy;
        o.texCoord.z = atan2(v.vertex.z, v.vertex.x) * 0.15915494309;
        return o;
      }

      // Mie scattering
      // Copied from Scatterer/Proland
      float PhaseFunctionM(float mu, float mieG)
      {
        // Mie phase function
        return 1.5 * 1.0 / (4.0 * M_PI) * (1.0 - mieG * mieG) * pow(1.0 + (mieG * mieG) - 2.0 * mieG * mu, -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG * mieG);
      }

      // Eclipse function from Scatterer
      // Used here to cast the planet shadow on the ring
      // Will simplify it later and keep only the necessary bits for the ring
      // Original Source:   wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
      float getEclipseShadow(float3 worldPos, float3 worldLightPos, float3 occluderSpherePosition, float3 occluderSphereRadius, float3 lightSourceRadius)
      {
        float3 lightDirection = float3(worldLightPos - worldPos);
        float3 lightDistance  = length(lightDirection);
        lightDirection = lightDirection / lightDistance;

        // Computation of level of shadowing w
        // Occluder planet
        float3 sphereDirection = float3(occluderSpherePosition - worldPos);
        float  sphereDistance  = length(sphereDirection);
        sphereDirection = sphereDirection / sphereDistance;

        float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) - asin(min(1.0, occluderSphereRadius / sphereDistance)));

        float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius)
            * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));

        return (1 - w);
      }

      // Check whether our shadow squares cover this pixel
      float getInnerShadeShadow(v2f i)
      {
        // The shade only slides around the ring, so we use the X tex coord.
        float2 shadeTexCoord = float2(
          i.texCoord.x,
          i.texCoord.y / innerShadeTiles + innerShadeOffset
        );
        // Check the pixel currently above the one we're rendering.
        float4 shadeColor = tex2D(_InnerShadeTexture, shadeTexCoord);
        // If the shade is solid, then it blocks the light.
        // If it's transparent, then the light goes through.
        return 1 - 0.8 * shadeColor.a;
      }

      // Either we're a sun with a ringworld and shadow squares,
      // or we're a planet orbiting a sun and casting shadows.
      // There's no middle ground. So if shadow squares are turned on,
      // disable eclipse shadows.
      float getShadow(v2f i)
      {
        if (innerShadeTiles > 0) {
          return getInnerShadeShadow(i);
        } else {
          // Do everything relative to planet position
          // *6000 to convert to local space, might be simpler in scaled?
          float3 worldPosRelPlanet = i.worldPos.xyz/i.worldPos.w - i.planetOrigin.xyz/i.planetOrigin.w;
          return getEclipseShadow(worldPosRelPlanet * 6000, sunPosRelativeToPlanet, 0, planetRadius, sunRadius * penumbraMultiplier);
        }
      }

      // Choose a color to use for the pixel represented by 'i'
      float4 frag(v2f i): COLOR
      {
        // Lighting
        // The built-in unity directional light direction seem to be borked somehow, I guess it is only reliable for the current planet we are orbiting and planetarium shaders pass their own light directions
        // In this case calculate lightDirection from the sun position
        float3 lightDir = normalize(sunPosRelativeToPlanet);

        // Instead use the viewing direction (inspired from observing space engine rings)
        // Looks more interesting than I expected
        float3 viewdir  = i.worldPos.xyz / i.worldPos.w - _WorldSpaceCameraPos;
        float camDist   = length(viewdir);
        viewdir /= camDist;
        float  mu       = dot(lightDir, -viewdir);
        float  dotLight = 0.5 * (mu + 1);

        // Mie scattering through rings when observed from the back
        // Needs to be negative?
        float mieG = -1 * anisotropy;   // was -0.95
        // Result too bright for some reason, the 0.03 fixes it
        // A value of 1.92466 keeps old brightness with new normalization value; this is set as the default scatteringStrength
        float mieScattering = PhaseFunctionM(mu, mieG) / PhaseFunctionM(-1, mieG);

        // Planet shadow on ring, or inner shade shadow on inner face
        float shadow = getShadow(i);

        // Fade in some noise here when getting close to the rings
        // Detail UVs do not seem to be set up correctly for the ring.
        float2 detailUV = i.texCoord.xz;
        float4 detailMask = tex2D(_DetailRegionsTex, i.texCoord.xy) * detailRegionsMask;
        float detailCoarse = dot(
            detailMask * coarseDetailMask,
            lerp(
                coarseDetailAlphaMin,
                coarseDetailAlphaMax,
                tex2D(_CoarseDetailNoiseTex, detailTiling.xy * detailUV)
            )
        );
        float detailFine = dot(
            detailMask * fineDetailMask,
            lerp(
                fineDetailAlphaMin,
                fineDetailAlphaMax,
                tex2D(_FineDetailNoiseTex, detailTiling.zw * detailUV)
            )
        );
        // xy, zw are (coarse, fine)
        float4 detailFade = smoothstep(detailFade0, detailFade1, camDist);
        // xy is fade-in, zw is fade-out. Multiply them to get a band pass function.
        // Scale the max height of the band-pass function with the per-channel strength multiplier.
        float2 detailBands = detailFade.xy * detailFade.zw * float2(coarseDetailStrength, fineDetailStrength);
        // Detail is ready.

        // Fade-out when close to the rings.
        float proximityAlpha = smoothstep(fadeoutStopDistance, fadeoutStartDistance, camDist);
        proximityAlpha = lerp(fadeoutMinAlpha, 1.0, proximityAlpha);

        // Look up the texture colors
        float4 color = tex2D(_MainTex, i.texCoord.xy);
        float4 backColor = tex2D(_BacklitTexture, i.texCoord.xy);
        // Combine material color with texture color and shadow
        color.xyz = _Color * shadow * (albedoStrength * color.xyz * dotLight + scatteringStrength * backColor.xyz * mieScattering);
        color.w = max(color.w, backColor.w * mieScattering);

        // Apply detail noise.
        color.w = lerp(color.w, color.w * detailCoarse, detailBands.x);
        color.w = lerp(color.w, color.w * detailFine,   detailBands.y);

        // Multiply alpha with proximity fadeout.
        color.w *= proximityAlpha;

        // I'm kinda proud of this shader so far, it's short and clean
        return color;
      }
      ENDCG
    }
  }
}
