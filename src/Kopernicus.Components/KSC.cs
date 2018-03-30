/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Stores information about the KSC
        /// </summary>
        public class KSC : MonoBehaviour
        {
            // PQSCity
            public Double? latitude;
            public Double? longitude;
            public Vector3? reorientInitialUp;
            public Vector3? repositionRadial;
            public Boolean? repositionToSphere;
            public Boolean? repositionToSphereSurface;
            public Boolean? repositionToSphereSurfaceAddHeight;
            public Boolean? reorientToSphere;
            public Double? repositionRadiusOffset;
            public Double? lodvisibleRangeMult;
            public Single? reorientFinalAngle;

            // PQSMod_MapDecalTangent
            public Vector3? position;
            public Double? radius;
            public Double? heightMapDeformity;
            public Double? absoluteOffset;
            public Boolean? absolute;
            public Double? decalLatitude;
            public Double? decalLongitude;

            // Material
            public Texture2D mainTexture;
            public Color? color;

            // Mods
            private CelestialBody body;
            private PQSCity ksc;
            private PQSMod_MapDecalTangent mapDecal;

            // Apply the patches
            public void Start()
            {
                // Get the KSC-object from Kerbins PQS
                if (FlightGlobals.Bodies != null)
                {
                    body = FlightGlobals.Bodies.First(b => b.isHomeWorld);
                    ksc = body.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");
                    mapDecal = body.pqsController.GetComponentsInChildren<PQSMod_MapDecalTangent>(true).First(m => m.name == "KSC");
                }
                else
                {
                    CelestialBody[] bodies = Resources.FindObjectsOfTypeAll<CelestialBody>();
                    foreach (CelestialBody b in bodies)
                    {
                        if (!b.isHomeWorld)
                            continue;

                        if (b.pqsController == null)
                            continue;

                        ksc = b.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");
                        if (ksc != null)
                        {
                            body = b;
                            mapDecal = body.pqsController.GetComponentsInChildren<PQSMod_MapDecalTangent>(true).First(m => m.name == "KSC");
                            break;
                        }
                    }
                }
                if (ksc == null)
                {
                    Debug.LogError("[Kopernicus] KSC: Unable to find homeworld body with PQSCity named KSC");
                    return;
                }
                if (mapDecal == null)
                {
                    Debug.LogError("[Kopernicus] KSC: Unable to find homeworld body with PQSMod_MapDecalTangent named KSC");
                    return;
                }

                // Load new data into the PQSCity
                if (latitude.HasValue && longitude.HasValue)
                {
                    ksc.repositionRadial = LLAtoECEF(latitude.Value, longitude.Value, 0, body.Radius);
                }
                else if (repositionRadial.HasValue)
                {
                    ksc.repositionRadial = repositionRadial.Value;
                }
                else
                {
                    repositionRadial = ksc.repositionRadial;
                }

                if (reorientInitialUp.HasValue)
                {
                    ksc.reorientInitialUp = reorientInitialUp.Value;
                }
                else
                {
                    reorientInitialUp = ksc.reorientInitialUp;
                }

                if (repositionToSphere.HasValue)
                {
                    ksc.repositionToSphere = repositionToSphere.Value;
                }
                else
                {
                    repositionToSphere = ksc.repositionToSphere;
                }

                if (repositionToSphereSurface.HasValue)
                {
                    ksc.repositionToSphereSurface = repositionToSphereSurface.Value;
                }
                else
                {
                    repositionToSphereSurface = ksc.repositionToSphereSurface;
                }

                if (repositionToSphereSurfaceAddHeight.HasValue)
                {
                    ksc.repositionToSphereSurfaceAddHeight = repositionToSphereSurfaceAddHeight.Value;
                }
                else
                {
                    repositionToSphereSurfaceAddHeight = ksc.repositionToSphereSurfaceAddHeight;
                }

                if (reorientToSphere.HasValue)
                {
                    ksc.reorientToSphere = reorientToSphere.Value;
                }
                else
                {
                    reorientToSphere = ksc.reorientToSphere;
                }

                if (repositionRadiusOffset.HasValue)
                {
                    ksc.repositionRadiusOffset = repositionRadiusOffset.Value;
                }
                else
                {
                    repositionRadiusOffset = ksc.repositionRadiusOffset;
                }

                if (lodvisibleRangeMult.HasValue)
                {
                    foreach (PQSCity.LODRange lodRange in ksc.lod)
                        lodRange.visibleRange *= (Single)lodvisibleRangeMult.Value;
                }
                else
                {
                    lodvisibleRangeMult = 1;
                }

                if (reorientFinalAngle.HasValue)
                {
                    ksc.reorientFinalAngle = reorientFinalAngle.Value;
                }
                else
                {
                    reorientFinalAngle = ksc.reorientFinalAngle;
                }

                // Load new data into the MapDecal
                if (radius.HasValue)
                {
                    mapDecal.radius = radius.Value;
                }
                else
                {
                    radius = mapDecal.radius;
                }

                if (heightMapDeformity.HasValue)
                {
                    mapDecal.heightMapDeformity = heightMapDeformity.Value;
                }
                else
                {
                    heightMapDeformity = mapDecal.heightMapDeformity;
                }

                if (absoluteOffset.HasValue)
                {
                    mapDecal.absoluteOffset = absoluteOffset.Value;
                }
                else
                {
                    absoluteOffset = mapDecal.absoluteOffset;
                }

                if (absolute.HasValue)
                {
                    mapDecal.absolute = absolute.Value;
                }
                else
                {
                    absolute = mapDecal.absolute;
                }

                if (decalLatitude.HasValue && decalLongitude.HasValue)
                {
                    mapDecal.position = LLAtoECEF(decalLatitude.Value, decalLongitude.Value, 0, body.Radius);
                }
                else if (position.HasValue)
                {
                    mapDecal.position = position.Value;
                }
                else
                {
                    position = mapDecal.position;
                }

                // Move the SpaceCenter
                if (SpaceCenter.Instance != null)
                {
                    SpaceCenter.Instance.transform.localPosition = ksc.transform.localPosition;
                    SpaceCenter.Instance.transform.localRotation = ksc.transform.localRotation;

                    // Reset the SpaceCenter
                    SpaceCenter.Instance.Start();
                }
                else
                {
                    Debug.Log("[Kopernicus]: KSC: No SpaceCenter instance!");
                }

                // Add a material fixer
                DontDestroyOnLoad(gameObject.AddComponent<MaterialFixer>());

                // Events
                Events.OnSwitchKSC.Fire(this);
            }

            // Convert latitude-longitude-altitude with body radius to a vector.
            private Vector3 LLAtoECEF(Double lat, Double lon, Double alt, Double radius)
            {
                const Double degreesToRadians = Math.PI / 180.0;
                lat = (lat - 90) * degreesToRadians;
                lon *= degreesToRadians;
                Double x, y, z;
                Double n = radius; // for now, it's still a sphere, so just the radius
                x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
                y = (n + alt) * Math.Cos(lat); // for now, it's still a sphere, so no eccentricity
                z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
                return new Vector3((Single)x, (Single)y, (Single)z);
            }

            // MaterialFixer
            private class MaterialFixer : MonoBehaviour
            {
                void Update()
                {
                    if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                        return;

                    KSC ksc = GetComponent<KSC>();

                    // Loop through all Materials and change their settings
                    try
                    {
                        foreach (Material material in Resources.FindObjectsOfTypeAll<Material>().Where(m => m.color.ToString() == new Color(0.640f, 0.728f, 0.171f, 0.729f).ToString()))
                        {
                            // Patch the texture
                            if (ksc.mainTexture != null)
                            {
                                material.mainTexture = ksc.mainTexture;
                            }
                            else
                            {
                                ksc.mainTexture = material.mainTexture as Texture2D;
                            }
    
                            // Patch the color
                            if (ksc.color.HasValue)
                            {
                                material.color = ksc.color.Value;
                            }
                            else
                            {
                                ksc.color = material.color;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("[Kopernicus]: MaterialFixer: Exception " + e);
                    }
                    Destroy(this);
                }
            }
        }
    }
}
