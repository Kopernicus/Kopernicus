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
using System.Reflection;

namespace Kopernicus
{
    /// <summary>
    /// A small class to fix the science archives
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class RnDFixer : MonoBehaviour
    {
        void Start()
        {
            // Loop through the Container
            foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>())
            {
                // Barycenter
                if (Templates.barycenters.Contains(planetItem.label_planetName.text) || Templates.notSelectable.Contains(planetItem.label_planetName.text))
                {
                    // Call function recursively
                    Utility.DoRecursive(planetItem, i => Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(p => p.parent == i), (item) =>
                    {
                        // Reparent
                        foreach (RDPlanetListItemContainer child in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>().Where(p => p.parent == item))
                        {
                            child.parent = item.parent;
                            child.hierarchy_level += 1;
                            float scale = 0.8f;
                            float container_height = 60f;
                            if (child.hierarchy_level > 1)
                            {
                                scale = 0.5f;
                                container_height = 60f;
                            }
                            float magnitude = child.planet.GetComponent<MeshFilter>().mesh.bounds.size.magnitude;
                            FieldInfo float0 = child.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == typeof(float)).ToArray()[0];
                            FieldInfo float1 = child.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType == typeof(float)).ToArray()[1];
                            float0.SetValue(child, 80f / magnitude * scale);
                            float1.SetValue(child, (80f / magnitude * scale) * 1.1f);
                            child.planet.transform.localScale = Vector3.one * (80f / magnitude * scale);
                            child.layoutElement.preferredHeight = container_height;
                        }
                    });

                    // Hide
                    planetItem.Hide();
                }

                // namechanges
                if (FindObjectsOfType<NameChanger>().Where(n => n.oldName == planetItem.label_planetName.text).Count() != 0 && !planetItem.label_planetName.name.EndsWith("NAMECHANGER"))
                {
                    NameChanger changer = FindObjectsOfType<NameChanger>().First(n => n.oldName == planetItem.label_planetName.text);
                    planetItem.label_planetName.text = changer.newName;
                    planetItem.label_planetName.name += "NAMECHANGER";
                }
            }
        }
    }
}