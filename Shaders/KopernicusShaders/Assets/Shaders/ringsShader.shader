//Ring shader for Kopernicus
//by Ghassen Lahmar (blackrack)

Shader "Kopernicus/Rings"
{
  SubShader
  {
    Tags
    {
      "Queue" = "Transparent"
      "IgnoreProjector" = "True"
      "RenderType" = "Transparent"
    }

    Pass
    {
      ZWrite off

      Cull Off
      Blend SrcAlpha OneMinusSrcAlpha //alpha blend

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma glsl
      #pragma target 3.0

      #include "UnityCG.cginc"

      uniform sampler2D _MainTex;

      uniform sampler2D _noiseTexture;

      uniform float innerRadius;
      uniform float outerRadius;

      uniform float planetRadius;
      uniform float sunRadius;

      uniform float3 sunPosRelativeToPlanet;

      uniform float penumbraMultiplier;

      #define M_PI 3.1415926535897932384626

      struct v2f
      {
        float4 pos: SV_POSITION;
        float3 worldPos: TEXCOORD0;
        //float3 worldNormal: TEXCOORD1;  //we don't need normals where we're going
        //LIGHTING_COORDS(3,4) //nor do we need this crap
        float3 planetOrigin: TEXCOORD1; //moved from fragment shader
        //float4 objectVertex: TEXCOORD2;
      };

      v2f vert(appdata_base v)
      {
        v2f o;
        o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

        o.worldPos = mul(unity_ObjectToWorld, v.vertex);

        //o.viewDir = normalize(o.worldPos - _WorldSpaceCameraPos); //viewdir calculation moved to fragment shader because of interpolation artifacts (that frankly don't make any sense to me)

        //o.worldNormal = normalize(mul( unity_ObjectToWorld, float4(v.normal, 0)).xyz); //should be fine

        o.planetOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

        //o.objectVertex = v.vertex;

        return o;
      }

      //mie scattering
      //copied from scatterer/Proland
      float PhaseFunctionM(float mu, float mieG)
      {
        // Mie phase function
        return 1.5 * 1.0 / (4.0 * M_PI) * (1.0 - mieG * mieG) * pow(1.0 + (mieG * mieG) - 2.0 * mieG * mu, -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG * mieG);
      }

      //eclipse function from scatterer
      //used here to cast the planet shadow on the ring
      //will simplify it later and keep only the necessary bits for the ring
      //Original Source:   wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
      float getEclipseShadow(float3 worldPos, float3 worldLightPos, float3 occluderSpherePosition,float3 occluderSphereRadius, float3 lightSourceRadius)
      {
        float3 lightDirection = float3(worldLightPos - worldPos);
        float3 lightDistance = length(lightDirection);
        lightDirection = lightDirection / lightDistance;

        // computation of level of shadowing w  
        float3 sphereDirection = float3(occluderSpherePosition - worldPos); //occluder planet
        float sphereDistance = length(sphereDirection);
        sphereDirection = sphereDirection / sphereDistance;

        float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) - asin(min(1.0, occluderSphereRadius / sphereDistance)));

        float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius);
        w = w * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));

        return (1 - w);
      }

      float4 frag(v2f i): COLOR
      {

        //fix this for additional lights later, will be useful when I do the planetshine update for scatterer	
        float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); //assuming directional light only for now

        float3 worldPosRelPlanet = i.worldPos - i.planetOrigin;
        float distance = length(worldPosRelPlanet);

        float texturePosition = (distance - innerRadius) / (outerRadius - innerRadius);
        texturePosition = 1 - texturePosition; //flip to match UVs

        //lighting
        //float dotLight = dot (i.worldNormal,lightDir);  //boring
        //instead use the viewing direction (inspired from observing space engine rings)
        //looks more interesting than I expected 
        float3 viewdir = normalize(i.worldPos - _WorldSpaceCameraPos);
        float mu = dot(lightDir, -viewdir);
        float dotLight = 0.5 * (mu + 1);

        //mie scattering through rings when observed from the back
        float mieG = -0.95; //needs to be negative?
        float mieScattering = PhaseFunctionM(mu, mieG);
        mieScattering *= 0.03; // result too bright for some reason, this fixes it

        //planet shadow on ring								
        //do everything relative to planet position
        float shadow = getEclipseShadow(worldPosRelPlanet * 6000, sunPosRelativeToPlanet, 0, planetRadius, sunRadius * penumbraMultiplier); //*6000 to convert to local space, might be simpler in scaled?

        //TODO: Fade in some noise here when getting close to the rings
        //make it procedural noise?

        float4 color = tex2D(_MainTex, float2(texturePosition, 0.25)); //let's say left side = front texture and right side = back texture
        float4 backColor = tex2D(_MainTex, float2(texturePosition, 0.75)); //back color, same as frontColor if only one band is included
        //haven't tested a different back texture yet

        color.xyz = color.xyz * dotLight + backColor.xyz * mieScattering; //for now alpha is taken from front color only, I'll try to think of something
        color.xyz *= shadow;

        color = (texturePosition > 1 || texturePosition < 0) ? float4(0, 0, 0, 0) : color; //return transparent pixels if not between inner and outer radiuses

        //I'm kinda proud of this shader so far, it's short and clean

        return color;
      }
      ENDCG
    }
  }
}