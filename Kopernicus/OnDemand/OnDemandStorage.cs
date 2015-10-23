/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        // Class to store OnDemand stuff
        public static class OnDemandStorage
        {
            // Lists
            public static Dictionary<string, List<ILoadOnDemand>> maps = new Dictionary<string, List<ILoadOnDemand>>();
            public static Dictionary<PQS, PQSMod_OnDemandHandler> handlers = new Dictionary<PQS, PQSMod_OnDemandHandler>();
            public static string currentBody = "";

            // OnDemand flags
            public static bool useOnDemand = true;
            public static bool useOnDemandBiomes = true;
            public static bool onDemandLoadOnMissing = true;
            public static bool onDemandLogOnMissing = true;

            // Add the management handler to the PQS
            public static void AddHandler(PQS pqsVersion)
            {
                PQSMod_OnDemandHandler handler = new GameObject("OnDemandHandler").AddComponent<PQSMod_OnDemandHandler>();
                handler.transform.parent = pqsVersion.transform;
                Object.DontDestroyOnLoad(handler);
                handler.sphere = pqsVersion;
                handler.order = 1;
                handlers[pqsVersion] = handler;
            }

            // Add a map to the map-list
            public static void AddMap(string body, ILoadOnDemand map)
            {
                // If the map is null, abort
                if (map == null)
                    return;

                // Create the sublist
                if (!maps.ContainsKey(body)) maps[body] = new List<ILoadOnDemand>();

                // Add the map
                if (!maps[body].Contains(map))
                {
                    maps[body].Add(map);

                    // Log
                    Debug.Log("[OD] Adding for body " + body + " map named " + map.name + " of path = " + map.Path);
                }
                else
                {
                    Debug.Log("[OD] WARNING: trying to add a map but is already tracked! Current body is " + body + " and map name is " + map.name + " and path is " + map.Path);
                }
            }

            // Remove a map from the list
            public static void RemoveMap(string body, ILoadOnDemand map)
            {
                // If the map is null, abort
                if (map == null)
                    return;

                // If the sublist exists, remove the map
                if (maps.ContainsKey(body))
                {
                    if (maps[body].Contains(map))
                    {
                        maps[body].Remove(map);
                    }
                    else
                    {
                        Debug.Log("[OD] WARNING: Trying to remove a map from a body, but the map is not tracked for the body!");
                    }
                }
                else
                {
                    Debug.Log("[OD] WARNING: Trying to remove a map from a body, but the body is not known!");
                }

                // If all maps of the body are unloaded, remove the body completely
                if (maps[body].Count == 0)
                    maps.Remove(body);
            }

            // Enable a list of maps
            public static void EnableMapList(List<ILoadOnDemand> maps, [Optional] List<ILoadOnDemand> exclude)
            {
                // If the excludes are null, create an empty list
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                // Go through all maps
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    // If excluded...
                    if (exclude.Contains(maps[i])) continue;

                    // Load the map
                    maps[i].Load();
                }
            }

            // Disable a list of maps
            public static void DisableMapList(List<ILoadOnDemand> maps, [Optional] List<ILoadOnDemand> exclude)
            {
                // If the excludes are null, create an empty list
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                // Go through all maps
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    // If excluded...
                    if (exclude.Contains(maps[i])) continue;

                    // Load the map
                    maps[i].Unload();
                }
            }

            // Enable all maps of a body
            public static bool EnableBody(string body)
            {
                if (maps.ContainsKey(body))
                {
                    EnableMapList(maps[body]);
                    return true;
                }
                return false;
                
            }

            // Unload all maps of a body
            public static bool DisableBody(string body)
            {
                if (maps.ContainsKey(body))
                {
                    DisableMapList(maps[body]);
                    return true;
                }
                return false;
            }
        }
    }
}
