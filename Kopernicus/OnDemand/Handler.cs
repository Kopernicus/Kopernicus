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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSP;

namespace Kopernicus
{
    namespace OnDemand
    {
        public static class OnDemandStorage
        {
            public static List<string> activeBodies = new List<string>();
            public static Dictionary<ILoadOnDemand, bool> enabledMaps = new Dictionary<ILoadOnDemand,bool>();
            public static Dictionary<ILoadOnDemand, float> mapTimes = new Dictionary<ILoadOnDemand, float>();
            public static Dictionary<ILoadOnDemand, List<string>> mapBodies = new Dictionary<ILoadOnDemand, List<string>>();
            public static Dictionary<string, List<ILoadOnDemand>> bodyMapLists = new Dictionary<string,List<ILoadOnDemand>>();
            public static Dictionary<PQS, PQSMod_OnDemandHandler> handlers = new Dictionary<PQS, PQSMod_OnDemandHandler>();
            public static List<ILoadOnDemand> mapList = new List<ILoadOnDemand>();
            public static string homeworldBody = "Kerbin";
            public static string currentBody = "";
            public static bool useOnDemand = true;
            public static bool useOnDemandBiomes = false;
            public static bool onDemandLoadOnMissing = true;
            public static bool onDemandLogOnMissing = true;
            public static bool onDemandForceCollect = false;

            public static void AddHandler(PQS pqsVersion)
            {
                GameObject handlerObject = new GameObject("OnDemandHandler");
                handlerObject.transform.parent = Utility.Deactivator;
                PQSMod_OnDemandHandler handler = handlerObject.AddComponent<PQSMod_OnDemandHandler>();
                handler.transform.parent = pqsVersion.transform;
                handler.sphere = pqsVersion;
                handler.order = 1;
                handlers[pqsVersion] = handler;
            }

            public static void UpdateHandler(PQS pqs, PQSMod_OnDemandHandler handler)
            {
                // just in case
                List<PQS> remlist = new List<PQS>();
                foreach(KeyValuePair<PQS, PQSMod_OnDemandHandler> kvp in handlers)
                {
                    if(kvp.Value == handler)
                        remlist.Add(kvp.Key);
                }
                foreach(PQS p in remlist)
                    handlers.Remove(p);

                handlers[pqs] = handler;
            }

            public static void AddMap(string body, ILoadOnDemand map)
            {
                if (map == null)
                    return;

                if(!bodyMapLists.ContainsKey(body))
                    bodyMapLists[body] = new List<ILoadOnDemand>();
                bodyMapLists[body].Add(map);
                Debug.Log("OD: Adding for body " + body + " map named " + map.name + " of path = " + map.Path);
                if (!mapList.Contains(map))
                {
                    mapList.Add(map);
                    enabledMaps[map] = false;
                    mapTimes[map] = 0f;
                }
                else
                {
                    Debug.Log("OD: WARNING: trying to add a map but is already tracked! Current body is " + body + " and map name is " + map.name + " and path is " + map.Path);
                }

                if (!mapBodies.ContainsKey(map))
                    mapBodies[map] = new List<string>();
                if(!mapBodies[map].Contains(body))
                    mapBodies[map].Add(body);
            }
            public static void RemoveMap(string body, ILoadOnDemand map)
            {
                if (map == null)
                    return;

                if (bodyMapLists.ContainsKey(body))
                    bodyMapLists[body].Remove(map);

                mapList.Remove(map);
                enabledMaps.Remove(map);
                mapTimes.Remove(map);

                List<string> bodies = mapBodies[map];
                if (bodies.Count <= 1)
                    mapBodies.Remove(map);
                else
                    bodies.Remove(body);
            }

            public static void EnableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
            {
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                ILoadOnDemand curmap;
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    curmap = maps[i];
                    if (exclude.Contains(curmap))
                        continue;
                    curmap.Load();
                }
            }
            public static void DisableMapList(List<ILoadOnDemand> maps, List<ILoadOnDemand> exclude = null)
            {
                if (exclude == null)
                    exclude = new List<ILoadOnDemand>();

                ILoadOnDemand curmap;
                for (int i = maps.Count - 1; i >= 0; --i)
                {
                    curmap = maps[i];
                    if (exclude.Contains(curmap))
                        continue;
                    curmap.Unload();
                }
            }

            public static bool EnableBody(string bname)
            {
                if (bodyMapLists.ContainsKey(bname) && !activeBodies.Contains(bname))
                {
                    EnableMapList(bodyMapLists[bname]);
                    activeBodies.Add(bname);
                    return true;
                }
                return false;
                
            }
            public static bool DisableBody(string bname)
            {
                if (bodyMapLists.ContainsKey(bname) && activeBodies.Contains(bname))
                {
                    DisableMapList(bodyMapLists[bname]);
                    activeBodies.Remove(bname);
                    return true;
                }
                return false;
            }
        }
    }
}
