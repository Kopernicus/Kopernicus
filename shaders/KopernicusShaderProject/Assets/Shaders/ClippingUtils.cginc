uniform float _ScattererCameraOverlap;

//not sure if good idea to use a bool
bool fragmentInsideOfClippingRange(float depth)
{
	UNITY_BRANCH
	if (_ProjectionParams.z > 1000.0) 	//if farcamera
	{
		return (depth  > (_ProjectionParams.y+1.0+_ScattererCameraOverlap)); //if fragment depth outside of current camera clipping range, return empty pixel
	}
	else								//if nearcamera
	{
		return (depth <= (_ProjectionParams.z+1.0));
	}
}

bool oceanFragmentInsideOfClippingRange(float depth)
{
	UNITY_BRANCH
	if (_ProjectionParams.z > 1000.0) 	//if farcamera
	{
		return (depth  > (_ProjectionParams.y+_ScattererCameraOverlap)); //if fragment depth outside of current camera clipping range, return empty pixel
	}
	else								//if nearcamera
	{
		return (depth <= (_ProjectionParams.z+1.0));
	}
}