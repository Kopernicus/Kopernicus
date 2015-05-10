using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class PostInject : MonoBehaviour
    {
        private const string rootNodeName = "Kopernicus";
        private const string finalizeName = "Finalize";
        private const string bodyNodeName = "Body";

        static private CelestialBody GetBody(string bodyName)
        {
            foreach (CelestialBody b in FlightGlobals.Bodies)
                if (b.name.Equals(bodyName))
                    return b;
            return null;
        }
        /*private List<PQSLandControl.LandClass> GetListLC(ConfigNode node)
        {
            ConfigNode curNodes = new ConfigNode();
            node.CopyTo(curNodes);
            List<PQSLandControl.LandClass> list = new List<PQSLandControl.LandClass>();
            Type cType = typeof(PQSLandControl.LandClass);
            while (curNodes.nodes.Count > 0)
            {
                PQSLandControl.LandClass obj = new PQSLandControl.LandClass();
                ParseObject(obj, curNodes.nodes[0]);
                curNodes.nodes.Remove(curNodes.nodes[0]);
            }
            return list;
        }
        private List<PQSLandControl.LandClassScatterAmount> GetListLCSA(ConfigNode node)
        {
            ConfigNode curNodes = new ConfigNode();
            node.CopyTo(curNodes);
            List<PQSLandControl.LandClassScatterAmount> list = new List<PQSLandControl.LandClassScatterAmount>();
            Type cType = typeof(PQSLandControl.LandClass);
            while (curNodes.nodes.Count > 0)
            {
                PQSLandControl.LandClassScatterAmount obj = new PQSLandControl.LandClassScatterAmount();
                ParseObject(obj, curNodes.nodes[0]);
                curNodes.nodes.Remove(curNodes.nodes[0]);
            }
            return list;
        }*/
        private void ParseField(object m, FieldInfo fi, ConfigNode modNode)
        {
            string name = fi.Name;
            /*if (fi.FieldType == typeof(IList))
            {
                if(modNode.HasNode(name))
                {
                    IList list = fi.GetValue(m) as IList;
                    if (list[0] != null)
                    {
                        Type objType = list[0].GetType();
                        ConfigNode listNode = modNode.GetNode(name);
                        list.Clear();
                        if(objType == typeof(PQSLandControl.LandClass))
                            fi.SetValue(m, GetListLC(listNode));
                    }
                }
            }
            else*/
            if (fi.FieldType == typeof(string))
            {
                if (modNode.HasValue(name))
                {
                    string val = modNode.GetValue(name);
                    fi.SetValue(m, val);
                }
            }
            else if (fi.FieldType == typeof(bool))
            {
                bool val;
                if (modNode.HasValue(name))
                    if (bool.TryParse(modNode.GetValue(name), out val))
                        fi.SetValue(m, val);
            }
            else if (fi.FieldType == typeof(int))
            {
                int val;
                if (modNode.HasValue(name))
                    if (int.TryParse(modNode.GetValue(name), out val))
                        fi.SetValue(m, val);
            }
            else if (fi.FieldType == typeof(float))
            {
                float val;
                if (modNode.HasValue(name))
                    if (float.TryParse(modNode.GetValue(name), out val))
                        fi.SetValue(m, val);
            }
            else if (fi.FieldType == typeof(double))
            {
                double val;
                if (modNode.HasValue(name))
                    if (double.TryParse(modNode.GetValue(name), out val))
                        fi.SetValue(m, val);
            }
            else if (fi.FieldType == typeof(MapSO))
            {
                if (modNode.HasValue(name))
                {
                    MapSO map = fi.GetValue(m) as MapSO;
                    if (map.Depth == MapSO.MapDepth.Greyscale)
                    {
                        Configuration.MapSOParser_GreyScale<MapSO> newMap = new Configuration.MapSOParser_GreyScale<MapSO>();
                        newMap.SetFromString(modNode.GetValue(name));
                        if (newMap.value != null)
                            fi.SetValue(m, newMap.value);
                    }
                    else
                    {
                        Configuration.MapSOParser_RGB<MapSO> newMap = new Configuration.MapSOParser_RGB<MapSO>();
                        newMap.SetFromString(modNode.GetValue(name));
                        if (newMap.value != null)
                            fi.SetValue(m, newMap.value);
                    }
                }
            }
            else if (fi.FieldType == typeof(AnimationCurve))
            {
                if (modNode.HasNode(name))
                {
                    FloatCurve fc = new FloatCurve();
                    fc.Load(modNode.GetNode(name));
                    fi.SetValue(m, fc.Curve);
                }
            }
            else if (fi.FieldType == typeof(FloatCurve))
            {
                if (modNode.HasNode(name))
                {
                    FloatCurve fc = new FloatCurve();
                    fc.Load(modNode.GetNode(name));
                    fi.SetValue(m, fc);
                }
            }
            else if (fi.FieldType == typeof(Texture2D))
            {
                if (modNode.HasValue(name))
                {
                    Configuration.Texture2DParser newParser = new Configuration.Texture2DParser();
                    newParser.SetFromString(name);
                    if (newParser.value != null)
                        fi.SetValue(m, newParser.value);
                    else
                    {
                        Texture2D newTex = Utility.LoadTexture(name, true, false, false);
                        if (newTex != null)
                            fi.SetValue(m, newParser.value);
                    }
                }
            }
            else if (fi.FieldType == typeof(PQSLandControl.LandClass))
            {
                if(modNode.HasNode(name))
                {
                    PQSLandControl.LandClass lc = fi.GetValue(m) as PQSLandControl.LandClass;
                    ParseObject(lc, modNode.GetNode(name));
                }
            }
            else if (fi.FieldType == typeof(PQSMod_CelestialBodyTransform.AltitudeFade))
            {
                if (modNode.HasNode(name))
                {
                    PQSMod_CelestialBodyTransform.AltitudeFade af = fi.GetValue(m) as PQSMod_CelestialBodyTransform.AltitudeFade;
                    ParseObject(af, modNode.GetNode(name));
                }
            }
            else if (fi.FieldType == typeof(PQSMod_CelestialBodyTransform.AltitudeFade[]))
            {
                if (modNode.HasNode(name))
                {
                    PQSMod_CelestialBodyTransform.AltitudeFade[] secFades = fi.GetValue(m) as PQSMod_CelestialBodyTransform.AltitudeFade[];
                    ConfigNode newNodes = new ConfigNode();
                    modNode.GetNode(name).CopyTo(newNodes);
                    foreach (PQSMod_CelestialBodyTransform.AltitudeFade af in secFades)
                    {
                        ParseObject(af, newNodes.nodes[0]);
                        newNodes.nodes.Remove(newNodes.nodes[0]);
                    }
                }
            }
        }
        private void ParseObject(object m, ConfigNode modNode)
        {
            foreach (FieldInfo fi in m.GetType().GetFields())
            {
                ParseField(m, fi, modNode);
            }
        }
        private void ModDecal(PQSMod m, ConfigNode node)
        {
            Type mType = m.GetType();
            bool city = mType == typeof(PQSCity);
            if (node.HasValue("latitude") && node.HasValue("longitude"))
            {
                // get the field to set
                FieldInfo posField = null;
                string fname = city ? "repositionRadial" : "position";
                foreach(FieldInfo fi in mType.GetFields())
                    if (fi.Name == fname)
                    {
                        posField = fi;
                        break;
                    }
                // Get the lat and long
                double lat, lon;
                double.TryParse(node.GetValue("latitude"), out lat);
                double.TryParse(node.GetValue("longitude"), out lon);
                Vector3 posV = Utility.LLAtoECEF(lat, lon, 0, m.sphere.radius);
                if (posField != null)
                    posField.SetValue(m, posV);
            }
            if (city)
            {
                if (node.HasValue("lodvisibleRangeMult"))
                {
                    PQSCity mod = m as PQSCity;
                    double dtmp;
                    if (double.TryParse(node.GetValue("lodvisibleRangeMult"), out dtmp))
                        foreach (PQSCity.LODRange l in mod.lod)
                            l.visibleRange = (float)(dtmp * l.visibleRange);
                }
            }
        }
        private bool PatchPQS(PQS pqs, ConfigNode node)
        {
            bool pqsChanged = false;
            double oldR = pqs.radius;
            if (node.HasValue("radius"))
                double.TryParse(node.GetValue("radius"), out pqs.radius);
            if (pqs.radius != oldR)
                pqsChanged = true;

            List<PQSMod> mods;
            if (node.HasNode("Mods"))
            {
                mods = pqs.transform.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == pqs).ToList<PQSMod>();
                foreach (ConfigNode modNode in node.GetNode("Mods").nodes)
                {
                    PQSMod delMod = null;
                    foreach (PQSMod m in mods)
                    {
                        Type mType = m.GetType();
                        if (mType.ToString() != modNode.name)
                            continue;
                        if (modNode.HasValue("name"))
                            if (m.name != modNode.GetValue("name"))
                                continue;
                        pqsChanged = true;
                        ParseObject(m, modNode);
                        if (mType == typeof(PQSCity) || mType == typeof(PQSMod_MapDecal) || mType == typeof(PQSMod_MapDecalTangent))
                            ModDecal(m, modNode);
                        delMod = m;
                        break;
                    }
                    // If we found the mod, remove from the list since we edited it.
                    if (delMod != null)
                        mods.Remove(delMod);
                }
            }
            // Get the whole list again.
            mods = pqs.transform.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == pqs).ToList<PQSMod>();
            if (node.HasNode("RemoveMods"))
            {
                List<GameObject> toCheck = new List<GameObject>();
                foreach (ConfigNode modNode in node.GetNode("RemoveMods").nodes)
                {
                    PQSMod delMod = null;
                    foreach (PQSMod m in mods)
                    {
                        Type mType = m.GetType();
                        if (mType.ToString() != modNode.name)
                            continue;
                        if (modNode.HasValue("name"))
                            if (m.name != modNode.GetValue("name"))
                                continue;
                        delMod = m;
                        break;
                    }
                    // If we found the mod, remove from the list since we edited it.
                    if (delMod != null)
                    {
                        pqsChanged = true;
                        toCheck.Add(delMod.gameObject);
                        mods.Remove(delMod);
                        delMod.sphere = null;
                        PQSMod.DestroyImmediate(delMod);
                    }
                }
                Utility.RemoveEmptyGO(toCheck);
            }
            // just in case, run setup for everyone.
            mods = pqs.transform.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == pqs).ToList<PQSMod>();
            foreach (var m in mods)
            {
                m.OnSetup();
                m.OnPostSetup();
            }
            try
            {
                pqs.RebuildSphere();
            }
            catch (Exception e)
            {
                Logger.Active.Log("Rebuild sphere for " + node.name + " failed: " + e.Message);
            }

            return pqsChanged;
        }
        private bool PatchBody(ConfigNode node)
        {
            if (node != null && node.HasValue("name"))
            {
                CelestialBody body = GetBody(node.GetValue("name"));
                if (body == null)
                {
                    Logger.Active.Log("Could not find body " + node.GetValue("name"));
                    return false;
                }
                Logger.Active.Log("Patching " + body.bodyName);
                Logger.Active.Flush();
                bool pqsChanged = true;
                if (body.pqsController != null)
                {
                    if (node.HasNode("PQS"))
                        pqsChanged |= PatchPQS(body.pqsController, node.GetNode("PQS"));
                    PQS[] pqsArr = body.pqsController.GetComponentsInChildren<PQS>(true);
                    foreach (PQS p in pqsArr)
                    {
                        if (node.HasNode("PQS" + p.name))
                            pqsChanged |= PatchPQS(p, node.GetNode("PQS" + p.name));
                    }
                }
                //if (pqsChanged)
                //{
                    Logger.Active.Log("Rebuilding scaledVersion mesh for " + body.bodyName);
                    Logger.Active.Flush();
                    // get prefab body
                    GameObject scaled = null;
                    if(PSystemManager.Instance != null)
                        if(PSystemManager.Instance.systemPrefab != null)
                            if (PSystemManager.Instance.systemPrefab.rootBody != null)
                            {
                                PSystemBody pBody = Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.name);
                                if (pBody != null)
                                    scaled = pBody.scaledVersion;
                            }

                    // get scaledspace if it exists
                    GameObject ssObj = null;
                    if (ScaledSpace.Instance != null && ScaledSpace.Instance.scaledSpaceTransforms != null)
                    {
                        foreach(Transform t in ScaledSpace.Instance.scaledSpaceTransforms)
                            if (t.name == body.name)
                            {
                                ssObj = t.gameObject;
                                break;
                            }
                    }
                    if (ssObj != null && ssObj != scaled)
                        Logger.Active.Log("ERROR: scaled space instance has different object than prefab!");
                    if (ssObj == null)
                        ssObj = scaled;
                    else
                        Logger.Active.Log("Using PSystem prefab gameobject");
                    Logger.Active.Flush();
                    if (ssObj != null)
                        Utility.UpdateScaledMesh(ssObj, body.pqsController, body, "GameData/Kopernicus/Cache", true);
                    else
                        Logger.Active.Log("Could not find a scaledVersion to remake.");
                //}
                return true;
            }
            return false;
        }
        private void FinalizeOrbits()
        {
            foreach (CelestialBody body in FlightGlobals.fetch.bodies)
            {
                if (body.orbitDriver != null)
                {
                    if (body.referenceBody != null)
                    {
                        body.hillSphere = body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1 / 3);
                        body.sphereOfInfluence = body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                        if (body.sphereOfInfluence < body.Radius * 1.5 || body.sphereOfInfluence < body.Radius + 20000.0)
                            body.sphereOfInfluence = Math.Max(body.Radius * 1.5, body.Radius + 20000.0); // sanity check

                        // period should be (body.Mass + body.referenceBody.Mass) at the end, not just ref body, but KSP seems to ignore that bit so I will too.
                        body.orbit.period = 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.referenceBody.Mass));

                        if (body.orbit.eccentricity <= 1.0)
                        {
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.orbitPercent = body.orbit.meanAnomalyAtEpoch / (Math.PI * 2);
                            body.orbit.ObTAtEpoch = body.orbit.orbitPercent * body.orbit.period;
                        }
                        else
                        {
                            // ignores this body's own mass for this one...
                            body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                            body.orbit.ObT = Math.Pow(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3.0) / body.orbit.referenceBody.gravParameter, 0.5) * body.orbit.meanAnomaly;
                            body.orbit.ObTAtEpoch = body.orbit.ObT;
                        }
                    }
                    else
                    {
                        body.sphereOfInfluence = double.PositiveInfinity;
                        body.hillSphere = double.PositiveInfinity;
                    }
                }
                try
                {
                    body.CBUpdate();
                }
                catch (Exception e)
                {
                    Logger.Active.Log("CBUpdate for " + body.name + " failed: " + e.Message);
                }
            }
        }

        private void UpdateMenuTex()
        {
            PSystemBody home = Utility.FindHomeBody(PSystemManager.Instance.systemPrefab.rootBody);
            Texture homeMain = home.scaledVersion.renderer.sharedMaterial.GetTexture("_MainTex");
            Texture homeBump = home.scaledVersion.renderer.sharedMaterial.GetTexture("_BumpMap");
            Logger.Active.Log("Replaceing color map " + Templates.instance.origKerbinTex.name + " with " + homeMain.name);
            Logger.Active.Log("Replaceing normal map " + Templates.instance.origKerbinBump.name + " with " + homeBump.name);
            Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material m in mats)
            {
                if (m.GetTexture("_MainTex") == Templates.instance.origKerbinTex)
                    m.SetTexture("_MainTex", homeMain);
                if (m.GetTexture("_BumpMap") == Templates.instance.origKerbinBump)
                    m.SetTexture("_BumpMap", homeBump);
            }
        }
        private void RemoveUnused()
        {
            Logger.Active.Log("Removing unused MapSOs and textures");
            List<MapSO> usedMaps = new List<MapSO>();
            List<Texture> usedTex = new List<Texture>();
            Templates.GetUsedLists(usedMaps, usedTex, PSystemManager.Instance.systemPrefab.rootBody);

            foreach (MapSO map in Templates.instance.origMapSOs)
            {
                if (map != null)
                {
                    string n1 = "NULL";
                    string n2 = "NULL";
                    try
                    {
                        n2 = map.MapName;
                        n1 = map.name;
                    }
                    catch(Exception e)
                    {
                        Logger.Active.Log("Exception getting MapSO name: " + e);
                    }
                    Logger.Active.Log("Checking MapSO " + n1 + " of mapname " + n2);
                    if (!usedMaps.Contains(map))
                    {
                        MapSO.DestroyImmediate(map);
                        Logger.Active.Log("Removed MapSO " + n1 + " with mapname " + n2);
                    }
                }
            }

            foreach (Texture tex in Templates.instance.origTextures)
            {
                if (tex != null)
                {
                    string n = "NULL";
                    try
                    {
                        n = tex.name;
                    }
                    catch (Exception e)
                    {
                        Logger.Active.Log("Exception getting tex name: " + e);
                    }
                    Logger.Active.Log("Checking Texture " + n);
                    if (!usedTex.Contains(tex))
                    {
                        Texture.DestroyImmediate(tex);
                        Logger.Active.Log("Removed Texture " + n);
                    }
                }
            }

        }
        public void Start()
        {
            // Get the data node
            ConfigNode rootConfig = GameDatabase.Instance.GetConfigs(rootNodeName)[0].config;

            // Patch bodies
            if (rootConfig != null)
            {
                if (rootConfig.HasNode(finalizeName))
                {
                    // Get the current time
                    DateTime start = DateTime.Now;
                    Logger finalizeLogger = new Logger("Finalize");
                    finalizeLogger.SetAsActive();
                    Logger.Active.Log("**** Finalizing things");

                    rootConfig = rootConfig.GetNode(finalizeName);
                    bool finalizeOrbits = false;
                    bool removeUnused = false;
                    if (rootConfig.HasValue("finalizeOrbits"))
                        bool.TryParse(rootConfig.GetValue("finalizeOribts"), out finalizeOrbits);
                    if (rootConfig.HasValue("removeUnused"))
                        bool.TryParse(rootConfig.GetValue("removeUnused"), out removeUnused);
                    Logger.Active.Flush();
                    // Update the bodies
                    foreach (ConfigNode node in rootConfig.GetNodes(bodyNodeName))
                    {
                        Logger.Active.Log("Found body node");
                        Logger.Active.Flush();
                        if (!PatchBody(node))
                        {
                            if (node.HasValue("name"))
                            {
                                Logger.Active.Log("Failed to patch " + bodyNodeName + " " + node.GetValue("name"));
                            }
                            else
                            {
                                Logger.Active.Log("Failed to patch " + bodyNodeName + " node with no name");
                            }
                        }
                    }
                    if (finalizeOrbits)
                        FinalizeOrbits();
                    
                    UpdateMenuTex();

                    if(removeUnused)
                        RemoveUnused();
                    TimeSpan duration = (DateTime.Now - start);
                    Logger.Active.Log("Finalization pass done! Completed in: " + duration.TotalMilliseconds + " ms");
                    Logger.Active.Close();
                }
            }
        }
    }
}
