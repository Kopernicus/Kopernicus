float times_frc(float a, float b) {
	return lerp(0.0, a * b, b != 0.0 ? 1.0 : 0.0);
}

float div_frc(float a, float b) {
	return lerp(0.0, a / b, b != 0.0 ? 1.0 : 0.0);
}

float plus_frc(float a, float b) {
	return lerp(a, a + b, b != 0.0 ? 1.0 : 0.0);
}

float minus_frc(float a, float b) {
	return lerp(a, a - b, b != 0.0 ? 1.0 : 0.0);
}

// create double-single number from float
float2 ds_set(float a) {
	return float2(a, 0.0);
}

// Double emulation based on GLSL Mandelbrot Shader by Henry Thasler (www.thasler.org/blog)
//
// Emulation based on Fortran-90 double-single package. See http://crd.lbl.gov/~dhbailey/mpdist/
// Substract: res = ds_add(a, b) => res = a + b
float2 ds_add (float2 dsa, float2 dsb) {
	float2 dsc;
	float t1, t2, e;

	t1 = plus_frc(dsa.x, dsb.x);
	e = minus_frc(t1, dsa.x);
	t2 = plus_frc(plus_frc(plus_frc(minus_frc(dsb.x, e), minus_frc(dsa.x, minus_frc(t1, e))), dsa.y), dsb.y);
	dsc.x = plus_frc(t1, t2);
	dsc.y = minus_frc(t2, minus_frc(dsc.x, t1));
	return dsc;
}

// Substract: res = ds_sub(a, b) => res = a - b
float2 ds_sub (float2 dsa, float2 dsb) {
	float2 dsc;
	float e, t1, t2;

	t1 = minus_frc(dsa.x, dsb.x);
	e = minus_frc(t1, dsa.x);
	t2 = minus_frc(plus_frc(plus_frc(minus_frc(minus_frc(0.0, dsb.x), e), minus_frc(dsa.x, minus_frc(t1, e))), dsa.y), dsb.y);

	dsc.x = plus_frc(t1, t2);
	dsc.y = minus_frc(t2, minus_frc(dsc.x, t1));
	return dsc;
}

// Compare: res = -1 if a < b
//              = 0 if a == b
//              = 1 if a > b
float ds_cmp(float2 dsa, float2 dsb) {
	if (dsa.x < dsb.x) {
		return -1.;
	}
	if (dsa.x > dsb.x) {
		return 1.;
	}
	if (dsa.y < dsb.y) {
		return -1.;
	}
	if (dsa.y > dsb.y) {
		return 1.;
	}
	return 0.;
}

// Multiply: res = ds_mul(a, b) => res = a * b
float2 ds_mul (float2 dsa, float2 dsb) {
	float2 dsc;
	float c11, c21, c2, e, t1, t2;
	float a1, a2, b1, b2, cona, conb, split = 8193.;

	cona = times_frc(dsa.x, split);
	conb = times_frc(dsb.x, split);
	a1 = minus_frc(cona, minus_frc(cona, dsa.x));
	b1 = minus_frc(conb, minus_frc(conb, dsb.x));
	a2 = minus_frc(dsa.x, a1);
	b2 = minus_frc(dsb.x, b1);

	c11 = times_frc(dsa.x, dsb.x);
	c21 = plus_frc(times_frc(a2, b2), plus_frc(times_frc(a2, b1), plus_frc(times_frc(a1, b2), minus_frc(times_frc(a1, b1), c11))));

	c2 = plus_frc(times_frc(dsa.x, dsb.y), times_frc(dsa.y, dsb.x));

	t1 = plus_frc(c11, c2);
	e = minus_frc(t1, c11);
	t2 = plus_frc(plus_frc(times_frc(dsa.y, dsb.y), plus_frc(minus_frc(c2, e), minus_frc(c11, minus_frc(t1, e)))), c21);

	dsc.x = plus_frc(t1, t2);
	dsc.y = minus_frc(t2, minus_frc(dsc.x, t1));

	return dsc;
}

