//Factor to divide the godray depth by before storing
//Half have a max value of ~65000, we need to divide the depth value to be able to store it in a half texture
#define textureDivisionFactor 100.0;

inline float sampleGodrayDepth(sampler2D _godrayDepthTexture, float2 depthUV, float _godrayStrength)
{
	float godrayDepth = tex2Dlod(_godrayDepthTexture, float4(depthUV,0,0)).r;
	godrayDepth*=_godrayStrength*textureDivisionFactor;
	return max(godrayDepth,0.0);
}

inline float writeGodrayToTexture(float depth)
{
	return depth / textureDivisionFactor;
}