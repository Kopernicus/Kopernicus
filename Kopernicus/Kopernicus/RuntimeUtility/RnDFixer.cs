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
            bool hiddenObjects = false;

            // Loop through the Container
            foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>())
            {
                // If the body needs to be hidden
                if (Templates.hiddenRnD.ContainsKey(planetItem.label_planetName.text) && Templates.hiddenRnD[planetItem.label_planetName.text] == PropertiesLoader.RDVisibility.HIDDEN)
                {
                    // Select the body to hide and its parentrndf
                    PSystemBody hidden = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == planetItem.label_planetName.text);
                    PSystemBody reference = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.children.Contains(hidden));
                    
                    if (reference != null)
                    {
                        // Find where the hidden body is
                        int index = reference.children.IndexOf(hidden);

                        // Put its children in its place
                        reference.children.InsertRange(index, hidden.children);

                        // Remove the hidden body from its parent's children list so it won't show up when clicking the parent
                        reference.children.Remove(hidden);

                        // Set this to 'true' so we know we need to trigger AddPlanets
                        hiddenObjects = true;
                    }
                }
            }
            if (hiddenObjects)
            {
                // Stuff needed for AddPlanets
                FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).First();
                MethodInfo add = typeof(RDArchivesController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Skip(27).First();
                var RDAC = Resources.FindObjectsOfTypeAll<RDArchivesController>().First();

                // AddPlanets requires this list to be empty when triggered
                list.SetValue(RDAC, new Dictionary<string, List<RDArchivesController.Filter>>());

                // AddPlanets!
                add.Invoke(RDAC, null);
            }



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
                    else if (visibility != PropertiesLoader.RDVisibility.HIDDEN) // 'HIDDEN' has already be taken care of in the earlier loop
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
