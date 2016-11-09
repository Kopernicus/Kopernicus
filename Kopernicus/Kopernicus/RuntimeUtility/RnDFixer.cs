/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
            // FIX POSTSPAWN REPARENTING HERE

            foreach (string name in Templates.orbitPatches.Keys)
            {
                // Fix position if the body gets moved PostSpawn
                if (Templates.orbitPatches[name].GetValue("referenceBody") != null || Templates.orbitPatches[name].GetValue("semiMajorAxis") != null)
                {
                    // Get the body, the old parent and the new parent
                    PSystemBody body = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == name);
                    PSystemBody oldParent = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.children.Contains(body));
                    PSystemBody newParent = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == Templates.orbitPatches[name].GetValue("referenceBody"));

                    if (body != null && oldParent != null)
					{
                        // If there is no new parent it means only SMA changed
                        if (newParent == null)
                            newParent = oldParent;

                        // If there is no new SMA it means only the parent changed
                        NumericParser<double> newSMA = body.orbitDriver.orbit.semiMajorAxis;
                        if (Templates.orbitPatches[name].GetValue("semiMajorAxis") != null)
                            newSMA.SetFromString(Templates.orbitPatches[name].GetValue("semiMajorAxis"));

                        // Count how many children comes before our body in the newParent.child list
                        int index = 0;
                        foreach (PSystemBody child in newParent.children)
                        {
                            if (child.orbitDriver.orbit.semiMajorAxis < newSMA.value)
                                index++;
                        }

                        // Add the body as child for the new parent and remove it for the old parent
                        newParent.children.Insert(index, body);
                        oldParent.children.Remove(body);
                    }
                }
            }


            // FIX FOR   RDVisibility = SKIP

            List<KeyValuePair<PSystemBody, PSystemBody>> skipList = new List<KeyValuePair<PSystemBody, PSystemBody>>();

            // Create a list with body to hide and their parent
            foreach (string name in Templates.hiddenRnD.Keys)
            {
                if (Templates.hiddenRnD[name] == PropertiesLoader.RDVisibility.SKIP)
                {
                    PSystemBody hidden = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == name);

                    if (hidden.children.Count == 0)
                    {
                        Templates.hiddenRnD[name] = PropertiesLoader.RDVisibility.HIDDEN;
                    }
                    else
                    {
                        PSystemBody parent = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.children.Contains(hidden));
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
                // Stuff needed for AddPlanets
                FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).First();
                MethodInfo add = typeof(RDArchivesController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Skip(27).First();
                var RDAC = Resources.FindObjectsOfTypeAll<RDArchivesController>().First();

                // AddPlanets requires this list to be empty when triggered
                list.SetValue(RDAC, new Dictionary<string, List<RDArchivesController.Filter>>());

                // AddPlanets!
                add.Invoke(RDAC, null);


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



            // FIX FOR   RDVisibility = HIDDEN  and  NOICON

            // Loop through the Container
            foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>())
            {
                // Barycenter
                if (Templates.barycenters.Contains(planetItem.label_planetName.text) || Templates.notSelectable.Contains(planetItem.label_planetName.text))
                {
                    planetItem.planet.SetActive(false);
                    planetItem.label_planetName.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                }

                // RD Visibility
                if (Templates.hiddenRnD.ContainsKey(planetItem.label_planetName.text))
                {
                    PropertiesLoader.RDVisibility visibility = Templates.hiddenRnD[planetItem.label_planetName.text];
                    if (visibility == PropertiesLoader.RDVisibility.NOICON)
                    {
                        planetItem.planet.SetActive(false);
                        planetItem.label_planetName.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
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
    }
}