#pragma once

float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint, out float parallel)
{
	float tlength;
	float dotNumerator;
	float dotDenominator;

	float3 intersectVector;
	float3 intersection = 0.0;

	//calculate the distance between the linePoint and the line-plane intersection point
	dotNumerator = dot((planePoint - linePoint), planeNormal);
	dotDenominator = dot(lineVec, planeNormal);

	//line and plane are not parallel
	if(dotDenominator != 0.0f)
	{
		tlength =  dotNumerator / dotDenominator;
		intersection= (tlength > 0.0) ? linePoint + normalize(lineVec) * (tlength) : linePoint;
		parallel = 0.0;
	}
	else
	{
		parallel = 1.0;
	}

	return intersection;
}

//works from outside only
//p1 starting point, d look direction, p3 is the sphere center
float intersectSphereOutside(float3 p1, float3 d, float3 p3, float r) {

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;

	float test = b * b - 4.0 * a * c;

	float u = (test < 0) ? -1.0 : (-b - sqrt(test)) / (2.0 * a);

	return u;
}

//works from inside and outside
//p1 starting point, d look direction, p3 is the sphere center
float intersectSphereInside(float3 p1, float3 d, float3 p3, float r)
{
	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r*r;

	float test = b*b - 4.0*a*c;

	if (test<0)
	{
		return -1.0;
	}

	float u = (-b - sqrt(test)) / (2.0 * a);

	u = (u < 0) ? (-b + sqrt(test)) / (2.0 * a) : u;

	return u;
}