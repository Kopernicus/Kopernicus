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
            public static Dictionary<ILoadOnDemand, bool> enabledMaps = new Dictionary<ILoadOnDemand,bool>();
            public static Dictionary<ILoadOnDemand, float> mapTimes = new Dictionary<ILoadOnDemand, float>();
            public static Dictionary<ILoadOnDemand, List<string>> mapBodies = new Dictionary<ILoadOnDemand, List<string>>();
            public static Dictionary<string, List<ILoadOnDemand>> bodyMapLists = new Dictionary<string,List<ILoadOnDemand>>();
            public static List<ILoadOnDemand> mapList = new List<ILoadOnDemand>();
            public static string homeworldBody = "Kerbin";
            public static string currentBody = "";
            public static bool useOnDemand = false;

            public static void AddMap(string body, ILoadOnDemand map)
            {
                if (map == null)
                    return;

                if(!bodyMapLists.ContainsKey(body))
                    bodyMapLists[body] = new List<ILoadOnDemand>();
                bodyMapLists[body].Add(map);
                if (mapList.Contains(map))
                {
                    Debug.Log("OD: ERROR: trying to add a map but is already tracked.");
                }
                else
                {
                    mapList.Add(map);
                    enabledMaps[map] = false;
                    mapTimes[map] = 0f;

                    if (!mapBodies.ContainsKey(map))
                        mapBodies[map] = new List<string>();
                    mapBodies[map].Add(body);
                }
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

            public static void EnableBody(string bname)
            {
                if (bodyMapLists.ContainsKey(bname))
                    EnableMapList(bodyMapLists[bname]);
            }
            public static void DisableBody(string bname)
            {
                if (bodyMapLists.ContainsKey(bname))
                {
                    DisableMapList(bodyMapLists[bname]);
                }
            }
        }

        [KSPAddon(KSPAddon.Startup.EveryScene, false)]
        public class OnDemandHandler : MonoBehaviour
        {
            protected static string FindHomeworld()
            {
                if (Planetarium.fetch != null)
                    return Planetarium.fetch.Home.bodyName;

                if (FlightGlobals.Bodies != null)
                {
                    for (int i = 0; i < FlightGlobals.Bodies.Count; ++i)
                        if (FlightGlobals.Bodies[i].isHomeWorld)
                            return FlightGlobals.Bodies[i].bodyName;
                }

                return "Kerbin";
            }

            protected static void RecurseFillBodies(PSystemBody body)
            {
                if (body != null)
                {
                    bodiesToEnable[body.celestialBody.bodyName] = false;
                    foreach (PSystemBody child in body.children)
                        RecurseFillBodies(child);
                }
            }
            protected static void FillBodyList()
            {
                bool fail = false;
                bodiesToEnable.Clear();
                try
                {
                    if (FlightGlobals.Bodies != null)
                    {
                        int bCount = FlightGlobals.Bodies.Count;
                        //Debug.Log("OD: Filling body list with " + bCount + " bodies");

                        for (int i = 0; i < bCount; ++i)
                            bodiesToEnable[FlightGlobals.Bodies[i].bodyName] = false;
                    }
                }
                catch(Exception e)
                {
                    fail = true;
                    Debug.Log("OD: Failed to get body list because something threw: " + e);
                    RecurseFillBodies(PSystemManager.Instance.systemPrefab.rootBody);
                }
            }

            static Dictionary<string, bool> bodiesToEnable = new Dictionary<string, bool>();
            static float waitBeforeUnload = 5f;
            static bool dontUpdate = true;

            public void Start()
            {
                if (dontUpdate && (/*Templates.loadFinished ||*/ HighLogic.LoadedScene == GameScenes.MAINMENU))
                    dontUpdate = false;

                if (!dontUpdate)
                {
                    OnDemandStorage.homeworldBody = FindHomeworld();
                    FillBodyList();
                }

                BodyUpdate();
            }

            public void Update()
            {
                BodyUpdate();
            }

            public void BodyUpdate()
            {
                // wait until Kopernicus is done loading
                if (dontUpdate)
                    return;

                // Set all false.
                foreach (string b in bodiesToEnable.Keys.ToList())
                    bodiesToEnable[b] = false;

                if (!HighLogic.LoadedSceneIsFlight)
                {
                    bodiesToEnable[OnDemandStorage.homeworldBody] = true;
                }
                if (FlightGlobals.currentMainBody != null)
                {
                    bodiesToEnable[FlightGlobals.currentMainBody.bodyName] = true;
                }
                else if (FlightGlobals.ActiveVessel != null) // HOW!?
                {
                    if (FlightGlobals.ActiveVessel.mainBody != null)
                        bodiesToEnable[FlightGlobals.ActiveVessel.mainBody.bodyName] = true;
                }

                // find if we have maps that should be loaded that aren't, or vice versa
                // and enable/disable as required
                ILoadOnDemand map;
                List<string> bodyNames;
                for (int i = OnDemandStorage.mapList.Count - 1; i >= 0; --i)
                {
                    map = OnDemandStorage.mapList[i]; // get the map itself

                    bodyNames = OnDemandStorage.mapBodies[map]; // get all the bodies that use it
                    bool shouldEnable = false;
                    for (int j = bodyNames.Count - 1; j >= 0; --j)
                    {
                        if (bodiesToEnable.ContainsKey(bodyNames[j]))
                            shouldEnable |= bodiesToEnable[bodyNames[j]];
                        else
                        {
                            if(bodyNames[j] == "Kerbin" && OnDemandStorage.homeworldBody != "Kerbin")
                                shouldEnable |= bodiesToEnable[OnDemandStorage.homeworldBody];
                            else
                                Debug.Log("OD: ERROR: bodies list does not contain " + bodyNames[j] + " and homeworldBody is the same!");
                        }
                    }
                    bool mapEnabled = OnDemandStorage.enabledMaps[map];

                    // if not as desired, deal with the map.
                    if (mapEnabled)
                    {
                        if (!shouldEnable)
                        {
                            float curTime = OnDemandStorage.mapTimes[map];
                            //Debug.Log("OD: Should disable " + (map as MapSO).name + ", current time " + curTime);
                            curTime += Time.deltaTime;
                            if (curTime >= waitBeforeUnload)
                            {
                                map.Unload();
                            }
                            else
                                OnDemandStorage.mapTimes[map] = curTime;
                        }
                    }
                    else
                    {
                        if (shouldEnable)
                        {
                            //Debug.Log("OD: Should enable " + (map as MapSO).name);
                            map.Load();
                        }
                    }
                }
            }
        }
    }
}
