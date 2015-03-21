// StarSystems code -- OvenProofMars, Fastwings, medsouz -- modified by Thomas P.

using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    public class StarLightSwitcher : MonoBehaviour
    {
        private static Dictionary<CelestialBody, CelestialBody> StarDistance = new Dictionary<CelestialBody, CelestialBody>();
        private double DistanceCB;
        private double DistanceStar;

        public void AddStar(CelestialBody StarCB)
        {
            StarDistance[StarCB] = StarCB;
        }

        void Update()
        {
            Vector3 position = Vector3.zero;
            if (PlanetariumCamera.fetch.enabled == true)
                position = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.fetch.GetCameraTransform().position);
            else if (FlightGlobals.ActiveVessel != null)
                position = FlightGlobals.ActiveVessel.GetTransform().position;
            if (position != Vector3.zero)
            {
                foreach (CelestialBody CB in StarDistance.Keys)
                {
                    //Compare distance between active star and star
                    DistanceCB = FlightGlobals.getAltitudeAtPos(position, CB);
                    DistanceStar = FlightGlobals.getAltitudeAtPos(position, Sun.Instance.sun);
                    if (DistanceCB < DistanceStar && Sun.Instance.sun != CB)
                    {
                        setSun(CB);
                    }
                }
            }
        }

        public static void setSun(CelestialBody CB)
        {
            if (StarDistance[CB] != null)
            {
                if (!StarDistance[CB].light.gameObject.activeSelf)
                    return;
            }
            //Set star as active star
            Sun.Instance.sun = CB;
            Planetarium.fetch.Sun = CB;
            Debug.Log("Active sun set to: " + CB.name);

            //Set sunflare color
            if (StarDistance[CB] != null)
            {
                Sun.Instance.sunFlare.color = StarDistance[CB].light.color;
            }
            else
            {
                Sun.Instance.sunFlare.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }

            //Reset solar panels (Credit to Kcreator)
            foreach (ModuleDeployableSolarPanel panel in FindObjectsOfType(typeof(ModuleDeployableSolarPanel)))
            {
                panel.OnStart(PartModule.StartState.Orbital);
            }
        }
    }
}
