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
using UnityEngine.Networking;

namespace Kopernicus
{
    namespace Components
    {
        /// <summary>
        /// Stores information about the KSC
        /// </summary>
        public class KSC : SerializableMonoBehaviour
        {
            // PQSCity
            [SerializeField]
            public Double? latitude;
            [SerializeField]
            public Double? longitude;
            [SerializeField]
            public Vector3? reorientInitialUp;
            [SerializeField]
            public Vector3? repositionRadial;
            [SerializeField]
            public Boolean? repositionToSphere;
            [SerializeField]
            public Boolean? repositionToSphereSurface;
            [SerializeField]
            public Boolean? repositionToSphereSurfaceAddHeight;
            [SerializeField]
            public Boolean? reorientToSphere;
            [SerializeField]
            public Double? repositionRadiusOffset;
            [SerializeField]
            public Double? lodvisibleRangeMult;
            [SerializeField]
            public Single? reorientFinalAngle;

            // PQSMod_MapDecalTangent
            [SerializeField]
            public Vector3? position;
            [SerializeField]
            public Double? radius;
            [SerializeField]
            public Double? heightMapDeformity;
            [SerializeField]
            public Double? absoluteOffset;
            [SerializeField]
            public Boolean? absolute;
            [SerializeField]
            public Double? decalLatitude;
            [SerializeField]
            public Double? decalLongitude;

            // PQSCity Ground Material
            [SerializeField]
            public Texture2D mainTexture;
            [SerializeField]
            public Color? color;

            // Editor Ground Material
            [SerializeField]
            public Texture2D editorGroundTex;
            [SerializeField]
            public Color? editorGroundColor;
            [SerializeField]
            public Vector2? editorGroundTexScale;
            [SerializeField]
            public Vector2? editorGroundTexOffset;

            public Material groundMaterial;

            // Current Instance
            public static KSC Instance = null;

            void Awake()
            {
                Instance = this;
            }

            // Mods
            private CelestialBody body;
            private PQSCity ksc;
            private PQSMod_MapDecalTangent mapDecal;

            // Apply the patches
            public void Start()
            {
                body = GetComponent<CelestialBody>();
                if (!body.isHomeWorld)
                {
                    Destroy(this);
                    return;
                }

                ksc = body.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");
                mapDecal = body.pqsController.GetComponentsInChildren<PQSMod_MapDecalTangent>(true)
                    .First(m => m.name == "KSC");

                if (ksc == null)
                {
                    Debug.LogError("[Kopernicus] KSC: Unable to find homeworld body with PQSCity named KSC");
                    return;
                }

                if (mapDecal == null)
                {
                    Debug.LogError(
                        "[Kopernicus] KSC: Unable to find homeworld body with PQSMod_MapDecalTangent named KSC");
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
                        Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

                        int? n = materials?.Length;

                        for (int i = 0; i < n; i++)
                        {
                            Material material = materials[i];

                            if (material?.name == "ksc_exterior_terrain_grass_02")
                            {
                                if (ksc.groundMaterial != null)
                                {
                                    material.shader = ksc.groundMaterial.shader;
                                    material.CopyPropertiesFromMaterial(ksc.groundMaterial);
                                }
                                else
                                {
                                    // Remember this material
                                    ksc.groundMaterial = material;

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

        [KSPAddon(KSPAddon.Startup.EditorAny, false)]
        public class EditorMaterialFixer : MonoBehaviour
        {
            EditorFacility editor;

            void Update()
            {
                if (editor != EditorDriver.editorFacility)
                {
                    editor = EditorDriver.editorFacility;
                    FixMaterials();
                }
            }

            void FixMaterials()
            {
                KSC ksc = KSC.Instance;

                if (ksc == null) return;

                Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

                int? n = materials?.Length;

                for (int i = 0; i < n; i++)
                {
                    Material material = materials[i];

                    if (material?.name == "ksc_exterior_terrain_grass_02")
                    {
                        if (ksc.groundMaterial != null)
                        {
                            material.shader = ksc.groundMaterial.shader;
                            material.CopyPropertiesFromMaterial(ksc.groundMaterial);
                        }
                    }

                    else

                    if (material?.name == "ksc_terrain_TX")
                    {
                        if (ksc.groundMaterial != null)
                        {
                            material.shader = ksc.groundMaterial.shader;
                            material.CopyPropertiesFromMaterial(ksc.groundMaterial);
                        }

                        if (ksc.editorGroundColor.HasValue)
                        {
                            material.color = ksc.editorGroundColor.Value;
                        }

                        if (ksc.editorGroundTex != null)
                        {
                            material.mainTexture = ksc.editorGroundTex;
                        }

                        if (ksc.editorGroundTexScale.HasValue)
                        {
                            material.mainTextureScale = ksc.editorGroundTexScale.Value;
                        }

                        if (ksc.editorGroundTexOffset.HasValue)
                        {
                            material.mainTextureOffset = ksc.editorGroundTexOffset.Value;
                        }
                    }
                }
            }
        }
    }
}
