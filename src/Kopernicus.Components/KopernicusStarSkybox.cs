#if FALSE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
	    /// <summary>
	    /// Modifies the Skybox Animation to support multiple stars
	    /// </summary>
	    public class KopernicusStarGalaxyCubeControl : MonoBehaviour
	    {
		    private GalaxyCubeControl _animation;

		    private List<Single> gFades = new List<Single>();

		    private List<Single> aFades = new List<Single>();

		    private List<Single> dFades = new List<Single>();

		    private Single _lastGFade;

		    void LateUpdate()
		    {
			    if (_animation == null)
			    {
				    _animation = GetComponent<GalaxyCubeControl>();
			    }

			    gFades.Clear();
			    aFades.Clear();
			    dFades.Clear();

			    for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
			    {
				    KopernicusStar star = KopernicusStar.Stars[i];
				    if (InLineWith(star))
				    {
					    Double angle =
						    Math.Acos(Vector3.Dot(-star.sunDirection, _animation.tgt.transform.forward)) *
						    (180 / Math.PI);
					    if (Double.IsNaN(angle))
					    {
						    angle = 0.0;
					    }

					    Double fov = _animation.tgt.fieldOfView * 0.5;
					    Single t = angle < fov ? 0f : Mathf.Clamp01((Single) ((angle + 10.0 - fov) * 0.05));

					    gFades.Add(Mathf.Lerp(_lastGFade, Mathf.Lerp(_animation.glareFadeLimit, 0f, t), _animation.glareFadeLerpRate));
				    }
				    else
				    {
					    gFades.Add(Mathf.Lerp(_lastGFade, 0f, _animation.glareFadeLerpRate));
				    }

				    if ((HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.FLIGHT) &&
				        MapView.MapIsEnabled)
				    {
					    aFades.Add(0f);
					    dFades.Add(0f);
				    }
				    else
				    {
					    Vector3d localPosition = ScaledSpace.ScaledToLocalSpace(_animation.tgt.transform.position);
					    CelestialBody mainBody = FlightGlobals.getMainBody(localPosition);
					    Double altitude = mainBody.GetAltitude(localPosition);
					    Double pressure = FlightGlobals.getStaticPressure(altitude, mainBody) * 0.0098692326671601278;
					    if (pressure > 0.0)
					    {
						    Double sunSrfAngle =
							    Math.Acos(Vector3.Dot(-star.sunDirection, FlightGlobals.getUpAxis(localPosition))) *
							    (180 / Math.PI);
						    sunSrfAngle = UtilMath.Clamp01((sunSrfAngle - 75.0) * 0.025);
						    if (Double.IsNaN(sunSrfAngle))
						    {
							    sunSrfAngle = 0.0;
						    }

						    dFades.Add((float) pressure * Mathf.Lerp(_animation.daytimeFadeLimit, 0f, (float) sunSrfAngle));
						    aFades.Add(Mathf.Lerp(0f, _animation.atmosFadeLimit, (float) pressure * _animation.airPressureFade));
					    }
					    else
					    {
						    dFades.Add(0f);
						    aFades.Add(0f);
					    }
				    }

				    Color galaxyColor = Color.Lerp(_animation.maxGalaxyColor, _animation.minGalaxyColor,
					    aFades.Max() + dFades.Max() + (_lastGFade = gFades.Max()));
				    foreach (Renderer renderer in _animation.GetComponentsInChildren<Renderer>())
				    {
					    renderer.material.SetColor("_Color", galaxyColor);
				    }
			    }
		    }

		    public Boolean InLineWith(KopernicusStar star)
		    {
			    Vector3 direction = star.sun.scaledBody.transform.position - ScaledCamera.Instance.transform.position;
			    return !Physics.Raycast(new Ray(ScaledCamera.Instance.transform.position, direction), out RaycastHit raycastHit,
				           direction.magnitude, 1 << 10) || raycastHit.collider.gameObject == star.sun.scaledBody;
		    }
	    }

	    /// <summary>
        /// Modifies the Skybox Animation to support multiple stars
        /// </summary>
        public class KopernicusStarSkySphereControl : MonoBehaviour
        {
            private SkySphereControl _animation;
	        
	        private List<Single> dots = new List<Single>();

	        void LateUpdate()
	        {
		        if (_animation == null)
		        {
			        _animation = GetComponent<SkySphereControl>();
		        }

		        dots.Clear();
		        for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
		        {
			        KopernicusStar star = KopernicusStar.Stars[i];
			        dots.Add(Vector3.Dot(-star.sunDirection,
				                 FlightGlobals.getUpAxis(ScaledSpace.ScaledToLocalSpace(_animation.tgt.transform.position))) *
			                 star.RelativeIntensity);
		        }

		        Double sunSrfAngle = Math.Acos(dots.Min()) * (180 / Math.PI);
		        sunSrfAngle = Math.Max(0.0, Math.Min(1.0, (sunSrfAngle - 80.0) / 20.0));
		        if (Double.IsNaN(sunSrfAngle))
		        {
			        sunSrfAngle = 0.0;
		        }

		        dots.Clear();
		        for (Int32 i = 0; i < KopernicusStar.Stars.Count; i++)
		        {
			        KopernicusStar star = KopernicusStar.Stars[i];
			        dots.Add(Vector3.Dot(-star.sunDirection, _animation.tgt.transform.forward) * star.RelativeIntensity);
		        }

		        Double sunCamAngle = Math.Acos(dots.Min()) * (180 / Math.PI);
		        sunCamAngle = Math.Max(0.0,
			        Math.Min(1.0, (sunCamAngle - _animation.tgt.fieldOfView * 0.5) / 20.0));

		        Renderer renderer = GetComponent<Renderer>();
		        renderer.material.SetFloat("_dayNightBlend", (Single) sunSrfAngle);
		        renderer.material.SetColor("_Color2",
			        !MapView.MapIsEnabled
				        ? Color.Lerp(_animation.dayTimeSpaceColorShift, Color.white, (Single) sunCamAngle)
				        : Color.white);
		        renderer.material.SetFloat("_spaceBlend",
			        Mathf.InverseLerp(_animation.atmosphereLimit * _animation.skyFadeStart, _animation.atmosphereLimit,
				        (Single) FlightGlobals.getAltitudeAtPos(
					        ScaledSpace.ScaledToLocalSpace(_animation.tgt.transform.position))));
	        }
        }
    }
}
#endif