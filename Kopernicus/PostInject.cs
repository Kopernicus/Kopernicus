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
            else*/ if (fi.FieldType == typeof(string))
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
                    PQSLandControl.LandClass lc = new PQSLandControl.LandClass();
                    ParseObject(lc, modNode.GetNode(name));
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
        private void PatchPQS(PQS pqs, ConfigNode node)
        {
            if (node.HasValue("radius"))
                double.TryParse(node.GetValue("radius"), out pqs.radius);

            List<PQSMod> mods = pqs.transform.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == pqs).ToList<PQSMod>();
            if (node.HasNode("Mods"))
            {
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
                Logger.Default.Log("Rebuild sphere for " + node.name + " failed: " + e.Message);
            }
        }
        private bool PatchBody(ConfigNode node)
        {
            if (node != null && node.HasValue("name"))
            {
                CelestialBody body = GetBody(node.GetValue("name"));
                if(node.HasNode("PQS"))
                    PatchPQS(body.pqsController, node.GetNode("PQS"));
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
                    Logger.Default.Log("CBUpdate for " + body.name + " failed: " + e.Message);
                }
            }
        }

        private void UpdateMenuTex()
        {
            PSystemBody home = Utility.FindHomeBody(PSystemManager.Instance.systemPrefab.rootBody);
            Texture homeMain = home.scaledVersion.renderer.material.GetTexture("_MainTex");
            Texture homeBump = home.scaledVersion.renderer.material.GetTexture("_BumpMap");
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
            Logger.Default.Log("Removing unused MapSOs and textures");
            List<MapSO> usedMaps = new List<MapSO>();
            List<Texture> usedTex = new List<Texture>();
            Templates.GetUsedLists(usedMaps, usedTex, PSystemManager.Instance.systemPrefab.rootBody);

            foreach (MapSO map in Templates.instance.origMapSOs)
                if (!usedMaps.Contains(map))
                {
                    MapSO.DestroyImmediate(map);
                    Logger.Default.Log("Removed MapSO " + map.name + " with mapname " + map.MapName);
                }

            foreach (Texture tex in Templates.instance.origTextures)
                if (!usedTex.Contains(tex))
                {
                    Texture.DestroyImmediate(tex);
                    Logger.Default.Log("Removed Texture " + tex.name);
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
                    Logger.Default.Log("**** Finalizing things");

                    rootConfig = rootConfig.GetNode(finalizeName);
                    bool finalizeOrbits = false;
                    bool removeUnused = false;
                    if (rootConfig.HasValue("finalizeOrbits"))
                        bool.TryParse(rootConfig.GetValue("finalizeOribts"), out finalizeOrbits);
                    if (rootConfig.HasValue("removeUnused"))
                        bool.TryParse(rootConfig.GetValue("removeUnused"), out removeUnused);

                    // Update the bodies
                    foreach (ConfigNode node in rootConfig.GetNodes(bodyNodeName))
                        if (!PatchBody(node))
                        {
                            if(node.HasValue("name"))
                            {
                                Logger.Default.Log("Failed to patch " + bodyNodeName + " " + node.GetValue("name"));
                            }
                            else
                            {
                                Logger.Default.Log("Failed to patch " + bodyNodeName + " node with no name");
                            }
                        }
                    if (finalizeOrbits)
                        FinalizeOrbits();
                    
                    UpdateMenuTex();

                    if(removeUnused)
                        RemoveUnused();

                    Logger.Default.Log("Finalization pass done!");
                }
            }
        }
    }
}
