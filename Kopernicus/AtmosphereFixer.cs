/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSP;
using System.Reflection;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AtmosphereFixer : MonoBehaviour
    {
        public static List<AtmosphereFromGround> atmospheres = new List<AtmosphereFromGround>();

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Debug.Log("Start()");
                foreach (AtmosphereFromGround afg in Resources.FindObjectsOfTypeAll<AtmosphereFromGround>().Where(a => a.planet != null && a.mainCamera != null && a.sunLight != null))
                {
                    Debug.Log("AFG " + afg.planet.bodyName);
                    AtmosphereFromGround afg_clone = null;
                    try
                    {
                        afg_clone = atmospheres.Find(a => a.planet.GetTransform().name == afg.planet.GetTransform().name);
                    }
                    catch
                    {
                        Debug.Log("[Kopernicus] AtmosphereFixer => Couldn't find an AtmosphereFromGround template for " + afg.planet.bodyName + "!");
                    }
                    if (afg_clone != null && afg != null)
                    {
                        Type[] types = new Type[] { typeof(float), typeof(Color) };
                        foreach (FieldInfo inf in afg.GetType().GetFields().Where(f => types.Contains(f.FieldType)))
                        {
                            inf.SetValue(afg, inf.GetValue(afg_clone));
                        }
                        Debug.Log(afg.outerRadius);
                        try
                        {
                            MethodInfo afgSetMaterial = typeof(AtmosphereFromGround).GetMethod("SetMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                            Debug.Log("1"); afgSetMaterial.Invoke(afg, new object[] { true });
                        }
                        catch
                        {
                            Debug.Log("[Kopernicus] AtmosphereFixer => Material-resetting for AtmosphereFromGround on " + afg.planet.bodyName + " failed!");
                        }

                        Debug.Log("Patched AFG");
                    }
                }
            }
        }
    }
}