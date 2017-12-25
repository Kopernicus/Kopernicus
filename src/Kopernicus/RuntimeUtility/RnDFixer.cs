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

using Kopernicus.Components;
using Kopernicus.Configuration;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Kopernicus
{
    /// <summary>
    /// A small class to fix the science archives
    /// </summary>
    public class RnDFixer : MonoBehaviour
    {
        internal static List<RDPlanetListItemContainer> RnDRotationKill = new List<RDPlanetListItemContainer>();

        void LateUpdate()
        {
            // Kill the rotation of selected planet thumbnails
            for (Int32 i = 0; i < RnDRotationKill.Count(); i++)
            {
                RDPlanetListItemContainer planetItem = RnDRotationKill[i];
                planetItem.planet.transform.rotation = Quaternion.identity;
            }
        }

        void Start()
        {
            //  FIX BODIES MOVED POSTSPAWN  //
            Boolean postSpawnChanges = false;

            // Replaced 'foreach' with 'for' to improve performance
            CelestialBody[] postSpawnBodies = PSystemManager.Instance?.localBodies?.Where(b => b.Has("orbitPatches"))?.ToArray();

            for (Int32 i = 0; i < postSpawnBodies?.Length; i++)
            {
                CelestialBody cb = postSpawnBodies[i];

                // Fix position if the body gets moved PostSpawn
                ConfigNode patch = cb.Get<ConfigNode>("orbitPatches");
                if (patch.GetValue("referenceBody") != null || patch.GetValue("semiMajorAxis") != null)
                {
                    // Get the body
                    PSystemBody body = PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true)?.FirstOrDefault(b => b.name == cb.transform.name);
                    if (body == null)
                    {
                        Debug.Log("[Kopernicus]: RnDFixer: Could not find PSystemBody => " + cb.transform.name);
                        continue;
                    }

                    // Get the parent
                    PSystemBody oldParent = PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true)?.FirstOrDefault(b => b.children.Contains(body));
                    if (oldParent == null)
                    {
                        Debug.Log("[Kopernicus]: RnDFixer: Could not find referenceBody of CelestialBody => " + cb.transform.name);
                        continue;
                    }

                    // Check if PostSpawnOrbit changes referenceBody
                    PSystemBody newParent = oldParent;
                    if (patch.GetValue("referenceBody") != null)
                        newParent = PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true).FirstOrDefault(b => b.name == patch.GetValue("referenceBody"));
                    if (oldParent == null)
                    {
                        Debug.Log("[Kopernicus]: RnDFixer: Could not find PostSpawn referenceBody of CelestialBody => " + cb.transform.name);
                        newParent = oldParent;
                    }

                    // Check if PostSpawnOrbit changes semiMajorAxis
                    if (body?.orbitDriver?.orbit?.semiMajorAxis == null)
                    {
                        Debug.Log("[Kopernicus]: RnDFixer: Could not find PostSpawn semiMajorAxis of CelestialBody => " + cb.transform.name);
                        continue;
                    }
                    NumericParser<Double> newSMA = body.orbitDriver.orbit.semiMajorAxis;
                    if (patch.GetValue("semiMajorAxis") != null)
                        newSMA.SetFromString(patch.GetValue("semiMajorAxis"));

                    // Remove the body from oldParent.children
                    oldParent.children.Remove(body);

                    // Find the index of the body in newParent.children
                    Int32 index = newParent.children.FindAll(c => c.orbitDriver.orbit.semiMajorAxis < newSMA.Value).Count;

                    // Add the body to newParent.children
                    if (index > newParent.children.Count)
                        newParent.children.Add(body);
                    else
                        newParent.children.Insert(index, body);

                    // Signal that the system has PostSpawn changes
                    postSpawnChanges = true;
                }
            }

            // Rebuild Archives
            if (postSpawnChanges)
                AddPlanets();



            //  RDVisibility = HIDDEN  //  RDVisibility = SKIP  //

            // Create a list with body to hide and their parent
            List<KeyValuePair<PSystemBody, KeyValuePair<PSystemBody, Int32>>> hideList = new List<KeyValuePair<PSystemBody, KeyValuePair<PSystemBody, int>>>();
            // Create a list with body to skip and their parent
            List<KeyValuePair<PSystemBody, PSystemBody>> skipList = new List<KeyValuePair<PSystemBody, PSystemBody>>();

            // Replaced 'foreach' with 'for' to improve performance
            PSystemBody[] bodies = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true);
            CelestialBody[] hideBodies = PSystemManager.Instance?.localBodies?.Where(cb => cb.Has("hiddenRnD"))?.ToArray();

            for (Int32 i = 0; i < hideBodies?.Length; i++)
            {
                CelestialBody body = hideBodies[i];
                PropertiesLoader.RDVisibility visibility = body.Get<PropertiesLoader.RDVisibility>("hiddenRnD");

                if (visibility == PropertiesLoader.RDVisibility.HIDDEN || visibility == PropertiesLoader.RDVisibility.SKIP)
                {
                    PSystemBody hidden = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.transform.name);
                    PSystemBody parent = bodies.FirstOrDefault(b => b.children.Contains(hidden));
                    if (parent != null)
                    {
                        // Hide
                        if (hidden.children.Count == 0 || visibility == PropertiesLoader.RDVisibility.HIDDEN)
                        {
                            body.Set("hiddenRnd", PropertiesLoader.RDVisibility.HIDDEN);
                            hideList.Add(new KeyValuePair<PSystemBody, KeyValuePair<PSystemBody, Int32>>(hidden, new KeyValuePair<PSystemBody, int>(parent, 0)));
                        }
                        // Skip
                        else
                        {
                            if (skipList.Any(b => b.Key == parent))
                            {
                                Int32 index = skipList.IndexOf(skipList.FirstOrDefault(b => b.Key == parent));
                                skipList.Insert(index, new KeyValuePair<PSystemBody, PSystemBody>(hidden, parent));
                            }
                            else
                                skipList.Add(new KeyValuePair<PSystemBody, PSystemBody>(hidden, parent));
                        }
                    }
                }
            }

            // Skip bodies
            for (Int32 i = 0; i < skipList.Count; i++)
            {
                KeyValuePair<PSystemBody, PSystemBody> pair = skipList[i];

                // Get hidden body and parent
                PSystemBody hidden = pair.Key;
                PSystemBody parent = pair.Value;

                // Find where the hidden body is
                Int32 index = parent.children.IndexOf(hidden);

                // Remove the hidden body from its parent's children list so it won't show up when clicking the parent
                parent.children.Remove(hidden);

                // Put its children in its place
                parent.children.InsertRange(index, hidden.children);
            }

            // Hide bodies
            for (Int32 i = 0; i < hideList.Count; i++)
            {
                KeyValuePair<PSystemBody, KeyValuePair<PSystemBody, Int32>> pair = hideList[i];

                // Get hidden body and parent
                PSystemBody hidden = pair.Key;
                PSystemBody parent = bodies.FirstOrDefault(b => b.children.Contains(hidden));

                // Find where the hidden body is
                Int32 index = parent.children.IndexOf(hidden);

                // Remove the hidden body from its parent's children list so it won't show up when clicking the parent
                parent.children.Remove(hidden);

                // Save the position in the hideList
                hideList[i] = new KeyValuePair<PSystemBody, KeyValuePair<PSystemBody, Int32>>(hidden, new KeyValuePair<PSystemBody, int>(parent, index));
            }

            // Apply changes and revert to the original PSystem
            if (hideList.Count > 0 || skipList?.Count > 0)
            {
                // Rebuild Archives
                AddPlanets();

                // Undo the changes to the PSystem (hide)
                for (Int32 i = hideList.Count - 1; i > -1; i--)
                {
                    PSystemBody hidden = hideList[i].Key;
                    PSystemBody parent = hideList[i].Value.Key;
                    Int32 oldIndex = hideList[i].Value.Value;
                    parent.children.Insert(oldIndex, hidden);
                }
                // Undo the changes to the PSystem (skip)
                for (Int32 i = skipList.Count - 1; i > -1; i--)
                {
                    PSystemBody hidden = skipList[i].Key;
                    PSystemBody parent = skipList[i].Value;
                    Int32 oldIndex = parent.children.IndexOf(hidden.children.FirstOrDefault());
                    parent.children.Insert(oldIndex, hidden);

                    for (Int32 j = 0; j < hidden.children.Count; j++)
                    {
                        PSystemBody child = hidden.children[j];

                        if (parent.children.Contains(child))
                            parent.children.Remove(child);
                    }
                }
            }



            //  RDVisibility = NOICON  //  Kill Rotation //
            // Loop through the Containers
            RDPlanetListItemContainer[] containers = Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(i => i.label_planetName.text != "Planet name").ToArray();
            for (Int32 i = 0; i < containers?.Count(); i++)
            {
                RDPlanetListItemContainer planetItem = containers[i];

                // The label text is set from the CelestialBody's displayName
                CelestialBody body = PSystemManager.Instance?.localBodies?.FirstOrDefault(cb => cb.transform.name == planetItem.name);
                if (body == null)
                {
                    Debug.Log("[Kopernicus]: RnDFixer: Could not find CelestialBody for the label => " + planetItem.name);
                    continue;
                }

                // Barycenter
                if (body.Has("barycenter") || body.Has("notSelectable"))
                {
                    planetItem.planet.SetActive(false);
                    planetItem.label_planetName.alignment = TextAlignmentOptions.MidlineLeft;
                }

                // RD Visibility
                if (body.Has("hiddenRnD"))
                {
                    PropertiesLoader.RDVisibility visibility = body.Get<PropertiesLoader.RDVisibility>("hiddenRnD");
                    if (visibility == PropertiesLoader.RDVisibility.NOICON)
                    {
                        planetItem.planet.SetActive(false);
                        planetItem.label_planetName.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                    else
                    {
                        planetItem.planet.SetActive(true);
                        planetItem.label_planetName.alignment = TextAlignmentOptions.MidlineRight;
                    }
                }

                // Add planetItems to 'RnDRotationKill'
                if (planetItem?.planet?.transform?.rotation == null) continue;
                if (body.Has("RnDRotation") ? !body.Get<Boolean>("RnDRotation") : body?.scaledBody?.GetComponentInChildren<SunCoronas>(true) != null)
                {
                    RnDRotationKill.Add(planetItem);
                }
            }
        }

        public static void AddPlanets()
        {
            RDPlanetListItemContainer[] planetItems = Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(i => i.label_planetName.text != "Planet name").ToArray();
            for (int i = 0; i < planetItems.Length; i++)
            {
                DestroyImmediate(planetItems[i]);
            }

            // Stuff needed for AddPlanets
            FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).FirstOrDefault();
            MethodInfo add = typeof(RDArchivesController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)?.Skip(26)?.FirstOrDefault();
            RDArchivesController RDAC = Resources.FindObjectsOfTypeAll<RDArchivesController>().FirstOrDefault();

            // AddPlanets requires this list to be empty when triggered
            list.SetValue(RDAC, new Dictionary<String, List<RDArchivesController.Filter>>());

            // AddPlanets!
            add.Invoke(RDAC, null);
        }
    }
}