// Divide: res = ds_div(a, b) => res = a / b
float2 ds_div (float2 dsa, float2 dsb) {
	float2 dsc;
	float a1, a2, b1, b2, cona, conb, split = 8193.;
	float c11, c2, c21, e, s1, s2, t1, t2, t11, t12, t21, t22;

	s1 = div_frc(dsa.x,dsb.x);

	cona = times_frc(s1, split);
	conb = times_frc(dsb.x, split);

	a1 = minus_frc(cona, minus_frc(cona, s1));
	b1 = minus_frc(conb, minus_frc(conb, dsb.x));

	a2 = minus_frc(s1, a1);
	b2 = minus_frc(dsb.x, b1);

	c11 = times_frc(s1, dsb.x);
	c21 = plus_frc(times_frc(a2, b2), plus_frc(times_frc(a2, b1), plus_frc(times_frc(a1, b2), minus_frc(times_frc(a1, b1), c11))));

	c2 = times_frc(s1, dsb.y);

	t1 = plus_frc(c11, c2);
	e = minus_frc(t1, c11);

	t2 = plus_frc(plus_frc(minus_frc(c2, e), minus_frc(c11, minus_frc(t1, e))), c21);

	t12 = plus_frc(t1,t2);
	t22 = minus_frc(t2,minus_frc(t12,t1));

	t11 = minus_frc(dsa.x, t12);
	e = minus_frc(t11, dsa.x);
		
	t21 = minus_frc(plus_frc( plus_frc(minus_frc(-t12,e),minus_frc(dsa.x,minus_frc(t11,e))) ,dsa.y),t22);

	s2 = div_frc(plus_frc(t11,t21), dsb.x);

	dsc.x = plus_frc(s1, s2);
	dsc.y = minus_frc(s2, minus_frc(dsc.x, s1));

	return dsc;
}


float2 ds_sqrt (float2 dsa) {
	float2 dsb;

	float t1, t2, t3;
	float2 f, s0, s1;

	if (dsa.x == 0.0)
	{
		return float2(0.0,0.0);
	}

	t1 = div_frc(1.0, sqrt(dsa.x));
	t2 = times_frc(dsa.x, t1);

	s0 = ds_mul(ds_set(t2),ds_set(t2));
	s1 = ds_sub(dsa, s0);

	t3 = times_frc(0.5, times_frc(s1.x,t1));

	s0.x = t2;
	s0.y = 0.0;
	s1.x = t3;
	s1.y = 0.0;

	dsb = ds_add(s0, s1);

	return dsb;
}


// Couldn't get this to work
// Sets B to the integer part of the DS number A and sets C equal to the
// fractional part of A.  Note that if A = -3.3, then B = -3 and C = -0.3.
float4 ds_infr(float2 dsa) {
	int ic;
	float2 dsb, dsc, con, f, s0, s1;
	float t47, t23;

	t47 = pow(2,47);
	t23 = pow(2,23);

	con = float2(t47, t23); //not sure about this, data operator

	if (dsa.x == 0.0)
		return 0.0;

	//omitted dsa < 2^47 check here, should be fine

	f = float2(1.0,0.0);

	if (dsa.x > 0.0)
	{
		s0 = ds_add(dsa, con);
		dsb = ds_sub(s0,con);
		ic = ds_cmp(dsa, dsb);

		if (ic >= 0.0)
		{
			dsc = ds_sub(dsa,dsb);
		}
		else
		{
			s1 = ds_sub(dsb,f);
			dsb = s1;
			dsc = ds_sub(dsa,dsb);
		}
	}
	else
	{
		s0 = ds_sub(dsa,con);
		dsb = ds_add(s0,con);
		ic = ds_cmp(dsa,dsb);
		if (ic <= 0.0)
		{
			dsc = ds_sub(dsa,dsb);
		}
		else
		{
			s1 = ds_add(dsb,f);
			dsb = s1;
			dsc = ds_sub(dsa,dsb);
		}
	}

	return float4(dsb, dsc);
}

//Couldn't get this to work either, maybe something wrong with t47 and t23?
//   This sets B equal to the integer nearest to the DS number A.
float2 dsn_int (float2 dsa)
{
	float2 dsb, con, s0;
	float t47, t23;

	t47 = pow(2,47);
	t23 = pow(2,23);

	con = float2(t47, t23); //not sure about this, data operator, it may do something low-level, in which case this is screwed

	if (dsa.x == 0.0)
	{
		return float2(0.0,0.0);
	}

	if (dsa.x > 0.0)
	{
		s0  = ds_add(dsa, con);
		dsb = ds_sub(s0, con);
	}
	else
	{
		s0  = ds_sub(dsa,con);
		dsb = ds_add(s0,con);
	}

	return dsb;
}