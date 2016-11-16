/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

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
        /// Component to add Colliders and Science to the Scatter-Objects
        /// </summary>
        public class Scatter : MonoBehaviour
        {
            /// Contains the patched vessels (=> Kerbals)
            public static List<Guid> vesselID = new List<Guid>();

            /// Contains the blocked List of Base-Events
            public List<BaseEvent> blocked = new List<BaseEvent>();

            /// Contains a List of colliders for the scatter
            public List<MeshCollider> meshColliders = new List<MeshCollider>();

            /// Which experiment should we run?
            public ConfigNode experimentNode;

            /// The experiment
            public ModuleScienceExperiment experiment;

            /// Should we add Science?
            public bool science;

            /// Should we add Colliders?
            public bool colliders;

            /// <summary>
            /// Create a new ScatterExtension
            /// </summary>
            public static Scatter CreateInstance(GameObject o, bool science, bool colliders, ConfigNode experimentNode)
            {
                Scatter scatter = o.AddComponent<Scatter>();
                scatter.science = science;
                scatter.colliders = colliders;
                scatter.experimentNode = experimentNode;
                return scatter;
            }

            /// <summary>
            /// Create colliders for the scatter
            /// </summary>
            void Start()
            {
                // Register us as the parental object for the scatter
                PQSLandControl landControl = transform.parent.GetComponentInChildren<PQSLandControl>();
                PQSLandControl.LandClassScatter scatter = landControl.scatters.First(s => s.scatterName == name.Split(' ').Last());
                scatter.GetType().GetField("scatterParent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scatter, gameObject);

                // The ConfigNode is lost, so find it again!
                PSystemBody body = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == transform.parent.name);
                experiment = body.pqsVersion.gameObject.GetChild(name).GetComponent<Scatter>().experiment;
            }

            void Update()
            {
                // If there's nothing to do, discard any old colliders and abort
                if (transform.childCount == 0)
                {
                    if (!meshColliders.Any()) return;
                    Debug.LogWarning("[Kopernicus] Discard old colliders");
                    foreach (MeshCollider collider in meshColliders.Where(collider => collider))
                        Destroy(collider);
                    meshColliders.Clear();
                    return;
                }

                if (colliders)
                {
                    bool rebuild = false;
                    if (transform.childCount > meshColliders.Count)
                    {
                        Debug.LogWarning("[Kopernicus] Add " + (transform.childCount - meshColliders.Count) + " colliders");
                        rebuild = true;
                    }
                    else if (transform.childCount < meshColliders.Count)
                    {
                        Debug.LogWarning("[Kopernicus] Remove " + (meshColliders.Count - transform.childCount) + " colliders");
                        rebuild = true;
                    }
                    else if (meshColliders[0].sharedMesh != transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh)
                    {
                        Debug.LogWarning("[Kopernicus] Replacing colliders");
                        rebuild = true;
                    }
                    if (rebuild)
                    {
                        foreach (Transform t in transform)
                        {
                            MeshCollider collider = t.gameObject.GetComponent<MeshCollider>();
                            if (collider) continue;
                            collider = t.gameObject.AddComponent<MeshCollider>();
                            collider.sharedMesh = t.gameObject.GetComponent<MeshFilter>().sharedMesh;
                            collider.sharedMesh.Optimize();
                            collider.enabled = true;
                            meshColliders.Add(collider);
                        }
                    }
                }
                else if (meshColliders.Any())
                {
                    Debug.LogWarning("[Kopernicus] Discard unused colliders");
                    foreach (MeshCollider collider in meshColliders.Where(collider => collider))
                        Destroy(collider);
                    meshColliders.Clear();
                }

                // Node null
                if (experimentNode == null)
                    return;

                // Process science
                if (!FlightGlobals.ActiveVessel)
                    return;

                // We need eva's
                if (!FlightGlobals.ActiveVessel.isEVA)
                    return;

                // We need an Experiment
                if (FlightGlobals.ActiveVessel.evaController.part.FindModulesImplementing<ModuleScienceExperiment>().All(e => e.experimentID != experimentNode.GetValue("experimentID")) &&
                    !vesselID.Contains(FlightGlobals.ActiveVessel.id) &&
                    !FlightGlobals.ActiveVessel.packed)
                    AddScienceExperiment();

                if (experiment == null)
                    return;

                // Toggle the experiment
                ToggleExperiment(Physics.OverlapSphere(FlightGlobals.ship_position, 10f).Any(c => c.gameObject.transform.parent.name == name));
            }

            /// <summary>
            /// Loads the experiment from the config node and attaches it to a vessel
            /// </summary>
            void AddScienceExperiment()
            {
                // If the Node is null, abort
                if (experimentNode == null)
                    return;

                // Create the ScienceExperiment
                Part kerbal = FlightGlobals.ActiveVessel.evaController.part;
                experiment = kerbal.AddModule(typeof(ModuleScienceExperiment).Name) as ModuleScienceExperiment;

                // Load the experiment
                ConfigNode.LoadObjectFromConfig(experiment, experimentNode);

                // Deactivate some things
                experiment.resettable = false;

                // Start the experiment
                experiment.OnStart(PartModule.StartState.None);
                vesselID.Add(kerbal.vessel.id);
            }

            /// <summary>
            /// Toggles the visibility of the experiment
            /// </summary>
            public void ToggleExperiment(bool state)
            {
                // Activate
                if (state && blocked.Contains(experiment.Events["DeployExperiment"]))
                {
                    experiment.Events["DeployExperiment"].guiActive = true;
                    blocked.Remove(experiment.Events["DeployExperiment"]);
                }

                // Deactivate
                else if (!state && !blocked.Contains(experiment.Events["DeployExperiment"]))
                {
                    experiment.Events["DeployExperiment"].guiActive = false;
                    blocked.Add(experiment.Events["DeployExperiment"]);
                }
            }
        }
    }
}