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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kopernicus.Components.Serialization;
using UnityEngine;

namespace Kopernicus.Components
{
    /// <summary>
    /// Stores information about the KSC
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class KSC : SerializableMonoBehaviour
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

        // PQSCity Ground Material
        public Texture2D mainTexture;
        public Color? color;

        // Grass Material
        public GrassMaterial Material;

        // Editor Ground Material
        public Texture2D editorGroundTex;
        public Color? editorGroundColor;
        public Vector2? editorGroundTexScale;
        public Vector2? editorGroundTexOffset;

        // Current Instance
        public static KSC Instance;

        private void Awake()
        {
            if (Material == null)
            {
                Material = new GrassMaterial();
            }

            Instance = this;
        }

        // Mods
        private CelestialBody _body;
        private PQSCity _ksc;
        private PQSMod_MapDecalTangent _mapDecal;

        // Apply the patches
        public void Start()
        {
            _body = GetComponent<CelestialBody>();
            if (!_body.isHomeWorld)
            {
                Destroy(this);
                return;
            }

            _ksc = _body.pqsController.GetComponentsInChildren<PQSCity>(true).First(m => m.name == "KSC");
            _mapDecal = _body.pqsController.GetComponentsInChildren<PQSMod_MapDecalTangent>(true)
                .First(m => m.name == "KSC");

            if (_ksc == null)
            {
                Debug.LogError("[Kopernicus] KSC: Unable to find homeworld body with PQSCity named KSC");
                return;
            }

            if (_mapDecal == null)
            {
                Debug.LogError(
                    "[Kopernicus] KSC: Unable to find homeworld body with PQSMod_MapDecalTangent named KSC");
                return;
            }

            // Load new data into the PQSCity
            if (latitude.HasValue && longitude.HasValue)
            {
                _ksc.repositionRadial = Utility.LLAtoECEF(latitude.Value, longitude.Value, 0, _body.Radius);
            }
            else if (repositionRadial.HasValue)
            {
                _ksc.repositionRadial = repositionRadial.Value;
            }
            else
            {
                repositionRadial = _ksc.repositionRadial;
            }

            if (reorientInitialUp.HasValue)
            {
                _ksc.reorientInitialUp = reorientInitialUp.Value;
            }
            else
            {
                reorientInitialUp = _ksc.reorientInitialUp;
            }

            if (repositionToSphere.HasValue)
            {
                _ksc.repositionToSphere = repositionToSphere.Value;
            }
            else
            {
                repositionToSphere = _ksc.repositionToSphere;
            }

            if (repositionToSphereSurface.HasValue)
            {
                _ksc.repositionToSphereSurface = repositionToSphereSurface.Value;
            }
            else
            {
                repositionToSphereSurface = _ksc.repositionToSphereSurface;
            }

            if (repositionToSphereSurfaceAddHeight.HasValue)
            {
                _ksc.repositionToSphereSurfaceAddHeight = repositionToSphereSurfaceAddHeight.Value;
            }
            else
            {
                repositionToSphereSurfaceAddHeight = _ksc.repositionToSphereSurfaceAddHeight;
            }

            if (reorientToSphere.HasValue)
            {
                _ksc.reorientToSphere = reorientToSphere.Value;
            }
            else
            {
                reorientToSphere = _ksc.reorientToSphere;
            }

            if (repositionRadiusOffset.HasValue)
            {
                _ksc.repositionRadiusOffset = repositionRadiusOffset.Value;
            }
            else
            {
                repositionRadiusOffset = _ksc.repositionRadiusOffset;
            }

            if (lodvisibleRangeMult.HasValue)
            {
                foreach (PQSCity.LODRange lodRange in _ksc.lod)
                {
                    lodRange.visibleRange *= (Single)lodvisibleRangeMult.Value;
                }
            }
            else
            {
                lodvisibleRangeMult = 1;
            }

            if (reorientFinalAngle.HasValue)
            {
                _ksc.reorientFinalAngle = reorientFinalAngle.Value;
            }
            else
            {
                reorientFinalAngle = _ksc.reorientFinalAngle;
            }

            // Load new data into the MapDecal
            if (radius.HasValue)
            {
                _mapDecal.radius = radius.Value;
            }
            else
            {
                radius = _mapDecal.radius;
            }

            if (heightMapDeformity.HasValue)
            {
                _mapDecal.heightMapDeformity = heightMapDeformity.Value;
            }
            else
            {
                heightMapDeformity = _mapDecal.heightMapDeformity;
            }

            if (absoluteOffset.HasValue)
            {
                _mapDecal.absoluteOffset = absoluteOffset.Value;
            }
            else
            {
                absoluteOffset = _mapDecal.absoluteOffset;
            }

            if (absolute.HasValue)
            {
                _mapDecal.absolute = absolute.Value;
            }
            else
            {
                absolute = _mapDecal.absolute;
            }

            if (decalLatitude.HasValue && decalLongitude.HasValue)
            {
                _mapDecal.position = Utility.LLAtoECEF(decalLatitude.Value, decalLongitude.Value, 0, _body.Radius);
            }
            else if (position.HasValue)
            {
                _mapDecal.position = position.Value;
            }
            else
            {
                position = _mapDecal.position;
            }

            // Move the SpaceCenter
            if (SpaceCenter.Instance != null)
            {
                Transform spaceCenterTransform = SpaceCenter.Instance.transform;
                Transform kscTransform = _ksc.transform;
                spaceCenterTransform.localPosition = kscTransform.localPosition;
                spaceCenterTransform.localRotation = kscTransform.localRotation;

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

        // Material
        public class GrassMaterial
        {
            // Grass
            [SerializeField]
            public Texture2D nearGrassTexture;
            [SerializeField]
            public Single? nearGrassTiling;
            [SerializeField]
            public Texture2D farGrassTexture;
            [SerializeField]
            public Single? farGrassTiling;
            [SerializeField]
            public Single? farGrassBlendDistance;
            [SerializeField]
            public Color? grassColor;

            // Tarmac
            [SerializeField]
            public Texture2D tarmacTexture;
            [SerializeField]
            public Vector2? tarmacTextureOffset;
            [SerializeField]
            public Vector2? tarmacTextureScale;

            // Other	
            [SerializeField]
            public Single? opacity;
            [SerializeField]
            public Color? rimColor;
            [SerializeField]
            public Single? rimFalloff;
            [SerializeField]
            public Single? underwaterFogFactor;
        }

        // MaterialFixer
        private class MaterialFixer : MonoBehaviour
        {
            private void Update()
            {
                if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                {
                    return;
                }

                Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(m => (m.shader.name.Contains("Ground KSC"))).ToArray();
                for (int i = materials.Length; i > 0; i--)
                {
                    var material = materials[i - 1];

                    // Grass
                    if (Instance.mainTexture) material.SetTexture("_NearGrassTexture", Instance.mainTexture);
                    if (Instance.Material.nearGrassTexture) material.SetTexture("_NearGrassTexture", Instance.Material.nearGrassTexture);
                    if (Instance.Material.nearGrassTiling.HasValue) material.SetFloat("_NearGrassTiling", material.GetFloat("_NearGrassTiling") * Instance.Material.nearGrassTiling.Value);
                    if (Instance.Material.farGrassTexture) material.SetTexture("_FarGrassTexture", Instance.Material.farGrassTexture);
                    if (Instance.Material.farGrassTiling.HasValue) material.SetFloat("_FarGrassTiling", material.GetFloat("_FarGrassTiling") * Instance.Material.farGrassTiling.Value);
                    if (Instance.Material.farGrassBlendDistance.HasValue) material.SetFloat("_FarGrassBlendDistance", Instance.Material.farGrassBlendDistance.Value);
                    if (Instance.color.HasValue) material.SetColor("_GrassColor", Instance.color.Value);
                    if (Instance.Material.grassColor.HasValue) material.SetColor("_GrassColor", Instance.Material.grassColor.Value);
                    // Tarmac
                    if (Instance.Material.tarmacTexture) material.SetTexture("_TarmacTexture", Instance.Material.tarmacTexture);
                    if (Instance.Material.tarmacTextureOffset.HasValue) material.SetTextureOffset("_TarmacTexture", Instance.Material.tarmacTextureOffset.Value);
                    if (Instance.Material.tarmacTextureScale.HasValue) material.SetTextureScale("_TarmacTexture", material.GetTextureScale("_TarmacTexture") * Instance.Material.tarmacTextureScale.Value);
                    // Other
                    if (Instance.Material.opacity.HasValue) material.SetFloat("_Opacity", Instance.Material.opacity.Value);
                    if (Instance.Material.rimColor.HasValue) material.SetColor("_RimColor", Instance.Material.rimColor.Value);
                    if (Instance.Material.rimFalloff.HasValue) material.SetFloat("_RimFalloff", Instance.Material.rimFalloff.Value);
                    if (Instance.Material.underwaterFogFactor.HasValue) material.SetFloat("_UnderwaterFogFactor", Instance.Material.underwaterFogFactor.Value);
                }

                Destroy(this);
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorMaterialFixer : MonoBehaviour
    {
        private void Start()
        {
            KSC ksc = KSC.Instance;
            if (!ksc)
            {
                return;
            }

            GameObject scenery = (GameObject.Find("VABscenery") != null)
                ? GameObject.Find("VABscenery")
                : GameObject.Find("SPHscenery");
            Material material = null;
            if (scenery != null) material = scenery.GetChild("ksc_terrain").GetComponent<Renderer>().sharedMaterial;

            if (material == null)
            {
                return;
            }

            if (ksc.editorGroundColor.HasValue)
            {
                material.color = ksc.editorGroundColor.Value;
            }

            if (ksc.editorGroundTex)
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
