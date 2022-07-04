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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.Components.Serialization;
using Kopernicus.Configuration.ModularScatterLoader;
using Kopernicus.Constants;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Component to add other Components to Scatter objects easily
    /// </summary>
    public class ModularScatter : SerializableMonoBehaviour, IComponentSystem<ModularScatter>
    {
        /// <summary>
        /// Components that can be added to the Ring
        /// </summary>
        public List<IComponent<ModularScatter>> Components
        {
            get { return components; }
            set { components = value; }
        }
        /// <summary>
        /// The celestial body we are attached to
        /// </summary>
        public CelestialBody body;

        /// <summary>
        /// The mod we are attached to
        /// </summary>
        public PQSLandControl landControl;

        /// <summary>
        /// The scatter instance we are attached to
        /// </summary>
        public PQSLandControl.LandClassScatter scatter;

        /// <summary>
        /// The actual models that are scattered over the surface. Only populated if
        /// there is at least one component defined.
        /// </summary>
        public List<GameObject> scatterObjects;

        /// <summary>
        /// Whether to treat the density calculation as an actual floating point value
        /// </summary>
        public Boolean useBetterDensity;

        /// <summary>
        /// What biomes this scatter may spawn in.  Empty means all.
        /// </summary>
        public List<String> allowedBiomes = new List<String>();

        /// <summary>
        /// Makes the density calculation ignore the game setting for scatter density
        /// </summary>
        public Boolean ignoreDensityGameSetting;

        /// <summary>
        /// How much variation should the scatter density have?
        /// </summary>
        public List<Single> densityVariance = new List<Single> { -0.5f, 0.5f };

        /// <summary>
        /// How much should the scatter be able to rotate
        /// </summary>
        public List<Single> rotation = new List<Single> { 0, 360f };

        /// <summary>
        /// How large is the chance that a scatter object spawns on a quad?
        /// </summary>
        public Single spawnChance = 1f;

        /// <summary>
        /// A list of all meshes that can be used for the
        /// </summary>
        public List<Mesh> meshes = new List<Mesh>();

        /// <summary>
        /// The base mesh for the scatter.
        /// </summary>
        public Mesh baseMesh;

        /// <summary>
        /// A shared instance for a cube mesh, that is used to replace any mesh that KSP would want to spawn itself
        /// </summary>
        private static Mesh _cubeMesh;

        private Dictionary<PQSMod_LandClassScatterQuad, int> scatterQuads = new Dictionary<PQSMod_LandClassScatterQuad, int>(128);
        private HashSet<GameObject> destroyedScatters = new HashSet<GameObject>();

        private static bool reflectionSetupDone = false;
        private static Func<PQSLandControl.LandClassScatter, List<PQSMod_LandClassScatterQuad>> GetAssignedQuads;
        private static Func<PQSLandControl.LandClassScatter, List<PQSMod_LandClassScatterQuad>> GetUnassignedQuads;

        private static void SetupReflection()
        {
            if (reflectionSetupDone)
                return;

            FieldInfo f_cacheAssigned = typeof(PQSLandControl.LandClassScatter).GetField("cacheAssigned", BindingFlags.Instance | BindingFlags.NonPublic);
            GetAssignedQuads = Utility.CreateGetter<PQSLandControl.LandClassScatter, List<PQSMod_LandClassScatterQuad>>(f_cacheAssigned);

            FieldInfo f_cacheUnassigned = typeof(PQSLandControl.LandClassScatter).GetField("cacheUnassigned", BindingFlags.Instance | BindingFlags.NonPublic);
            GetUnassignedQuads = Utility.CreateGetter<PQSLandControl.LandClassScatter, List<PQSMod_LandClassScatterQuad>>(f_cacheUnassigned);

            reflectionSetupDone = true;
        }

        [SerializeField]
        private List<IComponent<ModularScatter>> components;

        /// <summary>
        /// Create a new ScatterExtension
        /// </summary>
        public ModularScatter()
        {
            Components = new List<IComponent<ModularScatter>>();
        }

        /// <summary>
        /// Create colliders for the scatter
        /// </summary>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void Start()
        {
            SetupReflection();

            // Apply the modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Apply(this);
            }

            // Register us as the parental object for the scatter
            landControl = transform.parent.GetComponent<PQSLandControl>();
            transform.parent = landControl.sphere.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            scatter = landControl.scatters.First(s => s.scatterName == scatter.scatterName); // I hate Unity
            typeof(PQSLandControl.LandClassScatter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.FieldType == typeof(GameObject))?.SetValue(scatter, gameObject);
            scatterObjects = new List<GameObject>();
            body = Part.GetComponentUpwards<CelestialBody>(landControl.gameObject);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            if (!baseMesh && meshes.Count > 0)
            {
                baseMesh = meshes[0];
            }

            // Generate a dummy mesh that is used to replace the mesh KSP want's to render itself.
            if (!_cubeMesh)
            {
                Vector3[] vertices =
                {
                    new Vector3 (0, 0, 0),
                    new Vector3 (1, 0, 0),
                    new Vector3 (1, 1, 0),
                    new Vector3 (0, 1, 0),
                    new Vector3 (0, 1, 1),
                    new Vector3 (1, 1, 1),
                    new Vector3 (1, 0, 1),
                    new Vector3 (0, 0, 1),
                };

                Int32[] triangles =
                {
                    0, 2, 1, //face front
                    0, 3, 2,
                    2, 3, 4, //face top
                    2, 4, 5,
                    1, 2, 5, //face right
                    1, 5, 6,
                    0, 7, 4, //face left
                    0, 4, 3,
                    5, 4, 7, //face back
                    5, 7, 6,
                    0, 6, 7, //face bottom
                    0, 1, 6
                };

                _cubeMesh = new Mesh { vertices = vertices, triangles = triangles, name = "Kopernicus-CubeDummy" };
            }

            scatter.baseMesh = _cubeMesh;

            // PostApply for the Modules
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].PostApply(this);
            }
        }

        /// <summary>
        /// Destroy the generated objects on a scene change so they don't appear in random positions
        /// </summary>
        private void OnGameSceneLoadRequested(GameScenes data)
        {
            foreach (GameObject scatterObj in scatterObjects)
            {
                Destroy(scatterObj);
            }

            scatterQuads.Clear();
            destroyedScatters.Clear();
            scatterObjects.Clear();
        }

        private void Update()
        {
            if (FlightGlobals.currentMainBody != body)
                return;

            // Reprocess the stock scatter models, since they are merged into
            // one gigantic mesh per quad, but we want unique objects
            foreach (PQSMod_LandClassScatterQuad assignedQuad in GetAssignedQuads(scatter))
            {
                if (!scatterQuads.TryGetValue(assignedQuad, out int quadId))
                {
                    // disable the stock mesh renderer (only need to do it once)
                    if (assignedQuad.mr && assignedQuad.mr.enabled)
                        assignedQuad.mr.enabled = false;

                    CreateScatterMeshes(assignedQuad);
                    assignedQuad.mesh.Clear();
                    scatterQuads.Add(assignedQuad, assignedQuad.quad.id);
                }
                else if (quadId != assignedQuad.quad.id)
                {
                    Transform quadTransform = assignedQuad.transform;
                    for (int i = quadTransform.childCount; i-- > 0;)
                    {
                        destroyedScatters.Add(quadTransform.GetChild(i).gameObject);
                    }

                    CreateScatterMeshes(assignedQuad);
                    assignedQuad.mesh.Clear();
                    scatterQuads[assignedQuad] = assignedQuad.quad.id;
                }
            }

            foreach (PQSMod_LandClassScatterQuad unassignedQuad in GetUnassignedQuads(scatter))
            {
                if (scatterQuads.Remove(unassignedQuad))
                {
                    Transform quadTransform = unassignedQuad.transform;
                    for (int i = quadTransform.childCount; i-- > 0;)
                    {
                        destroyedScatters.Add(quadTransform.GetChild(i).gameObject);
                    }
                }
            }

            if (destroyedScatters.Count > 0)
            {
                for (int i = scatterObjects.Count; i-- > 0;)
                {
                    if (destroyedScatters.Remove(scatterObjects[i]))
                    {
                        Destroy(scatterObjects[i]);
                        scatterObjects.RemoveAt(i);
                    }

                    if (destroyedScatters.Count == 0)
                        break;
                }
                destroyedScatters.Clear();
            }

            // Update components
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update(this);
            }
        }

        /// <summary>
        /// By default, KSP merges all scatters that are on one quad into one gigantic mesh that is then rendered.
        /// Because Kopernicus wants to know where each scatter actually is, we have to create our own scatter objects
        /// </summary>
        private void CreateScatterMeshes(PQSMod_LandClassScatterQuad quad)
        {
            if (allowedBiomes.Count > 0)
            {
                UnityEngine.Vector2d latLon = FlightGlobals.currentMainBody.GetLatitudeAndLongitude(quad.transform.position);
                string scatterBiome = PQSMod_BiomeSampler.GetCachedBiome(latLon.x, latLon.y, body);
                if (!allowedBiomes.Contains(scatterBiome))
                {
                    return;
                }
            }

            Random.InitState(quad.seed);

            // Redo the density calculation
            if (useBetterDensity)
            {
                Dictionary<String, Double> densities = quad.quad.GetComponent<DensityContainer>().densities;
                if (densities.ContainsKey(quad.scatter.scatterName))
                {
                    Double density = densities[quad.scatter.scatterName];
                    if (ignoreDensityGameSetting && PQS.Global_ScatterFactor > 0)
                    {
                        density /= PQS.Global_ScatterFactor;
                    }
                    Double scatterN = density * quad.scatter.densityFactor *
                                   (quad.quad.quadArea / quad.quad.sphereRoot.radius / 1000.0) *
                                   quad.scatter.maxScatter;
                    scatterN += Random.Range(-0.5f, 0.5f);
                    quad.count = Math.Min((Int32)Math.Round(scatterN), quad.scatter.maxScatter);
                }
            }

            int maxScatters = quad.count;
            while (maxScatters-- > 0)
            {
                if (useBetterDensity)
                {
                    // Generate a random number between 0 and 1. If it is above the spawn chance, abort
                    if (Random.value > spawnChance)
                    {
                        if (RuntimeUtility.RuntimeUtility.KopernicusConfig.UseIncorrectScatterDensityLogic)
                            break;

                        continue;
                    }
                }

                Int32 num2 = -1;
                Int32 num3 = -1;
                while (num3 == num2)
                {
                    Int32 num4 = Random.Range(1, PQS.cacheRes + 1);
                    Int32 num5 = Random.Range(1, PQS.cacheRes + 1);
                    Int32 x = num4 + Random.Range(-1, 1);
                    Int32 z = num5 + Random.Range(-1, 1);
                    num3 = PQS.vi(num4, num5);
                    num2 = PQS.vi(x, z);
                }

                Vector3 scatterPos = Vector3.Lerp(quad.quad.verts[num3], quad.quad.verts[num2], Random.value);
                Vector3 scatterUp = quad.quad.sphereRoot.surfaceRelativeQuads
                    ? (Vector3)(scatterPos + quad.quad.positionPlanet).normalized
                    : scatterPos.normalized;

                scatterPos += scatterUp * quad.scatter.verticalOffset;
                Single scatterAngle = Random.Range(rotation[0], rotation[1]);
                Quaternion scatterRot = QuaternionD.AngleAxis(scatterAngle, scatterUp) * quad.quad.quadRotation;
                Single scatterScale = Random.Range(quad.scatter.minScale, quad.scatter.maxScale);

                GameObject scatterObject = new GameObject("Scatter");
                scatterObject.layer = GameLayers.LOCAL_SPACE;
                Transform scatterTransform = scatterObject.transform;
                scatterTransform.parent = quad.obj.transform;
                scatterTransform.localPosition = scatterPos;
                scatterTransform.localRotation = scatterRot;
                scatterTransform.localScale = Vector3.one * scatterScale;
                MeshFilter filter = scatterObject.AddComponent<MeshFilter>();
                filter.sharedMesh = meshes.Count > 1 ? meshes[Random.Range(0, meshes.Count)] : baseMesh;
                MeshRenderer renderer = scatterObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = quad.scatter.material;
                renderer.shadowCastingMode = quad.scatter.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = quad.scatter.recieveShadows;

                scatterObject.AddComponent<KopernicusSurfaceObject>().objectName = quad.scatter.scatterName;
                
                scatterObjects.Add(scatterObject);
            }
        }
    }
}
