using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    // Class to replace the PartBuoyancy component of parts with our own implementation
    // TL;DR: Support for non-spherical oceans
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class OceanUtility : MonoBehaviour
    {
        void Awake()
        {
            // Add the handler
            GameEvents.onPartUnpack.Add(OnPartUnpack);

            // We have to work, Garbage Collector
            DontDestroyOnLoad(this);
        }

        void OnDestroy()
        {
            // Remove the Handler
            GameEvents.onPartUnpack.Remove(OnPartUnpack);
        }

        void OnPartUnpack(Part part)
        {
            // If there's nothing to do, abort
            if (part.partBuoyancy == null)
                return;

            // Replace PartBuoyancy with KopernicusBuoyancy
            KopernicusBuoyancy buoyancy = part.gameObject.AddComponent<KopernicusBuoyancy>();
            Utility.CopyObjectFields(part.partBuoyancy, buoyancy, false);
            part.partBuoyancy = buoyancy;
            Destroy(part.GetComponent<PartBuoyancy>());
        }
    }

    // Our own implementation of PartBuoyancy
    public class KopernicusBuoyancy : PartBuoyancy
    {
        public Part part { get { return GetComponent<Part>(); } }

        private PQS _ocean;
        private CelestialBody mainBody;

        private void FixedUpdate()
        {
            if (!FlightGlobals.ready || !part.vessel.mainBody.ocean)
                return;

            if (!part.rb)
                return;

            if (GetOcean() == null)
                return;

            // Get the center of the Buoyancy
            Vector3 oldCenter = centerOfBuoyancy;
            centerOfBuoyancy = part.partTransform.position + (part.partTransform.rotation * part.CenterOfBuoyancy);

            // Get the current altitude
            double altitude = GetAltitudeFromOcean(centerOfBuoyancy);

            // Get the force
            buoyancyForce = Mathf.Max(0f, (float)-altitude);
            float dot = 0f;
            
            // If we are already splashed
            if (buoyancyForce <= 0f || part.State == PartStates.DEAD)
            {
                if (splashed)
                {
                    part.WaterContact = false;
                    part.vessel.checkSplashed();
                    splashed = false;
                    rigidbody.drag = 0f;
                }
            }
            else
            {
                if (!splashed)
                {
                    // Get the speed of the part
                    double speed = part.rb.velocity.sqrMagnitude + Krakensbane.GetFrameVelocity().sqrMagnitude;

                    if (speed > 4 && Vector3.Distance(part.partTransform.position, FlightGlobals.camera_position) < 5000f)
                    {
                        // Splash
                        Vector3 position = this.centerOfBuoyancy - (oldCenter - (Krakensbane.GetFrameVelocityV3f() * TimeWarp.fixedDeltaTime));
                        dot = Mathf.Abs(Vector3.Dot(position, FlightGlobals.getUpAxis(FlightGlobals.currentMainBody, this.centerOfBuoyancy)));
                        Vector3 finalPositon = centerOfBuoyancy - (dot == 0f ? Vector3.zero : position * (buoyancyForce / dot));
                        FXMonger.Splash(finalPositon, part.rb.velocity.magnitude / 10f);
                    }

                    // Set spalshed to true
                    splashed = true;

                    // Destroy the part
                    if (speed > part.crashTolerance * part.crashTolerance * 1.2f)
                    {
                        GameEvents.onCrashSplashdown.Fire(new EventReport(FlightEvents.SPLASHDOWN_CRASH, part, part.partInfo.title, "an unidentified object", 0, string.Empty, 0f));
                        part.Die();
                        return;
                    }

                    // Set the part to splashed down
                    part.WaterContact = true;
                    part.vessel.Splashed = true;
                    if (IsInvoking())
                        CancelInvoke();
                    Invoke("ReportSplashDown", 2f);
                }
                 
                // Set forces
                effectiveForce = (-FlightGlobals.getGeeForceAtPosition(part.partTransform.position) * (dot == 0f ? buoyancy : dot)) * Mathf.Min(buoyancyForce, 1.5f);
                part.rb.drag = 3f;
                part.rb.AddForceAtPosition(effectiveForce, centerOfBuoyancy, 0);
            }
        }
        
        private float GetAltitudeFromOcean(Vector3 position)
        {
            PQS ocean = GetOcean();
            return Mathf.Abs(Vector3.Distance(position, mainBody.position)) - (float)ocean.GetSurfaceHeight(ocean.GetRelativePosition(position));
        }

        private PQS GetOcean()
        {
            if (_ocean == null || mainBody != part.vessel.mainBody)
            {
                mainBody = part.vessel.mainBody;
                _ocean = mainBody.GetComponentsInChildren<PQS>(true).FirstOrDefault(p => p.name == mainBody.transform.name + "Ocean");
            }
            return _ocean;
        }
    }
}
