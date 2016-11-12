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

using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using Kopernicus.Components;
using Kopernicus.Configuration;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

namespace Kopernicus
{
    /// <summary>
    /// A small class to fix the science archives
    /// </summary>
    public class RnDFixer : MonoBehaviour
    {
        void Start()
        {
            //  FIX BODIES MOVED POSTSPAWN  //
            bool postSpawnChanges = false;
            foreach (CelestialBody cb in PSystemManager.Instance.localBodies.Where(b => b.Has("orbitPatches")))
            {
                // Fix position if the body gets moved PostSpawn
                ConfigNode patch = cb.Get<ConfigNode>("orbitPatches");
                if (patch.GetValue("referenceBody") != null || patch.GetValue("semiMajorAxis") != null)
                {
                    // Get the body, the old parent and the new parent
                    PSystemBody body = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == name);
                    PSystemBody oldParent = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.children.Contains(body));
                    PSystemBody newParent = oldParent;
                    if (patch.GetValue("referenceBody") != null)
                        newParent = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == patch.GetValue("referenceBody"));

                    if (body != null && oldParent != null)
                    {
                        // If there is no new SMA it means only the parent changed
                        NumericParser<double> newSMA = body.orbitDriver.orbit.semiMajorAxis;
                        if (patch.GetValue("semiMajorAxis") != null)
                            newSMA.SetFromString(patch.GetValue("semiMajorAxis"));
                        
                        // Count how many children comes before our body in the newParent.child list
                        int index = 0;
                        foreach (PSystemBody child in newParent.children)
                        {
                            if (child.orbitDriver.orbit.semiMajorAxis < newSMA.value)
                                index++;
                        }
                        
                        // Add the body as child for the new parent and remove it for the old parent
                        if (index > newParent.children.Count)
                            newParent.children.Add(body);
                        else
                            newParent.children.Insert(index, body);
                        
                        oldParent.children.Remove(body);
                        postSpawnChanges = true;
                    }
                }
            }

            // Rebuild Archives
            if (postSpawnChanges)
                AddPlanets();



            //  RDVisibility = SKIP  //
            List<KeyValuePair<PSystemBody, PSystemBody>> skipList = new List<KeyValuePair<PSystemBody, PSystemBody>>();

            // Create a list with body to hide and their parent
            PSystemBody[] bodies = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true);
            foreach (CelestialBody body in PSystemManager.Instance.localBodies.Where(b => b.Has("hiddenRnD")))
            { 
                if (body.Get<PropertiesLoader.RDVisibility>("hiddenRnD") == PropertiesLoader.RDVisibility.SKIP)
                {
                    PSystemBody hidden = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, name);
                    if (hidden.children.Count == 0)
                    {
                        body.Set("hiddenRnd", PropertiesLoader.RDVisibility.HIDDEN);
                    }
                    else
                    {
                        PSystemBody parent = bodies.First(b => b.children.Contains(hidden));
                        if (parent != null)
                        {
                            if (skipList.Any(b => b.Key == parent))
                            {
                                int index = skipList.IndexOf(skipList.First(b => b.Key == parent));
                                skipList.Insert(index, new KeyValuePair<PSystemBody, PSystemBody>(hidden, parent));
                            }
                            else
                                skipList.Add(new KeyValuePair<PSystemBody, PSystemBody>(hidden, parent));
                        }
                    }
                }
            }

            foreach (KeyValuePair<PSystemBody, PSystemBody> pair in skipList)
            {
                // Get hidden body and parent
                PSystemBody hidden = pair.Key;
                PSystemBody parent = pair.Value;

                // Find where the hidden body is
                int index = parent.children.IndexOf(hidden);

                // Put its children in its place
                parent.children.InsertRange(index, hidden.children);

                // Remove the hidden body from its parent's children list so it won't show up when clicking the parent
                parent.children.Remove(hidden);
            }

            if (skipList.Count > 0)
            {
                // Rebuild Archives
                AddPlanets();

                // Undo the changes to the PSystem
                for (int i = skipList.Count; i > 0; i = i - 1)
                {
                    PSystemBody hidden = skipList.ElementAt(i).Key;
                    PSystemBody parent = skipList.ElementAt(i).Value;
                    int oldIndex = parent.children.IndexOf(hidden.children.First());
                    parent.children.Insert(oldIndex, hidden);
                    foreach (PSystemBody child in hidden.children)
                    {
                        if (parent.children.Contains(child))
                            parent.children.Remove(child);
                    }
                }
            }



            //  RDVisibility = HIDDEN  //  RDVisibility = NOICON  //
            // Loop through the Container
            foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>())
            {
                // Barycenter
                CelestialBody body = PSystemManager.Instance.localBodies.Find(b => b.transform.name == planetItem.label_planetName.text);
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
                    else if (visibility == PropertiesLoader.RDVisibility.HIDDEN)
                    {
                        planetItem.gameObject.SetActive(false);
                        planetItem.Hide();
                        planetItem.HideChildren();
                    }
                    else
                    {
                        planetItem.planet.SetActive(true);
                        planetItem.label_planetName.alignment = TextAlignmentOptions.MidlineRight;
                    }
                }

                // namechanges
                if (FindObjectsOfType<NameChanger>().Count(n => n.oldName == planetItem.label_planetName.text) != 0 && !planetItem.label_planetName.name.EndsWith("NAMECHANGER"))
                {
                    NameChanger changer = FindObjectsOfType<NameChanger>().First(n => n.oldName == planetItem.label_planetName.text);
                    planetItem.label_planetName.text = changer.newName;
                    planetItem.label_planetName.name += "NAMECHANGER";
                }
            }
        }


        void AddPlanets()
        {
            // Stuff needed for AddPlanets
            FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).First();
            MethodInfo add = typeof(RDArchivesController).GetMethod("AddPlanets");
            var RDAC = Resources.FindObjectsOfTypeAll<RDArchivesController>().First();

            // AddPlanets requires this list to be empty when triggered
            list.SetValue(RDAC, new Dictionary<string, List<RDArchivesController.Filter>>());

            // AddPlanets!
            add.Invoke(RDAC, null);
        }
    }
}