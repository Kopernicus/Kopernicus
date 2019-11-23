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

        // Grass
        [SerializeField]
        public Texture2D nearGrassTexture;
        [SerializeField]
        public Vector2? nearGrassTextureOffset;
        [SerializeField]
        public Vector2? nearGrassTextureScale;
        [SerializeField]
        public Single? nearGrassTiling;
        [SerializeField]
        public Texture2D farGrassTexture;
        [SerializeField]
        public Vector2? farGrassTextureOffset;
        [SerializeField]
        public Vector2? farGrassTextureScale;
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
        [SerializeField]
        public Color? tarmacColor;

        // Other	
        [SerializeField]
        public Single? opacity;
        [SerializeField]
        public Color? rimColor;
        [SerializeField]
        public Single? rimFalloff;
        [SerializeField]
        public Single? underwaterFogFactor;


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
        public static KSC Instance;

        private void Awake()
        {
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

        // MaterialFixer
        private class MaterialFixer : MonoBehaviour
        {
            private void Update()
            {
                if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                {
                    return;
                }

                Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(m => m?.shader?.name == "KSP/Scenery/Diffuse Ground KSC").ToArray();
                for (int i = materials.Length; i > 0; i--)
                {
                    var material = materials[i - 1];
                    // Grass
                    if (Instance.nearGrassTexture) material.SetTexture("_NearGrassTexture", Instance.nearGrassTexture);
                    if (Instance.nearGrassTextureOffset.HasValue) material.SetTextureOffset("_NearGrassTexture", Instance.nearGrassTextureOffset.Value);
                    if (Instance.nearGrassTextureScale.HasValue) material.SetTextureScale("_NearGrassTexture", Instance.nearGrassTextureScale.Value);
                    if (Instance.nearGrassTiling.HasValue) material.SetFloat("_NearGrassTiling", Instance.nearGrassTiling.Value);
                    if (Instance.farGrassTexture) material.SetTexture("_FarGrassTexture", Instance.farGrassTexture);
                    if (Instance.farGrassTextureOffset.HasValue) material.SetTextureOffset("_FarGrassTexture", Instance.farGrassTextureOffset.Value);
                    if (Instance.farGrassTextureScale.HasValue) material.SetTextureScale("_FarGrassTexture", Instance.farGrassTextureScale.Value);
                    if (Instance.farGrassTiling.HasValue) material.SetFloat("_FarGrassTiling", Instance.farGrassTiling.Value);
                    if (Instance.farGrassBlendDistance.HasValue) material.SetFloat("_FarGrassBlendDistance", Instance.farGrassBlendDistance.Value);
                    if (Instance.grassColor.HasValue) material.SetColor("_GrassColor", Instance.grassColor.Value);
                    // Tarmac                    
                    if (Instance.tarmacTexture) material.SetTexture("_TarmacTexture", Instance.tarmacTexture);
                    if (Instance.tarmacTextureOffset.HasValue) material.SetTextureOffset("_TarmacTexture", Instance.tarmacTextureOffset.Value);
                    if (Instance.tarmacTextureScale.HasValue) material.SetTextureScale("_TarmacTexture", Instance.tarmacTextureScale.Value);
                    if (Instance.tarmacColor.HasValue) material.SetColor("_TarmacColor", Instance.tarmacColor.Value);
                    // Other                    
                    if (Instance.opacity.HasValue) material.SetFloat("_Opacity", Instance.opacity.Value);
                    if (Instance.rimColor.HasValue) material.SetColor("_RimColor", Instance.rimColor.Value);
                    if (Instance.rimFalloff.HasValue) material.SetFloat("_RimFalloff", Instance.rimFalloff.Value);
                    if (Instance.underwaterFogFactor.HasValue) material.SetFloat("_UnderwaterFogFactor", Instance.underwaterFogFactor.Value);
                }

                Destroy(this);
            }
        }
    }
}
